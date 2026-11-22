namespace Inventa.UC.Producto
{
    partial class ProdPreciosVentaUC
    {
        private System.ComponentModel.IContainer components = null;

        // ── Layout Principal ──────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpMain;

        // ── Panel Superior: Selector + Grilla ──────────────────────────────────
        private System.Windows.Forms.Panel pnlSuperior;
        private System.Windows.Forms.Panel pnlSelector;
        private System.Windows.Forms.Label lblPresentacion;
        private System.Windows.Forms.ComboBox cboPresentacion;
        private System.Windows.Forms.Label lblTipoPrecio;
        private System.Windows.Forms.ComboBox cboTipoPrecio;
        private System.Windows.Forms.CheckBox chkSoloActivos;
        private System.Windows.Forms.ToolStrip tsPrecios;
        private System.Windows.Forms.ToolStripButton tsbNuevo;
        private System.Windows.Forms.ToolStripButton tsbEditar;
        private System.Windows.Forms.ToolStripButton tsbEliminar;
        private System.Windows.Forms.ToolStripButton tsbHistorial;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripLabel tslInfo;
        private System.Windows.Forms.DataGridView dgvPrecios;

        // ── Panel de Resumen (Footer de grilla) ────────────────────────────────
        private System.Windows.Forms.Panel pnlResumen;
        private System.Windows.Forms.Label lblPrecioActual;
        private System.Windows.Forms.Label lblVigencia;
        private System.Windows.Forms.Label lblProximoCambio;

        // ── Splitter ───────────────────────────────────────────────────────────
        private System.Windows.Forms.Splitter splitter;

        // ── Panel Inferior: Edición Inline ───────────────────────────────────
        private System.Windows.Forms.Panel pnlEdicion;
        private System.Windows.Forms.Panel pnlEdicionHeader;
        private System.Windows.Forms.Label lblModoEdicion;
        private System.Windows.Forms.Button btnCerrarEdicion;

        // ── Campos de edición ────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpCampos;
        private System.Windows.Forms.Label lblTipoPrecioEdicion;
        private System.Windows.Forms.ComboBox cboTipoPrecioEdicion;
        private System.Windows.Forms.Label lblValor;
        private System.Windows.Forms.NumericUpDown numValor;
        private System.Windows.Forms.Label lblFecVig;
        private System.Windows.Forms.DateTimePicker dtpFecVig;
        private System.Windows.Forms.CheckBox chkActivo;

        // ── Panel de Validación ────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlValidacion;
        private System.Windows.Forms.PictureBox picAlerta;
        private System.Windows.Forms.Label lblMensajeValidacion;

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
            pnlSuperior = new Panel();
            pnlResumen = new Panel();
            lblProximoCambio = new Label();
            lblVigencia = new Label();
            lblPrecioActual = new Label();
            dgvPrecios = new DataGridView();
            tsPrecios = new ToolStrip();
            tsbNuevo = new ToolStripButton();
            tsbEditar = new ToolStripButton();
            tsbEliminar = new ToolStripButton();
            tsbHistorial = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            tslInfo = new ToolStripLabel();
            pnlSelector = new Panel();
            chkSoloActivos = new CheckBox();
            cboTipoPrecio = new ComboBox();
            lblTipoPrecio = new Label();
            cboPresentacion = new ComboBox();
            lblPresentacion = new Label();
            splitter = new Splitter();
            pnlEdicion = new Panel();
            pnlAcciones = new Panel();
            btnCancelar = new Button();
            btnGuardar = new Button();
            pnlValidacion = new Panel();
            lblMensajeValidacion = new Label();
            picAlerta = new PictureBox();
            tlpCampos = new TableLayoutPanel();
            lblTipoPrecioEdicion = new Label();
            cboTipoPrecioEdicion = new ComboBox();
            lblValor = new Label();
            numValor = new NumericUpDown();
            lblFecVig = new Label();
            dtpFecVig = new DateTimePicker();
            chkActivo = new CheckBox();
            pnlEdicionHeader = new Panel();
            btnCerrarEdicion = new Button();
            lblModoEdicion = new Label();
            tlpMain.SuspendLayout();
            pnlSuperior.SuspendLayout();
            pnlResumen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPrecios).BeginInit();
            tsPrecios.SuspendLayout();
            pnlSelector.SuspendLayout();
            pnlEdicion.SuspendLayout();
            pnlAcciones.SuspendLayout();
            pnlValidacion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picAlerta).BeginInit();
            tlpCampos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numValor).BeginInit();
            pnlEdicionHeader.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(pnlSuperior, 0, 0);
            tlpMain.Controls.Add(splitter, 0, 1);
            tlpMain.Controls.Add(pnlEdicion, 0, 2);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 6F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            tlpMain.Size = new Size(1024, 600);
            tlpMain.TabIndex = 0;
            // 
            // pnlSuperior
            // 
            pnlSuperior.Controls.Add(pnlResumen);
            pnlSuperior.Controls.Add(dgvPrecios);
            pnlSuperior.Controls.Add(tsPrecios);
            pnlSuperior.Controls.Add(pnlSelector);
            pnlSuperior.Dock = DockStyle.Fill;
            pnlSuperior.Location = new Point(3, 3);
            pnlSuperior.Name = "pnlSuperior";
            pnlSuperior.Padding = new Padding(8);
            pnlSuperior.Size = new Size(1018, 380);
            pnlSuperior.TabIndex = 0;
            // 
            // pnlResumen
            // 
            pnlResumen.BackColor = Color.FromArgb(243, 242, 241);
            pnlResumen.BorderStyle = BorderStyle.FixedSingle;
            pnlResumen.Controls.Add(lblProximoCambio);
            pnlResumen.Controls.Add(lblVigencia);
            pnlResumen.Controls.Add(lblPrecioActual);
            pnlResumen.Dock = DockStyle.Bottom;
            pnlResumen.Location = new Point(8, 328);
            pnlResumen.Name = "pnlResumen";
            pnlResumen.Padding = new Padding(12, 4, 12, 4);
            pnlResumen.Size = new Size(1002, 44);
            pnlResumen.TabIndex = 3;
            // 
            // lblProximoCambio
            // 
            lblProximoCambio.Dock = DockStyle.Right;
            lblProximoCambio.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblProximoCambio.ForeColor = Color.FromArgb(150, 150, 150);
            lblProximoCambio.Location = new Point(700, 4);
            lblProximoCambio.Name = "lblProximoCambio";
            lblProximoCambio.Size = new Size(288, 34);
            lblProximoCambio.TabIndex = 2;
            lblProximoCambio.Text = "⏭️ Próximo cambio: --/--/----";
            lblProximoCambio.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblVigencia
            // 
            lblVigencia.Dock = DockStyle.Left;
            lblVigencia.Font = new Font("Segoe UI", 9F);
            lblVigencia.ForeColor = Color.FromArgb(96, 94, 92);
            lblVigencia.Location = new Point(262, 4);
            lblVigencia.Name = "lblVigencia";
            lblVigencia.Size = new Size(300, 34);
            lblVigencia.TabIndex = 1;
            lblVigencia.Text = "📅 Vigente desde: --/--/----";
            lblVigencia.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPrecioActual
            // 
            lblPrecioActual.Dock = DockStyle.Left;
            lblPrecioActual.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPrecioActual.ForeColor = Color.FromArgb(46, 204, 113);
            lblPrecioActual.Location = new Point(12, 4);
            lblPrecioActual.Name = "lblPrecioActual";
            lblPrecioActual.Size = new Size(250, 34);
            lblPrecioActual.TabIndex = 0;
            lblPrecioActual.Text = "💰 Precio actual: $0.00";
            lblPrecioActual.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dgvPrecios
            // 
            dgvPrecios.AllowUserToAddRows = false;
            dgvPrecios.AllowUserToDeleteRows = false;
            dgvPrecios.BackgroundColor = Color.White;
            dgvPrecios.BorderStyle = BorderStyle.None;
            dgvPrecios.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvPrecios.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvPrecios.ColumnHeadersHeight = 36;
            dgvPrecios.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvPrecios.Dock = DockStyle.Fill;
            dgvPrecios.EnableHeadersVisualStyles = false;
            dgvPrecios.GridColor = Color.FromArgb(237, 235, 233);
            dgvPrecios.Location = new Point(8, 79);
            dgvPrecios.MultiSelect = false;
            dgvPrecios.Name = "dgvPrecios";
            dgvPrecios.ReadOnly = true;
            dgvPrecios.RowHeadersVisible = false;
            dgvPrecios.RowTemplate.Height = 36;
            dgvPrecios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPrecios.Size = new Size(1002, 293);
            dgvPrecios.TabIndex = 2;
            // 
            // tsPrecios
            // 
            tsPrecios.GripStyle = ToolStripGripStyle.Hidden;
            tsPrecios.Items.AddRange(new ToolStripItem[] { tsbNuevo, tsbEditar, tsbEliminar, tsbHistorial, tsSep1, tslInfo });
            tsPrecios.Location = new Point(8, 48);
            tsPrecios.Name = "tsPrecios";
            tsPrecios.Padding = new Padding(4);
            tsPrecios.Size = new Size(1002, 31);
            tsPrecios.TabIndex = 1;
            // 
            // tsbNuevo
            // 
            tsbNuevo.Image = Properties.Resources.add;
            tsbNuevo.ImageTransparentColor = Color.Magenta;
            tsbNuevo.Name = "tsbNuevo";
            tsbNuevo.Size = new Size(62, 20);
            tsbNuevo.Text = "Nuevo";
            tsbNuevo.ToolTipText = "Agregar nuevo precio";
            tsbNuevo.Click += TsbNuevo_Click;
            // 
            // tsbEditar
            // 
            tsbEditar.Enabled = false;
            tsbEditar.Image = Properties.Resources.edit;
            tsbEditar.ImageTransparentColor = Color.Magenta;
            tsbEditar.Name = "tsbEditar";
            tsbEditar.Size = new Size(57, 20);
            tsbEditar.Text = "Editar";
            tsbEditar.ToolTipText = "Editar precio seleccionado";
            tsbEditar.Click += TsbEditar_Click;
            // 
            // tsbEliminar
            // 
            tsbEliminar.Enabled = false;
            tsbEliminar.Image = Properties.Resources.delete;
            tsbEliminar.ImageTransparentColor = Color.Magenta;
            tsbEliminar.Name = "tsbEliminar";
            tsbEliminar.Size = new Size(70, 20);
            tsbEliminar.Text = "Eliminar";
            tsbEliminar.ToolTipText = "Eliminar precio seleccionado";
            tsbEliminar.Click += TsbEliminar_Click;
            // 
            // tsbHistorial
            // 
            tsbHistorial.Image = Properties.Resources.add;
            tsbHistorial.ImageTransparentColor = Color.Magenta;
            tsbHistorial.Name = "tsbHistorial";
            tsbHistorial.Size = new Size(86, 20);
            tsbHistorial.Text = "📊 Historial";
            tsbHistorial.ToolTipText = "Ver historial completo de precios";
            // 
            // tsSep1
            // 
            tsSep1.Name = "tsSep1";
            tsSep1.Size = new Size(6, 23);
            // 
            // tslInfo
            // 
            tslInfo.Alignment = ToolStripItemAlignment.Right;
            tslInfo.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            tslInfo.ForeColor = Color.FromArgb(96, 94, 92);
            tslInfo.Name = "tslInfo";
            tslInfo.Size = new Size(113, 20);
            tslInfo.Text = "0 precios registrados";
            tslInfo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // pnlSelector
            // 
            pnlSelector.Controls.Add(chkSoloActivos);
            pnlSelector.Controls.Add(cboTipoPrecio);
            pnlSelector.Controls.Add(lblTipoPrecio);
            pnlSelector.Controls.Add(cboPresentacion);
            pnlSelector.Controls.Add(lblPresentacion);
            pnlSelector.Dock = DockStyle.Top;
            pnlSelector.Location = new Point(8, 8);
            pnlSelector.Name = "pnlSelector";
            pnlSelector.Padding = new Padding(0, 0, 0, 8);
            pnlSelector.Size = new Size(1002, 40);
            pnlSelector.TabIndex = 0;
            pnlSelector.Paint += pnlSelector_Paint;
            // 
            // chkSoloActivos
            // 
            chkSoloActivos.AutoSize = true;
            chkSoloActivos.Checked = true;
            chkSoloActivos.CheckState = CheckState.Checked;
            chkSoloActivos.Dock = DockStyle.Left;
            chkSoloActivos.Font = new Font("Segoe UI", 9F);
            chkSoloActivos.Location = new Point(570, 0);
            chkSoloActivos.Margin = new Padding(12, 3, 3, 3);
            chkSoloActivos.Name = "chkSoloActivos";
            chkSoloActivos.Size = new Size(130, 32);
            chkSoloActivos.TabIndex = 4;
            chkSoloActivos.Text = "Solo precios activos";
            chkSoloActivos.UseVisualStyleBackColor = true;
            // 
            // cboTipoPrecio
            // 
            cboTipoPrecio.Dock = DockStyle.Left;
            cboTipoPrecio.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoPrecio.Font = new Font("Segoe UI", 9F);
            cboTipoPrecio.FormattingEnabled = true;
            cboTipoPrecio.Items.AddRange(new object[] { "Todos", "Público", "Mayorista", "Oferta" });
            cboTipoPrecio.Location = new Point(450, 0);
            cboTipoPrecio.Name = "cboTipoPrecio";
            cboTipoPrecio.Size = new Size(120, 23);
            cboTipoPrecio.TabIndex = 3;
            // 
            // lblTipoPrecio
            // 
            lblTipoPrecio.Dock = DockStyle.Left;
            lblTipoPrecio.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTipoPrecio.ForeColor = Color.FromArgb(50, 49, 48);
            lblTipoPrecio.Location = new Point(370, 0);
            lblTipoPrecio.Margin = new Padding(12, 0, 0, 0);
            lblTipoPrecio.Name = "lblTipoPrecio";
            lblTipoPrecio.Size = new Size(80, 32);
            lblTipoPrecio.TabIndex = 2;
            lblTipoPrecio.Text = "Tipo:";
            lblTipoPrecio.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboPresentacion
            // 
            cboPresentacion.Dock = DockStyle.Left;
            cboPresentacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPresentacion.Font = new Font("Segoe UI", 9F);
            cboPresentacion.FormattingEnabled = true;
            cboPresentacion.Location = new Point(90, 0);
            cboPresentacion.Name = "cboPresentacion";
            cboPresentacion.Size = new Size(280, 23);
            cboPresentacion.TabIndex = 1;
            // 
            // lblPresentacion
            // 
            lblPresentacion.Dock = DockStyle.Left;
            lblPresentacion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPresentacion.ForeColor = Color.FromArgb(50, 49, 48);
            lblPresentacion.Location = new Point(0, 0);
            lblPresentacion.Name = "lblPresentacion";
            lblPresentacion.Size = new Size(90, 32);
            lblPresentacion.TabIndex = 0;
            lblPresentacion.Text = "📦 Presentación:";
            lblPresentacion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // splitter
            // 
            splitter.BackColor = Color.FromArgb(200, 200, 200);
            splitter.Location = new Point(3, 389);
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
            pnlEdicion.Controls.Add(pnlValidacion);
            pnlEdicion.Controls.Add(tlpCampos);
            pnlEdicion.Controls.Add(pnlEdicionHeader);
            pnlEdicion.Dock = DockStyle.Fill;
            pnlEdicion.Location = new Point(3, 395);
            pnlEdicion.Name = "pnlEdicion";
            pnlEdicion.Padding = new Padding(12);
            pnlEdicion.Size = new Size(1018, 202);
            pnlEdicion.TabIndex = 2;
            pnlEdicion.Visible = false;
            // 
            // pnlAcciones
            // 
            pnlAcciones.Controls.Add(btnCancelar);
            pnlAcciones.Controls.Add(btnGuardar);
            pnlAcciones.Dock = DockStyle.Bottom;
            pnlAcciones.Location = new Point(12, 148);
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
            btnCancelar.Click += BtnCancelar_Click;
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
            btnGuardar.Click += BtnGuardar_Click;
            // 
            // pnlValidacion
            // 
            pnlValidacion.BackColor = Color.FromArgb(255, 243, 205);
            pnlValidacion.BorderStyle = BorderStyle.FixedSingle;
            pnlValidacion.Controls.Add(lblMensajeValidacion);
            pnlValidacion.Controls.Add(picAlerta);
            pnlValidacion.Dock = DockStyle.Fill;
            pnlValidacion.Location = new Point(12, 120);
            pnlValidacion.Name = "pnlValidacion";
            pnlValidacion.Padding = new Padding(8);
            pnlValidacion.Size = new Size(992, 68);
            pnlValidacion.TabIndex = 2;
            pnlValidacion.Visible = false;
            // 
            // lblMensajeValidacion
            // 
            lblMensajeValidacion.Dock = DockStyle.Fill;
            lblMensajeValidacion.Font = new Font("Segoe UI", 9F);
            lblMensajeValidacion.ForeColor = Color.FromArgb(133, 100, 4);
            lblMensajeValidacion.Location = new Point(32, 8);
            lblMensajeValidacion.Name = "lblMensajeValidacion";
            lblMensajeValidacion.Size = new Size(950, 50);
            lblMensajeValidacion.TabIndex = 1;
            lblMensajeValidacion.Text = "⚠️ Este precio se solapa con otro precio del mismo tipo. Se desactivará el precio anterior.";
            lblMensajeValidacion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // picAlerta
            // 
            picAlerta.Dock = DockStyle.Left;
            picAlerta.Location = new Point(8, 8);
            picAlerta.Name = "picAlerta";
            picAlerta.Size = new Size(24, 50);
            picAlerta.SizeMode = PictureBoxSizeMode.CenterImage;
            picAlerta.TabIndex = 0;
            picAlerta.TabStop = false;
            // 
            // tlpCampos
            // 
            tlpCampos.ColumnCount = 6;
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpCampos.Controls.Add(lblTipoPrecioEdicion, 0, 0);
            tlpCampos.Controls.Add(cboTipoPrecioEdicion, 1, 0);
            tlpCampos.Controls.Add(lblValor, 2, 0);
            tlpCampos.Controls.Add(numValor, 3, 0);
            tlpCampos.Controls.Add(lblFecVig, 4, 0);
            tlpCampos.Controls.Add(dtpFecVig, 5, 0);
            tlpCampos.Controls.Add(chkActivo, 1, 1);
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
            // lblTipoPrecioEdicion
            // 
            lblTipoPrecioEdicion.Dock = DockStyle.Fill;
            lblTipoPrecioEdicion.Font = new Font("Segoe UI", 9F);
            lblTipoPrecioEdicion.ForeColor = Color.FromArgb(96, 94, 92);
            lblTipoPrecioEdicion.Location = new Point(3, 8);
            lblTipoPrecioEdicion.Name = "lblTipoPrecioEdicion";
            lblTipoPrecioEdicion.Size = new Size(94, 36);
            lblTipoPrecioEdicion.TabIndex = 0;
            lblTipoPrecioEdicion.Text = "Tipo Precio:";
            lblTipoPrecioEdicion.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboTipoPrecioEdicion
            // 
            cboTipoPrecioEdicion.Dock = DockStyle.Fill;
            cboTipoPrecioEdicion.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoPrecioEdicion.Font = new Font("Segoe UI", 9F);
            cboTipoPrecioEdicion.FormattingEnabled = true;
            cboTipoPrecioEdicion.Location = new Point(103, 11);
            cboTipoPrecioEdicion.Name = "cboTipoPrecioEdicion";
            cboTipoPrecioEdicion.Size = new Size(231, 23);
            cboTipoPrecioEdicion.TabIndex = 1;
            // 
            // lblValor
            // 
            lblValor.Dock = DockStyle.Fill;
            lblValor.Font = new Font("Segoe UI", 9F);
            lblValor.ForeColor = Color.FromArgb(96, 94, 92);
            lblValor.Location = new Point(340, 8);
            lblValor.Name = "lblValor";
            lblValor.Size = new Size(74, 36);
            lblValor.TabIndex = 2;
            lblValor.Text = "Valor ($):";
            lblValor.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numValor
            // 
            numValor.DecimalPlaces = 4;
            numValor.Dock = DockStyle.Fill;
            numValor.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            numValor.Location = new Point(420, 11);
            numValor.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numValor.Name = "numValor";
            numValor.Size = new Size(231, 25);
            numValor.TabIndex = 3;
            numValor.TextAlign = HorizontalAlignment.Right;
            numValor.ThousandsSeparator = true;
            // 
            // lblFecVig
            // 
            lblFecVig.Dock = DockStyle.Fill;
            lblFecVig.Font = new Font("Segoe UI", 9F);
            lblFecVig.ForeColor = Color.FromArgb(96, 94, 92);
            lblFecVig.Location = new Point(657, 8);
            lblFecVig.Name = "lblFecVig";
            lblFecVig.Size = new Size(94, 36);
            lblFecVig.TabIndex = 4;
            lblFecVig.Text = "Vigente desde:";
            lblFecVig.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dtpFecVig
            // 
            dtpFecVig.Dock = DockStyle.Fill;
            dtpFecVig.Font = new Font("Segoe UI", 9F);
            dtpFecVig.Format = DateTimePickerFormat.Short;
            dtpFecVig.Location = new Point(757, 11);
            dtpFecVig.Name = "dtpFecVig";
            dtpFecVig.Size = new Size(232, 23);
            dtpFecVig.TabIndex = 5;
            dtpFecVig.Value = new DateTime(2026, 3, 25, 0, 0, 0, 0);
            // 
            // chkActivo
            // 
            chkActivo.AutoSize = true;
            chkActivo.Checked = true;
            chkActivo.CheckState = CheckState.Checked;
            chkActivo.Dock = DockStyle.Fill;
            chkActivo.Font = new Font("Segoe UI", 9F);
            chkActivo.Location = new Point(103, 47);
            chkActivo.Name = "chkActivo";
            chkActivo.Size = new Size(231, 26);
            chkActivo.TabIndex = 6;
            chkActivo.Text = "Precio activo";
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
            btnCerrarEdicion.Click += BtnCerrarEdicion_Click;
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
            lblModoEdicion.Text = "💰 Nuevo Precio de Venta";
            lblModoEdicion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ProdPreciosVentaUC
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(tlpMain);
            Font = new Font("Segoe UI", 9F);
            Name = "ProdPreciosVentaUC";
            Size = new Size(1024, 600);
            tlpMain.ResumeLayout(false);
            pnlSuperior.ResumeLayout(false);
            pnlSuperior.PerformLayout();
            pnlResumen.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPrecios).EndInit();
            tsPrecios.ResumeLayout(false);
            tsPrecios.PerformLayout();
            pnlSelector.ResumeLayout(false);
            pnlSelector.PerformLayout();
            pnlEdicion.ResumeLayout(false);
            pnlAcciones.ResumeLayout(false);
            pnlValidacion.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picAlerta).EndInit();
            tlpCampos.ResumeLayout(false);
            tlpCampos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numValor).EndInit();
            pnlEdicionHeader.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}