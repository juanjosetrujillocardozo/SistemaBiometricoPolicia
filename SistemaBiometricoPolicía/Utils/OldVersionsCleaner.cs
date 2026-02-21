using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace SistemaBiometricoPolicia.Utils
{
    public class InstalledVersionInfo
    {
        public string DisplayName { get; set; }
        public string DisplayVersion { get; set; }
        public string UninstallString { get; set; }

        public override string ToString()
        {
            return $"{DisplayName}  (v{DisplayVersion})";
        }
    }

    public static class OldVersionsCleaner
    {
        private const string PRODUCTO_NOMBRE_BASE = "Sistema Biométrico - TRUJO TECHNOLOGIES";

        public static List<InstalledVersionInfo> BuscarVersionesInstaladas()
        {
            var lista = new List<InstalledVersionInfo>();

            try
            {
                // Rutas típicas de uninstall en Windows 7 (32 y 64 bits)
                string[] rutas = new[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
                };

                foreach (var ruta in rutas)
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(ruta))
                    {
                        if (key == null) continue;

                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            try
                            {
                                using (var subKey = key.OpenSubKey(subKeyName))
                                {
                                    if (subKey == null) continue;

                                    string displayName = subKey.GetValue("DisplayName") as string;
                                    if (string.IsNullOrEmpty(displayName)) continue;

                                    if (!displayName.Contains(PRODUCTO_NOMBRE_BASE))
                                        continue;

                                    string displayVersion = subKey.GetValue("DisplayVersion") as string ?? "N/D";
                                    string uninstallString = subKey.GetValue("UninstallString") as string;

                                    if (!string.IsNullOrEmpty(uninstallString))
                                    {
                                        lista.Add(new InstalledVersionInfo
                                        {
                                            DisplayName = displayName,
                                            DisplayVersion = displayVersion,
                                            UninstallString = uninstallString
                                        });
                                    }
                                }
                            }
                            catch
                            {
                                // ignorar entradas corruptas
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("OldVersionsCleaner.BuscarVersionesInstaladas", ex);
            }

            return lista;
        }

        public static void DesinstalarVersionesAntiguas(Form owner)
        {
            try
            {
                var versiones = BuscarVersionesInstaladas();

                if (versiones.Count <= 1)
                {
                    StatusHub.PushEvento("🧹 No se encontraron versiones antiguas para desinstalar.");
                    return;
                }

                // Ordenar por versión de texto (simple), la última asumimos que es la más nueva
                versiones.Sort((a, b) => string.Compare(a.DisplayVersion, b.DisplayVersion, StringComparison.OrdinalIgnoreCase));

                var masReciente = versiones[versiones.Count - 1];
                var antiguas = versiones.GetRange(0, versiones.Count - 1);

                string msg = "Se encontraron las siguientes versiones del sistema instaladas:\n\n";
                foreach (var v in versiones)
                    msg += $"- {v.DisplayName} (v{v.DisplayVersion})\n";

                msg += $"\nLa versión más reciente detectada es: {masReciente.DisplayVersion}.\n" +
                       "¿Desea DESINSTALAR automáticamente las versiones más antiguas y dejar solo la más reciente?";

                var r = MessageBox.Show(owner, msg, "Limpiar versiones antiguas",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (r != DialogResult.Yes)
                {
                    StatusHub.PushEvento("🧹 Limpieza de versiones antiguas cancelada por el usuario.");
                    return;
                }

                foreach (var v in antiguas)
                {
                    try
                    {
                        StatusHub.PushEvento($"🧹 Desinstalando: {v.DisplayName} (v{v.DisplayVersion})");
                        EjecutarUninstall(v.UninstallString);
                    }
                    catch (Exception ex)
                    {
                        StatusHub.PushEvento($"✗ Error al desinstalar {v.DisplayName}: {ex.Message}");
                        LogHelper.RegistrarError("OldVersionsCleaner.DesinstalarVersion", ex);
                    }
                }

                MessageBox.Show(owner,
                    "Proceso de desinstalación de versiones antiguas finalizado.\n" +
                    "Es posible que Windows solicite confirmar algunas desinstalaciones.",
                    "Limpieza completada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                StatusHub.PushEvento("✗ Error en limpieza de versiones antiguas: " + ex.Message);
                LogHelper.RegistrarError("OldVersionsCleaner.DesinstalarVersionesAntiguas", ex);
            }
        }

        private static void EjecutarUninstall(string uninstallString)
        {
            // Algunos uninstallString vienen entre comillas y con parámetros, los respetamos.
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C " + uninstallString,
                UseShellExecute = true,
                Verb = "runas" // intentar como administrador
            };
            Process.Start(psi);
        }
    }
}