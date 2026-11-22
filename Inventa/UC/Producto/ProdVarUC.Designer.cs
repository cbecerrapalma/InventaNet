namespace Inventa.UC.Producto
{
    partial class ProdVarUC
    {
        private System.ComponentModel.IContainer components = null;

// ── Layout Principal ──────────────────────────────────────────────────
         private System.Windows.Forms.TableLayoutPanel tlpMain;


        // ── Panel Superior: Grilla de Variantes ────────────────────────────────
        private System.Windows.Forms.Panel pnlGrilla;
        private System.Windows.Forms.ToolStrip tsVariantes;
        private System.Windows.Forms.ToolStripButton tsbNuevo;
        private System.Windows.Forms.ToolStripButton tsbEditar;
        private System.Windows.Forms.ToolStripButton tsbEliminar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripLabel tslVarianteSeleccionada;
        private System.Windows.Forms.DataGridView dgvVariantes;

        // ── Splitter ───────────────────────────────────────────────────────────
        private System.Windows.Forms.Splitter splitter;

        // ── Panel Inferior: Edición Inline ───────────────────────────────────
        private System.Windows.Forms.Panel pnlEdicion;
        private System.Windows.Forms.Panel pnlEdicionHeader;
        private System.Windows.Forms.Label lblModoEdicion;
        private System.Windows.Forms.Button btnCerrarEdicion;

        // ── Campos de edición ────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpCampos;
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblCostoProm;
        private System.Windows.Forms.NumericUpDown numCostoProm;
        private System.Windows.Forms.CheckBox chkActivo;

        // ── Panel de Atributos Dinámicos ───────────────────────────────────────
        private System.Windows.Forms.GroupBox gpbAtributos;
        private System.Windows.Forms.FlowLayoutPanel flpAtributos;

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
            dgvVariantes = new DataGridView();
            tsVariantes = new ToolStrip();
            tsbNuevo = new ToolStripButton();
            tsbEditar = new ToolStripButton();
            tsbEliminar = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            tslVarianteSeleccionada = new ToolStripLabel();
            splitter = new Splitter();
            pnlEdicion = new Panel();
            pnlAcciones = new Panel();
            btnCancelar = new Button();
            btnGuardar = new Button();
            gpbAtributos = new GroupBox();
            flpAtributos = new FlowLayoutPanel();
            tlpCampos = new TableLayoutPanel();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblNombre = new Label();
            txtNombre = new TextBox();
            lblCostoProm = new Label();
            numCostoProm = new NumericUpDown();
            chkActivo = new CheckBox();
            pnlEdicionHeader = new Panel();
            btnCerrarEdicion = new Button();
            lblModoEdicion = new Label();
            tlpMain.SuspendLayout();
            pnlGrilla.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvVariantes).BeginInit();
            tsVariantes.SuspendLayout();
            pnlEdicion.SuspendLayout();
            pnlAcciones.SuspendLayout();
            gpbAtributos.SuspendLayout();
            tlpCampos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCostoProm).BeginInit();
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
            pnlGrilla.Controls.Add(dgvVariantes);
            pnlGrilla.Controls.Add(tsVariantes);
            pnlGrilla.Dock = DockStyle.Fill;
            pnlGrilla.Location = new Point(3, 53);
            pnlGrilla.Name = "pnlGrilla";
            pnlGrilla.Padding = new Padding(8);
            pnlGrilla.Size = new Size(1018, 320);
            pnlGrilla.TabIndex = 0;
            // 
            // dgvVariantes
            // 
            dgvVariantes.AllowUserToAddRows = false;
            dgvVariantes.AllowUserToDeleteRows = false;
            dgvVariantes.BackgroundColor = Color.White;
            dgvVariantes.BorderStyle = BorderStyle.None;
            dgvVariantes.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvVariantes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvVariantes.ColumnHeadersHeight = 36;
            dgvVariantes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvVariantes.Dock = DockStyle.Fill;
            dgvVariantes.EnableHeadersVisualStyles = false;
            dgvVariantes.GridColor = Color.FromArgb(237, 235, 233);
            dgvVariantes.Location = new Point(8, 39);
            dgvVariantes.MultiSelect = false;
            dgvVariantes.Name = "dgvVariantes";
            dgvVariantes.ReadOnly = true;
            dgvVariantes.RowHeadersVisible = false;
            dgvVariantes.RowTemplate.Height = 36;
            dgvVariantes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvVariantes.Size = new Size(1002, 273);
            dgvVariantes.TabIndex = 1;
            // 
            // tsVariantes
            // 
            tsVariantes.GripStyle = ToolStripGripStyle.Hidden;
            tsVariantes.Items.AddRange(new ToolStripItem[] { tsbNuevo, tsbEditar, tsbEliminar, tsSep1, tslVarianteSeleccionada });
            tsVariantes.Location = new Point(8, 8);
            tsVariantes.Name = "tsVariantes";
            tsVariantes.Padding = new Padding(4);
            tsVariantes.Size = new Size(1002, 31);
            tsVariantes.TabIndex = 0;
            // 
            // tsbNuevo
            // 
            tsbNuevo.Image = Properties.Resources.add;
            tsbNuevo.ImageTransparentColor = Color.Magenta;
            tsbNuevo.Name = "tsbNuevo";
            tsbNuevo.Size = new Size(62, 20);
            tsbNuevo.Text = "Nuevo";
            tsbNuevo.ToolTipText = "Crear nueva variante (Insert)";
            // 
            // tsbEditar
            // 
            tsbEditar.Enabled = false;
            tsbEditar.Image = Properties.Resources.edit;
            tsbEditar.ImageTransparentColor = Color.Magenta;
            tsbEditar.Name = "tsbEditar";
            tsbEditar.Size = new Size(57, 20);
            tsbEditar.Text = "Editar";
            tsbEditar.ToolTipText = "Editar variante seleccionada (F2)";
            // 
            // tsbEliminar
            // 
            tsbEliminar.Enabled = false;
            tsbEliminar.Image = Properties.Resources.delete;
            tsbEliminar.ImageTransparentColor = Color.Magenta;
            tsbEliminar.Name = "tsbEliminar";
            tsbEliminar.Size = new Size(70, 20);
            tsbEliminar.Text = "Eliminar";
            tsbEliminar.ToolTipText = "Eliminar variante seleccionada (Supr)";
            // 
            // tsSep1
            // 
            tsSep1.Name = "tsSep1";
            tsSep1.Size = new Size(6, 23);
            // 
            // tslVarianteSeleccionada
            // 
            tslVarianteSeleccionada.Alignment = ToolStripItemAlignment.Right;
            tslVarianteSeleccionada.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            tslVarianteSeleccionada.ForeColor = Color.FromArgb(96, 94, 92);
            tslVarianteSeleccionada.Name = "tslVarianteSeleccionada";
            tslVarianteSeleccionada.Size = new Size(140, 20);
            tslVarianteSeleccionada.Text = "Sin variante seleccionada";
            tslVarianteSeleccionada.TextAlign = ContentAlignment.MiddleRight;
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
            pnlEdicion.Controls.Add(gpbAtributos);
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
            // gpbAtributos
            // 
            gpbAtributos.Controls.Add(flpAtributos);
            gpbAtributos.Dock = DockStyle.Fill;
            gpbAtributos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gpbAtributos.ForeColor = Color.FromArgb(50, 49, 48);
            gpbAtributos.Location = new Point(12, 120);
            gpbAtributos.Name = "gpbAtributos";
            gpbAtributos.Padding = new Padding(8);
            gpbAtributos.Size = new Size(992, 78);
            gpbAtributos.TabIndex = 2;
            gpbAtributos.TabStop = false;
            gpbAtributos.Text = "🎨 Atributos de la Categoría";
            gpbAtributos.Visible = false;
            // 
            // flpAtributos
            // 
            flpAtributos.AutoScroll = true;
            flpAtributos.Dock = DockStyle.Fill;
            flpAtributos.Location = new Point(8, 24);
            flpAtributos.Name = "flpAtributos";
            flpAtributos.Padding = new Padding(4);
            flpAtributos.Size = new Size(976, 46);
            flpAtributos.TabIndex = 0;
            // 
            // tlpCampos
            // 
            tlpCampos.ColumnCount = 6;
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpCampos.Controls.Add(lblCodigo, 0, 0);
            tlpCampos.Controls.Add(txtCodigo, 1, 0);
            tlpCampos.Controls.Add(lblNombre, 2, 0);
            tlpCampos.Controls.Add(txtNombre, 3, 0);
            tlpCampos.Controls.Add(lblCostoProm, 4, 0);
            tlpCampos.Controls.Add(numCostoProm, 5, 0);
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
            txtCodigo.Font = new Font("Segoe UI", 9F);
            txtCodigo.Location = new Point(103, 11);
            txtCodigo.MaxLength = 50;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.PlaceholderText = "VAR-001";
            txtCodigo.Size = new Size(201, 23);
            txtCodigo.TabIndex = 1;
            // 
            // lblNombre
            // 
            lblNombre.Dock = DockStyle.Fill;
            lblNombre.Font = new Font("Segoe UI", 9F);
            lblNombre.ForeColor = Color.FromArgb(96, 94, 92);
            lblNombre.Location = new Point(310, 8);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(94, 36);
            lblNombre.TabIndex = 2;
            lblNombre.Text = "Nombre:";
            lblNombre.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtNombre
            // 
            txtNombre.Dock = DockStyle.Fill;
            txtNombre.Font = new Font("Segoe UI", 9F);
            txtNombre.Location = new Point(410, 11);
            txtNombre.MaxLength = 100;
            txtNombre.Name = "txtNombre";
            txtNombre.PlaceholderText = "Descripción de la variante";
            txtNombre.Size = new Size(270, 23);
            txtNombre.TabIndex = 3;
            // 
            // lblCostoProm
            // 
            lblCostoProm.Dock = DockStyle.Fill;
            lblCostoProm.Font = new Font("Segoe UI", 9F);
            lblCostoProm.ForeColor = Color.FromArgb(96, 94, 92);
            lblCostoProm.Location = new Point(686, 8);
            lblCostoProm.Name = "lblCostoProm";
            lblCostoProm.Size = new Size(94, 36);
            lblCostoProm.TabIndex = 4;
            lblCostoProm.Text = "Costo Prom.:";
            lblCostoProm.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numCostoProm
            // 
            numCostoProm.DecimalPlaces = 4;
            numCostoProm.Dock = DockStyle.Fill;
            numCostoProm.Font = new Font("Segoe UI", 9F);
            numCostoProm.Location = new Point(786, 11);
            numCostoProm.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numCostoProm.Name = "numCostoProm";
            numCostoProm.Size = new Size(203, 23);
            numCostoProm.TabIndex = 5;
            numCostoProm.TextAlign = HorizontalAlignment.Right;
            numCostoProm.ThousandsSeparator = true;
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
            chkActivo.Size = new Size(201, 26);
            chkActivo.TabIndex = 6;
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
            lblModoEdicion.Text = "🆕 Nueva Variante";
            lblModoEdicion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ProdVarUC
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(tlpMain);
            Font = new Font("Segoe UI", 9F);
            Name = "ProdVarUC";
            Size = new Size(1024, 600);
tlpMain.ResumeLayout(false);
             pnlGrilla.ResumeLayout(false);
            pnlGrilla.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvVariantes).EndInit();
            tsVariantes.ResumeLayout(false);
            tsVariantes.PerformLayout();
            pnlEdicion.ResumeLayout(false);
            pnlAcciones.ResumeLayout(false);
            gpbAtributos.ResumeLayout(false);
            tlpCampos.ResumeLayout(false);
            tlpCampos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCostoProm).EndInit();
            pnlEdicionHeader.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
