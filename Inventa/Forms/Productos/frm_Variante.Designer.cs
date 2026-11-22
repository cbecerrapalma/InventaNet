namespace Inventa.Forms.Productos
{
    partial class frm_Variante
    {
        private System.ComponentModel.IContainer components = null;

        // ── Panel header ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblProductoLabel;
        private System.Windows.Forms.Label lblProductoValor;

        // ── Layout principal ──────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpMain;

        // ── Panel datos de la variante ────────────────────────────────────────
        private System.Windows.Forms.GroupBox gpbDatosVariante;
        private System.Windows.Forms.TableLayoutPanel tlpDatos;
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblCostoProm;
        private System.Windows.Forms.NumericUpDown numCostoProm;
        private System.Windows.Forms.Label lblActivo;
        private System.Windows.Forms.CheckBox chkActivo;

        // ── Panel atributos ───────────────────────────────────────────────────
        private System.Windows.Forms.GroupBox gpbAtributos;
        private System.Windows.Forms.ToolStrip tsAtributos;
        private System.Windows.Forms.ToolStripButton btnAgregarAtributo;
        private System.Windows.Forms.ToolStripButton btnEditarAtributo;
        private System.Windows.Forms.ToolStripButton btnEliminarAtributo;
        private System.Windows.Forms.ToolStripSeparator tsSepAtributos;
        private System.Windows.Forms.ToolStripButton btnRefrescarAtributos;
        private System.Windows.Forms.ToolStripLabel tsLblAtributos;
        private System.Windows.Forms.DataGridView dgvAtributos;

        // ── Panel botones ─────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.FlowLayoutPanel flpBotones;
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
            lblProductoValor = new Label();
            lblProductoLabel = new Label();
            lblTitulo = new Label();
            tlpMain = new TableLayoutPanel();
            gpbDatosVariante = new GroupBox();
            tlpDatos = new TableLayoutPanel();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblNombre = new Label();
            txtNombre = new TextBox();
            lblCostoProm = new Label();
            numCostoProm = new NumericUpDown();
            lblActivo = new Label();
            chkActivo = new CheckBox();
            gpbAtributos = new GroupBox();
            dgvAtributos = new DataGridView();
            tsAtributos = new ToolStrip();
            btnAgregarAtributo = new ToolStripButton();
            btnEditarAtributo = new ToolStripButton();
            btnEliminarAtributo = new ToolStripButton();
            tsSepAtributos = new ToolStripSeparator();
            btnRefrescarAtributos = new ToolStripButton();
            tsLblAtributos = new ToolStripLabel();
            panelBotones = new Panel();
            flpBotones = new FlowLayoutPanel();
            btnCerrar = new Button();
            btnGuardar = new Button();
            btnCancelar = new Button();
            panelHeader.SuspendLayout();
            tlpMain.SuspendLayout();
            gpbDatosVariante.SuspendLayout();
            tlpDatos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCostoProm).BeginInit();
            gpbAtributos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAtributos).BeginInit();
            tsAtributos.SuspendLayout();
            panelBotones.SuspendLayout();
            flpBotones.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(30, 60, 114);
            panelHeader.Controls.Add(lblProductoValor);
            panelHeader.Controls.Add(lblProductoLabel);
            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new Padding(16, 0, 16, 0);
            panelHeader.Size = new Size(700, 60);
            panelHeader.TabIndex = 0;
            // 
            // lblProductoValor
            // 
            lblProductoValor.Dock = DockStyle.Top;
            lblProductoValor.Font = new Font("Segoe UI", 8.5F);
            lblProductoValor.ForeColor = Color.FromArgb(200, 220, 240);
            lblProductoValor.Location = new Point(16, 44);
            lblProductoValor.Name = "lblProductoValor";
            lblProductoValor.Size = new Size(668, 16);
            lblProductoValor.TabIndex = 0;
            lblProductoValor.Text = "-";
            lblProductoValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblProductoLabel
            // 
            lblProductoLabel.Dock = DockStyle.Top;
            lblProductoLabel.Font = new Font("Segoe UI", 7.5F);
            lblProductoLabel.ForeColor = Color.FromArgb(180, 200, 230);
            lblProductoLabel.Location = new Point(16, 30);
            lblProductoLabel.Name = "lblProductoLabel";
            lblProductoLabel.Size = new Size(668, 14);
            lblProductoLabel.TabIndex = 1;
            lblProductoLabel.Text = "PRODUCTO";
            lblProductoLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTitulo
            // 
            lblTitulo.Dock = DockStyle.Top;
            lblTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(16, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(668, 30);
            lblTitulo.TabIndex = 2;
            lblTitulo.Text = "Variante de Producto";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(gpbDatosVariante, 0, 0);
            tlpMain.Controls.Add(gpbAtributos, 0, 1);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 60);
            tlpMain.Name = "tlpMain";
            tlpMain.Padding = new Padding(10, 8, 10, 4);
            tlpMain.RowCount = 2;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 165F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.Size = new Size(700, 426);
            tlpMain.TabIndex = 1;
            // 
            // gpbDatosVariante
            // 
            gpbDatosVariante.Controls.Add(tlpDatos);
            gpbDatosVariante.Dock = DockStyle.Fill;
            gpbDatosVariante.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gpbDatosVariante.ForeColor = Color.FromArgb(50, 49, 48);
            gpbDatosVariante.Location = new Point(13, 11);
            gpbDatosVariante.Name = "gpbDatosVariante";
            gpbDatosVariante.Size = new Size(674, 159);
            gpbDatosVariante.TabIndex = 0;
            gpbDatosVariante.TabStop = false;
            gpbDatosVariante.Text = "Datos de la Variante";
            // 
            // tlpDatos
            // 
            tlpDatos.ColumnCount = 2;
            tlpDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tlpDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpDatos.Controls.Add(lblCodigo, 0, 0);
            tlpDatos.Controls.Add(txtCodigo, 1, 0);
            tlpDatos.Controls.Add(lblNombre, 0, 1);
            tlpDatos.Controls.Add(txtNombre, 1, 1);
            tlpDatos.Controls.Add(lblCostoProm, 0, 2);
            tlpDatos.Controls.Add(numCostoProm, 1, 2);
            tlpDatos.Controls.Add(lblActivo, 0, 3);
            tlpDatos.Controls.Add(chkActivo, 1, 3);
            tlpDatos.Dock = DockStyle.Fill;
            tlpDatos.Location = new Point(3, 19);
            tlpDatos.Name = "tlpDatos";
            tlpDatos.Padding = new Padding(6, 4, 6, 4);
            tlpDatos.RowCount = 4;
            tlpDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpDatos.Size = new Size(668, 137);
            tlpDatos.TabIndex = 0;
            // 
            // lblCodigo
            // 
            lblCodigo.Dock = DockStyle.Fill;
            lblCodigo.Font = new Font("Segoe UI", 9F);
            lblCodigo.ForeColor = Color.FromArgb(96, 94, 92);
            lblCodigo.Location = new Point(9, 4);
            lblCodigo.Name = "lblCodigo";
            lblCodigo.Size = new Size(124, 32);
            lblCodigo.TabIndex = 0;
            lblCodigo.Text = "Codigo:";
            lblCodigo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtCodigo
            // 
            txtCodigo.CharacterCasing = CharacterCasing.Upper;
            txtCodigo.Dock = DockStyle.Fill;
            txtCodigo.Font = new Font("Segoe UI", 9F);
            txtCodigo.Location = new Point(139, 7);
            txtCodigo.MaxLength = 50;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.Size = new Size(520, 23);
            txtCodigo.TabIndex = 1;
            txtCodigo.Leave += TxtCodigo_Leave;
            // 
            // lblNombre
            // 
            lblNombre.Dock = DockStyle.Fill;
            lblNombre.Font = new Font("Segoe UI", 9F);
            lblNombre.ForeColor = Color.FromArgb(96, 94, 92);
            lblNombre.Location = new Point(9, 36);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(124, 32);
            lblNombre.TabIndex = 2;
            lblNombre.Text = "Nombre:";
            lblNombre.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtNombre
            // 
            txtNombre.Dock = DockStyle.Fill;
            txtNombre.Font = new Font("Segoe UI", 9F);
            txtNombre.Location = new Point(139, 39);
            txtNombre.MaxLength = 100;
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(520, 23);
            txtNombre.TabIndex = 2;
            // 
            // lblCostoProm
            // 
            lblCostoProm.Dock = DockStyle.Fill;
            lblCostoProm.Font = new Font("Segoe UI", 9F);
            lblCostoProm.ForeColor = Color.FromArgb(96, 94, 92);
            lblCostoProm.Location = new Point(9, 68);
            lblCostoProm.Name = "lblCostoProm";
            lblCostoProm.Size = new Size(124, 32);
            lblCostoProm.TabIndex = 3;
            lblCostoProm.Text = "Costo Promedio:";
            lblCostoProm.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numCostoProm
            // 
            numCostoProm.DecimalPlaces = 4;
            numCostoProm.Dock = DockStyle.Fill;
            numCostoProm.Font = new Font("Segoe UI", 9F);
            numCostoProm.Location = new Point(139, 71);
            numCostoProm.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numCostoProm.Name = "numCostoProm";
            numCostoProm.Size = new Size(520, 23);
            numCostoProm.TabIndex = 3;
            numCostoProm.ThousandsSeparator = true;
            // 
            // lblActivo
            // 
            lblActivo.Dock = DockStyle.Fill;
            lblActivo.Font = new Font("Segoe UI", 9F);
            lblActivo.ForeColor = Color.FromArgb(96, 94, 92);
            lblActivo.Location = new Point(9, 100);
            lblActivo.Name = "lblActivo";
            lblActivo.Size = new Size(124, 33);
            lblActivo.TabIndex = 4;
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
            chkActivo.Location = new Point(139, 103);
            chkActivo.Name = "chkActivo";
            chkActivo.Size = new Size(520, 27);
            chkActivo.TabIndex = 4;
            // 
            // gpbAtributos
            // 
            gpbAtributos.Controls.Add(dgvAtributos);
            gpbAtributos.Controls.Add(tsAtributos);
            gpbAtributos.Dock = DockStyle.Fill;
            gpbAtributos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gpbAtributos.ForeColor = Color.FromArgb(50, 49, 48);
            gpbAtributos.Location = new Point(13, 176);
            gpbAtributos.Name = "gpbAtributos";
            gpbAtributos.Size = new Size(674, 243);
            gpbAtributos.TabIndex = 1;
            gpbAtributos.TabStop = false;
            gpbAtributos.Text = "Atributos de la Variante";
            // 
            // dgvAtributos
            // 
            dgvAtributos.AllowUserToAddRows = false;
            dgvAtributos.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(250, 249, 248);
            dgvAtributos.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvAtributos.BackgroundColor = Color.White;
            dgvAtributos.BorderStyle = BorderStyle.None;
            dgvAtributos.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(243, 242, 241);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle2.Padding = new Padding(8, 0, 0, 0);
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvAtributos.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvAtributos.ColumnHeadersHeight = 36;
            dgvAtributos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle3.Padding = new Padding(8, 2, 8, 2);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(229, 243, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvAtributos.DefaultCellStyle = dataGridViewCellStyle3;
            dgvAtributos.Dock = DockStyle.Fill;
            dgvAtributos.EnableHeadersVisualStyles = false;
            dgvAtributos.GridColor = Color.FromArgb(237, 235, 233);
            dgvAtributos.Location = new Point(3, 44);
            dgvAtributos.MultiSelect = false;
            dgvAtributos.Name = "dgvAtributos";
            dgvAtributos.ReadOnly = true;
            dgvAtributos.RowHeadersVisible = false;
            dgvAtributos.RowTemplate.Height = 32;
            dgvAtributos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAtributos.Size = new Size(668, 196);
            dgvAtributos.TabIndex = 0;
            // 
            // tsAtributos
            // 
            tsAtributos.GripStyle = ToolStripGripStyle.Hidden;
            tsAtributos.Items.AddRange(new ToolStripItem[] { btnAgregarAtributo, btnEditarAtributo, btnEliminarAtributo, tsSepAtributos, btnRefrescarAtributos, tsLblAtributos });
            tsAtributos.Location = new Point(3, 19);
            tsAtributos.Name = "tsAtributos";
            tsAtributos.RenderMode = ToolStripRenderMode.System;
            tsAtributos.Size = new Size(668, 25);
            tsAtributos.TabIndex = 1;
            // 
            // btnAgregarAtributo
            // 
            btnAgregarAtributo.Enabled = false;
            btnAgregarAtributo.Image = Properties.Resources.add;
            btnAgregarAtributo.ImageTransparentColor = Color.Magenta;
            btnAgregarAtributo.Name = "btnAgregarAtributo";
            btnAgregarAtributo.Size = new Size(127, 22);
            btnAgregarAtributo.Text = "Gestionar atributos";
            btnAgregarAtributo.ToolTipText = "Abrir el editor de atributos de la variante";
            btnAgregarAtributo.Click += BtnAgregarAtributo_Click;
            // 
            // btnEditarAtributo
            // 
            btnEditarAtributo.Enabled = false;
            btnEditarAtributo.Image = Properties.Resources.edit;
            btnEditarAtributo.ImageTransparentColor = Color.Magenta;
            btnEditarAtributo.Name = "btnEditarAtributo";
            btnEditarAtributo.Size = new Size(107, 22);
            btnEditarAtributo.Text = "Editar atributos";
            btnEditarAtributo.Click += BtnEditarAtributo_Click;
            // 
            // btnEliminarAtributo
            // 
            btnEliminarAtributo.Enabled = false;
            btnEliminarAtributo.Image = Properties.Resources.delete;
            btnEliminarAtributo.ImageTransparentColor = Color.Magenta;
            btnEliminarAtributo.Name = "btnEliminarAtributo";
            btnEliminarAtributo.Size = new Size(60, 22);
            btnEliminarAtributo.Text = "Quitar";
            btnEliminarAtributo.ToolTipText = "Quitar la asignacion del valor seleccionado";
            btnEliminarAtributo.Click += BtnEliminarAtributo_Click;
            // 
            // tsSepAtributos
            // 
            tsSepAtributos.Name = "tsSepAtributos";
            tsSepAtributos.Size = new Size(6, 25);
            // 
            // btnRefrescarAtributos
            // 
            btnRefrescarAtributos.Image = Properties.Resources.refresh;
            btnRefrescarAtributos.ImageTransparentColor = Color.Magenta;
            btnRefrescarAtributos.Name = "btnRefrescarAtributos";
            btnRefrescarAtributos.Size = new Size(75, 22);
            btnRefrescarAtributos.Text = "Refrescar";
            btnRefrescarAtributos.Click += BtnRefrescarAtributos_Click;
            // 
            // tsLblAtributos
            // 
            tsLblAtributos.Alignment = ToolStripItemAlignment.Right;
            tsLblAtributos.Font = new Font("Segoe UI", 8F);
            tsLblAtributos.ForeColor = Color.FromArgb(96, 94, 92);
            tsLblAtributos.Name = "tsLblAtributos";
            tsLblAtributos.Size = new Size(129, 22);
            tsLblAtributos.Text = "Sin atributos asignados";
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.FromArgb(243, 242, 241);
            panelBotones.Controls.Add(flpBotones);
            panelBotones.Dock = DockStyle.Bottom;
            panelBotones.Location = new Point(0, 486);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(700, 54);
            panelBotones.TabIndex = 2;
            // 
            // flpBotones
            // 
            flpBotones.Controls.Add(btnCerrar);
            flpBotones.Controls.Add(btnGuardar);
            flpBotones.Controls.Add(btnCancelar);
            flpBotones.Dock = DockStyle.Fill;
            flpBotones.FlowDirection = FlowDirection.RightToLeft;
            flpBotones.Location = new Point(0, 0);
            flpBotones.Name = "flpBotones";
            flpBotones.Padding = new Padding(8);
            flpBotones.Size = new Size(700, 54);
            flpBotones.TabIndex = 0;
            flpBotones.WrapContents = false;
            // 
            // btnCerrar
            // 
            btnCerrar.BackColor = Color.FromArgb(30, 60, 114);
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCerrar.ForeColor = Color.White;
            btnCerrar.Location = new Point(554, 8);
            btnCerrar.Margin = new Padding(6, 0, 0, 0);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(130, 36);
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
            btnGuardar.Location = new Point(438, 8);
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
            btnCancelar.Location = new Point(322, 8);
            btnCancelar.Margin = new Padding(6, 0, 0, 0);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += BtnCancelar_Click;
            // 
            // frm_Variante
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(700, 540);
            Controls.Add(tlpMain);
            Controls.Add(panelBotones);
            Controls.Add(panelHeader);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(640, 500);
            Name = "frm_Variante";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Variante de Producto";
            Load += Frm_Variante_Load;
            panelHeader.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            gpbDatosVariante.ResumeLayout(false);
            tlpDatos.ResumeLayout(false);
            tlpDatos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCostoProm).EndInit();
            gpbAtributos.ResumeLayout(false);
            gpbAtributos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAtributos).EndInit();
            tsAtributos.ResumeLayout(false);
            tsAtributos.PerformLayout();
            panelBotones.ResumeLayout(false);
            flpBotones.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}