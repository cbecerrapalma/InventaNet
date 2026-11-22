namespace Inventa.Forms
{
    partial class frm_Principal
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Panel panelSuperior;
        private System.Windows.Forms.Panel panelMenu;
        private System.Windows.Forms.Panel panelContenido;
        private System.Windows.Forms.Panel panelInferior;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.Label lblTenant;
        private System.Windows.Forms.Label lblFecha;
        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private System.Windows.Forms.MenuStrip menuStripPrincipal;
        private System.Windows.Forms.ToolStripMenuItem mnuArchivo;
        private System.Windows.Forms.ToolStripMenuItem mnuSalir;
        private System.Windows.Forms.ToolStripMenuItem mnuConfiguracion;
        private System.Windows.Forms.ToolStripMenuItem mnuTenant;
        private System.Windows.Forms.ToolStripMenuItem mnuUsuarios;
        private System.Windows.Forms.ToolStripMenuItem mnuBackup;
        private System.Windows.Forms.ToolStripMenuItem mnuMaestros;
        private System.Windows.Forms.ToolStripMenuItem mnuProductos;
        private System.Windows.Forms.ToolStripMenuItem mnuCategorias;
        private System.Windows.Forms.ToolStripMenuItem mnuMarcas;
        private System.Windows.Forms.ToolStripMenuItem mnuUnidades;
        private System.Windows.Forms.ToolStripMenuItem mnuProveedores;
        private System.Windows.Forms.ToolStripMenuItem mnuClientes;
        private System.Windows.Forms.ToolStripMenuItem mnuBodegas;
        private System.Windows.Forms.ToolStripMenuItem mnuInventario;
        private System.Windows.Forms.ToolStripMenuItem mnuStock;
        private System.Windows.Forms.ToolStripMenuItem mnuAjustes;
        private System.Windows.Forms.ToolStripMenuItem mnuTransferencias;
        private System.Windows.Forms.ToolStripMenuItem mnuCompras;
        private System.Windows.Forms.ToolStripMenuItem mnuOrdenCompra;
        private System.Windows.Forms.ToolStripMenuItem mnuRecepcion;
        private System.Windows.Forms.ToolStripMenuItem mnuVentas;
        private System.Windows.Forms.ToolStripMenuItem mnuPOS;
        private System.Windows.Forms.ToolStripMenuItem mnuClientesVentas;
        private System.Windows.Forms.ToolStripMenuItem mnuReportes;
        private System.Windows.Forms.ToolStripMenuItem mnuReporteStock;
        private System.Windows.Forms.ToolStripMenuItem mnuReporteVentas;
        private System.Windows.Forms.ToolStripMenuItem mnuReporteCompras;
        private System.Windows.Forms.ToolStripMenuItem mnuReporteUtilidad;
        private System.Windows.Forms.ToolStripMenuItem mnuAyuda;
        private System.Windows.Forms.ToolStripMenuItem mnuAcercaDe;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tsslUsuario;
        private System.Windows.Forms.ToolStripStatusLabel tsslTenant;
        private System.Windows.Forms.ToolStripStatusLabel tsslFecha;
        private System.Windows.Forms.ToolStripStatusLabel tsslVersion;
        private System.Windows.Forms.Timer timerReloj;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TreeView treeViewMenu;
        private System.Windows.Forms.Panel panelDashboard;
        private System.Windows.Forms.TableLayoutPanel tlpDashboard;
        private System.Windows.Forms.GroupBox gpbResumen;
        private System.Windows.Forms.TableLayoutPanel tlpResumen;
        private System.Windows.Forms.Label lblTotalProductos;
        private System.Windows.Forms.Label lblTotalProductosValor;
        private System.Windows.Forms.Label lblStockBajo;
        private System.Windows.Forms.Label lblStockBajoValor;
        private System.Windows.Forms.Label lblVentasHoy;
        private System.Windows.Forms.Label lblVentasHoyValor;
        private System.Windows.Forms.Label lblComprasHoy;
        private System.Windows.Forms.Label lblComprasHoyValor;
        private System.Windows.Forms.Label lblClientes;
        private System.Windows.Forms.Label lblClientesValor;
        private System.Windows.Forms.Label lblProveedores;
        private System.Windows.Forms.Label lblProveedoresValor;
        private System.Windows.Forms.GroupBox gpbAlertas;
        private System.Windows.Forms.ListBox lstAlertas;
        private System.Windows.Forms.GroupBox gpbAccionesRapidas;
        private System.Windows.Forms.FlowLayoutPanel flpAcciones;
        private System.Windows.Forms.Button btnNuevoProducto;
        private System.Windows.Forms.Button btnNuevaVenta;
        private System.Windows.Forms.Button btnNuevaCompra;
        private System.Windows.Forms.Button btnAjusteStock;
        private System.Windows.Forms.Button btnReporteDiario;
        private System.Windows.Forms.DataGridView dgvProductosCriticos;
        private System.Windows.Forms.DataGridView dgvUltimosMovimientos;

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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            tlpMain = new TableLayoutPanel();
            panelSuperior = new Panel();
            pictureBoxLogo = new PictureBox();
            lblTitulo = new Label();
            lblUsuario = new Label();
            lblTenant = new Label();
            lblFecha = new Label();
            menuStripPrincipal = new MenuStrip();
            mnuArchivo = new ToolStripMenuItem();
            mnuConfiguracion = new ToolStripMenuItem();
            mnuTenant = new ToolStripMenuItem();
            mnuUsuarios = new ToolStripMenuItem();
            mnuBackup = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            mnuSalir = new ToolStripMenuItem();
            mnuMaestros = new ToolStripMenuItem();
            mnuProductos = new ToolStripMenuItem();
            mnuCategorias = new ToolStripMenuItem();
            mnuMarcas = new ToolStripMenuItem();
            mnuUnidades = new ToolStripMenuItem();
            mnuProveedores = new ToolStripMenuItem();
            mnuClientes = new ToolStripMenuItem();
            mnuBodegas = new ToolStripMenuItem();
            mnuInventario = new ToolStripMenuItem();
            mnuStock = new ToolStripMenuItem();
            mnuAjustes = new ToolStripMenuItem();
            mnuTransferencias = new ToolStripMenuItem();
            mnuCompras = new ToolStripMenuItem();
            mnuOrdenCompra = new ToolStripMenuItem();
            mnuRecepcion = new ToolStripMenuItem();
            mnuVentas = new ToolStripMenuItem();
            mnuPOS = new ToolStripMenuItem();
            mnuClientesVentas = new ToolStripMenuItem();
            mnuReportes = new ToolStripMenuItem();
            mnuReporteStock = new ToolStripMenuItem();
            mnuReporteVentas = new ToolStripMenuItem();
            mnuReporteCompras = new ToolStripMenuItem();
            mnuReporteUtilidad = new ToolStripMenuItem();
            mnuAyuda = new ToolStripMenuItem();
            mnuAcercaDe = new ToolStripMenuItem();
            splitContainer = new SplitContainer();
            treeViewMenu = new TreeView();
            panelDashboard = new Panel();
            tlpDashboard = new TableLayoutPanel();
            gpbResumen = new GroupBox();
            tlpResumen = new TableLayoutPanel();
            lblTotalProductos = new Label();
            lblTotalProductosValor = new Label();
            lblStockBajo = new Label();
            lblStockBajoValor = new Label();
            lblVentasHoy = new Label();
            lblVentasHoyValor = new Label();
            lblComprasHoy = new Label();
            lblComprasHoyValor = new Label();
            lblClientes = new Label();
            lblClientesValor = new Label();
            lblProveedores = new Label();
            lblProveedoresValor = new Label();
            gpbAlertas = new GroupBox();
            lstAlertas = new ListBox();
            gpbAccionesRapidas = new GroupBox();
            flpAcciones = new FlowLayoutPanel();
            btnNuevoProducto = new Button();
            btnNuevaVenta = new Button();
            btnNuevaCompra = new Button();
            btnAjusteStock = new Button();
            btnReporteDiario = new Button();
            dgvProductosCriticos = new DataGridView();
            dgvUltimosMovimientos = new DataGridView();
            panelInferior = new Panel();
            statusStrip = new StatusStrip();
            tsslUsuario = new ToolStripStatusLabel();
            tsslTenant = new ToolStripStatusLabel();
            tsslFecha = new ToolStripStatusLabel();
            tsslVersion = new ToolStripStatusLabel();
            timerReloj = new System.Windows.Forms.Timer(components);
            tlpMain.SuspendLayout();
            panelSuperior.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).BeginInit();
            menuStripPrincipal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            panelDashboard.SuspendLayout();
            tlpDashboard.SuspendLayout();
            gpbResumen.SuspendLayout();
            tlpResumen.SuspendLayout();
            gpbAlertas.SuspendLayout();
            gpbAccionesRapidas.SuspendLayout();
            flpAcciones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProductosCriticos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvUltimosMovimientos).BeginInit();
            panelInferior.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(panelSuperior, 0, 0);
            tlpMain.Controls.Add(splitContainer, 0, 1);
            tlpMain.Controls.Add(panelInferior, 0, 2);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 114F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpMain.Size = new Size(1200, 700);
            tlpMain.TabIndex = 0;
            // 
            // panelSuperior
            // 
            panelSuperior.BackColor = Color.FromArgb(30, 60, 114);
            panelSuperior.Controls.Add(pictureBoxLogo);
            panelSuperior.Controls.Add(lblTitulo);
            panelSuperior.Controls.Add(lblUsuario);
            panelSuperior.Controls.Add(lblTenant);
            panelSuperior.Controls.Add(lblFecha);
            panelSuperior.Controls.Add(menuStripPrincipal);
            panelSuperior.Dock = DockStyle.Fill;
            panelSuperior.Location = new Point(3, 3);
            panelSuperior.Name = "panelSuperior";
            panelSuperior.Size = new Size(1194, 108);
            panelSuperior.TabIndex = 0;
            // 
            // pictureBoxLogo
            // 
            pictureBoxLogo.BackColor = Color.White;
            pictureBoxLogo.Location = new Point(10, 25);
            pictureBoxLogo.Name = "pictureBoxLogo";
            pictureBoxLogo.Size = new Size(60, 60);
            pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxLogo.TabIndex = 4;
            pictureBoxLogo.TabStop = false;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(80, 35);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(306, 32);
            lblTitulo.TabIndex = 3;
            lblTitulo.Text = "SISTEMA DE INVENTARIO";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUsuario
            // 
            lblUsuario.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUsuario.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUsuario.ForeColor = Color.White;
            lblUsuario.Location = new Point(950, 25);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(200, 20);
            lblUsuario.TabIndex = 1;
            lblUsuario.Text = "Usuario: ADMIN";
            lblUsuario.TextAlign = ContentAlignment.TopRight;
            // 
            // lblTenant
            // 
            lblTenant.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTenant.Font = new Font("Segoe UI", 9F);
            lblTenant.ForeColor = Color.White;
            lblTenant.Location = new Point(950, 45);
            lblTenant.Name = "lblTenant";
            lblTenant.Size = new Size(200, 20);
            lblTenant.TabIndex = 2;
            lblTenant.Text = "Tenant: EMPRESA DEMO";
            lblTenant.TextAlign = ContentAlignment.TopRight;
            // 
            // lblFecha
            // 
            lblFecha.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblFecha.Font = new Font("Segoe UI", 9F);
            lblFecha.ForeColor = Color.White;
            lblFecha.Location = new Point(950, 65);
            lblFecha.Name = "lblFecha";
            lblFecha.Size = new Size(200, 20);
            lblFecha.TabIndex = 0;
            lblFecha.Text = "24/02/2026 14:30:00";
            lblFecha.TextAlign = ContentAlignment.TopRight;
            // 
            // menuStripPrincipal
            // 
            menuStripPrincipal.BackColor = Color.FromArgb(30, 60, 114);
            menuStripPrincipal.Dock = DockStyle.Bottom;
            menuStripPrincipal.Items.AddRange(new ToolStripItem[] { mnuArchivo, mnuMaestros, mnuInventario, mnuCompras, mnuVentas, mnuReportes, mnuAyuda });
            menuStripPrincipal.Location = new Point(0, 84);
            menuStripPrincipal.Name = "menuStripPrincipal";
            menuStripPrincipal.Size = new Size(1194, 24);
            menuStripPrincipal.TabIndex = 5;
            menuStripPrincipal.Text = "menuStrip1";
            // 
            // mnuArchivo
            // 
            mnuArchivo.DropDownItems.AddRange(new ToolStripItem[] { mnuConfiguracion, toolStripSeparator1, mnuSalir });
            mnuArchivo.ForeColor = Color.White;
            mnuArchivo.Name = "mnuArchivo";
            mnuArchivo.Size = new Size(60, 20);
            mnuArchivo.Text = "Archivo";
            // 
            // mnuConfiguracion
            // 
            mnuConfiguracion.DropDownItems.AddRange(new ToolStripItem[] { mnuTenant, mnuUsuarios, mnuBackup });
            mnuConfiguracion.Name = "mnuConfiguracion";
            mnuConfiguracion.Size = new Size(180, 22);
            mnuConfiguracion.Text = "Configuración";
            // 
            // mnuTenant
            // 
            mnuTenant.Name = "mnuTenant";
            mnuTenant.Size = new Size(119, 22);
            mnuTenant.Text = "Tenant";
            // 
            // mnuUsuarios
            // 
            mnuUsuarios.Name = "mnuUsuarios";
            mnuUsuarios.Size = new Size(119, 22);
            mnuUsuarios.Text = "Usuarios";
            // 
            // mnuBackup
            // 
            mnuBackup.Name = "mnuBackup";
            mnuBackup.Size = new Size(119, 22);
            mnuBackup.Text = "Backup";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(177, 6);
            // 
            // mnuSalir
            // 
            mnuSalir.Name = "mnuSalir";
            mnuSalir.Size = new Size(180, 22);
            mnuSalir.Text = "Salir";
            // 
            // mnuMaestros
            // 
            mnuMaestros.DropDownItems.AddRange(new ToolStripItem[] { mnuProductos, mnuCategorias, mnuMarcas, mnuUnidades, mnuProveedores, mnuClientes, mnuBodegas });
            mnuMaestros.ForeColor = Color.White;
            mnuMaestros.Name = "mnuMaestros";
            mnuMaestros.Size = new Size(67, 20);
            mnuMaestros.Text = "Maestros";
            // 
            // mnuProductos
            // 
            mnuProductos.Name = "mnuProductos";
            mnuProductos.Size = new Size(180, 22);
            mnuProductos.Text = "Productos";
            // 
            // mnuCategorias
            // 
            mnuCategorias.Name = "mnuCategorias";
            mnuCategorias.Size = new Size(180, 22);
            mnuCategorias.Text = "Categorías";
            // 
            // mnuMarcas
            // 
            mnuMarcas.Name = "mnuMarcas";
            mnuMarcas.Size = new Size(180, 22);
            mnuMarcas.Text = "Marcas";
            // 
            // mnuUnidades
            // 
            mnuUnidades.Name = "mnuUnidades";
            mnuUnidades.Size = new Size(180, 22);
            mnuUnidades.Text = "Unidades";
            // 
            // mnuProveedores
            // 
            mnuProveedores.Name = "mnuProveedores";
            mnuProveedores.Size = new Size(180, 22);
            mnuProveedores.Text = "Proveedores";
            // 
            // mnuClientes
            // 
            mnuClientes.Name = "mnuClientes";
            mnuClientes.Size = new Size(180, 22);
            mnuClientes.Text = "Clientes";
            // 
            // mnuBodegas
            // 
            mnuBodegas.Name = "mnuBodegas";
            mnuBodegas.Size = new Size(180, 22);
            mnuBodegas.Text = "Bodegas";
            // 
            // mnuInventario
            // 
            mnuInventario.DropDownItems.AddRange(new ToolStripItem[] { mnuStock, mnuAjustes, mnuTransferencias });
            mnuInventario.ForeColor = Color.White;
            mnuInventario.Name = "mnuInventario";
            mnuInventario.Size = new Size(72, 20);
            mnuInventario.Text = "Inventario";
            // 
            // mnuStock
            // 
            mnuStock.Name = "mnuStock";
            mnuStock.Size = new Size(180, 22);
            mnuStock.Text = "Stock";
            // 
            // mnuAjustes
            // 
            mnuAjustes.Name = "mnuAjustes";
            mnuAjustes.Size = new Size(180, 22);
            mnuAjustes.Text = "Ajustes";
            // 
            // mnuTransferencias
            // 
            mnuTransferencias.Name = "mnuTransferencias";
            mnuTransferencias.Size = new Size(180, 22);
            mnuTransferencias.Text = "Transferencias";
            // 
            // mnuCompras
            // 
            mnuCompras.DropDownItems.AddRange(new ToolStripItem[] { mnuOrdenCompra, mnuRecepcion });
            mnuCompras.ForeColor = Color.White;
            mnuCompras.Name = "mnuCompras";
            mnuCompras.Size = new Size(67, 20);
            mnuCompras.Text = "Compras";
            // 
            // mnuOrdenCompra
            // 
            mnuOrdenCompra.Name = "mnuOrdenCompra";
            mnuOrdenCompra.Size = new Size(180, 22);
            mnuOrdenCompra.Text = "Ordenes de Compra";
            // 
            // mnuRecepcion
            // 
            mnuRecepcion.Name = "mnuRecepcion";
            mnuRecepcion.Size = new Size(180, 22);
            mnuRecepcion.Text = "Recepción";
            // 
            // mnuVentas
            // 
            mnuVentas.DropDownItems.AddRange(new ToolStripItem[] { mnuPOS, mnuClientesVentas });
            mnuVentas.ForeColor = Color.White;
            mnuVentas.Name = "mnuVentas";
            mnuVentas.Size = new Size(53, 20);
            mnuVentas.Text = "Ventas";
            // 
            // mnuPOS
            // 
            mnuPOS.Name = "mnuPOS";
            mnuPOS.Size = new Size(154, 22);
            mnuPOS.Text = "Punto de Venta";
            // 
            // mnuClientesVentas
            // 
            mnuClientesVentas.Name = "mnuClientesVentas";
            mnuClientesVentas.Size = new Size(154, 22);
            mnuClientesVentas.Text = "Clientes";
            // 
            // mnuReportes
            // 
            mnuReportes.DropDownItems.AddRange(new ToolStripItem[] { mnuReporteStock, mnuReporteVentas, mnuReporteCompras, mnuReporteUtilidad });
            mnuReportes.ForeColor = Color.White;
            mnuReportes.Name = "mnuReportes";
            mnuReportes.Size = new Size(65, 20);
            mnuReportes.Text = "Reportes";
            // 
            // mnuReporteStock
            // 
            mnuReporteStock.Name = "mnuReporteStock";
            mnuReporteStock.Size = new Size(122, 22);
            mnuReporteStock.Text = "Stock";
            // 
            // mnuReporteVentas
            // 
            mnuReporteVentas.Name = "mnuReporteVentas";
            mnuReporteVentas.Size = new Size(122, 22);
            mnuReporteVentas.Text = "Ventas";
            // 
            // mnuReporteCompras
            // 
            mnuReporteCompras.Name = "mnuReporteCompras";
            mnuReporteCompras.Size = new Size(122, 22);
            mnuReporteCompras.Text = "Compras";
            // 
            // mnuReporteUtilidad
            // 
            mnuReporteUtilidad.Name = "mnuReporteUtilidad";
            mnuReporteUtilidad.Size = new Size(122, 22);
            mnuReporteUtilidad.Text = "Utilidad";
            // 
            // mnuAyuda
            // 
            mnuAyuda.DropDownItems.AddRange(new ToolStripItem[] { mnuAcercaDe });
            mnuAyuda.ForeColor = Color.White;
            mnuAyuda.Name = "mnuAyuda";
            mnuAyuda.Size = new Size(53, 20);
            mnuAyuda.Text = "Ayuda";
            // 
            // mnuAcercaDe
            // 
            mnuAcercaDe.Name = "mnuAcercaDe";
            mnuAcercaDe.Size = new Size(135, 22);
            mnuAcercaDe.Text = "Acerca de...";
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.FixedPanel = FixedPanel.Panel1;
            splitContainer.Location = new Point(3, 117);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(treeViewMenu);
            splitContainer.Panel1MinSize = 200;
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(panelDashboard);
            splitContainer.Size = new Size(1194, 550);
            splitContainer.SplitterDistance = 250;
            splitContainer.TabIndex = 1;
            // 
            // treeViewMenu
            // 
            treeViewMenu.BackColor = Color.FromArgb(245, 247, 250);
            treeViewMenu.BorderStyle = BorderStyle.None;
            treeViewMenu.Dock = DockStyle.Fill;
            treeViewMenu.Font = new Font("Segoe UI", 10F);
            treeViewMenu.FullRowSelect = true;
            treeViewMenu.HideSelection = false;
            treeViewMenu.HotTracking = true;
            treeViewMenu.Indent = 20;
            treeViewMenu.ItemHeight = 30;
            treeViewMenu.Location = new Point(0, 0);
            treeViewMenu.Name = "treeViewMenu";
            treeViewMenu.ShowLines = false;
            treeViewMenu.ShowRootLines = false;
            treeViewMenu.Size = new Size(250, 550);
            treeViewMenu.TabIndex = 0;
            // 
            // panelDashboard
            // 
            panelDashboard.BackColor = Color.White;
            panelDashboard.Controls.Add(tlpDashboard);
            panelDashboard.Dock = DockStyle.Fill;
            panelDashboard.Location = new Point(0, 0);
            panelDashboard.Name = "panelDashboard";
            panelDashboard.Padding = new Padding(10);
            panelDashboard.Size = new Size(940, 550);
            panelDashboard.TabIndex = 0;
            // 
            // tlpDashboard
            // 
            tlpDashboard.ColumnCount = 2;
            tlpDashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpDashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpDashboard.Controls.Add(gpbResumen, 0, 0);
            tlpDashboard.Controls.Add(gpbAlertas, 1, 0);
            tlpDashboard.Controls.Add(gpbAccionesRapidas, 0, 1);
            tlpDashboard.Controls.Add(dgvProductosCriticos, 0, 2);
            tlpDashboard.Controls.Add(dgvUltimosMovimientos, 1, 2);
            tlpDashboard.Dock = DockStyle.Fill;
            tlpDashboard.Location = new Point(10, 10);
            tlpDashboard.Name = "tlpDashboard";
            tlpDashboard.RowCount = 3;
            tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpDashboard.Size = new Size(920, 530);
            tlpDashboard.TabIndex = 0;
            // 
            // gpbResumen
            // 
            gpbResumen.Controls.Add(tlpResumen);
            gpbResumen.Dock = DockStyle.Fill;
            gpbResumen.Location = new Point(3, 3);
            gpbResumen.Name = "gpbResumen";
            gpbResumen.Padding = new Padding(10);
            gpbResumen.Size = new Size(454, 114);
            gpbResumen.TabIndex = 0;
            gpbResumen.TabStop = false;
            gpbResumen.Text = "Resumen del Día";
            // 
            // tlpResumen
            // 
            tlpResumen.ColumnCount = 4;
            tlpResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpResumen.Controls.Add(lblTotalProductos, 0, 0);
            tlpResumen.Controls.Add(lblTotalProductosValor, 0, 1);
            tlpResumen.Controls.Add(lblStockBajo, 1, 0);
            tlpResumen.Controls.Add(lblStockBajoValor, 1, 1);
            tlpResumen.Controls.Add(lblVentasHoy, 2, 0);
            tlpResumen.Controls.Add(lblVentasHoyValor, 2, 1);
            tlpResumen.Controls.Add(lblComprasHoy, 3, 0);
            tlpResumen.Controls.Add(lblComprasHoyValor, 3, 1);
            tlpResumen.Controls.Add(lblClientes, 0, 2);
            tlpResumen.Controls.Add(lblClientesValor, 0, 3);
            tlpResumen.Controls.Add(lblProveedores, 1, 2);
            tlpResumen.Controls.Add(lblProveedoresValor, 1, 3);
            tlpResumen.Dock = DockStyle.Fill;
            tlpResumen.Location = new Point(10, 26);
            tlpResumen.Name = "tlpResumen";
            tlpResumen.RowCount = 4;
            tlpResumen.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tlpResumen.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tlpResumen.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tlpResumen.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tlpResumen.Size = new Size(434, 78);
            tlpResumen.TabIndex = 0;
            // 
            // lblTotalProductos
            // 
            lblTotalProductos.Dock = DockStyle.Fill;
            lblTotalProductos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotalProductos.Location = new Point(3, 0);
            lblTotalProductos.Name = "lblTotalProductos";
            lblTotalProductos.Size = new Size(102, 19);
            lblTotalProductos.TabIndex = 0;
            lblTotalProductos.Text = "Total Productos:";
            lblTotalProductos.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTotalProductosValor
            // 
            lblTotalProductosValor.Dock = DockStyle.Fill;
            lblTotalProductosValor.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTotalProductosValor.ForeColor = Color.FromArgb(30, 60, 114);
            lblTotalProductosValor.Location = new Point(3, 19);
            lblTotalProductosValor.Name = "lblTotalProductosValor";
            lblTotalProductosValor.Size = new Size(102, 19);
            lblTotalProductosValor.TabIndex = 1;
            lblTotalProductosValor.Text = "0";
            lblTotalProductosValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblStockBajo
            // 
            lblStockBajo.Dock = DockStyle.Fill;
            lblStockBajo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStockBajo.Location = new Point(111, 0);
            lblStockBajo.Name = "lblStockBajo";
            lblStockBajo.Size = new Size(102, 19);
            lblStockBajo.TabIndex = 2;
            lblStockBajo.Text = "Stock Bajo:";
            lblStockBajo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblStockBajoValor
            // 
            lblStockBajoValor.Dock = DockStyle.Fill;
            lblStockBajoValor.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblStockBajoValor.ForeColor = Color.FromArgb(192, 57, 43);
            lblStockBajoValor.Location = new Point(111, 19);
            lblStockBajoValor.Name = "lblStockBajoValor";
            lblStockBajoValor.Size = new Size(102, 19);
            lblStockBajoValor.TabIndex = 3;
            lblStockBajoValor.Text = "0";
            lblStockBajoValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblVentasHoy
            // 
            lblVentasHoy.Dock = DockStyle.Fill;
            lblVentasHoy.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblVentasHoy.Location = new Point(219, 0);
            lblVentasHoy.Name = "lblVentasHoy";
            lblVentasHoy.Size = new Size(102, 19);
            lblVentasHoy.TabIndex = 4;
            lblVentasHoy.Text = "Ventas Hoy:";
            lblVentasHoy.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblVentasHoyValor
            // 
            lblVentasHoyValor.Dock = DockStyle.Fill;
            lblVentasHoyValor.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblVentasHoyValor.ForeColor = Color.FromArgb(39, 174, 96);
            lblVentasHoyValor.Location = new Point(219, 19);
            lblVentasHoyValor.Name = "lblVentasHoyValor";
            lblVentasHoyValor.Size = new Size(102, 19);
            lblVentasHoyValor.TabIndex = 5;
            lblVentasHoyValor.Text = "$0";
            lblVentasHoyValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblComprasHoy
            // 
            lblComprasHoy.Dock = DockStyle.Fill;
            lblComprasHoy.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblComprasHoy.Location = new Point(327, 0);
            lblComprasHoy.Name = "lblComprasHoy";
            lblComprasHoy.Size = new Size(104, 19);
            lblComprasHoy.TabIndex = 6;
            lblComprasHoy.Text = "Compras Hoy:";
            lblComprasHoy.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblComprasHoyValor
            // 
            lblComprasHoyValor.Dock = DockStyle.Fill;
            lblComprasHoyValor.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblComprasHoyValor.ForeColor = Color.FromArgb(241, 176, 53);
            lblComprasHoyValor.Location = new Point(327, 19);
            lblComprasHoyValor.Name = "lblComprasHoyValor";
            lblComprasHoyValor.Size = new Size(104, 19);
            lblComprasHoyValor.TabIndex = 7;
            lblComprasHoyValor.Text = "$0";
            lblComprasHoyValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblClientes
            // 
            lblClientes.Dock = DockStyle.Fill;
            lblClientes.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClientes.Location = new Point(3, 38);
            lblClientes.Name = "lblClientes";
            lblClientes.Size = new Size(102, 19);
            lblClientes.TabIndex = 8;
            lblClientes.Text = "Clientes:";
            lblClientes.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblClientesValor
            // 
            lblClientesValor.Dock = DockStyle.Fill;
            lblClientesValor.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblClientesValor.Location = new Point(3, 57);
            lblClientesValor.Name = "lblClientesValor";
            lblClientesValor.Size = new Size(102, 21);
            lblClientesValor.TabIndex = 9;
            lblClientesValor.Text = "0";
            lblClientesValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblProveedores
            // 
            lblProveedores.Dock = DockStyle.Fill;
            lblProveedores.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblProveedores.Location = new Point(111, 38);
            lblProveedores.Name = "lblProveedores";
            lblProveedores.Size = new Size(102, 19);
            lblProveedores.TabIndex = 10;
            lblProveedores.Text = "Proveedores:";
            lblProveedores.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblProveedoresValor
            // 
            lblProveedoresValor.Dock = DockStyle.Fill;
            lblProveedoresValor.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblProveedoresValor.Location = new Point(111, 57);
            lblProveedoresValor.Name = "lblProveedoresValor";
            lblProveedoresValor.Size = new Size(102, 21);
            lblProveedoresValor.TabIndex = 11;
            lblProveedoresValor.Text = "0";
            lblProveedoresValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // gpbAlertas
            // 
            gpbAlertas.Controls.Add(lstAlertas);
            gpbAlertas.Dock = DockStyle.Fill;
            gpbAlertas.Location = new Point(463, 3);
            gpbAlertas.Name = "gpbAlertas";
            gpbAlertas.Padding = new Padding(10);
            gpbAlertas.Size = new Size(454, 114);
            gpbAlertas.TabIndex = 1;
            gpbAlertas.TabStop = false;
            gpbAlertas.Text = "Alertas";
            // 
            // lstAlertas
            // 
            lstAlertas.BackColor = Color.FromArgb(255, 249, 249);
            lstAlertas.BorderStyle = BorderStyle.None;
            lstAlertas.Dock = DockStyle.Fill;
            lstAlertas.Font = new Font("Segoe UI", 9F);
            lstAlertas.FormattingEnabled = true;
            lstAlertas.Location = new Point(10, 26);
            lstAlertas.Name = "lstAlertas";
            lstAlertas.Size = new Size(434, 78);
            lstAlertas.TabIndex = 0;
            // 
            // gpbAccionesRapidas
            // 
            gpbAccionesRapidas.Controls.Add(flpAcciones);
            gpbAccionesRapidas.Dock = DockStyle.Fill;
            gpbAccionesRapidas.Location = new Point(3, 123);
            gpbAccionesRapidas.Name = "gpbAccionesRapidas";
            gpbAccionesRapidas.Padding = new Padding(10);
            gpbAccionesRapidas.Size = new Size(454, 94);
            gpbAccionesRapidas.TabIndex = 2;
            gpbAccionesRapidas.TabStop = false;
            gpbAccionesRapidas.Text = "Acciones Rápidas";
            // 
            // flpAcciones
            // 
            flpAcciones.Controls.Add(btnNuevoProducto);
            flpAcciones.Controls.Add(btnNuevaVenta);
            flpAcciones.Controls.Add(btnNuevaCompra);
            flpAcciones.Controls.Add(btnAjusteStock);
            flpAcciones.Controls.Add(btnReporteDiario);
            flpAcciones.Dock = DockStyle.Fill;
            flpAcciones.Location = new Point(10, 26);
            flpAcciones.Name = "flpAcciones";
            flpAcciones.Padding = new Padding(5);
            flpAcciones.Size = new Size(434, 58);
            flpAcciones.TabIndex = 0;
            // 
            // btnNuevoProducto
            // 
            btnNuevoProducto.BackColor = Color.FromArgb(30, 60, 114);
            btnNuevoProducto.FlatStyle = FlatStyle.Flat;
            btnNuevoProducto.ForeColor = Color.White;
            btnNuevoProducto.Location = new Point(8, 8);
            btnNuevoProducto.Name = "btnNuevoProducto";
            btnNuevoProducto.Size = new Size(80, 40);
            btnNuevoProducto.TabIndex = 0;
            btnNuevoProducto.Text = "Producto";
            btnNuevoProducto.UseVisualStyleBackColor = false;
            // 
            // btnNuevaVenta
            // 
            btnNuevaVenta.BackColor = Color.FromArgb(39, 174, 96);
            btnNuevaVenta.FlatStyle = FlatStyle.Flat;
            btnNuevaVenta.ForeColor = Color.White;
            btnNuevaVenta.Location = new Point(94, 8);
            btnNuevaVenta.Name = "btnNuevaVenta";
            btnNuevaVenta.Size = new Size(80, 40);
            btnNuevaVenta.TabIndex = 1;
            btnNuevaVenta.Text = "Venta";
            btnNuevaVenta.UseVisualStyleBackColor = false;
            // 
            // btnNuevaCompra
            // 
            btnNuevaCompra.BackColor = Color.FromArgb(241, 176, 53);
            btnNuevaCompra.FlatStyle = FlatStyle.Flat;
            btnNuevaCompra.ForeColor = Color.White;
            btnNuevaCompra.Location = new Point(180, 8);
            btnNuevaCompra.Name = "btnNuevaCompra";
            btnNuevaCompra.Size = new Size(80, 40);
            btnNuevaCompra.TabIndex = 2;
            btnNuevaCompra.Text = "Compra";
            btnNuevaCompra.UseVisualStyleBackColor = false;
            // 
            // btnAjusteStock
            // 
            btnAjusteStock.BackColor = Color.FromArgb(52, 73, 94);
            btnAjusteStock.FlatStyle = FlatStyle.Flat;
            btnAjusteStock.ForeColor = Color.White;
            btnAjusteStock.Location = new Point(266, 8);
            btnAjusteStock.Name = "btnAjusteStock";
            btnAjusteStock.Size = new Size(80, 40);
            btnAjusteStock.TabIndex = 3;
            btnAjusteStock.Text = "Ajuste";
            btnAjusteStock.UseVisualStyleBackColor = false;
            // 
            // btnReporteDiario
            // 
            btnReporteDiario.BackColor = Color.FromArgb(155, 89, 182);
            btnReporteDiario.FlatStyle = FlatStyle.Flat;
            btnReporteDiario.ForeColor = Color.White;
            btnReporteDiario.Location = new Point(8, 54);
            btnReporteDiario.Name = "btnReporteDiario";
            btnReporteDiario.Size = new Size(80, 40);
            btnReporteDiario.TabIndex = 4;
            btnReporteDiario.Text = "Reporte";
            btnReporteDiario.UseVisualStyleBackColor = false;
            // 
            // dgvProductosCriticos
            // 
            dgvProductosCriticos.AllowUserToAddRows = false;
            dgvProductosCriticos.AllowUserToDeleteRows = false;
            dgvProductosCriticos.BackgroundColor = Color.White;
            dgvProductosCriticos.BorderStyle = BorderStyle.None;
            dgvProductosCriticos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(30, 60, 114);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dgvProductosCriticos.DefaultCellStyle = dataGridViewCellStyle1;
            dgvProductosCriticos.Dock = DockStyle.Fill;
            dgvProductosCriticos.Location = new Point(3, 223);
            dgvProductosCriticos.Name = "dgvProductosCriticos";
            dgvProductosCriticos.ReadOnly = true;
            dgvProductosCriticos.RowHeadersVisible = false;
            dgvProductosCriticos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductosCriticos.Size = new Size(454, 304);
            dgvProductosCriticos.TabIndex = 3;
            dgvProductosCriticos.Text = "Productos con Stock Crítico";
            // 
            // dgvUltimosMovimientos
            // 
            dgvUltimosMovimientos.AllowUserToAddRows = false;
            dgvUltimosMovimientos.AllowUserToDeleteRows = false;
            dgvUltimosMovimientos.BackgroundColor = Color.White;
            dgvUltimosMovimientos.BorderStyle = BorderStyle.None;
            dgvUltimosMovimientos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(30, 60, 114);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvUltimosMovimientos.DefaultCellStyle = dataGridViewCellStyle2;
            dgvUltimosMovimientos.Dock = DockStyle.Fill;
            dgvUltimosMovimientos.Location = new Point(463, 223);
            dgvUltimosMovimientos.Name = "dgvUltimosMovimientos";
            dgvUltimosMovimientos.ReadOnly = true;
            dgvUltimosMovimientos.RowHeadersVisible = false;
            dgvUltimosMovimientos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUltimosMovimientos.Size = new Size(454, 304);
            dgvUltimosMovimientos.TabIndex = 4;
            dgvUltimosMovimientos.Text = "Últimos Movimientos";
            // 
            // panelInferior
            // 
            panelInferior.BackColor = Color.FromArgb(52, 73, 94);
            panelInferior.Controls.Add(statusStrip);
            panelInferior.Dock = DockStyle.Fill;
            panelInferior.Location = new Point(3, 673);
            panelInferior.Name = "panelInferior";
            panelInferior.Size = new Size(1194, 24);
            panelInferior.TabIndex = 2;
            // 
            // statusStrip
            // 
            statusStrip.BackColor = Color.FromArgb(52, 73, 94);
            statusStrip.Items.AddRange(new ToolStripItem[] { tsslUsuario, tsslTenant, tsslFecha, tsslVersion });
            statusStrip.Location = new Point(0, 2);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1194, 22);
            statusStrip.TabIndex = 0;
            statusStrip.Text = "statusStrip1";
            // 
            // tsslUsuario
            // 
            tsslUsuario.ForeColor = Color.White;
            tsslUsuario.Name = "tsslUsuario";
            tsslUsuario.Size = new Size(89, 17);
            tsslUsuario.Text = "Usuario: Admin";
            // 
            // tsslTenant
            // 
            tsslTenant.ForeColor = Color.White;
            tsslTenant.Name = "tsslTenant";
            tsslTenant.Size = new Size(82, 17);
            tsslTenant.Text = "Tenant: DEMO";
            // 
            // tsslFecha
            // 
            tsslFecha.ForeColor = Color.White;
            tsslFecha.Name = "tsslFecha";
            tsslFecha.Size = new Size(95, 17);
            tsslFecha.Text = "24/02/2026 14:30";
            // 
            // tsslVersion
            // 
            tsslVersion.ForeColor = Color.White;
            tsslVersion.Name = "tsslVersion";
            tsslVersion.Size = new Size(63, 17);
            tsslVersion.Text = "Versión 1.0";
            // 
            // timerReloj
            // 
            timerReloj.Enabled = true;
            timerReloj.Interval = 1000;
            timerReloj.Tick += TimerReloj_Tick;
            // 
            // frm_Principal
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 700);
            Controls.Add(tlpMain);
            Font = new Font("Segoe UI", 9F);
            MainMenuStrip = menuStripPrincipal;
            MinimumSize = new Size(1024, 600);
            Name = "frm_Principal";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Sistema de Inventario Multi-Tenant";
            WindowState = FormWindowState.Maximized;
            tlpMain.ResumeLayout(false);
            panelSuperior.ResumeLayout(false);
            panelSuperior.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).EndInit();
            menuStripPrincipal.ResumeLayout(false);
            menuStripPrincipal.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            panelDashboard.ResumeLayout(false);
            tlpDashboard.ResumeLayout(false);
            gpbResumen.ResumeLayout(false);
            tlpResumen.ResumeLayout(false);
            gpbAlertas.ResumeLayout(false);
            gpbAccionesRapidas.ResumeLayout(false);
            flpAcciones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvProductosCriticos).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvUltimosMovimientos).EndInit();
            panelInferior.ResumeLayout(false);
            panelInferior.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
        }

        private ToolStripSeparator toolStripSeparator1;
    }
}