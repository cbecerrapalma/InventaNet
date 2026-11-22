namespace Inventa.UC.Producto
{
    partial class ProdConvUC
    {
        private System.ComponentModel.IContainer components = null;

// ── Layout Principal ──────────────────────────────────────────────────
         private System.Windows.Forms.TableLayoutPanel tlpMain;

         // ── Header de Variante (promoted inner table) ──────────────────────────

        // ── Panel Superior: Selector + Grilla ──────────────────────────────────
        private System.Windows.Forms.Panel pnlSuperior;
        private System.Windows.Forms.ToolStrip tsConversiones;
        private System.Windows.Forms.ToolStripButton tsbNuevo;
        private System.Windows.Forms.ToolStripButton tsbEditar;
        private System.Windows.Forms.ToolStripButton tsbEliminar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripLabel tslInfo;
        private System.Windows.Forms.DataGridView dgvConversiones;

        // ── Splitter ───────────────────────────────────────────────────────────
        private System.Windows.Forms.Splitter splitter;

        // ── Panel Inferior: Edición Inline ───────────────────────────────────
        private System.Windows.Forms.Panel pnlEdicion;
        private System.Windows.Forms.Panel pnlEdicionHeader;
        private System.Windows.Forms.Label lblModoEdicion;
        private System.Windows.Forms.Button btnCerrarEdicion;

        // ── Campos de edición ────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpCampos;
        private System.Windows.Forms.Label lblUnidad;
        private System.Windows.Forms.ComboBox cboUnidad;
        private System.Windows.Forms.Label lblFactor;
        private System.Windows.Forms.TextBox txtFactor;

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
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
tlpMain = new TableLayoutPanel();
             pnlSuperior = new Panel();
            dgvConversiones = new DataGridView();
            tsConversiones = new ToolStrip();
            tsbNuevo = new ToolStripButton();
            tsbEditar = new ToolStripButton();
            tsbEliminar = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            tslInfo = new ToolStripLabel();
            splitter = new Splitter();
            pnlEdicion = new Panel();
            pnlAcciones = new Panel();
            btnCancelar = new Button();
            btnGuardar = new Button();
            tlpCampos = new TableLayoutPanel();
            lblUnidad = new Label();
            cboUnidad = new ComboBox();
            lblFactor = new Label();
            txtFactor = new TextBox();
            pnlEdicionHeader = new Panel();
            btnCerrarEdicion = new Button();
            lblModoEdicion = new Label();
tlpMain.SuspendLayout();
            pnlSuperior.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvConversiones).BeginInit();
            tsConversiones.SuspendLayout();
            pnlEdicion.SuspendLayout();
            pnlAcciones.SuspendLayout();
            tlpCampos.SuspendLayout();
            pnlEdicionHeader.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(pnlSuperior, 0, 1);
            tlpMain.Controls.Add(splitter, 0, 2);
            tlpMain.Controls.Add(pnlEdicion, 0, 3);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 6F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            tlpMain.Size = new Size(1024, 608);
            tlpMain.TabIndex = 0;
            // 

            // 
            // 
            // 
            // 
            // 
            // 
            // 
            // pnlSuperior
            // 
            pnlSuperior.Controls.Add(dgvConversiones);
            pnlSuperior.Controls.Add(tsConversiones);
            pnlSuperior.Dock = DockStyle.Fill;
            pnlSuperior.Location = new Point(3, 53);
            pnlSuperior.Name = "pnlSuperior";
            pnlSuperior.Padding = new Padding(8);
            pnlSuperior.Size = new Size(1018, 325);
            pnlSuperior.TabIndex = 0;
            // 
            // dgvConversiones
            // 
            dgvConversiones.AllowUserToAddRows = false;
            dgvConversiones.AllowUserToDeleteRows = false;
            dgvConversiones.BackgroundColor = Color.White;
            dgvConversiones.BorderStyle = BorderStyle.None;
            dgvConversiones.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(30, 60, 114);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(30, 60, 114);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvConversiones.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvConversiones.ColumnHeadersHeight = 36;
            dgvConversiones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvConversiones.Dock = DockStyle.Fill;
            dgvConversiones.EnableHeadersVisualStyles = false;
            dgvConversiones.GridColor = Color.FromArgb(237, 235, 233);
            dgvConversiones.Location = new Point(8, 39);
            dgvConversiones.MultiSelect = false;
            dgvConversiones.Name = "dgvConversiones";
            dgvConversiones.ReadOnly = true;
            dgvConversiones.RowHeadersVisible = false;
            dgvConversiones.RowTemplate.Height = 36;
            dgvConversiones.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvConversiones.Size = new Size(1002, 278);
            dgvConversiones.TabIndex = 2;
            // 
            // tsConversiones
            // 
            tsConversiones.GripStyle = ToolStripGripStyle.Hidden;
            tsConversiones.Items.AddRange(new ToolStripItem[] { tsbNuevo, tsbEditar, tsbEliminar, tsSep1, tslInfo });
            tsConversiones.Location = new Point(8, 8);
            tsConversiones.Name = "tsConversiones";
            tsConversiones.Padding = new Padding(4);
            tsConversiones.Size = new Size(1002, 31);
            tsConversiones.TabIndex = 1;
            // 
            // tsbNuevo
            // 
            tsbNuevo.Image = Properties.Resources.add;
            tsbNuevo.ImageTransparentColor = Color.Magenta;
            tsbNuevo.Name = "tsbNuevo";
            tsbNuevo.Size = new Size(62, 20);
            tsbNuevo.Text = "Nuevo";
            tsbNuevo.ToolTipText = "Crear nueva conversión";
            // 
            // tsbEditar
            // 
            tsbEditar.Enabled = false;
            tsbEditar.Image = Properties.Resources.edit;
            tsbEditar.ImageTransparentColor = Color.Magenta;
            tsbEditar.Name = "tsbEditar";
            tsbEditar.Size = new Size(57, 20);
            tsbEditar.Text = "Editar";
            tsbEditar.ToolTipText = "Editar conversión seleccionada";
            // 
            // tsbEliminar
            // 
            tsbEliminar.Enabled = false;
            tsbEliminar.Image = Properties.Resources.delete;
            tsbEliminar.ImageTransparentColor = Color.Magenta;
            tsbEliminar.Name = "tsbEliminar";
            tsbEliminar.Size = new Size(70, 20);
            tsbEliminar.Text = "Eliminar";
            tsbEliminar.ToolTipText = "Eliminar conversión seleccionada";
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
            tslInfo.Size = new Size(134, 20);
            tslInfo.Text = "0 conversiones cargadas";
            tslInfo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // splitter
            // 
            splitter.BackColor = Color.FromArgb(200, 200, 200);
            splitter.Location = new Point(3, 384);
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
            pnlEdicion.Controls.Add(tlpCampos);
            pnlEdicion.Controls.Add(pnlEdicionHeader);
            pnlEdicion.Dock = DockStyle.Fill;
            pnlEdicion.Location = new Point(3, 390);
            pnlEdicion.Name = "pnlEdicion";
            pnlEdicion.Padding = new Padding(12);
            pnlEdicion.Size = new Size(1018, 215);
            pnlEdicion.TabIndex = 2;
            pnlEdicion.Visible = false;
            // 
            // pnlAcciones
            // 
            pnlAcciones.Controls.Add(btnCancelar);
            pnlAcciones.Controls.Add(btnGuardar);
            pnlAcciones.Dock = DockStyle.Bottom;
            pnlAcciones.Location = new Point(12, 161);
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
            // tlpCampos
            // 
            tlpCampos.ColumnCount = 4;
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpCampos.Controls.Add(lblUnidad, 0, 0);
            tlpCampos.Controls.Add(cboUnidad, 1, 0);
            tlpCampos.Controls.Add(lblFactor, 2, 0);
            tlpCampos.Controls.Add(txtFactor, 3, 0);
            tlpCampos.Dock = DockStyle.Top;
            tlpCampos.Location = new Point(12, 44);
            tlpCampos.Name = "tlpCampos";
            tlpCampos.Padding = new Padding(0, 8, 0, 8);
            tlpCampos.RowCount = 1;
            tlpCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpCampos.Size = new Size(992, 52);
            tlpCampos.TabIndex = 1;
            // 
            // lblUnidad
            // 
            lblUnidad.Dock = DockStyle.Fill;
            lblUnidad.Font = new Font("Segoe UI", 9F);
            lblUnidad.ForeColor = Color.FromArgb(96, 94, 92);
            lblUnidad.Location = new Point(3, 8);
            lblUnidad.Name = "lblUnidad";
            lblUnidad.Size = new Size(84, 36);
            lblUnidad.TabIndex = 0;
            lblUnidad.Text = "Unidad:";
            lblUnidad.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboUnidad
            // 
            cboUnidad.Dock = DockStyle.Fill;
            cboUnidad.DropDownStyle = ComboBoxStyle.DropDownList;
            cboUnidad.Font = new Font("Segoe UI", 9F);
            cboUnidad.FormattingEnabled = true;
            cboUnidad.Location = new Point(93, 11);
            cboUnidad.Name = "cboUnidad";
            cboUnidad.Size = new Size(400, 23);
            cboUnidad.TabIndex = 1;
            // 
            // lblFactor
            // 
            lblFactor.Dock = DockStyle.Fill;
            lblFactor.Font = new Font("Segoe UI", 9F);
            lblFactor.ForeColor = Color.FromArgb(96, 94, 92);
            lblFactor.Location = new Point(499, 8);
            lblFactor.Name = "lblFactor";
            lblFactor.Size = new Size(84, 36);
            lblFactor.TabIndex = 2;
            lblFactor.Text = "Factor:";
            lblFactor.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtFactor
            // 
            txtFactor.Dock = DockStyle.Fill;
            txtFactor.Font = new Font("Segoe UI", 9F);
            txtFactor.Location = new Point(589, 11);
            txtFactor.Name = "txtFactor";
            txtFactor.Size = new Size(400, 23);
            txtFactor.TabIndex = 3;
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
            btnCerrarEdicion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrarEdicion.BackColor = Color.Transparent;
            btnCerrarEdicion.FlatAppearance.BorderSize = 0;
            btnCerrarEdicion.FlatStyle = FlatStyle.Flat;
            btnCerrarEdicion.Font = new Font("Segoe UI", 10F);
            btnCerrarEdicion.Location = new Point(965, 4);
            btnCerrarEdicion.Name = "btnCerrarEdicion";
            btnCerrarEdicion.Size = new Size(24, 24);
            btnCerrarEdicion.TabIndex = 1;
            btnCerrarEdicion.Text = "✕";
            btnCerrarEdicion.UseVisualStyleBackColor = false;
            // 
            // lblModoEdicion
            // 
            lblModoEdicion.Dock = DockStyle.Left;
            lblModoEdicion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblModoEdicion.ForeColor = Color.FromArgb(50, 49, 48);
            lblModoEdicion.Location = new Point(0, 0);
            lblModoEdicion.Name = "lblModoEdicion";
            lblModoEdicion.Size = new Size(200, 32);
            lblModoEdicion.TabIndex = 0;
            lblModoEdicion.Text = "🆕 Nueva Conversión";
            lblModoEdicion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ProdConvUC
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tlpMain);
            Name = "ProdConvUC";
            Size = new Size(1024, 608);
            tlpMain.ResumeLayout(false);
             pnlSuperior.ResumeLayout(false);
            pnlSuperior.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvConversiones).EndInit();
            tsConversiones.ResumeLayout(false);
            tsConversiones.PerformLayout();
            pnlEdicion.ResumeLayout(false);
            pnlAcciones.ResumeLayout(false);
            tlpCampos.ResumeLayout(false);
            tlpCampos.PerformLayout();
            pnlEdicionHeader.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
