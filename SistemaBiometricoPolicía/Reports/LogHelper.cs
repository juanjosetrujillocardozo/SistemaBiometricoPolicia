using System;
using System.IO;

namespace SistemaBiometricoPolicia.Utils
{
    public static class LogHelper
    {
        private static string logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SistemaBiometrico",
            "Logs"
        );

        static LogHelper()
        {
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);
        }

        public static void RegistrarEvento(string mensaje, string tipo = "INFO")
        {
            try
            {
                string fileName = $"log_{DateTime.Now:yyyyMMdd}.txt";
                string fullPath = Path.Combine(logPath, fileName);

                string linea = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{tipo}] {mensaje}";

                File.AppendAllText(fullPath, linea + Environment.NewLine);
            }
            catch { }
        }

        public static void RegistrarError(string mensaje, Exception ex)
        {
            RegistrarEvento($"ERROR: {mensaje} - {ex.Message}\nStack: {ex.StackTrace}", "ERROR");
        }
    }
}