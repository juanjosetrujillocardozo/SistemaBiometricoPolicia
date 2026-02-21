namespace SistemaBiometricoPolicia.Forms
{
    partial class FormControlComedor
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBoxFoto = new System.Windows.Forms.PictureBox();
            this.lblServicio = new System.Windows.Forms.Label();
            this.lblNombre = new System.Windows.Forms.Label();
            this.lblSeccion = new System.Windows.Forms.Label();
            this.lblDocumento = new System.Windows.Forms.Label();
            this.lblEstado = new System.Windows.Forms.Label();
            this.lblRegistrosHoy = new System.Windows.Forms.Label();
            this.lblHora = new System.Windows.Forms.Label();
            this.timerLimpiar = new System.Windows.Forms.Timer(this.components);
            this.timerReloj = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFoto)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxFoto
            // 
            this.pictureBoxFoto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxFoto.Location = new System.Drawing.Point(30, 120);
            this.pictureBoxFoto.Name = "pictureBoxFoto";
            this.pictureBoxFoto.Size = new System.Drawing.Size(320, 320);
            this.pictureBoxFoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxFoto.TabIndex = 0;
            this.pictureBoxFoto.TabStop = false;
            // 
            // lblServicio
            // 
            this.lblServicio.BackColor = System.Drawing.Color.DarkBlue;
            this.lblServicio.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblServicio.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            this.lblServicio.ForeColor = System.Drawing.Color.White;
            this.lblServicio.Location = new System.Drawing.Point(0, 0);
            this.lblServicio.Name = "lblServicio";
            this.lblServicio.Size = new System.Drawing.Size(1024, 100);
            this.lblServicio.TabIndex = 1;
            this.lblServicio.Text = "SERVICIO: DESAYUNO";
            this.lblServicio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblNombre
            // 
            this.lblNombre.AutoSize = true;
            this.lblNombre.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblNombre.Location = new System.Drawing.Point(380, 140);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(150, 32);
            this.lblNombre.TabIndex = 2;
            this.lblNombre.Text = "NOMBRE: ";
            // 
            // lblSeccion
            // 
            this.lblSeccion.AutoSize = true;
            this.lblSeccion.Font = new System.Drawing.Font("Segoe UI", 18F);
            this.lblSeccion.Location = new System.Drawing.Point(380, 190);
            this.lblSeccion.Name = "lblSeccion";
            this.lblSeccion.Size = new System.Drawing.Size(140, 32);
            this.lblSeccion.TabIndex = 3;
            this.lblSeccion.Text = "SECCIÓN: ";
            // 
            // lblDocumento
            // 
            this.lblDocumento.AutoSize = true;
            this.lblDocumento.Font = new System.Drawing.Font("Segoe UI", 18F);
            this.lblDocumento.Location = new System.Drawing.Point(380, 240);
            this.lblDocumento.Name = "lblDocumento";
            this.lblDocumento.Size = new System.Drawing.Size(190, 32);
            this.lblDocumento.TabIndex = 4;
            this.lblDocumento.Text = "DOCUMENTO: ";
            // 
            // lblEstado
            // 
            this.lblEstado.BackColor = System.Drawing.Color.LightGray;
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblEstado.ForeColor = System.Drawing.Color.Black;
            this.lblEstado.Location = new System.Drawing.Point(380, 310);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(600, 80);
            this.lblEstado.TabIndex = 5;
            this.lblEstado.Text = "Coloque su huella...";
            this.lblEstado.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRegistrosHoy
            // 
            this.lblRegistrosHoy.AutoSize = true;
            this.lblRegistrosHoy.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblRegistrosHoy.Location = new System.Drawing.Point(30, 460);
            this.lblRegistrosHoy.Name = "lblRegistrosHoy";
            this.lblRegistrosHoy.Size = new System.Drawing.Size(160, 25);
            this.lblRegistrosHoy.TabIndex = 6;
            this.lblRegistrosHoy.Text = "Registros Hoy: 0";
            // 
            // lblHora
            //
            this.lblHora.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHora.BackColor = System.Drawing.Color.DimGray;
            this.lblHora.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblHora.ForeColor = System.Drawing.Color.White;
            this.lblHora.Location = new System.Drawing.Point(820, 30);
            this.lblHora.Name = "lblHora";
            this.lblHora.Size = new System.Drawing.Size(180, 40);
            this.lblHora.TabIndex = 7;
            this.lblHora.Text = "00:00:00";
            this.lblHora.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // timerLimpiar
            // 
            this.timerLimpiar.Interval = 3000;
            this.timerLimpiar.Tick += new System.EventHandler(this.timerLimpiar_Tick);
            // 
            // timerReloj
            // 
            this.timerReloj.Enabled = true;
            this.timerReloj.Interval = 1000;
            this.timerReloj.Tick += new System.EventHandler(this.timerReloj_Tick);
            // 
            // FormControlComedor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1024, 520);
            this.Controls.Add(this.lblHora);
            this.Controls.Add(this.lblRegistrosHoy);
            this.Controls.Add(this.lblEstado);
            this.Controls.Add(this.lblDocumento);
            this.Controls.Add(this.lblSeccion);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.pictureBoxFoto);
            this.Controls.Add(this.lblServicio);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormControlComedor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Control de Comedor - Policía Nacional";
            this.Load += new System.EventHandler(this.FormControlComedor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFoto)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.PictureBox pictureBoxFoto;
        private System.Windows.Forms.Label lblServicio;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Label lblSeccion;
        private System.Windows.Forms.Label lblDocumento;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Label lblRegistrosHoy;
        private System.Windows.Forms.Label lblHora;
        private System.Windows.Forms.Timer timerLimpiar;
        private System.Windows.Forms.Timer timerReloj;
    }
}