using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SistemaBiometricoPolicia.Utils;

namespace SistemaBiometricoPolicia.Forms
{
    public partial class FormAcercaDe : Form
    {
        public FormAcercaDe()
        {
            InitializeComponent();
            this.Load += FormAcercaDe_Load;
        }

        private async void FormAcercaDe_Load(object sender, EventArgs e)
        {
            var localVersion = UpdateService.GetLocalVersion();
            lblVersionLocal.Text = "Versión instalada: " + localVersion.ToString();

            await CargarVersionRemotaAsync();
            await CargarEstadoLicenciaAsync();
        }

        private async Task CargarVersionRemotaAsync()
        {
            try
            {
                lblVersionRemota.Text = "Consultando servidor...";
                lblVersionRemota.ForeColor = Color.Gray;

                var remoteVersion = await UpdateService.GetRemoteVersionAsync();

                if (remoteVersion == null)
                {
                    lblVersionRemota.Text = "No se pudo consultar el servidor";
                    lblVersionRemota.ForeColor = Color.Red;
                }
                else
                {
                    var localVersion = UpdateService.GetLocalVersion();
                    if (remoteVersion > localVersion)
                    {
                        lblVersionRemota.Text = "Nueva versión disponible: " + remoteVersion.ToString();
                        lblVersionRemota.ForeColor = Color.Green;
                        btnBuscarActualizaciones.Enabled = true;
                    }
                    else
                    {
                        lblVersionRemota.Text = "Última versión: " + remoteVersion.ToString() + " (Actualizado)";
                        lblVersionRemota.ForeColor = Color.Green;
                        btnBuscarActualizaciones.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                lblVersionRemota.Text = "Error al consultar versión remota";
                lblVersionRemota.ForeColor = Color.Red;
                LogHelper.RegistrarError("Error en CargarVersionRemotaAsync", ex);
            }
        }

        private async Task CargarEstadoLicenciaAsync()
        {
            try
            {
                lblEstadoLicencia.Text = "Verificando licencia...";
                lblEstadoLicencia.ForeColor = Color.Gray;

                var esValida = await LicenciaService.ValidarLicenciaAsync();
                var ultimaVerif = LicenciaService.ObtenerUltimaVerificacion();
                var expiracion = LicenciaService.ObtenerFechaExpiracion();

                if (esValida)
                {
                    lblEstadoLicencia.Text =
                        $"Licencia: ACTIVA  |  Vence: {expiracion}\n" +
                        $"Última verificación remota: {ultimaVerif}";
                    lblEstadoLicencia.ForeColor = Color.DarkGreen;
                }
                else
                {
                    lblEstadoLicencia.Text =
                        $"Licencia: BLOQUEADA  |  Vence: {expiracion}\n" +
                        $"Última verificación remota: {ultimaVerif}";
                    lblEstadoLicencia.ForeColor = Color.DarkRed;
                }
            }
            catch (Exception ex)
            {
                lblEstadoLicencia.Text = "Error al verificar licencia";
                lblEstadoLicencia.ForeColor = Color.Red;
                LogHelper.RegistrarError("Error en CargarEstadoLicenciaAsync", ex);
            }
        }

        private async void btnBuscarActualizaciones_Click(object sender, EventArgs e)
        {
            btnBuscarActualizaciones.Enabled = false;
            btnBuscarActualizaciones.Text = "Buscando...";

            await UpdateService.CheckForUpdatesAsync(this);

            btnBuscarActualizaciones.Text = "Buscar actualizaciones";
            await CargarVersionRemotaAsync();
        }

        private async void btnVerificarLicencia_Click(object sender, EventArgs e)
        {
            btnVerificarLicencia.Enabled = false;
            btnVerificarLicencia.Text = "Verificando...";

            await CargarEstadoLicenciaAsync();

            btnVerificarLicencia.Text = "Verificar licencia";
            btnVerificarLicencia.Enabled = true;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}