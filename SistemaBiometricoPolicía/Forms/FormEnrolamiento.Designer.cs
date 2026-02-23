namespace SistemaBiometricoPolicia.Forms
{
    partial class FormEnrolamiento
    {
#pragma warning disable CS0414
        private System.ComponentModel.IContainer components = null;
#pragma warning restore CS0414

        private void InitializeComponent()
        {
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlIzquierdo = new System.Windows.Forms.Panel();
            this.lstEstudiantes = new System.Windows.Forms.ListView();
            this.colDocumento = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colApellidos = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colNombres = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSeccion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblTotalPendientes = new System.Windows.Forms.Label();
            this.pnlDerecho = new System.Windows.Forms.Panel();
            this.pnlBotones = new System.Windows.Forms.Panel();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.grpHuella = new System.Windows.Forms.GroupBox();
            this.lblEstadoBiometrico = new System.Windows.Forms.Label();
            this.lblEstado = new System.Windows.Forms.Label();
            this.lblProgreso = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnIniciarCaptura = new System.Windows.Forms.Button();
            this.lblDedo = new System.Windows.Forms.Label();
            this.cmbDedo = new System.Windows.Forms.ComboBox();
            this.pbHuella = new System.Windows.Forms.PictureBox();
            this.grpFotos = new System.Windows.Forms.GroupBox();
            this.btnCapturarFoto = new System.Windows.Forms.Button();
            this.lblFotoCapturada = new System.Windows.Forms.Label();
            this.lblWebcam = new System.Windows.Forms.Label();
            this.pbFotoCapturada = new System.Windows.Forms.PictureBox();
            this.pbWebcam = new System.Windows.Forms.PictureBox();
            this.lblEstudianteSeleccionado = new System.Windows.Forms.Label();
            this.tlpMain.SuspendLayout();
            this.pnlIzquierdo.SuspendLayout();
            this.pnlDerecho.SuspendLayout();
            this.pnlBotones.SuspendLayout();
            this.grpHuella.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbHuella)).BeginInit();
            this.grpFotos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFotoCapturada)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbWebcam)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tlpMain.Controls.Add(this.pnlIzquierdo, 0, 0);
            this.tlpMain.Controls.Add(this.pnlDerecho, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(1200, 700);
            this.tlpMain.TabIndex = 0;
            // 
            // pnlIzquierdo
            // 
            this.pnlIzquierdo.Controls.Add(this.lstEstudiantes);
            this.pnlIzquierdo.Controls.Add(this.lblTotalPendientes);
            this.pnlIzquierdo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlIzquierdo.Location = new System.Drawing.Point(3, 3);
            this.pnlIzquierdo.Name = "pnlIzquierdo";
            this.pnlIzquierdo.Padding = new System.Windows.Forms.Padding(10);
            this.pnlIzquierdo.Size = new System.Drawing.Size(414, 694);
            this.pnlIzquierdo.TabIndex = 0;
            // 
            // lstEstudiantes
            // 
            this.lstEstudiantes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDocumento,
            this.colApellidos,
            this.colNombres,
            this.colSeccion});
            this.lstEstudiantes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstEstudiantes.FullRowSelect = true;
            this.lstEstudiantes.HideSelection = false;
            this.lstEstudiantes.Location = new System.Drawing.Point(10, 35);
            this.lstEstudiantes.Name = "lstEstudiantes";
            this.lstEstudiantes.Size = new System.Drawing.Size(394, 649);
            this.lstEstudiantes.TabIndex = 0;
            this.lstEstudiantes.UseCompatibleStateImageBehavior = false;
            this.lstEstudiantes.View = System.Windows.Forms.View.Details;
            this.lstEstudiantes.SelectedIndexChanged += new System.EventHandler(this.lstEstudiantes_SelectedIndexChanged);
            // 
            // colDocumento
            // 
            this.colDocumento.Text = "Documento";
            this.colDocumento.Width = 90;
            // 
            // colApellidos
            // 
            this.colApellidos.Text = "Apellidos";
            this.colApellidos.Width = 120;
            // 
            // colNombres
            // 
            this.colNombres.Text = "Nombres";
            this.colNombres.Width = 120;
            // 
            // colSeccion
            // 
            this.colSeccion.Text = "Sección";
            // 
            // lblTotalPendientes
            // 
            this.lblTotalPendientes.BackColor = System.Drawing.Color.Navy;
            this.lblTotalPendientes.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTotalPendientes.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalPendientes.ForeColor = System.Drawing.Color.White;
            this.lblTotalPendientes.Location = new System.Drawing.Point(10, 10);
            this.lblTotalPendientes.Name = "lblTotalPendientes";
            this.lblTotalPendientes.Size = new System.Drawing.Size(394, 25);
            this.lblTotalPendientes.TabIndex = 1;
            this.lblTotalPendientes.Text = "Estudiantes sin huella: 0";
            this.lblTotalPendientes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlDerecho
            // 
            this.pnlDerecho.Controls.Add(this.pnlBotones);
            this.pnlDerecho.Controls.Add(this.grpHuella);
            this.pnlDerecho.Controls.Add(this.grpFotos);
            this.pnlDerecho.Controls.Add(this.lblEstudianteSeleccionado);
            this.pnlDerecho.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDerecho.Location = new System.Drawing.Point(423, 3);
            this.pnlDerecho.Name = "pnlDerecho";
            this.pnlDerecho.Padding = new System.Windows.Forms.Padding(10);
            this.pnlDerecho.Size = new System.Drawing.Size(774, 694);
            this.pnlDerecho.TabIndex = 1;
            // 
            // pnlBotones
            // 
            this.pnlBotones.Controls.Add(this.btnCancelar);
            this.pnlBotones.Controls.Add(this.btnGuardar);
            this.pnlBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBotones.Location = new System.Drawing.Point(10, 624);
            this.pnlBotones.Name = "pnlBotones";
            this.pnlBotones.Size = new System.Drawing.Size(754, 60);
            this.pnlBotones.TabIndex = 3;
            // 
            // btnCancelar
            // 
            this.btnCancelar.BackColor = System.Drawing.Color.Maroon;
            this.btnCancelar.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(384, 0);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(370, 60);
            this.btnCancelar.TabIndex = 1;
            this.btnCancelar.Text = "🔄 REINICIAR";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // btnGuardar
            // 
            this.btnGuardar.BackColor = System.Drawing.Color.Navy;
            this.btnGuardar.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(0, 0);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(370, 60);
            this.btnGuardar.TabIndex = 0;
            this.btnGuardar.Text = "💾 GUARDAR TODO";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // grpHuella
            // 
            this.grpHuella.Controls.Add(this.lblEstadoBiometrico);
            this.grpHuella.Controls.Add(this.lblEstado);
            this.grpHuella.Controls.Add(this.lblProgreso);
            this.grpHuella.Controls.Add(this.progressBar);
            this.grpHuella.Controls.Add(this.btnIniciarCaptura);
            this.grpHuella.Controls.Add(this.lblDedo);
            this.grpHuella.Controls.Add(this.cmbDedo);
            this.grpHuella.Controls.Add(this.pbHuella);
            this.grpHuella.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpHuella.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpHuella.Location = new System.Drawing.Point(10, 325);
            this.grpHuella.Name = "grpHuella";
            this.grpHuella.Size = new System.Drawing.Size(754, 280);
            this.grpHuella.TabIndex = 2;
            this.grpHuella.TabStop = false;
            this.grpHuella.Text = "👆 CAPTURA DE HUELLA DACTILAR";
            // 
            // lblEstadoBiometrico
            // 
            this.lblEstadoBiometrico.BackColor = System.Drawing.Color.LightGray;
            this.lblEstadoBiometrico.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblEstadoBiometrico.ForeColor = System.Drawing.Color.Black;
            this.lblEstadoBiometrico.Location = new System.Drawing.Point(360, 19);
            this.lblEstadoBiometrico.Name = "lblEstadoBiometrico";
            this.lblEstadoBiometrico.Size = new System.Drawing.Size(360, 25);
            this.lblEstadoBiometrico.TabIndex = 0;
            this.lblEstadoBiometrico.Text = "Lector biométrico: inicializando...";
            this.lblEstadoBiometrico.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblEstado
            // 
            this.lblEstado.BackColor = System.Drawing.Color.LightGray;
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblEstado.Location = new System.Drawing.Point(200, 210);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(520, 50);
            this.lblEstado.TabIndex = 6;
            this.lblEstado.Text = "Presione \'Iniciar Captura\' para comenzar";
            this.lblEstado.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblProgreso
            // 
            this.lblProgreso.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblProgreso.Location = new System.Drawing.Point(200, 185);
            this.lblProgreso.Name = "lblProgreso";
            this.lblProgreso.Size = new System.Drawing.Size(520, 20);
            this.lblProgreso.TabIndex = 5;
            this.lblProgreso.Text = "Muestras capturadas: 0/4";
            this.lblProgreso.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(200, 150);
            this.progressBar.Maximum = 4;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(520, 30);
            this.progressBar.TabIndex = 4;
            // 
            // btnIniciarCaptura
            // 
            this.btnIniciarCaptura.BackColor = System.Drawing.Color.Green;
            this.btnIniciarCaptura.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIniciarCaptura.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnIniciarCaptura.ForeColor = System.Drawing.Color.White;
            this.btnIniciarCaptura.Location = new System.Drawing.Point(200, 90);
            this.btnIniciarCaptura.Name = "btnIniciarCaptura";
            this.btnIniciarCaptura.Size = new System.Drawing.Size(520, 50);
            this.btnIniciarCaptura.TabIndex = 3;
            this.btnIniciarCaptura.Text = "▶ INICIAR CAPTURA DE HUELLA (4 muestras)";
            this.btnIniciarCaptura.UseVisualStyleBackColor = false;
            this.btnIniciarCaptura.Click += new System.EventHandler(this.btnIniciarCaptura_Click);
            // 
            // lblDedo
            // 
            this.lblDedo.AutoSize = true;
            this.lblDedo.Location = new System.Drawing.Point(200, 30);
            this.lblDedo.Name = "lblDedo";
            this.lblDedo.Size = new System.Drawing.Size(114, 15);
            this.lblDedo.TabIndex = 2;
            this.lblDedo.Text = "Seleccione el dedo:";
            // 
            // cmbDedo
            // 
            this.cmbDedo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDedo.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbDedo.FormattingEnabled = true;
            this.cmbDedo.Items.AddRange(new object[] {
            "Índice Derecho",
            "Pulgar Derecho",
            "Medio Derecho",
            "Anular Derecho",
            "Meñique Derecho",
            "Índice Izquierdo",
            "Pulgar Izquierdo",
            "Medio Izquierdo",
            "Anular Izquierdo",
            "Meñique Izquierdo"});
            this.cmbDedo.Location = new System.Drawing.Point(200, 50);
            this.cmbDedo.Name = "cmbDedo";
            this.cmbDedo.Size = new System.Drawing.Size(520, 25);
            this.cmbDedo.TabIndex = 1;
            // 
            // pbHuella
            // 
            this.pbHuella.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbHuella.Location = new System.Drawing.Point(20, 30);
            this.pbHuella.Name = "pbHuella";
            this.pbHuella.Size = new System.Drawing.Size(150, 200);
            this.pbHuella.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbHuella.TabIndex = 0;
            this.pbHuella.TabStop = false;
            // 
            // grpFotos
            // 
            this.grpFotos.Controls.Add(this.btnCapturarFoto);
            this.grpFotos.Controls.Add(this.lblFotoCapturada);
            this.grpFotos.Controls.Add(this.lblWebcam);
            this.grpFotos.Controls.Add(this.pbFotoCapturada);
            this.grpFotos.Controls.Add(this.pbWebcam);
            this.grpFotos.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpFotos.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpFotos.Location = new System.Drawing.Point(10, 45);
            this.grpFotos.Name = "grpFotos";
            this.grpFotos.Size = new System.Drawing.Size(754, 280);
            this.grpFotos.TabIndex = 1;
            this.grpFotos.TabStop = false;
            this.grpFotos.Text = "📸 CAPTURA DE FOTO";
            // 
            // btnCapturarFoto
            // 
            this.btnCapturarFoto.BackColor = System.Drawing.Color.Purple;
            this.btnCapturarFoto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCapturarFoto.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnCapturarFoto.ForeColor = System.Drawing.Color.White;
            this.btnCapturarFoto.Location = new System.Drawing.Point(20, 230);
            this.btnCapturarFoto.Name = "btnCapturarFoto";
            this.btnCapturarFoto.Size = new System.Drawing.Size(700, 40);
            this.btnCapturarFoto.TabIndex = 4;
            this.btnCapturarFoto.Text = "📸 CAPTURAR FOTO DEL ESTUDIANTE";
            this.btnCapturarFoto.UseVisualStyleBackColor = false;
            this.btnCapturarFoto.Click += new System.EventHandler(this.btnCapturarFoto_Click);
            // 
            // lblFotoCapturada
            // 
            this.lblFotoCapturada.Location = new System.Drawing.Point(400, 25);
            this.lblFotoCapturada.Name = "lblFotoCapturada";
            this.lblFotoCapturada.Size = new System.Drawing.Size(320, 20);
            this.lblFotoCapturada.TabIndex = 3;
            this.lblFotoCapturada.Text = "Foto Capturada";
            this.lblFotoCapturada.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWebcam
            // 
            this.lblWebcam.Location = new System.Drawing.Point(20, 25);
            this.lblWebcam.Name = "lblWebcam";
            this.lblWebcam.Size = new System.Drawing.Size(320, 20);
            this.lblWebcam.TabIndex = 2;
            this.lblWebcam.Text = "Vista Previa Cámara";
            this.lblWebcam.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbFotoCapturada
            // 
            this.pbFotoCapturada.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbFotoCapturada.Location = new System.Drawing.Point(400, 45);
            this.pbFotoCapturada.Name = "pbFotoCapturada";
            this.pbFotoCapturada.Size = new System.Drawing.Size(320, 180);
            this.pbFotoCapturada.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbFotoCapturada.TabIndex = 1;
            this.pbFotoCapturada.TabStop = false;
            // 
            // pbWebcam
            // 
            this.pbWebcam.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbWebcam.Location = new System.Drawing.Point(20, 45);
            this.pbWebcam.Name = "pbWebcam";
            this.pbWebcam.Size = new System.Drawing.Size(320, 180);
            this.pbWebcam.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbWebcam.TabIndex = 0;
            this.pbWebcam.TabStop = false;
            // 
            // lblEstudianteSeleccionado
            // 
            this.lblEstudianteSeleccionado.BackColor = System.Drawing.Color.Orange;
            this.lblEstudianteSeleccionado.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblEstudianteSeleccionado.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblEstudianteSeleccionado.ForeColor = System.Drawing.Color.White;
            this.lblEstudianteSeleccionado.Location = new System.Drawing.Point(10, 10);
            this.lblEstudianteSeleccionado.Name = "lblEstudianteSeleccionado";
            this.lblEstudianteSeleccionado.Size = new System.Drawing.Size(754, 35);
            this.lblEstudianteSeleccionado.TabIndex = 0;
            this.lblEstudianteSeleccionado.Text = "Seleccione un estudiante de la lista";
            this.lblEstudianteSeleccionado.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormEnrolamiento
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.tlpMain);
            this.Name = "FormEnrolamiento";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Enrolamiento de Huellas y Foto - TRUJO TECHNOLOGIES";
            this.tlpMain.ResumeLayout(false);
            this.pnlIzquierdo.ResumeLayout(false);
            this.pnlDerecho.ResumeLayout(false);
            this.pnlBotones.ResumeLayout(false);
            this.grpHuella.ResumeLayout(false);
            this.grpHuella.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbHuella)).EndInit();
            this.grpFotos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbFotoCapturada)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbWebcam)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Panel pnlIzquierdo;
        private System.Windows.Forms.ListView lstEstudiantes;
        private System.Windows.Forms.ColumnHeader colDocumento;
        private System.Windows.Forms.ColumnHeader colApellidos;
        private System.Windows.Forms.ColumnHeader colNombres;
        private System.Windows.Forms.ColumnHeader colSeccion;
        private System.Windows.Forms.Label lblTotalPendientes;
        private System.Windows.Forms.Panel pnlDerecho;
        private System.Windows.Forms.Label lblEstudianteSeleccionado;
        private System.Windows.Forms.Label lblEstadoBiometrico;
        private System.Windows.Forms.GroupBox grpFotos;
        private System.Windows.Forms.Button btnCapturarFoto;
        private System.Windows.Forms.Label lblFotoCapturada;
        private System.Windows.Forms.Label lblWebcam;
        private System.Windows.Forms.PictureBox pbFotoCapturada;
        private System.Windows.Forms.PictureBox pbWebcam;
        private System.Windows.Forms.GroupBox grpHuella;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Label lblProgreso;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnIniciarCaptura;
        private System.Windows.Forms.Label lblDedo;
        private System.Windows.Forms.ComboBox cmbDedo;
        private System.Windows.Forms.PictureBox pbHuella;
        private System.Windows.Forms.Panel pnlBotones;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnGuardar;
    }
}