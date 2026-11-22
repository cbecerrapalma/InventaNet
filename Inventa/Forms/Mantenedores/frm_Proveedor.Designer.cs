namespace Inventa.Forms.Mantenedores
{
    partial class frm_Proveedor
    {
        private System.ComponentModel.IContainer components = null;

        // ── Panel header ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelTitulo;
        private System.Windows.Forms.Label lblTitulo;

        // ── ToolStrip ─────────────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip tsProveedores;
        private System.Windows.Forms.ToolStripButton btnNuevo;
        private System.Windows.Forms.ToolStripButton btnEditar;
        private System.Windows.Forms.ToolStripButton btnEliminar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton btnRefrescar;
        private System.Windows.Forms.ToolStripLabel tsLblContador;

        // ── Grilla ────────────────────────────────────────────────────────────
        private System.Windows.Forms.DataGridView dgvProveedores;

        // ── Panel edición (Dock=Bottom) ───────────────────────────────────────
        private System.Windows.Forms.Panel panelEdicion;
        private System.Windows.Forms.TableLayoutPanel tlpMain;

        // Fila 0: Código | ID
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.Label lblIdValor;

        // Fila 1: Razón Social | Nombre Comercial
        private System.Windows.Forms.Label lblRazonSocial;
        private System.Windows.Forms.TextBox txtRazonSocial;
        private System.Windows.Forms.Label lblNombreComercial;
        private System.Windows.Forms.TextBox txtNombreComercial;

        // Fila 2: RUT | Dirección
        private System.Windows.Forms.Label lblRUT;
        private System.Windows.Forms.TextBox txtRUT;
        private System.Windows.Forms.Label lblDireccion;
        private System.Windows.Forms.TextBox txtDireccion;

        // Fila 3: Teléfono | Email
        private System.Windows.Forms.Label lblTelefono;
        private System.Windows.Forms.TextBox txtTelefono;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtEmail;

        // Fila 4: Contacto | Activo
        private System.Windows.Forms.Label lblContacto;
        private System.Windows.Forms.TextBox txtContacto;
        private System.Windows.Forms.Label lblActivo;
        private System.Windows.Forms.CheckBox chkActivo;

        // Fila 5: F.Creación | ID Tenant
        private System.Windows.Forms.Label lblFechaCreacion;
        private System.Windows.Forms.Label lblFechaCreacionValor;
        private System.Windows.Forms.Label lblIdTenant;
        private System.Windows.Forms.Label lblIdTenantValor;

        // Fila 6: Empresa (span)
        private System.Windows.Forms.Label lblTenant;
        private System.Windows.Forms.Label lblTenantValor;

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
            panelTitulo = new System.Windows.Forms.Panel();
            lblTitulo = new System.Windows.Forms.Label();
            tsProveedores = new System.Windows.Forms.ToolStrip();
            btnNuevo = new System.Windows.Forms.ToolStripButton();
            btnEditar = new System.Windows.Forms.ToolStripButton();
            btnEliminar = new System.Windows.Forms.ToolStripButton();
            tsSep1 = new System.Windows.Forms.ToolStripSeparator();
            btnRefrescar = new System.Windows.Forms.ToolStripButton();
            tsLblContador = new System.Windows.Forms.ToolStripLabel();
            dgvProveedores = new System.Windows.Forms.DataGridView();
            panelEdicion = new System.Windows.Forms.Panel();
            tlpMain = new System.Windows.Forms.TableLayoutPanel();
            lblCodigo = new System.Windows.Forms.Label();
            txtCodigo = new System.Windows.Forms.TextBox();
            lblId = new System.Windows.Forms.Label();
            lblIdValor = new System.Windows.Forms.Label();
            lblRazonSocial = new System.Windows.Forms.Label();
            txtRazonSocial = new System.Windows.Forms.TextBox();
            lblNombreComercial = new System.Windows.Forms.Label();
            txtNombreComercial = new System.Windows.Forms.TextBox();
            lblRUT = new System.Windows.Forms.Label();
            txtRUT = new System.Windows.Forms.TextBox();
            lblDireccion = new System.Windows.Forms.Label();
            txtDireccion = new System.Windows.Forms.TextBox();
            lblTelefono = new System.Windows.Forms.Label();
            txtTelefono = new System.Windows.Forms.TextBox();
            lblEmail = new System.Windows.Forms.Label();
            txtEmail = new System.Windows.Forms.TextBox();
            lblContacto = new System.Windows.Forms.Label();
            txtContacto = new System.Windows.Forms.TextBox();
            lblActivo = new System.Windows.Forms.Label();
            chkActivo = new System.Windows.Forms.CheckBox();
            lblFechaCreacion = new System.Windows.Forms.Label();
            lblFechaCreacionValor = new System.Windows.Forms.Label();
            lblIdTenant = new System.Windows.Forms.Label();
            lblIdTenantValor = new System.Windows.Forms.Label();
            lblTenant = new System.Windows.Forms.Label();
            lblTenantValor = new System.Windows.Forms.Label();
            panelBotones = new System.Windows.Forms.Panel();
            flpBotones = new System.Windows.Forms.FlowLayoutPanel();
            btnGuardar = new System.Windows.Forms.Button();
            btnCancelar = new System.Windows.Forms.Button();

            panelTitulo.SuspendLayout();
            tsProveedores.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProveedores).BeginInit();
            panelEdicion.SuspendLayout();
            tlpMain.SuspendLayout();
            panelBotones.SuspendLayout();
            flpBotones.SuspendLayout();
            SuspendLayout();

            // ── Panel header azul ─────────────────────────────────────────────
            panelTitulo.BackColor = System.Drawing.Color.FromArgb(30, 60, 114);
            panelTitulo.Controls.Add(lblTitulo);
            panelTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            panelTitulo.Name = "panelTitulo";
            panelTitulo.Size = new System.Drawing.Size(984, 44);
            panelTitulo.TabIndex = 0;

            lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            lblTitulo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblTitulo.ForeColor = System.Drawing.Color.White;
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Text = "MANTENEDOR DE PROVEEDORES";
            lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ── ToolStrip ─────────────────────────────────────────────────────
            tsProveedores.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            tsProveedores.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            tsProveedores.Dock = System.Windows.Forms.DockStyle.Top;
            tsProveedores.Location = new System.Drawing.Point(0, 44);
            tsProveedores.Name = "tsProveedores";
            tsProveedores.Size = new System.Drawing.Size(984, 27);
            tsProveedores.TabIndex = 1;
            tsProveedores.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                btnNuevo, btnEditar, btnEliminar, tsSep1, btnRefrescar, tsLblContador
            });

            btnNuevo.Image = global::Inventa.Properties.Resources.add;
            btnNuevo.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Text = "Nuevo";
            btnNuevo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnNuevo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            btnNuevo.ToolTipText = "Nuevo proveedor (Ins)";
            btnNuevo.Click += BtnNuevo_Click;

            btnEditar.Image = global::Inventa.Properties.Resources.edit;
            btnEditar.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnEditar.Name = "btnEditar";
            btnEditar.Text = "Editar";
            btnEditar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnEditar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            btnEditar.Enabled = false;
            btnEditar.Click += BtnEditar_Click;

            btnEliminar.Image = global::Inventa.Properties.Resources.delete;
            btnEliminar.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Text = "Eliminar";
            btnEliminar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnEliminar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            btnEliminar.Enabled = false;
            btnEliminar.Click += BtnEliminar_Click;

            tsSep1.Name = "tsSep1";

            btnRefrescar.Image = global::Inventa.Properties.Resources.refresh;
            btnRefrescar.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Text = "Refrescar (F5)";
            btnRefrescar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnRefrescar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            btnRefrescar.Click += BtnRefrescar_Click;

            tsLblContador.Name = "tsLblContador";
            tsLblContador.Text = "";
            tsLblContador.Font = new System.Drawing.Font("Segoe UI", 8F);
            tsLblContador.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            tsLblContador.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;

            // ── DataGridView ──────────────────────────────────────────────────
            dgvProveedores.AllowUserToAddRows = false;
            dgvProveedores.AllowUserToDeleteRows = false;
            dgvProveedores.AutoGenerateColumns = false;
            dgvProveedores.BackgroundColor = System.Drawing.Color.White;
            dgvProveedores.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dgvProveedores.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dgvProveedores.GridColor = System.Drawing.Color.FromArgb(237, 235, 233);
            dgvProveedores.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvProveedores.ColumnHeadersHeight = 36;
            dgvProveedores.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(243, 242, 241);
            dgvProveedores.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            dgvProveedores.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dgvProveedores.ColumnHeadersDefaultCellStyle.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            dgvProveedores.EnableHeadersVisualStyles = false;
            dgvProveedores.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            dgvProveedores.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            dgvProveedores.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            dgvProveedores.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(229, 243, 255);
            dgvProveedores.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            dgvProveedores.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            dgvProveedores.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(250, 249, 248);
            dgvProveedores.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvProveedores.MultiSelect = false;
            dgvProveedores.Name = "dgvProveedores";
            dgvProveedores.ReadOnly = true;
            dgvProveedores.RowHeadersVisible = false;
            dgvProveedores.RowTemplate.Height = 32;
            dgvProveedores.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvProveedores.TabIndex = 2;

            // ── Panel edición (Dock=Bottom) ───────────────────────────────────
            panelEdicion.BackColor = System.Drawing.Color.White;
            panelEdicion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelEdicion.Controls.Add(tlpMain);
            panelEdicion.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelEdicion.Name = "panelEdicion";
            panelEdicion.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            panelEdicion.Size = new System.Drawing.Size(984, 242);
            panelEdicion.TabIndex = 3;

            // tlpMain — 4 col × 7 filas
            tlpMain.ColumnCount = 4;
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            tlpMain.RowCount = 7;
            for (int i = 0; i < 7; i++)
                tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tlpMain.Controls.Add(lblCodigo, 0, 0);
            tlpMain.Controls.Add(txtCodigo, 1, 0);
            tlpMain.Controls.Add(lblId, 2, 0);
            tlpMain.Controls.Add(lblIdValor, 3, 0);
            tlpMain.Controls.Add(lblRazonSocial, 0, 1);
            tlpMain.Controls.Add(txtRazonSocial, 1, 1);
            tlpMain.Controls.Add(lblNombreComercial, 2, 1);
            tlpMain.Controls.Add(txtNombreComercial, 3, 1);
            tlpMain.Controls.Add(lblRUT, 0, 2);
            tlpMain.Controls.Add(txtRUT, 1, 2);
            tlpMain.Controls.Add(lblDireccion, 2, 2);
            tlpMain.Controls.Add(txtDireccion, 3, 2);
            tlpMain.Controls.Add(lblTelefono, 0, 3);
            tlpMain.Controls.Add(txtTelefono, 1, 3);
            tlpMain.Controls.Add(lblEmail, 2, 3);
            tlpMain.Controls.Add(txtEmail, 3, 3);
            tlpMain.Controls.Add(lblContacto, 0, 4);
            tlpMain.Controls.Add(txtContacto, 1, 4);
            tlpMain.Controls.Add(lblActivo, 2, 4);
            tlpMain.Controls.Add(chkActivo, 3, 4);
            tlpMain.Controls.Add(lblFechaCreacion, 0, 5);
            tlpMain.Controls.Add(lblFechaCreacionValor, 1, 5);
            tlpMain.Controls.Add(lblIdTenant, 2, 5);
            tlpMain.Controls.Add(lblIdTenantValor, 3, 5);
            tlpMain.Controls.Add(lblTenant, 0, 6);
            tlpMain.Controls.Add(lblTenantValor, 1, 6);
            tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpMain.Name = "tlpMain";
            tlpMain.TabIndex = 0;

            // Etiquetas
            lblCodigo.Dock = System.Windows.Forms.DockStyle.Fill;
            lblCodigo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblCodigo.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblCodigo.Name = "lblCodigo";
            lblCodigo.Text = "Codigo:";
            lblCodigo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblId.Dock = System.Windows.Forms.DockStyle.Fill;
            lblId.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblId.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblId.Name = "lblId";
            lblId.Text = "ID:";
            lblId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblIdValor.Dock = System.Windows.Forms.DockStyle.Fill;
            lblIdValor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblIdValor.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            lblIdValor.Name = "lblIdValor";
            lblIdValor.Text = "<< Nuevo >>";
            lblIdValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblRazonSocial.Dock = System.Windows.Forms.DockStyle.Fill;
            lblRazonSocial.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblRazonSocial.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblRazonSocial.Name = "lblRazonSocial";
            lblRazonSocial.Text = "Razon Social:";
            lblRazonSocial.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblNombreComercial.Dock = System.Windows.Forms.DockStyle.Fill;
            lblNombreComercial.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblNombreComercial.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblNombreComercial.Name = "lblNombreComercial";
            lblNombreComercial.Text = "Nombre Comercial:";
            lblNombreComercial.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblRUT.Dock = System.Windows.Forms.DockStyle.Fill;
            lblRUT.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblRUT.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblRUT.Name = "lblRUT";
            lblRUT.Text = "RUT:";
            lblRUT.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblDireccion.Dock = System.Windows.Forms.DockStyle.Fill;
            lblDireccion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblDireccion.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblDireccion.Name = "lblDireccion";
            lblDireccion.Text = "Direccion:";
            lblDireccion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblTelefono.Dock = System.Windows.Forms.DockStyle.Fill;
            lblTelefono.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblTelefono.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblTelefono.Name = "lblTelefono";
            lblTelefono.Text = "Telefono:";
            lblTelefono.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblEmail.Dock = System.Windows.Forms.DockStyle.Fill;
            lblEmail.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblEmail.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblEmail.Name = "lblEmail";
            lblEmail.Text = "Email:";
            lblEmail.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblContacto.Dock = System.Windows.Forms.DockStyle.Fill;
            lblContacto.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblContacto.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblContacto.Name = "lblContacto";
            lblContacto.Text = "Contacto:";
            lblContacto.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblActivo.Dock = System.Windows.Forms.DockStyle.Fill;
            lblActivo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblActivo.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblActivo.Name = "lblActivo";
            lblActivo.Text = "Activo:";
            lblActivo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblFechaCreacion.Dock = System.Windows.Forms.DockStyle.Fill;
            lblFechaCreacion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblFechaCreacion.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblFechaCreacion.Name = "lblFechaCreacion";
            lblFechaCreacion.Text = "F. Creacion:";
            lblFechaCreacion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblFechaCreacionValor.Dock = System.Windows.Forms.DockStyle.Fill;
            lblFechaCreacionValor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblFechaCreacionValor.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            lblFechaCreacionValor.Name = "lblFechaCreacionValor";
            lblFechaCreacionValor.Text = "-";
            lblFechaCreacionValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblIdTenant.Dock = System.Windows.Forms.DockStyle.Fill;
            lblIdTenant.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblIdTenant.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblIdTenant.Name = "lblIdTenant";
            lblIdTenant.Text = "ID Tenant:";
            lblIdTenant.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblIdTenantValor.Dock = System.Windows.Forms.DockStyle.Fill;
            lblIdTenantValor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblIdTenantValor.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            lblIdTenantValor.Name = "lblIdTenantValor";
            lblIdTenantValor.Text = "-";
            lblIdTenantValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblTenant.Dock = System.Windows.Forms.DockStyle.Fill;
            lblTenant.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            lblTenant.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblTenant.Name = "lblTenant";
            lblTenant.Text = "Empresa:";
            lblTenant.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblTenantValor.Dock = System.Windows.Forms.DockStyle.Fill;
            lblTenantValor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblTenantValor.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            lblTenantValor.Name = "lblTenantValor";
            lblTenantValor.Text = "-";
            lblTenantValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Controles de entrada
            txtCodigo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            txtCodigo.Dock = System.Windows.Forms.DockStyle.Fill;
            txtCodigo.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtCodigo.MaxLength = 50; txtCodigo.Name = "txtCodigo"; txtCodigo.TabIndex = 1;

            txtRazonSocial.Dock = System.Windows.Forms.DockStyle.Fill;
            txtRazonSocial.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtRazonSocial.MaxLength = 200; txtRazonSocial.Name = "txtRazonSocial"; txtRazonSocial.TabIndex = 2;

            txtNombreComercial.Dock = System.Windows.Forms.DockStyle.Fill;
            txtNombreComercial.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtNombreComercial.MaxLength = 200; txtNombreComercial.Name = "txtNombreComercial"; txtNombreComercial.TabIndex = 3;

            txtRUT.Dock = System.Windows.Forms.DockStyle.Fill;
            txtRUT.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtRUT.MaxLength = 20; txtRUT.Name = "txtRUT"; txtRUT.TabIndex = 4;

            txtDireccion.Dock = System.Windows.Forms.DockStyle.Fill;
            txtDireccion.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtDireccion.MaxLength = 200; txtDireccion.Name = "txtDireccion"; txtDireccion.TabIndex = 5;

            txtTelefono.Dock = System.Windows.Forms.DockStyle.Fill;
            txtTelefono.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtTelefono.MaxLength = 50; txtTelefono.Name = "txtTelefono"; txtTelefono.TabIndex = 6;

            txtEmail.Dock = System.Windows.Forms.DockStyle.Fill;
            txtEmail.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtEmail.MaxLength = 100; txtEmail.Name = "txtEmail"; txtEmail.TabIndex = 7;

            txtContacto.Dock = System.Windows.Forms.DockStyle.Fill;
            txtContacto.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtContacto.MaxLength = 100; txtContacto.Name = "txtContacto"; txtContacto.TabIndex = 8;

            chkActivo.AutoSize = true; chkActivo.Checked = true;
            chkActivo.CheckState = System.Windows.Forms.CheckState.Checked;
            chkActivo.Dock = System.Windows.Forms.DockStyle.Fill;
            chkActivo.Font = new System.Drawing.Font("Segoe UI", 9F);
            chkActivo.Name = "chkActivo"; chkActivo.TabIndex = 9;

            // ── Panel botones ─────────────────────────────────────────────────
            panelBotones.BackColor = System.Drawing.Color.FromArgb(243, 242, 241);
            panelBotones.Controls.Add(flpBotones);
            panelBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new System.Drawing.Size(984, 54);
            panelBotones.TabIndex = 4;

            flpBotones.Controls.Add(btnGuardar);
            flpBotones.Controls.Add(btnCancelar);
            flpBotones.Dock = System.Windows.Forms.DockStyle.Fill;
            flpBotones.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flpBotones.Name = "flpBotones";
            flpBotones.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            flpBotones.WrapContents = false;

            btnGuardar.BackColor = System.Drawing.Color.FromArgb(30, 60, 114);
            btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnGuardar.ForeColor = System.Drawing.Color.White;
            btnGuardar.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new System.Drawing.Size(110, 36);
            btnGuardar.TabIndex = 0;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += BtnGuardar_Click;

            btnCancelar.BackColor = System.Drawing.Color.White;
            btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(138, 136, 134);
            btnCancelar.FlatAppearance.BorderSize = 1;
            btnCancelar.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnCancelar.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            btnCancelar.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new System.Drawing.Size(110, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += BtnCancelar_Click;

            // ── Formulario ────────────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(984, 730);
            Controls.Add(dgvProveedores);
            Controls.Add(tsProveedores);
            Controls.Add(panelEdicion);
            Controls.Add(panelBotones);
            Controls.Add(panelTitulo);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            MinimumSize = new System.Drawing.Size(860, 620);
            Name = "frm_Proveedor";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Mantenedor de Proveedores";
            Load += Frm_Proveedor_Load;

            panelTitulo.ResumeLayout(false);
            tsProveedores.ResumeLayout(false);
            tsProveedores.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProveedores).EndInit();
            panelEdicion.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            panelBotones.ResumeLayout(false);
            flpBotones.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}