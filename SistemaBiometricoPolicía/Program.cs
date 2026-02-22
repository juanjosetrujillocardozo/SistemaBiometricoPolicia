using SistemaBiometricoPolicia.Forms;
using SistemaBiometricoPolicia.Utils;
using System;
using System.Windows.Forms;

namespace SistemaBiometricoPolicia
{
    static class Program
    {
        [STAThread]
        static void Main()
        {

            // ✅ CRÍTICO: Forzar TLS 1.2 para que funcione en Windows 7 con servidores HTTPS modernos
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

#pragma warning disable CS0618
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
#pragma warning restore CS0618

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.ThreadException += (s, e) =>
            {
                StatusHub.Set("APP", EstadoComponente.Error, e.Exception.Message);
                StatusHub.PushEvento("EXCEPCION UI: " + e.Exception);
                LogHelper.RegistrarError("EXCEPCION UI", e.Exception);
                MessageBox.Show(e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                StatusHub.Set("APP", EstadoComponente.Error, ex?.Message ?? "Error desconocido");
                StatusHub.PushEvento("EXCEPCION FATAL: " + ex);
                if (ex != null) LogHelper.RegistrarError("EXCEPCION FATAL", ex);
            };

            // Iniciar monitor de estado
            SystemHealthMonitor.Start();
            StatusHub.PushEvento("=== SISTEMA INICIADO ===");

            Application.Run(new FormPrincipal());

            // Detener monitor al salir
            SystemHealthMonitor.Stop();
        }
    }
}