namespace Inventa.Forms.Mantenedores
{
    partial class frm_Marca
    {
        private System.ComponentModel.IContainer components = null;

        // ── Panel header ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelTitulo;
        private System.Windows.Forms.Label lblTitulo;

        // ── ToolStrip ─────────────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip tsMarcas;
        private System.Windows.Forms.ToolStripButton btnNuevo;
        private System.Windows.Forms.ToolStripButton btnEditar;
        private System.Windows.Forms.ToolStripButton btnEliminar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton btnRefrescar;
        private System.Windows.Forms.ToolStripLabel tsLblContador;

        // ── Grilla ────────────────────────────────────────────────────────────
        private System.Windows.Forms.DataGridView dgvMarcas;

        // ── Panel edición (Dock=Bottom) ───────────────────────────────────────
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

        // Fila 2: F.Creacion | ID Tenant
        private System.Windows.Forms.Label lblFechaCreacion;
        private System.Windows.Forms.Label lblFechaCreacionValor;
        private System.Windows.Forms.Label lblIdTenant;
        private System.Windows.Forms.Label lblIdTenantValor;

        // Fila 3: Empresa (span)
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
            tsMarcas = new System.Windows.Forms.ToolStrip();
            btnNuevo = new System.Windows.Forms.ToolStripButton();
            btnEditar = new System.Windows.Forms.ToolStripButton();
            btnEliminar = new System.Windows.Forms.ToolStripButton();
            tsSep1 = new System.Windows.Forms.ToolStripSeparator();
            btnRefrescar = new System.Windows.Forms.ToolStripButton();
            tsLblContador = new System.Windows.Forms.ToolStripLabel();
            dgvMarcas = new System.Windows.Forms.DataGridView();
            panelEdicion = new System.Windows.Forms.Panel();
            tlpMain = new System.Windows.Forms.TableLayoutPanel();
            lblCodigo = new System.Windows.Forms.Label();
            txtCodigo = new System.Windows.Forms.TextBox();
            lblId = new System.Windows.Forms.Label();
            lblIdValor = new System.Windows.Forms.Label();
            lblNombre = new System.Windows.Forms.Label();
            txtNombre = new System.Windows.Forms.TextBox();
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
            tsMarcas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMarcas).BeginInit();
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
            panelTitulo.Size = new System.Drawing.Size(884, 44);
            panelTitulo.TabIndex = 0;

            lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            lblTitulo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblTitulo.ForeColor = System.Drawing.Color.White;
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Text = "MANTENEDOR DE MARCAS";
            lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ── ToolStrip ─────────────────────────────────────────────────────
            tsMarcas.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            tsMarcas.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            tsMarcas.Dock = System.Windows.Forms.DockStyle.Top;
            tsMarcas.Location = new System.Drawing.Point(0, 44);
            tsMarcas.Name = "tsMarcas";
            tsMarcas.Size = new System.Drawing.Size(884, 27);
            tsMarcas.TabIndex = 1;
            tsMarcas.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                btnNuevo, btnEditar, btnEliminar, tsSep1, btnRefrescar, tsLblContador
            });

            btnNuevo.Image = global::Inventa.Properties.Resources.add;
            btnNuevo.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Text = "Nuevo";
            btnNuevo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnNuevo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            btnNuevo.ToolTipText = "Nueva marca (Ins)";
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
            dgvMarcas.AllowUserToAddRows = false;
            dgvMarcas.AllowUserToDeleteRows = false;
            dgvMarcas.AutoGenerateColumns = false;
            dgvMarcas.BackgroundColor = System.Drawing.Color.White;
            dgvMarcas.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dgvMarcas.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dgvMarcas.GridColor = System.Drawing.Color.FromArgb(237, 235, 233);
            dgvMarcas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvMarcas.ColumnHeadersHeight = 36;
            dgvMarcas.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(243, 242, 241);
            dgvMarcas.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            dgvMarcas.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dgvMarcas.ColumnHeadersDefaultCellStyle.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            dgvMarcas.EnableHeadersVisualStyles = false;
            dgvMarcas.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            dgvMarcas.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            dgvMarcas.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            dgvMarcas.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(229, 243, 255);
            dgvMarcas.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            dgvMarcas.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            dgvMarcas.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(250, 249, 248);
            dgvMarcas.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvMarcas.MultiSelect = false;
            dgvMarcas.Name = "dgvMarcas";
            dgvMarcas.ReadOnly = true;
            dgvMarcas.RowHeadersVisible = false;
            dgvMarcas.RowTemplate.Height = 32;
            dgvMarcas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvMarcas.TabIndex = 2;

            // ── Panel edición (Dock=Bottom) ───────────────────────────────────
            panelEdicion.BackColor = System.Drawing.Color.White;
            panelEdicion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelEdicion.Controls.Add(tlpMain);
            panelEdicion.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelEdicion.Name = "panelEdicion";
            panelEdicion.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            panelEdicion.Size = new System.Drawing.Size(884, 142);
            panelEdicion.TabIndex = 3;

            // tlpMain — 4 col × 4 filas
            tlpMain.ColumnCount = 4;
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            tlpMain.RowCount = 4;
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tlpMain.Controls.Add(lblCodigo, 0, 0);
            tlpMain.Controls.Add(txtCodigo, 1, 0);
            tlpMain.Controls.Add(lblId, 2, 0);
            tlpMain.Controls.Add(lblIdValor, 3, 0);
            tlpMain.Controls.Add(lblNombre, 0, 1);
            tlpMain.Controls.Add(txtNombre, 1, 1);
            tlpMain.Controls.Add(lblActivo, 2, 1);
            tlpMain.Controls.Add(chkActivo, 3, 1);
            tlpMain.Controls.Add(lblFechaCreacion, 0, 2);
            tlpMain.Controls.Add(lblFechaCreacionValor, 1, 2);
            tlpMain.Controls.Add(lblIdTenant, 2, 2);
            tlpMain.Controls.Add(lblIdTenantValor, 3, 2);
            tlpMain.Controls.Add(lblTenant, 0, 3);
            tlpMain.Controls.Add(lblTenantValor, 1, 3);
            tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpMain.Name = "tlpMain";
            tlpMain.TabIndex = 0;

            // Etiquetas
            lblCodigo.Dock = System.Windows.Forms.DockStyle.Fill;
            lblCodigo.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblCodigo.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblCodigo.Name = "lblCodigo"; lblCodigo.Text = "Codigo:";
            lblCodigo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblId.Dock = System.Windows.Forms.DockStyle.Fill;
            lblId.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblId.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblId.Name = "lblId"; lblId.Text = "ID:";
            lblId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblIdValor.Dock = System.Windows.Forms.DockStyle.Fill;
            lblIdValor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblIdValor.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            lblIdValor.Name = "lblIdValor"; lblIdValor.Text = "<< Nuevo >>";
            lblIdValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblNombre.Dock = System.Windows.Forms.DockStyle.Fill;
            lblNombre.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblNombre.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblNombre.Name = "lblNombre"; lblNombre.Text = "Nombre:";
            lblNombre.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblActivo.Dock = System.Windows.Forms.DockStyle.Fill;
            lblActivo.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblActivo.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblActivo.Name = "lblActivo"; lblActivo.Text = "Activo:";
            lblActivo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblFechaCreacion.Dock = System.Windows.Forms.DockStyle.Fill;
            lblFechaCreacion.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblFechaCreacion.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblFechaCreacion.Name = "lblFechaCreacion"; lblFechaCreacion.Text = "F. Creacion:";
            lblFechaCreacion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblFechaCreacionValor.Dock = System.Windows.Forms.DockStyle.Fill;
            lblFechaCreacionValor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblFechaCreacionValor.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            lblFechaCreacionValor.Name = "lblFechaCreacionValor"; lblFechaCreacionValor.Text = "-";
            lblFechaCreacionValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblIdTenant.Dock = System.Windows.Forms.DockStyle.Fill;
            lblIdTenant.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblIdTenant.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblIdTenant.Name = "lblIdTenant"; lblIdTenant.Text = "ID Tenant:";
            lblIdTenant.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblIdTenantValor.Dock = System.Windows.Forms.DockStyle.Fill;
            lblIdTenantValor.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblIdTenantValor.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            lblIdTenantValor.Name = "lblIdTenantValor"; lblIdTenantValor.Text = "-";
            lblIdTenantValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblTenant.Dock = System.Windows.Forms.DockStyle.Fill;
            lblTenant.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblTenant.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblTenant.Name = "lblTenant"; lblTenant.Text = "Empresa:";
            lblTenant.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblTenantValor.Dock = System.Windows.Forms.DockStyle.Fill;
            lblTenantValor.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblTenantValor.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            lblTenantValor.Name = "lblTenantValor"; lblTenantValor.Text = "-";
            lblTenantValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Controles de entrada
            txtCodigo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            txtCodigo.Dock = System.Windows.Forms.DockStyle.Fill;
            txtCodigo.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtCodigo.MaxLength = 50;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.TabIndex = 1;

            txtNombre.Dock = System.Windows.Forms.DockStyle.Fill;
            txtNombre.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtNombre.MaxLength = 100;
            txtNombre.Name = "txtNombre";
            txtNombre.TabIndex = 2;

            chkActivo.AutoSize = true;
            chkActivo.Checked = true;
            chkActivo.CheckState = System.Windows.Forms.CheckState.Checked;
            chkActivo.Dock = System.Windows.Forms.DockStyle.Fill;
            chkActivo.Font = new System.Drawing.Font("Segoe UI", 9F);
            chkActivo.Name = "chkActivo";
            chkActivo.TabIndex = 3;

            // ── Panel botones ─────────────────────────────────────────────────
            panelBotones.BackColor = System.Drawing.Color.FromArgb(243, 242, 241);
            panelBotones.Controls.Add(flpBotones);
            panelBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new System.Drawing.Size(884, 54);
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
            ClientSize = new System.Drawing.Size(884, 600);
            Controls.Add(dgvMarcas);
            Controls.Add(tsMarcas);
            Controls.Add(panelEdicion);
            Controls.Add(panelBotones);
            Controls.Add(panelTitulo);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            MinimumSize = new System.Drawing.Size(760, 520);
            Name = "frm_Marca";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Mantenedor de Marcas";
            Load += Frm_Marca_Load;

            panelTitulo.ResumeLayout(false);
            tsMarcas.ResumeLayout(false);
            tsMarcas.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMarcas).EndInit();
            panelEdicion.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            panelBotones.ResumeLayout(false);
            flpBotones.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}