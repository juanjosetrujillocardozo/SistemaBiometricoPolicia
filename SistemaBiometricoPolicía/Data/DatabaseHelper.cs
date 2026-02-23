using SistemaBiometricoPolicia.Models;
using SistemaBiometricoPolicia.Utils;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace SistemaBiometricoPolicia.Data
{
    public static class DatabaseHelper
    {
        private static string dbPath;
        private static string ConnectionString;

        static DatabaseHelper()
        {
            // SOLUCIÓN PROFESIONAL: Mover la BD a ProgramData para evitar errores de permisos en Program Files
            string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "TRUJO",
                "SistemaBiometricoPolicia"
            );

            // Crear la carpeta si no existe (C:\ProgramData\TRUJO\SistemaBiometricoPolicia)
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            dbPath = Path.Combine(folderPath, "biometrico.db");

            // ConnectionString con la nueva ruta absoluta
            ConnectionString = $"Data Source={dbPath};Version=3;BusyTimeout=5000;";
        }

        public static SQLiteConnection ObtenerConexion()
            => new SQLiteConnection(ConnectionString);

        public static void InicializarBaseDeDatos()
        {
            try
            {
                if (!File.Exists(dbPath))
                {
                    SQLiteConnection.CreateFile(dbPath);
                }

                using (var conn = ObtenerConexion())
                {
                    conn.Open();

                    // Optimizaciones de rendimiento
                    using (var cmd = new SQLiteCommand("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    CrearTablas(conn);
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error crítico al inicializar base de datos en: " + dbPath, ex);
                throw; // Re-lanzar para que el StartupValidator lo capture
            }
        }

        private static void CrearTablas(SQLiteConnection conn)
        {
            const string sqlLicencia = @"
                CREATE TABLE IF NOT EXISTS Licencia (
                    Id                       INTEGER PRIMARY KEY AUTOINCREMENT,
                    TokenActivacion          TEXT,
                    FechaExpiracion          DATETIME,
                    UltimaVerificacionRemota DATETIME,
                    Estado                   TEXT DEFAULT 'ACTIVO'
                );";

            const string sqlTablas = @"
                CREATE TABLE IF NOT EXISTS Estudiantes (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    NumeroDocumento TEXT UNIQUE NOT NULL,
                    Nombres         TEXT NOT NULL,
                    Apellidos       TEXT NOT NULL,
                    Seccion         TEXT NOT NULL,
                    RutaFoto        TEXT,
                    FechaRegistro   DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS Huellas (
                    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                    EstudianteId   INTEGER NOT NULL,
                    TemplateHuella BLOB NOT NULL,
                    Dedo           TEXT,
                    FechaRegistro  DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id)
                );

                CREATE TABLE IF NOT EXISTS RegistrosAlimentacion (
                    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    EstudianteId INTEGER NOT NULL,
                    TipoServicio TEXT NOT NULL,
                    FechaHora    DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FotoPath     TEXT,
                    Sincronizado INTEGER DEFAULT 0,
                    FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id)
                );

                CREATE INDEX IF NOT EXISTS idx_estudiante_documento ON Estudiantes(NumeroDocumento);
                CREATE INDEX IF NOT EXISTS idx_registro_fecha ON RegistrosAlimentacion(FechaHora);";

            using (var cmd = new SQLiteCommand(sqlLicencia, conn)) cmd.ExecuteNonQuery();
            using (var cmd = new SQLiteCommand(sqlTablas, conn)) cmd.ExecuteNonQuery();
        }

        public static string ObtenerRutaBaseDeDatos() => dbPath;

        // ... (El resto de tus métodos ObtenerEstudiantesSinHuellas, GuardarHuella, etc. se mantienen igual)
        // Solo asegúrate de que sigan usando ObtenerConexion() que ya tiene la nueva ruta.

        public static List<Estudiante> ObtenerEstudiantesSinHuellas()
        {
            var lista = new List<Estudiante>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    SELECT * FROM Estudiantes
                    WHERE Id NOT IN (SELECT EstudianteId FROM Huellas)
                    ORDER BY Apellidos ASC";

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Estudiante
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            NumeroDocumento = reader["NumeroDocumento"].ToString(),
                            Apellidos = reader["Apellidos"].ToString(),
                            Nombres = reader["Nombres"].ToString(),
                            Seccion = reader["Seccion"].ToString(),
                            RutaFoto = reader["RutaFoto"] != DBNull.Value ? reader["RutaFoto"].ToString() : null
                        });
                    }
                }
            }
            return lista;
        }

        public static bool GuardarHuella(int estudianteId, string dedo, byte[] template)
        {
            try
            {
                using (var conn = ObtenerConexion())
                {
                    conn.Open();
                    const string sql = "INSERT INTO Huellas (EstudianteId, Dedo, TemplateHuella) VALUES (@estudianteId, @dedo, @template)";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@estudianteId", estudianteId);
                        cmd.Parameters.AddWithValue("@dedo", dedo);
                        cmd.Parameters.AddWithValue("@template", template);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al guardar huella", ex);
                return false;
            }
        }

        public static void ActualizarFotoEstudiante(int estudianteId, string rutaFoto)
        {
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                const string sql = "UPDATE Estudiantes SET RutaFoto = @ruta WHERE Id = @id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ruta", rutaFoto);
                    cmd.Parameters.AddWithValue("@id", estudianteId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static (int total, int desayunos, int almuerzos, int cenas) ObtenerEstadisticasHoy()
        {
            try
            {
                using (var conn = ObtenerConexion())
                {
                    conn.Open();
                    const string sql = @"
                SELECT 
                    COUNT(*) AS total,
                    SUM(CASE WHEN TipoServicio = 'DESAYUNO' THEN 1 ELSE 0 END) AS desayunos,
                    SUM(CASE WHEN TipoServicio = 'ALMUERZO' THEN 1 ELSE 0 END) AS almuerzos,
                    SUM(CASE WHEN TipoServicio = 'CENA'     THEN 1 ELSE 0 END) AS cenas
                FROM RegistrosAlimentacion
                WHERE date(FechaHora) = date('now', 'localtime')";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (
                                Convert.ToInt32(reader["total"]),
                                Convert.ToInt32(reader["desayunos"]),
                                Convert.ToInt32(reader["almuerzos"]),
                                Convert.ToInt32(reader["cenas"])
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error en ObtenerEstadisticasHoy", ex);
            }
            return (0, 0, 0, 0);
        }
    }
}