namespace Inventa.UC.Producto
{
    partial class ProdCodBarUC
    {
        private System.ComponentModel.IContainer components = null;

        // ── Layout Principal ──────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpMain;

        // ── Panel de Identificación de Presentación ─────────────────────────

        // ── Panel Superior: Grilla de Códigos ──────────────────────────────────
        private System.Windows.Forms.Panel pnlGrilla;
        private System.Windows.Forms.Panel pnlFiltros;
        private System.Windows.Forms.Label lblTipoCod;
        private System.Windows.Forms.ComboBox cboTipoCod;
        private System.Windows.Forms.CheckBox chkSoloPrincipales;
        private System.Windows.Forms.ToolStrip tsCodigos;
        private System.Windows.Forms.ToolStripButton tsbNuevo;
        private System.Windows.Forms.ToolStripButton tsbEditar;
        private System.Windows.Forms.ToolStripButton tsbEliminar;
        private System.Windows.Forms.ToolStripButton tsbImprimir;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripLabel tslInfo;
        private System.Windows.Forms.DataGridView dgvCodigos;

        // ── Splitter ───────────────────────────────────────────────────────────
        private System.Windows.Forms.Splitter splitter;

        // ── Panel Inferior: Edición Rápida/Escáner ─────────────────────────────
        private System.Windows.Forms.Panel pnlEdicion;
        private System.Windows.Forms.Panel pnlEdicionHeader;
        private System.Windows.Forms.Label lblModoEdicion;
        private System.Windows.Forms.Button btnCerrarEdicion;

        // ── Campos de edición ────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpCampos;
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblTipoCodEdicion;
        private System.Windows.Forms.ComboBox cboTipoCodEdicion;
        private System.Windows.Forms.CheckBox chkPrincipal;
        private System.Windows.Forms.Panel pnlPreview;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.PictureBox picBarcode;

        // ── Panel de Acciones ────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlAcciones;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnNuevoContinuar;

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
            dgvCodigos = new DataGridView();
            tsCodigos = new ToolStrip();
            tsbNuevo = new ToolStripButton();
            tsbEditar = new ToolStripButton();
            tsbEliminar = new ToolStripButton();
            tsbImprimir = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            tslInfo = new ToolStripLabel();
            pnlFiltros = new Panel();
            chkSoloPrincipales = new CheckBox();
            cboTipoCod = new ComboBox();
            lblTipoCod = new Label();
            splitter = new Splitter();
            pnlEdicion = new Panel();
            pnlAcciones = new Panel();
            btnNuevoContinuar = new Button();
            btnCancelar = new Button();
            btnGuardar = new Button();
            pnlPreview = new Panel();
            picBarcode = new PictureBox();
            lblPreview = new Label();
            tlpCampos = new TableLayoutPanel();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblTipoCodEdicion = new Label();
            cboTipoCodEdicion = new ComboBox();
            chkPrincipal = new CheckBox();
            pnlEdicionHeader = new Panel();
            btnCerrarEdicion = new Button();
            lblModoEdicion = new Label();
            tlpMain.SuspendLayout();
            pnlGrilla.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCodigos).BeginInit();
            tsCodigos.SuspendLayout();
            pnlFiltros.SuspendLayout();
            pnlEdicion.SuspendLayout();
            pnlAcciones.SuspendLayout();
            pnlPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picBarcode).BeginInit();
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
            // 
            // 
            // pnlGrilla
            // 
            pnlGrilla.Controls.Add(dgvCodigos);
            pnlGrilla.Controls.Add(tsCodigos);
            pnlGrilla.Controls.Add(pnlFiltros);
            pnlGrilla.Dock = DockStyle.Fill;
            pnlGrilla.Location = new Point(3, 53);
            pnlGrilla.Name = "pnlGrilla";
            pnlGrilla.Padding = new Padding(8);
            pnlGrilla.Size = new Size(1018, 320);
            pnlGrilla.TabIndex = 0;
            // 
            // dgvCodigos
            // 
            dgvCodigos.AllowUserToAddRows = false;
            dgvCodigos.AllowUserToDeleteRows = false;
            dgvCodigos.BackgroundColor = Color.White;
            dgvCodigos.BorderStyle = BorderStyle.None;
            dgvCodigos.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.White;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(30, 60, 114);
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            dgvCodigos.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvCodigos.ColumnHeadersHeight = 36;
            dgvCodigos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvCodigos.Dock = DockStyle.Fill;
            dgvCodigos.EnableHeadersVisualStyles = false;
            dgvCodigos.GridColor = Color.FromArgb(237, 235, 233);
            dgvCodigos.Location = new Point(8, 79);
            dgvCodigos.MultiSelect = false;
            dgvCodigos.Name = "dgvCodigos";
            dgvCodigos.ReadOnly = true;
            dgvCodigos.RowHeadersVisible = false;
            dgvCodigos.RowTemplate.Height = 36;
            dgvCodigos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCodigos.Size = new Size(1002, 233);
            dgvCodigos.TabIndex = 2;
            // 
            // tsCodigos
            // 
            tsCodigos.GripStyle = ToolStripGripStyle.Hidden;
            tsCodigos.Items.AddRange(new ToolStripItem[] { tsbNuevo, tsbEditar, tsbEliminar, tsbImprimir, tsSep1, tslInfo });
            tsCodigos.Location = new Point(8, 48);
            tsCodigos.Name = "tsCodigos";
            tsCodigos.Padding = new Padding(4);
            tsCodigos.Size = new Size(1002, 31);
            tsCodigos.TabIndex = 1;
            // 
            // tsbNuevo
            // 
            tsbNuevo.Image = Properties.Resources.add;
            tsbNuevo.ImageTransparentColor = Color.Magenta;
            tsbNuevo.Name = "tsbNuevo";
            tsbNuevo.Size = new Size(62, 20);
            tsbNuevo.Text = "Nuevo";
            tsbNuevo.ToolTipText = "Agregar código de barra";
            // 
            // tsbEditar
            // 
            tsbEditar.Enabled = false;
            tsbEditar.Image = Properties.Resources.edit;
            tsbEditar.ImageTransparentColor = Color.Magenta;
            tsbEditar.Name = "tsbEditar";
            tsbEditar.Size = new Size(57, 20);
            tsbEditar.Text = "Editar";
            tsbEditar.ToolTipText = "Editar código seleccionado";
            // 
            // tsbEliminar
            // 
            tsbEliminar.Enabled = false;
            tsbEliminar.Image = Properties.Resources.delete;
            tsbEliminar.ImageTransparentColor = Color.Magenta;
            tsbEliminar.Name = "tsbEliminar";
            tsbEliminar.Size = new Size(70, 20);
            tsbEliminar.Text = "Eliminar";
            tsbEliminar.ToolTipText = "Eliminar código seleccionado";
            // 
            // tsbImprimir
            // 
            tsbImprimir.Enabled = false;
            tsbImprimir.Image = Properties.Resources.add;
            tsbImprimir.ImageTransparentColor = Color.Magenta;
            tsbImprimir.Name = "tsbImprimir";
            tsbImprimir.Size = new Size(88, 20);
            tsbImprimir.Text = "🖨️ Imprimir";
            tsbImprimir.ToolTipText = "Imprimir etiqueta de código de barra";
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
            tslInfo.Size = new Size(103, 20);
            tslInfo.Text = "0 códigos de barra";
            tslInfo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // pnlFiltros
            // 
            pnlFiltros.BackColor = Color.FromArgb(245, 248, 250);
            pnlFiltros.BorderStyle = BorderStyle.FixedSingle;
            pnlFiltros.Controls.Add(chkSoloPrincipales);
            pnlFiltros.Controls.Add(cboTipoCod);
            pnlFiltros.Controls.Add(lblTipoCod);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(8, 8);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Padding = new Padding(0, 0, 0, 8);
            pnlFiltros.Size = new Size(1002, 40);
            pnlFiltros.TabIndex = 0;
            // 
            // chkSoloPrincipales
            // 
            chkSoloPrincipales.AutoSize = true;
            chkSoloPrincipales.Dock = DockStyle.Left;
            chkSoloPrincipales.Font = new Font("Segoe UI", 9F);
            chkSoloPrincipales.Location = new Point(200, 0);
            chkSoloPrincipales.Margin = new Padding(12, 3, 3, 3);
            chkSoloPrincipales.Name = "chkSoloPrincipales";
            chkSoloPrincipales.Size = new Size(122, 30);
            chkSoloPrincipales.TabIndex = 4;
            chkSoloPrincipales.Text = "⭐ Solo principales";
            chkSoloPrincipales.UseVisualStyleBackColor = true;
            // 
            // cboTipoCod
            // 
            cboTipoCod.Dock = DockStyle.Left;
            cboTipoCod.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoCod.Font = new Font("Segoe UI", 9F);
            cboTipoCod.FormattingEnabled = true;
            cboTipoCod.Items.AddRange(new object[] { "Todos", "Normal", "Pesable", "Interno", "Proveedor" });
            cboTipoCod.Location = new Point(80, 0);
            cboTipoCod.Name = "cboTipoCod";
            cboTipoCod.Size = new Size(120, 23);
            cboTipoCod.TabIndex = 3;
            // 
            // lblTipoCod
            // 
            lblTipoCod.Dock = DockStyle.Left;
            lblTipoCod.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTipoCod.ForeColor = Color.FromArgb(50, 49, 48);
            lblTipoCod.Location = new Point(0, 0);
            lblTipoCod.Margin = new Padding(12, 0, 0, 0);
            lblTipoCod.Name = "lblTipoCod";
            lblTipoCod.Size = new Size(80, 30);
            lblTipoCod.TabIndex = 2;
            lblTipoCod.Text = "🔍 Filtro:";
            lblTipoCod.TextAlign = ContentAlignment.MiddleRight;
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
            pnlEdicion.Controls.Add(pnlPreview);
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
            pnlAcciones.Controls.Add(btnNuevoContinuar);
            pnlAcciones.Controls.Add(btnCancelar);
            pnlAcciones.Controls.Add(btnGuardar);
            pnlAcciones.Dock = DockStyle.Bottom;
            pnlAcciones.Location = new Point(12, 158);
            pnlAcciones.Name = "pnlAcciones";
            pnlAcciones.Padding = new Padding(0, 8, 0, 0);
            pnlAcciones.Size = new Size(992, 40);
            pnlAcciones.TabIndex = 3;
            // 
            // btnNuevoContinuar
            // 
            btnNuevoContinuar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNuevoContinuar.BackColor = Color.FromArgb(52, 152, 219);
            btnNuevoContinuar.FlatAppearance.BorderSize = 0;
            btnNuevoContinuar.FlatStyle = FlatStyle.Flat;
            btnNuevoContinuar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNuevoContinuar.ForeColor = Color.White;
            btnNuevoContinuar.Location = new Point(632, 8);
            btnNuevoContinuar.Name = "btnNuevoContinuar";
            btnNuevoContinuar.Size = new Size(126, 32);
            btnNuevoContinuar.TabIndex = 2;
            btnNuevoContinuar.Text = "➕ Guardar y Nuevo";
            btnNuevoContinuar.UseVisualStyleBackColor = false;
            btnNuevoContinuar.Visible = false;
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
            // pnlPreview
            // 
            pnlPreview.BackColor = Color.White;
            pnlPreview.BorderStyle = BorderStyle.FixedSingle;
            pnlPreview.Controls.Add(picBarcode);
            pnlPreview.Controls.Add(lblPreview);
            pnlPreview.Dock = DockStyle.Fill;
            pnlPreview.Location = new Point(12, 96);
            pnlPreview.Name = "pnlPreview";
            pnlPreview.Padding = new Padding(8);
            pnlPreview.Size = new Size(992, 102);
            pnlPreview.TabIndex = 2;
            // 
            // picBarcode
            // 
            picBarcode.Dock = DockStyle.Fill;
            picBarcode.Location = new Point(8, 24);
            picBarcode.Name = "picBarcode";
            picBarcode.Size = new Size(974, 68);
            picBarcode.SizeMode = PictureBoxSizeMode.CenterImage;
            picBarcode.TabIndex = 1;
            picBarcode.TabStop = false;
            // 
            // lblPreview
            // 
            lblPreview.Dock = DockStyle.Top;
            lblPreview.Font = new Font("Segoe UI", 8F);
            lblPreview.ForeColor = Color.FromArgb(150, 150, 150);
            lblPreview.Location = new Point(8, 8);
            lblPreview.Name = "lblPreview";
            lblPreview.Size = new Size(974, 16);
            lblPreview.TabIndex = 0;
            lblPreview.Text = "Vista previa del código de barras";
            lblPreview.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tlpCampos
            // 
            tlpCampos.ColumnCount = 6;
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpCampos.Controls.Add(lblCodigo, 0, 0);
            tlpCampos.Controls.Add(txtCodigo, 1, 0);
            tlpCampos.Controls.Add(lblTipoCodEdicion, 2, 0);
            tlpCampos.Controls.Add(cboTipoCodEdicion, 3, 0);
            tlpCampos.Controls.Add(chkPrincipal, 4, 0);
            tlpCampos.Dock = DockStyle.Top;
            tlpCampos.Location = new Point(12, 44);
            tlpCampos.Name = "tlpCampos";
            tlpCampos.Padding = new Padding(0, 8, 0, 8);
            tlpCampos.RowCount = 1;
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpCampos.Size = new Size(992, 52);
            tlpCampos.TabIndex = 1;
            // 
            // lblCodigo
            // 
            lblCodigo.Dock = DockStyle.Fill;
            lblCodigo.Font = new Font("Segoe UI", 9F);
            lblCodigo.ForeColor = Color.FromArgb(96, 94, 92);
            lblCodigo.Location = new Point(3, 8);
            lblCodigo.Name = "lblCodigo";
            lblCodigo.Size = new Size(94, 36);
            lblCodigo.TabIndex = 0;
            lblCodigo.Text = "Código:";
            lblCodigo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtCodigo
            // 
            txtCodigo.CharacterCasing = CharacterCasing.Upper;
            txtCodigo.Dock = DockStyle.Fill;
            txtCodigo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            txtCodigo.Location = new Point(103, 11);
            txtCodigo.MaxLength = 50;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.PlaceholderText = "Escaneé o digite código...";
            txtCodigo.Size = new Size(270, 32);
            txtCodigo.TabIndex = 1;
            // 
            // lblTipoCodEdicion
            // 
            lblTipoCodEdicion.Dock = DockStyle.Fill;
            lblTipoCodEdicion.Font = new Font("Segoe UI", 9F);
            lblTipoCodEdicion.ForeColor = Color.FromArgb(96, 94, 92);
            lblTipoCodEdicion.Location = new Point(379, 8);
            lblTipoCodEdicion.Name = "lblTipoCodEdicion";
            lblTipoCodEdicion.Size = new Size(94, 36);
            lblTipoCodEdicion.TabIndex = 2;
            lblTipoCodEdicion.Text = "Tipo:";
            lblTipoCodEdicion.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboTipoCodEdicion
            // 
            cboTipoCodEdicion.Dock = DockStyle.Fill;
            cboTipoCodEdicion.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoCodEdicion.Font = new Font("Segoe UI", 9F);
            cboTipoCodEdicion.FormattingEnabled = true;
            cboTipoCodEdicion.Items.AddRange(new object[] { "Normal", "Pesable (P)", "Interno (I)", "Proveedor (S)" });
            cboTipoCodEdicion.Location = new Point(479, 11);
            cboTipoCodEdicion.Name = "cboTipoCodEdicion";
            cboTipoCodEdicion.Size = new Size(201, 23);
            cboTipoCodEdicion.TabIndex = 3;
            // 
            // chkPrincipal
            // 
            chkPrincipal.AutoSize = true;
            chkPrincipal.Dock = DockStyle.Fill;
            chkPrincipal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            chkPrincipal.ForeColor = Color.FromArgb(230, 126, 34);
            chkPrincipal.Location = new Point(686, 11);
            chkPrincipal.Name = "chkPrincipal";
            chkPrincipal.Size = new Size(94, 30);
            chkPrincipal.TabIndex = 4;
            chkPrincipal.Text = "⭐ Principal";
            chkPrincipal.UseVisualStyleBackColor = true;
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
            lblModoEdicion.Text = "📷 Modo Escáner - Lectura Rápida";
            lblModoEdicion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ProdCodBarUC
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(tlpMain);
            Font = new Font("Segoe UI", 9F);
            Name = "ProdCodBarUC";
            Size = new Size(1024, 600);
            tlpMain.ResumeLayout(false);
            pnlGrilla.ResumeLayout(false);
            pnlGrilla.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCodigos).EndInit();
            tsCodigos.ResumeLayout(false);
            tsCodigos.PerformLayout();
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            pnlEdicion.ResumeLayout(false);
            pnlAcciones.ResumeLayout(false);
            pnlPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picBarcode).EndInit();
            tlpCampos.ResumeLayout(false);
            tlpCampos.PerformLayout();
            pnlEdicionHeader.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
