using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using SistemaBiometricoPolicia.Data;

namespace SistemaBiometricoPolicia.Utils
{
    public static class StartupValidator
    {
        public static bool ValidarTodo(out string mensajeError)
        {
            mensajeError = "";
            List<string> errores = new List<string>();

            // 1. Verificar Carpetas Críticas
            string[] carpetas = { "Sonidos", "Fotos", "FotosEstudiantes", "FotosConsumo", "Reportes", "Logs" };
            foreach (string carpeta in carpetas)
            {
                string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, carpeta);
                if (!Directory.Exists(ruta))
                {
                    try { Directory.CreateDirectory(ruta); }
                    catch { errores.Add($"- No se pudo crear la carpeta: {carpeta}"); }
                }
            }

            // 2. Verificar Archivos de Sonido (Vital para el comedor)
            string[] sonidos = { "success.wav", "error.wav", "alert.wav" };
            foreach (string sonido in sonidos)
            {
                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sonidos", sonido)))
                {
                    errores.Add($"- Falta archivo de sonido: Sonidos/{sonido}");
                }
            }

            // 3. Verificar Base de Datos
            try
            {
                DatabaseHelper.InicializarBaseDeDatos();
            }
            catch (Exception ex)
            {
                errores.Add("- Error al conectar con la Base de Datos: " + ex.Message);
            }

            // 4. Verificar DLLs Críticas (Evita que el programa explote luego)
            string[] dlls = { "DPFPDevNET.dll", "DPFPEngNET.dll", "System.Data.SQLite.dll", "EPPlus.dll" };
            foreach (string dll in dlls)
            {
                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dll)))
                {
                    errores.Add($"- Falta componente crítico: {dll}");
                }
            }

            if (errores.Count > 0)
            {
                mensajeError = string.Join(Environment.NewLine, errores);
                return false;
            }

            return true;
        }
    }
}