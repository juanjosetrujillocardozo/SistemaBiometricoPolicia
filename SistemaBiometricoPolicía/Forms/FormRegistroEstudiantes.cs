using OfficeOpenXml;
using SistemaBiometricoPolicia.Data;
using SistemaBiometricoPolicia.Utils;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SistemaBiometricoPolicia.Forms
{
    public partial class FormRegistroEstudiantes : Form
    {
        private bool procesandoImportacion = false;

        public FormRegistroEstudiantes()
        {
            InitializeComponent();
            CargarListaEstudiantes();
        }

        private async void btnImportarExcel_Click(object sender, EventArgs e)
        {
            if (procesandoImportacion) return;

            try
            {
                procesandoImportacion = true;
                btnImportarExcel.Enabled = false;
                btnImportarExcel.Text = "? Procesando... Espere";
                this.Cursor = Cursors.WaitCursor;

                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Archivos de Excel (*.xlsx)|*.xlsx";
                    openFileDialog.Title = "Seleccionar Listado de Estudiantes";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string rutaArchivo = openFileDialog.FileName;

                        // CORRECCIÓN: Llamar al método de esta misma clase
                        int totalImportados = await Task.Run(() => ImportarEstudiantesDesdeExcel(rutaArchivo));

                        MessageBox.Show($"? Importación completada con éxito.\n\nTotal importados: {totalImportados}",
                            "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        CargarListaEstudiantes(); // Actualizar el contador
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error durante la importación:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.RegistrarError("Error en importación Excel", ex);
            }
            finally
            {
                procesandoImportacion = false;
                btnImportarExcel.Enabled = true;
                btnImportarExcel.Text = "?? Importar desde Excel";
                this.Cursor = Cursors.Default;
            }
        }

        private int ImportarEstudiantesDesdeExcel(string rutaArchivo)
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            int totalImportados = 0;

            using (var package = new ExcelPackage(new FileInfo(rutaArchivo)))
            {
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    string nombreHoja = worksheet.Name.Trim().ToUpper();
                    int rowCount = worksheet.Dimension?.Rows ?? 0;

                    using (var conn = DatabaseHelper.ObtenerConexion())
                    {
                        conn.Open();
                        using (var transaction = conn.BeginTransaction())
                        {
                            string seccionInterna = "SIN SECCIÓN";

                            for (int row = 1; row <= rowCount; row++)
                            {
                                try
                                {
                                    var celdaA = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                    var celdaB = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                    var celdaC = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                    var celdaD = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                                    if (!string.IsNullOrEmpty(celdaA) &&
                                        (celdaA.Contains("SECCIÓN") || celdaA.Contains("SECCION")))
                                    {
                                        seccionInterna = celdaA.ToUpper();
                                        continue;
                                    }

                                    if (!string.IsNullOrEmpty(celdaA) &&
                                        (celdaA.Equals("NRO.", StringComparison.OrdinalIgnoreCase) ||
                                         celdaA.Equals("NO", StringComparison.OrdinalIgnoreCase)))
                                    {
                                        continue;
                                    }

                                    if (!string.IsNullOrEmpty(celdaA) && !string.IsNullOrEmpty(celdaB) && int.TryParse(celdaA, out _))
                                    {
                                        string nombreCompleto = celdaB;
                                        string documento = !string.IsNullOrEmpty(celdaD) ? celdaD : celdaC;

                                        documento = new string(documento.Where(char.IsDigit).ToArray());
                                        if (documento.Length < 5) continue;

                                        string seccionCompleta = $"{nombreHoja} - {seccionInterna}";

                                        string[] partes = nombreCompleto.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                        string apellidos = "POR DEFINIR";
                                        string nombres = nombreCompleto;

                                        if (partes.Length >= 2)
                                        {
                                            apellidos = partes.Length >= 4 ? $"{partes[0]} {partes[1]}" : partes[0];
                                            nombres = partes.Length >= 4 ? string.Join(" ", partes.Skip(2)) : string.Join(" ", partes.Skip(1));
                                        }

                                        string sql = @"INSERT OR REPLACE INTO Estudiantes (NumeroDocumento, Nombres, Apellidos, Seccion) 
                                              VALUES (@doc, @nombres, @apellidos, @seccion)";

                                        using (var cmd = new SQLiteCommand(sql, conn, transaction))
                                        {
                                            cmd.Parameters.AddWithValue("@doc", documento);
                                            cmd.Parameters.AddWithValue("@nombres", nombres.ToUpper());
                                            cmd.Parameters.AddWithValue("@apellidos", apellidos.ToUpper());
                                            cmd.Parameters.AddWithValue("@seccion", seccionCompleta.ToUpper());

                                            if (cmd.ExecuteNonQuery() > 0)
                                                totalImportados++;
                                        }
                                    }
                                }
                                catch { continue; }
                            }
                            transaction.Commit();
                        }
                    }
                }
            }
            return totalImportados;
        }

        private void CargarListaEstudiantes()
        {
            try
            {
                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM Estudiantes";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        int total = Convert.ToInt32(cmd.ExecuteScalar());
                        if (lblTotalEstudiantes != null)
                            lblTotalEstudiantes.Text = $"Total de estudiantes: {total}";
                    }
                }
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            this.Cursor = Cursors.Default;
            base.OnFormClosing(e);
        }
    }
}