using Newtonsoft.Json.Linq;
using SistemaBiometricoPolicia.Biometric;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SistemaBiometricoPolicia.Utils
{
    public static class UpdateService
    {
        private static readonly HttpClient http = new HttpClient();
        private const string URL_CHECK = "https://licencias.trujotechnologies.com/api/check-update.php";

        // ─── Versión local (lee AssemblyVersion del ejecutable) ───────────────
        public static Version GetLocalVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        // ─── Versión remota (consulta el servidor) ────────────────────────────
        public static async Task<Version> GetRemoteVersionAsync()
        {
            try
            {
                string token = LicenciaService.TokenActual;
                if (string.IsNullOrEmpty(token)) return null;

                string versionActual = GetLocalVersion().ToString();
                string url = $"{URL_CHECK}?token={token}&version={versionActual}";

                System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;

                var response = await http.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);

                if (data["ok"]?.Value<bool>() != true) return null;

                string latestStr = data["latest_version"]?.ToString();
                if (string.IsNullOrEmpty(latestStr)) return null;

                return Version.TryParse(latestStr, out var v) ? v : null;
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("UpdateService.GetRemoteVersionAsync", ex);
                return null;
            }
        }

        // ─── Verificación automática al iniciar (llamada desde FormPrincipal) ─
        public static async Task CheckForUpdatesAsync(Form owner)
        {
            try
            {
                string token = LicenciaService.TokenActual;
                if (string.IsNullOrEmpty(token)) return;

                string versionActual = GetLocalVersion().ToString();
                string url = $"{URL_CHECK}?token={token}&version={versionActual}";

                StatusHub.PushEvento("🔍 Buscando actualizaciones...");

                System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;

                var response = await http.GetAsync(url);
                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);

                if (data["ok"]?.Value<bool>() != true) return;

                bool hayUpdate = data["update_available"]?.Value<bool>() == true;

                if (!hayUpdate)
                {
                    StatusHub.PushEvento("✅ Sistema actualizado.");
                    return;
                }

                string nuevaVersion = data["latest_version"]?.ToString();
                string urlDescarga = data["download_url"]?.ToString();
                string notas = data["release_notes"]?.ToString();

                StatusHub.PushEvento($"✨ Nueva versión disponible: {nuevaVersion}");

                var respuesta = MessageBox.Show(owner,
                    $"Nueva versión disponible: {nuevaVersion}\n\nNotas:\n{notas}\n\n¿Desea instalarla ahora?",
                    "Actualización - TRUJO TECHNOLOGIES",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (respuesta == DialogResult.Yes)
                    await DescargarEInstalar(urlDescarga, nuevaVersion);
            }
            catch (Exception ex)
            {
                StatusHub.PushEvento("✗ Error al verificar updates: " + ex.Message);
                LogHelper.RegistrarError("UpdateService.CheckForUpdatesAsync", ex);
            }
        }

        // ─── Descarga y ejecuta el instalador ────────────────────────────────
        private static async Task DescargarEInstalar(string url, string version)
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), $"Setup_Biometrico_{version}.exe");

                StatusHub.PushEvento("⬇ Descargando actualización...");

                using (var client = new HttpClient())
                {
                    System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
                    var bytes = await client.GetByteArrayAsync(url);
                    File.WriteAllBytes(tempPath, bytes);
                }

                StatusHub.PushEvento("✅ Descarga completada. Preparando actualización...");

                // PASO 1: Delay para terminar operaciones pendientes
                await Task.Delay(1000);

                // PASO 2: Liberar lector biométrico
                try
                {
                    if (BiometricService.Instancia != null)
                    {
                        StatusHub.PushEvento("🛑 Liberando lector biométrico...");
                        BiometricService.Instancia.Dispose();
                    }
                }
                catch { }

                // PASO 3: Forzar liberación de COM objects (evita RaceOnRCWCleanup)
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // PASO 4: Pausa para que el runtime libere los RCW
                await Task.Delay(500);

                // PASO 5: Lanzar instalador silencioso
                StatusHub.PushEvento("🚀 Iniciando instalación silenciosa...");

                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    Arguments = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-",
                    UseShellExecute = true,
                    Verb = "runas"
                });

                // AGREGA ESTO:
                MessageBox.Show("La actualización se está instalando en segundo plano.\n\nEl sistema se cerrará y se volverá a abrir automáticamente en unos segundos.",
                    "Actualización en curso - TRUJO TECHNOLOGIES",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LogHelper.RegistrarEvento("Cerrando app para actualización silenciosa a " + version, "UPDATE");

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error crítico en actualización", ex);
                MessageBox.Show("No se pudo completar la actualización automática:\n" + ex.Message,
                    "Error de TRUJO TECHNOLOGIES", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}