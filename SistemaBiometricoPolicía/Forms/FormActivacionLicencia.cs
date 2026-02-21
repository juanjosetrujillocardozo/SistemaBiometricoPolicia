using System;
using System.Data.SQLite;
using System.Windows.Forms;
using SistemaBiometricoPolicia.Data;
using SistemaBiometricoPolicia.Utils;

namespace SistemaBiometricoPolicia.Forms
{
    public partial class FormActivacionLicencia : Form
    {
        private const string CLAVE_MAESTRA = "TRUJO2026"; // Cambia esto por tu clave secreta

        public FormActivacionLicencia()
        {
            InitializeComponent();
        }

        private void btnActivar_Click(object sender, EventArgs e)
        {
            if (txtClaveMaestra.Text != CLAVE_MAESTRA)
            {
                MessageBox.Show("❌ Clave maestra incorrecta", "Acceso Denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string token = txtToken.Text.Trim();
                int meses = (int)numMeses.Value;

                if (string.IsNullOrEmpty(token))
                {
                    MessageBox.Show("Debe ingresar un token de activación", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime fechaExpiracion = DateTime.Now.AddMonths(meses);

                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();

                    // Verificar si ya existe una licencia
                    var cmdCheck = new SQLiteCommand("SELECT COUNT(*) FROM Licencia", conn);
                    int count = Convert.ToInt32(cmdCheck.ExecuteScalar());

                    if (count > 0)
                    {
                        // Actualizar licencia existente
                        var cmdUpdate = new SQLiteCommand(@"
                            UPDATE Licencia SET 
                            TokenActivacion = @token, 
                            FechaExpiracion = @fecha, 
                            Estado = 'ACTIVO',
                            UltimaVerificacionRemota = @ahora", conn);
                        cmdUpdate.Parameters.AddWithValue("@token", token);
                        cmdUpdate.Parameters.AddWithValue("@fecha", fechaExpiracion);
                        cmdUpdate.Parameters.AddWithValue("@ahora", DateTime.Now);
                        cmdUpdate.ExecuteNonQuery();
                    }
                    else
                    {
                        // Insertar nueva licencia
                        var cmdInsert = new SQLiteCommand(@"
                            INSERT INTO Licencia (TokenActivacion, FechaExpiracion, Estado, UltimaVerificacionRemota)
                            VALUES (@token, @fecha, 'ACTIVO', @ahora)", conn);
                        cmdInsert.Parameters.AddWithValue("@token", token);
                        cmdInsert.Parameters.AddWithValue("@fecha", fechaExpiracion);
                        cmdInsert.Parameters.AddWithValue("@ahora", DateTime.Now);
                        cmdInsert.ExecuteNonQuery();
                    }
                }

                LogHelper.RegistrarEvento($"Licencia activada hasta: {fechaExpiracion:dd/MM/yyyy}", "ADMIN");

                MessageBox.Show(
                    $"✅ Licencia activada exitosamente\n\n" +
                    $"Token: {token}\n" +
                    $"Válida hasta: {fechaExpiracion:dd/MM/yyyy HH:mm}\n\n" +
                    $"TRUJO TECHNOLOGIES S.A.S.",
                    "Activación Exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al activar licencia:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.RegistrarError("Error en activación de licencia", ex);
            }
        }

        private void btnBloquear_Click(object sender, EventArgs e)
        {
            if (txtClaveMaestra.Text != CLAVE_MAESTRA)
            {
                MessageBox.Show("❌ Clave maestra incorrecta", "Acceso Denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirmacion = MessageBox.Show(
                "⚠️ ¿Está seguro de BLOQUEAR el sistema?\n\nEsta acción deshabilitará todos los módulos.",
                "Confirmar Bloqueo",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacion == DialogResult.Yes)
            {
                try
                {
                    using (var conn = DatabaseHelper.ObtenerConexion())
                    {
                        conn.Open();
                        var cmd = new SQLiteCommand("UPDATE Licencia SET Estado = 'BLOQUEADO'", conn);
                        cmd.ExecuteNonQuery();
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
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}