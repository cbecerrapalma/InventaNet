namespace Inventa.UC.Producto
{
    partial class ProdStockUC
    {
        private System.ComponentModel.IContainer components = null;

        // ── Layout Principal ──────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpMain;

        // ── Panel Superior: Filtros + Grilla ───────────────────────────────────
        private System.Windows.Forms.Panel pnlSuperior;
        private System.Windows.Forms.Panel pnlFiltros;
        private System.Windows.Forms.Label lblBodega;
        private System.Windows.Forms.ComboBox cboBodega;
        private System.Windows.Forms.Label lblVariante;
        private System.Windows.Forms.ComboBox cboVariante;
        private System.Windows.Forms.CheckBox chkSoloConStock;
        private System.Windows.Forms.CheckBox chkIncluirLotes;
        private System.Windows.Forms.ToolStrip tsStock;
        private System.Windows.Forms.ToolStripButton tsbAjustar;
        private System.Windows.Forms.ToolStripButton tsbTransferir;
        private System.Windows.Forms.ToolStripButton tsbVerMovimientos;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripLabel tslInfo;
        private System.Windows.Forms.DataGridView dgvStock;

        // ── Panel de Totales (Footer de grilla) ────────────────────────────────
        private System.Windows.Forms.Panel pnlTotales;
        private System.Windows.Forms.Label lblTotalUnidades;
        private System.Windows.Forms.Label lblTotalValorizado;
        private System.Windows.Forms.Label lblTotalBodegas;

        // ── Splitter ───────────────────────────────────────────────────────────
        private System.Windows.Forms.Splitter splitter;

        // ── Panel Inferior: Acciones de Stock ──────────────────────────────────
        private System.Windows.Forms.Panel pnlAccionesStock;
        private System.Windows.Forms.TabControl tabAcciones;

        // ── Tab: Ajuste de Inventario ──────────────────────────────────────────
        private System.Windows.Forms.TabPage tabAjuste;
        private System.Windows.Forms.TableLayoutPanel tlpAjuste;
        private System.Windows.Forms.Label lblTipoAjuste;
        private System.Windows.Forms.ComboBox cboTipoAjuste;
        private System.Windows.Forms.Label lblCantidadAjuste;
        private System.Windows.Forms.NumericUpDown numCantidadAjuste;
        private System.Windows.Forms.Label lblMotivoAjuste;
        private System.Windows.Forms.TextBox txtMotivoAjuste;
        private System.Windows.Forms.Label lblCostoAjuste;
        private System.Windows.Forms.NumericUpDown numCostoAjuste;
        private System.Windows.Forms.Label lblBodegaAjuste;
        private System.Windows.Forms.ComboBox cboBodegaAjuste;
        private System.Windows.Forms.Label lblLoteAjuste;
        private System.Windows.Forms.ComboBox cboLoteAjuste;
        private System.Windows.Forms.Button btnEjecutarAjuste;

        // ── Tab: Transferencia ─────────────────────────────────────────────────
        private System.Windows.Forms.TabPage tabTransferencia;
        private System.Windows.Forms.TableLayoutPanel tlpTransferencia;
        private System.Windows.Forms.Label lblBodegaOrigen;
        private System.Windows.Forms.ComboBox cboBodegaOrigen;
        private System.Windows.Forms.Label lblBodegaDestino;
        private System.Windows.Forms.ComboBox cboBodegaDestino;
        private System.Windows.Forms.Label lblCantidadTransferencia;
        private System.Windows.Forms.NumericUpDown numCantidadTransferencia;
        private System.Windows.Forms.Label lblLoteTransferencia;
        private System.Windows.Forms.ComboBox cboLoteTransferencia;
        private System.Windows.Forms.Button btnEjecutarTransferencia;

        // ── Panel de Estado de Acción ─────────────────────────────────────────
        private System.Windows.Forms.Panel pnlEstadoAccion;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblEstadoOperacion;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            tlpMain = new TableLayoutPanel();
            pnlSuperior = new Panel();
            pnlTotales = new Panel();
            lblTotalBodegas = new Label();
            lblTotalValorizado = new Label();
            lblTotalUnidades = new Label();
            dgvStock = new DataGridView();
            tsStock = new ToolStrip();
            tsbAjustar = new ToolStripButton();
            tsbTransferir = new ToolStripButton();
            tsbVerMovimientos = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            tslInfo = new ToolStripLabel();
            pnlFiltros = new Panel();
            chkIncluirLotes = new CheckBox();
            chkSoloConStock = new CheckBox();
            cboVariante = new ComboBox();
            lblVariante = new Label();
            cboBodega = new ComboBox();
            lblBodega = new Label();
            splitter = new Splitter();
            pnlAccionesStock = new Panel();
            pnlEstadoAccion = new Panel();
            lblEstadoOperacion = new Label();
            progressBar = new ProgressBar();
            tabAcciones = new TabControl();
            tabAjuste = new TabPage();
            tlpAjuste = new TableLayoutPanel();
            lblTipoAjuste = new Label();
            cboTipoAjuste = new ComboBox();
            lblBodegaAjuste = new Label();
            cboBodegaAjuste = new ComboBox();
            lblLoteAjuste = new Label();
            cboLoteAjuste = new ComboBox();
            lblCantidadAjuste = new Label();
            numCantidadAjuste = new NumericUpDown();
            lblCostoAjuste = new Label();
            numCostoAjuste = new NumericUpDown();
            lblMotivoAjuste = new Label();
            txtMotivoAjuste = new TextBox();
            btnEjecutarAjuste = new Button();
            tabTransferencia = new TabPage();
            tlpTransferencia = new TableLayoutPanel();
            lblBodegaOrigen = new Label();
            cboBodegaOrigen = new ComboBox();
            lblBodegaDestino = new Label();
            cboBodegaDestino = new ComboBox();
            lblLoteTransferencia = new Label();
            cboLoteTransferencia = new ComboBox();
            lblCantidadTransferencia = new Label();
            numCantidadTransferencia = new NumericUpDown();
            btnEjecutarTransferencia = new Button();
            tlpMain.SuspendLayout();
            pnlSuperior.SuspendLayout();
            pnlTotales.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStock).BeginInit();
            tsStock.SuspendLayout();
            pnlFiltros.SuspendLayout();
            pnlAccionesStock.SuspendLayout();
            pnlEstadoAccion.SuspendLayout();
            tabAcciones.SuspendLayout();
            tabAjuste.SuspendLayout();
            tlpAjuste.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidadAjuste).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCostoAjuste).BeginInit();
            tabTransferencia.SuspendLayout();
            tlpTransferencia.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidadTransferencia).BeginInit();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(pnlSuperior, 0, 0);
            tlpMain.Controls.Add(splitter, 0, 1);
            tlpMain.Controls.Add(pnlAccionesStock, 0, 2);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 6F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            tlpMain.Size = new Size(1024, 600);
            tlpMain.TabIndex = 0;
            // 
            // pnlSuperior
            // 
            pnlSuperior.Controls.Add(pnlTotales);
            pnlSuperior.Controls.Add(dgvStock);
            pnlSuperior.Controls.Add(tsStock);
            pnlSuperior.Controls.Add(pnlFiltros);
            pnlSuperior.Dock = DockStyle.Fill;
            pnlSuperior.Location = new Point(3, 3);
            pnlSuperior.Name = "pnlSuperior";
            pnlSuperior.Padding = new Padding(8);
            pnlSuperior.Size = new Size(1018, 350);
            pnlSuperior.TabIndex = 0;
            // 
            // pnlTotales
            // 
            pnlTotales.BackColor = Color.FromArgb(243, 242, 241);
            pnlTotales.BorderStyle = BorderStyle.FixedSingle;
            pnlTotales.Controls.Add(lblTotalBodegas);
            pnlTotales.Controls.Add(lblTotalValorizado);
            pnlTotales.Controls.Add(lblTotalUnidades);
            pnlTotales.Dock = DockStyle.Bottom;
            pnlTotales.Location = new Point(8, 298);
            pnlTotales.Name = "pnlTotales";
            pnlTotales.Padding = new Padding(12, 4, 12, 4);
            pnlTotales.Size = new Size(1002, 44);
            pnlTotales.TabIndex = 3;
            // 
            // lblTotalBodegas
            // 
            lblTotalBodegas.Dock = DockStyle.Right;
            lblTotalBodegas.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblTotalBodegas.ForeColor = Color.FromArgb(150, 150, 150);
            lblTotalBodegas.Location = new Point(700, 4);
            lblTotalBodegas.Name = "lblTotalBodegas";
            lblTotalBodegas.Size = new Size(288, 34);
            lblTotalBodegas.TabIndex = 2;
            lblTotalBodegas.Text = "🏭 En X bodegas";
            lblTotalBodegas.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTotalValorizado
            // 
            lblTotalValorizado.Dock = DockStyle.Left;
            lblTotalValorizado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTotalValorizado.ForeColor = Color.FromArgb(46, 204, 113);
            lblTotalValorizado.Location = new Point(212, 4);
            lblTotalValorizado.Name = "lblTotalValorizado";
            lblTotalValorizado.Size = new Size(250, 34);
            lblTotalValorizado.TabIndex = 1;
            lblTotalValorizado.Text = "💰 Valorizado: $0.00";
            lblTotalValorizado.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTotalUnidades
            // 
            lblTotalUnidades.Dock = DockStyle.Left;
            lblTotalUnidades.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTotalUnidades.ForeColor = Color.FromArgb(52, 152, 219);
            lblTotalUnidades.Location = new Point(12, 4);
            lblTotalUnidades.Name = "lblTotalUnidades";
            lblTotalUnidades.Size = new Size(200, 34);
            lblTotalUnidades.TabIndex = 0;
            lblTotalUnidades.Text = "📦 Total: 0 unidades";
            lblTotalUnidades.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dgvStock
            // 
            dgvStock.AllowUserToAddRows = false;
            dgvStock.AllowUserToDeleteRows = false;
            dgvStock.BackgroundColor = Color.White;
            dgvStock.BorderStyle = BorderStyle.None;
            dgvStock.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvStock.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvStock.ColumnHeadersHeight = 36;
            dgvStock.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvStock.Dock = DockStyle.Fill;
            dgvStock.EnableHeadersVisualStyles = false;
            dgvStock.GridColor = Color.FromArgb(237, 235, 233);
            dgvStock.Location = new Point(8, 79);
            dgvStock.MultiSelect = false;
            dgvStock.Name = "dgvStock";
            dgvStock.ReadOnly = true;
            dgvStock.RowHeadersVisible = false;
            dgvStock.RowTemplate.Height = 36;
            dgvStock.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvStock.Size = new Size(1002, 263);
            dgvStock.TabIndex = 2;
            // 
            // tsStock
            // 
            tsStock.GripStyle = ToolStripGripStyle.Hidden;
            tsStock.Items.AddRange(new ToolStripItem[] { tsbAjustar, tsbTransferir, tsbVerMovimientos, tsSep1, tslInfo });
            tsStock.Location = new Point(8, 48);
            tsStock.Name = "tsStock";
            tsStock.Padding = new Padding(4);
            tsStock.Size = new Size(1002, 31);
            tsStock.TabIndex = 1;
            // 
            // tsbAjustar
            // 
            tsbAjustar.Image = Properties.Resources.add;
            tsbAjustar.ImageTransparentColor = Color.Magenta;
            tsbAjustar.Name = "tsbAjustar";
            tsbAjustar.Size = new Size(79, 20);
            tsbAjustar.Text = "⚖️ Ajustar";
            tsbAjustar.ToolTipText = "Ajustar inventario (entrada/salida)";
            // 
            // tsbTransferir
            // 
            tsbTransferir.Image = Properties.Resources.add;
            tsbTransferir.ImageTransparentColor = Color.Magenta;
            tsbTransferir.Name = "tsbTransferir";
            tsbTransferir.Size = new Size(90, 20);
            tsbTransferir.Text = "🚚 Transferir";
            tsbTransferir.ToolTipText = "Transferir stock entre bodegas";
            // 
            // tsbVerMovimientos
            // 
            tsbVerMovimientos.Image = Properties.Resources.add;
            tsbVerMovimientos.ImageTransparentColor = Color.Magenta;
            tsbVerMovimientos.Name = "tsbVerMovimientos";
            tsbVerMovimientos.Size = new Size(112, 20);
            tsbVerMovimientos.Text = "📋 Movimientos";
            tsbVerMovimientos.ToolTipText = "Ver kardex de movimientos";
            // 
            // tsSep1
            // 
            tsSep1.Name = "tsSep1";
            tsSep1.Size = new Size(6, 23);
            // 
            // tslInfo
            // 
            tslInfo.Alignment = ToolStripItemAlignment.Right;
            tslInfo.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            tslInfo.ForeColor = Color.FromArgb(96, 94, 92);
            tslInfo.Name = "tslInfo";
            tslInfo.Size = new Size(105, 20);
            tslInfo.Text = "0 registros de stock";
            tslInfo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(chkIncluirLotes);
            pnlFiltros.Controls.Add(chkSoloConStock);
            pnlFiltros.Controls.Add(cboVariante);
            pnlFiltros.Controls.Add(lblVariante);
            pnlFiltros.Controls.Add(cboBodega);
            pnlFiltros.Controls.Add(lblBodega);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(8, 8);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Padding = new Padding(0, 0, 0, 8);
            pnlFiltros.Size = new Size(1002, 40);
            pnlFiltros.TabIndex = 0;
            pnlFiltros.Paint += pnlFiltros_Paint;
            // 
            // chkIncluirLotes
            // 
            chkIncluirLotes.AutoSize = true;
            chkIncluirLotes.Dock = DockStyle.Left;
            chkIncluirLotes.Font = new Font("Segoe UI", 9F);
            chkIncluirLotes.Location = new Point(675, 0);
            chkIncluirLotes.Margin = new Padding(12, 3, 3, 3);
            chkIncluirLotes.Name = "chkIncluirLotes";
            chkIncluirLotes.Size = new Size(87, 32);
            chkIncluirLotes.TabIndex = 5;
            chkIncluirLotes.Text = "Incluir lotes";
            chkIncluirLotes.UseVisualStyleBackColor = true;
            // 
            // chkSoloConStock
            // 
            chkSoloConStock.AutoSize = true;
            chkSoloConStock.Checked = true;
            chkSoloConStock.CheckState = CheckState.Checked;
            chkSoloConStock.Dock = DockStyle.Left;
            chkSoloConStock.Font = new Font("Segoe UI", 9F);
            chkSoloConStock.Location = new Point(540, 0);
            chkSoloConStock.Margin = new Padding(12, 3, 3, 3);
            chkSoloConStock.Name = "chkSoloConStock";
            chkSoloConStock.Size = new Size(135, 32);
            chkSoloConStock.TabIndex = 4;
            chkSoloConStock.Text = "Solo items con stock";
            chkSoloConStock.UseVisualStyleBackColor = true;
            // 
            // cboVariante
            // 
            cboVariante.Dock = DockStyle.Left;
            cboVariante.DropDownStyle = ComboBoxStyle.DropDownList;
            cboVariante.Font = new Font("Segoe UI", 9F);
            cboVariante.FormattingEnabled = true;
            cboVariante.Items.AddRange(new object[] { "Todas las variantes" });
            cboVariante.Location = new Point(320, 0);
            cboVariante.Name = "cboVariante";
            cboVariante.Size = new Size(220, 23);
            cboVariante.TabIndex = 3;
            // 
            // lblVariante
            // 
            lblVariante.Dock = DockStyle.Left;
            lblVariante.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblVariante.ForeColor = Color.FromArgb(50, 49, 48);
            lblVariante.Location = new Point(250, 0);
            lblVariante.Margin = new Padding(12, 0, 0, 0);
            lblVariante.Name = "lblVariante";
            lblVariante.Size = new Size(70, 32);
            lblVariante.TabIndex = 2;
            lblVariante.Text = "🎨 Variante:";
            lblVariante.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboBodega
            // 
            cboBodega.Dock = DockStyle.Left;
            cboBodega.DropDownStyle = ComboBoxStyle.DropDownList;
            cboBodega.Font = new Font("Segoe UI", 9F);
            cboBodega.FormattingEnabled = true;
            cboBodega.Items.AddRange(new object[] { "Todas las bodegas" });
            cboBodega.Location = new Point(70, 0);
            cboBodega.Name = "cboBodega";
            cboBodega.Size = new Size(180, 23);
            cboBodega.TabIndex = 1;
            // 
            // lblBodega
            // 
            lblBodega.Dock = DockStyle.Left;
            lblBodega.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblBodega.ForeColor = Color.FromArgb(50, 49, 48);
            lblBodega.Location = new Point(0, 0);
            lblBodega.Name = "lblBodega";
            lblBodega.Size = new Size(70, 32);
            lblBodega.TabIndex = 0;
            lblBodega.Text = "🏭 Bodega:";
            lblBodega.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // splitter
            // 
            splitter.BackColor = Color.FromArgb(200, 200, 200);
            splitter.Location = new Point(3, 359);
            splitter.Name = "splitter";
            splitter.Size = new Size(1018, 1);
            splitter.TabIndex = 1;
            splitter.TabStop = false;
            // 
            // pnlAccionesStock
            // 
            pnlAccionesStock.Controls.Add(pnlEstadoAccion);
            pnlAccionesStock.Controls.Add(tabAcciones);
            pnlAccionesStock.Dock = DockStyle.Fill;
            pnlAccionesStock.Location = new Point(3, 365);
            pnlAccionesStock.Name = "pnlAccionesStock";
            pnlAccionesStock.Padding = new Padding(8);
            pnlAccionesStock.Size = new Size(1018, 232);
            pnlAccionesStock.TabIndex = 2;
            // 
            // pnlEstadoAccion
            // 
            pnlEstadoAccion.Controls.Add(lblEstadoOperacion);
            pnlEstadoAccion.Controls.Add(progressBar);
            pnlEstadoAccion.Dock = DockStyle.Bottom;
            pnlEstadoAccion.Location = new Point(8, 196);
            pnlEstadoAccion.Name = "pnlEstadoAccion";
            pnlEstadoAccion.Padding = new Padding(0, 4, 0, 0);
            pnlEstadoAccion.Size = new Size(1002, 28);
            pnlEstadoAccion.TabIndex = 1;
            pnlEstadoAccion.Visible = false;
            // 
            // lblEstadoOperacion
            // 
            lblEstadoOperacion.Dock = DockStyle.Fill;
            lblEstadoOperacion.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblEstadoOperacion.ForeColor = Color.FromArgb(52, 152, 219);
            lblEstadoOperacion.Location = new Point(200, 4);
            lblEstadoOperacion.Name = "lblEstadoOperacion";
            lblEstadoOperacion.Size = new Size(802, 24);
            lblEstadoOperacion.TabIndex = 1;
            lblEstadoOperacion.Text = "Procesando operación...";
            lblEstadoOperacion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            progressBar.Dock = DockStyle.Left;
            progressBar.Location = new Point(0, 4);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(200, 24);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 2;
            // 
            // tabAcciones
            // 
            tabAcciones.Appearance = TabAppearance.FlatButtons;
            tabAcciones.Controls.Add(tabAjuste);
            tabAcciones.Controls.Add(tabTransferencia);
            tabAcciones.Dock = DockStyle.Fill;
            tabAcciones.Location = new Point(8, 8);
            tabAcciones.Name = "tabAcciones";
            tabAcciones.SelectedIndex = 0;
            tabAcciones.Size = new Size(1002, 216);
            tabAcciones.TabIndex = 0;
            // 
            // tabAjuste
            // 
            tabAjuste.BackColor = Color.FromArgb(250, 250, 250);
            tabAjuste.Controls.Add(tlpAjuste);
            tabAjuste.Location = new Point(4, 27);
            tabAjuste.Name = "tabAjuste";
            tabAjuste.Padding = new Padding(12);
            tabAjuste.Size = new Size(994, 185);
            tabAjuste.TabIndex = 0;
            tabAjuste.Text = "⚖️ Ajuste de Inventario";
            // 
            // tlpAjuste
            // 
            tlpAjuste.ColumnCount = 6;
            tlpAjuste.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpAjuste.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpAjuste.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpAjuste.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpAjuste.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpAjuste.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpAjuste.Controls.Add(lblTipoAjuste, 0, 0);
            tlpAjuste.Controls.Add(cboTipoAjuste, 1, 0);
            tlpAjuste.Controls.Add(lblBodegaAjuste, 2, 0);
            tlpAjuste.Controls.Add(cboBodegaAjuste, 3, 0);
            tlpAjuste.Controls.Add(lblLoteAjuste, 4, 0);
            tlpAjuste.Controls.Add(cboLoteAjuste, 5, 0);
            tlpAjuste.Controls.Add(lblCantidadAjuste, 0, 1);
            tlpAjuste.Controls.Add(numCantidadAjuste, 1, 1);
            tlpAjuste.Controls.Add(lblCostoAjuste, 2, 1);
            tlpAjuste.Controls.Add(numCostoAjuste, 3, 1);
            tlpAjuste.Controls.Add(lblMotivoAjuste, 4, 1);
            tlpAjuste.Controls.Add(txtMotivoAjuste, 5, 1);
            tlpAjuste.Controls.Add(btnEjecutarAjuste, 5, 2);
            tlpAjuste.Dock = DockStyle.Fill;
            tlpAjuste.Location = new Point(12, 12);
            tlpAjuste.Name = "tlpAjuste";
            tlpAjuste.RowCount = 3;
            tlpAjuste.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpAjuste.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpAjuste.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tlpAjuste.Size = new Size(970, 161);
            tlpAjuste.TabIndex = 0;
            // 
            // lblTipoAjuste
            // 
            lblTipoAjuste.Dock = DockStyle.Fill;
            lblTipoAjuste.Font = new Font("Segoe UI", 9F);
            lblTipoAjuste.ForeColor = Color.FromArgb(96, 94, 92);
            lblTipoAjuste.Location = new Point(3, 0);
            lblTipoAjuste.Name = "lblTipoAjuste";
            lblTipoAjuste.Size = new Size(94, 36);
            lblTipoAjuste.TabIndex = 0;
            lblTipoAjuste.Text = "Tipo:";
            lblTipoAjuste.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboTipoAjuste
            // 
            cboTipoAjuste.Dock = DockStyle.Fill;
            cboTipoAjuste.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoAjuste.Font = new Font("Segoe UI", 9F);
            cboTipoAjuste.FormattingEnabled = true;
            cboTipoAjuste.Items.AddRange(new object[] { "Entrada (+)", "Salida (-)" });
            cboTipoAjuste.Location = new Point(103, 3);
            cboTipoAjuste.Name = "cboTipoAjuste";
            cboTipoAjuste.Size = new Size(217, 23);
            cboTipoAjuste.TabIndex = 1;
            // 
            // lblBodegaAjuste
            // 
            lblBodegaAjuste.Dock = DockStyle.Fill;
            lblBodegaAjuste.Font = new Font("Segoe UI", 9F);
            lblBodegaAjuste.ForeColor = Color.FromArgb(96, 94, 92);
            lblBodegaAjuste.Location = new Point(326, 0);
            lblBodegaAjuste.Name = "lblBodegaAjuste";
            lblBodegaAjuste.Size = new Size(94, 36);
            lblBodegaAjuste.TabIndex = 2;
            lblBodegaAjuste.Text = "Bodega:";
            lblBodegaAjuste.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboBodegaAjuste
            // 
            cboBodegaAjuste.Dock = DockStyle.Fill;
            cboBodegaAjuste.DropDownStyle = ComboBoxStyle.DropDownList;
            cboBodegaAjuste.Font = new Font("Segoe UI", 9F);
            cboBodegaAjuste.FormattingEnabled = true;
            cboBodegaAjuste.Location = new Point(426, 3);
            cboBodegaAjuste.Name = "cboBodegaAjuste";
            cboBodegaAjuste.Size = new Size(217, 23);
            cboBodegaAjuste.TabIndex = 3;
            // 
            // lblLoteAjuste
            // 
            lblLoteAjuste.Dock = DockStyle.Fill;
            lblLoteAjuste.Font = new Font("Segoe UI", 9F);
            lblLoteAjuste.ForeColor = Color.FromArgb(96, 94, 92);
            lblLoteAjuste.Location = new Point(649, 0);
            lblLoteAjuste.Name = "lblLoteAjuste";
            lblLoteAjuste.Size = new Size(94, 36);
            lblLoteAjuste.TabIndex = 4;
            lblLoteAjuste.Text = "Lote:";
            lblLoteAjuste.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboLoteAjuste
            // 
            cboLoteAjuste.Dock = DockStyle.Fill;
            cboLoteAjuste.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLoteAjuste.Enabled = false;
            cboLoteAjuste.Font = new Font("Segoe UI", 9F);
            cboLoteAjuste.FormattingEnabled = true;
            cboLoteAjuste.Location = new Point(749, 3);
            cboLoteAjuste.Name = "cboLoteAjuste";
            cboLoteAjuste.Size = new Size(218, 23);
            cboLoteAjuste.TabIndex = 5;
            // 
            // lblCantidadAjuste
            // 
            lblCantidadAjuste.Dock = DockStyle.Fill;
            lblCantidadAjuste.Font = new Font("Segoe UI", 9F);
            lblCantidadAjuste.ForeColor = Color.FromArgb(96, 94, 92);
            lblCantidadAjuste.Location = new Point(3, 36);
            lblCantidadAjuste.Name = "lblCantidadAjuste";
            lblCantidadAjuste.Size = new Size(94, 36);
            lblCantidadAjuste.TabIndex = 6;
            lblCantidadAjuste.Text = "Cantidad:";
            lblCantidadAjuste.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numCantidadAjuste
            // 
            numCantidadAjuste.DecimalPlaces = 4;
            numCantidadAjuste.Dock = DockStyle.Fill;
            numCantidadAjuste.Font = new Font("Segoe UI", 9F);
            numCantidadAjuste.Location = new Point(103, 39);
            numCantidadAjuste.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numCantidadAjuste.Name = "numCantidadAjuste";
            numCantidadAjuste.Size = new Size(217, 23);
            numCantidadAjuste.TabIndex = 7;
            numCantidadAjuste.TextAlign = HorizontalAlignment.Right;
            numCantidadAjuste.ThousandsSeparator = true;
            // 
            // lblCostoAjuste
            // 
            lblCostoAjuste.Dock = DockStyle.Fill;
            lblCostoAjuste.Font = new Font("Segoe UI", 9F);
            lblCostoAjuste.ForeColor = Color.FromArgb(96, 94, 92);
            lblCostoAjuste.Location = new Point(326, 36);
            lblCostoAjuste.Name = "lblCostoAjuste";
            lblCostoAjuste.Size = new Size(94, 36);
            lblCostoAjuste.TabIndex = 8;
            lblCostoAjuste.Text = "Costo Unit.:";
            lblCostoAjuste.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numCostoAjuste
            // 
            numCostoAjuste.DecimalPlaces = 4;
            numCostoAjuste.Dock = DockStyle.Fill;
            numCostoAjuste.Font = new Font("Segoe UI", 9F);
            numCostoAjuste.Location = new Point(426, 39);
            numCostoAjuste.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numCostoAjuste.Name = "numCostoAjuste";
            numCostoAjuste.Size = new Size(217, 23);
            numCostoAjuste.TabIndex = 9;
            numCostoAjuste.TextAlign = HorizontalAlignment.Right;
            numCostoAjuste.ThousandsSeparator = true;
            // 
            // lblMotivoAjuste
            // 
            lblMotivoAjuste.Dock = DockStyle.Fill;
            lblMotivoAjuste.Font = new Font("Segoe UI", 9F);
            lblMotivoAjuste.ForeColor = Color.FromArgb(96, 94, 92);
            lblMotivoAjuste.Location = new Point(649, 36);
            lblMotivoAjuste.Name = "lblMotivoAjuste";
            lblMotivoAjuste.Size = new Size(94, 36);
            lblMotivoAjuste.TabIndex = 10;
            lblMotivoAjuste.Text = "Motivo:";
            lblMotivoAjuste.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtMotivoAjuste
            // 
            txtMotivoAjuste.Dock = DockStyle.Fill;
            txtMotivoAjuste.Font = new Font("Segoe UI", 9F);
            txtMotivoAjuste.Location = new Point(749, 39);
            txtMotivoAjuste.MaxLength = 200;
            txtMotivoAjuste.Name = "txtMotivoAjuste";
            txtMotivoAjuste.PlaceholderText = "Motivo del ajuste...";
            txtMotivoAjuste.Size = new Size(218, 23);
            txtMotivoAjuste.TabIndex = 11;
            // 
            // btnEjecutarAjuste
            // 
            btnEjecutarAjuste.BackColor = Color.FromArgb(230, 126, 34);
            btnEjecutarAjuste.Dock = DockStyle.Right;
            btnEjecutarAjuste.FlatAppearance.BorderSize = 0;
            btnEjecutarAjuste.FlatStyle = FlatStyle.Flat;
            btnEjecutarAjuste.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnEjecutarAjuste.ForeColor = Color.White;
            btnEjecutarAjuste.Location = new Point(847, 78);
            btnEjecutarAjuste.Margin = new Padding(3, 6, 3, 3);
            btnEjecutarAjuste.Name = "btnEjecutarAjuste";
            btnEjecutarAjuste.Size = new Size(120, 80);
            btnEjecutarAjuste.TabIndex = 12;
            btnEjecutarAjuste.Text = "⚡ Ejecutar Ajuste";
            btnEjecutarAjuste.UseVisualStyleBackColor = false;
            // 
            // tabTransferencia
            // 
            tabTransferencia.BackColor = Color.FromArgb(250, 250, 250);
            tabTransferencia.Controls.Add(tlpTransferencia);
            tabTransferencia.Location = new Point(4, 27);
            tabTransferencia.Name = "tabTransferencia";
            tabTransferencia.Padding = new Padding(12);
            tabTransferencia.Size = new Size(994, 185);
            tabTransferencia.TabIndex = 1;
            tabTransferencia.Text = "🚚 Transferencia";
            // 
            // tlpTransferencia
            // 
            tlpTransferencia.ColumnCount = 6;
            tlpTransferencia.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpTransferencia.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpTransferencia.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpTransferencia.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpTransferencia.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpTransferencia.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpTransferencia.Controls.Add(lblBodegaOrigen, 0, 0);
            tlpTransferencia.Controls.Add(cboBodegaOrigen, 1, 0);
            tlpTransferencia.Controls.Add(lblBodegaDestino, 2, 0);
            tlpTransferencia.Controls.Add(cboBodegaDestino, 3, 0);
            tlpTransferencia.Controls.Add(lblLoteTransferencia, 4, 0);
            tlpTransferencia.Controls.Add(cboLoteTransferencia, 5, 0);
            tlpTransferencia.Controls.Add(lblCantidadTransferencia, 0, 1);
            tlpTransferencia.Controls.Add(numCantidadTransferencia, 1, 1);
            tlpTransferencia.Controls.Add(btnEjecutarTransferencia, 5, 2);
            tlpTransferencia.Dock = DockStyle.Fill;
            tlpTransferencia.Location = new Point(12, 12);
            tlpTransferencia.Name = "tlpTransferencia";
            tlpTransferencia.RowCount = 3;
            tlpTransferencia.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpTransferencia.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpTransferencia.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tlpTransferencia.Size = new Size(970, 161);
            tlpTransferencia.TabIndex = 0;
            // 
            // lblBodegaOrigen
            // 
            lblBodegaOrigen.Dock = DockStyle.Fill;
            lblBodegaOrigen.Font = new Font("Segoe UI", 9F);
            lblBodegaOrigen.ForeColor = Color.FromArgb(96, 94, 92);
            lblBodegaOrigen.Location = new Point(3, 0);
            lblBodegaOrigen.Name = "lblBodegaOrigen";
            lblBodegaOrigen.Size = new Size(94, 36);
            lblBodegaOrigen.TabIndex = 0;
            lblBodegaOrigen.Text = "Origen:";
            lblBodegaOrigen.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboBodegaOrigen
            // 
            cboBodegaOrigen.Dock = DockStyle.Fill;
            cboBodegaOrigen.DropDownStyle = ComboBoxStyle.DropDownList;
            cboBodegaOrigen.Font = new Font("Segoe UI", 9F);
            cboBodegaOrigen.FormattingEnabled = true;
            cboBodegaOrigen.Location = new Point(103, 3);
            cboBodegaOrigen.Name = "cboBodegaOrigen";
            cboBodegaOrigen.Size = new Size(217, 23);
            cboBodegaOrigen.TabIndex = 1;
            // 
            // lblBodegaDestino
            // 
            lblBodegaDestino.Dock = DockStyle.Fill;
            lblBodegaDestino.Font = new Font("Segoe UI", 9F);
            lblBodegaDestino.ForeColor = Color.FromArgb(96, 94, 92);
            lblBodegaDestino.Location = new Point(326, 0);
            lblBodegaDestino.Name = "lblBodegaDestino";
            lblBodegaDestino.Size = new Size(94, 36);
            lblBodegaDestino.TabIndex = 2;
            lblBodegaDestino.Text = "Destino:";
            lblBodegaDestino.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboBodegaDestino
            // 
            cboBodegaDestino.Dock = DockStyle.Fill;
            cboBodegaDestino.DropDownStyle = ComboBoxStyle.DropDownList;
            cboBodegaDestino.Font = new Font("Segoe UI", 9F);
            cboBodegaDestino.FormattingEnabled = true;
            cboBodegaDestino.Location = new Point(426, 3);
            cboBodegaDestino.Name = "cboBodegaDestino";
            cboBodegaDestino.Size = new Size(217, 23);
            cboBodegaDestino.TabIndex = 3;
            // 
            // lblLoteTransferencia
            // 
            lblLoteTransferencia.Dock = DockStyle.Fill;
            lblLoteTransferencia.Font = new Font("Segoe UI", 9F);
            lblLoteTransferencia.ForeColor = Color.FromArgb(96, 94, 92);
            lblLoteTransferencia.Location = new Point(649, 0);
            lblLoteTransferencia.Name = "lblLoteTransferencia";
            lblLoteTransferencia.Size = new Size(94, 36);
            lblLoteTransferencia.TabIndex = 4;
            lblLoteTransferencia.Text = "Lote:";
            lblLoteTransferencia.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboLoteTransferencia
            // 
            cboLoteTransferencia.Dock = DockStyle.Fill;
            cboLoteTransferencia.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLoteTransferencia.Enabled = false;
            cboLoteTransferencia.Font = new Font("Segoe UI", 9F);
            cboLoteTransferencia.FormattingEnabled = true;
            cboLoteTransferencia.Location = new Point(749, 3);
            cboLoteTransferencia.Name = "cboLoteTransferencia";
            cboLoteTransferencia.Size = new Size(218, 23);
            cboLoteTransferencia.TabIndex = 5;
            // 
            // lblCantidadTransferencia
            // 
            lblCantidadTransferencia.Dock = DockStyle.Fill;
            lblCantidadTransferencia.Font = new Font("Segoe UI", 9F);
            lblCantidadTransferencia.ForeColor = Color.FromArgb(96, 94, 92);
            lblCantidadTransferencia.Location = new Point(3, 36);
            lblCantidadTransferencia.Name = "lblCantidadTransferencia";
            lblCantidadTransferencia.Size = new Size(94, 36);
            lblCantidadTransferencia.TabIndex = 6;
            lblCantidadTransferencia.Text = "Cantidad:";
            lblCantidadTransferencia.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numCantidadTransferencia
            // 
            numCantidadTransferencia.DecimalPlaces = 4;
            numCantidadTransferencia.Dock = DockStyle.Fill;
            numCantidadTransferencia.Font = new Font("Segoe UI", 9F);
            numCantidadTransferencia.Location = new Point(103, 39);
            numCantidadTransferencia.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numCantidadTransferencia.Name = "numCantidadTransferencia";
            numCantidadTransferencia.Size = new Size(217, 23);
            numCantidadTransferencia.TabIndex = 7;
            numCantidadTransferencia.TextAlign = HorizontalAlignment.Right;
            numCantidadTransferencia.ThousandsSeparator = true;
            // 
            // btnEjecutarTransferencia
            // 
            btnEjecutarTransferencia.BackColor = Color.FromArgb(52, 152, 219);
            btnEjecutarTransferencia.Dock = DockStyle.Right;
            btnEjecutarTransferencia.FlatAppearance.BorderSize = 0;
            btnEjecutarTransferencia.FlatStyle = FlatStyle.Flat;
            btnEjecutarTransferencia.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnEjecutarTransferencia.ForeColor = Color.White;
            btnEjecutarTransferencia.Location = new Point(847, 78);
            btnEjecutarTransferencia.Margin = new Padding(3, 6, 3, 3);
            btnEjecutarTransferencia.Name = "btnEjecutarTransferencia";
            btnEjecutarTransferencia.Size = new Size(120, 80);
            btnEjecutarTransferencia.TabIndex = 8;
            btnEjecutarTransferencia.Text = "🚚 Transferir";
            btnEjecutarTransferencia.UseVisualStyleBackColor = false;
            // 
            // ProdStockUC
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(tlpMain);
            Font = new Font("Segoe UI", 9F);
            Name = "ProdStockUC";
            Size = new Size(1024, 600);
            tlpMain.ResumeLayout(false);
            pnlSuperior.ResumeLayout(false);
            pnlSuperior.PerformLayout();
            pnlTotales.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvStock).EndInit();
            tsStock.ResumeLayout(false);
            tsStock.PerformLayout();
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            pnlAccionesStock.ResumeLayout(false);
            pnlEstadoAccion.ResumeLayout(false);
            tabAcciones.ResumeLayout(false);
            tabAjuste.ResumeLayout(false);
            tlpAjuste.ResumeLayout(false);
            tlpAjuste.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidadAjuste).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCostoAjuste).EndInit();
            tabTransferencia.ResumeLayout(false);
            tlpTransferencia.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numCantidadTransferencia).EndInit();
            ResumeLayout(false);
        }
    }
}