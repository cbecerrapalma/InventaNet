namespace Inventa.Forms.Productos
{
    partial class frm_Producto
    {
        private System.ComponentModel.IContainer components = null;

        // ── Layout Principal ──────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblBreadcrumb;

        // ── Split vertical: Explorer | Col2 | Col3 ─────────────────────────
        private System.Windows.Forms.SplitContainer splitExplorer;

        // ── Split vertical: Workspace (Col 2) | Detail (Col 3) ─────────────
        private System.Windows.Forms.SplitContainer splitCol2Col3;

        // ── Explorer (Col 1 — 280px) ───────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel panelExplorer;
        private System.Windows.Forms.TextBox txtBuscarProducto;
        private System.Windows.Forms.ComboBox cboFiltroCategoria;
        private System.Windows.Forms.ComboBox cboFiltroMarca;
        private System.Windows.Forms.ComboBox cboFiltroEstado;
        private System.Windows.Forms.ComboBox cboFiltroStock;
        private System.Windows.Forms.CheckBox chkSoloConVariantes;
        private System.Windows.Forms.Button btnRefrescar;
        private System.Windows.Forms.Button btnLimpiarFiltros;
        private System.Windows.Forms.DataGridView dgvProductosLista;
        private System.Windows.Forms.Label lblContadorProductos;
        private System.Windows.Forms.Label lblFiltrosActivos;
        private System.Windows.Forms.ToolTip toolTipFiltros;

        // ── Workspace (Col 2) ───────────────────────────────────────────
        private System.Windows.Forms.SplitContainer splitWorkspace;
        private System.Windows.Forms.Panel pnlGeneralHost;
        private System.Windows.Forms.Panel pnlVariantesHost;

        // ── Detail (Col 2 — bottom) ───────────────────────────────────────────
        private System.Windows.Forms.SplitContainer splitDetail;
        private System.Windows.Forms.Panel pnlPresentacionHost;
        private System.Windows.Forms.TabControl tabDetail;
        private System.Windows.Forms.TabPage tabConversiones;
        private System.Windows.Forms.Panel pnlConversionesHost;
        private System.Windows.Forms.TabPage tabCodigos;
        private System.Windows.Forms.Panel pnlCodigosHost;
        private System.Windows.Forms.TabPage tabPreciosVenta;
        private System.Windows.Forms.Panel pnlPreciosVentaHost;
        private System.Windows.Forms.TabPage tabStock;
        private System.Windows.Forms.Panel pnlStockHost;

        // ── Empty state labels ────────────────────────────────────────────────
        private System.Windows.Forms.Label lblEmptyVariante;
        private System.Windows.Forms.Label lblEmptyPresentacion;
        private System.Windows.Forms.Label lblEmptyProducto;

        // ── Botones + Status ───────────────────────────────────────────────────
        private System.Windows.Forms.FlowLayoutPanel flpBotonesIzq;
        private System.Windows.Forms.Button _btnDark;
        private System.Windows.Forms.Button _btnLight;
        private System.Windows.Forms.Button _btnSystem;
        private System.Windows.Forms.Button _btnNotificacion;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tslProductoActual;
        private System.Windows.Forms.ToolStripStatusLabel tslEstado;
        private System.Windows.Forms.ToolStripStatusLabel tslCambiosPendientes;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tlpRoot = new TableLayoutPanel();
            panelHeader = new Panel();
            lblBreadcrumb = new Label();
            splitExplorer = new SplitContainer();
            panelExplorer = new TableLayoutPanel();
            txtBuscarProducto = new TextBox();
            cboFiltroCategoria = new ComboBox();
            cboFiltroMarca = new ComboBox();
            cboFiltroEstado = new ComboBox();
            cboFiltroStock = new ComboBox();
            chkSoloConVariantes = new CheckBox();
            btnRefrescar = new Button();
            btnLimpiarFiltros = new Button();
            dgvProductosLista = new DataGridView();
            lblContadorProductos = new Label();
            lblFiltrosActivos = new Label();
            splitCol2Col3 = new SplitContainer();
            splitWorkspace = new SplitContainer();
            pnlGeneralHost = new Panel();
            lblEmptyProducto = new Label();
            pnlVariantesHost = new Panel();
            lblEmptyVariante = new Label();
            splitDetail = new SplitContainer();
            pnlPresentacionHost = new Panel();
            lblEmptyPresentacion = new Label();
            tabDetail = new TabControl();
            tabConversiones = new TabPage();
            pnlConversionesHost = new Panel();
            tabCodigos = new TabPage();
            pnlCodigosHost = new Panel();
            tabPreciosVenta = new TabPage();
            pnlPreciosVentaHost = new Panel();
            tabStock = new TabPage();
            pnlStockHost = new Panel();
            flpBotonesIzq = new FlowLayoutPanel();
            _btnDark = new Button();
            _btnLight = new Button();
            _btnSystem = new Button();
            _btnNotificacion = new Button();
            statusStrip = new StatusStrip();
            tslProductoActual = new ToolStripStatusLabel();
            tslEstado = new ToolStripStatusLabel();
            tslCambiosPendientes = new ToolStripStatusLabel();
            toolTipFiltros = new ToolTip(components);
            tlpRoot.SuspendLayout();
            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitExplorer).BeginInit();
            splitExplorer.Panel1.SuspendLayout();
            splitExplorer.Panel2.SuspendLayout();
            splitExplorer.SuspendLayout();
            panelExplorer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProductosLista).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitCol2Col3).BeginInit();
            splitCol2Col3.Panel1.SuspendLayout();
            splitCol2Col3.Panel2.SuspendLayout();
            splitCol2Col3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitWorkspace).BeginInit();
            splitWorkspace.Panel1.SuspendLayout();
            splitWorkspace.Panel2.SuspendLayout();
            splitWorkspace.SuspendLayout();
            pnlGeneralHost.SuspendLayout();
            pnlVariantesHost.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitDetail).BeginInit();
            splitDetail.Panel1.SuspendLayout();
            splitDetail.Panel2.SuspendLayout();
            splitDetail.SuspendLayout();
            pnlPresentacionHost.SuspendLayout();
            tabDetail.SuspendLayout();
            tabConversiones.SuspendLayout();
            tabCodigos.SuspendLayout();
            tabPreciosVenta.SuspendLayout();
            tabStock.SuspendLayout();
            flpBotonesIzq.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // tlpRoot
            // 
            tlpRoot.ColumnCount = 1;
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.Controls.Add(panelHeader, 0, 0);
            tlpRoot.Controls.Add(splitExplorer, 0, 1);
            tlpRoot.Controls.Add(flpBotonesIzq, 0, 2);
            tlpRoot.Controls.Add(statusStrip, 0, 3);
            tlpRoot.Dock = DockStyle.Fill;
            tlpRoot.Location = new Point(0, 0);
            tlpRoot.Name = "tlpRoot";
            tlpRoot.RowCount = 4;
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tlpRoot.Size = new Size(1400, 750);
            tlpRoot.TabIndex = 0;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(30, 60, 114);
            panelHeader.Controls.Add(lblBreadcrumb);
            panelHeader.Dock = DockStyle.Fill;
            panelHeader.Location = new Point(3, 3);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1394, 42);
            panelHeader.TabIndex = 0;
            // 
            // lblBreadcrumb
            // 
            lblBreadcrumb.Dock = DockStyle.Fill;
            lblBreadcrumb.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblBreadcrumb.ForeColor = Color.White;
            lblBreadcrumb.Location = new Point(0, 0);
            lblBreadcrumb.Name = "lblBreadcrumb";
            lblBreadcrumb.Padding = new Padding(36, 0, 12, 0);
            lblBreadcrumb.Size = new Size(1394, 42);
            lblBreadcrumb.TabIndex = 1;
            lblBreadcrumb.Text = "Productos";
            lblBreadcrumb.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // splitExplorer
            // 
            splitExplorer.Dock = DockStyle.Fill;
            splitExplorer.Location = new Point(3, 51);
            splitExplorer.Name = "splitExplorer";
            // 
            // splitExplorer.Panel1
            // 
            splitExplorer.Panel1.Controls.Add(panelExplorer);
            splitExplorer.Panel1MinSize = 280;
            // 
            // splitExplorer.Panel2
            // 
            splitExplorer.Panel2.Controls.Add(splitCol2Col3);
            splitExplorer.Panel2MinSize = 200;
            splitExplorer.Size = new Size(1394, 632);
            splitExplorer.SplitterDistance = 280;
            splitExplorer.TabIndex = 1;
            // 
            // panelExplorer
            // 
            panelExplorer.BackColor = Color.FromArgb(245, 247, 250);
            panelExplorer.BorderStyle = BorderStyle.FixedSingle;
            panelExplorer.ColumnCount = 1;
            panelExplorer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panelExplorer.Controls.Add(txtBuscarProducto, 0, 0);
            panelExplorer.Controls.Add(cboFiltroCategoria, 0, 1);
            panelExplorer.Controls.Add(cboFiltroMarca, 0, 2);
            panelExplorer.Controls.Add(cboFiltroEstado, 0, 3);
            panelExplorer.Controls.Add(cboFiltroStock, 0, 4);
            panelExplorer.Controls.Add(chkSoloConVariantes, 0, 5);
            panelExplorer.Controls.Add(btnRefrescar, 0, 6);
            panelExplorer.Controls.Add(btnLimpiarFiltros, 0, 7);
            panelExplorer.Controls.Add(dgvProductosLista, 0, 8);
            panelExplorer.Controls.Add(lblContadorProductos, 0, 9);
            panelExplorer.Controls.Add(lblFiltrosActivos, 0, 10);
            panelExplorer.Dock = DockStyle.Fill;
            panelExplorer.Location = new Point(0, 0);
            panelExplorer.Name = "panelExplorer";
            panelExplorer.Padding = new Padding(6);
            panelExplorer.RowCount = 11;
            panelExplorer.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            panelExplorer.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            panelExplorer.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            panelExplorer.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            panelExplorer.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            panelExplorer.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            panelExplorer.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            panelExplorer.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            panelExplorer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panelExplorer.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            panelExplorer.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            panelExplorer.Size = new Size(280, 632);
            panelExplorer.TabIndex = 0;
            // 
            // txtBuscarProducto
            // 
            txtBuscarProducto.Dock = DockStyle.Fill;
            txtBuscarProducto.Font = new Font("Segoe UI", 10F);
            txtBuscarProducto.Location = new Point(6, 6);
            txtBuscarProducto.Margin = new Padding(0, 0, 0, 2);
            txtBuscarProducto.Name = "txtBuscarProducto";
            txtBuscarProducto.PlaceholderText = "Código o nombre...";
            txtBuscarProducto.Size = new Size(266, 25);
            txtBuscarProducto.TabIndex = 0;
            // 
            // cboFiltroCategoria
            // 
            cboFiltroCategoria.Dock = DockStyle.Fill;
            cboFiltroCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFiltroCategoria.Font = new Font("Segoe UI", 9F);
            cboFiltroCategoria.Location = new Point(6, 34);
            cboFiltroCategoria.Margin = new Padding(0, 0, 0, 2);
            cboFiltroCategoria.Name = "cboFiltroCategoria";
            cboFiltroCategoria.Size = new Size(266, 23);
            cboFiltroCategoria.TabIndex = 1;
            // 
            // cboFiltroMarca
            // 
            cboFiltroMarca.Dock = DockStyle.Fill;
            cboFiltroMarca.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFiltroMarca.Font = new Font("Segoe UI", 9F);
            cboFiltroMarca.Location = new Point(6, 60);
            cboFiltroMarca.Margin = new Padding(0, 0, 0, 2);
            cboFiltroMarca.Name = "cboFiltroMarca";
            cboFiltroMarca.Size = new Size(266, 23);
            cboFiltroMarca.TabIndex = 2;
            // 
            // cboFiltroEstado
            // 
            cboFiltroEstado.Dock = DockStyle.Fill;
            cboFiltroEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFiltroEstado.Font = new Font("Segoe UI", 9F);
            cboFiltroEstado.Items.AddRange(new object[] { "Todos", "Solo activos", "Solo inactivos" });
            cboFiltroEstado.Location = new Point(6, 86);
            cboFiltroEstado.Margin = new Padding(0, 0, 0, 2);
            cboFiltroEstado.Name = "cboFiltroEstado";
            cboFiltroEstado.Size = new Size(266, 23);
            cboFiltroEstado.TabIndex = 3;
            // 
            // cboFiltroStock
            // 
            cboFiltroStock.Dock = DockStyle.Fill;
            cboFiltroStock.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFiltroStock.Font = new Font("Segoe UI", 9F);
            cboFiltroStock.Items.AddRange(new object[] { "Cualquier stock", "Con stock", "Sin stock", "Stock bajo" });
            cboFiltroStock.Location = new Point(6, 112);
            cboFiltroStock.Margin = new Padding(0, 0, 0, 2);
            cboFiltroStock.Name = "cboFiltroStock";
            cboFiltroStock.Size = new Size(266, 23);
            cboFiltroStock.TabIndex = 4;
            // 
            // chkSoloConVariantes
            // 
            chkSoloConVariantes.Dock = DockStyle.Fill;
            chkSoloConVariantes.Font = new Font("Segoe UI", 8.5F);
            chkSoloConVariantes.Location = new Point(6, 140);
            chkSoloConVariantes.Margin = new Padding(0, 2, 0, 2);
            chkSoloConVariantes.Name = "chkSoloConVariantes";
            chkSoloConVariantes.Size = new Size(266, 24);
            chkSoloConVariantes.TabIndex = 5;
            chkSoloConVariantes.Text = "Solo con variantes";
            // 
            // btnRefrescar
            // 
            btnRefrescar.BackColor = Color.FromArgb(52, 73, 94);
            btnRefrescar.Dock = DockStyle.Fill;
            btnRefrescar.FlatAppearance.BorderSize = 0;
            btnRefrescar.FlatStyle = FlatStyle.Flat;
            btnRefrescar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRefrescar.ForeColor = Color.White;
            btnRefrescar.Location = new Point(6, 166);
            btnRefrescar.Margin = new Padding(0, 0, 0, 2);
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Size = new Size(266, 26);
            btnRefrescar.TabIndex = 6;
            btnRefrescar.Text = "Buscar (F5)";
            btnRefrescar.UseVisualStyleBackColor = false;
            // 
            // btnLimpiarFiltros
            // 
            btnLimpiarFiltros.BackColor = Color.FromArgb(189, 195, 199);
            btnLimpiarFiltros.Dock = DockStyle.Fill;
            btnLimpiarFiltros.Enabled = false;
            btnLimpiarFiltros.FlatAppearance.BorderSize = 0;
            btnLimpiarFiltros.FlatStyle = FlatStyle.Flat;
            btnLimpiarFiltros.Font = new Font("Segoe UI", 9F);
            btnLimpiarFiltros.Location = new Point(9, 197);
            btnLimpiarFiltros.Name = "btnLimpiarFiltros";
            btnLimpiarFiltros.Size = new Size(260, 26);
            btnLimpiarFiltros.TabIndex = 7;
            btnLimpiarFiltros.Text = "Limpiar filtros";
            btnLimpiarFiltros.UseVisualStyleBackColor = false;
            // 
            // dgvProductosLista
            // 
            dgvProductosLista.AllowUserToAddRows = false;
            dgvProductosLista.AllowUserToDeleteRows = false;
            dgvProductosLista.AllowUserToResizeColumns = false;
            dgvProductosLista.AllowUserToResizeRows = false;
            dgvProductosLista.BackgroundColor = Color.White;
            dgvProductosLista.BorderStyle = BorderStyle.None;
            dgvProductosLista.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgvProductosLista.ColumnHeadersVisible = false;
            dgvProductosLista.Dock = DockStyle.Fill;
            dgvProductosLista.Location = new Point(9, 229);
            dgvProductosLista.MultiSelect = false;
            dgvProductosLista.Name = "dgvProductosLista";
            dgvProductosLista.ReadOnly = true;
            dgvProductosLista.RowHeadersVisible = false;
            dgvProductosLista.RowTemplate.Height = 36;
            dgvProductosLista.ScrollBars = ScrollBars.Vertical;
            dgvProductosLista.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductosLista.Size = new Size(260, 352);
            dgvProductosLista.TabIndex = 8;
            dgvProductosLista.VirtualMode = true;
            // 
            // lblContadorProductos
            // 
            lblContadorProductos.Dock = DockStyle.Fill;
            lblContadorProductos.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblContadorProductos.ForeColor = Color.FromArgb(96, 94, 92);
            lblContadorProductos.Location = new Point(9, 584);
            lblContadorProductos.Name = "lblContadorProductos";
            lblContadorProductos.Size = new Size(260, 22);
            lblContadorProductos.TabIndex = 9;
            lblContadorProductos.Text = "0 productos";
            lblContadorProductos.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblFiltrosActivos
            // 
            lblFiltrosActivos.Dock = DockStyle.Fill;
            lblFiltrosActivos.Font = new Font("Segoe UI", 7.5F, FontStyle.Italic);
            lblFiltrosActivos.ForeColor = Color.FromArgb(230, 126, 34);
            lblFiltrosActivos.Location = new Point(9, 606);
            lblFiltrosActivos.Name = "lblFiltrosActivos";
            lblFiltrosActivos.Size = new Size(260, 18);
            lblFiltrosActivos.TabIndex = 10;
            lblFiltrosActivos.TextAlign = ContentAlignment.MiddleCenter;
            lblFiltrosActivos.Visible = false;
            // 
            // splitCol2Col3
            // 
            splitCol2Col3.Dock = DockStyle.Fill;
            splitCol2Col3.Location = new Point(0, 0);
            splitCol2Col3.Name = "splitCol2Col3";
            // 
            // splitCol2Col3.Panel1
            // 
            splitCol2Col3.Panel1.Controls.Add(splitWorkspace);
            splitCol2Col3.Panel1MinSize = 200;
            // 
            // splitCol2Col3.Panel2
            // 
            splitCol2Col3.Panel2.Controls.Add(splitDetail);
            splitCol2Col3.Panel2MinSize = 200;
            splitCol2Col3.Size = new Size(1110, 632);
            splitCol2Col3.SplitterDistance = 420;
            splitCol2Col3.TabIndex = 0;
            // 
            // splitWorkspace
            // 
            splitWorkspace.Dock = DockStyle.Fill;
            splitWorkspace.FixedPanel = FixedPanel.Panel1;
            splitWorkspace.Location = new Point(0, 0);
            splitWorkspace.Name = "splitWorkspace";
            splitWorkspace.Orientation = Orientation.Horizontal;
            // 
            // splitWorkspace.Panel1
            // 
            splitWorkspace.Panel1.Controls.Add(pnlGeneralHost);
            splitWorkspace.Panel1MinSize = 180;
            // 
            // splitWorkspace.Panel2
            // 
            splitWorkspace.Panel2.Controls.Add(pnlVariantesHost);
            splitWorkspace.Panel2MinSize = 120;
            splitWorkspace.Size = new Size(420, 632);
            splitWorkspace.SplitterDistance = 283;
            splitWorkspace.TabIndex = 0;
            // 
            // pnlGeneralHost
            // 
            pnlGeneralHost.AutoScroll = true;
            pnlGeneralHost.BackColor = Color.White;
            pnlGeneralHost.BorderStyle = BorderStyle.FixedSingle;
            pnlGeneralHost.Controls.Add(lblEmptyProducto);
            pnlGeneralHost.Dock = DockStyle.Fill;
            pnlGeneralHost.Location = new Point(0, 0);
            pnlGeneralHost.Name = "pnlGeneralHost";
            pnlGeneralHost.Size = new Size(420, 283);
            pnlGeneralHost.TabIndex = 0;
            // 
            // lblEmptyProducto
            // 
            lblEmptyProducto.Dock = DockStyle.Fill;
            lblEmptyProducto.Font = new Font("Segoe UI", 11F, FontStyle.Italic);
            lblEmptyProducto.ForeColor = Color.FromArgb(150, 150, 150);
            lblEmptyProducto.Location = new Point(0, 0);
            lblEmptyProducto.Name = "lblEmptyProducto";
            lblEmptyProducto.Size = new Size(418, 281);
            lblEmptyProducto.TabIndex = 0;
            lblEmptyProducto.Text = "Seleccione un producto de la lista";
            lblEmptyProducto.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlVariantesHost
            // 
            pnlVariantesHost.AutoScroll = true;
            pnlVariantesHost.BackColor = Color.White;
            pnlVariantesHost.BorderStyle = BorderStyle.FixedSingle;
            pnlVariantesHost.Controls.Add(lblEmptyVariante);
            pnlVariantesHost.Dock = DockStyle.Fill;
            pnlVariantesHost.Location = new Point(0, 0);
            pnlVariantesHost.Name = "pnlVariantesHost";
            pnlVariantesHost.Size = new Size(420, 345);
            pnlVariantesHost.TabIndex = 1;
            // 
            // lblEmptyVariante
            // 
            lblEmptyVariante.Dock = DockStyle.Fill;
            lblEmptyVariante.Font = new Font("Segoe UI", 11F, FontStyle.Italic);
            lblEmptyVariante.ForeColor = Color.FromArgb(150, 150, 150);
            lblEmptyVariante.Location = new Point(0, 0);
            lblEmptyVariante.Name = "lblEmptyVariante";
            lblEmptyVariante.Size = new Size(418, 343);
            lblEmptyVariante.TabIndex = 0;
            lblEmptyVariante.Text = "Seleccione una variante para ver presentaciones";
            lblEmptyVariante.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // splitDetail
            // 
            splitDetail.Dock = DockStyle.Fill;
            splitDetail.FixedPanel = FixedPanel.Panel1;
            splitDetail.Location = new Point(0, 0);
            splitDetail.Name = "splitDetail";
            splitDetail.Orientation = Orientation.Horizontal;
            // 
            // splitDetail.Panel1
            // 
            splitDetail.Panel1.Controls.Add(pnlPresentacionHost);
            splitDetail.Panel1MinSize = 80;
            // 
            // splitDetail.Panel2
            // 
            splitDetail.Panel2.Controls.Add(tabDetail);
            splitDetail.Size = new Size(686, 632);
            splitDetail.SplitterDistance = 270;
            splitDetail.TabIndex = 1;
            // 
            // pnlPresentacionHost
            // 
            pnlPresentacionHost.AutoScroll = true;
            pnlPresentacionHost.BackColor = Color.White;
            pnlPresentacionHost.BorderStyle = BorderStyle.FixedSingle;
            pnlPresentacionHost.Controls.Add(lblEmptyPresentacion);
            pnlPresentacionHost.Dock = DockStyle.Fill;
            pnlPresentacionHost.Location = new Point(0, 0);
            pnlPresentacionHost.Name = "pnlPresentacionHost";
            pnlPresentacionHost.Size = new Size(686, 270);
            pnlPresentacionHost.TabIndex = 0;
            // 
            // lblEmptyPresentacion
            // 
            lblEmptyPresentacion.Dock = DockStyle.Fill;
            lblEmptyPresentacion.Font = new Font("Segoe UI", 11F, FontStyle.Italic);
            lblEmptyPresentacion.ForeColor = Color.FromArgb(150, 150, 150);
            lblEmptyPresentacion.Location = new Point(0, 0);
            lblEmptyPresentacion.Name = "lblEmptyPresentacion";
            lblEmptyPresentacion.Size = new Size(684, 268);
            lblEmptyPresentacion.TabIndex = 0;
            lblEmptyPresentacion.Text = "Seleccione una presentación para ver detalles";
            lblEmptyPresentacion.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tabDetail
            // 
            tabDetail.Appearance = TabAppearance.FlatButtons;
            tabDetail.Controls.Add(tabConversiones);
            tabDetail.Controls.Add(tabCodigos);
            tabDetail.Controls.Add(tabPreciosVenta);
            tabDetail.Controls.Add(tabStock);
            tabDetail.Dock = DockStyle.Fill;
            tabDetail.Font = new Font("Segoe UI", 9F);
            tabDetail.Location = new Point(0, 0);
            tabDetail.Name = "tabDetail";
            tabDetail.SelectedIndex = 0;
            tabDetail.Size = new Size(686, 358);
            tabDetail.TabIndex = 0;
            // 
            // tabConversiones
            // 
            tabConversiones.BackColor = Color.White;
            tabConversiones.Controls.Add(pnlConversionesHost);
            tabConversiones.Location = new Point(4, 27);
            tabConversiones.Name = "tabConversiones";
            tabConversiones.Padding = new Padding(4);
            tabConversiones.Size = new Size(678, 327);
            tabConversiones.TabIndex = 0;
            tabConversiones.Text = "Conversiones";
            // 
            // pnlConversionesHost
            // 
            pnlConversionesHost.Dock = DockStyle.Fill;
            pnlConversionesHost.Location = new Point(4, 4);
            pnlConversionesHost.Name = "pnlConversionesHost";
            pnlConversionesHost.Size = new Size(670, 319);
            pnlConversionesHost.TabIndex = 0;
            // 
            // tabCodigos
            // 
            tabCodigos.BackColor = Color.White;
            tabCodigos.Controls.Add(pnlCodigosHost);
            tabCodigos.Location = new Point(4, 27);
            tabCodigos.Name = "tabCodigos";
            tabCodigos.Padding = new Padding(4);
            tabCodigos.Size = new Size(678, 327);
            tabCodigos.TabIndex = 1;
            tabCodigos.Text = "Códigos";
            // 
            // pnlCodigosHost
            // 
            pnlCodigosHost.Dock = DockStyle.Fill;
            pnlCodigosHost.Location = new Point(4, 4);
            pnlCodigosHost.Name = "pnlCodigosHost";
            pnlCodigosHost.Size = new Size(670, 319);
            pnlCodigosHost.TabIndex = 0;
            // 
            // tabPreciosVenta
            // 
            tabPreciosVenta.BackColor = Color.White;
            tabPreciosVenta.Controls.Add(pnlPreciosVentaHost);
            tabPreciosVenta.Location = new Point(4, 27);
            tabPreciosVenta.Name = "tabPreciosVenta";
            tabPreciosVenta.Padding = new Padding(4);
            tabPreciosVenta.Size = new Size(678, 327);
            tabPreciosVenta.TabIndex = 2;
            tabPreciosVenta.Text = "Precios Venta";
            // 
            // pnlPreciosVentaHost
            // 
            pnlPreciosVentaHost.Dock = DockStyle.Fill;
            pnlPreciosVentaHost.Location = new Point(4, 4);
            pnlPreciosVentaHost.Name = "pnlPreciosVentaHost";
            pnlPreciosVentaHost.Size = new Size(670, 319);
            pnlPreciosVentaHost.TabIndex = 0;
            // 
            // tabStock
            // 
            tabStock.BackColor = Color.White;
            tabStock.Controls.Add(pnlStockHost);
            tabStock.Location = new Point(4, 27);
            tabStock.Name = "tabStock";
            tabStock.Padding = new Padding(4);
            tabStock.Size = new Size(678, 327);
            tabStock.TabIndex = 4;
            tabStock.Text = "Stock";
            // 
            // pnlStockHost
            // 
            pnlStockHost.Dock = DockStyle.Fill;
            pnlStockHost.Location = new Point(4, 4);
            pnlStockHost.Name = "pnlStockHost";
            pnlStockHost.Size = new Size(670, 319);
            pnlStockHost.TabIndex = 0;
            // 
            // flpBotonesIzq
            // 
            flpBotonesIzq.BackColor = Color.FromArgb(243, 242, 241);
            flpBotonesIzq.Controls.Add(_btnDark);
            flpBotonesIzq.Controls.Add(_btnLight);
            flpBotonesIzq.Controls.Add(_btnSystem);
            flpBotonesIzq.Controls.Add(_btnNotificacion);
            flpBotonesIzq.Dock = DockStyle.Fill;
            flpBotonesIzq.Location = new Point(3, 689);
            flpBotonesIzq.Name = "flpBotonesIzq";
            flpBotonesIzq.Padding = new Padding(8);
            flpBotonesIzq.Size = new Size(1394, 34);
            flpBotonesIzq.TabIndex = 2;
            // 
            // _btnDark
            // 
            _btnDark.Location = new Point(11, 11);
            _btnDark.Name = "_btnDark";
            _btnDark.Size = new Size(75, 23);
            _btnDark.TabIndex = 0;
            _btnDark.Text = "Dark";
            _btnDark.UseVisualStyleBackColor = true;
            // 
            // _btnLight
            // 
            _btnLight.Location = new Point(92, 11);
            _btnLight.Name = "_btnLight";
            _btnLight.Size = new Size(75, 23);
            _btnLight.TabIndex = 1;
            _btnLight.Text = "Light";
            _btnLight.UseVisualStyleBackColor = true;
            // 
            // _btnSystem
            // 
            _btnSystem.Location = new Point(173, 11);
            _btnSystem.Name = "_btnSystem";
            _btnSystem.Size = new Size(75, 23);
            _btnSystem.TabIndex = 2;
            _btnSystem.Text = "System";
            _btnSystem.UseVisualStyleBackColor = true;
            // 
            // _btnNotificacion
            // 
            _btnNotificacion.Location = new Point(254, 11);
            _btnNotificacion.Name = "_btnNotificacion";
            _btnNotificacion.Size = new Size(75, 23);
            _btnNotificacion.TabIndex = 3;
            _btnNotificacion.Text = "Notificacion";
            _btnNotificacion.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            statusStrip.Dock = DockStyle.Fill;
            statusStrip.Items.AddRange(new ToolStripItem[] { tslProductoActual, tslEstado, tslCambiosPendientes });
            statusStrip.Location = new Point(0, 726);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1400, 24);
            statusStrip.SizingGrip = false;
            statusStrip.TabIndex = 3;
            // 
            // tslProductoActual
            // 
            tslProductoActual.BorderSides = ToolStripStatusLabelBorderSides.Right;
            tslProductoActual.BorderStyle = Border3DStyle.Etched;
            tslProductoActual.Name = "tslProductoActual";
            tslProductoActual.Size = new Size(151, 19);
            tslProductoActual.Text = "Sin producto seleccionado";
            // 
            // tslEstado
            // 
            tslEstado.Name = "tslEstado";
            tslEstado.Size = new Size(1234, 19);
            tslEstado.Spring = true;
            tslEstado.Text = "Listo";
            // 
            // tslCambiosPendientes
            // 
            tslCambiosPendientes.ForeColor = Color.FromArgb(230, 126, 34);
            tslCambiosPendientes.Name = "tslCambiosPendientes";
            tslCambiosPendientes.Size = new Size(0, 19);
            // 
            // toolTipFiltros
            // 
            toolTipFiltros.AutoPopDelay = 5000;
            toolTipFiltros.BackColor = Color.FromArgb(255, 249, 230);
            toolTipFiltros.ForeColor = Color.FromArgb(50, 49, 48);
            toolTipFiltros.InitialDelay = 500;
            toolTipFiltros.ReshowDelay = 100;
            toolTipFiltros.ShowAlways = true;
            // 
            // frm_Producto
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1400, 750);
            Controls.Add(tlpRoot);
            Font = new Font("Segoe UI", 9F);
            ForeColor = SystemColors.WindowText;
            KeyPreview = true;
            MinimumSize = new Size(1024, 600);
            Name = "frm_Producto";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Productos - Inventa";
            WindowState = FormWindowState.Maximized;
            tlpRoot.ResumeLayout(false);
            tlpRoot.PerformLayout();
            panelHeader.ResumeLayout(false);
            splitExplorer.Panel1.ResumeLayout(false);
            splitExplorer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitExplorer).EndInit();
            splitExplorer.ResumeLayout(false);
            panelExplorer.ResumeLayout(false);
            panelExplorer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProductosLista).EndInit();
            splitCol2Col3.Panel1.ResumeLayout(false);
            splitCol2Col3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitCol2Col3).EndInit();
            splitCol2Col3.ResumeLayout(false);
            splitWorkspace.Panel1.ResumeLayout(false);
            splitWorkspace.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitWorkspace).EndInit();
            splitWorkspace.ResumeLayout(false);
            pnlGeneralHost.ResumeLayout(false);
            pnlVariantesHost.ResumeLayout(false);
            splitDetail.Panel1.ResumeLayout(false);
            splitDetail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitDetail).EndInit();
            splitDetail.ResumeLayout(false);
            pnlPresentacionHost.ResumeLayout(false);
            tabDetail.ResumeLayout(false);
            tabConversiones.ResumeLayout(false);
            tabCodigos.ResumeLayout(false);
            tabPreciosVenta.ResumeLayout(false);
            tabStock.ResumeLayout(false);
            flpBotonesIzq.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
        }
    }
}
