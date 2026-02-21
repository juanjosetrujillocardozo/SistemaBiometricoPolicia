using SistemaBiometricoPolicia.Data;
using System;
using System.Data.SQLite;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace SistemaBiometricoPolicia.Utils
{
    public static class SystemHealthMonitor
    {
        private static Timer _timer;
        private static int _running = 0;

        public static void Start()
        {
            if (_timer != null) return;
            StatusHub.PushEvento("Monitor de estado iniciado.");
            _timer = new Timer(async _ => await Tick(), null, 0, 2000);
        }

        public static void Stop()
        {
            _timer?.Dispose();
            _timer = null;
            StatusHub.PushEvento("Monitor de estado detenido.");
        }

        private static async Task Tick()
        {
            if (Interlocked.Exchange(ref _running, 1) == 1) return;

            try
            {
                await Task.WhenAll(
                    Task.Run(CheckDB),
                    Task.Run(CheckNetwork),
                    Task.Run(CheckBiometricoDetectado),
                    Task.Run(CheckCamaraDetectada)
                );
            }
            catch (Exception ex)
            {
                StatusHub.Set("MONITOR", EstadoComponente.Error, ex.Message);
                StatusHub.PushEvento("Error en monitor: " + ex.Message);
            }
            finally
            {
                Interlocked.Exchange(ref _running, 0);
            }
        }

        private static void CheckDB()
        {
            try
            {
                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Estudiantes';", conn))
                    {
                        var r = cmd.ExecuteScalar();
                        if (r == null)
                            StatusHub.Set("BD", EstadoComponente.Advertencia, "Conectó, pero falta tabla Estudiantes.");
                        else
                            StatusHub.Set("BD", EstadoComponente.Ok, "Conectada y tablas OK.");
                    }
                }
            }
            catch (Exception ex)
            {
                StatusHub.Set("BD", EstadoComponente.Error, "Fallo conexión SQLite: " + ex.Message);
            }
        }

        private static void CheckNetwork()
        {
            try
            {
                bool hayRed = NetworkInterface.GetIsNetworkAvailable();
                StatusHub.Set("RED", hayRed ? EstadoComponente.Ok : EstadoComponente.Advertencia,
                    hayRed ? "Red disponible." : "Sin red (modo offline).");
            }
            catch (Exception ex)
            {
                StatusHub.Set("RED", EstadoComponente.Error, ex.Message);
            }
        }

        private static void CheckCamaraDetectada()
        {
            try
            {
                int cams = CountPnPDevicesByKeywords(new[] { "camera", "webcam", "usb video", "integrated camera" });
                if (cams > 0)
                    StatusHub.Set("CAMARA", EstadoComponente.Ok, $"Detectada ({cams}).");
                else
                    StatusHub.Set("CAMARA", EstadoComponente.Advertencia, "No se detecta cámara.");
            }
            catch (Exception ex)
            {
                StatusHub.Set("CAMARA", EstadoComponente.Error, ex.Message);
            }
        }

        private static void CheckBiometricoDetectado()
        {
            try
            {
                int bio = CountPnPDevicesByKeywords(new[] { "digitalpersona", "u.are.u", "fingerprint", "hid global" });
                if (bio > 0)
                    StatusHub.Set("BIOMETRICO", EstadoComponente.Ok, $"Detectado en Windows ({bio}).");
                else
                    StatusHub.Set("BIOMETRICO", EstadoComponente.Advertencia, "No se detecta (driver/USB).");
            }
            catch (Exception ex)
            {
                StatusHub.Set("BIOMETRICO", EstadoComponente.Error, ex.Message);
            }
        }

        private static int CountPnPDevicesByKeywords(string[] keywords)
        {
            var query = "SELECT Name FROM Win32_PnPEntity WHERE Name IS NOT NULL";
            using (var searcher = new ManagementObjectSearcher(query))
            using (var results = searcher.Get())
            {
                int count = 0;
                foreach (ManagementObject obj in results)
                {
                    var name = (obj["Name"]?.ToString() ?? "").ToLowerInvariant();
                    if (keywords.Any(k => name.Contains(k)))
                        count++;
                }
                return count;
            }
        }
    }
}