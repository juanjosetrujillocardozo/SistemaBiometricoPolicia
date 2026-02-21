namespace SistemaBiometricoPolicia.Forms
{
    partial class FormRegistroEstudiantes
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
            this.btnImportarExcel = new System.Windows.Forms.Button();
            this.lblTotalEstudiantes = new System.Windows.Forms.Label();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnImportarExcel
            // 
            this.btnImportarExcel.BackColor = System.Drawing.Color.DarkBlue;
            this.btnImportarExcel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnImportarExcel.ForeColor = System.Drawing.Color.White;
            this.btnImportarExcel.Location = new System.Drawing.Point(50, 100);
            this.btnImportarExcel.Name = "btnImportarExcel";
            this.btnImportarExcel.Size = new System.Drawing.Size(300, 60);
            this.btnImportarExcel.TabIndex = 0;
            this.btnImportarExcel.Text = "📂 IMPORTAR EXCEL";
            this.btnImportarExcel.UseVisualStyleBackColor = false;
            this.btnImportarExcel.Click += new System.EventHandler(this.btnImportarExcel_Click);
            // 
            // lblTotalEstudiantes
            // 
            this.lblTotalEstudiantes.AutoSize = true;
            this.lblTotalEstudiantes.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblTotalEstudiantes.Location = new System.Drawing.Point(50, 180);
            this.lblTotalEstudiantes.Name = "lblTotalEstudiantes";
            this.lblTotalEstudiantes.Size = new System.Drawing.Size(200, 21);
            this.lblTotalEstudiantes.TabIndex = 1;
            this.lblTotalEstudiantes.Text = "Total de estudiantes: 0";
            // 
            // lblTitulo
            // 
            this.lblTitulo.BackColor = System.Drawing.Color.DarkGreen;
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(484, 70);
            this.lblTitulo.TabIndex = 2;
            this.lblTitulo.Text = "REGISTRO DE ESTUDIANTES";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormRegistroEstudiantes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(484, 261);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblTotalEstudiantes);
            this.Controls.Add(this.btnImportarExcel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FormRegistroEstudiantes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Registro de Estudiantes - Policía Nacional";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button btnImportarExcel;
        private System.Windows.Forms.Label lblTotalEstudiantes;
        private System.Windows.Forms.Label lblTitulo;
    }
}