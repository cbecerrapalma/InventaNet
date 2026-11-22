namespace Inventa.Forms.Mantenedores
{
    partial class frm_Unidad
    {
        private System.ComponentModel.IContainer components = null;

        // ── Panel header ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelTitulo;
        private System.Windows.Forms.Label lblTitulo;

        // ── ToolStrip ─────────────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip tsUnidades;
        private System.Windows.Forms.ToolStripButton btnNuevo;
        private System.Windows.Forms.ToolStripButton btnEditar;
        private System.Windows.Forms.ToolStripButton btnEliminar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton btnRefrescar;
        private System.Windows.Forms.ToolStripLabel tsLblContador;

        // ── Grilla ────────────────────────────────────────────────────────────
        private System.Windows.Forms.DataGridView dgvUnidades;

        // ── Panel edición (Dock=Bottom) ───────────────────────────────────────
        private System.Windows.Forms.Panel panelEdicion;
        private System.Windows.Forms.TableLayoutPanel tlpMain;

        // Fila 0: Codigo | ID (valor)
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.Label lblIdValor;

        // Fila 1: Nombre | (vacío)
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;

        // ── GroupBox información ──────────────────────────────────────────────
        private System.Windows.Forms.GroupBox gpbInformacion;
        private System.Windows.Forms.TableLayoutPanel tlpInfo;
        private System.Windows.Forms.Label lblIdUnidad;
        private System.Windows.Forms.Label lblIdUnidadValor;
        private System.Windows.Forms.Label lblUsos;
        private System.Windows.Forms.Label lblUsosValor;

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
            tsUnidades = new ToolStrip();
            btnNuevo = new ToolStripButton();
            btnEditar = new ToolStripButton();
            btnEliminar = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            btnRefrescar = new ToolStripButton();
            tsLblContador = new ToolStripLabel();
            dgvUnidades = new DataGridView();
            panelEdicion = new Panel();
            tlpMain = new TableLayoutPanel();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblId = new Label();
            lblIdValor = new Label();
            lblNombre = new Label();
            txtNombre = new TextBox();
            gpbInformacion = new GroupBox();
            tlpInfo = new TableLayoutPanel();
            lblIdUnidad = new Label();
            lblIdUnidadValor = new Label();
            lblUsos = new Label();
            lblUsosValor = new Label();
            panelBotones = new Panel();
            flpBotones = new FlowLayoutPanel();
            btnGuardar = new Button();
            btnCancelar = new Button();
            panelTitulo.SuspendLayout();
            tsUnidades.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUnidades).BeginInit();
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
            panelTitulo.Size = new Size(784, 44);
            panelTitulo.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(0, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(784, 44);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "MANTENEDOR DE UNIDADES DE MEDIDA";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tsUnidades
            // 
            tsUnidades.GripStyle = ToolStripGripStyle.Hidden;
            tsUnidades.Items.AddRange(new ToolStripItem[] { btnNuevo, btnEditar, btnEliminar, tsSep1, btnRefrescar, tsLblContador });
            tsUnidades.Location = new Point(0, 44);
            tsUnidades.Name = "tsUnidades";
            tsUnidades.RenderMode = ToolStripRenderMode.System;
            tsUnidades.Size = new Size(784, 25);
            tsUnidades.TabIndex = 1;
            // 
            // btnNuevo
            // 
            btnNuevo.Image = Properties.Resources.add;
            btnNuevo.ImageTransparentColor = Color.Magenta;
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(62, 22);
            btnNuevo.Text = "Nuevo";
            btnNuevo.ToolTipText = "Nueva unidad (Ins)";
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
            // dgvUnidades
            // 
            dgvUnidades.AllowUserToAddRows = false;
            dgvUnidades.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(250, 249, 248);
            dgvUnidades.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvUnidades.BackgroundColor = Color.White;
            dgvUnidades.BorderStyle = BorderStyle.None;
            dgvUnidades.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(243, 242, 241);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle2.Padding = new Padding(8, 0, 0, 0);
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvUnidades.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvUnidades.ColumnHeadersHeight = 36;
            dgvUnidades.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(96, 94, 92);
            dataGridViewCellStyle3.Padding = new Padding(8, 2, 8, 2);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(229, 243, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvUnidades.DefaultCellStyle = dataGridViewCellStyle3;
            dgvUnidades.Dock = DockStyle.Fill;
            dgvUnidades.EnableHeadersVisualStyles = false;
            dgvUnidades.GridColor = Color.FromArgb(237, 235, 233);
            dgvUnidades.Location = new Point(0, 69);
            dgvUnidades.MultiSelect = false;
            dgvUnidades.Name = "dgvUnidades";
            dgvUnidades.ReadOnly = true;
            dgvUnidades.RowHeadersVisible = false;
            dgvUnidades.RowTemplate.Height = 32;
            dgvUnidades.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUnidades.Size = new Size(784, 313);
            dgvUnidades.TabIndex = 2;
            // 
            // panelEdicion
            // 
            panelEdicion.BackColor = Color.White;
            panelEdicion.BorderStyle = BorderStyle.FixedSingle;
            panelEdicion.Controls.Add(tlpMain);
            panelEdicion.Dock = DockStyle.Bottom;
            panelEdicion.Location = new Point(0, 382);
            panelEdicion.Name = "panelEdicion";
            panelEdicion.Padding = new Padding(8, 6, 8, 6);
            panelEdicion.Size = new Size(784, 78);
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
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(8, 6);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 2;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpMain.Size = new Size(766, 64);
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
            txtCodigo.MaxLength = 10;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.Size = new Size(212, 23);
            txtCodigo.TabIndex = 1;
            // 
            // lblId
            // 
            lblId.Dock = DockStyle.Fill;
            lblId.Font = new Font("Segoe UI", 9F);
            lblId.ForeColor = Color.FromArgb(96, 94, 92);
            lblId.Location = new Point(331, 0);
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
            lblIdValor.Location = new Point(441, 0);
            lblIdValor.Name = "lblIdValor";
            lblIdValor.Size = new Size(322, 32);
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
            txtNombre.MaxLength = 50;
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(212, 23);
            txtNombre.TabIndex = 2;
            // 
            // gpbInformacion
            // 
            gpbInformacion.Controls.Add(tlpInfo);
            gpbInformacion.Dock = DockStyle.Bottom;
            gpbInformacion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gpbInformacion.ForeColor = Color.FromArgb(50, 49, 48);
            gpbInformacion.Location = new Point(0, 460);
            gpbInformacion.Name = "gpbInformacion";
            gpbInformacion.Padding = new Padding(8);
            gpbInformacion.Size = new Size(784, 46);
            gpbInformacion.TabIndex = 4;
            gpbInformacion.TabStop = false;
            gpbInformacion.Text = "Informacion de la Unidad seleccionada";
            // 
            // tlpInfo
            // 
            tlpInfo.ColumnCount = 4;
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpInfo.Controls.Add(lblIdUnidad, 0, 0);
            tlpInfo.Controls.Add(lblIdUnidadValor, 1, 0);
            tlpInfo.Controls.Add(lblUsos, 2, 0);
            tlpInfo.Controls.Add(lblUsosValor, 3, 0);
            tlpInfo.Dock = DockStyle.Fill;
            tlpInfo.Location = new Point(8, 24);
            tlpInfo.Name = "tlpInfo";
            tlpInfo.RowCount = 1;
            tlpInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpInfo.Size = new Size(768, 14);
            tlpInfo.TabIndex = 0;
            // 
            // lblIdUnidad
            // 
            lblIdUnidad.Dock = DockStyle.Fill;
            lblIdUnidad.Font = new Font("Segoe UI", 8.5F);
            lblIdUnidad.ForeColor = Color.FromArgb(96, 94, 92);
            lblIdUnidad.Location = new Point(3, 0);
            lblIdUnidad.Name = "lblIdUnidad";
            lblIdUnidad.Size = new Size(104, 14);
            lblIdUnidad.TabIndex = 0;
            lblIdUnidad.Text = "ID Unidad:";
            lblIdUnidad.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblIdUnidadValor
            // 
            lblIdUnidadValor.Dock = DockStyle.Fill;
            lblIdUnidadValor.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblIdUnidadValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblIdUnidadValor.Location = new Point(113, 0);
            lblIdUnidadValor.Name = "lblIdUnidadValor";
            lblIdUnidadValor.Size = new Size(268, 14);
            lblIdUnidadValor.TabIndex = 1;
            lblIdUnidadValor.Text = "-";
            lblIdUnidadValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUsos
            // 
            lblUsos.Dock = DockStyle.Fill;
            lblUsos.Font = new Font("Segoe UI", 8.5F);
            lblUsos.ForeColor = Color.FromArgb(96, 94, 92);
            lblUsos.Location = new Point(387, 0);
            lblUsos.Name = "lblUsos";
            lblUsos.Size = new Size(104, 14);
            lblUsos.TabIndex = 2;
            lblUsos.Text = "En uso:";
            lblUsos.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblUsosValor
            // 
            lblUsosValor.Dock = DockStyle.Fill;
            lblUsosValor.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblUsosValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblUsosValor.Location = new Point(497, 0);
            lblUsosValor.Name = "lblUsosValor";
            lblUsosValor.Size = new Size(268, 14);
            lblUsosValor.TabIndex = 3;
            lblUsosValor.Text = "-";
            lblUsosValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(243, 242, 241);
            panelBotones.Controls.Add(flpBotones);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 506);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(784, 54);
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
            flpBotones.Size = new Size(784, 54);
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
            btnGuardar.Location = new Point(658, 8);
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
            btnCancelar.Location = new Point(542, 8);
            btnCancelar.Margin = new Padding(6, 0, 0, 0);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += BtnCancelar_Click;
            // 
            // frm_Unidad
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(784, 560);
            Controls.Add(dgvUnidades);
            Controls.Add(tsUnidades);
            Controls.Add(panelEdicion);
            Controls.Add(gpbInformacion);
            Controls.Add(panelBotones);
            Controls.Add(panelTitulo);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(660, 480);
            Name = "frm_Unidad";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Mantenedor de Unidades de Medida";
            Load += Frm_Unidad_Load;
            panelTitulo.ResumeLayout(false);
            tsUnidades.ResumeLayout(false);
            tsUnidades.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUnidades).EndInit();
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