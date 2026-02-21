namespace SistemaBiometricoPolicia.Forms
{
    partial class FormActivacionLicencia
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
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblClave = new System.Windows.Forms.Label();
            this.txtClaveMaestra = new System.Windows.Forms.TextBox();
            this.lblToken = new System.Windows.Forms.Label();
            this.txtToken = new System.Windows.Forms.TextBox();
            this.lblMeses = new System.Windows.Forms.Label();
            this.numMeses = new System.Windows.Forms.NumericUpDown();
            this.btnActivar = new System.Windows.Forms.Button();
            this.btnBloquear = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.lblAdvertencia = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numMeses)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(32)))), ((int)(((byte)(96)))));
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(450, 50);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "🔐 PANEL DE ACTIVACIÓN - TRUJO TECHNOLOGIES";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblClave
            // 
            this.lblClave.AutoSize = true;
            this.lblClave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblClave.Location = new System.Drawing.Point(30, 70);
            this.lblClave.Name = "lblClave";
            this.lblClave.Size = new System.Drawing.Size(110, 19);
            this.lblClave.TabIndex = 1;
            this.lblClave.Text = "Clave Maestra:";
            // 
            // txtClaveMaestra
            // 
            this.txtClaveMaestra.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtClaveMaestra.Location = new System.Drawing.Point(30, 95);
            this.txtClaveMaestra.Name = "txtClaveMaestra";
            this.txtClaveMaestra.PasswordChar = '●';
            this.txtClaveMaestra.Size = new System.Drawing.Size(390, 25);
            this.txtClaveMaestra.TabIndex = 2;
            // 
            // lblToken
            // 
            this.lblToken.AutoSize = true;
            this.lblToken.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblToken.Location = new System.Drawing.Point(30, 135);
            this.lblToken.Name = "lblToken";
            this.lblToken.Size = new System.Drawing.Size(140, 19);
            this.lblToken.TabIndex = 3;
            this.lblToken.Text = "Token de Activación:";
            // 
            // txtToken
            // 
            this.txtToken.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtToken.Location = new System.Drawing.Point(30, 160);
            this.txtToken.Name = "txtToken";
            this.txtToken.Size = new System.Drawing.Size(390, 23);
            this.txtToken.TabIndex = 4;
            this.txtToken.Text = "FUSAGASUGA-2026";
            // 
            // lblMeses
            // 
            this.lblMeses.AutoSize = true;
            this.lblMeses.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblMeses.Location = new System.Drawing.Point(30, 200);
            this.lblMeses.Name = "lblMeses";
            this.lblMeses.Size = new System.Drawing.Size(120, 19);
            this.lblMeses.TabIndex = 5;
            this.lblMeses.Text = "Meses de Validez:";
            // 
            // numMeses
            // 
            this.numMeses.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.numMeses.Location = new System.Drawing.Point(30, 225);
            this.numMeses.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numMeses.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMeses.Name = "numMeses";
            this.numMeses.Size = new System.Drawing.Size(120, 25);
            this.numMeses.TabIndex = 6;
            this.numMeses.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnActivar
            // 
            this.btnActivar.BackColor = System.Drawing.Color.Green;
            this.btnActivar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnActivar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActivar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnActivar.ForeColor = System.Drawing.Color.White;
            this.btnActivar.Location = new System.Drawing.Point(30, 270);
            this.btnActivar.Name = "btnActivar";
            this.btnActivar.Size = new System.Drawing.Size(180, 45);
            this.btnActivar.TabIndex = 7;
            this.btnActivar.Text = "✅ ACTIVAR LICENCIA";
            this.btnActivar.UseVisualStyleBackColor = false;
            this.btnActivar.Click += new System.EventHandler(this.btnActivar_Click);
            // 
            // btnBloquear
            // 
            this.btnBloquear.BackColor = System.Drawing.Color.DarkRed;
            this.btnBloquear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBloquear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBloquear.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnBloquear.ForeColor = System.Drawing.Color.White;
            this.btnBloquear.Location = new System.Drawing.Point(240, 270);
            this.btnBloquear.Name = "btnBloquear";
            this.btnBloquear.Size = new System.Drawing.Size(180, 45);
            this.btnBloquear.TabIndex = 8;
            this.btnBloquear.Text = "🔒 BLOQUEAR SISTEMA";
            this.btnBloquear.UseVisualStyleBackColor = false;
            this.btnBloquear.Click += new System.EventHandler(this.btnBloquear_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.BackColor = System.Drawing.Color.Gray;
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(30, 330);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(390, 35);
            this.btnCancelar.TabIndex = 9;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // lblAdvertencia
            // 
            this.lblAdvertencia.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
            this.lblAdvertencia.ForeColor = System.Drawing.Color.Red;
            this.lblAdvertencia.Location = new System.Drawing.Point(30, 375);
            this.lblAdvertencia.Name = "lblAdvertencia";
            this.lblAdvertencia.Size = new System.Drawing.Size(390, 30);
            this.lblAdvertencia.TabIndex = 10;
            this.lblAdvertencia.Text = "⚠️ ACCESO RESTRINGIDO - Solo personal autorizado de TRUJO TECHNOLOGIES";
            this.lblAdvertencia.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormActivacionLicencia
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(450, 420);
            this.Controls.Add(this.lblAdvertencia);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnBloquear);
            this.Controls.Add(this.btnActivar);
            this.Controls.Add(this.numMeses);
            this.Controls.Add(this.lblMeses);
            this.Controls.Add(this.txtToken);
            this.Controls.Add(this.lblToken);
            this.Controls.Add(this.txtClaveMaestra);
            this.Controls.Add(this.lblClave);
            this.Controls.Add(this.lblTitulo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormActivacionLicencia";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TRUJO TECHNOLOGIES - Panel de Licencias";
            ((System.ComponentModel.ISupportInitialize)(this.numMeses)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblClave;
        private System.Windows.Forms.TextBox txtClaveMaestra;
        private System.Windows.Forms.Label lblToken;
        private System.Windows.Forms.TextBox txtToken;
        private System.Windows.Forms.Label lblMeses;
        private System.Windows.Forms.NumericUpDown numMeses;
        private System.Windows.Forms.Button btnActivar;
        private System.Windows.Forms.Button btnBloquear;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Label lblAdvertencia;
    }
}