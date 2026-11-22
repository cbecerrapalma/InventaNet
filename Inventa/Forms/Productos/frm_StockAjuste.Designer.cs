namespace Inventa.Forms.Productos
{
    partial class frm_StockAjuste
    {
        private System.ComponentModel.IContainer components = null;

        // ── Panel header ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitulo;

        // ── Campos del formulario ─────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpMain;

        // Fila 0: Producto (solo lectura)
        private System.Windows.Forms.Label lblProducto;
        private System.Windows.Forms.Label lblProductoValor;

        // Fila 1: Presentación
        private System.Windows.Forms.Label lblPresentacion;
        private System.Windows.Forms.ComboBox cboPresentacion;

        // Fila 2: Bodega — NUEVO (campo crítico que faltaba)
        private System.Windows.Forms.Label lblBodega;
        private System.Windows.Forms.ComboBox cboBodega;

        // Fila 3: Lote (visible solo si el producto maneja lotes)
        private System.Windows.Forms.Label lblLote;
        private System.Windows.Forms.ComboBox cboLote;

        // Fila 4: Stock actual (solo lectura, contexto para el operador)
        private System.Windows.Forms.Label lblStockActual;
        private System.Windows.Forms.Label lblStockActualValor;

        // Fila 5: Tipo de ajuste
        private System.Windows.Forms.Label lblTipoAjuste;
        private System.Windows.Forms.ComboBox cboTipoAjuste;

        // Fila 6: Cantidad
        private System.Windows.Forms.Label lblCantidad;
        private System.Windows.Forms.NumericUpDown numCantidad;

        // Fila 7: Motivo (obligatorio — va a MovInv.Motivo)
        private System.Windows.Forms.Label lblMotivo;
        private System.Windows.Forms.TextBox txtMotivo;

        // Fila 8: Observación (opcional)
        private System.Windows.Forms.Label lblObservacion;
        private System.Windows.Forms.TextBox txtObservacion;

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
            panelHeader = new Panel();
            lblTitulo = new Label();
            tlpMain = new TableLayoutPanel();
            lblProducto = new Label();
            lblProductoValor = new Label();
            lblPresentacion = new Label();
            cboPresentacion = new ComboBox();
            lblBodega = new Label();
            cboBodega = new ComboBox();
            lblLote = new Label();
            cboLote = new ComboBox();
            lblStockActual = new Label();
            lblStockActualValor = new Label();
            lblTipoAjuste = new Label();
            cboTipoAjuste = new ComboBox();
            lblCantidad = new Label();
            numCantidad = new NumericUpDown();
            lblMotivo = new Label();
            txtMotivo = new TextBox();
            lblObservacion = new Label();
            txtObservacion = new TextBox();
            panelBotones = new Panel();
            flpBotones = new FlowLayoutPanel();
            btnGuardar = new Button();
            btnCancelar = new Button();
            panelHeader.SuspendLayout();
            tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidad).BeginInit();
            panelBotones.SuspendLayout();
            flpBotones.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(30, 60, 114);
            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(500, 44);
            panelHeader.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(0, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(500, 44);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "AJUSTE DE STOCK";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 2;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(lblProducto, 0, 0);
            tlpMain.Controls.Add(lblProductoValor, 1, 0);
            tlpMain.Controls.Add(lblPresentacion, 0, 1);
            tlpMain.Controls.Add(cboPresentacion, 1, 1);
            tlpMain.Controls.Add(lblBodega, 0, 2);
            tlpMain.Controls.Add(cboBodega, 1, 2);
            tlpMain.Controls.Add(lblLote, 0, 3);
            tlpMain.Controls.Add(cboLote, 1, 3);
            tlpMain.Controls.Add(lblStockActual, 0, 4);
            tlpMain.Controls.Add(lblStockActualValor, 1, 4);
            tlpMain.Controls.Add(lblTipoAjuste, 0, 5);
            tlpMain.Controls.Add(cboTipoAjuste, 1, 5);
            tlpMain.Controls.Add(lblCantidad, 0, 6);
            tlpMain.Controls.Add(numCantidad, 1, 6);
            tlpMain.Controls.Add(lblMotivo, 0, 7);
            tlpMain.Controls.Add(txtMotivo, 1, 7);
            tlpMain.Controls.Add(lblObservacion, 0, 8);
            tlpMain.Controls.Add(txtObservacion, 1, 8);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 44);
            tlpMain.Name = "tlpMain";
            tlpMain.Padding = new Padding(14, 10, 14, 4);
            tlpMain.RowCount = 9;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.Size = new Size(500, 402);
            tlpMain.TabIndex = 1;
            // 
            // lblProducto
            // 
            lblProducto.Dock = DockStyle.Fill;
            lblProducto.Font = new Font("Segoe UI", 9F);
            lblProducto.ForeColor = Color.FromArgb(96, 94, 92);
            lblProducto.Location = new Point(17, 10);
            lblProducto.Name = "lblProducto";
            lblProducto.Size = new Size(114, 32);
            lblProducto.TabIndex = 0;
            lblProducto.Text = "Producto:";
            lblProducto.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblProductoValor
            // 
            lblProductoValor.Dock = DockStyle.Fill;
            lblProductoValor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblProductoValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblProductoValor.Location = new Point(137, 10);
            lblProductoValor.Name = "lblProductoValor";
            lblProductoValor.Size = new Size(346, 32);
            lblProductoValor.TabIndex = 1;
            lblProductoValor.Text = "-";
            lblProductoValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPresentacion
            // 
            lblPresentacion.Dock = DockStyle.Fill;
            lblPresentacion.Font = new Font("Segoe UI", 9F);
            lblPresentacion.ForeColor = Color.FromArgb(96, 94, 92);
            lblPresentacion.Location = new Point(17, 42);
            lblPresentacion.Name = "lblPresentacion";
            lblPresentacion.Size = new Size(114, 38);
            lblPresentacion.TabIndex = 2;
            lblPresentacion.Text = "Presentacion:";
            lblPresentacion.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboPresentacion
            // 
            cboPresentacion.Dock = DockStyle.Fill;
            cboPresentacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPresentacion.Font = new Font("Segoe UI", 9F);
            cboPresentacion.FormattingEnabled = true;
            cboPresentacion.Location = new Point(137, 45);
            cboPresentacion.Name = "cboPresentacion";
            cboPresentacion.Size = new Size(346, 23);
            cboPresentacion.TabIndex = 1;
            // 
            // lblBodega
            // 
            lblBodega.Dock = DockStyle.Fill;
            lblBodega.Font = new Font("Segoe UI", 9F);
            lblBodega.ForeColor = Color.FromArgb(96, 94, 92);
            lblBodega.Location = new Point(17, 80);
            lblBodega.Name = "lblBodega";
            lblBodega.Size = new Size(114, 38);
            lblBodega.TabIndex = 3;
            lblBodega.Text = "Bodega:";
            lblBodega.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboBodega
            // 
            cboBodega.Dock = DockStyle.Fill;
            cboBodega.DropDownStyle = ComboBoxStyle.DropDownList;
            cboBodega.Font = new Font("Segoe UI", 9F);
            cboBodega.FormattingEnabled = true;
            cboBodega.Location = new Point(137, 83);
            cboBodega.Name = "cboBodega";
            cboBodega.Size = new Size(346, 23);
            cboBodega.TabIndex = 2;
            // 
            // lblLote
            // 
            lblLote.Dock = DockStyle.Fill;
            lblLote.Font = new Font("Segoe UI", 9F);
            lblLote.ForeColor = Color.FromArgb(96, 94, 92);
            lblLote.Location = new Point(17, 118);
            lblLote.Name = "lblLote";
            lblLote.Size = new Size(114, 38);
            lblLote.TabIndex = 4;
            lblLote.Text = "Lote:";
            lblLote.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboLote
            // 
            cboLote.Dock = DockStyle.Fill;
            cboLote.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLote.Font = new Font("Segoe UI", 9F);
            cboLote.FormattingEnabled = true;
            cboLote.Location = new Point(137, 121);
            cboLote.Name = "cboLote";
            cboLote.Size = new Size(346, 23);
            cboLote.TabIndex = 3;
            // 
            // lblStockActual
            // 
            lblStockActual.Dock = DockStyle.Fill;
            lblStockActual.Font = new Font("Segoe UI", 8.5F);
            lblStockActual.ForeColor = Color.FromArgb(96, 94, 92);
            lblStockActual.Location = new Point(17, 156);
            lblStockActual.Name = "lblStockActual";
            lblStockActual.Size = new Size(114, 32);
            lblStockActual.TabIndex = 5;
            lblStockActual.Text = "Stock actual:";
            lblStockActual.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblStockActualValor
            // 
            lblStockActualValor.Dock = DockStyle.Fill;
            lblStockActualValor.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblStockActualValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblStockActualValor.Location = new Point(137, 156);
            lblStockActualValor.Name = "lblStockActualValor";
            lblStockActualValor.Size = new Size(346, 32);
            lblStockActualValor.TabIndex = 6;
            lblStockActualValor.Text = "-";
            lblStockActualValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTipoAjuste
            // 
            lblTipoAjuste.Dock = DockStyle.Fill;
            lblTipoAjuste.Font = new Font("Segoe UI", 9F);
            lblTipoAjuste.ForeColor = Color.FromArgb(96, 94, 92);
            lblTipoAjuste.Location = new Point(17, 188);
            lblTipoAjuste.Name = "lblTipoAjuste";
            lblTipoAjuste.Size = new Size(114, 38);
            lblTipoAjuste.TabIndex = 7;
            lblTipoAjuste.Text = "Tipo ajuste:";
            lblTipoAjuste.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboTipoAjuste
            // 
            cboTipoAjuste.Dock = DockStyle.Fill;
            cboTipoAjuste.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoAjuste.Font = new Font("Segoe UI", 9F);
            cboTipoAjuste.FormattingEnabled = true;
            cboTipoAjuste.Location = new Point(137, 191);
            cboTipoAjuste.Name = "cboTipoAjuste";
            cboTipoAjuste.Size = new Size(346, 23);
            cboTipoAjuste.TabIndex = 4;
            // 
            // lblCantidad
            // 
            lblCantidad.Dock = DockStyle.Fill;
            lblCantidad.Font = new Font("Segoe UI", 9F);
            lblCantidad.ForeColor = Color.FromArgb(96, 94, 92);
            lblCantidad.Location = new Point(17, 226);
            lblCantidad.Name = "lblCantidad";
            lblCantidad.Size = new Size(114, 38);
            lblCantidad.TabIndex = 8;
            lblCantidad.Text = "Cantidad:";
            lblCantidad.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numCantidad
            // 
            numCantidad.DecimalPlaces = 4;
            numCantidad.Dock = DockStyle.Fill;
            numCantidad.Font = new Font("Segoe UI", 9F);
            numCantidad.Location = new Point(137, 229);
            numCantidad.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numCantidad.Name = "numCantidad";
            numCantidad.Size = new Size(346, 23);
            numCantidad.TabIndex = 5;
            numCantidad.ThousandsSeparator = true;
            // 
            // lblMotivo
            // 
            lblMotivo.Dock = DockStyle.Fill;
            lblMotivo.Font = new Font("Segoe UI", 9F);
            lblMotivo.ForeColor = Color.FromArgb(96, 94, 92);
            lblMotivo.Location = new Point(17, 264);
            lblMotivo.Name = "lblMotivo";
            lblMotivo.Size = new Size(114, 38);
            lblMotivo.TabIndex = 9;
            lblMotivo.Text = "Motivo *:";
            lblMotivo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtMotivo
            // 
            txtMotivo.Dock = DockStyle.Fill;
            txtMotivo.Font = new Font("Segoe UI", 9F);
            txtMotivo.Location = new Point(137, 267);
            txtMotivo.MaxLength = 100;
            txtMotivo.Name = "txtMotivo";
            txtMotivo.Size = new Size(346, 23);
            txtMotivo.TabIndex = 6;
            // 
            // lblObservacion
            // 
            lblObservacion.Dock = DockStyle.Fill;
            lblObservacion.Font = new Font("Segoe UI", 9F);
            lblObservacion.ForeColor = Color.FromArgb(96, 94, 92);
            lblObservacion.Location = new Point(17, 302);
            lblObservacion.Name = "lblObservacion";
            lblObservacion.Padding = new Padding(0, 6, 0, 0);
            lblObservacion.Size = new Size(114, 96);
            lblObservacion.TabIndex = 10;
            lblObservacion.Text = "Observacion:";
            lblObservacion.TextAlign = ContentAlignment.TopRight;
            // 
            // txtObservacion
            // 
            txtObservacion.Dock = DockStyle.Fill;
            txtObservacion.Font = new Font("Segoe UI", 9F);
            txtObservacion.Location = new Point(137, 305);
            txtObservacion.MaxLength = 500;
            txtObservacion.Multiline = true;
            txtObservacion.Name = "txtObservacion";
            txtObservacion.ScrollBars = ScrollBars.Vertical;
            txtObservacion.Size = new Size(346, 90);
            txtObservacion.TabIndex = 7;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(243, 242, 241);
            panelBotones.Controls.Add(flpBotones);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 446);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(500, 56);
            panelBotones.TabIndex = 2;
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
            flpBotones.Size = new Size(500, 56);
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
            btnGuardar.Location = new Point(374, 8);
            btnGuardar.Margin = new Padding(6, 0, 0, 0);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 36);
            btnGuardar.TabIndex = 0;
            btnGuardar.Text = "Ajustar";
            btnGuardar.UseVisualStyleBackColor = false;
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = Color.White;
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(138, 136, 134);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Segoe UI", 9F);
            btnCancelar.ForeColor = Color.FromArgb(50, 49, 48);
            btnCancelar.Location = new Point(258, 8);
            btnCancelar.Margin = new Padding(6, 0, 0, 0);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            // 
            // frm_StockAjuste
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 502);
            Controls.Add(tlpMain);
            Controls.Add(panelBotones);
            Controls.Add(panelHeader);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frm_StockAjuste";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Ajuste de Stock";
            Load += Frm_StockAjuste_Load;
            panelHeader.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidad).EndInit();
            panelBotones.ResumeLayout(false);
            flpBotones.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}