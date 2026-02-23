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

            // ✅ CRÍTICO: Habilitar TLS 1.2, 1.1 y 1.0 para compatibilidad con Windows 7
            // Windows 7 no habilita TLS 1.2 por defecto; sin esto, las llamadas HTTPS fallan.
            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12 |
                System.Net.SecurityProtocolType.Tls11 |
                System.Net.SecurityProtocolType.Tls;

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

            Application.Run(new FormPrincipal());

            // Detener monitor al salir (FormPrincipal.OnFormClosing lo hace primero, esto es por seguridad)
            SystemHealthMonitor.Stop();
        }
    }
}