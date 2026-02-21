using OfficeOpenXml;
using OfficeOpenXml.Style;
using SistemaBiometricoPolicia.Data;
using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;

namespace SistemaBiometricoPolicia.Utils
{
    public static class ExcelReportGenerator
    {
        public static string GenerarReporteMensual(int mes, int año)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Carpeta segura dentro del programa
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reportes");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = $"Reporte_Alimentacion_{año}_{mes:00}.xlsx";
            string filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath)) File.Delete(filePath);

            using (ExcelPackage package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("RESUMEN MENSUAL");

                // 1. ENCABEZADO INSTITUCIONAL
                ws.Cells[1, 1].Value = "REPORTE DE ALIMENTACIÓN - POLICÍA NACIONAL";
                ws.Cells[1, 1, 1, 7].Merge = true;
                ws.Cells[1, 1].Style.Font.Bold = true;
                ws.Cells[1, 1].Style.Font.Size = 16;
                ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 32, 96)); // Azul Oscuro Policía
                ws.Cells[1, 1].Style.Font.Color.SetColor(Color.White);

                ws.Cells[2, 1].Value = $"Mes: {mes:00}/{año}";
                ws.Cells[2, 1, 2, 7].Merge = true;
                ws.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[2, 1].Style.Font.Size = 12;

                // 2. TÍTULOS
                string[] headers = { "Documento", "Apellidos", "Nombres", "Sección", "Días Desayuno", "Días Almuerzo", "Días Cena" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[4, i + 1].Value = headers[i];
                    ws.Cells[4, i + 1].Style.Font.Bold = true;
                    ws.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    ws.Cells[4, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Cells[4, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // 3. DATOS
                int row = 5;
                int tDes = 0, tAlm = 0, tCen = 0;

                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    string sql = @"
                        SELECT e.NumeroDocumento, e.Apellidos, e.Nombres, e.Seccion,
                        COUNT(DISTINCT CASE WHEN r.TipoServicio = 'DESAYUNO' THEN DATE(r.FechaHora) END) as Des,
                        COUNT(DISTINCT CASE WHEN r.TipoServicio = 'ALMUERZO' THEN DATE(r.FechaHora) END) as Alm,
                        COUNT(DISTINCT CASE WHEN r.TipoServicio = 'CENA' THEN DATE(r.FechaHora) END) as Cen
                        FROM Estudiantes e
                        LEFT JOIN RegistrosAlimentacion r ON e.Id = r.EstudianteId 
                            AND strftime('%m', r.FechaHora) = @mes 
                            AND strftime('%Y', r.FechaHora) = @año
                        GROUP BY e.Id ORDER BY e.Seccion, e.Apellidos";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@mes", mes.ToString("00"));
                        cmd.Parameters.AddWithValue("@año", año.ToString());
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ws.Cells[row, 1].Value = reader["NumeroDocumento"];
                                ws.Cells[row, 2].Value = reader["Apellidos"];
                                ws.Cells[row, 3].Value = reader["Nombres"];
                                ws.Cells[row, 4].Value = reader["Seccion"];
                                ws.Cells[row, 5].Value = Convert.ToInt32(reader["Des"]);
                                ws.Cells[row, 6].Value = Convert.ToInt32(reader["Alm"]);
                                ws.Cells[row, 7].Value = Convert.ToInt32(reader["Cen"]);

                                // Bordes
                                for (int col = 1; col <= 7; col++)
                                {
                                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                }

                                tDes += Convert.ToInt32(reader["Des"]);
                                tAlm += Convert.ToInt32(reader["Alm"]);
                                tCen += Convert.ToInt32(reader["Cen"]);
                                row++;
                            }
                        }
                    }
                }

                // 4. TOTALES (Fila Amarilla)
                ws.Cells[row, 1, row, 4].Merge = true;
                ws.Cells[row, 1].Value = "TOTALES GENERALES";
                ws.Cells[row, 1].Style.Font.Bold = true;
                ws.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells[row, 5].Value = tDes;
                ws.Cells[row, 6].Value = tAlm;
                ws.Cells[row, 7].Value = tCen;
                ws.Cells[row, 5, row, 7].Style.Font.Bold = true;
                ws.Cells[row, 5, row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, 5, row, 7].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                ws.Cells[row, 5, row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // 5. PIE DE PÁGINA
                ws.Cells[row + 2, 1].Value = "SISTEMA DE CONTROL BIOMÉTRICO - TRUJO TECHNOLOGIES S.A.S.";
                ws.Cells[row + 3, 1].Value = "Contacto Soporte: +57 317 294 6935 | +57 312 590 5106";
                ws.Cells[row + 4, 1].Value = "Visítenos en: https://trujotechnologies.com/";
                ws.Cells[row + 2, 1, row + 4, 1].Style.Font.Italic = true;
                ws.Cells[row + 2, 1, row + 4, 1].Style.Font.Color.SetColor(Color.Gray);

                ws.Cells.AutoFitColumns();
                package.SaveAs(new FileInfo(filePath));
            }
            return filePath;
        }

        public static string GenerarReporteDetallado(DateTime inicio, DateTime fin)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reportes");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = $"Reporte_Detallado_{inicio:yyyyMMdd}.xlsx";
            string filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath)) File.Delete(filePath);

            using (ExcelPackage package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("DETALLE AUDITORÍA");

                // 1. ENCABEZADO
                ws.Cells[1, 1].Value = "REPORTE DETALLADO DE CONSUMOS";
                ws.Cells[1, 1, 1, 8].Merge = true;
                ws.Cells[1, 1].Style.Font.Bold = true;
                ws.Cells[1, 1].Style.Font.Size = 16;
                ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.DarkGreen);
                ws.Cells[1, 1].Style.Font.Color.SetColor(Color.White);

                ws.Cells[2, 1].Value = $"Período: {inicio:dd/MM/yyyy} - {fin:dd/MM/yyyy}";
                ws.Cells[2, 1, 2, 8].Merge = true;
                ws.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[2, 1].Style.Font.Size = 11;

                // 2. TÍTULOS (AHORA CON MÁS COLUMNAS PARA AUDITORÍA)
                string[] headers = { "Documento", "Estudiante", "Sección", "Fecha", "Hora", "Servicio", "Día Semana", "Observaciones" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[4, i + 1].Value = headers[i];
                    ws.Cells[4, i + 1].Style.Font.Bold = true;
                    ws.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    ws.Cells[4, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Cells[4, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // 3. DATOS DETALLADOS
                int row = 5;
                int totalRegistros = 0;

                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            e.NumeroDocumento, 
                            e.Apellidos || ' ' || e.Nombres as Nombre, 
                            e.Seccion, 
                            r.FechaHora, 
                            r.TipoServicio,
                            r.Id
                        FROM RegistrosAlimentacion r 
                        JOIN Estudiantes e ON r.EstudianteId = e.Id
                        WHERE date(r.FechaHora) BETWEEN date(@ini) AND date(@fin)
                        ORDER BY r.FechaHora DESC";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ini", inicio.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@fin", fin.ToString("yyyy-MM-dd"));
                        using (var reader = cmd.ExecuteReader())
                        {
                            string ultimoDocumento = "";
                            string ultimaFecha = "";
                            string ultimoServicio = "";

                            while (reader.Read())
                            {
                                DateTime fh = Convert.ToDateTime(reader["FechaHora"]);
                                string documento = reader["NumeroDocumento"].ToString();
                                string servicio = reader["TipoServicio"].ToString();
                                string fecha = fh.ToString("dd/MM/yyyy");

                                ws.Cells[row, 1].Value = documento;
                                ws.Cells[row, 2].Value = reader["Nombre"];
                                ws.Cells[row, 3].Value = reader["Seccion"];
                                ws.Cells[row, 4].Value = fecha;
                                ws.Cells[row, 5].Value = fh.ToString("HH:mm:ss");
                                ws.Cells[row, 6].Value = servicio;
                                ws.Cells[row, 7].Value = ObtenerDiaSemana(fh);

                                // 🔍 DETECCIÓN DE ANOMALÍAS (Doble consumo)
                                string observacion = "";
                                if (documento == ultimoDocumento && fecha == ultimaFecha && servicio == ultimoServicio)
                                {
                                    observacion = "⚠️ POSIBLE DUPLICADO";
                                    ws.Cells[row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    ws.Cells[row, 8].Style.Fill.BackgroundColor.SetColor(Color.Orange);
                                    ws.Cells[row, 8].Style.Font.Bold = true;
                                }

                                ws.Cells[row, 8].Value = observacion;

                                // Bordes
                                for (int col = 1; col <= 8; col++)
                                {
                                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                }

                                ultimoDocumento = documento;
                                ultimaFecha = fecha;
                                ultimoServicio = servicio;

                                totalRegistros++;
                                row++;
                            }
                        }
                    }
                }

                // 4. RESUMEN FINAL
                ws.Cells[row + 1, 1, row + 1, 3].Merge = true;
                ws.Cells[row + 1, 1].Value = "TOTAL DE REGISTROS:";
                ws.Cells[row + 1, 1].Style.Font.Bold = true;
                ws.Cells[row + 1, 4].Value = totalRegistros;
                ws.Cells[row + 1, 4].Style.Font.Bold = true;
                ws.Cells[row + 1, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row + 1, 4].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

                // 5. PIE DE PÁGINA
                ws.Cells[row + 3, 1].Value = "SISTEMA DE CONTROL BIOMÉTRICO - TRUJO TECHNOLOGIES S.A.S.";
                ws.Cells[row + 4, 1].Value = "Contacto Soporte: +57 317 294 6935 | +57 312 590 5106";
                ws.Cells[row + 5, 1].Value = "Visítenos en: https://trujotechnologies.com/";
                ws.Cells[row + 3, 1, row + 5, 1].Style.Font.Italic = true;
                ws.Cells[row + 3, 1, row + 5, 1].Style.Font.Color.SetColor(Color.Gray);

                ws.Cells.AutoFitColumns();
                package.SaveAs(new FileInfo(filePath));
            }
            return filePath;
        }

        // 📅 Función auxiliar para obtener el día de la semana en español
        private static string ObtenerDiaSemana(DateTime fecha)
        {
            switch (fecha.DayOfWeek)
            {
                case DayOfWeek.Monday: return "Lunes";
                case DayOfWeek.Tuesday: return "Martes";
                case DayOfWeek.Wednesday: return "Miércoles";
                case DayOfWeek.Thursday: return "Jueves";
                case DayOfWeek.Friday: return "Viernes";
                case DayOfWeek.Saturday: return "Sábado";
                case DayOfWeek.Sunday: return "Domingo";
                default: return "";
            }
        }
    }
}