namespace Inventa.UC.Producto
{
    partial class ProdPresUC
    {
        private System.ComponentModel.IContainer components = null;

// ── Layout Principal ──────────────────────────────────────────────────
         private System.Windows.Forms.TableLayoutPanel tlpMain;

         // ── Label de Identificación del Variante (promoted) ────────────────────

// ── Panel Superior: Grilla de Presentaciones ────────────────────────────
         private System.Windows.Forms.Panel pnlVarianteHeader;
         private System.Windows.Forms.Panel pnlGrilla;
        private System.Windows.Forms.ToolStrip tsPresentaciones;
        private System.Windows.Forms.ToolStripButton tsbNuevo;
        private System.Windows.Forms.ToolStripButton tsbEditar;
        private System.Windows.Forms.ToolStripButton tsbEliminar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripLabel tslPresentacionSeleccionada;
        private System.Windows.Forms.DataGridView dgvPresentaciones;

        // ── Splitter ───────────────────────────────────────────────────────────
        private System.Windows.Forms.Splitter splitter;

        // ── Panel Inferior: Edición Inline ───────────────────────────────────
        private System.Windows.Forms.Panel pnlEdicion;
        private System.Windows.Forms.Panel pnlEdicionHeader;
        private System.Windows.Forms.Label lblModoEdicion;
        private System.Windows.Forms.Button btnCerrarEdicion;

        // ── Campos de edición ────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpCampos;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.TextBox txtDescripcion;
        private System.Windows.Forms.Label lblUnidad;
        private System.Windows.Forms.ComboBox cboUnidad;
        private System.Windows.Forms.Button btnAgregarUnidad;
        private System.Windows.Forms.CheckBox chkEsVenta;
        private System.Windows.Forms.CheckBox chkEsCompra;
        private System.Windows.Forms.CheckBox chkActivo;

        // ── Panel de Conversión (opcional, para unidades no base) ─────────────
        private System.Windows.Forms.GroupBox gpbConversion;
        private System.Windows.Forms.TableLayoutPanel tlpConversion;
        private System.Windows.Forms.Label lblUniBase;
        private System.Windows.Forms.Label lblUniBaseValor;
        private System.Windows.Forms.Label lblEquivalencia;
        private System.Windows.Forms.Label lblEquivalenciaValor;

        // ── Panel de Acciones ────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlAcciones;
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
tlpMain = new TableLayoutPanel();
              pnlGrilla = new Panel();
            dgvPresentaciones = new DataGridView();
            tsPresentaciones = new ToolStrip();
            tsbNuevo = new ToolStripButton();
            tsbEditar = new ToolStripButton();
            tsbEliminar = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            tslPresentacionSeleccionada = new ToolStripLabel();
            splitter = new Splitter();
            pnlEdicion = new Panel();
            pnlAcciones = new Panel();
            btnCancelar = new Button();
            btnGuardar = new Button();
            gpbConversion = new GroupBox();
            tlpConversion = new TableLayoutPanel();
            lblUniBase = new Label();
            lblUniBaseValor = new Label();
            lblEquivalencia = new Label();
            lblEquivalenciaValor = new Label();
            tlpCampos = new TableLayoutPanel();
            lblDescripcion = new Label();
            txtDescripcion = new TextBox();
            lblUnidad = new Label();
            cboUnidad = new ComboBox();
            btnAgregarUnidad = new Button();
            chkEsVenta = new CheckBox();
            chkEsCompra = new CheckBox();
            chkActivo = new CheckBox();
pnlEdicionHeader = new Panel();
             btnCerrarEdicion = new Button();
             lblModoEdicion = new Label();
             pnlVarianteHeader = new Panel();
             tlpMain.SuspendLayout();
             pnlGrilla.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPresentaciones).BeginInit();
            tsPresentaciones.SuspendLayout();
            pnlEdicion.SuspendLayout();
            pnlAcciones.SuspendLayout();
            gpbConversion.SuspendLayout();
            tlpConversion.SuspendLayout();
            tlpCampos.SuspendLayout();
            pnlEdicionHeader.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(pnlGrilla, 0, 1);
            tlpMain.Controls.Add(splitter, 0, 2);
            tlpMain.Controls.Add(pnlEdicion, 0, 3);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 6F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            tlpMain.Size = new Size(1024, 600);
            tlpMain.TabIndex = 0;
            // 

            // 
            // pnlGrilla
            // 
            pnlGrilla.Controls.Add(dgvPresentaciones);
            pnlGrilla.Controls.Add(tsPresentaciones);
            pnlGrilla.Dock = DockStyle.Fill;
            pnlGrilla.Location = new Point(3, 53);
            pnlGrilla.Name = "pnlGrilla";
            pnlGrilla.Padding = new Padding(8);
            pnlGrilla.Size = new Size(1018, 320);
            pnlGrilla.TabIndex = 0;
            // 
            // dgvPresentaciones
            // 
            dgvPresentaciones.AllowUserToAddRows = false;
            dgvPresentaciones.AllowUserToDeleteRows = false;
            dgvPresentaciones.BackgroundColor = Color.White;
            dgvPresentaciones.BorderStyle = BorderStyle.None;
            dgvPresentaciones.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.White;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(30, 60, 114);
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(230, 240, 250);
            dataGridViewCellStyle1.SelectionForeColor = Color.FromArgb(30, 60, 114);
            dgvPresentaciones.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvPresentaciones.ColumnHeadersHeight = 36;
            dgvPresentaciones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvPresentaciones.Dock = DockStyle.Fill;
            dgvPresentaciones.EnableHeadersVisualStyles = false;
            dgvPresentaciones.GridColor = Color.FromArgb(237, 235, 233);
            dgvPresentaciones.Location = new Point(8, 39);
            dgvPresentaciones.MultiSelect = false;
            dgvPresentaciones.Name = "dgvPresentaciones";
            dgvPresentaciones.ReadOnly = true;
            dgvPresentaciones.RowHeadersVisible = false;
            dgvPresentaciones.RowTemplate.Height = 36;
            dgvPresentaciones.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPresentaciones.Size = new Size(1002, 273);
            dgvPresentaciones.TabIndex = 1;
            // 
            // tsPresentaciones
            // 
            tsPresentaciones.GripStyle = ToolStripGripStyle.Hidden;
            tsPresentaciones.Items.AddRange(new ToolStripItem[] { tsbNuevo, tsbEditar, tsbEliminar, tsSep1, tslPresentacionSeleccionada });
            tsPresentaciones.Location = new Point(8, 8);
            tsPresentaciones.Name = "tsPresentaciones";
            tsPresentaciones.Padding = new Padding(4);
            tsPresentaciones.Size = new Size(1002, 31);
            tsPresentaciones.TabIndex = 0;
            // 
            // tsbNuevo
            // 
            tsbNuevo.Image = Properties.Resources.add;
            tsbNuevo.ImageTransparentColor = Color.Magenta;
            tsbNuevo.Name = "tsbNuevo";
            tsbNuevo.Size = new Size(62, 20);
            tsbNuevo.Text = "Nuevo";
            tsbNuevo.ToolTipText = "Crear nueva presentación (Insert)";
            // 
            // tsbEditar
            // 
            tsbEditar.Enabled = false;
            tsbEditar.Image = Properties.Resources.edit;
            tsbEditar.ImageTransparentColor = Color.Magenta;
            tsbEditar.Name = "tsbEditar";
            tsbEditar.Size = new Size(57, 20);
            tsbEditar.Text = "Editar";
            tsbEditar.ToolTipText = "Editar presentación seleccionada (F2)";
            // 
            // tsbEliminar
            // 
            tsbEliminar.Enabled = false;
            tsbEliminar.Image = Properties.Resources.delete;
            tsbEliminar.ImageTransparentColor = Color.Magenta;
            tsbEliminar.Name = "tsbEliminar";
            tsbEliminar.Size = new Size(70, 20);
            tsbEliminar.Text = "Eliminar";
            tsbEliminar.ToolTipText = "Eliminar presentación seleccionada (Supr)";
            // 
            // tsSep1
            // 
            tsSep1.Name = "tsSep1";
            tsSep1.Size = new Size(6, 23);
            // 
            // tslPresentacionSeleccionada
            // 
            tslPresentacionSeleccionada.Alignment = ToolStripItemAlignment.Right;
            tslPresentacionSeleccionada.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            tslPresentacionSeleccionada.ForeColor = Color.FromArgb(96, 94, 92);
            tslPresentacionSeleccionada.Name = "tslPresentacionSeleccionada";
            tslPresentacionSeleccionada.Size = new Size(163, 20);
            tslPresentacionSeleccionada.Text = "Sin presentación seleccionada";
            tslPresentacionSeleccionada.TextAlign = ContentAlignment.MiddleRight;
            // 
            // splitter
            // 
            splitter.BackColor = Color.FromArgb(200, 200, 200);
            splitter.Location = new Point(3, 379);
            splitter.Name = "splitter";
            splitter.Size = new Size(1018, 1);
            splitter.TabIndex = 1;
            splitter.TabStop = false;
            // 
            // pnlEdicion
            // 
            pnlEdicion.BackColor = Color.FromArgb(250, 250, 250);
            pnlEdicion.BorderStyle = BorderStyle.FixedSingle;
            pnlEdicion.Controls.Add(pnlAcciones);
            pnlEdicion.Controls.Add(gpbConversion);
            pnlEdicion.Controls.Add(tlpCampos);
            pnlEdicion.Controls.Add(pnlEdicionHeader);
            pnlEdicion.Dock = DockStyle.Fill;
            pnlEdicion.Location = new Point(3, 385);
            pnlEdicion.Name = "pnlEdicion";
            pnlEdicion.Padding = new Padding(12);
            pnlEdicion.Size = new Size(1018, 212);
            pnlEdicion.TabIndex = 2;
            pnlEdicion.Visible = false;
            // 
            // pnlAcciones
            // 
            pnlAcciones.Controls.Add(btnCancelar);
            pnlAcciones.Controls.Add(btnGuardar);
            pnlAcciones.Dock = DockStyle.Bottom;
            pnlAcciones.Location = new Point(12, 158);
            pnlAcciones.Name = "pnlAcciones";
            pnlAcciones.Padding = new Padding(0, 8, 0, 0);
            pnlAcciones.Size = new Size(992, 40);
            pnlAcciones.TabIndex = 3;
            // 
            // btnCancelar
            // 
            btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelar.BackColor = Color.White;
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(189, 195, 199);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Segoe UI", 9F);
            btnCancelar.ForeColor = Color.FromArgb(50, 49, 48);
            btnCancelar.Location = new Point(764, 8);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 32);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            // 
            // btnGuardar
            // 
            btnGuardar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGuardar.BackColor = Color.FromArgb(46, 204, 113);
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(882, 8);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 32);
            btnGuardar.TabIndex = 0;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = false;
            // 
            // gpbConversion
            // 
            gpbConversion.Controls.Add(tlpConversion);
            gpbConversion.Dock = DockStyle.Fill;
            gpbConversion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gpbConversion.ForeColor = Color.FromArgb(50, 49, 48);
            gpbConversion.Location = new Point(12, 120);
            gpbConversion.Name = "gpbConversion";
            gpbConversion.Padding = new Padding(8);
            gpbConversion.Size = new Size(992, 78);
            gpbConversion.TabIndex = 2;
            gpbConversion.TabStop = false;
            gpbConversion.Text = "📐 Información de Conversión";
            gpbConversion.Visible = false;
            // 
            // tlpConversion
            // 
            tlpConversion.ColumnCount = 4;
            tlpConversion.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tlpConversion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpConversion.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tlpConversion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpConversion.Controls.Add(lblUniBase, 0, 0);
            tlpConversion.Controls.Add(lblUniBaseValor, 1, 0);
            tlpConversion.Controls.Add(lblEquivalencia, 2, 0);
            tlpConversion.Controls.Add(lblEquivalenciaValor, 3, 0);
            tlpConversion.Dock = DockStyle.Fill;
            tlpConversion.Location = new Point(8, 24);
            tlpConversion.Name = "tlpConversion";
            tlpConversion.RowCount = 1;
            tlpConversion.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpConversion.Size = new Size(976, 46);
            tlpConversion.TabIndex = 0;
            // 
            // lblUniBase
            // 
            lblUniBase.Dock = DockStyle.Fill;
            lblUniBase.Font = new Font("Segoe UI", 9F);
            lblUniBase.ForeColor = Color.FromArgb(96, 94, 92);
            lblUniBase.Location = new Point(3, 0);
            lblUniBase.Name = "lblUniBase";
            lblUniBase.Size = new Size(114, 46);
            lblUniBase.TabIndex = 0;
            lblUniBase.Text = "Unidad Base:";
            lblUniBase.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblUniBaseValor
            // 
            lblUniBaseValor.Dock = DockStyle.Fill;
            lblUniBaseValor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUniBaseValor.ForeColor = Color.FromArgb(30, 60, 114);
            lblUniBaseValor.Location = new Point(123, 0);
            lblUniBaseValor.Name = "lblUniBaseValor";
            lblUniBaseValor.Size = new Size(362, 46);
            lblUniBaseValor.TabIndex = 1;
            lblUniBaseValor.Text = "-";
            lblUniBaseValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblEquivalencia
            // 
            lblEquivalencia.Dock = DockStyle.Fill;
            lblEquivalencia.Font = new Font("Segoe UI", 9F);
            lblEquivalencia.ForeColor = Color.FromArgb(96, 94, 92);
            lblEquivalencia.Location = new Point(491, 0);
            lblEquivalencia.Name = "lblEquivalencia";
            lblEquivalencia.Size = new Size(114, 46);
            lblEquivalencia.TabIndex = 2;
            lblEquivalencia.Text = "Equivalencia:";
            lblEquivalencia.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblEquivalenciaValor
            // 
            lblEquivalenciaValor.Dock = DockStyle.Fill;
            lblEquivalenciaValor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblEquivalenciaValor.ForeColor = Color.FromArgb(46, 204, 113);
            lblEquivalenciaValor.Location = new Point(611, 0);
            lblEquivalenciaValor.Name = "lblEquivalenciaValor";
            lblEquivalenciaValor.Size = new Size(362, 46);
            lblEquivalenciaValor.TabIndex = 3;
            lblEquivalenciaValor.Text = "1 UNI = 1 UNI";
            lblEquivalenciaValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tlpCampos
            // 
            tlpCampos.ColumnCount = 6;
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tlpCampos.Controls.Add(lblDescripcion, 0, 0);
            tlpCampos.Controls.Add(txtDescripcion, 1, 0);
            tlpCampos.Controls.Add(lblUnidad, 3, 0);
            tlpCampos.Controls.Add(cboUnidad, 4, 0);
            tlpCampos.Controls.Add(btnAgregarUnidad, 5, 0);
            tlpCampos.Controls.Add(chkEsVenta, 1, 1);
            tlpCampos.Controls.Add(chkEsCompra, 3, 1);
            tlpCampos.Controls.Add(chkActivo, 5, 1);
            tlpCampos.Dock = DockStyle.Top;
            tlpCampos.Location = new Point(12, 44);
            tlpCampos.Name = "tlpCampos";
            tlpCampos.Padding = new Padding(0, 8, 0, 8);
            tlpCampos.RowCount = 2;
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpCampos.Size = new Size(992, 76);
            tlpCampos.TabIndex = 1;
            // 
            // lblDescripcion
            // 
            lblDescripcion.Dock = DockStyle.Fill;
            lblDescripcion.Font = new Font("Segoe UI", 9F);
            lblDescripcion.ForeColor = Color.FromArgb(96, 94, 92);
            lblDescripcion.Location = new Point(3, 8);
            lblDescripcion.Name = "lblDescripcion";
            lblDescripcion.Size = new Size(84, 36);
            lblDescripcion.TabIndex = 0;
            lblDescripcion.Text = "Descripción:";
            lblDescripcion.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtDescripcion
            // 
            txtDescripcion.Dock = DockStyle.Fill;
            txtDescripcion.Font = new Font("Segoe UI", 9F);
            txtDescripcion.Location = new Point(93, 11);
            txtDescripcion.MaxLength = 100;
            txtDescripcion.Name = "txtDescripcion";
            txtDescripcion.PlaceholderText = "Ej: Caja x 12 unidades";
            txtDescripcion.Size = new Size(408, 23);
            txtDescripcion.TabIndex = 1;
            // 
            // lblUnidad
            // 
            lblUnidad.Dock = DockStyle.Fill;
            lblUnidad.Font = new Font("Segoe UI", 9F);
            lblUnidad.ForeColor = Color.FromArgb(96, 94, 92);
            lblUnidad.Location = new Point(539, 8);
            lblUnidad.Name = "lblUnidad";
            lblUnidad.Size = new Size(74, 36);
            lblUnidad.TabIndex = 2;
            lblUnidad.Text = "Unidad:";
            lblUnidad.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboUnidad
            // 
            cboUnidad.Dock = DockStyle.Fill;
            cboUnidad.DropDownStyle = ComboBoxStyle.DropDownList;
            cboUnidad.Font = new Font("Segoe UI", 9F);
            cboUnidad.FormattingEnabled = true;
            cboUnidad.Location = new Point(619, 11);
            cboUnidad.Name = "cboUnidad";
            cboUnidad.Size = new Size(289, 23);
            cboUnidad.TabIndex = 3;
            // 
            // btnAgregarUnidad
            // 
            btnAgregarUnidad.BackColor = Color.FromArgb(240, 247, 255);
            btnAgregarUnidad.Dock = DockStyle.Fill;
            btnAgregarUnidad.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
            btnAgregarUnidad.FlatStyle = FlatStyle.Flat;
            btnAgregarUnidad.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAgregarUnidad.ForeColor = Color.FromArgb(0, 120, 215);
            btnAgregarUnidad.Location = new Point(914, 11);
            btnAgregarUnidad.Margin = new Padding(3, 3, 12, 3);
            btnAgregarUnidad.Name = "btnAgregarUnidad";
            btnAgregarUnidad.Size = new Size(66, 30);
            btnAgregarUnidad.TabIndex = 4;
            btnAgregarUnidad.Text = "+";
            btnAgregarUnidad.UseVisualStyleBackColor = false;
            // 
            // chkEsVenta
            // 
            chkEsVenta.AutoSize = true;
            chkEsVenta.Checked = true;
            chkEsVenta.CheckState = CheckState.Checked;
            chkEsVenta.Dock = DockStyle.Fill;
            chkEsVenta.Font = new Font("Segoe UI", 9F);
            chkEsVenta.Location = new Point(93, 47);
            chkEsVenta.Name = "chkEsVenta";
            chkEsVenta.Size = new Size(408, 26);
            chkEsVenta.TabIndex = 7;
            chkEsVenta.Text = "✓ Usable en Ventas";
            chkEsVenta.UseVisualStyleBackColor = true;
            // 
            // chkEsCompra
            // 
            chkEsCompra.AutoSize = true;
            chkEsCompra.Checked = true;
            chkEsCompra.CheckState = CheckState.Checked;
            chkEsCompra.Dock = DockStyle.Fill;
            chkEsCompra.Font = new Font("Segoe UI", 9F);
            chkEsCompra.Location = new Point(539, 47);
            chkEsCompra.Name = "chkEsCompra";
            chkEsCompra.Size = new Size(74, 26);
            chkEsCompra.TabIndex = 8;
            chkEsCompra.Text = "✓ Usable en Compras";
            chkEsCompra.UseVisualStyleBackColor = true;
            // 
            // chkActivo
            // 
            chkActivo.AutoSize = true;
            chkActivo.Checked = true;
            chkActivo.CheckState = CheckState.Checked;
            chkActivo.Dock = DockStyle.Fill;
            chkActivo.Font = new Font("Segoe UI", 9F);
            chkActivo.Location = new Point(914, 47);
            chkActivo.Name = "chkActivo";
            chkActivo.Size = new Size(75, 26);
            chkActivo.TabIndex = 9;
            chkActivo.Text = "Activo";
            chkActivo.UseVisualStyleBackColor = true;
            // 
            // pnlEdicionHeader
            // 
            pnlEdicionHeader.Controls.Add(btnCerrarEdicion);
            pnlEdicionHeader.Controls.Add(lblModoEdicion);
            pnlEdicionHeader.Dock = DockStyle.Top;
            pnlEdicionHeader.Location = new Point(12, 12);
            pnlEdicionHeader.Name = "pnlEdicionHeader";
            pnlEdicionHeader.Size = new Size(992, 32);
            pnlEdicionHeader.TabIndex = 0;
            // 
            // btnCerrarEdicion
            // 
            btnCerrarEdicion.BackColor = Color.FromArgb(189, 195, 199);
            btnCerrarEdicion.Dock = DockStyle.Right;
            btnCerrarEdicion.FlatAppearance.BorderSize = 0;
            btnCerrarEdicion.FlatStyle = FlatStyle.Flat;
            btnCerrarEdicion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCerrarEdicion.ForeColor = Color.White;
            btnCerrarEdicion.Location = new Point(960, 0);
            btnCerrarEdicion.Name = "btnCerrarEdicion";
            btnCerrarEdicion.Size = new Size(32, 32);
            btnCerrarEdicion.TabIndex = 1;
            btnCerrarEdicion.Text = "✕";
            btnCerrarEdicion.UseVisualStyleBackColor = false;
            // 
            // lblModoEdicion
            // 
            lblModoEdicion.Dock = DockStyle.Fill;
            lblModoEdicion.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblModoEdicion.ForeColor = Color.FromArgb(30, 60, 114);
            lblModoEdicion.Location = new Point(0, 0);
            lblModoEdicion.Name = "lblModoEdicion";
            lblModoEdicion.Size = new Size(992, 32);
            lblModoEdicion.TabIndex = 0;
            lblModoEdicion.Text = "🆕 Nueva Presentación";
            lblModoEdicion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ProdPresUC
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(tlpMain);
            Font = new Font("Segoe UI", 9F);
            Name = "ProdPresUC";
            Size = new Size(1024, 600);
tlpMain.ResumeLayout(false);
             pnlGrilla.ResumeLayout(false);
            pnlGrilla.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPresentaciones).EndInit();
            tsPresentaciones.ResumeLayout(false);
            tsPresentaciones.PerformLayout();
            pnlEdicion.ResumeLayout(false);
            pnlAcciones.ResumeLayout(false);
            gpbConversion.ResumeLayout(false);
            tlpConversion.ResumeLayout(false);
            tlpCampos.ResumeLayout(false);
            tlpCampos.PerformLayout();
            pnlEdicionHeader.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}