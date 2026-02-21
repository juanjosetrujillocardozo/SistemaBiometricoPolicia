namespace SistemaBiometricoPolicia.Forms
{
    partial class FormAcercaDe
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
            this.panelInfo = new System.Windows.Forms.Panel();
            this.lblContacto = new System.Windows.Forms.Label();
            this.lblEmpresa = new System.Windows.Forms.Label();
            this.lblEstadoLicencia = new System.Windows.Forms.Label();
            this.lblVersionRemota = new System.Windows.Forms.Label();
            this.lblVersionLocal = new System.Windows.Forms.Label();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.btnVerificarLicencia = new System.Windows.Forms.Button();
            this.btnBuscarActualizaciones = new System.Windows.Forms.Button();
            this.panelHeader.SuspendLayout();
            this.panelInfo.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.panelHeader.Controls.Add(this.lblTitulo);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(500, 70);
            this.panelHeader.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(500, 70);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "INFORMACIÓN DEL SISTEMA\r\nTRUJO TECHNOLOGIES S.A.S.";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelInfo
            // 
            this.panelInfo.BackColor = System.Drawing.Color.White;
            this.panelInfo.Controls.Add(this.lblContacto);
            this.panelInfo.Controls.Add(this.lblEmpresa);
            this.panelInfo.Controls.Add(this.lblEstadoLicencia);
            this.panelInfo.Controls.Add(this.lblVersionRemota);
            this.panelInfo.Controls.Add(this.lblVersionLocal);
            this.panelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelInfo.Location = new System.Drawing.Point(0, 70);
            this.panelInfo.Name = "panelInfo";
            this.panelInfo.Size = new System.Drawing.Size(500, 200);
            this.panelInfo.TabIndex = 1;
            // 
            // lblContacto
            // 
            this.lblContacto.AutoSize = true;
            this.lblContacto.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblContacto.ForeColor = System.Drawing.Color.DimGray;
            this.lblContacto.Location = new System.Drawing.Point(30, 165);
            this.lblContacto.Name = "lblContacto";
            this.lblContacto.Size = new System.Drawing.Size(220, 15);
            this.lblContacto.TabIndex = 4;
            this.lblContacto.Text = "Contacto: trujotechnologies@gmail.com";
            // 
            // lblEmpresa
            // 
            this.lblEmpresa.AutoSize = true;
            this.lblEmpresa.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblEmpresa.Location = new System.Drawing.Point(30, 140);
            this.lblEmpresa.Name = "lblEmpresa";
            this.lblEmpresa.Size = new System.Drawing.Size(280, 19);
            this.lblEmpresa.TabIndex = 3;
            this.lblEmpresa.Text = "Desarrollado por: TRUJO TECHNOLOGIES S.A.S";
            // 
            // lblEstadoLicencia
            // 
            this.lblEstadoLicencia.AutoSize = true;
            this.lblEstadoLicencia.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblEstadoLicencia.Location = new System.Drawing.Point(30, 95);
            this.lblEstadoLicencia.Name = "lblEstadoLicencia";
            this.lblEstadoLicencia.Size = new System.Drawing.Size(150, 20);
            this.lblEstadoLicencia.TabIndex = 2;
            this.lblEstadoLicencia.Text = "Verificando licencia...";
            // 
            // lblVersionRemota
            // 
            this.lblVersionRemota.AutoSize = true;
            this.lblVersionRemota.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblVersionRemota.ForeColor = System.Drawing.Color.Gray;
            this.lblVersionRemota.Location = new System.Drawing.Point(30, 55);
            this.lblVersionRemota.Name = "lblVersionRemota";
            this.lblVersionRemota.Size = new System.Drawing.Size(150, 19);
            this.lblVersionRemota.TabIndex = 1;
            this.lblVersionRemota.Text = "Consultando servidor...";
            // 
            // lblVersionLocal
            // 
            this.lblVersionLocal.AutoSize = true;
            this.lblVersionLocal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblVersionLocal.Location = new System.Drawing.Point(30, 25);
            this.lblVersionLocal.Name = "lblVersionLocal";
            this.lblVersionLocal.Size = new System.Drawing.Size(150, 19);
            this.lblVersionLocal.TabIndex = 0;
            this.lblVersionLocal.Text = "Versión instalada: 1.0.0";
            // 
            // panelButtons
            // 
            this.panelButtons.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelButtons.Controls.Add(this.btnCerrar);
            this.panelButtons.Controls.Add(this.btnVerificarLicencia);
            this.panelButtons.Controls.Add(this.btnBuscarActualizaciones);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.Location = new System.Drawing.Point(0, 270);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(500, 130);
            this.panelButtons.TabIndex = 2;
            // 
            // btnCerrar
            // 
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.Location = new System.Drawing.Point(150, 75);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(200, 40);
            this.btnCerrar.TabIndex = 2;
            this.btnCerrar.Text = "CERRAR";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // btnVerificarLicencia
            // 
            this.btnVerificarLicencia.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.btnVerificarLicencia.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerificarLicencia.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVerificarLicencia.ForeColor = System.Drawing.Color.White;
            this.btnVerificarLicencia.Location = new System.Drawing.Point(260, 20);
            this.btnVerificarLicencia.Name = "btnVerificarLicencia";
            this.btnVerificarLicencia.Size = new System.Drawing.Size(210, 45);
            this.btnVerificarLicencia.TabIndex = 1;
            this.btnVerificarLicencia.Text = "VERIFICAR LICENCIA";
            this.btnVerificarLicencia.UseVisualStyleBackColor = false;
            this.btnVerificarLicencia.Click += new System.EventHandler(this.btnVerificarLicencia_Click);
            // 
            // btnBuscarActualizaciones
            // 
            this.btnBuscarActualizaciones.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnBuscarActualizaciones.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscarActualizaciones.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnBuscarActualizaciones.ForeColor = System.Drawing.Color.White;
            this.btnBuscarActualizaciones.Location = new System.Drawing.Point(30, 20);
            this.btnBuscarActualizaciones.Name = "btnBuscarActualizaciones";
            this.btnBuscarActualizaciones.Size = new System.Drawing.Size(210, 45);
            this.btnBuscarActualizaciones.TabIndex = 0;
            this.btnBuscarActualizaciones.Text = "BUSCAR ACTUALIZACIÓN";
            this.btnBuscarActualizaciones.UseVisualStyleBackColor = false;
            this.btnBuscarActualizaciones.Click += new System.EventHandler(this.btnBuscarActualizaciones_Click);
            // 
            // FormAcercaDe
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 400);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.panelInfo);
            this.Controls.Add(this.panelHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAcercaDe";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Acerca de TRUJO TECHNOLOGIES";
            this.panelHeader.ResumeLayout(false);
            this.panelInfo.ResumeLayout(false);
            this.panelInfo.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel panelInfo;
        private System.Windows.Forms.Label lblContacto;
        private System.Windows.Forms.Label lblEmpresa;
        private System.Windows.Forms.Label lblEstadoLicencia;
        private System.Windows.Forms.Label lblVersionRemota;
        private System.Windows.Forms.Label lblVersionLocal;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.Button btnVerificarLicencia;
        private System.Windows.Forms.Button btnBuscarActualizaciones;
    }
}