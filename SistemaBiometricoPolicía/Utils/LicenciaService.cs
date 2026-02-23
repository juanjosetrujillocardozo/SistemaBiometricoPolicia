using System;
using System.Data.SQLite;
using System.Net.Http;
using System.Threading.Tasks;
using SistemaBiometricoPolicia.Data;

namespace SistemaBiometricoPolicia.Utils
{
    public static class LicenciaService
    {
        // 🔧 MODO DESARROLLADOR - Cambiar a false antes de entregar al cliente
        private const bool MODO_DESARROLLADOR = true;

        private const string URL_VALIDACION = "https://licencias.trujotechnologies.com/check-license.php";

        public static bool LicenciaValida { get; private set; } = false;
        public static string MensajeEstado { get; private set; } = "Verificando...";

        public static async Task<bool> ValidarLicenciaAsync()
        {
#if DEBUG
            // En modo DEBUG siempre válido (sin warnings en Release)
            LicenciaValida = true;
            MensajeEstado = "Licencia Activa (Modo desarrollador)";
            return true;
#else
            if (MODO_DESARROLLADOR)
            {
                LicenciaValida = true;
                MensajeEstado = "Licencia Activa (Modo desarrollador)";
                return true;
            }

            try
            {
                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM Licencia LIMIT 1", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            MensajeEstado = "SISTEMA NO ACTIVADO. Contacte a TRUJO TECHNOLOGIES.";
                            LicenciaValida = false;
                            return false;
                        }

                        DateTime expiracion = Convert.ToDateTime(reader["FechaExpiracion"]);
                        string estado = reader["Estado"].ToString();
                        string token = reader["TokenActivacion"].ToString();

                        if (DateTime.Now > expiracion || estado == "BLOQUEADO")
                        {
                            MensajeEstado = "LICENCIA EXPIRADA O BLOQUEADA. Contacte a Soporte: +57 317 294 6935";
                            LicenciaValida = false;
                            return false;
                        }

                        await VerificarRemotamente(token);

                        string estadoActualizado;
                        using (var cmdEstado = new SQLiteCommand("SELECT Estado FROM Licencia LIMIT 1", conn))
                            estadoActualizado = cmdEstado.ExecuteScalar()?.ToString() ?? "OK";

                        if (estadoActualizado == "BLOQUEADO")
                        {
                            MensajeEstado = "LICENCIA BLOQUEADA REMOTAMENTE. Contacte a Soporte: +57 317 294 6935";
                            LicenciaValida = false;
                            return false;
                        }

                        LicenciaValida = true;
                        MensajeEstado = "Licencia Activa";
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al validar licencia", ex);
                MensajeEstado = "Error al validar licencia";
                LicenciaValida = false;
                return false;
            }
#endif
        }

        public static string ObtenerEstadoLocal()
        {
            try
            {
                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT Estado FROM Licencia LIMIT 1", conn))
                        return cmd.ExecuteScalar()?.ToString() ?? "OK";
                }
            }
            catch
            {
                return "ERROR";
            }
        }

        private static async Task VerificarRemotamente(string token)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = await client.GetStringAsync($"{URL_VALIDACION}?token={token}");

                    if (response != null && response.Trim().ToUpper().Contains("BLOQUEAR"))
                    {
                        ActualizarEstadoLocal("BLOQUEADO");
                        LogHelper.RegistrarEvento($"Licencia remota BLOQUEADA para token {token}", "LICENCIA");
                    }
                    else
                    {
                        ActualizarEstadoLocal("OK");
                        LogHelper.RegistrarEvento($"Licencia remota OK para token {token}", "LICENCIA");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al verificar licencia remotamente (se mantiene estado local)", ex);
            }
        }

        private static void ActualizarEstadoLocal(string nuevoEstado)
        {
            try
            {
                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("UPDATE Licencia SET Estado = @estado", conn))
                    {
                        cmd.Parameters.AddWithValue("@estado", nuevoEstado);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al actualizar estado local de licencia", ex);
            }
        }

        public static bool ValidarLicencia()
        {
            return ValidarLicenciaAsync().GetAwaiter().GetResult();
        }
    }
}