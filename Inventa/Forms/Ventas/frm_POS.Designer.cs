namespace Inventa.Forms.Ventas
{
    partial class frm_POS
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Panel panelSuperior;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblFecha;
        private System.Windows.Forms.Label lblCaja;
        private System.Windows.Forms.Label lblCajaValor;
        private System.Windows.Forms.Panel panelIzquierdo;
        private System.Windows.Forms.GroupBox gpbBusqueda;
        private System.Windows.Forms.TableLayoutPanel tlpBusqueda;
        private System.Windows.Forms.Label lblCodigoBarra;
        private System.Windows.Forms.TextBox txtCodigoBarra;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.Label lblProducto;
        private System.Windows.Forms.TextBox txtProductoBusqueda;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.GroupBox gpbCarrito;
        private System.Windows.Forms.DataGridView dgvCarrito;
        private System.Windows.Forms.Panel panelTotales;
        private System.Windows.Forms.TableLayoutPanel tlpTotales;
        private System.Windows.Forms.Label lblSubtotal;
        private System.Windows.Forms.Label lblSubtotalValor;
        private System.Windows.Forms.Label lblDescuento;
        private System.Windows.Forms.NumericUpDown numDescuento;
        private System.Windows.Forms.Label lblImpuesto;
        private System.Windows.Forms.Label lblImpuestoValor;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblTotalValor;
        private System.Windows.Forms.Panel panelDerecho;
        private System.Windows.Forms.GroupBox gpbCliente;
        private System.Windows.Forms.TableLayoutPanel tlpCliente;
        private System.Windows.Forms.Label lblCliente;
        private System.Windows.Forms.ComboBox cboCliente;
        private System.Windows.Forms.Button btnNuevoCliente;
        private System.Windows.Forms.Label lblRUT;
        private System.Windows.Forms.Label lblRUTValor;
        private System.Windows.Forms.Label lblTelefono;
        private System.Windows.Forms.Label lblTelefonoValor;
        private System.Windows.Forms.GroupBox gpbPago;
        private System.Windows.Forms.TableLayoutPanel tlpPago;
        private System.Windows.Forms.Label lblMetodoPago;
        private System.Windows.Forms.ComboBox cboMetodoPago;
        private System.Windows.Forms.Label lblMontoRecibido;
        private System.Windows.Forms.NumericUpDown numMontoRecibido;
        private System.Windows.Forms.Label lblCambio;
        private System.Windows.Forms.Label lblCambioValor;
        private System.Windows.Forms.GroupBox gpbResumen;
        private System.Windows.Forms.TableLayoutPanel tlpResumen;
        private System.Windows.Forms.Label lblItems;
        private System.Windows.Forms.Label lblItemsValor;
        private System.Windows.Forms.Label lblCantidadTotal;
        private System.Windows.Forms.Label lblCantidadTotalValor;
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.Button btnPagar;
        private System.Windows.Forms.Button btnCancelarVenta;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tsslUsuario;
        private System.Windows.Forms.ToolStripStatusLabel tsslCaja;
        private System.Windows.Forms.ToolStripStatusLabel tsslEstado;

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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            tlpMain = new TableLayoutPanel();
            panelSuperior = new Panel();
            lblTitulo = new Label();
            lblFecha = new Label();
            lblCaja = new Label();
            lblCajaValor = new Label();
            panelIzquierdo = new Panel();
            gpbCarrito = new GroupBox();
            dgvCarrito = new DataGridView();
            panelTotales = new Panel();
            tlpTotales = new TableLayoutPanel();
            lblSubtotal = new Label();
            lblSubtotalValor = new Label();
            lblDescuento = new Label();
            numDescuento = new NumericUpDown();
            lblImpuesto = new Label();
            lblImpuestoValor = new Label();
            lblTotal = new Label();
            lblTotalValor = new Label();
            gpbBusqueda = new GroupBox();
            tlpBusqueda = new TableLayoutPanel();
            lblCodigoBarra = new Label();
            txtCodigoBarra = new TextBox();
            btnBuscar = new Button();
            lblProducto = new Label();
            txtProductoBusqueda = new TextBox();
            btnAgregar = new Button();
            panelDerecho = new Panel();
            gpbResumen = new GroupBox();
            tlpResumen = new TableLayoutPanel();
            lblItems = new Label();
            lblItemsValor = new Label();
            lblCantidadTotal = new Label();
            lblCantidadTotalValor = new Label();
            gpbCliente = new GroupBox();
            tlpCliente = new TableLayoutPanel();
            lblCliente = new Label();
            cboCliente = new ComboBox();
            btnNuevoCliente = new Button();
            lblRUT = new Label();
            lblRUTValor = new Label();
            lblTelefono = new Label();
            lblTelefonoValor = new Label();
            gpbPago = new GroupBox();
            tlpPago = new TableLayoutPanel();
            lblMetodoPago = new Label();
            cboMetodoPago = new ComboBox();
            lblMontoRecibido = new Label();
            numMontoRecibido = new NumericUpDown();
            lblCambio = new Label();
            lblCambioValor = new Label();
            panelBotones = new Panel();
            btnPagar = new Button();
            btnCancelarVenta = new Button();
            btnLimpiar = new Button();
            btnCerrar = new Button();
            statusStrip = new StatusStrip();
            tsslUsuario = new ToolStripStatusLabel();
            tsslCaja = new ToolStripStatusLabel();
            tsslEstado = new ToolStripStatusLabel();
            tlpMain.SuspendLayout();
            panelSuperior.SuspendLayout();
            panelIzquierdo.SuspendLayout();
            gpbCarrito.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCarrito).BeginInit();
            panelTotales.SuspendLayout();
            tlpTotales.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numDescuento).BeginInit();
            gpbBusqueda.SuspendLayout();
            tlpBusqueda.SuspendLayout();
            panelDerecho.SuspendLayout();
            gpbResumen.SuspendLayout();
            tlpResumen.SuspendLayout();
            gpbCliente.SuspendLayout();
            tlpCliente.SuspendLayout();
            gpbPago.SuspendLayout();
            tlpPago.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMontoRecibido).BeginInit();
            panelBotones.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 2;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tlpMain.Controls.Add(panelSuperior, 0, 0);
            tlpMain.Controls.Add(panelIzquierdo, 0, 1);
            tlpMain.Controls.Add(panelDerecho, 1, 1);
            tlpMain.Controls.Add(panelBotones, 0, 2);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            tlpMain.Size = new Size(1200, 678);
            tlpMain.TabIndex = 0;
            // 
            // panelSuperior
            // 
            panelSuperior.BackColor = Color.FromArgb(30, 60, 114);
            tlpMain.SetColumnSpan(panelSuperior, 2);
            panelSuperior.Controls.Add(lblTitulo);
            panelSuperior.Controls.Add(lblFecha);
            panelSuperior.Controls.Add(lblCaja);
            panelSuperior.Controls.Add(lblCajaValor);
            panelSuperior.Dock = DockStyle.Fill;
            panelSuperior.Location = new Point(3, 3);
            panelSuperior.Name = "panelSuperior";
            panelSuperior.Size = new Size(1194, 54);
            panelSuperior.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(15, 12);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(218, 32);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "PUNTO DE VENTA";
            // 
            // lblFecha
            // 
            lblFecha.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblFecha.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFecha.ForeColor = Color.White;
            lblFecha.Location = new Point(900, 17);
            lblFecha.Name = "lblFecha";
            lblFecha.Size = new Size(150, 20);
            lblFecha.TabIndex = 1;
            lblFecha.Text = "24/02/2026 14:30";
            lblFecha.TextAlign = ContentAlignment.TopRight;
            // 
            // lblCaja
            // 
            lblCaja.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblCaja.Font = new Font("Segoe UI", 10F);
            lblCaja.ForeColor = Color.White;
            lblCaja.Location = new Point(1050, 17);
            lblCaja.Name = "lblCaja";
            lblCaja.Size = new Size(40, 20);
            lblCaja.TabIndex = 2;
            lblCaja.Text = "Caja:";
            // 
            // lblCajaValor
            // 
            lblCajaValor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblCajaValor.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCajaValor.ForeColor = Color.FromArgb(255, 255, 150);
            lblCajaValor.Location = new Point(1090, 17);
            lblCajaValor.Name = "lblCajaValor";
            lblCajaValor.Size = new Size(100, 20);
            lblCajaValor.TabIndex = 3;
            lblCajaValor.Text = "CAJA 001";
            // 
            // panelIzquierdo
            // 
            panelIzquierdo.Controls.Add(gpbCarrito);
            panelIzquierdo.Controls.Add(panelTotales);
            panelIzquierdo.Controls.Add(gpbBusqueda);
            panelIzquierdo.Dock = DockStyle.Fill;
            panelIzquierdo.Location = new Point(3, 63);
            panelIzquierdo.Name = "panelIzquierdo";
            panelIzquierdo.Padding = new Padding(5);
            panelIzquierdo.Size = new Size(774, 542);
            panelIzquierdo.TabIndex = 1;
            // 
            // gpbCarrito
            // 
            gpbCarrito.Controls.Add(dgvCarrito);
            gpbCarrito.Dock = DockStyle.Fill;
            gpbCarrito.Location = new Point(5, 115);
            gpbCarrito.Name = "gpbCarrito";
            gpbCarrito.Padding = new Padding(5);
            gpbCarrito.Size = new Size(764, 332);
            gpbCarrito.TabIndex = 2;
            gpbCarrito.TabStop = false;
            gpbCarrito.Text = "Carrito de Compras";
            // 
            // dgvCarrito
            // 
            dgvCarrito.AllowUserToAddRows = false;
            dgvCarrito.AllowUserToDeleteRows = false;
            dgvCarrito.BackgroundColor = Color.White;
            dgvCarrito.BorderStyle = BorderStyle.None;
            dgvCarrito.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(30, 60, 114);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dgvCarrito.DefaultCellStyle = dataGridViewCellStyle1;
            dgvCarrito.Dock = DockStyle.Fill;
            dgvCarrito.Location = new Point(5, 23);
            dgvCarrito.Name = "dgvCarrito";
            dgvCarrito.ReadOnly = true;
            dgvCarrito.RowHeadersVisible = false;
            dgvCarrito.RowTemplate.Height = 30;
            dgvCarrito.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCarrito.Size = new Size(754, 304);
            dgvCarrito.TabIndex = 0;
            // 
            // panelTotales
            // 
            panelTotales.BackColor = Color.FromArgb(245, 247, 250);
            panelTotales.BorderStyle = BorderStyle.FixedSingle;
            panelTotales.Controls.Add(tlpTotales);
            panelTotales.Dock = DockStyle.Bottom;
            panelTotales.Location = new Point(5, 447);
            panelTotales.Name = "panelTotales";
            panelTotales.Size = new Size(764, 90);
            panelTotales.TabIndex = 1;
            // 
            // tlpTotales
            // 
            tlpTotales.ColumnCount = 4;
            tlpTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tlpTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tlpTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpTotales.Controls.Add(lblSubtotal, 0, 0);
            tlpTotales.Controls.Add(lblSubtotalValor, 1, 0);
            tlpTotales.Controls.Add(lblDescuento, 2, 0);
            tlpTotales.Controls.Add(numDescuento, 3, 0);
            tlpTotales.Controls.Add(lblImpuesto, 0, 1);
            tlpTotales.Controls.Add(lblImpuestoValor, 1, 1);
            tlpTotales.Controls.Add(lblTotal, 2, 1);
            tlpTotales.Controls.Add(lblTotalValor, 3, 1);
            tlpTotales.Dock = DockStyle.Fill;
            tlpTotales.Location = new Point(0, 0);
            tlpTotales.Name = "tlpTotales";
            tlpTotales.Padding = new Padding(10);
            tlpTotales.RowCount = 2;
            tlpTotales.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpTotales.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpTotales.Size = new Size(762, 88);
            tlpTotales.TabIndex = 0;
            // 
            // lblSubtotal
            // 
            lblSubtotal.Dock = DockStyle.Fill;
            lblSubtotal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSubtotal.Location = new Point(13, 10);
            lblSubtotal.Name = "lblSubtotal";
            lblSubtotal.Size = new Size(74, 34);
            lblSubtotal.TabIndex = 0;
            lblSubtotal.Text = "Subtotal:";
            lblSubtotal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblSubtotalValor
            // 
            lblSubtotalValor.Dock = DockStyle.Fill;
            lblSubtotalValor.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblSubtotalValor.ForeColor = Color.FromArgb(30, 60, 114);
            lblSubtotalValor.Location = new Point(93, 10);
            lblSubtotalValor.Name = "lblSubtotalValor";
            lblSubtotalValor.Size = new Size(226, 34);
            lblSubtotalValor.TabIndex = 1;
            lblSubtotalValor.Text = "$ 0.00";
            lblSubtotalValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDescuento
            // 
            lblDescuento.Dock = DockStyle.Fill;
            lblDescuento.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDescuento.Location = new Point(325, 10);
            lblDescuento.Name = "lblDescuento";
            lblDescuento.Size = new Size(74, 34);
            lblDescuento.TabIndex = 2;
            lblDescuento.Text = "Dto. %:";
            lblDescuento.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numDescuento
            // 
            numDescuento.DecimalPlaces = 2;
            numDescuento.Dock = DockStyle.Fill;
            numDescuento.Location = new Point(405, 13);
            numDescuento.Name = "numDescuento";
            numDescuento.Size = new Size(344, 25);
            numDescuento.TabIndex = 3;
            numDescuento.TextAlign = HorizontalAlignment.Right;
            // 
            // lblImpuesto
            // 
            lblImpuesto.Dock = DockStyle.Fill;
            lblImpuesto.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblImpuesto.Location = new Point(13, 44);
            lblImpuesto.Name = "lblImpuesto";
            lblImpuesto.Size = new Size(74, 34);
            lblImpuesto.TabIndex = 4;
            lblImpuesto.Text = "IVA 19%:";
            lblImpuesto.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblImpuestoValor
            // 
            lblImpuestoValor.Dock = DockStyle.Fill;
            lblImpuestoValor.Font = new Font("Segoe UI", 12F);
            lblImpuestoValor.Location = new Point(93, 44);
            lblImpuestoValor.Name = "lblImpuestoValor";
            lblImpuestoValor.Size = new Size(226, 34);
            lblImpuestoValor.TabIndex = 5;
            lblImpuestoValor.Text = "$ 0.00";
            lblImpuestoValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTotal
            // 
            lblTotal.Dock = DockStyle.Fill;
            lblTotal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTotal.Location = new Point(325, 44);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(74, 34);
            lblTotal.TabIndex = 6;
            lblTotal.Text = "TOTAL:";
            lblTotal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTotalValor
            // 
            lblTotalValor.Dock = DockStyle.Fill;
            lblTotalValor.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTotalValor.ForeColor = Color.FromArgb(30, 60, 114);
            lblTotalValor.Location = new Point(405, 44);
            lblTotalValor.Name = "lblTotalValor";
            lblTotalValor.Size = new Size(344, 34);
            lblTotalValor.TabIndex = 7;
            lblTotalValor.Text = "$ 0.00";
            lblTotalValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // gpbBusqueda
            // 
            gpbBusqueda.Controls.Add(tlpBusqueda);
            gpbBusqueda.Dock = DockStyle.Top;
            gpbBusqueda.Location = new Point(5, 5);
            gpbBusqueda.Name = "gpbBusqueda";
            gpbBusqueda.Padding = new Padding(5);
            gpbBusqueda.Size = new Size(764, 110);
            gpbBusqueda.TabIndex = 0;
            gpbBusqueda.TabStop = false;
            gpbBusqueda.Text = "Búsqueda de Productos";
            // 
            // tlpBusqueda
            // 
            tlpBusqueda.ColumnCount = 4;
            tlpBusqueda.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpBusqueda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpBusqueda.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tlpBusqueda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpBusqueda.Controls.Add(lblCodigoBarra, 0, 0);
            tlpBusqueda.Controls.Add(txtCodigoBarra, 1, 0);
            tlpBusqueda.Controls.Add(btnBuscar, 2, 0);
            tlpBusqueda.Controls.Add(lblProducto, 0, 1);
            tlpBusqueda.Controls.Add(txtProductoBusqueda, 1, 1);
            tlpBusqueda.Controls.Add(btnAgregar, 2, 1);
            tlpBusqueda.Dock = DockStyle.Fill;
            tlpBusqueda.Location = new Point(5, 23);
            tlpBusqueda.Name = "tlpBusqueda";
            tlpBusqueda.Padding = new Padding(5);
            tlpBusqueda.RowCount = 2;
            tlpBusqueda.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpBusqueda.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpBusqueda.Size = new Size(754, 82);
            tlpBusqueda.TabIndex = 0;
            // 
            // lblCodigoBarra
            // 
            lblCodigoBarra.Dock = DockStyle.Fill;
            lblCodigoBarra.Font = new Font("Segoe UI", 10F);
            lblCodigoBarra.Location = new Point(8, 5);
            lblCodigoBarra.Name = "lblCodigoBarra";
            lblCodigoBarra.Size = new Size(94, 36);
            lblCodigoBarra.TabIndex = 0;
            lblCodigoBarra.Text = "Código Barras:";
            lblCodigoBarra.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtCodigoBarra
            // 
            txtCodigoBarra.CharacterCasing = CharacterCasing.Upper;
            txtCodigoBarra.Dock = DockStyle.Fill;
            txtCodigoBarra.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            txtCodigoBarra.Location = new Point(108, 8);
            txtCodigoBarra.Name = "txtCodigoBarra";
            txtCodigoBarra.Size = new Size(332, 29);
            txtCodigoBarra.TabIndex = 1;
            // 
            // btnBuscar
            // 
            btnBuscar.BackColor = Color.FromArgb(52, 73, 94);
            btnBuscar.Dock = DockStyle.Fill;
            btnBuscar.FlatStyle = FlatStyle.Flat;
            btnBuscar.ForeColor = Color.White;
            btnBuscar.Location = new Point(446, 8);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(74, 30);
            btnBuscar.TabIndex = 2;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = false;
            // 
            // lblProducto
            // 
            lblProducto.Dock = DockStyle.Fill;
            lblProducto.Font = new Font("Segoe UI", 10F);
            lblProducto.Location = new Point(8, 41);
            lblProducto.Name = "lblProducto";
            lblProducto.Size = new Size(94, 36);
            lblProducto.TabIndex = 3;
            lblProducto.Text = "Producto:";
            lblProducto.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtProductoBusqueda
            // 
            txtProductoBusqueda.Dock = DockStyle.Fill;
            txtProductoBusqueda.Location = new Point(108, 44);
            txtProductoBusqueda.Name = "txtProductoBusqueda";
            txtProductoBusqueda.Size = new Size(332, 25);
            txtProductoBusqueda.TabIndex = 4;
            // 
            // btnAgregar
            // 
            btnAgregar.BackColor = Color.FromArgb(30, 60, 114);
            btnAgregar.Dock = DockStyle.Fill;
            btnAgregar.FlatStyle = FlatStyle.Flat;
            btnAgregar.ForeColor = Color.White;
            btnAgregar.Location = new Point(446, 44);
            btnAgregar.Name = "btnAgregar";
            btnAgregar.Size = new Size(74, 30);
            btnAgregar.TabIndex = 5;
            btnAgregar.Text = "Agregar";
            btnAgregar.UseVisualStyleBackColor = false;
            // 
            // panelDerecho
            // 
            panelDerecho.Controls.Add(gpbResumen);
            panelDerecho.Controls.Add(gpbCliente);
            panelDerecho.Controls.Add(gpbPago);
            panelDerecho.Dock = DockStyle.Fill;
            panelDerecho.Location = new Point(783, 63);
            panelDerecho.Name = "panelDerecho";
            panelDerecho.Padding = new Padding(5);
            panelDerecho.Size = new Size(414, 542);
            panelDerecho.TabIndex = 2;
            // 
            // gpbResumen
            // 
            gpbResumen.Controls.Add(tlpResumen);
            gpbResumen.Dock = DockStyle.Top;
            gpbResumen.Location = new Point(5, 265);
            gpbResumen.Name = "gpbResumen";
            gpbResumen.Padding = new Padding(5);
            gpbResumen.Size = new Size(404, 80);
            gpbResumen.TabIndex = 2;
            gpbResumen.TabStop = false;
            gpbResumen.Text = "Resumen";
            // 
            // tlpResumen
            // 
            tlpResumen.ColumnCount = 4;
            tlpResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tlpResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpResumen.Controls.Add(lblItems, 0, 0);
            tlpResumen.Controls.Add(lblItemsValor, 1, 0);
            tlpResumen.Controls.Add(lblCantidadTotal, 2, 0);
            tlpResumen.Controls.Add(lblCantidadTotalValor, 3, 0);
            tlpResumen.Dock = DockStyle.Fill;
            tlpResumen.Location = new Point(5, 23);
            tlpResumen.Name = "tlpResumen";
            tlpResumen.RowCount = 1;
            tlpResumen.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpResumen.Size = new Size(394, 52);
            tlpResumen.TabIndex = 0;
            // 
            // lblItems
            // 
            lblItems.Dock = DockStyle.Fill;
            lblItems.Font = new Font("Segoe UI", 10F);
            lblItems.Location = new Point(3, 0);
            lblItems.Name = "lblItems";
            lblItems.Size = new Size(74, 52);
            lblItems.TabIndex = 0;
            lblItems.Text = "Items:";
            lblItems.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblItemsValor
            // 
            lblItemsValor.Dock = DockStyle.Fill;
            lblItemsValor.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblItemsValor.Location = new Point(83, 0);
            lblItemsValor.Name = "lblItemsValor";
            lblItemsValor.Size = new Size(101, 52);
            lblItemsValor.TabIndex = 1;
            lblItemsValor.Text = "0";
            lblItemsValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblCantidadTotal
            // 
            lblCantidadTotal.Dock = DockStyle.Fill;
            lblCantidadTotal.Font = new Font("Segoe UI", 10F);
            lblCantidadTotal.Location = new Point(190, 0);
            lblCantidadTotal.Name = "lblCantidadTotal";
            lblCantidadTotal.Size = new Size(94, 52);
            lblCantidadTotal.TabIndex = 2;
            lblCantidadTotal.Text = "Total Und.:";
            lblCantidadTotal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblCantidadTotalValor
            // 
            lblCantidadTotalValor.Dock = DockStyle.Fill;
            lblCantidadTotalValor.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblCantidadTotalValor.Location = new Point(290, 0);
            lblCantidadTotalValor.Name = "lblCantidadTotalValor";
            lblCantidadTotalValor.Size = new Size(101, 52);
            lblCantidadTotalValor.TabIndex = 3;
            lblCantidadTotalValor.Text = "0";
            lblCantidadTotalValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // gpbCliente
            // 
            gpbCliente.Controls.Add(tlpCliente);
            gpbCliente.Dock = DockStyle.Top;
            gpbCliente.Location = new Point(5, 135);
            gpbCliente.Name = "gpbCliente";
            gpbCliente.Padding = new Padding(5);
            gpbCliente.Size = new Size(404, 130);
            gpbCliente.TabIndex = 0;
            gpbCliente.TabStop = false;
            gpbCliente.Text = "Cliente";
            // 
            // tlpCliente
            // 
            tlpCliente.ColumnCount = 3;
            tlpCliente.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tlpCliente.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpCliente.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            tlpCliente.Controls.Add(lblCliente, 0, 0);
            tlpCliente.Controls.Add(cboCliente, 1, 0);
            tlpCliente.Controls.Add(btnNuevoCliente, 2, 0);
            tlpCliente.Controls.Add(lblRUT, 0, 1);
            tlpCliente.Controls.Add(lblRUTValor, 1, 1);
            tlpCliente.Controls.Add(lblTelefono, 0, 2);
            tlpCliente.Controls.Add(lblTelefonoValor, 1, 2);
            tlpCliente.Dock = DockStyle.Fill;
            tlpCliente.Location = new Point(5, 23);
            tlpCliente.Name = "tlpCliente";
            tlpCliente.RowCount = 3;
            tlpCliente.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tlpCliente.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpCliente.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpCliente.Size = new Size(394, 102);
            tlpCliente.TabIndex = 0;
            // 
            // lblCliente
            // 
            lblCliente.Dock = DockStyle.Fill;
            lblCliente.Font = new Font("Segoe UI", 10F);
            lblCliente.Location = new Point(3, 0);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new Size(74, 35);
            lblCliente.TabIndex = 0;
            lblCliente.Text = "Cliente:";
            lblCliente.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboCliente
            // 
            cboCliente.Dock = DockStyle.Fill;
            cboCliente.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCliente.Font = new Font("Segoe UI", 10F);
            cboCliente.FormattingEnabled = true;
            cboCliente.Location = new Point(83, 3);
            cboCliente.Name = "cboCliente";
            cboCliente.Size = new Size(268, 25);
            cboCliente.TabIndex = 1;
            // 
            // btnNuevoCliente
            // 
            btnNuevoCliente.BackColor = Color.FromArgb(39, 174, 96);
            btnNuevoCliente.Dock = DockStyle.Fill;
            btnNuevoCliente.FlatStyle = FlatStyle.Flat;
            btnNuevoCliente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNuevoCliente.ForeColor = Color.White;
            btnNuevoCliente.Location = new Point(357, 3);
            btnNuevoCliente.Name = "btnNuevoCliente";
            btnNuevoCliente.Size = new Size(34, 29);
            btnNuevoCliente.TabIndex = 2;
            btnNuevoCliente.Text = "+";
            btnNuevoCliente.UseVisualStyleBackColor = false;
            // 
            // lblRUT
            // 
            lblRUT.Dock = DockStyle.Fill;
            lblRUT.Font = new Font("Segoe UI", 9F);
            lblRUT.Location = new Point(3, 35);
            lblRUT.Name = "lblRUT";
            lblRUT.Size = new Size(74, 30);
            lblRUT.TabIndex = 3;
            lblRUT.Text = "RUT:";
            lblRUT.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblRUTValor
            // 
            lblRUTValor.Dock = DockStyle.Fill;
            lblRUTValor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRUTValor.Location = new Point(83, 35);
            lblRUTValor.Name = "lblRUTValor";
            lblRUTValor.Size = new Size(268, 30);
            lblRUTValor.TabIndex = 4;
            lblRUTValor.Text = "-";
            lblRUTValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTelefono
            // 
            lblTelefono.Dock = DockStyle.Fill;
            lblTelefono.Font = new Font("Segoe UI", 9F);
            lblTelefono.Location = new Point(3, 65);
            lblTelefono.Name = "lblTelefono";
            lblTelefono.Size = new Size(74, 37);
            lblTelefono.TabIndex = 5;
            lblTelefono.Text = "Teléfono:";
            lblTelefono.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTelefonoValor
            // 
            lblTelefonoValor.Dock = DockStyle.Fill;
            lblTelefonoValor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTelefonoValor.Location = new Point(83, 65);
            lblTelefonoValor.Name = "lblTelefonoValor";
            lblTelefonoValor.Size = new Size(268, 37);
            lblTelefonoValor.TabIndex = 6;
            lblTelefonoValor.Text = "-";
            lblTelefonoValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // gpbPago
            // 
            gpbPago.Controls.Add(tlpPago);
            gpbPago.Dock = DockStyle.Top;
            gpbPago.Location = new Point(5, 5);
            gpbPago.Name = "gpbPago";
            gpbPago.Padding = new Padding(5);
            gpbPago.Size = new Size(404, 130);
            gpbPago.TabIndex = 1;
            gpbPago.TabStop = false;
            gpbPago.Text = "Pago";
            // 
            // tlpPago
            // 
            tlpPago.ColumnCount = 2;
            tlpPago.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpPago.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpPago.Controls.Add(lblMetodoPago, 0, 0);
            tlpPago.Controls.Add(cboMetodoPago, 1, 0);
            tlpPago.Controls.Add(lblMontoRecibido, 0, 1);
            tlpPago.Controls.Add(numMontoRecibido, 1, 1);
            tlpPago.Controls.Add(lblCambio, 0, 2);
            tlpPago.Controls.Add(lblCambioValor, 1, 2);
            tlpPago.Dock = DockStyle.Fill;
            tlpPago.Location = new Point(5, 23);
            tlpPago.Name = "tlpPago";
            tlpPago.Padding = new Padding(5);
            tlpPago.RowCount = 3;
            tlpPago.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tlpPago.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tlpPago.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tlpPago.Size = new Size(394, 102);
            tlpPago.TabIndex = 0;
            // 
            // lblMetodoPago
            // 
            lblMetodoPago.Dock = DockStyle.Fill;
            lblMetodoPago.Font = new Font("Segoe UI", 10F);
            lblMetodoPago.Location = new Point(8, 5);
            lblMetodoPago.Name = "lblMetodoPago";
            lblMetodoPago.Size = new Size(94, 35);
            lblMetodoPago.TabIndex = 0;
            lblMetodoPago.Text = "Método Pago:";
            lblMetodoPago.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboMetodoPago
            // 
            cboMetodoPago.Dock = DockStyle.Fill;
            cboMetodoPago.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMetodoPago.Font = new Font("Segoe UI", 10F);
            cboMetodoPago.FormattingEnabled = true;
            cboMetodoPago.Location = new Point(108, 8);
            cboMetodoPago.Name = "cboMetodoPago";
            cboMetodoPago.Size = new Size(278, 25);
            cboMetodoPago.TabIndex = 1;
            // 
            // lblMontoRecibido
            // 
            lblMontoRecibido.Dock = DockStyle.Fill;
            lblMontoRecibido.Font = new Font("Segoe UI", 10F);
            lblMontoRecibido.Location = new Point(8, 40);
            lblMontoRecibido.Name = "lblMontoRecibido";
            lblMontoRecibido.Size = new Size(94, 35);
            lblMontoRecibido.TabIndex = 2;
            lblMontoRecibido.Text = "Recibido:";
            lblMontoRecibido.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numMontoRecibido
            // 
            numMontoRecibido.DecimalPlaces = 2;
            numMontoRecibido.Dock = DockStyle.Fill;
            numMontoRecibido.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            numMontoRecibido.Location = new Point(108, 43);
            numMontoRecibido.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            numMontoRecibido.Name = "numMontoRecibido";
            numMontoRecibido.Size = new Size(278, 29);
            numMontoRecibido.TabIndex = 3;
            numMontoRecibido.TextAlign = HorizontalAlignment.Right;
            numMontoRecibido.ThousandsSeparator = true;
            // 
            // lblCambio
            // 
            lblCambio.Dock = DockStyle.Fill;
            lblCambio.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCambio.Location = new Point(8, 75);
            lblCambio.Name = "lblCambio";
            lblCambio.Size = new Size(94, 35);
            lblCambio.TabIndex = 4;
            lblCambio.Text = "Cambio:";
            lblCambio.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblCambioValor
            // 
            lblCambioValor.Dock = DockStyle.Fill;
            lblCambioValor.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblCambioValor.ForeColor = Color.FromArgb(39, 174, 96);
            lblCambioValor.Location = new Point(108, 75);
            lblCambioValor.Name = "lblCambioValor";
            lblCambioValor.Size = new Size(278, 35);
            lblCambioValor.TabIndex = 5;
            lblCambioValor.Text = "$ 0.00";
            lblCambioValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(245, 247, 250);
            tlpMain.SetColumnSpan(panelBotones, 2);
            panelBotones.Controls.Add(btnPagar);
            panelBotones.Controls.Add(btnCancelarVenta);
            panelBotones.Controls.Add(btnLimpiar);
            panelBotones.Controls.Add(btnCerrar);
            panelBotones.Dock = DockStyle.Fill;
            panelBotones.Location = new Point(3, 611);
            panelBotones.Name = "panelBotones";
            panelBotones.Padding = new Padding(10);
            panelBotones.Size = new Size(1194, 64);
            panelBotones.TabIndex = 3;
            // 
            // btnPagar
            // 
            btnPagar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPagar.BackColor = Color.FromArgb(39, 174, 96);
            btnPagar.FlatStyle = FlatStyle.Flat;
            btnPagar.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPagar.ForeColor = Color.White;
            btnPagar.Location = new Point(974, 12);
            btnPagar.Name = "btnPagar";
            btnPagar.Size = new Size(120, 45);
            btnPagar.TabIndex = 0;
            btnPagar.Text = "PAGAR (F2)";
            btnPagar.UseVisualStyleBackColor = false;
            // 
            // btnCancelarVenta
            // 
            btnCancelarVenta.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelarVenta.BackColor = Color.FromArgb(192, 57, 43);
            btnCancelarVenta.FlatStyle = FlatStyle.Flat;
            btnCancelarVenta.ForeColor = Color.White;
            btnCancelarVenta.Location = new Point(848, 12);
            btnCancelarVenta.Name = "btnCancelarVenta";
            btnCancelarVenta.Size = new Size(120, 45);
            btnCancelarVenta.TabIndex = 1;
            btnCancelarVenta.Text = "Cancelar Venta";
            btnCancelarVenta.UseVisualStyleBackColor = false;
            // 
            // btnLimpiar
            // 
            btnLimpiar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLimpiar.BackColor = Color.FromArgb(52, 73, 94);
            btnLimpiar.FlatStyle = FlatStyle.Flat;
            btnLimpiar.ForeColor = Color.White;
            btnLimpiar.Location = new Point(722, 12);
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.Size = new Size(120, 45);
            btnLimpiar.TabIndex = 2;
            btnLimpiar.Text = "Limpiar";
            btnLimpiar.UseVisualStyleBackColor = false;
            // 
            // btnCerrar
            // 
            btnCerrar.Location = new Point(13, 12);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(100, 35);
            btnCerrar.TabIndex = 3;
            btnCerrar.Text = "Cerrar POS";
            btnCerrar.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { tsslUsuario, tsslCaja, tsslEstado });
            statusStrip.Location = new Point(0, 678);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1200, 22);
            statusStrip.TabIndex = 1;
            statusStrip.Text = "statusStrip1";
            // 
            // tsslUsuario
            // 
            tsslUsuario.Name = "tsslUsuario";
            tsslUsuario.Size = new Size(89, 17);
            tsslUsuario.Text = "Usuario: Admin";
            // 
            // tsslCaja
            // 
            tsslCaja.Name = "tsslCaja";
            tsslCaja.Size = new Size(54, 17);
            tsslCaja.Text = "Caja: 001";
            // 
            // tsslEstado
            // 
            tsslEstado.Name = "tsslEstado";
            tsslEstado.Size = new Size(82, 17);
            tsslEstado.Text = "Estado: Activo";
            // 
            // frm_POS
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 700);
            Controls.Add(tlpMain);
            Controls.Add(statusStrip);
            Font = new Font("Segoe UI", 9.75F);
            KeyPreview = true;
            MinimumSize = new Size(1024, 600);
            Name = "frm_POS";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Punto de Venta";
            tlpMain.ResumeLayout(false);
            panelSuperior.ResumeLayout(false);
            panelSuperior.PerformLayout();
            panelIzquierdo.ResumeLayout(false);
            gpbCarrito.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCarrito).EndInit();
            panelTotales.ResumeLayout(false);
            tlpTotales.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numDescuento).EndInit();
            gpbBusqueda.ResumeLayout(false);
            tlpBusqueda.ResumeLayout(false);
            tlpBusqueda.PerformLayout();
            panelDerecho.ResumeLayout(false);
            gpbResumen.ResumeLayout(false);
            tlpResumen.ResumeLayout(false);
            gpbCliente.ResumeLayout(false);
            tlpCliente.ResumeLayout(false);
            gpbPago.ResumeLayout(false);
            tlpPago.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numMontoRecibido).EndInit();
            panelBotones.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}