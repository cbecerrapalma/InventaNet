namespace Inventa.Forms.Mantenedores
{
    partial class frm_Categoria
    {
        private System.ComponentModel.IContainer components = null;

        // ── Panel header ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelTitulo;
        private System.Windows.Forms.Label lblTitulo;

        // ── ToolStrip ─────────────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip tsCategorias;
        private System.Windows.Forms.ToolStripButton btnNuevo;
        private System.Windows.Forms.ToolStripButton btnEditar;
        private System.Windows.Forms.ToolStripButton btnEliminar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton btnRefrescar;
        private System.Windows.Forms.ToolStripLabel tsLblContador;

        // ── Grilla ────────────────────────────────────────────────────────────
        private System.Windows.Forms.DataGridView dgvCategorias;

        // ── Panel edición (campos) ────────────────────────────────────────────
        private System.Windows.Forms.Panel panelEdicion;
        private System.Windows.Forms.TableLayoutPanel tlpMain;

        // Fila 0: Codigo | ID
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.Label lblIdValor;

        // Fila 1: Nombre | Activo
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblActivo;
        private System.Windows.Forms.CheckBox chkActivo;

        // Fila 2: ID Tenant | Nombre Tenant
        private System.Windows.Forms.Label lblIdTenant;
        private System.Windows.Forms.Label lblIdTenantValor;
        private System.Windows.Forms.Label lblTenant;
        private System.Windows.Forms.Label lblTenantValor;

        // ── GroupBox información ──────────────────────────────────────────────
        private System.Windows.Forms.GroupBox gpbInformacion;
        private System.Windows.Forms.TableLayoutPanel tlpInfo;
        private System.Windows.Forms.Label lblIdCategoria;
        private System.Windows.Forms.Label lblIdCategoriaValor;
        private System.Windows.Forms.Label lblTotalProductos;
        private System.Windows.Forms.Label lblTotalProductosValor;

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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            panelTitulo = new Panel();
            lblTitulo = new Label();
            tsCategorias = new ToolStrip();
            btnNuevo = new ToolStripButton();
            btnEditar = new ToolStripButton();
            btnEliminar = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            btnRefrescar = new ToolStripButton();
            tsLblContador = new ToolStripLabel();
            dgvCategorias = new DataGridView();
            panelEdicion = new Panel();
            tlpMain = new TableLayoutPanel();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblId = new Label();
            lblIdValor = new Label();
            lblNombre = new Label();
            txtNombre = new TextBox();
            lblActivo = new Label();
            chkActivo = new CheckBox();
            lblIdTenant = new Label();
            lblIdTenantValor = new Label();
            lblTenant = new Label();
            lblTenantValor = new Label();
            gpbInformacion = new GroupBox();
            tlpInfo = new TableLayoutPanel();
            lblIdCategoria = new Label();
            lblIdCategoriaValor = new Label();
            lblTotalProductos = new Label();
            lblTotalProductosValor = new Label();
            panelBotones = new Panel();
            flpBotones = new FlowLayoutPanel();
            btnGuardar = new Button();
            btnCancelar = new Button();
            panelTitulo.SuspendLayout();
            tsCategorias.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCategorias).BeginInit();
            panelEdicion.SuspendLayout();
            tlpMain.SuspendLayout();
            gpbInformacion.SuspendLayout();
            tlpInfo.SuspendLayout();
            panelBotones.SuspendLayout();
            flpBotones.SuspendLayout();
            SuspendLayout();
            // 
            // panelTitulo
            // 
            panelTitulo.BackColor = Color.FromArgb(30, 60, 114);
            panelTitulo.Controls.Add(lblTitulo);
            panelTitulo.Dock = DockStyle.Top;
            panelTitulo.Location = new Point(0, 0);
            panelTitulo.Name = "panelTitulo";
            panelTitulo.Size = new Size(884, 44);
            panelTitulo.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(0, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(884, 44);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "MANTENEDOR DE CATEGORIAS";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tsCategorias
            // 
            tsCategorias.GripStyle = ToolStripGripStyle.Hidden;
            tsCategorias.Items.AddRange(new ToolStripItem[] { btnNuevo, btnEditar, btnEliminar, tsSep1, btnRefrescar, tsLblContador });
            tsCategorias.Location = new Point(0, 44);
            tsCategorias.Name = "tsCategorias";
            tsCategorias.RenderMode = ToolStripRenderMode.System;
            tsCategorias.Size = new Size(884, 25);
            tsCategorias.TabIndex = 1;
            // 
            // btnNuevo
            // 
            btnNuevo.Image = Properties.Resources.add;
            btnNuevo.ImageTransparentColor = Color.Magenta;
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(62, 22);
            btnNuevo.Text = "Nuevo";
            btnNuevo.ToolTipText = "Nueva categoria (Ins)";
            btnNuevo.Click += BtnNuevo_Click;
            // 
            // btnEditar
            // 
            btnEditar.Enabled = false;
            btnEditar.Image = Properties.Resources.edit;
            btnEditar.ImageTransparentColor = Color.Magenta;
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new Size(57, 22);
            btnEditar.Text = "Editar";
            btnEditar.Click += BtnEditar_Click;
            // 
            // btnEliminar
            // 
            btnEliminar.Enabled = false;
            btnEliminar.Image = Properties.Resources.delete;
            btnEliminar.ImageTransparentColor = Color.Magenta;
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(70, 22);
            btnEliminar.Text = "Eliminar";
            btnEliminar.Click += BtnEliminar_Click;
            // 
            // tsSep1
            // 
            tsSep1.Name = "tsSep1";
            tsSep1.Size = new Size(6, 25);
            // 
            // btnRefrescar
            // 
            btnRefrescar.Image = Properties.Resources.refresh;
            btnRefrescar.ImageTransparentColor = Color.Magenta;
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Size = new Size(98, 22);
            btnRefrescar.Text = "Refrescar (F5)";
            btnRefrescar.Click += BtnRefrescar_Click;
            // 
            // tsLblContador
            // 
            tsLblContador.Alignment = ToolStripItemAlignment.Right;
            tsLblContador.Font = new Font("Segoe UI", 8F);
            tsLblContador.ForeColor = Color.FromArgb(96, 94, 92);
            tsLblContador.Name = "tsLblContador";
            tsLblContador.Size = new Size(0, 22);
            // 
            // dgvCategorias
            // 
            dgvCategorias.AllowUserToAddRows = false;
            dgvCategorias.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(250, 249, 248);
            dgvCategorias.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvCategorias.BackgroundColor = Color.White;
            dgvCategorias.BorderStyle = BorderStyle.None;
            dgvCategorias.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(243, 242, 241);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle2.Padding = new Padding(8, 0, 0, 0);
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvCategorias.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvCategorias.ColumnHeadersHeight = 36;
            dgvCategorias.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(96, 94, 92);
            dataGridViewCellStyle3.Padding = new Padding(8, 2, 8, 2);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(229, 243, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvCategorias.DefaultCellStyle = dataGridViewCellStyle3;
            dgvCategorias.Dock = DockStyle.Fill;
            dgvCategorias.EnableHeadersVisualStyles = false;
            dgvCategorias.GridColor = Color.FromArgb(237, 235, 233);
            dgvCategorias.Location = new Point(0, 69);
            dgvCategorias.MultiSelect = false;
            dgvCategorias.Name = "dgvCategorias";
            dgvCategorias.ReadOnly = true;
            dgvCategorias.RowHeadersVisible = false;
            dgvCategorias.RowTemplate.Height = 32;
            dgvCategorias.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCategorias.Size = new Size(884, 341);
            dgvCategorias.TabIndex = 2;
            // 
            // panelEdicion
            // 
            panelEdicion.BackColor = Color.White;
            panelEdicion.BorderStyle = BorderStyle.FixedSingle;
            panelEdicion.Controls.Add(tlpMain);
            panelEdicion.Dock = DockStyle.Bottom;
            panelEdicion.Location = new Point(0, 410);
            panelEdicion.Name = "panelEdicion";
            panelEdicion.Padding = new Padding(8, 6, 8, 6);
            panelEdicion.Size = new Size(884, 110);
            panelEdicion.TabIndex = 3;
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 4;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpMain.Controls.Add(lblCodigo, 0, 0);
            tlpMain.Controls.Add(txtCodigo, 1, 0);
            tlpMain.Controls.Add(lblId, 2, 0);
            tlpMain.Controls.Add(lblIdValor, 3, 0);
            tlpMain.Controls.Add(lblNombre, 0, 1);
            tlpMain.Controls.Add(txtNombre, 1, 1);
            tlpMain.Controls.Add(lblActivo, 2, 1);
            tlpMain.Controls.Add(chkActivo, 3, 1);
            tlpMain.Controls.Add(lblIdTenant, 0, 2);
            tlpMain.Controls.Add(lblIdTenantValor, 1, 2);
            tlpMain.Controls.Add(lblTenant, 2, 2);
            tlpMain.Controls.Add(lblTenantValor, 3, 2);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(8, 6);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.Size = new Size(866, 96);
            tlpMain.TabIndex = 0;
            // 
            // lblCodigo
            // 
            lblCodigo.Dock = DockStyle.Fill;
            lblCodigo.Font = new Font("Segoe UI", 9F);
            lblCodigo.ForeColor = Color.FromArgb(96, 94, 92);
            lblCodigo.Location = new Point(3, 0);
            lblCodigo.Name = "lblCodigo";
            lblCodigo.Size = new Size(104, 32);
            lblCodigo.TabIndex = 0;
            lblCodigo.Text = "Codigo:";
            lblCodigo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtCodigo
            // 
            txtCodigo.CharacterCasing = CharacterCasing.Upper;
            txtCodigo.Dock = DockStyle.Fill;
            txtCodigo.Font = new Font("Segoe UI", 9F);
            txtCodigo.Location = new Point(113, 3);
            txtCodigo.MaxLength = 20;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.Size = new Size(252, 23);
            txtCodigo.TabIndex = 1;
            // 
            // lblId
            // 
            lblId.Dock = DockStyle.Fill;
            lblId.Font = new Font("Segoe UI", 9F);
            lblId.ForeColor = Color.FromArgb(96, 94, 92);
            lblId.Location = new Point(371, 0);
            lblId.Name = "lblId";
            lblId.Size = new Size(104, 32);
            lblId.TabIndex = 2;
            lblId.Text = "ID:";
            lblId.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblIdValor
            // 
            lblIdValor.Dock = DockStyle.Fill;
            lblIdValor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblIdValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblIdValor.Location = new Point(481, 0);
            lblIdValor.Name = "lblIdValor";
            lblIdValor.Size = new Size(382, 32);
            lblIdValor.TabIndex = 3;
            lblIdValor.Text = "<< Nuevo >>";
            lblIdValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblNombre
            // 
            lblNombre.Dock = DockStyle.Fill;
            lblNombre.Font = new Font("Segoe UI", 9F);
            lblNombre.ForeColor = Color.FromArgb(96, 94, 92);
            lblNombre.Location = new Point(3, 32);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(104, 32);
            lblNombre.TabIndex = 4;
            lblNombre.Text = "Nombre:";
            lblNombre.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtNombre
            // 
            txtNombre.Dock = DockStyle.Fill;
            txtNombre.Font = new Font("Segoe UI", 9F);
            txtNombre.Location = new Point(113, 35);
            txtNombre.MaxLength = 100;
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(252, 23);
            txtNombre.TabIndex = 2;
            // 
            // lblActivo
            // 
            lblActivo.Dock = DockStyle.Fill;
            lblActivo.Font = new Font("Segoe UI", 9F);
            lblActivo.ForeColor = Color.FromArgb(96, 94, 92);
            lblActivo.Location = new Point(371, 32);
            lblActivo.Name = "lblActivo";
            lblActivo.Size = new Size(104, 32);
            lblActivo.TabIndex = 5;
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
            chkActivo.Location = new Point(481, 35);
            chkActivo.Name = "chkActivo";
            chkActivo.Size = new Size(382, 26);
            chkActivo.TabIndex = 3;
            // 
            // lblIdTenant
            // 
            lblIdTenant.Dock = DockStyle.Fill;
            lblIdTenant.Font = new Font("Segoe UI", 9F);
            lblIdTenant.ForeColor = Color.FromArgb(96, 94, 92);
            lblIdTenant.Location = new Point(3, 64);
            lblIdTenant.Name = "lblIdTenant";
            lblIdTenant.Size = new Size(104, 32);
            lblIdTenant.TabIndex = 6;
            lblIdTenant.Text = "ID Tenant:";
            lblIdTenant.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblIdTenantValor
            // 
            lblIdTenantValor.Dock = DockStyle.Fill;
            lblIdTenantValor.Font = new Font("Segoe UI", 9F);
            lblIdTenantValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblIdTenantValor.Location = new Point(113, 64);
            lblIdTenantValor.Name = "lblIdTenantValor";
            lblIdTenantValor.Size = new Size(252, 32);
            lblIdTenantValor.TabIndex = 7;
            lblIdTenantValor.Text = "-";
            lblIdTenantValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTenant
            // 
            lblTenant.Dock = DockStyle.Fill;
            lblTenant.Font = new Font("Segoe UI", 9F);
            lblTenant.ForeColor = Color.FromArgb(96, 94, 92);
            lblTenant.Location = new Point(371, 64);
            lblTenant.Name = "lblTenant";
            lblTenant.Size = new Size(104, 32);
            lblTenant.TabIndex = 8;
            lblTenant.Text = "Empresa:";
            lblTenant.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTenantValor
            // 
            lblTenantValor.Dock = DockStyle.Fill;
            lblTenantValor.Font = new Font("Segoe UI", 9F);
            lblTenantValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblTenantValor.Location = new Point(481, 64);
            lblTenantValor.Name = "lblTenantValor";
            lblTenantValor.Size = new Size(382, 32);
            lblTenantValor.TabIndex = 9;
            lblTenantValor.Text = "-";
            lblTenantValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // gpbInformacion
            // 
            gpbInformacion.Controls.Add(tlpInfo);
            gpbInformacion.Dock = DockStyle.Bottom;
            gpbInformacion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gpbInformacion.ForeColor = Color.FromArgb(50, 49, 48);
            gpbInformacion.Location = new Point(0, 520);
            gpbInformacion.Name = "gpbInformacion";
            gpbInformacion.Padding = new Padding(8);
            gpbInformacion.Size = new Size(884, 46);
            gpbInformacion.TabIndex = 4;
            gpbInformacion.TabStop = false;
            gpbInformacion.Text = "Informacion de la Categoria seleccionada";
            // 
            // tlpInfo
            // 
            tlpInfo.ColumnCount = 4;
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpInfo.Controls.Add(lblIdCategoria, 0, 0);
            tlpInfo.Controls.Add(lblIdCategoriaValor, 1, 0);
            tlpInfo.Controls.Add(lblTotalProductos, 2, 0);
            tlpInfo.Controls.Add(lblTotalProductosValor, 3, 0);
            tlpInfo.Dock = DockStyle.Fill;
            tlpInfo.Location = new Point(8, 24);
            tlpInfo.Name = "tlpInfo";
            tlpInfo.RowCount = 1;
            tlpInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpInfo.Size = new Size(868, 14);
            tlpInfo.TabIndex = 0;
            // 
            // lblIdCategoria
            // 
            lblIdCategoria.Dock = DockStyle.Fill;
            lblIdCategoria.Font = new Font("Segoe UI", 8.5F);
            lblIdCategoria.ForeColor = Color.FromArgb(96, 94, 92);
            lblIdCategoria.Location = new Point(3, 0);
            lblIdCategoria.Name = "lblIdCategoria";
            lblIdCategoria.Size = new Size(104, 14);
            lblIdCategoria.TabIndex = 0;
            lblIdCategoria.Text = "ID Categoria:";
            lblIdCategoria.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblIdCategoriaValor
            // 
            lblIdCategoriaValor.Dock = DockStyle.Fill;
            lblIdCategoriaValor.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblIdCategoriaValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblIdCategoriaValor.Location = new Point(113, 0);
            lblIdCategoriaValor.Name = "lblIdCategoriaValor";
            lblIdCategoriaValor.Size = new Size(318, 14);
            lblIdCategoriaValor.TabIndex = 1;
            lblIdCategoriaValor.Text = "-";
            lblIdCategoriaValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTotalProductos
            // 
            lblTotalProductos.Dock = DockStyle.Fill;
            lblTotalProductos.Font = new Font("Segoe UI", 8.5F);
            lblTotalProductos.ForeColor = Color.FromArgb(96, 94, 92);
            lblTotalProductos.Location = new Point(437, 0);
            lblTotalProductos.Name = "lblTotalProductos";
            lblTotalProductos.Size = new Size(104, 14);
            lblTotalProductos.TabIndex = 2;
            lblTotalProductos.Text = "Productos:";
            lblTotalProductos.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTotalProductosValor
            // 
            lblTotalProductosValor.Dock = DockStyle.Fill;
            lblTotalProductosValor.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblTotalProductosValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblTotalProductosValor.Location = new Point(547, 0);
            lblTotalProductosValor.Name = "lblTotalProductosValor";
            lblTotalProductosValor.Size = new Size(318, 14);
            lblTotalProductosValor.TabIndex = 3;
            lblTotalProductosValor.Text = "-";
            lblTotalProductosValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(243, 242, 241);
            panelBotones.Controls.Add(flpBotones);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 566);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(884, 54);
            panelBotones.TabIndex = 5;
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
            flpBotones.Size = new Size(884, 54);
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
            btnGuardar.Location = new Point(758, 8);
            btnGuardar.Margin = new Padding(6, 0, 0, 0);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 36);
            btnGuardar.TabIndex = 0;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += BtnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = Color.White;
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(138, 136, 134);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Segoe UI", 9F);
            btnCancelar.ForeColor = Color.FromArgb(50, 49, 48);
            btnCancelar.Location = new Point(642, 8);
            btnCancelar.Margin = new Padding(6, 0, 0, 0);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += BtnCancelar_Click;
            // 
            // frm_Categoria
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(884, 620);
            Controls.Add(dgvCategorias);
            Controls.Add(tsCategorias);
            Controls.Add(panelEdicion);
            Controls.Add(gpbInformacion);
            Controls.Add(panelBotones);
            Controls.Add(panelTitulo);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(800, 560);
            Name = "frm_Categoria";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Mantenedor de Categorias";
            Load += Frm_Categoria_Load;
            panelTitulo.ResumeLayout(false);
            tsCategorias.ResumeLayout(false);
            tsCategorias.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCategorias).EndInit();
            panelEdicion.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            gpbInformacion.ResumeLayout(false);
            tlpInfo.ResumeLayout(false);
            panelBotones.ResumeLayout(false);
            flpBotones.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}