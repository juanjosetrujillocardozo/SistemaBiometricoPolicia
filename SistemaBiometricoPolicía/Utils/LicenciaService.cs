using System;
using System.Data.SQLite;
using System.Net.Http;
using System.Threading.Tasks;
using SistemaBiometricoPolicia.Data;

namespace SistemaBiometricoPolicia.Utils
{
    public static class LicenciaService
    {
        private const string URL_VALIDACION = "https://licencias.trujotechnologies.com/check-license.php";

        public static bool LicenciaValida { get; private set; } = false;
        public static string MensajeEstado { get; private set; } = "Verificando...";

        /// <summary>
        /// Valida la licencia (local + remota si hay internet).
        /// Retorna el estado local actualizado.
        /// </summary>
        public static async Task<bool> ValidarLicenciaAsync()
        {
            try
            {
                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    var cmd = new SQLiteCommand("SELECT * FROM Licencia LIMIT 1", conn);
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

                        // 1. Verificación de Fecha Local (Funciona sin internet)
                        if (DateTime.Now > expiracion || estado == "BLOQUEADO")
                        {
                            MensajeEstado = "LICENCIA EXPIRADA O BLOQUEADA. Contacte a Soporte: +57 317 294 6935";
                            LicenciaValida = false;
                            return false;
                        }

                        // 2. Verificación Remota (Si hay internet, intenta validar con tu servidor)
                        await VerificarRemotamente(token);

                        // 3. Volver a leer estado local (por si se actualizó remotamente)
                        var cmdEstado = new SQLiteCommand("SELECT Estado FROM Licencia LIMIT 1", conn);
                        string estadoActualizado = cmdEstado.ExecuteScalar()?.ToString() ?? "OK";

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
        }

        /// <summary>
        /// Obtiene el estado de la licencia guardado localmente (para FormAcercaDe).
        /// </summary>
        public static string ObtenerEstadoLocal()
        {
            try
            {
                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    var cmd = new SQLiteCommand("SELECT Estado FROM Licencia LIMIT 1", conn);
                    var estado = cmd.ExecuteScalar()?.ToString() ?? "OK";
                    return estado;
                }
            }
            catch
            {
                return "ERROR";
            }
        }

        /// <summary>
        /// Verifica la licencia contra el servidor remoto.
        /// </summary>
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
                // Si falla la conexión, NO bloquear (puede ser problema de red temporal)
                LogHelper.RegistrarError("Error al verificar licencia remotamente (se mantiene estado local)", ex);
            }
        }

        /// <summary>
        /// Actualiza el estado local de la licencia en la base de datos.
        /// </summary>
        private static void ActualizarEstadoLocal(string nuevoEstado)
        {
            try
            {
                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    var cmd = new SQLiteCommand("UPDATE Licencia SET Estado = @estado", conn);
                    cmd.Parameters.AddWithValue("@estado", nuevoEstado);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al actualizar estado local de licencia", ex);
            }
        }

        /// <summary>
        /// Método de compatibilidad con código anterior (sin async).
        /// </summary>
        public static bool ValidarLicencia()
        {
            return ValidarLicenciaAsync().GetAwaiter().GetResult();
        }
    }
}