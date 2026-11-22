namespace Inventa.Forms.Productos
{
    partial class frm_StockTransferir
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

        // Fila 1: Bodega Origen
        private System.Windows.Forms.Label lblOrigen;
        private System.Windows.Forms.ComboBox cboOrigen;

        // Fila 2: Bodega Destino
        private System.Windows.Forms.Label lblDestino;
        private System.Windows.Forms.ComboBox cboDestino;

        // Fila 3: Presentación
        private System.Windows.Forms.Label lblPresentacion;
        private System.Windows.Forms.ComboBox cboPresentacion;

        // Fila 4: Lote (visible solo si el producto maneja lotes)
        private System.Windows.Forms.Label lblLote;
        private System.Windows.Forms.ComboBox cboLote;

        // Fila 5: Stock disponible en origen (solo lectura, contexto para el operador)
        private System.Windows.Forms.Label lblStockOrigen;
        private System.Windows.Forms.Label lblStockOrigenValor;

        // Fila 6: Cantidad
        private System.Windows.Forms.Label lblCantidad;
        private System.Windows.Forms.NumericUpDown numCantidad;

        // Fila 7: Observación (opcional)
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
            lblOrigen = new Label();
            cboOrigen = new ComboBox();
            lblDestino = new Label();
            cboDestino = new ComboBox();
            lblPresentacion = new Label();
            cboPresentacion = new ComboBox();
            lblLote = new Label();
            cboLote = new ComboBox();
            lblStockOrigen = new Label();
            lblStockOrigenValor = new Label();
            lblCantidad = new Label();
            numCantidad = new NumericUpDown();
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
            lblTitulo.Text = "TRANSFERENCIA DE STOCK";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 2;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(lblProducto, 0, 0);
            tlpMain.Controls.Add(lblProductoValor, 1, 0);
            tlpMain.Controls.Add(lblOrigen, 0, 1);
            tlpMain.Controls.Add(cboOrigen, 1, 1);
            tlpMain.Controls.Add(lblDestino, 0, 2);
            tlpMain.Controls.Add(cboDestino, 1, 2);
            tlpMain.Controls.Add(lblPresentacion, 0, 3);
            tlpMain.Controls.Add(cboPresentacion, 1, 3);
            tlpMain.Controls.Add(lblLote, 0, 4);
            tlpMain.Controls.Add(cboLote, 1, 4);
            tlpMain.Controls.Add(lblStockOrigen, 0, 5);
            tlpMain.Controls.Add(lblStockOrigenValor, 1, 5);
            tlpMain.Controls.Add(lblCantidad, 0, 6);
            tlpMain.Controls.Add(numCantidad, 1, 6);
            tlpMain.Controls.Add(lblObservacion, 0, 7);
            tlpMain.Controls.Add(txtObservacion, 1, 7);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 44);
            tlpMain.Name = "tlpMain";
            tlpMain.Padding = new Padding(14, 10, 14, 4);
            tlpMain.RowCount = 8;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.Size = new Size(500, 390);
            tlpMain.TabIndex = 1;
            // 
            // lblProducto
            // 
            lblProducto.Dock = DockStyle.Fill;
            lblProducto.Font = new Font("Segoe UI", 9F);
            lblProducto.ForeColor = Color.FromArgb(96, 94, 92);
            lblProducto.Location = new Point(17, 10);
            lblProducto.Name = "lblProducto";
            lblProducto.Size = new Size(124, 32);
            lblProducto.TabIndex = 0;
            lblProducto.Text = "Producto:";
            lblProducto.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblProductoValor
            // 
            lblProductoValor.Dock = DockStyle.Fill;
            lblProductoValor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblProductoValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblProductoValor.Location = new Point(147, 10);
            lblProductoValor.Name = "lblProductoValor";
            lblProductoValor.Size = new Size(336, 32);
            lblProductoValor.TabIndex = 1;
            lblProductoValor.Text = "-";
            lblProductoValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblOrigen
            // 
            lblOrigen.Dock = DockStyle.Fill;
            lblOrigen.Font = new Font("Segoe UI", 9F);
            lblOrigen.ForeColor = Color.FromArgb(96, 94, 92);
            lblOrigen.Location = new Point(17, 42);
            lblOrigen.Name = "lblOrigen";
            lblOrigen.Size = new Size(124, 38);
            lblOrigen.TabIndex = 2;
            lblOrigen.Text = "Bodega Origen:";
            lblOrigen.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboOrigen
            // 
            cboOrigen.Dock = DockStyle.Fill;
            cboOrigen.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOrigen.Font = new Font("Segoe UI", 9F);
            cboOrigen.FormattingEnabled = true;
            cboOrigen.Location = new Point(147, 45);
            cboOrigen.Name = "cboOrigen";
            cboOrigen.Size = new Size(336, 23);
            cboOrigen.TabIndex = 1;
            // 
            // lblDestino
            // 
            lblDestino.Dock = DockStyle.Fill;
            lblDestino.Font = new Font("Segoe UI", 9F);
            lblDestino.ForeColor = Color.FromArgb(96, 94, 92);
            lblDestino.Location = new Point(17, 80);
            lblDestino.Name = "lblDestino";
            lblDestino.Size = new Size(124, 38);
            lblDestino.TabIndex = 3;
            lblDestino.Text = "Bodega Destino:";
            lblDestino.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboDestino
            // 
            cboDestino.Dock = DockStyle.Fill;
            cboDestino.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDestino.Font = new Font("Segoe UI", 9F);
            cboDestino.FormattingEnabled = true;
            cboDestino.Location = new Point(147, 83);
            cboDestino.Name = "cboDestino";
            cboDestino.Size = new Size(336, 23);
            cboDestino.TabIndex = 2;
            // 
            // lblPresentacion
            // 
            lblPresentacion.Dock = DockStyle.Fill;
            lblPresentacion.Font = new Font("Segoe UI", 9F);
            lblPresentacion.ForeColor = Color.FromArgb(96, 94, 92);
            lblPresentacion.Location = new Point(17, 118);
            lblPresentacion.Name = "lblPresentacion";
            lblPresentacion.Size = new Size(124, 38);
            lblPresentacion.TabIndex = 4;
            lblPresentacion.Text = "Presentacion:";
            lblPresentacion.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboPresentacion
            // 
            cboPresentacion.Dock = DockStyle.Fill;
            cboPresentacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPresentacion.Font = new Font("Segoe UI", 9F);
            cboPresentacion.FormattingEnabled = true;
            cboPresentacion.Location = new Point(147, 121);
            cboPresentacion.Name = "cboPresentacion";
            cboPresentacion.Size = new Size(336, 23);
            cboPresentacion.TabIndex = 3;
            // 
            // lblLote
            // 
            lblLote.Dock = DockStyle.Fill;
            lblLote.Font = new Font("Segoe UI", 9F);
            lblLote.ForeColor = Color.FromArgb(96, 94, 92);
            lblLote.Location = new Point(17, 156);
            lblLote.Name = "lblLote";
            lblLote.Size = new Size(124, 38);
            lblLote.TabIndex = 5;
            lblLote.Text = "Lote:";
            lblLote.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboLote
            // 
            cboLote.Dock = DockStyle.Fill;
            cboLote.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLote.Font = new Font("Segoe UI", 9F);
            cboLote.FormattingEnabled = true;
            cboLote.Location = new Point(147, 159);
            cboLote.Name = "cboLote";
            cboLote.Size = new Size(336, 23);
            cboLote.TabIndex = 4;
            // 
            // lblStockOrigen
            // 
            lblStockOrigen.Dock = DockStyle.Fill;
            lblStockOrigen.Font = new Font("Segoe UI", 8.5F);
            lblStockOrigen.ForeColor = Color.FromArgb(96, 94, 92);
            lblStockOrigen.Location = new Point(17, 194);
            lblStockOrigen.Name = "lblStockOrigen";
            lblStockOrigen.Size = new Size(124, 30);
            lblStockOrigen.TabIndex = 6;
            lblStockOrigen.Text = "Disp. en origen:";
            lblStockOrigen.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblStockOrigenValor
            // 
            lblStockOrigenValor.Dock = DockStyle.Fill;
            lblStockOrigenValor.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblStockOrigenValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblStockOrigenValor.Location = new Point(147, 194);
            lblStockOrigenValor.Name = "lblStockOrigenValor";
            lblStockOrigenValor.Size = new Size(336, 30);
            lblStockOrigenValor.TabIndex = 7;
            lblStockOrigenValor.Text = "-";
            lblStockOrigenValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblCantidad
            // 
            lblCantidad.Dock = DockStyle.Fill;
            lblCantidad.Font = new Font("Segoe UI", 9F);
            lblCantidad.ForeColor = Color.FromArgb(96, 94, 92);
            lblCantidad.Location = new Point(17, 224);
            lblCantidad.Name = "lblCantidad";
            lblCantidad.Size = new Size(124, 38);
            lblCantidad.TabIndex = 8;
            lblCantidad.Text = "Cantidad:";
            lblCantidad.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numCantidad
            // 
            numCantidad.DecimalPlaces = 4;
            numCantidad.Dock = DockStyle.Fill;
            numCantidad.Font = new Font("Segoe UI", 9F);
            numCantidad.Location = new Point(147, 227);
            numCantidad.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numCantidad.Name = "numCantidad";
            numCantidad.Size = new Size(336, 23);
            numCantidad.TabIndex = 5;
            numCantidad.ThousandsSeparator = true;
            // 
            // lblObservacion
            // 
            lblObservacion.Dock = DockStyle.Fill;
            lblObservacion.Font = new Font("Segoe UI", 9F);
            lblObservacion.ForeColor = Color.FromArgb(96, 94, 92);
            lblObservacion.Location = new Point(17, 262);
            lblObservacion.Name = "lblObservacion";
            lblObservacion.Padding = new Padding(0, 6, 0, 0);
            lblObservacion.Size = new Size(124, 124);
            lblObservacion.TabIndex = 9;
            lblObservacion.Text = "Observacion:";
            lblObservacion.TextAlign = ContentAlignment.TopRight;
            // 
            // txtObservacion
            // 
            txtObservacion.Dock = DockStyle.Fill;
            txtObservacion.Font = new Font("Segoe UI", 9F);
            txtObservacion.Location = new Point(147, 265);
            txtObservacion.MaxLength = 500;
            txtObservacion.Multiline = true;
            txtObservacion.Name = "txtObservacion";
            txtObservacion.ScrollBars = ScrollBars.Vertical;
            txtObservacion.Size = new Size(336, 118);
            txtObservacion.TabIndex = 6;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(243, 242, 241);
            panelBotones.Controls.Add(flpBotones);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 434);
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
            btnGuardar.Text = "Transferir";
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
            // frm_StockTransferir
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 490);
            Controls.Add(tlpMain);
            Controls.Add(panelBotones);
            Controls.Add(panelHeader);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frm_StockTransferir";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Transferencia de Stock";
            Load += Frm_StockTransferir_Load;
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