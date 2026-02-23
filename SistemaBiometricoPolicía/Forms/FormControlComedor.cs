using SistemaBiometricoPolicia.Biometric;
using SistemaBiometricoPolicia.Data;
using SistemaBiometricoPolicia.Models;
using SistemaBiometricoPolicia.Utils;
using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SistemaBiometricoPolicia.Forms
{
    public partial class FormControlComedor : Form
    {
        private BiometricService biometricService;
        private string servicioActual = "";
        private string carpetaFotosConsumo;
        private bool formularioCerrado = false;

        public FormControlComedor()
        {
            InitializeComponent();
            carpetaFotosConsumo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FotosConsumo");
            if (!Directory.Exists(carpetaFotosConsumo))
                Directory.CreateDirectory(carpetaFotosConsumo);
        }

        private void FormControlComedor_Load(object sender, EventArgs e)
        {
            try
            {
                // 1. Inicializar biométrico
                biometricService = new BiometricService();
                biometricService.EstudianteIdentificado += BiometricService_EstudianteIdentificado;
                biometricService.MensajeEstado += BiometricService_MensajeEstado;
                biometricService.IniciarCaptura();

                // 2. Inicializar webcam
                if (WebcamHelper.HayWebcamDisponible())
                {
                    WebcamHelper.Iniciar(pictureBoxFoto);
                }

                DeterminarServicioAutomatico();
                ActualizarContadorRegistros();
                timerReloj.Start();

                lblEstado.Text = "ESPERANDO HUELLA...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.RegistrarError("FormControlComedor_Load", ex);
            }
        }

        private void DeterminarServicioAutomatico()
        {
            TimeSpan ahora = DateTime.Now.TimeOfDay;

            if (ahora >= new TimeSpan(4, 30, 0) && ahora <= new TimeSpan(9, 30, 0))
                servicioActual = "DESAYUNO";
            else if (ahora >= new TimeSpan(11, 30, 0) && ahora <= new TimeSpan(15, 30, 0))
                servicioActual = "ALMUERZO";
            else if (ahora >= new TimeSpan(17, 30, 0) && ahora <= new TimeSpan(21, 30, 0))
                servicioActual = "CENA";
            else
                servicioActual = "FUERA DE HORARIO";

            if (lblServicio.Text != $"SERVICIO: {servicioActual}")
            {
                lblServicio.Text = $"SERVICIO: {servicioActual}";
                lblServicio.BackColor = servicioActual == "FUERA DE HORARIO" ? Color.DarkRed : Color.DarkGreen;
            }
        }

        private void BiometricService_EstudianteIdentificado(object sender, EstudianteIdentificadoEventArgs e)
        {
            if (formularioCerrado || IsDisposed || !IsHandleCreated) return;

            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => BiometricService_EstudianteIdentificado(sender, e)));
                    return;
                }
                ProcesarIdentificacion(e.EstudianteId);
            }
            catch { }
        }

        private void BiometricService_MensajeEstado(object sender, string mensaje)
        {
            if (formularioCerrado || IsDisposed || !IsHandleCreated) return;

            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => BiometricService_MensajeEstado(sender, mensaje)));
                    return;
                }
                if (!timerLimpiar.Enabled)
                    lblEstado.Text = mensaje.ToUpper();
            }
            catch { }
        }

        private void ProcesarIdentificacion(int estudianteId)
        {
            try
            {
                if (servicioActual == "FUERA DE HORARIO")
                {
                    MostrarMensaje("FUERA DE HORARIO", Color.Red);
                    SoundHelper.PlayError();
                    return;
                }

                if (ValidarSiYaConsumio(estudianteId, servicioActual))
                {
                    CargarDatosEstudiante(estudianteId);
                    MostrarMensaje("YA CONSUMIÓ " + servicioActual, Color.Orange);
                    SoundHelper.PlayAlert();
                }
                else
                {
                    string fotoEvidencia = GuardarFotoEvidencia(estudianteId);
                    RegistrarConsumo(estudianteId, servicioActual, fotoEvidencia);
                    CargarDatosEstudiante(estudianteId);
                    MostrarMensaje("✓ REGISTRADO - ¡SIGA!", Color.Green);
                    SoundHelper.PlaySuccess();
                    ActualizarContadorRegistros();
                }
            }
            catch (SQLiteException ex) when (ex.Message.Contains("database is locked"))
            {
                LogHelper.RegistrarError("DB bloqueada durante identificación", ex);
                MostrarMensaje("ERROR BD - REINTENTE", Color.Red);
                SoundHelper.PlayError();
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error en ProcesarIdentificacion", ex);
                MostrarMensaje("ERROR - LLAME AL TÉCNICO", Color.Red);
                SoundHelper.PlayError();
            }
        }

        private bool ValidarSiYaConsumio(int estudianteId, string servicio)
        {
            using (var conn = DatabaseHelper.ObtenerConexion())
            {
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM RegistrosAlimentacion 
                               WHERE EstudianteId = @id AND TipoServicio = @servicio 
                               AND date(FechaHora) = date('now', 'localtime')";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", estudianteId);
                    cmd.Parameters.AddWithValue("@servicio", servicio);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        private void RegistrarConsumo(int estudianteId, string servicio, string fotoPath)
        {
            using (var conn = DatabaseHelper.ObtenerConexion())
            {
                conn.Open();
                string sql = @"INSERT INTO RegistrosAlimentacion (EstudianteId, TipoServicio, FechaHora, FotoPath, Sincronizado)
                       VALUES (@id, @servicio, datetime('now', 'localtime'), @foto, 0)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", estudianteId);
                    cmd.Parameters.AddWithValue("@servicio", servicio);
                    cmd.Parameters.AddWithValue("@foto", (object)fotoPath ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }

            // ✅ NUEVO: Log de auditoría
            LogHelper.RegistrarEvento(
                $"CONSUMO REGISTRADO - EstudianteId: {estudianteId} | Servicio: {servicio} | Foto: {fotoPath ?? "Sin foto"}",
                "CONSUMO"
            );
        }

        private void CargarDatosEstudiante(int id)
        {
            using (var conn = DatabaseHelper.ObtenerConexion())
            {
                conn.Open();
                string sql = "SELECT * FROM Estudiantes WHERE Id = @id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblNombre.Text = $"{reader["Apellidos"]} {reader["Nombres"]}".ToUpper();
                            lblSeccion.Text = $"SECCIÓN: {reader["Seccion"]}";
                            lblDocumento.Text = $"DOC: {reader["NumeroDocumento"]}";

                            string rutaFoto = reader["RutaFoto"]?.ToString();
                            if (!string.IsNullOrEmpty(rutaFoto) && File.Exists(rutaFoto))
                            {
                                WebcamHelper.Detener();
                                try
                                {
                                    if (File.Exists(rutaFoto))
                                    {
                                        using (var img = Image.FromFile(rutaFoto))
                                        {
                                            pictureBoxFoto.Image = new Bitmap(img);
                                        }
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                // Sin foto registrada: mantener webcam activa mostrando al policía en vivo
                                if (!WebcamHelper.EstaActiva())
                                    WebcamHelper.Iniciar(pictureBoxFoto);
                            }
                        }
                    }
                }
            }
        }

        private string GuardarFotoEvidencia(int estudianteId)
        {
            var foto = WebcamHelper.CapturarFoto();
            if (foto == null) return null;

            try
            {
                string fecha = DateTime.Now.ToString("yyyy-MM-dd");
                string subCarpeta = Path.Combine(carpetaFotosConsumo, fecha);
                if (!Directory.Exists(subCarpeta)) Directory.CreateDirectory(subCarpeta);

                string fileName = Path.Combine(subCarpeta, $"{estudianteId}_{DateTime.Now:HHmmss}.jpg");
                foto.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                foto.Dispose();
                return fileName;
            }
            catch { return null; }
        }

        private void MostrarMensaje(string texto, Color color)
        {
            lblEstado.Text = texto;
            lblEstado.BackColor = color;
            lblEstado.ForeColor = Color.White;
            timerLimpiar.Start();
        }

        private void ActualizarContadorRegistros()
        {
            using (var conn = DatabaseHelper.ObtenerConexion())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM RegistrosAlimentacion WHERE date(FechaHora) = date('now', 'localtime')";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    lblRegistrosHoy.Text = $"REGISTROS HOY: {cmd.ExecuteScalar()}";
                }
            }
        }

        private void timerLimpiar_Tick(object sender, EventArgs e)
        {
            timerLimpiar.Stop();
            lblEstado.Text = "ESPERANDO HUELLA...";
            lblEstado.BackColor = Color.LightGray;
            lblEstado.ForeColor = Color.Black;
            lblNombre.Text = "---";
            lblSeccion.Text = "SECCIÓN: ---";
            lblDocumento.Text = "DOC: ---";

            if (pictureBoxFoto.Image != null)
            {
                pictureBoxFoto.Image.Dispose();
                pictureBoxFoto.Image = null;
            }

            if (WebcamHelper.HayWebcamDisponible())
            {
                WebcamHelper.Iniciar(pictureBoxFoto);
            }
        }

        private void timerReloj_Tick(object sender, EventArgs e)
        {
            lblHora.Text = DateTime.Now.ToString("HH:mm:ss");
            DeterminarServicioAutomatico();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            formularioCerrado = true;

            try
            {
                timerReloj.Stop();
                timerLimpiar.Stop();

                if (biometricService != null)
                {
                    biometricService.EstudianteIdentificado -= BiometricService_EstudianteIdentificado;
                    biometricService.MensajeEstado -= BiometricService_MensajeEstado;
                    biometricService?.Dispose();
                    biometricService = null;
                }

                WebcamHelper.Detener();

                if (pictureBoxFoto.Image != null)
                {
                    pictureBoxFoto.Image.Dispose();
                    pictureBoxFoto.Image = null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al cerrar FormControlComedor", ex);
            }

            base.OnFormClosing(e);
        }
    }
}