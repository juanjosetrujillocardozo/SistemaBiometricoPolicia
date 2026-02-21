using System;
using System.Windows.Forms;
using SistemaBiometricoPolicia.Utils;
using System.IO;

namespace SistemaBiometricoPolicia.Forms
{
    public partial class FormReportes : Form
    {
        public FormReportes()
        {
            InitializeComponent();
            // Configurar el selector de fecha para que solo muestre Mes y Año
            dtpMes.Format = DateTimePickerFormat.Custom;
            dtpMes.CustomFormat = "MMMM yyyy";
            dtpMes.ShowUpDown = true;
        }

        private void btnGenerarMensual_Click(object sender, EventArgs e)
        {
            try
            {
                int mes = dtpMes.Value.Month;
                int año = dtpMes.Value.Year;

                string ruta = ExcelReportGenerator.GenerarReporteMensual(mes, año);

                MessageBox.Show("? Reporte Mensual generado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Abrir la carpeta y seleccionar el archivo
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{ruta}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerarDetallado_Click(object sender, EventArgs e)
        {
            try
            {
                string ruta = ExcelReportGenerator.GenerarReporteDetallado(dtpInicio.Value, dtpFin.Value);
                MessageBox.Show("? Reporte Detallado generado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{ruta}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}