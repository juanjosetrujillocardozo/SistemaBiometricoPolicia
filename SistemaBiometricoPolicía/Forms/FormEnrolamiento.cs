using DPFP;
using DPFP.Capture;
using SistemaBiometricoPolicia.Biometric;
using SistemaBiometricoPolicia.Data;
using SistemaBiometricoPolicia.Models;
using SistemaBiometricoPolicia.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SistemaBiometricoPolicia.Forms
{
    public partial class FormEnrolamiento : Form, DPFP.Capture.EventHandler
    {
        private Capture _capturer;
        private List<DPFP.Sample> _muestras = new List<DPFP.Sample>();
        private Estudiante _estudianteActual;
        private string _rutaFotoCapturada;
        private bool formularioCerrado = false;

        public FormEnrolamiento()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.PerformAutoScale();

            InicializarUIEstado();
            CargarEstudiantesSinHuellas();
            InicializarLector();
            InicializarWebcam();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            formularioCerrado = true;

            try
            {
                if (_capturer != null)
                {
                    try
                    {
                        _capturer.StopCapture();
                    }
                    catch { }

                    _capturer.EventHandler = null;
                    _capturer.Dispose();
                    _capturer = null;
                }

                WebcamHelper.Detener();

                if (pbWebcam.Image != null)
                {
                    pbWebcam.Image.Dispose();
                    pbWebcam.Image = null;
                }

                if (pbFotoCapturada.Image != null)
                {
                    pbFotoCapturada.Image.Dispose();
                    pbFotoCapturada.Image = null;
                }

                if (pbHuella.Image != null)
                {
                    pbHuella.Image.Dispose();
                    pbHuella.Image = null;
                }

                _muestras.Clear();
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al cerrar FormEnrolamiento", ex);
            }

            base.OnFormClosing(e);
        }

        private void InicializarUIEstado()
        {
            btnIniciarCaptura.Enabled = false;
            btnCapturarFoto.Enabled = false;
            btnGuardar.Enabled = false;
            btnCancelar.Enabled = false;
            cmbDedo.SelectedIndex = 0;
        }

        private void CargarEstudiantesSinHuellas()
        {
            try
            {
                var estudiantes = DatabaseHelper.ObtenerEstudiantesSinHuellas();
                lstEstudiantes.Items.Clear();

                foreach (var est in estudiantes)
                {
                    var item = new ListViewItem(est.NumeroDocumento);
                    item.SubItems.Add(est.Apellidos);
                    item.SubItems.Add(est.Nombres);
                    item.SubItems.Add(est.Seccion);
                    item.Tag = est;
                    lstEstudiantes.Items.Add(item);
                }

                lblTotalPendientes.Text = $"Estudiantes sin huella: {estudiantes.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estudiantes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InicializarLector()
        {
            try
            {
                StatusHub.PushEvento("🔧 Inicializando lector biométrico en enrolamiento...");
                _capturer = new Capture();

                if (_capturer != null)
                {
                    _capturer.EventHandler = this;
                    StatusHub.ActualizarEstado("Lector", "✅ Listo");
                    StatusHub.PushEvento("✓ Lector biométrico inicializado correctamente.");
                    ActualizarEstadoBiometrico("✓ Lector biométrico listo para enrolamiento");
                }
                else
                {
                    StatusHub.ActualizarEstado("Lector", "❌ No detectado");
                    StatusHub.PushEvento("✗ ERROR: No se pudo crear objeto Capture.");
                    ActualizarEstadoBiometrico("ERROR: Lector no detectado");
                }
            }
            catch (Exception ex)
            {
                StatusHub.ActualizarEstado("Lector", "❌ Error");
                StatusHub.PushEvento("✗ ERROR al inicializar lector: " + ex.Message);
                LogHelper.RegistrarError("Error al inicializar lector", ex);
                ActualizarEstadoBiometrico("ERROR: " + ex.Message);
            }
        }

        private void ActualizarEstadoBiometrico(string mensaje)
        {
            lblEstadoBiometrico.Text = mensaje;

            // Colores según mensaje
            if (mensaje.StartsWith("✓") || mensaje.Contains("listo") || mensaje.Contains("Esperando") || mensaje.Contains("Listo"))
            {
                lblEstadoBiometrico.BackColor = Color.FromArgb(192, 255, 192);
                lblEstadoBiometrico.ForeColor = Color.DarkGreen;
            }
            else if (mensaje.Contains("⚠") || mensaje.Contains("baja") || mensaje.Contains("no registrada") || mensaje.Contains("Calidad"))
            {
                lblEstadoBiometrico.BackColor = Color.Khaki;
                lblEstadoBiometrico.ForeColor = Color.DarkGoldenrod;
            }
            else if (mensaje.Contains("ERROR") || mensaje.Contains("No se pudo") || mensaje.Contains("desconectado"))
            {
                lblEstadoBiometrico.BackColor = Color.MistyRose;
                lblEstadoBiometrico.ForeColor = Color.DarkRed;
            }
            else
            {
                // neutro
                lblEstadoBiometrico.BackColor = Color.LightGray;
                lblEstadoBiometrico.ForeColor = Color.Black;
            }
        }

        private void InicializarWebcam()
        {
            try
            {
                WebcamHelper.Iniciar(pbWebcam);
                StatusHub.ActualizarEstado("Cámara", "✅ Activa");
            }
            catch (Exception ex)
            {
                StatusHub.ActualizarEstado("Cámara", "❌ No detectada");
                LogHelper.RegistrarError("Error al inicializar webcam", ex);
            }
        }

        private void lstEstudiantes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstEstudiantes.SelectedItems.Count == 0) return;

            _estudianteActual = (Estudiante)lstEstudiantes.SelectedItems[0].Tag;
            lblEstudianteSeleccionado.Text = $"Estudiante: {_estudianteActual.Apellidos} {_estudianteActual.Nombres} - {_estudianteActual.NumeroDocumento}";

            btnIniciarCaptura.Enabled = true;
            btnCapturarFoto.Enabled = true;
            ReiniciarCaptura();
        }

        private void btnCapturarFoto_Click(object sender, EventArgs e)
        {
            try
            {
                if (_estudianteActual == null)
                {
                    MessageBox.Show("Seleccione un estudiante primero", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string carpetaFotos = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fotos");
                if (!Directory.Exists(carpetaFotos))
                    Directory.CreateDirectory(carpetaFotos);

                _rutaFotoCapturada = Path.Combine(carpetaFotos, $"{_estudianteActual.NumeroDocumento}.jpg");

                var foto = WebcamHelper.CapturarFoto();
                if (foto != null)
                {
                    foto.Save(_rutaFotoCapturada, System.Drawing.Imaging.ImageFormat.Jpeg);

                    if (pbFotoCapturada.Image != null)
                        pbFotoCapturada.Image.Dispose();

                    pbFotoCapturada.Image = foto;
                    SoundHelper.ReproducirExito();
                    MessageBox.Show("✅ Foto capturada correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    SoundHelper.ReproducirError();
                    MessageBox.Show("❌ No se pudo capturar la foto", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                SoundHelper.ReproducirError();
                MessageBox.Show($"Error al capturar foto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.RegistrarError("Error en captura de foto", ex);
            }
        }

        private void btnIniciarCaptura_Click(object sender, EventArgs e)
        {
            if (_estudianteActual == null)
            {
                MessageBox.Show("Seleccione un estudiante primero", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbDedo.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione el dedo a registrar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _muestras.Clear();
                progressBar.Value = 0;
                lblProgreso.Text = "Muestras capturadas: 0/4";
                lblEstado.Text = "Coloque el dedo en el lector...";
                lblEstado.BackColor = Color.LightBlue;

                _capturer.StartCapture();
                btnIniciarCaptura.Enabled = false;
                btnCancelar.Enabled = true;
                StatusHub.RegistrarEvento("🔵 Captura iniciada");
                ActualizarEstadoBiometrico("Esperando huella para enrolamiento...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar captura: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.RegistrarError("Error al iniciar captura", ex);
                ActualizarEstadoBiometrico("ERROR al iniciar captura: " + ex.Message);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (_estudianteActual == null || _muestras.Count < 4)
                {
                    MessageBox.Show("Complete las 4 muestras de huella antes de guardar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var enrollment = new DPFP.Processing.Enrollment();
                foreach (var muestra in _muestras)
                {
                    var features = BiometricService.ExtraerCaracteristicas(muestra, DPFP.Processing.DataPurpose.Enrollment);
                    if (features != null)
                        enrollment.AddFeatures(features);
                }

                if (enrollment.TemplateStatus == DPFP.Processing.Enrollment.Status.Ready)
                {
                    var template = enrollment.Template;
                    using (var ms = new MemoryStream())
                    {
                        template.Serialize(ms);
                        byte[] templateBytes = ms.ToArray();

                        DatabaseHelper.GuardarHuella(_estudianteActual.Id, cmbDedo.SelectedItem.ToString(), templateBytes);

                        if (!string.IsNullOrEmpty(_rutaFotoCapturada) && File.Exists(_rutaFotoCapturada))
                        {
                            DatabaseHelper.ActualizarFotoEstudiante(_estudianteActual.Id, _rutaFotoCapturada);
                        }

                        SoundHelper.ReproducirExito();
                        MessageBox.Show("✅ Huella y foto guardadas correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        CargarEstudiantesSinHuellas();
                        ReiniciarCaptura();
                        ActualizarEstadoBiometrico("✓ Huella guardada correctamente");
                    }
                }
                else
                {
                    SoundHelper.ReproducirError();
                    MessageBox.Show("❌ No se pudo crear el template de huella. Intente nuevamente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ActualizarEstadoBiometrico("⚠ No se pudo crear template de huella");
                }
            }
            catch (Exception ex)
            {
                SoundHelper.ReproducirError();
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.RegistrarError("Error al guardar huella", ex);
                ActualizarEstadoBiometrico("ERROR al guardar: " + ex.Message);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            ReiniciarCaptura();
        }

        private void ReiniciarCaptura()
        {
            try
            {
                if (_capturer != null)
                    _capturer.StopCapture();

                _muestras.Clear();
                progressBar.Value = 0;
                lblProgreso.Text = "Muestras capturadas: 0/4";
                lblEstado.Text = "Presione 'Iniciar Captura' para comenzar";
                lblEstado.BackColor = Color.LightGray;

                if (pbHuella.Image != null)
                {
                    pbHuella.Image.Dispose();
                    pbHuella.Image = null;
                }

                btnIniciarCaptura.Enabled = _estudianteActual != null;
                btnGuardar.Enabled = false;
                btnCancelar.Enabled = false;
                ActualizarEstadoBiometrico("✓ Lector listo para nueva captura");
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al reiniciar captura", ex);
            }
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, Sample Sample)
        {
            if (formularioCerrado || IsDisposed || !IsHandleCreated) return;

            try
            {
                BeginInvoke(new Action(() =>
                {
                    try
                    {
                        _muestras.Add(Sample);
                        progressBar.Value = _muestras.Count;
                        lblProgreso.Text = $"Muestras capturadas: {_muestras.Count}/4";

                        var bmp = BiometricService.ConvertirSampleABitmap(Sample);
                        if (bmp != null)
                        {
                            if (pbHuella.Image != null)
                                pbHuella.Image.Dispose();

                            pbHuella.Image = bmp;
                        }

                        if (_muestras.Count >= 4)
                        {
                            _capturer.StopCapture();
                            lblEstado.Text = "✅ Captura completa - Presione GUARDAR";
                            lblEstado.BackColor = Color.LightGreen;
                            btnGuardar.Enabled = true;
                            btnIniciarCaptura.Enabled = false;
                            SoundHelper.ReproducirExito();
                            StatusHub.RegistrarEvento("✅ 4 muestras capturadas correctamente");
                            ActualizarEstadoBiometrico("✓ Captura completa - 4 muestras listas");
                        }
                        else
                        {
                            lblEstado.Text = $"Levante y vuelva a colocar el dedo ({_muestras.Count}/4)";
                            SoundHelper.ReproducirAlerta();
                            ActualizarEstadoBiometrico($"Capturando... {_muestras.Count}/4 muestras");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.RegistrarError("Error en OnComplete (UI)", ex);
                    }
                }));
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error en OnComplete (Invoke)", ex);
            }
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {
            if (formularioCerrado || IsDisposed || !IsHandleCreated) return;

            try
            {
                BeginInvoke(new Action(() =>
                {
                    lblEstado.Text = "Dedo retirado - Coloque nuevamente";
                    lblEstado.BackColor = Color.LightYellow;
                    ActualizarEstadoBiometrico("Dedo retirado - Esperando...");
                }));
            }
            catch { }
        }

        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
        {
            if (formularioCerrado || IsDisposed || !IsHandleCreated) return;

            try
            {
                BeginInvoke(new Action(() =>
                {
                    lblEstado.Text = "Dedo detectado - Procesando...";
                    lblEstado.BackColor = Color.LightBlue;
                    ActualizarEstadoBiometrico("☝ Dedo detectado - Capturando...");
                }));
            }
            catch { }
        }

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            StatusHub.ActualizarEstado("Lector", "✅ Conectado");
            StatusHub.PushEvento("✓ Lector biométrico CONECTADO en enrolamiento.");

            if (!formularioCerrado && !IsDisposed && IsHandleCreated)
            {
                try
                {
                    BeginInvoke(new Action(() => ActualizarEstadoBiometrico("✓ Lector conectado y listo")));
                }
                catch { }
            }
        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            StatusHub.ActualizarEstado("Lector", "❌ Desconectado");
            StatusHub.PushEvento("⚠ Lector biométrico DESCONECTADO en enrolamiento.");

            if (!formularioCerrado && !IsDisposed && IsHandleCreated)
            {
                try
                {
                    BeginInvoke(new Action(() => ActualizarEstadoBiometrico("⚠ Lector desconectado físicamente")));
                }
                catch { }
            }
        }

        public void OnSampleQuality(object Capture, string ReaderSerialNumber, CaptureFeedback CaptureFeedback)
        {
            if (formularioCerrado || IsDisposed || !IsHandleCreated) return;

            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (CaptureFeedback == CaptureFeedback.Good)
                    {
                        lblEstado.BackColor = Color.LightGreen;
                        ActualizarEstadoBiometrico("✓ Calidad de huella: BUENA");
                    }
                    else
                    {
                        lblEstado.Text = "⚠ Calidad baja - Intente nuevamente";
                        lblEstado.BackColor = Color.Orange;
                        ActualizarEstadoBiometrico("⚠ Calidad de huella baja - Reintente");
                    }
                }));
            }
            catch { }
        }
    }
}