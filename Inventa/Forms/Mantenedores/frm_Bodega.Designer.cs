namespace Inventa.Forms.Mantenedores
{
    partial class frm_Bodega
    {
        private System.ComponentModel.IContainer components = null;

        // ── ToolStrip ─────────────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip tsBodegas;
        private System.Windows.Forms.ToolStripButton btnNuevo;
        private System.Windows.Forms.ToolStripButton btnEditar;
        private System.Windows.Forms.ToolStripButton btnEliminar;
        private System.Windows.Forms.ToolStripButton btnRefrescar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripLabel tsLblContador;

        // ── Grilla ────────────────────────────────────────────────────────────
        private System.Windows.Forms.DataGridView dgvBodegas;

        // ── Panel título ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelTitulo;
        private System.Windows.Forms.Label lblTitulo;

        // ── Panel edición (bottom) ────────────────────────────────────────────
        private System.Windows.Forms.Panel panelEdicion;
        private System.Windows.Forms.TableLayoutPanel tlpCampos;
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.Label lblIdValor;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblActivo;
        private System.Windows.Forms.CheckBox chkActivo;
        private System.Windows.Forms.Label lblIdTenant;
        private System.Windows.Forms.Label lblIdTenantValor;
        private System.Windows.Forms.Label lblTenant;
        private System.Windows.Forms.Label lblTenantValor;

        // ── Panel botones ─────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.TableLayoutPanel tlpBotones;
        private System.Windows.Forms.FlowLayoutPanel flpBotonesDer;
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
            tsBodegas = new ToolStrip();
            btnNuevo = new ToolStripButton();
            btnEditar = new ToolStripButton();
            btnEliminar = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            btnRefrescar = new ToolStripButton();
            tsLblContador = new ToolStripLabel();
            dgvBodegas = new DataGridView();
            panelTitulo = new Panel();
            lblTitulo = new Label();
            panelEdicion = new Panel();
            tlpCampos = new TableLayoutPanel();
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
            panelBotones = new Panel();
            tlpBotones = new TableLayoutPanel();
            flpBotonesDer = new FlowLayoutPanel();
            btnGuardar = new Button();
            btnCancelar = new Button();
            tsBodegas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBodegas).BeginInit();
            panelTitulo.SuspendLayout();
            panelEdicion.SuspendLayout();
            tlpCampos.SuspendLayout();
            panelBotones.SuspendLayout();
            tlpBotones.SuspendLayout();
            flpBotonesDer.SuspendLayout();
            SuspendLayout();
            // 
            // tsBodegas
            // 
            tsBodegas.GripStyle = ToolStripGripStyle.Hidden;
            tsBodegas.Items.AddRange(new ToolStripItem[] { btnNuevo, btnEditar, btnEliminar, tsSep1, btnRefrescar, tsLblContador });
            tsBodegas.Location = new Point(0, 44);
            tsBodegas.Name = "tsBodegas";
            tsBodegas.RenderMode = ToolStripRenderMode.System;
            tsBodegas.Size = new Size(884, 25);
            tsBodegas.TabIndex = 1;
            // 
            // btnNuevo
            // 
            btnNuevo.Image = Properties.Resources.add;
            btnNuevo.ImageTransparentColor = Color.Magenta;
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(62, 22);
            btnNuevo.Text = "Nuevo";
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
            btnRefrescar.Size = new Size(75, 22);
            btnRefrescar.Text = "Refrescar";
            btnRefrescar.Click += BtnRefrescar_Click;
            // 
            // tsLblContador
            // 
            tsLblContador.Alignment = ToolStripItemAlignment.Right;
            tsLblContador.Font = new Font("Segoe UI", 8F);
            tsLblContador.ForeColor = Color.FromArgb(96, 94, 92);
            tsLblContador.Name = "tsLblContador";
            tsLblContador.Size = new Size(61, 22);
            tsLblContador.Text = "0 bodegas";
            // 
            // dgvBodegas
            // 
            dgvBodegas.AllowUserToAddRows = false;
            dgvBodegas.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(250, 249, 248);
            dgvBodegas.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvBodegas.BackgroundColor = Color.White;
            dgvBodegas.BorderStyle = BorderStyle.None;
            dgvBodegas.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(243, 242, 241);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvBodegas.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvBodegas.ColumnHeadersHeight = 36;
            dgvBodegas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(96, 94, 92);
            dataGridViewCellStyle3.Padding = new Padding(8, 2, 8, 2);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(229, 243, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvBodegas.DefaultCellStyle = dataGridViewCellStyle3;
            dgvBodegas.Dock = DockStyle.Fill;
            dgvBodegas.EnableHeadersVisualStyles = false;
            dgvBodegas.GridColor = Color.FromArgb(237, 235, 233);
            dgvBodegas.Location = new Point(0, 69);
            dgvBodegas.MultiSelect = false;
            dgvBodegas.Name = "dgvBodegas";
            dgvBodegas.ReadOnly = true;
            dgvBodegas.RowHeadersVisible = false;
            dgvBodegas.RowTemplate.Height = 32;
            dgvBodegas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBodegas.Size = new Size(884, 297);
            dgvBodegas.TabIndex = 2;
            dgvBodegas.SelectionChanged += DgvBodegas_SelectionChanged;
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
            lblTitulo.Text = "MANTENEDOR DE BODEGAS";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelEdicion
            // 
            panelEdicion.BackColor = Color.White;
            panelEdicion.BorderStyle = BorderStyle.FixedSingle;
            panelEdicion.Controls.Add(tlpCampos);
            panelEdicion.Dock = DockStyle.Bottom;
            panelEdicion.Location = new Point(0, 366);
            panelEdicion.Name = "panelEdicion";
            panelEdicion.Padding = new Padding(8);
            panelEdicion.Size = new Size(884, 134);
            panelEdicion.TabIndex = 3;
            // 
            // tlpCampos
            // 
            tlpCampos.ColumnCount = 4;
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpCampos.Controls.Add(lblCodigo, 0, 0);
            tlpCampos.Controls.Add(txtCodigo, 1, 0);
            tlpCampos.Controls.Add(lblId, 2, 0);
            tlpCampos.Controls.Add(lblIdValor, 3, 0);
            tlpCampos.Controls.Add(lblNombre, 0, 1);
            tlpCampos.Controls.Add(txtNombre, 1, 1);
            tlpCampos.Controls.Add(lblActivo, 2, 1);
            tlpCampos.Controls.Add(chkActivo, 3, 1);
            tlpCampos.Controls.Add(lblIdTenant, 0, 2);
            tlpCampos.Controls.Add(lblIdTenantValor, 1, 2);
            tlpCampos.Controls.Add(lblTenant, 2, 2);
            tlpCampos.Controls.Add(lblTenantValor, 3, 2);
            tlpCampos.Dock = DockStyle.Fill;
            tlpCampos.Location = new Point(8, 8);
            tlpCampos.Name = "tlpCampos";
            tlpCampos.Padding = new Padding(4);
            tlpCampos.RowCount = 3;
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpCampos.Size = new Size(866, 116);
            tlpCampos.TabIndex = 0;
            // 
            // lblCodigo
            // 
            lblCodigo.Dock = DockStyle.Fill;
            lblCodigo.Font = new Font("Segoe UI", 9F);
            lblCodigo.ForeColor = Color.FromArgb(96, 94, 92);
            lblCodigo.Location = new Point(7, 4);
            lblCodigo.Name = "lblCodigo";
            lblCodigo.Size = new Size(99, 36);
            lblCodigo.TabIndex = 0;
            lblCodigo.Text = "Codigo:";
            lblCodigo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtCodigo
            // 
            txtCodigo.CharacterCasing = CharacterCasing.Upper;
            txtCodigo.Dock = DockStyle.Fill;
            txtCodigo.Font = new Font("Segoe UI", 9F);
            txtCodigo.Location = new Point(112, 7);
            txtCodigo.MaxLength = 20;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.Size = new Size(253, 23);
            txtCodigo.TabIndex = 1;
            // 
            // lblId
            // 
            lblId.Dock = DockStyle.Fill;
            lblId.Font = new Font("Segoe UI", 9F);
            lblId.ForeColor = Color.FromArgb(96, 94, 92);
            lblId.Location = new Point(371, 4);
            lblId.Name = "lblId";
            lblId.Size = new Size(99, 36);
            lblId.TabIndex = 2;
            lblId.Text = "ID:";
            lblId.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblIdValor
            // 
            lblIdValor.Dock = DockStyle.Fill;
            lblIdValor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblIdValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblIdValor.Location = new Point(476, 4);
            lblIdValor.Name = "lblIdValor";
            lblIdValor.Size = new Size(383, 36);
            lblIdValor.TabIndex = 3;
            lblIdValor.Text = "<< Nuevo >>";
            lblIdValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblNombre
            // 
            lblNombre.Dock = DockStyle.Fill;
            lblNombre.Font = new Font("Segoe UI", 9F);
            lblNombre.ForeColor = Color.FromArgb(96, 94, 92);
            lblNombre.Location = new Point(7, 40);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(99, 36);
            lblNombre.TabIndex = 4;
            lblNombre.Text = "Nombre:";
            lblNombre.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtNombre
            // 
            txtNombre.Dock = DockStyle.Fill;
            txtNombre.Font = new Font("Segoe UI", 9F);
            txtNombre.Location = new Point(112, 43);
            txtNombre.MaxLength = 100;
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(253, 23);
            txtNombre.TabIndex = 2;
            // 
            // lblActivo
            // 
            lblActivo.Dock = DockStyle.Fill;
            lblActivo.Font = new Font("Segoe UI", 9F);
            lblActivo.ForeColor = Color.FromArgb(96, 94, 92);
            lblActivo.Location = new Point(371, 40);
            lblActivo.Name = "lblActivo";
            lblActivo.Size = new Size(99, 36);
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
            chkActivo.Location = new Point(476, 43);
            chkActivo.Name = "chkActivo";
            chkActivo.Size = new Size(383, 30);
            chkActivo.TabIndex = 3;
            // 
            // lblIdTenant
            // 
            lblIdTenant.Dock = DockStyle.Fill;
            lblIdTenant.Font = new Font("Segoe UI", 9F);
            lblIdTenant.ForeColor = Color.FromArgb(96, 94, 92);
            lblIdTenant.Location = new Point(7, 76);
            lblIdTenant.Name = "lblIdTenant";
            lblIdTenant.Size = new Size(99, 36);
            lblIdTenant.TabIndex = 6;
            lblIdTenant.Text = "ID Tenant:";
            lblIdTenant.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblIdTenantValor
            // 
            lblIdTenantValor.Dock = DockStyle.Fill;
            lblIdTenantValor.Font = new Font("Segoe UI", 9F);
            lblIdTenantValor.ForeColor = Color.FromArgb(96, 94, 92);
            lblIdTenantValor.Location = new Point(112, 76);
            lblIdTenantValor.Name = "lblIdTenantValor";
            lblIdTenantValor.Size = new Size(253, 36);
            lblIdTenantValor.TabIndex = 7;
            lblIdTenantValor.Text = "-";
            lblIdTenantValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTenant
            // 
            lblTenant.Dock = DockStyle.Fill;
            lblTenant.Font = new Font("Segoe UI", 9F);
            lblTenant.ForeColor = Color.FromArgb(96, 94, 92);
            lblTenant.Location = new Point(371, 76);
            lblTenant.Name = "lblTenant";
            lblTenant.Size = new Size(99, 36);
            lblTenant.TabIndex = 8;
            lblTenant.Text = "Tenant:";
            lblTenant.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTenantValor
            // 
            lblTenantValor.Dock = DockStyle.Fill;
            lblTenantValor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTenantValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblTenantValor.Location = new Point(476, 76);
            lblTenantValor.Name = "lblTenantValor";
            lblTenantValor.Size = new Size(383, 36);
            lblTenantValor.TabIndex = 9;
            lblTenantValor.Text = "-";
            lblTenantValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(243, 242, 241);
            panelBotones.Controls.Add(tlpBotones);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 500);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(884, 60);
            panelBotones.TabIndex = 4;
            // 
            // tlpBotones
            // 
            tlpBotones.ColumnCount = 2;
            tlpBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpBotones.Controls.Add(flpBotonesDer, 1, 0);
            tlpBotones.Dock = DockStyle.Fill;
            tlpBotones.Location = new Point(0, 0);
            tlpBotones.Name = "tlpBotones";
            tlpBotones.Padding = new Padding(8, 10, 8, 10);
            tlpBotones.RowCount = 1;
            tlpBotones.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpBotones.Size = new Size(884, 60);
            tlpBotones.TabIndex = 0;
            // 
            // flpBotonesDer
            // 
            flpBotonesDer.Controls.Add(btnGuardar);
            flpBotonesDer.Controls.Add(btnCancelar);
            flpBotonesDer.Dock = DockStyle.Fill;
            flpBotonesDer.FlowDirection = FlowDirection.RightToLeft;
            flpBotonesDer.Location = new Point(445, 13);
            flpBotonesDer.Name = "flpBotonesDer";
            flpBotonesDer.Size = new Size(428, 34);
            flpBotonesDer.TabIndex = 0;
            flpBotonesDer.WrapContents = false;
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.FromArgb(30, 60, 114);
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(318, 0);
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
            btnCancelar.Location = new Point(202, 0);
            btnCancelar.Margin = new Padding(6, 0, 0, 0);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += BtnCancelar_Click;
            // 
            // frm_Bodega
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 560);
            Controls.Add(dgvBodegas);
            Controls.Add(tsBodegas);
            Controls.Add(panelEdicion);
            Controls.Add(panelBotones);
            Controls.Add(panelTitulo);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(640, 480);
            Name = "frm_Bodega";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Mantenedor de Bodegas";
            Load += Frm_Bodega_Load;
            tsBodegas.ResumeLayout(false);
            tsBodegas.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBodegas).EndInit();
            panelTitulo.ResumeLayout(false);
            panelEdicion.ResumeLayout(false);
            tlpCampos.ResumeLayout(false);
            tlpCampos.PerformLayout();
            panelBotones.ResumeLayout(false);
            tlpBotones.ResumeLayout(false);
            flpBotonesDer.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}