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

        // ── Rangos de servicio (hora inicio, hora fin inclusive) ────────────────
        // Cubren las 24 horas sin dejar ningún hueco
        private static readonly (TimeSpan inicio, TimeSpan fin, string nombre)[] RangosServicio =
        {
            (new TimeSpan(0,  0, 0), new TimeSpan(10, 59, 59), "DESAYUNO"), // 00:00 - 10:59
            (new TimeSpan(11, 0, 0), new TimeSpan(16, 59, 59), "ALMUERZO"), // 11:00 - 16:59
            (new TimeSpan(17, 0, 0), new TimeSpan(23, 59, 59), "CENA"),     // 17:00 - 23:59
        };

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
                biometricService = new BiometricService();
                biometricService.EstudianteIdentificado += BiometricService_EstudianteIdentificado;
                biometricService.MensajeEstado += BiometricService_MensajeEstado;
                biometricService.IniciarCaptura();

                if (WebcamHelper.HayWebcamDisponible())
                    WebcamHelper.Iniciar(pictureBoxFoto);

                DeterminarServicioAutomatico();
                ActualizarContadorRegistros();
                timerReloj.Start();

                lblEstado.Text = "ESPERANDO HUELLA...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.RegistrarError("FormControlComedor_Load", ex);
            }
        }

        // ── Determina el servicio según la hora actual (cubre 24h) ──────────────
        private void DeterminarServicioAutomatico()
        {
            TimeSpan ahora = DateTime.Now.TimeOfDay;
            string nuevoServicio = "DESAYUNO"; // fallback (nunca debería usarse)

            foreach (var rango in RangosServicio)
            {
                if (ahora >= rango.inicio && ahora <= rango.fin)
                {
                    nuevoServicio = rango.nombre;
                    break;
                }
            }

            if (servicioActual != nuevoServicio)
            {
                servicioActual = nuevoServicio;
                lblServicio.Text = $"SERVICIO: {servicioActual}";

                // Color por servicio
                switch (servicioActual)
                {
                    case "DESAYUNO":
                        lblServicio.BackColor = Color.DarkOrange;
                        break;
                    case "ALMUERZO":
                        lblServicio.BackColor = Color.DarkGreen;
                        break;
                    case "CENA":
                        lblServicio.BackColor = Color.DarkBlue;
                        break;
                }
                lblServicio.ForeColor = Color.White;

                LogHelper.RegistrarEvento($"Servicio cambiado automáticamente a: {servicioActual}", "SISTEMA");
            }
        }

        // ── Callbacks del BiometricService ──────────────────────────────────────
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

        // ── Lógica principal de identificación ──────────────────────────────────
        private void ProcesarIdentificacion(int estudianteId)
        {
            try
            {
                // Ya NO hay bloqueo por "FUERA DE HORARIO" — opera 24h
                if (ValidarSiYaConsumio(estudianteId, servicioActual))
                {
                    CargarDatosEstudiante(estudianteId);
                    MostrarMensaje($"YA CONSUMIÓ {servicioActual}", Color.OrangeRed);
                    SoundHelper.PlayAlert();
                }
                else
                {
                    string fotoEvidencia = GuardarFotoEvidencia(estudianteId);
                    RegistrarConsumo(estudianteId, servicioActual, fotoEvidencia);
                    CargarDatosEstudiante(estudianteId);
                    MostrarMensaje($"✓ {servicioActual} REGISTRADO - ¡SIGA!", Color.Green);
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

        // ── Validación: ¿ya consumió ESTE servicio hoy? ─────────────────────────
        private bool ValidarSiYaConsumio(int estudianteId, string servicio)
        {
            using (var conn = DatabaseHelper.ObtenerConexion())
            {
                conn.Open();
                // Compara por tipo de servicio Y por fecha local
                const string sql = @"
                    SELECT COUNT(*) FROM RegistrosAlimentacion 
                    WHERE EstudianteId = @id 
                      AND TipoServicio = @servicio 
                      AND date(FechaHora) = date('now', 'localtime')";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", estudianteId);
                    cmd.Parameters.AddWithValue("@servicio", servicio);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        // ── Registro en BD ──────────────────────────────────────────────────────
        private void RegistrarConsumo(int estudianteId, string servicio, string fotoPath)
        {
            using (var conn = DatabaseHelper.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    INSERT INTO RegistrosAlimentacion 
                        (EstudianteId, TipoServicio, FechaHora, FotoPath, Sincronizado)
                    VALUES 
                        (@id, @servicio, datetime('now', 'localtime'), @foto, 0)";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", estudianteId);
                    cmd.Parameters.AddWithValue("@servicio", servicio);
                    cmd.Parameters.AddWithValue("@foto", (object)fotoPath ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }

            LogHelper.RegistrarEvento(
                $"CONSUMO REGISTRADO - EstudianteId: {estudianteId} | Servicio: {servicio} | Foto: {fotoPath ?? "Sin foto"}",
                "CONSUMO");
        }

        // ── Carga datos del estudiante en pantalla ──────────────────────────────
        private void CargarDatosEstudiante(int id)
        {
            using (var conn = DatabaseHelper.ObtenerConexion())
            {
                conn.Open();
                const string sql = "SELECT * FROM Estudiantes WHERE Id = @id";
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
                                    using (var img = Image.FromFile(rutaFoto))
                                        pictureBoxFoto.Image = new Bitmap(img);
                                }
                                catch { }
                            }
                            else
                            {
                                if (!WebcamHelper.EstaActiva())
                                    WebcamHelper.Iniciar(pictureBoxFoto);
                            }
                        }
                    }
                }
            }
        }

        // ── Captura foto de evidencia ───────────────────────────────────────────
        private string GuardarFotoEvidencia(int estudianteId)
        {
            var foto = WebcamHelper.CapturarFoto();
            if (foto == null) return null;

            try
            {
                string fecha = DateTime.Now.ToString("yyyy-MM-dd");
                string subCarpeta = Path.Combine(carpetaFotosConsumo, fecha);
                if (!Directory.Exists(subCarpeta))
                    Directory.CreateDirectory(subCarpeta);

                string fileName = Path.Combine(subCarpeta,
                    $"{estudianteId}_{servicioActual}_{DateTime.Now:HHmmss}.jpg");
                foto.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                foto.Dispose();
                return fileName;
            }
            catch { return null; }
        }

        // ── UI helpers ──────────────────────────────────────────────────────────
        private void MostrarMensaje(string texto, Color color)
        {
            lblEstado.Text = texto;
            lblEstado.BackColor = color;
            lblEstado.ForeColor = Color.White;
            timerLimpiar.Start();
        }

        private void ActualizarContadorRegistros()
        {
            var stats = DatabaseHelper.ObtenerEstadisticasHoy();
            lblRegistrosHoy.Text =
                $"HOY: {stats.total} registros  |  🍳 {stats.desayunos}  🍽 {stats.almuerzos}  🌙 {stats.cenas}";
        }

        // ── Timers ──────────────────────────────────────────────────────────────
        private void timerLimpiar_Tick(object sender, EventArgs e)
        {
            timerLimpiar.Stop();
            lblEstado.Text = "ESPERANDO HUELLA...";
            lblEstado.BackColor = Color.LightGray;
            lblEstado.ForeColor = Color.Black;
            lblNombre.Text = "---";
            lblSeccion.Text = "SECCIÓN: ---";
            lblDocumento.Text = "DOC: ---";

            pictureBoxFoto.Image?.Dispose();
            pictureBoxFoto.Image = null;

            if (WebcamHelper.HayWebcamDisponible())
                WebcamHelper.Iniciar(pictureBoxFoto);
        }

        private void timerReloj_Tick(object sender, EventArgs e)
        {
            lblHora.Text = DateTime.Now.ToString("HH:mm:ss");
            DeterminarServicioAutomatico(); // Cambia servicio automáticamente al cruzar el umbral
        }

        // ── Cierre ──────────────────────────────────────────────────────────────
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
                    biometricService.Dispose();
                    biometricService = null;
                }

                WebcamHelper.Detener();
                pictureBoxFoto.Image?.Dispose();
                pictureBoxFoto.Image = null;
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError("Error al cerrar FormControlComedor", ex);
            }
            base.OnFormClosing(e);
        }
    }
}