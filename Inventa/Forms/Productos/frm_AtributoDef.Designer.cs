namespace Inventa.Forms.Productos
{
    partial class frm_AtributoDef
    {
        private System.ComponentModel.IContainer components = null;

        // ── Panel header ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitulo;

        // ── ToolStrip ─────────────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip tsAcciones;
        private System.Windows.Forms.ToolStripButton btnNuevo;
        private System.Windows.Forms.ToolStripButton btnEditar;
        private System.Windows.Forms.ToolStripButton btnEliminar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton btnGestionarValores;
        private System.Windows.Forms.ToolStripSeparator tsSep2;
        private System.Windows.Forms.ToolStripButton btnRefrescar;
        private System.Windows.Forms.ToolStripLabel tsLblContador;

        // ── Grilla ────────────────────────────────────────────────────────────
        private System.Windows.Forms.DataGridView dgvAtriDef;

        // ── Panel edición (Dock=Bottom) ───────────────────────────────────────
        private System.Windows.Forms.Panel panelEdicion;
        private System.Windows.Forms.TableLayoutPanel tlpCampos;

        // Fila 0: Codigo | Nombre
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;

        // Fila 1: EsOblig | Activo
        private System.Windows.Forms.Label lblEsOblig;
        private System.Windows.Forms.CheckBox chkEsOblig;
        private System.Windows.Forms.Label lblActivo;
        private System.Windows.Forms.CheckBox chkActivo;

        // ── Panel botones (Dock=Bottom) ───────────────────────────────────────
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.FlowLayoutPanel flpBotonesDer;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnCerrar;

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
            panelHeader = new Panel();
            lblTitulo = new Label();
            tsAcciones = new ToolStrip();
            btnNuevo = new ToolStripButton();
            btnEditar = new ToolStripButton();
            btnEliminar = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            btnGestionarValores = new ToolStripButton();
            tsSep2 = new ToolStripSeparator();
            btnRefrescar = new ToolStripButton();
            tsLblContador = new ToolStripLabel();
            dgvAtriDef = new DataGridView();
            panelEdicion = new Panel();
            tlpCampos = new TableLayoutPanel();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblNombre = new Label();
            txtNombre = new TextBox();
            lblEsOblig = new Label();
            chkEsOblig = new CheckBox();
            lblActivo = new Label();
            chkActivo = new CheckBox();
            panelBotones = new Panel();
            flpBotonesDer = new FlowLayoutPanel();
            btnCerrar = new Button();
            btnGuardar = new Button();
            btnCancelar = new Button();
            panelHeader.SuspendLayout();
            tsAcciones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAtriDef).BeginInit();
            panelEdicion.SuspendLayout();
            tlpCampos.SuspendLayout();
            panelBotones.SuspendLayout();
            flpBotonesDer.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(30, 60, 114);
            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new Padding(16, 0, 16, 0);
            panelHeader.Size = new Size(760, 44);
            panelHeader.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(16, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(728, 44);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Definicion de Atributos";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tsAcciones
            // 
            tsAcciones.GripStyle = ToolStripGripStyle.Hidden;
            tsAcciones.Items.AddRange(new ToolStripItem[] { btnNuevo, btnEditar, btnEliminar, tsSep1, btnGestionarValores, tsSep2, btnRefrescar, tsLblContador });
            tsAcciones.Location = new Point(0, 44);
            tsAcciones.Name = "tsAcciones";
            tsAcciones.RenderMode = ToolStripRenderMode.System;
            tsAcciones.Size = new Size(760, 25);
            tsAcciones.TabIndex = 1;
            // 
            // btnNuevo
            // 
            btnNuevo.Image = Properties.Resources.add;
            btnNuevo.ImageTransparentColor = Color.Magenta;
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(62, 22);
            btnNuevo.Text = "Nuevo";
            btnNuevo.ToolTipText = "Nuevo atributo (Ins)";
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
            // btnGestionarValores
            // 
            btnGestionarValores.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnGestionarValores.Enabled = false;
            btnGestionarValores.Name = "btnGestionarValores";
            btnGestionarValores.Size = new Size(101, 22);
            btnGestionarValores.Text = "Gestionar valores";
            btnGestionarValores.ToolTipText = "Administrar valores para el atributo seleccionado";
            btnGestionarValores.Click += BtnGestionarValores_Click;
            // 
            // tsSep2
            // 
            tsSep2.Name = "tsSep2";
            tsSep2.Size = new Size(6, 25);
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
            // dgvAtriDef
            // 
            dgvAtriDef.AllowUserToAddRows = false;
            dgvAtriDef.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(250, 249, 248);
            dgvAtriDef.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvAtriDef.BackgroundColor = Color.White;
            dgvAtriDef.BorderStyle = BorderStyle.None;
            dgvAtriDef.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(243, 242, 241);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle2.Padding = new Padding(8, 0, 0, 0);
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvAtriDef.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvAtriDef.ColumnHeadersHeight = 36;
            dgvAtriDef.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(96, 94, 92);
            dataGridViewCellStyle3.Padding = new Padding(8, 2, 8, 2);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(229, 243, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvAtriDef.DefaultCellStyle = dataGridViewCellStyle3;
            dgvAtriDef.Dock = DockStyle.Fill;
            dgvAtriDef.EnableHeadersVisualStyles = false;
            dgvAtriDef.GridColor = Color.FromArgb(237, 235, 233);
            dgvAtriDef.Location = new Point(0, 69);
            dgvAtriDef.MultiSelect = false;
            dgvAtriDef.Name = "dgvAtriDef";
            dgvAtriDef.ReadOnly = true;
            dgvAtriDef.RowHeadersVisible = false;
            dgvAtriDef.RowTemplate.Height = 32;
            dgvAtriDef.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAtriDef.Size = new Size(760, 327);
            dgvAtriDef.TabIndex = 2;
            dgvAtriDef.CellDoubleClick += DgvAtriDef_CellDoubleClick;
            // 
            // panelEdicion
            // 
            panelEdicion.BackColor = Color.White;
            panelEdicion.BorderStyle = BorderStyle.FixedSingle;
            panelEdicion.Controls.Add(tlpCampos);
            panelEdicion.Dock = DockStyle.Bottom;
            panelEdicion.Location = new Point(0, 396);
            panelEdicion.Name = "panelEdicion";
            panelEdicion.Padding = new Padding(8, 6, 8, 6);
            panelEdicion.Size = new Size(760, 90);
            panelEdicion.TabIndex = 3;
            // 
            // tlpCampos
            // 
            tlpCampos.ColumnCount = 4;
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpCampos.Controls.Add(lblCodigo, 0, 0);
            tlpCampos.Controls.Add(txtCodigo, 1, 0);
            tlpCampos.Controls.Add(lblNombre, 2, 0);
            tlpCampos.Controls.Add(txtNombre, 3, 0);
            tlpCampos.Controls.Add(lblEsOblig, 0, 1);
            tlpCampos.Controls.Add(chkEsOblig, 1, 1);
            tlpCampos.Controls.Add(lblActivo, 2, 1);
            tlpCampos.Controls.Add(chkActivo, 3, 1);
            tlpCampos.Dock = DockStyle.Fill;
            tlpCampos.Location = new Point(8, 6);
            tlpCampos.Name = "tlpCampos";
            tlpCampos.RowCount = 2;
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpCampos.Size = new Size(742, 76);
            tlpCampos.TabIndex = 0;
            // 
            // lblCodigo
            // 
            lblCodigo.Dock = DockStyle.Fill;
            lblCodigo.Font = new Font("Segoe UI", 9F);
            lblCodigo.ForeColor = Color.FromArgb(96, 94, 92);
            lblCodigo.Location = new Point(3, 0);
            lblCodigo.Name = "lblCodigo";
            lblCodigo.Size = new Size(84, 36);
            lblCodigo.TabIndex = 0;
            lblCodigo.Text = "Codigo:";
            lblCodigo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtCodigo
            // 
            txtCodigo.CharacterCasing = CharacterCasing.Upper;
            txtCodigo.Dock = DockStyle.Fill;
            txtCodigo.Font = new Font("Segoe UI", 9F);
            txtCodigo.Location = new Point(93, 3);
            txtCodigo.MaxLength = 20;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.Size = new Size(134, 23);
            txtCodigo.TabIndex = 1;
            // 
            // lblNombre
            // 
            lblNombre.Dock = DockStyle.Fill;
            lblNombre.Font = new Font("Segoe UI", 9F);
            lblNombre.ForeColor = Color.FromArgb(96, 94, 92);
            lblNombre.Location = new Point(233, 0);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(84, 36);
            lblNombre.TabIndex = 2;
            lblNombre.Text = "Nombre:";
            lblNombre.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtNombre
            // 
            txtNombre.Dock = DockStyle.Fill;
            txtNombre.Font = new Font("Segoe UI", 9F);
            txtNombre.Location = new Point(323, 3);
            txtNombre.MaxLength = 100;
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(416, 23);
            txtNombre.TabIndex = 2;
            // 
            // lblEsOblig
            // 
            lblEsOblig.Dock = DockStyle.Fill;
            lblEsOblig.Font = new Font("Segoe UI", 9F);
            lblEsOblig.ForeColor = Color.FromArgb(96, 94, 92);
            lblEsOblig.Location = new Point(3, 36);
            lblEsOblig.Name = "lblEsOblig";
            lblEsOblig.Size = new Size(84, 40);
            lblEsOblig.TabIndex = 3;
            lblEsOblig.Text = "Obligatorio:";
            lblEsOblig.TextAlign = ContentAlignment.MiddleRight;
            // 
            // chkEsOblig
            // 
            chkEsOblig.Dock = DockStyle.Fill;
            chkEsOblig.Font = new Font("Segoe UI", 9F);
            chkEsOblig.Location = new Point(93, 39);
            chkEsOblig.Name = "chkEsOblig";
            chkEsOblig.Size = new Size(134, 34);
            chkEsOblig.TabIndex = 3;
            // 
            // lblActivo
            // 
            lblActivo.Dock = DockStyle.Fill;
            lblActivo.Font = new Font("Segoe UI", 9F);
            lblActivo.ForeColor = Color.FromArgb(96, 94, 92);
            lblActivo.Location = new Point(233, 36);
            lblActivo.Name = "lblActivo";
            lblActivo.Size = new Size(84, 40);
            lblActivo.TabIndex = 4;
            lblActivo.Text = "Activo:";
            lblActivo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // chkActivo
            // 
            chkActivo.Checked = true;
            chkActivo.CheckState = CheckState.Checked;
            chkActivo.Dock = DockStyle.Fill;
            chkActivo.Font = new Font("Segoe UI", 9F);
            chkActivo.Location = new Point(323, 39);
            chkActivo.Name = "chkActivo";
            chkActivo.Size = new Size(416, 34);
            chkActivo.TabIndex = 4;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(243, 242, 241);
            panelBotones.Controls.Add(flpBotonesDer);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 486);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(760, 54);
            panelBotones.TabIndex = 4;
            // 
            // flpBotonesDer
            // 
            flpBotonesDer.Controls.Add(btnCerrar);
            flpBotonesDer.Controls.Add(btnGuardar);
            flpBotonesDer.Controls.Add(btnCancelar);
            flpBotonesDer.Dock = DockStyle.Fill;
            flpBotonesDer.FlowDirection = FlowDirection.RightToLeft;
            flpBotonesDer.Location = new Point(0, 0);
            flpBotonesDer.Name = "flpBotonesDer";
            flpBotonesDer.Padding = new Padding(8);
            flpBotonesDer.Size = new Size(760, 54);
            flpBotonesDer.TabIndex = 0;
            flpBotonesDer.WrapContents = false;
            // 
            // btnCerrar
            // 
            btnCerrar.BackColor = Color.FromArgb(30, 60, 114);
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCerrar.ForeColor = Color.White;
            btnCerrar.Location = new Point(644, 8);
            btnCerrar.Margin = new Padding(6, 0, 0, 0);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(100, 36);
            btnCerrar.TabIndex = 2;
            btnCerrar.Text = "Cerrar";
            btnCerrar.UseVisualStyleBackColor = false;
            btnCerrar.Click += BtnCerrar_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.FromArgb(16, 124, 16);
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(538, 8);
            btnGuardar.Margin = new Padding(6, 0, 0, 0);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(100, 36);
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
            btnCancelar.Location = new Point(432, 8);
            btnCancelar.Margin = new Padding(6, 0, 0, 0);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(100, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += BtnCancelar_Click;
            // 
            // frm_AtributoDef
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(760, 540);
            Controls.Add(dgvAtriDef);
            Controls.Add(tsAcciones);
            Controls.Add(panelEdicion);
            Controls.Add(panelBotones);
            Controls.Add(panelHeader);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(640, 480);
            Name = "frm_AtributoDef";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Definicion de Atributos";
            Load += Frm_AtributoDef_Load;
            panelHeader.ResumeLayout(false);
            tsAcciones.ResumeLayout(false);
            tsAcciones.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAtriDef).EndInit();
            panelEdicion.ResumeLayout(false);
            tlpCampos.ResumeLayout(false);
            tlpCampos.PerformLayout();
            panelBotones.ResumeLayout(false);
            flpBotonesDer.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}