using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SistemaBiometricoPolicia.Utils
{
    public static class UpdateService
    {
        private const string URL_UPDATE =
            "https://licencias.trujotechnologies.com/update-check.php";

        // El servidor debe devolver JSON:
        // { "version":"1.0.7",
        //   "url":"https://licencias.trujotechnologies.com/releases/SistemaBiometricoPolicia-1.0.7-setup.exe",
        //   "notas":"..." }

        [DataContract]
        private class VersionInfo
        {
            [DataMember(Name = "version")] public string Version { get; set; }
            [DataMember(Name = "url")] public string Url { get; set; }
            [DataMember(Name = "notas")] public string Notas { get; set; }
        }

        public static Version GetLocalVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var versionInfo = assembly.GetName().Version;
                return versionInfo ?? new Version(1, 0, 0, 0);
            }
            catch
            {
                return new Version(1, 0, 0, 0);
            }
        }

        public static async Task CheckForUpdatesAsync(Form parentForm)
        {
            try
            {
                var localVersion = GetLocalVersion();
                VersionInfo infoRemota = await GetRemoteInfoAsync();

                if (infoRemota == null || string.IsNullOrWhiteSpace(infoRemota.Version))
                {
                    StatusHub.PushEvento("⚠ No se pudo verificar actualizaciones (sin internet, servidor no disponible o respuesta inválida)");
                    return;
                }

                if (!Version.TryParse(infoRemota.Version, out var remoteVersion))
                {
                    StatusHub.PushEvento($"⚠ Versión remota inválida: {infoRemota.Version}");
                    return;
                }

                if (remoteVersion <= localVersion)
                {
                    StatusHub.PushEvento($"✓ Sistema actualizado (v{localVersion})");
                    return;
                }

                var message = $"🔔 HAY UNA NUEVA VERSIÓN DISPONIBLE\n\n" +
                              $"Versión actual: {localVersion}\n" +
                              $"Versión nueva: {remoteVersion}\n\n" +
                              $"{infoRemota.Notas}\n\n" +
                              "¿Desea descargar e instalar la actualización ahora?\n\n" +
                              "NOTA: El sistema se cerrará durante la instalación.";

                var result = MessageBox.Show(parentForm, message,
                    "Actualización Disponible - TRUJO TECHNOLOGIES",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    await DownloadAndRunInstallerAsync(infoRemota, remoteVersion, parentForm);
                }
                else
                {
                    StatusHub.PushEvento("⚠ Actualización pospuesta por el usuario");
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error general en CheckForUpdatesAsync", ex);
            }
        }

        private static async Task<VersionInfo> GetRemoteInfoAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var json = await client.GetStringAsync(URL_UPDATE);

                    if (string.IsNullOrWhiteSpace(json))
                        return null;

                    var ser = new DataContractJsonSerializer(typeof(VersionInfo));
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    {
                        var info = ser.ReadObject(ms) as VersionInfo;
                        return info;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al obtener información de actualización remota", ex);
                return null;
            }
        }

        public static async Task<Version> GetRemoteVersionAsync()
        {
            try
            {
                VersionInfo info = await GetRemoteInfoAsync();

                if (info == null || string.IsNullOrWhiteSpace(info.Version))
                    return null;

                if (Version.TryParse(info.Version, out var version))
                    return version;

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static async Task DownloadAndRunInstallerAsync(VersionInfo info, Version remoteVersion, Form parentForm)
        {
            if (string.IsNullOrWhiteSpace(info.Url))
            {
                MessageBox.Show(parentForm,
                    "❌ No se encontró URL de descarga en el servidor.\n" +
                    "Contacte a TRUJO TECHNOLOGIES.",
                    "Error en Actualización",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string versionString = $"{remoteVersion.Major}.{remoteVersion.Minor}.{remoteVersion.Build}";
            string fileName = Path.GetFileName(info.Url);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = $"SistemaBiometricoPolicia-{versionString}-setup.exe";
            }

            string tempPath = Path.Combine(Path.GetTempPath(), fileName);

            try
            {
                StatusHub.PushEvento($"📥 Descargando actualización desde {info.Url}...");

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(15);
                    using (var response = await client.GetAsync(info.Url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                }

                LogHelper.RegistrarEvento($"Instalador descargado en {tempPath}", "UPDATE");
                StatusHub.PushEvento("✓ Descarga completada");

                var msg = "✅ DESCARGA COMPLETADA\n\n" +
                          "La aplicación se cerrará para realizar la actualización.\n" +
                          "Al finalizar la instalación, por favor abra el sistema nuevamente.\n\n" +
                          "IMPORTANTE: No apague el equipo durante la instalación.";

                MessageBox.Show(parentForm, msg, "Instalación de Actualización",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                var psi = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(psi);
                Application.Exit();
            }
            catch (HttpRequestException ex)
            {
                LogHelper.RegistrarError("Error de red al descargar actualización", ex);
                MessageBox.Show(parentForm,
                    "❌ No se pudo descargar la actualización.\n\n" +
                    "Posibles causas:\n" +
                    "• Sin conexión a internet\n" +
                    "• Archivo no disponible en el servidor\n\n" +
                    "Contacte a TRUJO TECHNOLOGIES:\n" +
                    "+57 317 294 6935 | +57 312 590 5106",
                    "Error en Actualización",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al ejecutar el instalador de actualización", ex);
                MessageBox.Show(parentForm,
                    "❌ Error al ejecutar la actualización.\n\n" +
                    $"Detalles: {ex.Message}\n\n" +
                    "Contacte a soporte técnico.",
                    "Error en Actualización",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}