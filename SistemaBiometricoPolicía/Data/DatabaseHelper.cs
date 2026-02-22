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
            dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "biometrico.db");
            // Fase 0: Sin contraseña para asegurar estabilidad con System.Data.SQLite estándar
            ConnectionString = $"Data Source={dbPath};Version=3;";
            InicializarBaseDeDatos();
        }

        public static SQLiteConnection ObtenerConexion()
            => new SQLiteConnection(ConnectionString);

        public static void InicializarBaseDeDatos()
        {
            if (!File.Exists(dbPath))
                SQLiteConnection.CreateFile(dbPath);

            using (var conn = ObtenerConexion())
            {
                conn.Open();
                CrearTablas(conn);
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

            using (var cmd = new SQLiteCommand(sqlLicencia, conn))
                cmd.ExecuteNonQuery();

            const string sql = @"
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
                CREATE INDEX IF NOT EXISTS idx_registro_fecha ON RegistrosAlimentacion(FechaHora);
                CREATE INDEX IF NOT EXISTS idx_registro_estudiante ON RegistrosAlimentacion(EstudianteId);";

            using (var cmd = new SQLiteCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        public static string ObtenerRutaBaseDeDatos() => dbPath;

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
    }
}