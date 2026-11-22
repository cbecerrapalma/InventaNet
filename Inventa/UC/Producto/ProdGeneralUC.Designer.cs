using System.Xml.Linq;

namespace Inventa.UC.Producto
{
    partial class ProdGeneralUC
    {
        private System.ComponentModel.IContainer components = null;

        // ── Contenedores Principales ───────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tlpCampos;

        // ── Labels de Información (Read-only) ──────────────────────────────────

        // ── Campos Editable ───────────────────────────────────────────────────
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblCategoria;
        private System.Windows.Forms.ComboBox cboCategoria;
        private System.Windows.Forms.Button btnAgregarCategoria;
        private System.Windows.Forms.Label lblMarca;
        private System.Windows.Forms.ComboBox cboMarca;
        private System.Windows.Forms.Button btnAgregarMarca;
        private System.Windows.Forms.Label lblUnidadBase;
        private System.Windows.Forms.ComboBox cboUnidadBase;
        private System.Windows.Forms.Button btnAgregarUnidadBase;
        private System.Windows.Forms.Label lblDiasVencimiento;
        private System.Windows.Forms.NumericUpDown numDiasVencimiento;
        private System.Windows.Forms.Label lblManejaLote;
        private System.Windows.Forms.CheckBox chkManejaLote;
        private System.Windows.Forms.Label lblEsPesable;
        private System.Windows.Forms.CheckBox chkEsPesable;
        private System.Windows.Forms.Label lblActivo;
        private System.Windows.Forms.CheckBox chkActivo;

        // ── Panel de Validación ────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelValidacion;
        private System.Windows.Forms.Label lblMensajeValidacion;
        private System.Windows.Forms.PictureBox picIconoValidacion;

        // ── Indicadores de Cambio ─────────────────────────────────────────────
        private System.Windows.Forms.Label lblEstadoEdicion;

        // ── ToolStrip para botones CRUD ────────────────────────────────────────
        private System.Windows.Forms.ToolStrip tsProducto;
        private System.Windows.Forms.ToolStripButton tsbNuevo;
        private System.Windows.Forms.ToolStripButton tsbEditar;
        private System.Windows.Forms.ToolStripButton tsbEliminar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripLabel tslEstadoProducto;

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
            tlpMain = new TableLayoutPanel();
            tsProducto = new ToolStrip();
            tsbNuevo = new ToolStripButton();
            tsbEditar = new ToolStripButton();
            tsbEliminar = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            tslEstadoProducto = new ToolStripLabel();
            tlpCampos = new TableLayoutPanel();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblNombre = new Label();
            txtNombre = new TextBox();
            lblCategoria = new Label();
            cboCategoria = new ComboBox();
            btnAgregarCategoria = new Button();
            lblMarca = new Label();
            cboMarca = new ComboBox();
            btnAgregarMarca = new Button();
            lblUnidadBase = new Label();
            cboUnidadBase = new ComboBox();
            btnAgregarUnidadBase = new Button();
            lblDiasVencimiento = new Label();
            numDiasVencimiento = new NumericUpDown();
            lblManejaLote = new Label();
            chkManejaLote = new CheckBox();
            lblEsPesable = new Label();
            chkEsPesable = new CheckBox();
            lblActivo = new Label();
            chkActivo = new CheckBox();
            panelValidacion = new Panel();
            lblMensajeValidacion = new Label();
            picIconoValidacion = new PictureBox();
            pnlAcciones = new Panel();
            button1 = new Button();
            btnGuardar = new Button();
            btnCancelar = new Button();
            lblEstadoEdicion = new Label();
            tlpMain.SuspendLayout();
            tsProducto.SuspendLayout();
            tlpCampos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numDiasVencimiento).BeginInit();
            panelValidacion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picIconoValidacion).BeginInit();
            pnlAcciones.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(tsProducto, 0, 0);
            tlpMain.Controls.Add(tlpCampos, 0, 1);
            tlpMain.Controls.Add(panelValidacion, 0, 2);
            tlpMain.Controls.Add(pnlAcciones, 0, 3);
            tlpMain.Controls.Add(lblEstadoEdicion, 0, 4);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.Padding = new Padding(8);
tlpMain.RowCount = 5;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tlpMain.Size = new Size(800, 500);
            tlpMain.TabIndex = 0;
            // 

            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // tsProducto
            // 
            tsProducto.GripStyle = ToolStripGripStyle.Hidden;
            tsProducto.Items.AddRange(new ToolStripItem[] { tsbNuevo, tsbEditar, tsbEliminar, tsSep1, tslEstadoProducto });
            tsProducto.Location = new Point(8, 88);
            tsProducto.Name = "tsProducto";
            tsProducto.Padding = new Padding(4);
            tsProducto.Size = new Size(784, 31);
            tsProducto.TabIndex = 0;
            // 
            // tsbNuevo
            // 
            tsbNuevo.Image = Properties.Resources.add;
            tsbNuevo.ImageTransparentColor = Color.Magenta;
            tsbNuevo.Name = "tsbNuevo";
            tsbNuevo.Size = new Size(62, 20);
            tsbNuevo.Text = "Nuevo";
            tsbNuevo.ToolTipText = "Crear nuevo producto (Ctrl+N)";
            // 
            // tsbEditar
            // 
            tsbEditar.Enabled = false;
            tsbEditar.Image = Properties.Resources.edit;
            tsbEditar.ImageTransparentColor = Color.Magenta;
            tsbEditar.Name = "tsbEditar";
            tsbEditar.Size = new Size(57, 20);
            tsbEditar.Text = "Editar";
            tsbEditar.ToolTipText = "Editar producto seleccionado (F2)";
            // 
            // tsbEliminar
            // 
            tsbEliminar.Enabled = false;
            tsbEliminar.Image = Properties.Resources.delete;
            tsbEliminar.ImageTransparentColor = Color.Magenta;
            tsbEliminar.Name = "tsbEliminar";
            tsbEliminar.Size = new Size(70, 20);
            tsbEliminar.Text = "Eliminar";
            tsbEliminar.ToolTipText = "Eliminar producto seleccionado (Supr)";
            // 
            // tsSep1
            // 
            tsSep1.Name = "tsSep1";
            tsSep1.Size = new Size(6, 23);
            // 
            // tslEstadoProducto
            // 
            tslEstadoProducto.Alignment = ToolStripItemAlignment.Right;
            tslEstadoProducto.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            tslEstadoProducto.ForeColor = Color.FromArgb(96, 94, 92);
            tslEstadoProducto.Name = "tslEstadoProducto";
            tslEstadoProducto.Size = new Size(141, 20);
            tslEstadoProducto.Text = "Sin producto seleccionado";
            tslEstadoProducto.TextAlign = ContentAlignment.MiddleRight;
            // 
            // tlpCampos
            // 
            tlpCampos.ColumnCount = 6;
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36F));
            tlpCampos.Controls.Add(lblCodigo, 0, 0);
            tlpCampos.Controls.Add(txtCodigo, 1, 0);
            tlpCampos.Controls.Add(lblNombre, 3, 0);
            tlpCampos.Controls.Add(txtNombre, 4, 0);
            tlpCampos.Controls.Add(lblCategoria, 0, 1);
            tlpCampos.Controls.Add(cboCategoria, 1, 1);
            tlpCampos.Controls.Add(btnAgregarCategoria, 2, 1);
            tlpCampos.Controls.Add(lblMarca, 3, 1);
            tlpCampos.Controls.Add(cboMarca, 4, 1);
            tlpCampos.Controls.Add(btnAgregarMarca, 5, 1);
            tlpCampos.Controls.Add(lblUnidadBase, 0, 2);
            tlpCampos.Controls.Add(cboUnidadBase, 1, 2);
            tlpCampos.Controls.Add(btnAgregarUnidadBase, 2, 2);
            tlpCampos.Controls.Add(lblDiasVencimiento, 3, 2);
            tlpCampos.Controls.Add(numDiasVencimiento, 4, 2);
            tlpCampos.Controls.Add(lblManejaLote, 0, 3);
            tlpCampos.Controls.Add(chkManejaLote, 1, 3);
            tlpCampos.Controls.Add(lblEsPesable, 3, 3);
            tlpCampos.Controls.Add(chkEsPesable, 4, 3);
            tlpCampos.Controls.Add(lblActivo, 0, 4);
            tlpCampos.Controls.Add(chkActivo, 1, 4);
            tlpCampos.Dock = DockStyle.Fill;
            tlpCampos.Location = new Point(11, 131);
            tlpCampos.Name = "tlpCampos";
            tlpCampos.Padding = new Padding(4);
            tlpCampos.RowCount = 5;
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tlpCampos.Size = new Size(778, 246);
            tlpCampos.TabIndex = 1;
            // 
            // lblCodigo
            // 
            lblCodigo.Dock = DockStyle.Fill;
            lblCodigo.Font = new Font("Segoe UI", 9F);
            lblCodigo.ForeColor = Color.FromArgb(96, 94, 92);
            lblCodigo.Location = new Point(7, 4);
            lblCodigo.Name = "lblCodigo";
            lblCodigo.Size = new Size(104, 40);
            lblCodigo.TabIndex = 0;
            lblCodigo.Text = "Código *";
            lblCodigo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtCodigo
            // 
            txtCodigo.CharacterCasing = CharacterCasing.Upper;
            txtCodigo.Dock = DockStyle.Fill;
            txtCodigo.Font = new Font("Segoe UI", 10F);
            txtCodigo.Location = new Point(117, 10);
            txtCodigo.Margin = new Padding(3, 6, 3, 6);
            txtCodigo.MaxLength = 50;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.PlaceholderText = "Código único del producto";
            txtCodigo.Size = new Size(185, 25);
            txtCodigo.TabIndex = 1;
            // 
            // lblNombre
            // 
            lblNombre.Dock = DockStyle.Fill;
            lblNombre.Font = new Font("Segoe UI", 9F);
            lblNombre.ForeColor = Color.FromArgb(96, 94, 92);
            lblNombre.Location = new Point(344, 4);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(104, 40);
            lblNombre.TabIndex = 2;
            lblNombre.Text = "Nombre *";
            lblNombre.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtNombre
            // 
            txtNombre.Dock = DockStyle.Fill;
            txtNombre.Font = new Font("Segoe UI", 10F);
            txtNombre.Location = new Point(454, 10);
            txtNombre.Margin = new Padding(3, 6, 3, 6);
            txtNombre.MaxLength = 150;
            txtNombre.Name = "txtNombre";
            txtNombre.PlaceholderText = "Nombre descriptivo del producto";
            txtNombre.Size = new Size(280, 25);
            txtNombre.TabIndex = 3;
            // 
            // lblCategoria
            // 
            lblCategoria.Dock = DockStyle.Fill;
            lblCategoria.Font = new Font("Segoe UI", 9F);
            lblCategoria.ForeColor = Color.FromArgb(96, 94, 92);
            lblCategoria.Location = new Point(7, 44);
            lblCategoria.Name = "lblCategoria";
            lblCategoria.Size = new Size(104, 40);
            lblCategoria.TabIndex = 4;
            lblCategoria.Text = "Categoría *";
            lblCategoria.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboCategoria
            // 
            cboCategoria.Dock = DockStyle.Fill;
            cboCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategoria.Font = new Font("Segoe UI", 9F);
            cboCategoria.FormattingEnabled = true;
            cboCategoria.Location = new Point(117, 50);
            cboCategoria.Margin = new Padding(3, 6, 3, 6);
            cboCategoria.Name = "cboCategoria";
            cboCategoria.Size = new Size(185, 23);
            cboCategoria.TabIndex = 5;
            // 
            // btnAgregarCategoria
            // 
            btnAgregarCategoria.BackColor = Color.FromArgb(240, 247, 255);
            btnAgregarCategoria.Dock = DockStyle.Fill;
            btnAgregarCategoria.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
            btnAgregarCategoria.FlatStyle = FlatStyle.Flat;
            btnAgregarCategoria.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnAgregarCategoria.ForeColor = Color.FromArgb(0, 120, 215);
            btnAgregarCategoria.Location = new Point(308, 47);
            btnAgregarCategoria.Margin = new Padding(3, 3, 3, 7);
            btnAgregarCategoria.Name = "btnAgregarCategoria";
            btnAgregarCategoria.Size = new Size(30, 30);
            btnAgregarCategoria.TabIndex = 6;
            btnAgregarCategoria.Text = "+";
            btnAgregarCategoria.UseVisualStyleBackColor = false;
            // 
            // lblMarca
            // 
            lblMarca.Dock = DockStyle.Fill;
            lblMarca.Font = new Font("Segoe UI", 9F);
            lblMarca.ForeColor = Color.FromArgb(96, 94, 92);
            lblMarca.Location = new Point(344, 44);
            lblMarca.Name = "lblMarca";
            lblMarca.Size = new Size(104, 40);
            lblMarca.TabIndex = 7;
            lblMarca.Text = "Marca";
            lblMarca.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboMarca
            // 
            cboMarca.Dock = DockStyle.Fill;
            cboMarca.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMarca.Font = new Font("Segoe UI", 9F);
            cboMarca.FormattingEnabled = true;
            cboMarca.Location = new Point(454, 50);
            cboMarca.Margin = new Padding(3, 6, 3, 6);
            cboMarca.Name = "cboMarca";
            cboMarca.Size = new Size(280, 23);
            cboMarca.TabIndex = 8;
            // 
            // btnAgregarMarca
            // 
            btnAgregarMarca.BackColor = Color.FromArgb(240, 247, 255);
            btnAgregarMarca.Dock = DockStyle.Fill;
            btnAgregarMarca.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
            btnAgregarMarca.FlatStyle = FlatStyle.Flat;
            btnAgregarMarca.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnAgregarMarca.ForeColor = Color.FromArgb(0, 120, 215);
            btnAgregarMarca.Location = new Point(740, 47);
            btnAgregarMarca.Margin = new Padding(3, 3, 3, 7);
            btnAgregarMarca.Name = "btnAgregarMarca";
            btnAgregarMarca.Size = new Size(31, 30);
            btnAgregarMarca.TabIndex = 9;
            btnAgregarMarca.Text = "+";
            btnAgregarMarca.UseVisualStyleBackColor = false;
            // 
            // lblUnidadBase
            // 
            lblUnidadBase.Dock = DockStyle.Fill;
            lblUnidadBase.Font = new Font("Segoe UI", 9F);
            lblUnidadBase.ForeColor = Color.FromArgb(96, 94, 92);
            lblUnidadBase.Location = new Point(7, 84);
            lblUnidadBase.Name = "lblUnidadBase";
            lblUnidadBase.Size = new Size(104, 40);
            lblUnidadBase.TabIndex = 10;
            lblUnidadBase.Text = "Unidad Base *";
            lblUnidadBase.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboUnidadBase
            // 
            cboUnidadBase.Dock = DockStyle.Fill;
            cboUnidadBase.DropDownStyle = ComboBoxStyle.DropDownList;
            cboUnidadBase.Font = new Font("Segoe UI", 9F);
            cboUnidadBase.FormattingEnabled = true;
            cboUnidadBase.Location = new Point(117, 90);
            cboUnidadBase.Margin = new Padding(3, 6, 3, 6);
            cboUnidadBase.Name = "cboUnidadBase";
            cboUnidadBase.Size = new Size(185, 23);
            cboUnidadBase.TabIndex = 11;
            // 
            // btnAgregarUnidadBase
            // 
            btnAgregarUnidadBase.BackColor = Color.FromArgb(240, 247, 255);
            btnAgregarUnidadBase.Dock = DockStyle.Fill;
            btnAgregarUnidadBase.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
            btnAgregarUnidadBase.FlatStyle = FlatStyle.Flat;
            btnAgregarUnidadBase.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnAgregarUnidadBase.ForeColor = Color.FromArgb(0, 120, 215);
            btnAgregarUnidadBase.Location = new Point(308, 87);
            btnAgregarUnidadBase.Margin = new Padding(3, 3, 3, 7);
            btnAgregarUnidadBase.Name = "btnAgregarUnidadBase";
            btnAgregarUnidadBase.Size = new Size(30, 30);
            btnAgregarUnidadBase.TabIndex = 12;
            btnAgregarUnidadBase.Text = "+";
            btnAgregarUnidadBase.UseVisualStyleBackColor = false;
            // 
            // lblDiasVencimiento
            // 
            lblDiasVencimiento.Dock = DockStyle.Fill;
            lblDiasVencimiento.Font = new Font("Segoe UI", 9F);
            lblDiasVencimiento.ForeColor = Color.FromArgb(96, 94, 92);
            lblDiasVencimiento.Location = new Point(344, 84);
            lblDiasVencimiento.Name = "lblDiasVencimiento";
            lblDiasVencimiento.Size = new Size(104, 40);
            lblDiasVencimiento.TabIndex = 13;
            lblDiasVencimiento.Text = "Días Venc.:";
            lblDiasVencimiento.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numDiasVencimiento
            // 
            numDiasVencimiento.Dock = DockStyle.Fill;
            numDiasVencimiento.Font = new Font("Segoe UI", 9F);
            numDiasVencimiento.Location = new Point(454, 90);
            numDiasVencimiento.Margin = new Padding(3, 6, 3, 6);
            numDiasVencimiento.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numDiasVencimiento.Name = "numDiasVencimiento";
            numDiasVencimiento.Size = new Size(280, 23);
            numDiasVencimiento.TabIndex = 14;
            // 
            // lblManejaLote
            // 
            lblManejaLote.Dock = DockStyle.Fill;
            lblManejaLote.Font = new Font("Segoe UI", 9F);
            lblManejaLote.ForeColor = Color.FromArgb(96, 94, 92);
            lblManejaLote.Location = new Point(7, 124);
            lblManejaLote.Name = "lblManejaLote";
            lblManejaLote.Size = new Size(104, 40);
            lblManejaLote.TabIndex = 15;
            lblManejaLote.Text = "Maneja Lote:";
            lblManejaLote.TextAlign = ContentAlignment.MiddleRight;
            // 
            // chkManejaLote
            // 
            chkManejaLote.AutoSize = true;
            chkManejaLote.Dock = DockStyle.Fill;
            chkManejaLote.Font = new Font("Segoe UI", 9F);
            chkManejaLote.Location = new Point(117, 130);
            chkManejaLote.Margin = new Padding(3, 6, 3, 6);
            chkManejaLote.Name = "chkManejaLote";
            chkManejaLote.Padding = new Padding(4, 4, 0, 0);
            chkManejaLote.Size = new Size(185, 28);
            chkManejaLote.TabIndex = 16;
            chkManejaLote.Text = "Sí, requiere control de lotes";
            chkManejaLote.UseVisualStyleBackColor = true;
            // 
            // lblEsPesable
            // 
            lblEsPesable.Dock = DockStyle.Fill;
            lblEsPesable.Font = new Font("Segoe UI", 9F);
            lblEsPesable.ForeColor = Color.FromArgb(96, 94, 92);
            lblEsPesable.Location = new Point(344, 124);
            lblEsPesable.Name = "lblEsPesable";
            lblEsPesable.Size = new Size(104, 40);
            lblEsPesable.TabIndex = 17;
            lblEsPesable.Text = "Es Pesable:";
            lblEsPesable.TextAlign = ContentAlignment.MiddleRight;
            // 
            // chkEsPesable
            // 
            chkEsPesable.AutoSize = true;
            chkEsPesable.Dock = DockStyle.Fill;
            chkEsPesable.Font = new Font("Segoe UI", 9F);
            chkEsPesable.Location = new Point(454, 130);
            chkEsPesable.Margin = new Padding(3, 6, 3, 6);
            chkEsPesable.Name = "chkEsPesable";
            chkEsPesable.Padding = new Padding(4, 4, 0, 0);
            chkEsPesable.Size = new Size(280, 28);
            chkEsPesable.TabIndex = 18;
            chkEsPesable.Text = "Sí, se vende por peso (requiere báscula)";
            chkEsPesable.UseVisualStyleBackColor = true;
            // 
            // lblActivo
            // 
            lblActivo.Dock = DockStyle.Fill;
            lblActivo.Font = new Font("Segoe UI", 9F);
            lblActivo.ForeColor = Color.FromArgb(96, 94, 92);
            lblActivo.Location = new Point(7, 164);
            lblActivo.Name = "lblActivo";
            lblActivo.Size = new Size(104, 78);
            lblActivo.TabIndex = 19;
            lblActivo.Text = "Activo:";
            lblActivo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // chkActivo
            // 
            chkActivo.AutoSize = true;
            chkActivo.Checked = true;
            chkActivo.CheckState = CheckState.Checked;
            chkActivo.Dock = DockStyle.Fill;
            chkActivo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            chkActivo.ForeColor = Color.FromArgb(46, 204, 113);
            chkActivo.Location = new Point(117, 170);
            chkActivo.Margin = new Padding(3, 6, 3, 6);
            chkActivo.Name = "chkActivo";
            chkActivo.Padding = new Padding(4, 4, 0, 0);
            chkActivo.Size = new Size(185, 66);
            chkActivo.TabIndex = 20;
            chkActivo.Text = "✓ Producto activo";
            chkActivo.UseVisualStyleBackColor = true;
            // 
            // panelValidacion
            // 
            panelValidacion.BackColor = Color.FromArgb(254, 245, 231);
            panelValidacion.BorderStyle = BorderStyle.FixedSingle;
            panelValidacion.Controls.Add(lblMensajeValidacion);
            panelValidacion.Controls.Add(picIconoValidacion);
            panelValidacion.Dock = DockStyle.Fill;
            panelValidacion.Location = new Point(11, 386);
            panelValidacion.Margin = new Padding(3, 6, 3, 3);
            panelValidacion.Name = "panelValidacion";
            panelValidacion.Padding = new Padding(8);
            panelValidacion.Size = new Size(778, 31);
            panelValidacion.TabIndex = 2;
            panelValidacion.Visible = false;
            // 
            // lblMensajeValidacion
            // 
            lblMensajeValidacion.Dock = DockStyle.Fill;
            lblMensajeValidacion.Font = new Font("Segoe UI", 9F);
            lblMensajeValidacion.ForeColor = Color.FromArgb(211, 84, 0);
            lblMensajeValidacion.Location = new Point(28, 8);
            lblMensajeValidacion.Name = "lblMensajeValidacion";
            lblMensajeValidacion.Size = new Size(740, 13);
            lblMensajeValidacion.TabIndex = 1;
            lblMensajeValidacion.Text = "Mensaje de validación";
            lblMensajeValidacion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // picIconoValidacion
            // 
            picIconoValidacion.Dock = DockStyle.Left;
            picIconoValidacion.Location = new Point(8, 8);
            picIconoValidacion.Name = "picIconoValidacion";
            picIconoValidacion.Size = new Size(20, 13);
            picIconoValidacion.SizeMode = PictureBoxSizeMode.Zoom;
            picIconoValidacion.TabIndex = 0;
            picIconoValidacion.TabStop = false;
            // 
            // pnlAcciones
            // 
            pnlAcciones.Controls.Add(button1);
            pnlAcciones.Controls.Add(btnGuardar);
            pnlAcciones.Controls.Add(btnCancelar);
            pnlAcciones.Dock = DockStyle.Fill;
            pnlAcciones.Location = new Point(11, 423);
            pnlAcciones.Name = "pnlAcciones";
            pnlAcciones.Padding = new Padding(0, 8, 8, 0);
            pnlAcciones.RightToLeft = RightToLeft.Yes;
            pnlAcciones.Size = new Size(778, 42);
            pnlAcciones.TabIndex = 4;
            pnlAcciones.Visible = false;
            // 
            // button1
            // 
            button1.Location = new Point(7, 13);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 2;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGuardar.BackColor = Color.FromArgb(46, 204, 113);
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(658, 8);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 32);
            btnGuardar.TabIndex = 0;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = false;
            // 
            // btnCancelar
            // 
            btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelar.BackColor = Color.White;
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(189, 195, 199);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Segoe UI", 9F);
            btnCancelar.ForeColor = Color.FromArgb(50, 49, 48);
            btnCancelar.Location = new Point(538, 8);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 32);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            // 
            // lblEstadoEdicion
            // 
            lblEstadoEdicion.Dock = DockStyle.Fill;
            lblEstadoEdicion.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            lblEstadoEdicion.ForeColor = Color.FromArgb(149, 165, 166);
            lblEstadoEdicion.Location = new Point(11, 468);
            lblEstadoEdicion.Name = "lblEstadoEdicion";
            lblEstadoEdicion.Size = new Size(778, 24);
            lblEstadoEdicion.TabIndex = 3;
            lblEstadoEdicion.Text = "💡 Doble clic en un producto de la lista para editar • Los campos con * son obligatorios";
            lblEstadoEdicion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ProdGeneralUC
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(tlpMain);
            Font = new Font("Segoe UI", 9F);
            Name = "ProdGeneralUC";
            Size = new Size(800, 500);
tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            tsProducto.ResumeLayout(false);
            tsProducto.PerformLayout();
            tlpCampos.ResumeLayout(false);
            tlpCampos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numDiasVencimiento).EndInit();
            panelValidacion.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picIconoValidacion).EndInit();
            pnlAcciones.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Button button1;
    }
}
