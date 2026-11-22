namespace Inventa.Forms.Productos
{
    partial class frm_Presentacion
    {
        private System.ComponentModel.IContainer components = null;

        // ── Controles principales ───────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.GroupBox gpbDatos;
        private System.Windows.Forms.TableLayoutPanel tlpDatos;
        private System.Windows.Forms.Label lblProducto;
        private System.Windows.Forms.TextBox txtProductoInfo;
        private System.Windows.Forms.Label lblDescrip;
        private System.Windows.Forms.TextBox txtDescrip;
        private System.Windows.Forms.Label lblUnidad;
        private System.Windows.Forms.ComboBox cboUnidad;
        private System.Windows.Forms.CheckBox chkEsVenta;
        private System.Windows.Forms.CheckBox chkEsCompra;
        private System.Windows.Forms.CheckBox chkActivo;
        private System.Windows.Forms.FlowLayoutPanel flpBotones;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tlpMain = new TableLayoutPanel();
            lblTitulo = new Label();
            gpbDatos = new GroupBox();
            tlpDatos = new TableLayoutPanel();
            lblProducto = new Label();
            txtProductoInfo = new TextBox();
            lblDescrip = new Label();
            txtDescrip = new TextBox();
            lblUnidad = new Label();
            cboUnidad = new ComboBox();
            chkEsVenta = new CheckBox();
            chkEsCompra = new CheckBox();
            chkActivo = new CheckBox();
            flpBotones = new FlowLayoutPanel();
            btnOK = new Button();
            btnCancel = new Button();
            tlpMain.SuspendLayout();
            gpbDatos.SuspendLayout();
            tlpDatos.SuspendLayout();
            flpBotones.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(lblTitulo, 0, 0);
            tlpMain.Controls.Add(gpbDatos, 0, 1);
            tlpMain.Controls.Add(flpBotones, 0, 2);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.Padding = new Padding(10);
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tlpMain.Size = new Size(500, 320);
            tlpMain.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitulo.Location = new Point(13, 10);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(474, 30);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Presentación de Producto";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // gpbDatos
            // 
            gpbDatos.Controls.Add(tlpDatos);
            gpbDatos.Dock = DockStyle.Fill;
            gpbDatos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gpbDatos.Location = new Point(13, 43);
            gpbDatos.Name = "gpbDatos";
            gpbDatos.Size = new Size(474, 214);
            gpbDatos.TabIndex = 1;
            gpbDatos.TabStop = false;
            gpbDatos.Text = "Datos de la Presentación";
            // 
            // tlpDatos
            // 
            tlpDatos.ColumnCount = 2;
            tlpDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpDatos.Controls.Add(lblProducto, 0, 0);
            tlpDatos.Controls.Add(txtProductoInfo, 1, 0);
            tlpDatos.Controls.Add(lblDescrip, 0, 1);
            tlpDatos.Controls.Add(txtDescrip, 1, 1);
            tlpDatos.Controls.Add(lblUnidad, 0, 2);
            tlpDatos.Controls.Add(cboUnidad, 1, 2);
            tlpDatos.Controls.Add(chkEsVenta, 0, 3);
            tlpDatos.Controls.Add(chkEsCompra, 1, 3);
            tlpDatos.Controls.Add(chkActivo, 0, 4);
            tlpDatos.Dock = DockStyle.Fill;
            tlpDatos.Location = new Point(3, 19);
            tlpDatos.Name = "tlpDatos";
            tlpDatos.Padding = new Padding(5);
            tlpDatos.RowCount = 5;
            tlpDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpDatos.Size = new Size(468, 192);
            tlpDatos.TabIndex = 0;
            // 
            // lblProducto
            // 
            lblProducto.Dock = DockStyle.Fill;
            lblProducto.Font = new Font("Segoe UI", 9F);
            lblProducto.Location = new Point(8, 5);
            lblProducto.Name = "lblProducto";
            lblProducto.Size = new Size(94, 30);
            lblProducto.TabIndex = 0;
            lblProducto.Text = "Producto:";
            lblProducto.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtProductoInfo
            // 
            txtProductoInfo.BackColor = Color.White;
            txtProductoInfo.Dock = DockStyle.Fill;
            txtProductoInfo.Font = new Font("Segoe UI", 9F);
            txtProductoInfo.Location = new Point(108, 8);
            txtProductoInfo.Name = "txtProductoInfo";
            txtProductoInfo.ReadOnly = true;
            txtProductoInfo.Size = new Size(352, 23);
            txtProductoInfo.TabIndex = 1;
            txtProductoInfo.TabStop = false;
            // 
            // lblDescrip
            // 
            lblDescrip.Dock = DockStyle.Fill;
            lblDescrip.Font = new Font("Segoe UI", 9F);
            lblDescrip.Location = new Point(8, 35);
            lblDescrip.Name = "lblDescrip";
            lblDescrip.Size = new Size(94, 30);
            lblDescrip.TabIndex = 2;
            lblDescrip.Text = "Descripción:";
            lblDescrip.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtDescrip
            // 
            txtDescrip.Dock = DockStyle.Fill;
            txtDescrip.Font = new Font("Segoe UI", 9F);
            txtDescrip.Location = new Point(108, 38);
            txtDescrip.MaxLength = 100;
            txtDescrip.Name = "txtDescrip";
            txtDescrip.Size = new Size(352, 23);
            txtDescrip.TabIndex = 3;
            // 
            // lblUnidad
            // 
            lblUnidad.Dock = DockStyle.Fill;
            lblUnidad.Font = new Font("Segoe UI", 9F);
            lblUnidad.Location = new Point(8, 65);
            lblUnidad.Name = "lblUnidad";
            lblUnidad.Size = new Size(94, 30);
            lblUnidad.TabIndex = 4;
            lblUnidad.Text = "Unidad:";
            lblUnidad.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboUnidad
            // 
            cboUnidad.Dock = DockStyle.Fill;
            cboUnidad.DropDownStyle = ComboBoxStyle.DropDownList;
            cboUnidad.Font = new Font("Segoe UI", 9F);
            cboUnidad.FormattingEnabled = true;
            cboUnidad.Location = new Point(108, 68);
            cboUnidad.Name = "cboUnidad";
            cboUnidad.Size = new Size(352, 23);
            cboUnidad.TabIndex = 5;
            // 
            // chkEsVenta
            // 
            chkEsVenta.AutoSize = true;
            chkEsVenta.Dock = DockStyle.Fill;
            chkEsVenta.Font = new Font("Segoe UI", 9F);
            chkEsVenta.Location = new Point(8, 98);
            chkEsVenta.Name = "chkEsVenta";
            chkEsVenta.Size = new Size(94, 24);
            chkEsVenta.TabIndex = 6;
            chkEsVenta.Text = "Venta";
            chkEsVenta.UseVisualStyleBackColor = true;
            // 
            // chkEsCompra
            // 
            chkEsCompra.AutoSize = true;
            chkEsCompra.Dock = DockStyle.Fill;
            chkEsCompra.Font = new Font("Segoe UI", 9F);
            chkEsCompra.Location = new Point(108, 98);
            chkEsCompra.Name = "chkEsCompra";
            chkEsCompra.Size = new Size(352, 24);
            chkEsCompra.TabIndex = 7;
            chkEsCompra.Text = "Compra";
            chkEsCompra.UseVisualStyleBackColor = true;
            // 
            // chkActivo
            // 
            chkActivo.AutoSize = true;
            tlpDatos.SetColumnSpan(chkActivo, 2);
            chkActivo.Font = new Font("Segoe UI", 9F);
            chkActivo.Location = new Point(8, 128);
            chkActivo.Name = "chkActivo";
            chkActivo.Size = new Size(60, 19);
            chkActivo.TabIndex = 8;
            chkActivo.Text = "Activo";
            chkActivo.UseVisualStyleBackColor = true;
            // 
            // flpBotones
            // 
            flpBotones.Controls.Add(btnOK);
            flpBotones.Controls.Add(btnCancel);
            flpBotones.Dock = DockStyle.Fill;
            flpBotones.FlowDirection = FlowDirection.RightToLeft;
            flpBotones.Location = new Point(13, 263);
            flpBotones.Name = "flpBotones";
            flpBotones.Padding = new Padding(0, 5, 0, 5);
            flpBotones.Size = new Size(474, 44);
            flpBotones.TabIndex = 2;
            // 
            // btnOK
            // 
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnOK.ForeColor = Color.White;
            btnOK.Location = new Point(368, 8);
            btnOK.Margin = new Padding(3, 3, 6, 3);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(100, 30);
            btnOK.TabIndex = 0;
            btnOK.Text = "Aceptar";
            btnOK.UseVisualStyleBackColor = false;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 9F);
            btnCancel.Location = new Point(262, 8);
            btnCancel.Margin = new Padding(0, 3, 3, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 30);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancelar";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += BtnCancel_Click;
            // 
            // frm_Presentacion
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(500, 320);
            Controls.Add(tlpMain);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frm_Presentacion";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Presentación";
            Load += Frm_Presentacion_Load;
            tlpMain.ResumeLayout(false);
            gpbDatos.ResumeLayout(false);
            tlpDatos.ResumeLayout(false);
            tlpDatos.PerformLayout();
            flpBotones.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}