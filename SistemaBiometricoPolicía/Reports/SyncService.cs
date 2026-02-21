using System;
using System.IO;
using System.Threading.Tasks;

namespace SistemaBiometricoPolicia.Utils
{
    public class SyncService
    {
        public event Action<string> OnProgress;

        public async Task<bool> SincronizarAsync(string rutaDestino)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(rutaDestino) || !Directory.Exists(rutaDestino))
                    {
                        OnProgress?.Invoke("❌ Error: Ruta de destino no válida.");
                        return false;
                    }

                    OnProgress?.Invoke("🚀 Iniciando sincronización...");

                    // 1. Sincronizar Base de Datos (usando la ruta real de DatabaseHelper)
                    OnProgress?.Invoke("📂 Localizando base de datos...");
                    string dbPath = Data.DatabaseHelper.ObtenerRutaBaseDeDatos();

                    if (!File.Exists(dbPath))
                    {
                        OnProgress?.Invoke($"❌ ERROR: No se encontró la base de datos en: {dbPath}");
                        return false;
                    }

                    string dbDestino = Path.Combine(rutaDestino, $"biometrico_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                    File.Copy(dbPath, dbDestino, true);
                    OnProgress?.Invoke($"✅ Base de datos copiada: {Path.GetFileName(dbDestino)}");

                    // 2. Sincronizar Fotos
                    string folderFotos = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fotos");
                    string folderFotosDestino = Path.Combine(rutaDestino, "Fotos");

                    if (Directory.Exists(folderFotos))
                    {
                        if (!Directory.Exists(folderFotosDestino))
                            Directory.CreateDirectory(folderFotosDestino);

                        string[] fotos = Directory.GetFiles(folderFotos, "*.jpg");
                        OnProgress?.Invoke($"📸 Sincronizando {fotos.Length} fotos...");

                        int contador = 0;
                        foreach (string foto in fotos)
                        {
                            string nombreArchivo = Path.GetFileName(foto);
                            File.Copy(foto, Path.Combine(folderFotosDestino, nombreArchivo), true);
                            contador++;

                            if (contador % 10 == 0)
                                OnProgress?.Invoke($"   → {contador}/{fotos.Length} fotos copiadas...");
                        }
                        OnProgress?.Invoke($"✅ {fotos.Length} fotos sincronizadas.");
                    }
                    else
                    {
                        OnProgress?.Invoke("⚠️ Carpeta de fotos no encontrada (se creará al capturar fotos).");
                    }

                    // 3. Sincronizar Reportes Excel
                    string folderReportes = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                    string folderReportesDestino = Path.Combine(rutaDestino, "Reportes_Excel");

                    if (Directory.Exists(folderReportes))
                    {
                        if (!Directory.Exists(folderReportesDestino))
                            Directory.CreateDirectory(folderReportesDestino);

                        string[] excels = Directory.GetFiles(folderReportes, "*.xlsx");
                        OnProgress?.Invoke($"📊 Sincronizando {excels.Length} reportes Excel...");

                        foreach (string excel in excels)
                        {
                            string nombreArchivo = Path.GetFileName(excel);
                            File.Copy(excel, Path.Combine(folderReportesDestino, nombreArchivo), true);
                            OnProgress?.Invoke($"   → {nombreArchivo}");
                        }
                        OnProgress?.Invoke($"✅ {excels.Length} reportes sincronizados.");
                    }
                    else
                    {
                        OnProgress?.Invoke("⚠️ Carpeta de reportes no encontrada (se creará al generar reportes).");
                    }

                    // 4. Sincronizar Logs
                    string folderLogs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                    string folderLogsDestino = Path.Combine(rutaDestino, "Logs");

                    if (Directory.Exists(folderLogs))
                    {
                        if (!Directory.Exists(folderLogsDestino))
                            Directory.CreateDirectory(folderLogsDestino);

                        string[] logs = Directory.GetFiles(folderLogs, "*.txt");
                        OnProgress?.Invoke($"📝 Sincronizando {logs.Length} archivos de log...");

                        foreach (string log in logs)
                        {
                            string nombreArchivo = Path.GetFileName(log);
                            File.Copy(log, Path.Combine(folderLogsDestino, nombreArchivo), true);
                        }
                        OnProgress?.Invoke($"✅ {logs.Length} logs sincronizados.");
                    }

                    // 5. Crear archivo de resumen
                    string archivoResumen = Path.Combine(rutaDestino, $"Sincronizacion_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                    File.WriteAllText(archivoResumen,
                        $"═══════════════════════════════════════════════════════\n" +
                        $"  SINCRONIZACIÓN EXITOSA - TRUJO TECHNOLOGIES S.A.S.\n" +
                        $"═══════════════════════════════════════════════════════\n\n" +
                        $"Fecha y Hora:    {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
                        $"Equipo:          {Environment.MachineName}\n" +
                        $"Usuario:         {Environment.UserName}\n" +
                        $"Sistema:         Control de Alimentación - Policía Nacional\n" +
                        $"Base de Datos:   {Path.GetFileName(dbPath)}\n" +
                        $"Destino:         {rutaDestino}\n\n" +
                        $"═══════════════════════════════════════════════════════\n" +
                        $"Contacto Soporte: +57 317 294 6935 | +57 312 590 5106\n" +
                        $"Web: https://trujotechnologies.com/\n" +
                        $"═══════════════════════════════════════════════════════\n");

                    OnProgress?.Invoke("🏁 Sincronización finalizada con éxito.");
                    LogHelper.RegistrarEvento($"Sincronización exitosa a: {rutaDestino}", "INFO");
                    return true;
                }
                catch (Exception ex)
                {
                    OnProgress?.Invoke($"⚠️ Error en sincronización: {ex.Message}");
                    LogHelper.RegistrarError("SyncService.Sincronizar", ex);
                    return false;
                }
            });
        }
    }
}