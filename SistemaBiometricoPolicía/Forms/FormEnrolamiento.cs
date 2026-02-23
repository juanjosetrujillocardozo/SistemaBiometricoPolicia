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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SistemaBiometricoPolicia.Forms
{
    public partial class FormEnrolamiento : Form, DPFP.Capture.EventHandler
    {
        // ── Campos ──────────────────────────────────────────────────────────────
        private Capture _capturer;
        private List<DPFP.Sample> _muestras = new List<DPFP.Sample>();
        private List<Estudiante> _todosEstudiantes = new List<Estudiante>();
        private Estudiante _estudianteActual;
        private string _rutaFotoCapturada;
        private bool formularioCerrado = false;

        // Win32: placeholder nativo para TextBox
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam,
            [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        // ── Constructor ─────────────────────────────────────────────────────────
        public FormEnrolamiento()
        {
            InitializeComponent();

            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.PerformAutoScale();

            InicializarUIEstado();
            SendMessage(txtFiltro.Handle, EM_SETCUEBANNER, 0, "Buscar por cédula o nombre...");
            CargarEstudiantesSinHuellas();
            InicializarLector();
            InicializarWebcam();
        }

        // ── Inicialización ──────────────────────────────────────────────────────
        private void InicializarUIEstado()
        {
            btnIniciarCaptura.Enabled = false;
            btnCapturarFoto.Enabled = false;
            btnGuardar.Enabled = false;
            btnCancelar.Enabled = false;
            cmbDedo.SelectedIndex = 0;
            lblPasos.Text = "Paso 1: Seleccione un estudiante de la lista.";
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

        // ── Carga y filtrado de estudiantes ─────────────────────────────────────
        private void CargarEstudiantesSinHuellas()
        {
            try
            {
                _todosEstudiantes = DatabaseHelper.ObtenerEstudiantesSinHuellas();
                FiltrarLista(txtFiltro.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estudiantes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FiltrarLista(string busqueda = "")
        {
            busqueda = busqueda?.ToLower().Trim() ?? "";

            var filtrados = string.IsNullOrEmpty(busqueda)
                ? _todosEstudiantes
                : _todosEstudiantes.Where(est =>
                    est.NumeroDocumento.Contains(busqueda) ||
                    est.Apellidos.ToLower().Contains(busqueda) ||
                    est.Nombres.ToLower().Contains(busqueda)).ToList();

            lstEstudiantes.BeginUpdate();
            lstEstudiantes.Items.Clear();
            foreach (var est in filtrados)
            {
                var item = new ListViewItem(est.NumeroDocumento);
                item.SubItems.Add(est.Apellidos);
                item.SubItems.Add(est.Nombres);
                item.SubItems.Add(est.Seccion);
                item.Tag = est;
                lstEstudiantes.Items.Add(item);
            }
            lstEstudiantes.EndUpdate();
            lblTotalPendientes.Text = $"Estudiantes sin huella: {filtrados.Count}";
        }

        private void txtFiltro_TextChanged(object sender, EventArgs e)
        {
            FiltrarLista(txtFiltro.Text);
        }

        // ── Selección de estudiante ─────────────────────────────────────────────
        private void lstEstudiantes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstEstudiantes.SelectedItems.Count == 0) return;

            _estudianteActual = (Estudiante)lstEstudiantes.SelectedItems[0].Tag;
            lblEstudianteSeleccionado.Text =
                $"Estudiante: {_estudianteActual.Apellidos} {_estudianteActual.Nombres} - {_estudianteActual.NumeroDocumento}";
            lblEstudianteSeleccionado.ForeColor = Color.DarkBlue;

            btnIniciarCaptura.Enabled = true;
            btnCapturarFoto.Enabled = true;
            lblPasos.Text = "Paso 2: Capture la foto y luego inicie la captura de huella.";
            ReiniciarCaptura();
        }

        // ── Captura de foto ─────────────────────────────────────────────────────
        private void btnCapturarFoto_Click(object sender, EventArgs e)
        {
            try
            {
                if (_estudianteActual == null)
                {
                    MessageBox.Show("Seleccione un estudiante primero", "Advertencia",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string carpetaFotos = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fotos");
                if (!Directory.Exists(carpetaFotos))
                    Directory.CreateDirectory(carpetaFotos);

                _rutaFotoCapturada = Path.Combine(carpetaFotos,
                    $"{_estudianteActual.NumeroDocumento}.jpg");

                var foto = WebcamHelper.CapturarFoto();
                if (foto != null)
                {
                    foto.Save(_rutaFotoCapturada, System.Drawing.Imaging.ImageFormat.Jpeg);

                    pbFotoCapturada.Image?.Dispose();
                    pbFotoCapturada.Image = foto;

                    SoundHelper.ReproducirExito();
                    lblPasos.Text = "Paso 3: Seleccione el dedo e inicie la captura de huella.";
                    MessageBox.Show("✅ Foto capturada correctamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    SoundHelper.ReproducirError();
                    MessageBox.Show("❌ No se pudo capturar la foto. Verifique la cámara.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                SoundHelper.ReproducirError();
                MessageBox.Show($"Error al capturar foto: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.RegistrarError("Error en captura de foto", ex);
            }
        }

        // ── Captura de huella ───────────────────────────────────────────────────
        private void btnIniciarCaptura_Click(object sender, EventArgs e)
        {
            if (_estudianteActual == null)
            {
                MessageBox.Show("Seleccione un estudiante primero", "Advertencia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbDedo.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione el dedo a registrar", "Advertencia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_capturer == null)
            {
                MessageBox.Show("El lector biométrico no está disponible.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                lblPasos.Text = "Paso 4: Coloque el mismo dedo 4 veces en el lector.";
                StatusHub.PushEvento("🔵 Captura iniciada");
                ActualizarEstadoBiometrico("Esperando huella para enrolamiento...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar captura: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.RegistrarError("Error al iniciar captura", ex);
                ActualizarEstadoBiometrico("ERROR al iniciar captura: " + ex.Message);
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

                btnIniciarCaptura.Enabled = (_estudianteActual != null && _capturer != null);
                btnGuardar.Enabled = false;
                btnCancelar.Enabled = false;
                ActualizarEstadoBiometrico("✓ Lector listo para nueva captura");
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al reiniciar captura", ex);
            }
        }

        // ── Guardar enrolamiento ────────────────────────────────────────────────
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (_estudianteActual == null || _muestras.Count < 4)
                {
                    MessageBox.Show("Complete las 4 muestras de huella antes de guardar", "Advertencia",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var enrollment = new DPFP.Processing.Enrollment();
                foreach (var muestra in _muestras)
                {
                    var features = BiometricService.ExtraerCaracteristicas(
                        muestra, DPFP.Processing.DataPurpose.Enrollment);
                    if (features != null)
                        enrollment.AddFeatures(features);
                }

                if (enrollment.TemplateStatus == DPFP.Processing.Enrollment.Status.Ready)
                {
                    using (var ms = new MemoryStream())
                    {
                        enrollment.Template.Serialize(ms);
                        byte[] templateBytes = ms.ToArray();

                        DatabaseHelper.GuardarHuella(
                            _estudianteActual.Id,
                            cmbDedo.SelectedItem.ToString(),
                            templateBytes);

                        if (!string.IsNullOrEmpty(_rutaFotoCapturada) && File.Exists(_rutaFotoCapturada))
                            DatabaseHelper.ActualizarFotoEstudiante(_estudianteActual.Id, _rutaFotoCapturada);
                    }

                    SoundHelper.ReproducirExito();
                    MessageBox.Show("✅ Huella y foto guardadas correctamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Limpiar y preparar para el siguiente estudiante
                    _estudianteActual = null;
                    _rutaFotoCapturada = null;
                    pbFotoCapturada.Image?.Dispose();
                    pbFotoCapturada.Image = null;

                    CargarEstudiantesSinHuellas();   // Recarga lista (el enrolado desaparece)
                    ReiniciarCaptura();
                    txtFiltro.Clear();
                    txtFiltro.Focus();               // Listo para buscar el siguiente
                    lblPasos.Text = "Paso 1: Seleccione un estudiante de la lista.";
                    ActualizarEstadoBiometrico("✓ Huella guardada correctamente");
                }
                else
                {
                    SoundHelper.ReproducirError();
                    MessageBox.Show("❌ No se pudo crear el template. Intente nuevamente.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ActualizarEstadoBiometrico("⚠ No se pudo crear template de huella");
                }
            }
            catch (Exception ex)
            {
                SoundHelper.ReproducirError();
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.RegistrarError("Error al guardar huella", ex);
                ActualizarEstadoBiometrico("ERROR al guardar: " + ex.Message);
            }
        }

        // ── UI de estado biométrico ─────────────────────────────────────────────
        private void ActualizarEstadoBiometrico(string mensaje)
        {
            lblEstadoBiometrico.Text = mensaje;

            if (mensaje.StartsWith("✓") || mensaje.Contains("listo") ||
                mensaje.Contains("Esperando") || mensaje.Contains("Listo"))
            {
                lblEstadoBiometrico.BackColor = Color.FromArgb(192, 255, 192);
                lblEstadoBiometrico.ForeColor = Color.DarkGreen;
            }
            else if (mensaje.Contains("⚠") || mensaje.Contains("baja") ||
                     mensaje.Contains("no registrada") || mensaje.Contains("Calidad"))
            {
                lblEstadoBiometrico.BackColor = Color.Khaki;
                lblEstadoBiometrico.ForeColor = Color.DarkGoldenrod;
            }
            else if (mensaje.Contains("ERROR") || mensaje.Contains("No se pudo") ||
                     mensaje.Contains("desconectado"))
            {
                lblEstadoBiometrico.BackColor = Color.MistyRose;
                lblEstadoBiometrico.ForeColor = Color.DarkRed;
            }
            else
            {
                lblEstadoBiometrico.BackColor = Color.LightGray;
                lblEstadoBiometrico.ForeColor = Color.Black;
            }
        }

        // ── Callbacks del SDK DPFP ──────────────────────────────────────────────
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
                            pbHuella.Image?.Dispose();
                            pbHuella.Image = bmp;
                        }

                        if (_muestras.Count >= 4)
                        {
                            _capturer.StopCapture();
                            lblEstado.Text = "✅ Captura completa - Presione GUARDAR";
                            lblEstado.BackColor = Color.LightGreen;
                            btnGuardar.Enabled = true;
                            btnIniciarCaptura.Enabled = false;
                            lblPasos.Text = "Paso Final: Presione 'Guardar Registro'.";
                            SoundHelper.ReproducirExito();
                            StatusHub.PushEvento("✅ 4 muestras capturadas correctamente");
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
                    BeginInvoke(new Action(() =>
                    {
                        ActualizarEstadoBiometrico("✓ Lector conectado y listo");
                        // Re-habilitar botón si hay estudiante seleccionado
                        btnIniciarCaptura.Enabled = (_estudianteActual != null);
                    }));
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
                    BeginInvoke(new Action(() =>
                    {
                        ActualizarEstadoBiometrico("⚠ Lector desconectado físicamente");
                        // Resetear estado de captura para evitar estado inconsistente
                        _muestras.Clear();
                        progressBar.Value = 0;
                        lblProgreso.Text = "Muestras capturadas: 0/4";
                        lblEstado.Text = "⚠ Lector desconectado - Reconecte el USB";
                        lblEstado.BackColor = Color.MistyRose;
                        btnIniciarCaptura.Enabled = false;
                        btnGuardar.Enabled = false;
                        btnCancelar.Enabled = false;
                    }));
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

        // ── Cierre del formulario ───────────────────────────────────────────────
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            formularioCerrado = true;

            try
            {
                if (_capturer != null)
                {
                    try { _capturer.StopCapture(); } catch { }
                    _capturer.EventHandler = null;
                    _capturer.Dispose();
                    _capturer = null;
                }

                WebcamHelper.Detener();

                pbWebcam.Image?.Dispose();
                pbWebcam.Image = null;

                pbFotoCapturada.Image?.Dispose();
                pbFotoCapturada.Image = null;

                pbHuella.Image?.Dispose();
                pbHuella.Image = null;

                _muestras.Clear();
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al cerrar FormEnrolamiento", ex);
            }

            base.OnFormClosing(e);
        }
    }
}