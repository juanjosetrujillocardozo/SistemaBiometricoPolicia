using System;
using System.Data.SQLite;
using System.Windows.Forms;
using SistemaBiometricoPolicia.Data;
using SistemaBiometricoPolicia.Utils;

namespace SistemaBiometricoPolicia.Forms
{
    public partial class FormActivacionLicencia : Form
    {
        public FormActivacionLicencia()
        {
            InitializeComponent();
            OcultarControlesObsoletos();
        }

        /// <summary>
        /// El campo txtClaveMaestra y su label quedan en el Designer por compatibilidad,
        /// pero ya no se usan: la validación real es responsabilidad del servidor HTTPS.
        /// </summary>
        private void OcultarControlesObsoletos()
        {
            foreach (Control ctrl in Controls)
            {
                if (ctrl.Name == "txtClaveMaestra" || ctrl.Name == "lblClaveMaestra")
                    ctrl.Visible = false;
            }
        }

        private async void btnActivar_Click(object sender, EventArgs e)
        {
            string token = txtToken.Text.Trim();
            if (string.IsNullOrEmpty(token))
            {
                MessageBox.Show("Debe ingresar un token de activación.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int meses = (int)numMeses.Value;
            DateTime fechaExpiracion = DateTime.Now.AddMonths(meses);

            btnActivar.Enabled = false;
            btnActivar.Text    = "Validando...";
            try
            {
                // 1. Persistir token localmente para que ValidarLicenciaAsync() lo lea
                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    int count = Convert.ToInt32(
                        new SQLiteCommand("SELECT COUNT(*) FROM Licencia", conn).ExecuteScalar());

                    if (count > 0)
                    {
                        var cmd = new SQLiteCommand(@"
                            UPDATE Licencia
                            SET TokenActivacion          = @token,
                                FechaExpiracion          = @fecha,
                                Estado                   = 'ACTIVO',
                                UltimaVerificacionRemota = @ahora", conn);
                        cmd.Parameters.AddWithValue("@token", token);
                        cmd.Parameters.AddWithValue("@fecha", fechaExpiracion);
                        cmd.Parameters.AddWithValue("@ahora", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var cmd = new SQLiteCommand(@"
                            INSERT INTO Licencia
                                (TokenActivacion, FechaExpiracion, Estado, UltimaVerificacionRemota)
                            VALUES (@token, @fecha, 'ACTIVO', @ahora)", conn);
                        cmd.Parameters.AddWithValue("@token", token);
                        cmd.Parameters.AddWithValue("@fecha", fechaExpiracion);
                        cmd.Parameters.AddWithValue("@ahora", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                }

                // 2. El servidor HTTPS determina si el token es realmente válido
                bool valida = await LicenciaService.ValidarLicenciaAsync();

                if (valida)
                {
                    LogHelper.RegistrarEvento($"Licencia activada hasta: {fechaExpiracion:dd/MM/yyyy}", "ADMIN");
                    MessageBox.Show(
                        $"✅ Licencia activada exitosamente\n\n"                  +
                        $"Token: {token}\n"                                       +
                        $"Válida hasta: {fechaExpiracion:dd/MM/yyyy HH:mm}\n\n"  +
                        $"TRUJO TECHNOLOGIES S.A.S.",
                        "Activación Exitosa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    LogHelper.RegistrarEvento($"Token rechazado por servidor: {token}", "ADMIN");
                    MessageBox.Show(
                        $"❌ Token inválido o rechazado por el servidor.\n\n"  +
                        $"Verifique el token e intente nuevamente.\n"          +
                        $"Soporte: +57 317 294 6935",
                        "Activación Fallida",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al activar licencia:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.RegistrarError("Error en activación de licencia", ex);
            }
            finally
            {
                btnActivar.Enabled = true;
                btnActivar.Text    = "Activar Licencia";
            }
        }

        private void btnBloquear_Click(object sender, EventArgs e)
        {
            var confirmacion = MessageBox.Show(
                "⚠️ ¿Está seguro de BLOQUEAR el sistema?\n\nEsta acción deshabilitará todos los módulos.",
                "Confirmar Bloqueo",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    new SQLiteCommand("UPDATE Licencia SET Estado = 'BLOQUEADO'", conn)
                        .ExecuteNonQuery();
                }
                LogHelper.RegistrarEvento("Sistema bloqueado manualmente", "ADMIN");
                MessageBox.Show("🔒 Sistema bloqueado exitosamente", "TRUJO TECHNOLOGIES",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al bloquear:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e) => this.Close();
    }
}
