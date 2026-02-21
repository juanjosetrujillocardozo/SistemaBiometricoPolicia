using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SistemaBiometricoPolicia.Utils
{
    public static class UpdateService
    {
        private const string VERSION_URL = "https://licencias.trujotechnologies.com/version.txt";
        private const string SETUP_BASE_URL = "https://licencias.trujotechnologies.com";
        private const string SETUP_FILE_PREFIX = "SistemaBiometricoPolicia-";
        private const string SETUP_FILE_SUFFIX = "-setup.exe";

        /// <summary>
        /// Obtiene la versión actual de la aplicación desde el Assembly.
        /// </summary>
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

        /// <summary>
        /// Descarga la versión disponible en el servidor (version.txt).
        /// Retorna null si falla.
        /// </summary>
        public static async Task<Version> GetRemoteVersionAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var content = await client.GetStringAsync(VERSION_URL);
                    if (string.IsNullOrWhiteSpace(content))
                        return null;

                    var versionString = content.Trim();
                    if (Version.TryParse(versionString, out var version))
                    {
                        return version;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al obtener versión remota", ex);
            }

            return null;
        }

        /// <summary>
        /// Compara versión local vs remota y, si hay nueva, ofrece actualizar.
        /// </summary>
        public static async Task CheckForUpdatesAsync(Form parentForm)
        {
            try
            {
                var localVersion = GetLocalVersion();
                var remoteVersion = await GetRemoteVersionAsync();

                if (remoteVersion == null)
                {
                    StatusHub.PushEvento("⚠ No se pudo verificar actualizaciones (sin internet o servidor no disponible)");
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
                              "¿Desea descargar e instalar la actualización ahora?\n\n" +
                              "NOTA: El sistema se cerrará durante la instalación.";

                var result = MessageBox.Show(parentForm, message, "Actualización Disponible - TRUJO TECHNOLOGIES",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    await DownloadAndRunInstallerAsync(remoteVersion, parentForm);
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

        private static async Task DownloadAndRunInstallerAsync(Version remoteVersion, Form parentForm)
        {
            string versionString = $"{remoteVersion.Major}.{remoteVersion.Minor}.{remoteVersion.Build}";
            string setupFileName = SETUP_FILE_PREFIX + versionString + SETUP_FILE_SUFFIX;
            string setupUrl = SETUP_BASE_URL + "/" + setupFileName;
            string tempPath = Path.Combine(Path.GetTempPath(), setupFileName);

            try
            {
                StatusHub.PushEvento($"📥 Descargando actualización desde {setupUrl}...");

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(15); // Aumentado para conexiones lentas

                    using (var response = await client.GetAsync(setupUrl, HttpCompletionOption.ResponseHeadersRead))
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

                MessageBox.Show(parentForm, msg, "Instalación de Actualización", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                var psi = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true,
                    Verb = "runas" // Ejecutar como administrador
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