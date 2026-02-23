using SistemaBiometricoPolicia.Data;
using SistemaBiometricoPolicia.Utils;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SistemaBiometricoPolicia.Forms
{
    public partial class FormPrincipal : Form
    {
        private StatusStrip _status;
        private ToolStripStatusLabel _lblBD, _lblRed, _lblUltimo;
        private TextBox _txtEventos;
        private Panel _panelBanner;
        private Label _lblBanner;
        private bool estaAbierto = false;

        public FormPrincipal()
        {
            InitializeComponent();

            this.Controls.SetChildIndex(panelHeader, 0);
            this.Controls.SetChildIndex(btnComedor, 1);
            this.Controls.SetChildIndex(btnEnrolamiento, 2);
            this.Controls.SetChildIndex(btnReportes, 3);
            this.Controls.SetChildIndex(btnSincronizar, 4);
            this.Controls.SetChildIndex(btnImportar, 5);
            this.Controls.SetChildIndex(btnAcercaDe, 6);
            this.Controls.SetChildIndex(btnSalir, 7);
            this.Controls.SetChildIndex(lblEstadisticas, 8);

            InicializarUIEstado();

            StatusHub.EstadoActualizado += (item) =>
            {
                if (IsDisposed || !IsHandleCreated) return;

                try
                {
                    if (InvokeRequired)
                        BeginInvoke(new Action(() => RefrescarEstado(item.Nombre)));
                    else
                        RefrescarEstado(item.Nombre);
                }
                catch { }
            };

            StatusHub.Evento += (msg) =>
            {
                if (IsDisposed) return;
                BeginInvoke(new Action(() =>
                {
                    if (_txtEventos == null) return;
                    _txtEventos.AppendText(msg + Environment.NewLine);

                    var lines = _txtEventos.Lines;
                    if (lines.Length > 200)
                    {
                        _txtEventos.Lines = lines.Skip(lines.Length - 200).ToArray();
                        _txtEventos.SelectionStart = _txtEventos.TextLength;
                        _txtEventos.ScrollToCaret();
                    }
                }));
            };
        }

        private async void FormPrincipal_Load(object sender, EventArgs e)
        {
            string errores;
            if (!StartupValidator.ValidarTodo(out errores))
            {
                MessageBox.Show("⚠️ EL SISTEMA NO PUEDE INICIAR:\n\n" + errores,
                                "Error de Requisitos - TRUJO TECHNOLOGIES",
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
                return;
            }

            SystemHealthMonitor.Start();
            StatusHub.PushEvento("=== SISTEMA INICIADO ===");

            try
            {
                StatusHub.PushEvento("Verificando base de datos...");
                DatabaseHelper.InicializarBaseDeDatos();
                StatusHub.Set("BD", EstadoComponente.Ok, "Base de datos inicializada.");
                StatusHub.PushEvento("✓ Base de datos lista.");
                LogHelper.RegistrarEvento("Sistema iniciado correctamente", "INFO");

                StatusHub.PushEvento("Validando licencia...");
                bool licenciaValida = await LicenciaService.ValidarLicenciaAsync();

                if (!licenciaValida)
                {
                    BloquearSistema();
                    return; // ← Salimos aquí, no hay nada más que hacer
                }

                StatusHub.PushEvento("✓ Licencia válida.");
                DesbloquearSistema();
                ActualizarEstadisticas();
                OldVersionsCleaner.DesinstalarVersionesAntiguas(this);
                LogHelper.RegistrarEvento("Licencia validada correctamente", "INFO");

                // ✅ Esperamos 500ms para que SQLite libere todas las conexiones
                // antes de que UpdateService intente leer TokenActual
                await Task.Delay(500);

                await UpdateService.CheckForUpdatesAsync(this);
            }
            catch (Exception ex)
            {
                StatusHub.Set("BD", EstadoComponente.Error, $"Error: {ex.Message}");
                StatusHub.PushEvento($"✗ Error BD: {ex.Message}");
                LogHelper.RegistrarError("FormPrincipal_Load", ex);
                MessageBox.Show($"Error al iniciar el sistema:\n{ex.Message}", "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BloquearSistema()
        {
            btnComedor.Enabled = false;
            btnEnrolamiento.Enabled = false;
            btnSincronizar.Enabled = false;
            btnReportes.Enabled = false;

            lblEstadisticas.Text = "⛔ SISTEMA BLOQUEADO - CONTACTE A TRUJO TECHNOLOGIES";
            lblEstadisticas.ForeColor = Color.Red;
            lblEstadisticas.Font = new Font(lblEstadisticas.Font, FontStyle.Bold);

            _lblBanner.Text = "⛔ SISTEMA BLOQUEADO\nCONTACTE A TRUJO";
            _lblBanner.ForeColor = Color.Red;
            _panelBanner.BackColor = Color.DarkRed;

            StatusHub.PushEvento("⛔ SISTEMA BLOQUEADO POR LICENCIA INVÁLIDA");
            MessageBox.Show(LicenciaService.MensajeEstado + "\n\nSoporte: +57 317 294 6935 | +57 312 590 5106",
                            "⚠ SISTEMA BLOQUEADO", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }

        private void DesbloquearSistema()
        {
            btnComedor.Enabled = true;
            btnEnrolamiento.Enabled = true;
            btnSincronizar.Enabled = true;
            btnReportes.Enabled = true;

            lblEstadisticas.ForeColor = Color.Black;
            lblEstadisticas.Font = new Font(lblEstadisticas.Font, FontStyle.Regular);

            _lblBanner.Text = "SISTEMA LISTO\nPARA OPERACIÓN";
            _lblBanner.ForeColor = Color.White;
            _panelBanner.BackColor = Color.FromArgb(15, 30, 60);

            StatusHub.PushEvento("✅ SISTEMA DESBLOQUEADO - LICENCIA ACTIVA");
        }

        private void ActualizarEstadisticas()
        {
            try
            {
                int totalEstudiantes;
                using (var conn = DatabaseHelper.ObtenerConexion())
                {
                    conn.Open();
                    using (var cmd = new System.Data.SQLite.SQLiteCommand("SELECT COUNT(*) FROM Estudiantes", conn))
                        totalEstudiantes = Convert.ToInt32(cmd.ExecuteScalar());
                }

                var stats = DatabaseHelper.ObtenerEstadisticasHoy();

                lblEstadisticas.Text =
                    $"📊 {totalEstudiantes} estudiantes  |  " +
                    $"Hoy: {stats.total} registros  " +
                    $"(🍳 {stats.desayunos}  🍽 {stats.almuerzos}  🌙 {stats.cenas})";

                StatusHub.PushEvento(
                    $"📊 {totalEstudiantes} estudiantes | " +
                    $"Hoy: D={stats.desayunos} A={stats.almuerzos} C={stats.cenas} Total={stats.total}");
            }
            catch (Exception ex)
            {
                lblEstadisticas.Text = "⚠ Error en estadísticas";
                StatusHub.PushEvento($"✗ Error al cargar estadísticas: {ex.Message}");
            }
        }

        private void btnComedor_Click(object sender, EventArgs e)
        {
            StatusHub.PushEvento("Abriendo control de comedor...");
            using (var formComedor = new FormControlComedor())
            {
                formComedor.ShowDialog();
            }
            ActualizarEstadisticas();
            StatusHub.PushEvento("Control de comedor cerrado.");
        }

        private void btnEnrolamiento_Click(object sender, EventArgs e)
        {
            try
            {
                StatusHub.PushEvento("Abriendo módulo de enrolamiento...");
                using (var formEnrolar = new FormEnrolamiento())
                {
                    LogHelper.RegistrarEvento("Módulo Enrolamiento Biométrico abierto", "INFO");
                    formEnrolar.ShowDialog();
                }
                ActualizarEstadisticas();
                StatusHub.PushEvento("Módulo de enrolamiento cerrado.");
            }
            catch (Exception ex)
            {
                StatusHub.PushEvento($"✗ Error al abrir enrolamiento: {ex.Message}");
                MessageBox.Show($"Error al abrir Enrolamiento:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            StatusHub.PushEvento("Abriendo módulo de reportes...");
            using (var formReportes = new FormReportes())
            {
                formReportes.ShowDialog();
            }
            StatusHub.PushEvento("Módulo de reportes cerrado.");
        }

        private async void btnSincronizar_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Seleccione la carpeta de destino para sincronizar (ej: OneDrive/Google Drive/Dropbox)";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    btnSincronizar.Enabled = false;
                    btnSincronizar.Text = "⏳ SINCRONIZANDO...";

                    StatusHub.PushEvento("Iniciando sincronización...");

                    try
                    {
                        var syncService = new SyncService();

                        syncService.OnProgress += (mensaje) =>
                        {
                            if (InvokeRequired)
                                BeginInvoke(new Action(() => StatusHub.PushEvento(mensaje)));
                            else
                                StatusHub.PushEvento(mensaje);
                        };

                        bool exito = await syncService.SincronizarAsync(folderDialog.SelectedPath);

                        if (exito)
                        {
                            MessageBox.Show(
                                $"✅ Sincronización exitosa\n\nDestino: {folderDialog.SelectedPath}\n\n" +
                                "Los datos han sido respaldados correctamente.",
                                "Sincronización Completa",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                "⚠️ La sincronización no se completó correctamente.\n\n" +
                                "Revise el log de eventos para más detalles.",
                                "Advertencia",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        StatusHub.PushEvento($"✗ Error crítico en sincronización: {ex.Message}");
                        MessageBox.Show(
                            $"❌ Error durante la sincronización:\n\n{ex.Message}",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        LogHelper.RegistrarError("btnSincronizar_Click", ex);
                    }
                    finally
                    {
                        btnSincronizar.Enabled = true;
                        btnSincronizar.Text = "☁ SINCRONIZAR DATOS";
                    }
                }
                else
                {
                    StatusHub.PushEvento("Sincronización cancelada por el usuario.");
                }
            }
        }

        private void btnAcercaDe_Click(object sender, EventArgs e)
        {
            StatusHub.PushEvento("Abriendo ventana 'Acerca De'...");
            using (var formAcercaDe = new FormAcercaDe())
            {
                formAcercaDe.ShowDialog(this);
            }
            StatusHub.PushEvento("Ventana 'Acerca De' cerrada.");
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea salir del sistema?", "Confirmar Salida", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                StatusHub.PushEvento("=== SISTEMA CERRADO POR EL USUARIO ===");
                Application.Exit();
            }
        }

        private void InicializarUIEstado()
        {
            // 1. Panel Banner (izquierda-abajo)
            _panelBanner = new Panel();
            _panelBanner.Location = new Point(20, 560); // ✅ Bajado de 540 a 560
            _panelBanner.Size = new Size(300, 140); // ✅ Reducido de 160 a 140
            _panelBanner.BackColor = Color.FromArgb(15, 30, 60);

            _panelBanner.Paint += (s, e) =>
            {
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    _panelBanner.ClientRectangle,
                    Color.FromArgb(15, 30, 60),
                    Color.FromArgb(0, 51, 102),
                    90f))
                {
                    e.Graphics.FillRectangle(brush, _panelBanner.ClientRectangle);
                }
            };

            _lblBanner = new Label();
            _lblBanner.Dock = DockStyle.Fill;
            _lblBanner.Text = "SISTEMA LISTO\nPARA OPERACIÓN";
            _lblBanner.TextAlign = ContentAlignment.MiddleCenter;
            _lblBanner.ForeColor = Color.White;
            _lblBanner.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            _lblBanner.BackColor = Color.Transparent;

            _panelBanner.Controls.Add(_lblBanner);
            this.Controls.Add(_panelBanner);

            // 2. Log de eventos (panel negro a la derecha)
            _txtEventos = new TextBox();
            _txtEventos.Multiline = true;
            _txtEventos.ReadOnly = true;
            _txtEventos.ScrollBars = ScrollBars.Vertical;
            _txtEventos.BackColor = Color.Black;
            _txtEventos.ForeColor = Color.Lime;
            _txtEventos.Font = new Font("Consolas", 8);

            // Tamaño y posición a la derecha
            int margen = 10;
            int anchoLog = 260;
            int altoLog = 570; // ✅ Aumentado de 520 a 570

            _txtEventos.Width = anchoLog;
            _txtEventos.Height = altoLog;
            _txtEventos.Left = this.ClientSize.Width - anchoLog - margen;
            _txtEventos.Top = 130;

            // Redimensionar al cambiar tamaño (por si acaso)
            this.Resize += (s, e) =>
            {
                _txtEventos.Left = this.ClientSize.Width - anchoLog - margen;
            };

            // 3. Barra de estado (BD, RED, ULTIMO) abajo
            _status = new StatusStrip();
            _status.Dock = DockStyle.Bottom;
            _lblBD = new ToolStripStatusLabel("BD: ...");
            _lblRed = new ToolStripStatusLabel("RED: ...");
            _lblUltimo = new ToolStripStatusLabel("ULT: ...") { Spring = true };
            _status.Items.AddRange(new ToolStripItem[] { _lblBD, _lblRed, _lblUltimo });

            this.Controls.Add(_txtEventos);
            this.Controls.Add(_status);

            // Asegurar orden visual
            _panelBanner.BringToFront();
            lblEstadisticas.BringToFront();
            _txtEventos.BringToFront();
        }

        private void btnImportar_Click(object sender, EventArgs e)
        {
            StatusHub.PushEvento("Abriendo módulo de importación...");
            using (var formImportar = new FormRegistroEstudiantes())
            {
                formImportar.ShowDialog();
            }
            ActualizarEstadisticas();
        }

        private void RefrescarEstado(string nombre)
        {
            var item = StatusHub.Get(nombre);
            var txt = $"{nombre}: {item.Detalle}";
            var color = item.Estado;

            ToolStripStatusLabel target = null;

            if (nombre.Equals("BD", StringComparison.OrdinalIgnoreCase)) target = _lblBD;
            else if (nombre.Equals("RED", StringComparison.OrdinalIgnoreCase)) target = _lblRed;

            if (target != null)
            {
                target.Text = txt;
                target.ForeColor = color == EstadoComponente.Ok
                    ? Color.DarkGreen
                    : (color == EstadoComponente.Advertencia ? Color.DarkOrange : Color.DarkRed);
            }

            _lblUltimo.Text = $"ULT: {item.Fecha:HH:mm:ss} | {item.Nombre}";
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private void FormPrincipal_KeyDown(object sender, KeyEventArgs e)
        {
            if (estaAbierto) return;

            if (e.Control && e.Alt && e.KeyCode == Keys.L)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                StatusHub.PushEvento("🔑 Atajo secreto: Activación de licencia (Ctrl+Alt+L)");
                AbrirVentanaSecreta(new FormActivacionLicencia());

                Task.Run(async () =>
                {
                    bool licenciaValida = await LicenciaService.ValidarLicenciaAsync();
                    BeginInvoke(new Action(() =>
                    {
                        if (licenciaValida)
                        {
                            DesbloquearSistema();
                            ActualizarEstadisticas();
                        }
                        else
                        {
                            BloquearSistema();
                        }
                    }));
                });
            }

            if (e.Control && e.Alt && e.KeyCode == Keys.I)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                StatusHub.PushEvento("📥 Atajo secreto: Importar estudiantes (Ctrl+Alt+I)");
                AbrirVentanaSecreta(new FormRegistroEstudiantes());
                ActualizarEstadisticas();
            }
        }

        private void AbrirVentanaSecreta(Form formulario)
        {
            if (estaAbierto) return;

            try
            {
                estaAbierto = true;
                SendKeys.SendWait("{ESC}");

                using (formulario)
                {
                    formulario.TopMost = true;
                    formulario.StartPosition = FormStartPosition.CenterParent;
                    SetForegroundWindow(this.Handle);
                    this.Activate();
                    formulario.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                StatusHub.PushEvento($"✗ Error al abrir ventana secreta: {ex.Message}");
                LogHelper.RegistrarError("Error al abrir ventana secreta", ex);
            }
            finally
            {
                this.TopMost = true;
                this.TopMost = false;
                this.Activate();

                Timer timerSeguridad = new Timer { Interval = 1000 };
                timerSeguridad.Tick += (s, ev) =>
                {
                    estaAbierto = false;
                    timerSeguridad.Stop();
                    timerSeguridad.Dispose();
                };
                timerSeguridad.Start();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SystemHealthMonitor.Stop();
            StatusHub.PushEvento("=== SISTEMA CERRADO ===");
            base.OnFormClosing(e);
        }
    }
}