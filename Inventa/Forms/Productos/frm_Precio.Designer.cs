namespace Inventa.Forms.Productos
{
    partial class frm_Precio
    {
        private System.ComponentModel.IContainer components = null;

        // ── Panel de contexto (header) ────────────────────────────────────────
        private System.Windows.Forms.Panel panelContexto;
        private System.Windows.Forms.Label lblContextoTitulo;
        private System.Windows.Forms.Label lblContextoPresentacion;
        private System.Windows.Forms.Label lblContextoPrecioVigente;

        // ── Separador visual ──────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelSeparador;

        // ── Campos del formulario ─────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblTipoPrecio;
        private System.Windows.Forms.ComboBox cboTipoPrecio;
        private System.Windows.Forms.Label lblValor;
        private System.Windows.Forms.NumericUpDown numValor;
        private System.Windows.Forms.Label lblFechaVigencia;
        private System.Windows.Forms.DateTimePicker dtpFechaVigencia;
        private System.Windows.Forms.Label lblActivo;
        private System.Windows.Forms.CheckBox chkActivo;

        // ── Panel botones ─────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.FlowLayoutPanel flpBotones;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelContexto = new Panel();
            lblContextoPrecioVigente = new Label();
            lblContextoPresentacion = new Label();
            lblContextoTitulo = new Label();
            panelSeparador = new Panel();
            tlpMain = new TableLayoutPanel();
            lblTipoPrecio = new Label();
            cboTipoPrecio = new ComboBox();
            lblValor = new Label();
            numValor = new NumericUpDown();
            lblFechaVigencia = new Label();
            dtpFechaVigencia = new DateTimePicker();
            lblActivo = new Label();
            chkActivo = new CheckBox();
            panelBotones = new Panel();
            flpBotones = new FlowLayoutPanel();
            btnGuardar = new Button();
            btnCancelar = new Button();
            panelContexto.SuspendLayout();
            tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numValor).BeginInit();
            panelBotones.SuspendLayout();
            flpBotones.SuspendLayout();
            SuspendLayout();
            // 
            // panelContexto
            // 
            panelContexto.BackColor = Color.FromArgb(30, 60, 114);
            panelContexto.Controls.Add(lblContextoPrecioVigente);
            panelContexto.Controls.Add(lblContextoPresentacion);
            panelContexto.Controls.Add(lblContextoTitulo);
            panelContexto.Dock = DockStyle.Top;
            panelContexto.Location = new Point(0, 0);
            panelContexto.Name = "panelContexto";
            panelContexto.Padding = new Padding(16, 8, 16, 8);
            panelContexto.Size = new Size(484, 72);
            panelContexto.TabIndex = 0;
            // 
            // lblContextoPrecioVigente
            // 
            lblContextoPrecioVigente.Dock = DockStyle.Top;
            lblContextoPrecioVigente.Font = new Font("Segoe UI", 8F);
            lblContextoPrecioVigente.ForeColor = Color.FromArgb(180, 200, 230);
            lblContextoPrecioVigente.Location = new Point(16, 46);
            lblContextoPrecioVigente.Name = "lblContextoPrecioVigente";
            lblContextoPrecioVigente.Size = new Size(452, 16);
            lblContextoPrecioVigente.TabIndex = 2;
            // 
            // lblContextoPresentacion
            // 
            lblContextoPresentacion.Dock = DockStyle.Top;
            lblContextoPresentacion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblContextoPresentacion.ForeColor = Color.White;
            lblContextoPresentacion.Location = new Point(16, 24);
            lblContextoPresentacion.Name = "lblContextoPresentacion";
            lblContextoPresentacion.Size = new Size(452, 22);
            lblContextoPresentacion.TabIndex = 1;
            lblContextoPresentacion.Text = "-";
            // 
            // lblContextoTitulo
            // 
            lblContextoTitulo.Dock = DockStyle.Top;
            lblContextoTitulo.Font = new Font("Segoe UI", 7.5F);
            lblContextoTitulo.ForeColor = Color.FromArgb(180, 200, 230);
            lblContextoTitulo.Location = new Point(16, 8);
            lblContextoTitulo.Name = "lblContextoTitulo";
            lblContextoTitulo.Size = new Size(452, 16);
            lblContextoTitulo.TabIndex = 0;
            lblContextoTitulo.Text = "PRESENTACION";
            // 
            // panelSeparador
            // 
            panelSeparador.BackColor = Color.FromArgb(210, 210, 210);
            panelSeparador.Dock = DockStyle.Top;
            panelSeparador.Location = new Point(0, 72);
            panelSeparador.Name = "panelSeparador";
            panelSeparador.Size = new Size(484, 1);
            panelSeparador.TabIndex = 1;
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 2;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(lblTipoPrecio, 0, 0);
            tlpMain.Controls.Add(cboTipoPrecio, 1, 0);
            tlpMain.Controls.Add(lblValor, 0, 1);
            tlpMain.Controls.Add(numValor, 1, 1);
            tlpMain.Controls.Add(lblFechaVigencia, 0, 2);
            tlpMain.Controls.Add(dtpFechaVigencia, 1, 2);
            tlpMain.Controls.Add(lblActivo, 0, 3);
            tlpMain.Controls.Add(chkActivo, 1, 3);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 73);
            tlpMain.Name = "tlpMain";
            tlpMain.Padding = new Padding(16, 8, 16, 4);
            tlpMain.RowCount = 4;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            tlpMain.Size = new Size(484, 184);
            tlpMain.TabIndex = 2;
            // 
            // lblTipoPrecio
            // 
            lblTipoPrecio.Dock = DockStyle.Fill;
            lblTipoPrecio.Font = new Font("Segoe UI", 9F);
            lblTipoPrecio.ForeColor = Color.FromArgb(96, 94, 92);
            lblTipoPrecio.Location = new Point(19, 8);
            lblTipoPrecio.Name = "lblTipoPrecio";
            lblTipoPrecio.Size = new Size(124, 44);
            lblTipoPrecio.TabIndex = 0;
            lblTipoPrecio.Text = "Tipo Precio:";
            lblTipoPrecio.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboTipoPrecio
            // 
            cboTipoPrecio.Dock = DockStyle.Fill;
            cboTipoPrecio.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoPrecio.Font = new Font("Segoe UI", 9F);
            cboTipoPrecio.FormattingEnabled = true;
            cboTipoPrecio.Location = new Point(149, 11);
            cboTipoPrecio.Name = "cboTipoPrecio";
            cboTipoPrecio.Size = new Size(316, 23);
            cboTipoPrecio.TabIndex = 1;
            // 
            // lblValor
            // 
            lblValor.Dock = DockStyle.Fill;
            lblValor.Font = new Font("Segoe UI", 9F);
            lblValor.ForeColor = Color.FromArgb(96, 94, 92);
            lblValor.Location = new Point(19, 52);
            lblValor.Name = "lblValor";
            lblValor.Size = new Size(124, 44);
            lblValor.TabIndex = 2;
            lblValor.Text = "Precio:";
            lblValor.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numValor
            // 
            numValor.DecimalPlaces = 2;
            numValor.Dock = DockStyle.Fill;
            numValor.Font = new Font("Segoe UI", 9F);
            numValor.Location = new Point(149, 55);
            numValor.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            numValor.Name = "numValor";
            numValor.Size = new Size(316, 23);
            numValor.TabIndex = 2;
            numValor.ThousandsSeparator = true;
            // 
            // lblFechaVigencia
            // 
            lblFechaVigencia.Dock = DockStyle.Fill;
            lblFechaVigencia.Font = new Font("Segoe UI", 9F);
            lblFechaVigencia.ForeColor = Color.FromArgb(96, 94, 92);
            lblFechaVigencia.Location = new Point(19, 96);
            lblFechaVigencia.Name = "lblFechaVigencia";
            lblFechaVigencia.Size = new Size(124, 44);
            lblFechaVigencia.TabIndex = 3;
            lblFechaVigencia.Text = "Vigencia desde:";
            lblFechaVigencia.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dtpFechaVigencia
            // 
            dtpFechaVigencia.Dock = DockStyle.Fill;
            dtpFechaVigencia.Font = new Font("Segoe UI", 9F);
            dtpFechaVigencia.Format = DateTimePickerFormat.Short;
            dtpFechaVigencia.Location = new Point(149, 99);
            dtpFechaVigencia.Name = "dtpFechaVigencia";
            dtpFechaVigencia.Size = new Size(316, 23);
            dtpFechaVigencia.TabIndex = 3;
            // 
            // lblActivo
            // 
            lblActivo.Dock = DockStyle.Fill;
            lblActivo.Font = new Font("Segoe UI", 9F);
            lblActivo.ForeColor = Color.FromArgb(96, 94, 92);
            lblActivo.Location = new Point(19, 140);
            lblActivo.Name = "lblActivo";
            lblActivo.Size = new Size(124, 44);
            lblActivo.TabIndex = 4;
            lblActivo.Text = "Activo:";
            lblActivo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // chkActivo
            // 
            chkActivo.AutoSize = true;
            chkActivo.Checked = true;
            chkActivo.CheckState = CheckState.Checked;
            chkActivo.Dock = DockStyle.Fill;
            chkActivo.Font = new Font("Segoe UI", 9F);
            chkActivo.Location = new Point(149, 143);
            chkActivo.Name = "chkActivo";
            chkActivo.Size = new Size(316, 38);
            chkActivo.TabIndex = 4;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(243, 242, 241);
            panelBotones.Controls.Add(flpBotones);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 257);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(484, 56);
            panelBotones.TabIndex = 3;
            // 
            // flpBotones
            // 
            flpBotones.Controls.Add(btnGuardar);
            flpBotones.Controls.Add(btnCancelar);
            flpBotones.Dock = DockStyle.Fill;
            flpBotones.FlowDirection = FlowDirection.RightToLeft;
            flpBotones.Location = new Point(0, 0);
            flpBotones.Name = "flpBotones";
            flpBotones.Padding = new Padding(8);
            flpBotones.Size = new Size(484, 56);
            flpBotones.TabIndex = 0;
            flpBotones.WrapContents = false;
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.FromArgb(30, 60, 114);
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(358, 8);
            btnGuardar.Margin = new Padding(6, 0, 0, 0);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 36);
            btnGuardar.TabIndex = 0;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = false;
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = Color.White;
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(138, 136, 134);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Segoe UI", 9F);
            btnCancelar.ForeColor = Color.FromArgb(50, 49, 48);
            btnCancelar.Location = new Point(242, 8);
            btnCancelar.Margin = new Padding(6, 0, 0, 0);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            // 
            // frm_Precio
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 313);
            Controls.Add(tlpMain);
            Controls.Add(panelSeparador);
            Controls.Add(panelContexto);
            Controls.Add(panelBotones);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frm_Precio";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Precio de Venta";
            Load += Frm_Precio_Load;
            panelContexto.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numValor).EndInit();
            panelBotones.ResumeLayout(false);
            flpBotones.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}