namespace SistemaBiometricoPolicia.Forms
{
    partial class FormPrincipal
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
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.btnComedor = new System.Windows.Forms.Button();
            this.btnEnrolamiento = new System.Windows.Forms.Button();
            this.btnReportes = new System.Windows.Forms.Button();
            this.btnSincronizar = new System.Windows.Forms.Button();
            this.btnImportar = new System.Windows.Forms.Button();
            this.btnAcercaDe = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();
            this.lblEstadisticas = new System.Windows.Forms.Label();
            this.panelHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.panelHeader.Controls.Add(this.lblTitulo);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(800, 120);
            this.panelHeader.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(800, 120);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "SISTEMA BIOMÉTRICO\r\nControl de Alimentación - Policía Nacional";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnComedor
            // 
            this.btnComedor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnComedor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnComedor.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnComedor.ForeColor = System.Drawing.Color.White;
            this.btnComedor.Location = new System.Drawing.Point(75, 140);
            this.btnComedor.Name = "btnComedor";
            this.btnComedor.Size = new System.Drawing.Size(400, 55);
            this.btnComedor.TabIndex = 1;
            this.btnComedor.Text = "🍴 CONTROL COMEDOR";
            this.btnComedor.UseVisualStyleBackColor = false;
            this.btnComedor.Click += new System.EventHandler(this.btnComedor_Click);
            // 
            // btnEnrolamiento
            // 
            this.btnEnrolamiento.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnEnrolamiento.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnrolamiento.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnEnrolamiento.ForeColor = System.Drawing.Color.White;
            this.btnEnrolamiento.Location = new System.Drawing.Point(75, 205);
            this.btnEnrolamiento.Name = "btnEnrolamiento";
            this.btnEnrolamiento.Size = new System.Drawing.Size(400, 55);
            this.btnEnrolamiento.TabIndex = 2;
            this.btnEnrolamiento.Text = "👤 ENROLAR ESTUDIANTES";
            this.btnEnrolamiento.UseVisualStyleBackColor = false;
            this.btnEnrolamiento.Click += new System.EventHandler(this.btnEnrolamiento_Click);
            // 
            // btnReportes
            // 
            this.btnReportes.BackColor = System.Drawing.Color.Green;
            this.btnReportes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReportes.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnReportes.ForeColor = System.Drawing.Color.White;
            this.btnReportes.Location = new System.Drawing.Point(75, 270);
            this.btnReportes.Name = "btnReportes";
            this.btnReportes.Size = new System.Drawing.Size(400, 55);
            this.btnReportes.TabIndex = 3;
            this.btnReportes.Text = "📊 GENERAR REPORTES";
            this.btnReportes.UseVisualStyleBackColor = false;
            this.btnReportes.Click += new System.EventHandler(this.btnReportes_Click);
            // 
            // btnSincronizar
            // 
            this.btnSincronizar.BackColor = System.Drawing.Color.Purple;
            this.btnSincronizar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSincronizar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnSincronizar.ForeColor = System.Drawing.Color.White;
            this.btnSincronizar.Location = new System.Drawing.Point(75, 335);
            this.btnSincronizar.Name = "btnSincronizar";
            this.btnSincronizar.Size = new System.Drawing.Size(400, 55);
            this.btnSincronizar.TabIndex = 4;
            this.btnSincronizar.Text = "☁ SINCRONIZAR DATOS";
            this.btnSincronizar.UseVisualStyleBackColor = false;
            this.btnSincronizar.Click += new System.EventHandler(this.btnSincronizar_Click);
            // 
            // btnImportar
            // 
            this.btnImportar.BackColor = System.Drawing.Color.Teal;
            this.btnImportar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImportar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnImportar.ForeColor = System.Drawing.Color.White;
            this.btnImportar.Location = new System.Drawing.Point(75, 400);
            this.btnImportar.Name = "btnImportar";
            this.btnImportar.Size = new System.Drawing.Size(400, 55);
            this.btnImportar.TabIndex = 5;
            this.btnImportar.Text = "📥 IMPORTAR ESTUDIANTES";
            this.btnImportar.UseVisualStyleBackColor = false;
            this.btnImportar.Click += new System.EventHandler(this.btnImportar_Click);
            // 
            // btnAcercaDe
            // 
            this.btnAcercaDe.BackColor = System.Drawing.Color.DarkSlateGray;
            this.btnAcercaDe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAcercaDe.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAcercaDe.ForeColor = System.Drawing.Color.White;
            // btnAcercaDe
            this.btnAcercaDe.Location = new System.Drawing.Point(75, 450); // ✅ Subido de 465 a 450
            this.btnAcercaDe.Name = "btnAcercaDe";
            this.btnAcercaDe.Size = new System.Drawing.Size(195, 40);
            this.btnAcercaDe.TabIndex = 6;
            this.btnAcercaDe.Text = "ℹ ACERCA DE";
            this.btnAcercaDe.UseVisualStyleBackColor = false;
            this.btnAcercaDe.Click += new System.EventHandler(this.btnAcercaDe_Click);
            // 
            // btnSalir
            // 
            this.btnSalir.BackColor = System.Drawing.Color.Maroon;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            // btnSalir
            this.btnSalir.Location = new System.Drawing.Point(280, 450); // ✅ Subido de 465 a 450
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(195, 40);
            this.btnSalir.TabIndex = 7;
            this.btnSalir.Text = "❌ SALIR";
            this.btnSalir.UseVisualStyleBackColor = false;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // lblEstadisticas
            // 
            this.lblEstadisticas.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lblEstadisticas.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            // lblEstadisticas
            this.lblEstadisticas.Location = new System.Drawing.Point(75, 495); // ✅ Subido de 515 a 495
            this.lblEstadisticas.Name = "lblEstadisticas";
            this.lblEstadisticas.Size = new System.Drawing.Size(400, 30);
            this.lblEstadisticas.TabIndex = 8;
            this.lblEstadisticas.Text = "📊 Estudiantes: 0 | Registros Hoy: 0";
            this.lblEstadisticas.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            // ClientSize del formulario
            this.ClientSize = new System.Drawing.Size(800, 750); // ✅ Ancho aumentado de 550 a 800
            this.Controls.Add(this.lblEstadisticas);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.btnAcercaDe);
            this.Controls.Add(this.btnImportar);
            this.Controls.Add(this.btnSincronizar);
            this.Controls.Add(this.btnReportes);
            this.Controls.Add(this.btnEnrolamiento);
            this.Controls.Add(this.btnComedor);
            this.Controls.Add(this.panelHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "FormPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sistema Biométrico - TRUJO TECHNOLOGIES";
            this.Load += new System.EventHandler(this.FormPrincipal_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormPrincipal_KeyDown);
            this.panelHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Button btnComedor;
        private System.Windows.Forms.Button btnEnrolamiento;
        private System.Windows.Forms.Button btnReportes;
        private System.Windows.Forms.Button btnSincronizar;
        private System.Windows.Forms.Button btnImportar;
        private System.Windows.Forms.Button btnAcercaDe;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.Label lblEstadisticas;
    }
}