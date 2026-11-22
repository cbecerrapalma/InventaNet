namespace Inventa.Forms.Mantenedores
{
    partial class frm_Cliente
    {
        private System.ComponentModel.IContainer components = null;

        // ── Panel header ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelTitulo;
        private System.Windows.Forms.Label lblTitulo;

        // ── ToolStrip ─────────────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip tsClientes;
        private System.Windows.Forms.ToolStripButton btnNuevo;
        private System.Windows.Forms.ToolStripButton btnEditar;
        private System.Windows.Forms.ToolStripButton btnEliminar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton btnRefrescar;
        private System.Windows.Forms.ToolStripLabel tsLblContador;

        // ── Grilla ────────────────────────────────────────────────────────────
        private System.Windows.Forms.DataGridView dgvClientes;

        // ── Panel edición (Dock=Bottom) ───────────────────────────────────────
        private System.Windows.Forms.Panel panelEdicion;
        private System.Windows.Forms.TableLayoutPanel tlpMain;

        // Fila 0: Código | ID
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.Label lblIdValor;

        // Fila 1: Nombre | RUT
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblRUT;
        private System.Windows.Forms.TextBox txtRUT;

        // Fila 2: Teléfono | Email
        private System.Windows.Forms.Label lblTelefono;
        private System.Windows.Forms.TextBox txtTelefono;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtEmail;

        // Fila 3: Tipo Cliente | Dirección
        private System.Windows.Forms.Label lblTipoCliente;
        private System.Windows.Forms.ComboBox cboTipoCliente;
        private System.Windows.Forms.Label lblDireccion;
        private System.Windows.Forms.TextBox txtDireccion;

        // Fila 4: Activo | ID Tenant
        private System.Windows.Forms.Label lblActivo;
        private System.Windows.Forms.CheckBox chkActivo;
        private System.Windows.Forms.Label lblIdTenant;
        private System.Windows.Forms.Label lblIdTenantValor;

        // Fila 5: Tenant nombre (span 4 cols)
        private System.Windows.Forms.Label lblTenant;
        private System.Windows.Forms.Label lblTenantValor;

        // ── GroupBox información ──────────────────────────────────────────────
        private System.Windows.Forms.GroupBox gpbInformacion;
        private System.Windows.Forms.TableLayoutPanel tlpInfo;
        private System.Windows.Forms.Label lblIdCliente;
        private System.Windows.Forms.Label lblIdClienteValor;
        private System.Windows.Forms.Label lblFechaCreacion;
        private System.Windows.Forms.Label lblFechaCreacionValor;

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
            tsClientes = new ToolStrip();
            btnNuevo = new ToolStripButton();
            btnEditar = new ToolStripButton();
            btnEliminar = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            btnRefrescar = new ToolStripButton();
            tsLblContador = new ToolStripLabel();
            dgvClientes = new DataGridView();
            panelEdicion = new Panel();
            tlpMain = new TableLayoutPanel();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblId = new Label();
            lblIdValor = new Label();
            lblNombre = new Label();
            txtNombre = new TextBox();
            lblRUT = new Label();
            txtRUT = new TextBox();
            lblTelefono = new Label();
            txtTelefono = new TextBox();
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblTipoCliente = new Label();
            cboTipoCliente = new ComboBox();
            lblDireccion = new Label();
            txtDireccion = new TextBox();
            lblActivo = new Label();
            chkActivo = new CheckBox();
            lblIdTenant = new Label();
            lblIdTenantValor = new Label();
            lblTenant = new Label();
            lblTenantValor = new Label();
            gpbInformacion = new GroupBox();
            tlpInfo = new TableLayoutPanel();
            lblIdCliente = new Label();
            lblIdClienteValor = new Label();
            lblFechaCreacion = new Label();
            lblFechaCreacionValor = new Label();
            panelBotones = new Panel();
            flpBotones = new FlowLayoutPanel();
            btnGuardar = new Button();
            btnCancelar = new Button();
            panelTitulo.SuspendLayout();
            tsClientes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvClientes).BeginInit();
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
            panelTitulo.Size = new Size(920, 44);
            panelTitulo.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(0, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(920, 44);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "MANTENEDOR DE CLIENTES";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tsClientes
            // 
            tsClientes.GripStyle = ToolStripGripStyle.Hidden;
            tsClientes.Items.AddRange(new ToolStripItem[] { btnNuevo, btnEditar, btnEliminar, tsSep1, btnRefrescar, tsLblContador });
            tsClientes.Location = new Point(0, 44);
            tsClientes.Name = "tsClientes";
            tsClientes.RenderMode = ToolStripRenderMode.System;
            tsClientes.Size = new Size(920, 25);
            tsClientes.TabIndex = 1;
            // 
            // btnNuevo
            // 
            btnNuevo.Image = Properties.Resources.add;
            btnNuevo.ImageTransparentColor = Color.Magenta;
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(62, 22);
            btnNuevo.Text = "Nuevo";
            btnNuevo.ToolTipText = "Nuevo cliente (Ins)";
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
            // dgvClientes
            // 
            dgvClientes.AllowUserToAddRows = false;
            dgvClientes.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(250, 249, 248);
            dgvClientes.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvClientes.BackgroundColor = Color.White;
            dgvClientes.BorderStyle = BorderStyle.None;
            dgvClientes.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(243, 242, 241);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle2.Padding = new Padding(8, 0, 0, 0);
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvClientes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvClientes.ColumnHeadersHeight = 36;
            dgvClientes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(96, 94, 92);
            dataGridViewCellStyle3.Padding = new Padding(8, 2, 8, 2);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(229, 243, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvClientes.DefaultCellStyle = dataGridViewCellStyle3;
            dgvClientes.Dock = DockStyle.Fill;
            dgvClientes.EnableHeadersVisualStyles = false;
            dgvClientes.GridColor = Color.FromArgb(237, 235, 233);
            dgvClientes.Location = new Point(0, 69);
            dgvClientes.MultiSelect = false;
            dgvClientes.Name = "dgvClientes";
            dgvClientes.ReadOnly = true;
            dgvClientes.RowHeadersVisible = false;
            dgvClientes.RowTemplate.Height = 32;
            dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClientes.Size = new Size(920, 361);
            dgvClientes.TabIndex = 2;
            // 
            // panelEdicion
            // 
            panelEdicion.BackColor = Color.White;
            panelEdicion.BorderStyle = BorderStyle.FixedSingle;
            panelEdicion.Controls.Add(tlpMain);
            panelEdicion.Dock = DockStyle.Bottom;
            panelEdicion.Location = new Point(0, 430);
            panelEdicion.Name = "panelEdicion";
            panelEdicion.Padding = new Padding(8, 6, 8, 6);
            panelEdicion.Size = new Size(920, 210);
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
            tlpMain.Controls.Add(lblRUT, 2, 1);
            tlpMain.Controls.Add(txtRUT, 3, 1);
            tlpMain.Controls.Add(lblTelefono, 0, 2);
            tlpMain.Controls.Add(txtTelefono, 1, 2);
            tlpMain.Controls.Add(lblEmail, 2, 2);
            tlpMain.Controls.Add(txtEmail, 3, 2);
            tlpMain.Controls.Add(lblTipoCliente, 0, 3);
            tlpMain.Controls.Add(cboTipoCliente, 1, 3);
            tlpMain.Controls.Add(lblDireccion, 2, 3);
            tlpMain.Controls.Add(txtDireccion, 3, 3);
            tlpMain.Controls.Add(lblActivo, 0, 4);
            tlpMain.Controls.Add(chkActivo, 1, 4);
            tlpMain.Controls.Add(lblIdTenant, 2, 4);
            tlpMain.Controls.Add(lblIdTenantValor, 3, 4);
            tlpMain.Controls.Add(lblTenant, 0, 5);
            tlpMain.Controls.Add(lblTenantValor, 1, 5);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(8, 6);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 6;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.Size = new Size(902, 196);
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
            txtCodigo.MaxLength = 50;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.Size = new Size(266, 23);
            txtCodigo.TabIndex = 1;
            // 
            // lblId
            // 
            lblId.Dock = DockStyle.Fill;
            lblId.Font = new Font("Segoe UI", 9F);
            lblId.ForeColor = Color.FromArgb(96, 94, 92);
            lblId.Location = new Point(385, 0);
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
            lblIdValor.Location = new Point(495, 0);
            lblIdValor.Name = "lblIdValor";
            lblIdValor.Size = new Size(404, 32);
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
            txtNombre.MaxLength = 200;
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(266, 23);
            txtNombre.TabIndex = 2;
            // 
            // lblRUT
            // 
            lblRUT.Dock = DockStyle.Fill;
            lblRUT.Font = new Font("Segoe UI", 9F);
            lblRUT.ForeColor = Color.FromArgb(96, 94, 92);
            lblRUT.Location = new Point(385, 32);
            lblRUT.Name = "lblRUT";
            lblRUT.Size = new Size(104, 32);
            lblRUT.TabIndex = 5;
            lblRUT.Text = "RUT:";
            lblRUT.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtRUT
            // 
            txtRUT.Dock = DockStyle.Fill;
            txtRUT.Font = new Font("Segoe UI", 9F);
            txtRUT.Location = new Point(495, 35);
            txtRUT.MaxLength = 20;
            txtRUT.Name = "txtRUT";
            txtRUT.Size = new Size(404, 23);
            txtRUT.TabIndex = 3;
            // 
            // lblTelefono
            // 
            lblTelefono.Dock = DockStyle.Fill;
            lblTelefono.Font = new Font("Segoe UI", 9F);
            lblTelefono.ForeColor = Color.FromArgb(96, 94, 92);
            lblTelefono.Location = new Point(3, 64);
            lblTelefono.Name = "lblTelefono";
            lblTelefono.Size = new Size(104, 32);
            lblTelefono.TabIndex = 6;
            lblTelefono.Text = "Telefono:";
            lblTelefono.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtTelefono
            // 
            txtTelefono.Dock = DockStyle.Fill;
            txtTelefono.Font = new Font("Segoe UI", 9F);
            txtTelefono.Location = new Point(113, 67);
            txtTelefono.MaxLength = 50;
            txtTelefono.Name = "txtTelefono";
            txtTelefono.Size = new Size(266, 23);
            txtTelefono.TabIndex = 4;
            // 
            // lblEmail
            // 
            lblEmail.Dock = DockStyle.Fill;
            lblEmail.Font = new Font("Segoe UI", 9F);
            lblEmail.ForeColor = Color.FromArgb(96, 94, 92);
            lblEmail.Location = new Point(385, 64);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(104, 32);
            lblEmail.TabIndex = 7;
            lblEmail.Text = "Email:";
            lblEmail.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtEmail
            // 
            txtEmail.Dock = DockStyle.Fill;
            txtEmail.Font = new Font("Segoe UI", 9F);
            txtEmail.Location = new Point(495, 67);
            txtEmail.MaxLength = 100;
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(404, 23);
            txtEmail.TabIndex = 5;
            // 
            // lblTipoCliente
            // 
            lblTipoCliente.Dock = DockStyle.Fill;
            lblTipoCliente.Font = new Font("Segoe UI", 9F);
            lblTipoCliente.ForeColor = Color.FromArgb(96, 94, 92);
            lblTipoCliente.Location = new Point(3, 96);
            lblTipoCliente.Name = "lblTipoCliente";
            lblTipoCliente.Size = new Size(104, 32);
            lblTipoCliente.TabIndex = 8;
            lblTipoCliente.Text = "Tipo Cliente:";
            lblTipoCliente.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboTipoCliente
            // 
            cboTipoCliente.Dock = DockStyle.Fill;
            cboTipoCliente.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoCliente.Font = new Font("Segoe UI", 9F);
            cboTipoCliente.FormattingEnabled = true;
            cboTipoCliente.Location = new Point(113, 99);
            cboTipoCliente.Name = "cboTipoCliente";
            cboTipoCliente.Size = new Size(266, 23);
            cboTipoCliente.TabIndex = 6;
            // 
            // lblDireccion
            // 
            lblDireccion.Dock = DockStyle.Fill;
            lblDireccion.Font = new Font("Segoe UI", 9F);
            lblDireccion.ForeColor = Color.FromArgb(96, 94, 92);
            lblDireccion.Location = new Point(385, 96);
            lblDireccion.Name = "lblDireccion";
            lblDireccion.Size = new Size(104, 32);
            lblDireccion.TabIndex = 9;
            lblDireccion.Text = "Direccion:";
            lblDireccion.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtDireccion
            // 
            txtDireccion.Dock = DockStyle.Fill;
            txtDireccion.Font = new Font("Segoe UI", 9F);
            txtDireccion.Location = new Point(495, 99);
            txtDireccion.MaxLength = 200;
            txtDireccion.Name = "txtDireccion";
            txtDireccion.Size = new Size(404, 23);
            txtDireccion.TabIndex = 7;
            // 
            // lblActivo
            // 
            lblActivo.Dock = DockStyle.Fill;
            lblActivo.Font = new Font("Segoe UI", 9F);
            lblActivo.ForeColor = Color.FromArgb(96, 94, 92);
            lblActivo.Location = new Point(3, 128);
            lblActivo.Name = "lblActivo";
            lblActivo.Size = new Size(104, 32);
            lblActivo.TabIndex = 10;
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
            chkActivo.Location = new Point(113, 131);
            chkActivo.Name = "chkActivo";
            chkActivo.Size = new Size(266, 26);
            chkActivo.TabIndex = 8;
            // 
            // lblIdTenant
            // 
            lblIdTenant.Dock = DockStyle.Fill;
            lblIdTenant.Font = new Font("Segoe UI", 9F);
            lblIdTenant.ForeColor = Color.FromArgb(96, 94, 92);
            lblIdTenant.Location = new Point(385, 128);
            lblIdTenant.Name = "lblIdTenant";
            lblIdTenant.Size = new Size(104, 32);
            lblIdTenant.TabIndex = 11;
            lblIdTenant.Text = "ID Tenant:";
            lblIdTenant.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblIdTenantValor
            // 
            lblIdTenantValor.Dock = DockStyle.Fill;
            lblIdTenantValor.Font = new Font("Segoe UI", 9F);
            lblIdTenantValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblIdTenantValor.Location = new Point(495, 128);
            lblIdTenantValor.Name = "lblIdTenantValor";
            lblIdTenantValor.Size = new Size(404, 32);
            lblIdTenantValor.TabIndex = 12;
            lblIdTenantValor.Text = "-";
            lblIdTenantValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTenant
            // 
            lblTenant.Dock = DockStyle.Fill;
            lblTenant.Font = new Font("Segoe UI", 9F);
            lblTenant.ForeColor = Color.FromArgb(96, 94, 92);
            lblTenant.Location = new Point(3, 160);
            lblTenant.Name = "lblTenant";
            lblTenant.Size = new Size(104, 36);
            lblTenant.TabIndex = 13;
            lblTenant.Text = "Empresa:";
            lblTenant.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTenantValor
            // 
            lblTenantValor.Dock = DockStyle.Fill;
            lblTenantValor.Font = new Font("Segoe UI", 9F);
            lblTenantValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblTenantValor.Location = new Point(113, 160);
            lblTenantValor.Name = "lblTenantValor";
            lblTenantValor.Size = new Size(266, 36);
            lblTenantValor.TabIndex = 14;
            lblTenantValor.Text = "-";
            lblTenantValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // gpbInformacion
            // 
            gpbInformacion.Controls.Add(tlpInfo);
            gpbInformacion.Dock = DockStyle.Bottom;
            gpbInformacion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gpbInformacion.ForeColor = Color.FromArgb(50, 49, 48);
            gpbInformacion.Location = new Point(0, 640);
            gpbInformacion.Name = "gpbInformacion";
            gpbInformacion.Padding = new Padding(8);
            gpbInformacion.Size = new Size(920, 46);
            gpbInformacion.TabIndex = 4;
            gpbInformacion.TabStop = false;
            gpbInformacion.Text = "Informacion del Cliente";
            // 
            // tlpInfo
            // 
            tlpInfo.ColumnCount = 4;
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpInfo.Controls.Add(lblIdCliente, 0, 0);
            tlpInfo.Controls.Add(lblIdClienteValor, 1, 0);
            tlpInfo.Controls.Add(lblFechaCreacion, 2, 0);
            tlpInfo.Controls.Add(lblFechaCreacionValor, 3, 0);
            tlpInfo.Dock = DockStyle.Fill;
            tlpInfo.Location = new Point(8, 24);
            tlpInfo.Name = "tlpInfo";
            tlpInfo.RowCount = 1;
            tlpInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpInfo.Size = new Size(904, 14);
            tlpInfo.TabIndex = 0;
            // 
            // lblIdCliente
            // 
            lblIdCliente.Dock = DockStyle.Fill;
            lblIdCliente.Font = new Font("Segoe UI", 8.5F);
            lblIdCliente.ForeColor = Color.FromArgb(96, 94, 92);
            lblIdCliente.Location = new Point(3, 0);
            lblIdCliente.Name = "lblIdCliente";
            lblIdCliente.Size = new Size(104, 14);
            lblIdCliente.TabIndex = 0;
            lblIdCliente.Text = "ID Cliente:";
            lblIdCliente.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblIdClienteValor
            // 
            lblIdClienteValor.Dock = DockStyle.Fill;
            lblIdClienteValor.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblIdClienteValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblIdClienteValor.Location = new Point(113, 0);
            lblIdClienteValor.Name = "lblIdClienteValor";
            lblIdClienteValor.Size = new Size(336, 14);
            lblIdClienteValor.TabIndex = 1;
            lblIdClienteValor.Text = "-";
            lblIdClienteValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFechaCreacion
            // 
            lblFechaCreacion.Dock = DockStyle.Fill;
            lblFechaCreacion.Font = new Font("Segoe UI", 8.5F);
            lblFechaCreacion.ForeColor = Color.FromArgb(96, 94, 92);
            lblFechaCreacion.Location = new Point(455, 0);
            lblFechaCreacion.Name = "lblFechaCreacion";
            lblFechaCreacion.Size = new Size(104, 14);
            lblFechaCreacion.TabIndex = 2;
            lblFechaCreacion.Text = "F. Creacion:";
            lblFechaCreacion.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblFechaCreacionValor
            // 
            lblFechaCreacionValor.Dock = DockStyle.Fill;
            lblFechaCreacionValor.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblFechaCreacionValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblFechaCreacionValor.Location = new Point(565, 0);
            lblFechaCreacionValor.Name = "lblFechaCreacionValor";
            lblFechaCreacionValor.Size = new Size(336, 14);
            lblFechaCreacionValor.TabIndex = 3;
            lblFechaCreacionValor.Text = "-";
            lblFechaCreacionValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(243, 242, 241);
            panelBotones.Controls.Add(flpBotones);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 686);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(920, 54);
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
            flpBotones.Size = new Size(920, 54);
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
            btnGuardar.Location = new Point(794, 8);
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
            btnCancelar.Location = new Point(678, 8);
            btnCancelar.Margin = new Padding(6, 0, 0, 0);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += BtnCancelar_Click;
            // 
            // frm_Cliente
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(920, 740);
            Controls.Add(dgvClientes);
            Controls.Add(tsClientes);
            Controls.Add(panelEdicion);
            Controls.Add(gpbInformacion);
            Controls.Add(panelBotones);
            Controls.Add(panelTitulo);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(800, 620);
            Name = "frm_Cliente";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Mantenedor de Clientes";
            Load += Frm_Cliente_Load;
            panelTitulo.ResumeLayout(false);
            tsClientes.ResumeLayout(false);
            tsClientes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvClientes).EndInit();
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