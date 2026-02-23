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
        private const bool MODO_DESARROLLADOR = false;

        private const string URL_VALIDACION = "https://licencias.trujotechnologies.com/check-license.php";

        public static bool LicenciaValida { get; private set; } = false;
        public static string MensajeEstado { get; private set; } = "Verificando...";

        public static async Task<bool> ValidarLicenciaAsync(string metodo = "inicio_sistema")
        {
#if DEBUG
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

                        await VerificarRemotamente(token, metodo).ConfigureAwait(false);

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

        private static async Task VerificarRemotamente(string token, string metodo = "inicio_sistema")
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);

                    string pcName = Uri.EscapeDataString(Environment.MachineName);
                    string userName = Uri.EscapeDataString(Environment.UserName);

                    string url = $"{URL_VALIDACION}?token={token}&metodo={metodo}&pc={pcName}&user={userName}";

                    var response = await client.GetStringAsync(url).ConfigureAwait(false);

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
                LogHelper.RegistrarError("Error al verificar licencia remotamente", ex);
            }
        }

        public static bool ValidarLicencia()
        {
            // Sigue siendo sync-friendly para el código existente
            return ValidarLicenciaAsync().GetAwaiter().GetResult();
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

    }
}