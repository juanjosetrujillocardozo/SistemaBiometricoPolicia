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
        // ── Fase 0: Clave temporal de cifrado ─────────────────────────────────
        // TODO Fase 1: mover a DPAPI / MachineKeyProvider para derivación por hardware.
        // El valor aquí es un placeholder; la BD real se cifrará con ChangePassword()
        // en MigrarACifrado() la primera vez que se ejecute esta versión.
        private const string DB_KEY = "TRUJO_DB_2026_TEMP";

        private static string dbPath;
        private static string ConnectionString;

        static DatabaseHelper()
        {
            dbPath           = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "biometrico.db");
            // Password= activa el cifrado de System.Data.SQLite (SEE/SQLCipher según la
            // native DLL disponible). Se migra la BD existente sin contraseña en MigrarACifrado().
            ConnectionString = $"Data Source={dbPath};Version=3;Password={DB_KEY};";

            MigrarACifrado();
            InicializarBaseDeDatos();
        }

        // ── Conexión ───────────────────────────────────────────────────────────

        public static SQLiteConnection ObtenerConexion()
            => new SQLiteConnection(ConnectionString);

        // ── Inicialización ─────────────────────────────────────────────────────

        public static void InicializarBaseDatos() => InicializarBaseDeDatos();

        private static void InicializarBaseDeDatos()
        {
            if (!File.Exists(dbPath))
                SQLiteConnection.CreateFile(dbPath);

            using (var conn = ObtenerConexion())
            {
                conn.Open();
                CrearTablas(conn);
            }
        }

        /// <summary>
        /// Si la BD existe y está sin contraseña (instalaciones previas a Fase 0),
        /// la cifra con DB_KEY usando ChangePassword().
        /// Si ya está cifrada, la apertura sin contraseña fallará y el bloque catch
        /// la deja intacta para que ConnectionString la abra con la clave correcta.
        /// </summary>
        private static void MigrarACifrado()
        {
            if (!File.Exists(dbPath)) return;

            try
            {
                // Intentar abrir SIN contraseña (BD no cifrada — escenario de migración)
                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    // Éxito → la BD no estaba cifrada; la ciframos ahora
                    conn.ChangePassword(DB_KEY);
                    LogHelper.RegistrarEvento(
                        "BD migrada a almacenamiento cifrado (Fase 0).", "SEGURIDAD");
                }
            }
            catch (SQLiteException)
            {
                // Ya está cifrada o requiere contraseña → no hacer nada
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("MigrarACifrado", ex);
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

                CREATE INDEX IF NOT EXISTS idx_estudiante_documento
                    ON Estudiantes(NumeroDocumento);

                CREATE INDEX IF NOT EXISTS idx_registro_fecha
                    ON RegistrosAlimentacion(FechaHora);

                CREATE INDEX IF NOT EXISTS idx_registro_estudiante
                    ON RegistrosAlimentacion(EstudianteId);";

            using (var cmd = new SQLiteCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        // ── Métodos de acceso ──────────────────────────────────────────────────

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

                using (var cmd    = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Estudiante
                        {
                            Id              = Convert.ToInt32(reader["Id"]),
                            NumeroDocumento = reader["NumeroDocumento"].ToString(),
                            Apellidos       = reader["Apellidos"].ToString(),
                            Nombres         = reader["Nombres"].ToString(),
                            Seccion         = reader["Seccion"].ToString(),
                            RutaFoto        = reader["RutaFoto"] != DBNull.Value
                                                  ? reader["RutaFoto"].ToString()
                                                  : null
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
                    const string sql =
                        "INSERT INTO Huellas (EstudianteId, Dedo, TemplateHuella) " +
                        "VALUES (@estudianteId, @dedo, @template)";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@estudianteId", estudianteId);
                        cmd.Parameters.AddWithValue("@dedo",         dedo);
                        cmd.Parameters.AddWithValue("@template",     template);
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
                    cmd.Parameters.AddWithValue("@id",   estudianteId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
