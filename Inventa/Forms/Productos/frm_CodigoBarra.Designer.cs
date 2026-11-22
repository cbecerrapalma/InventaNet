namespace Inventa.Forms.Productos
{
    partial class frm_CodigoBarra
    {
        private System.ComponentModel.IContainer components = null;

        // ── ToolStrip ─────────────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip tsCodigos;
        private System.Windows.Forms.ToolStripButton btnNuevo;
        private System.Windows.Forms.ToolStripButton btnEditar;
        private System.Windows.Forms.ToolStripButton btnEliminar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton btnRefrescar;
        private System.Windows.Forms.ToolStripLabel tsLblContador;

        // ── Grilla ────────────────────────────────────────────────────────────
        private System.Windows.Forms.DataGridView dgvCodigos;

        // ── Panel título ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelTitulo;
        private System.Windows.Forms.Label lblTitulo;

        // ── Panel edición ─────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelEdicion;
        private System.Windows.Forms.TableLayoutPanel tlpCampos;

        // Fila 0: Código | TipoCódigo
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblTipoCodigo;
        private System.Windows.Forms.ComboBox cboTipoCodigo;

        // Fila 1: Principal | ID
        private System.Windows.Forms.Label lblPrincipal;
        private System.Windows.Forms.CheckBox chkPrincipal;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.Label lblIdValor;

        // Fila 2: Presentación (modo modal: label+valor fijo | modo standalone: label+combo)
        private System.Windows.Forms.Label lblPresentacionFija;
        private System.Windows.Forms.Label lblPresProdValor;
        private System.Windows.Forms.Label lblPresentacionSelector;
        private System.Windows.Forms.ComboBox cboPresentacion;

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
            tsCodigos = new ToolStrip();
            btnNuevo = new ToolStripButton();
            btnEditar = new ToolStripButton();
            btnEliminar = new ToolStripButton();
            tsSep1 = new ToolStripSeparator();
            btnRefrescar = new ToolStripButton();
            tsLblContador = new ToolStripLabel();
            dgvCodigos = new DataGridView();
            panelTitulo = new Panel();
            lblTitulo = new Label();
            panelEdicion = new Panel();
            tlpCampos = new TableLayoutPanel();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblTipoCodigo = new Label();
            cboTipoCodigo = new ComboBox();
            lblPrincipal = new Label();
            chkPrincipal = new CheckBox();
            lblId = new Label();
            lblIdValor = new Label();
            lblPresentacionFija = new Label();
            lblPresProdValor = new Label();
            lblPresentacionSelector = new Label();
            cboPresentacion = new ComboBox();
            panelBotones = new Panel();
            tlpBotones = new TableLayoutPanel();
            flpBotonesDer = new FlowLayoutPanel();
            btnGuardar = new Button();
            btnCancelar = new Button();
            tsCodigos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCodigos).BeginInit();
            panelTitulo.SuspendLayout();
            panelEdicion.SuspendLayout();
            tlpCampos.SuspendLayout();
            panelBotones.SuspendLayout();
            tlpBotones.SuspendLayout();
            flpBotonesDer.SuspendLayout();
            SuspendLayout();
            // 
            // tsCodigos
            // 
            tsCodigos.GripStyle = ToolStripGripStyle.Hidden;
            tsCodigos.Items.AddRange(new ToolStripItem[] { btnNuevo, btnEditar, btnEliminar, tsSep1, btnRefrescar, tsLblContador });
            tsCodigos.Location = new Point(0, 44);
            tsCodigos.Name = "tsCodigos";
            tsCodigos.RenderMode = ToolStripRenderMode.System;
            tsCodigos.Size = new Size(884, 25);
            tsCodigos.TabIndex = 1;
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
            tsLblContador.Size = new Size(0, 22);
            // 
            // dgvCodigos
            // 
            dgvCodigos.AllowUserToAddRows = false;
            dgvCodigos.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(250, 249, 248);
            dgvCodigos.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvCodigos.BackgroundColor = Color.White;
            dgvCodigos.BorderStyle = BorderStyle.None;
            dgvCodigos.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(243, 242, 241);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvCodigos.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvCodigos.ColumnHeadersHeight = 36;
            dgvCodigos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(96, 94, 92);
            dataGridViewCellStyle3.Padding = new Padding(8, 2, 8, 2);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(229, 243, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvCodigos.DefaultCellStyle = dataGridViewCellStyle3;
            dgvCodigos.Dock = DockStyle.Fill;
            dgvCodigos.EnableHeadersVisualStyles = false;
            dgvCodigos.GridColor = Color.FromArgb(237, 235, 233);
            dgvCodigos.Location = new Point(0, 69);
            dgvCodigos.MultiSelect = false;
            dgvCodigos.Name = "dgvCodigos";
            dgvCodigos.ReadOnly = true;
            dgvCodigos.RowHeadersVisible = false;
            dgvCodigos.RowTemplate.Height = 32;
            dgvCodigos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCodigos.Size = new Size(884, 297);
            dgvCodigos.TabIndex = 2;
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
            lblTitulo.Text = "GESTION DE CODIGOS DE BARRA";
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
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpCampos.Controls.Add(lblCodigo, 0, 0);
            tlpCampos.Controls.Add(txtCodigo, 1, 0);
            tlpCampos.Controls.Add(lblTipoCodigo, 2, 0);
            tlpCampos.Controls.Add(cboTipoCodigo, 3, 0);
            tlpCampos.Controls.Add(lblPrincipal, 0, 1);
            tlpCampos.Controls.Add(chkPrincipal, 1, 1);
            tlpCampos.Controls.Add(lblId, 2, 1);
            tlpCampos.Controls.Add(lblIdValor, 3, 1);
            tlpCampos.Controls.Add(lblPresentacionFija, 0, 2);
            tlpCampos.Controls.Add(lblPresProdValor, 1, 2);
            tlpCampos.Controls.Add(lblPresentacionSelector, 2, 2);
            tlpCampos.Controls.Add(cboPresentacion, 3, 2);
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
            lblCodigo.Size = new Size(114, 36);
            lblCodigo.TabIndex = 0;
            lblCodigo.Text = "Codigo:";
            lblCodigo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtCodigo
            // 
            txtCodigo.CharacterCasing = CharacterCasing.Upper;
            txtCodigo.Dock = DockStyle.Fill;
            txtCodigo.Font = new Font("Segoe UI", 9F);
            txtCodigo.Location = new Point(127, 7);
            txtCodigo.MaxLength = 50;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.Size = new Size(241, 23);
            txtCodigo.TabIndex = 1;
            // 
            // lblTipoCodigo
            // 
            lblTipoCodigo.Dock = DockStyle.Fill;
            lblTipoCodigo.Font = new Font("Segoe UI", 9F);
            lblTipoCodigo.ForeColor = Color.FromArgb(96, 94, 92);
            lblTipoCodigo.Location = new Point(374, 4);
            lblTipoCodigo.Name = "lblTipoCodigo";
            lblTipoCodigo.Size = new Size(114, 36);
            lblTipoCodigo.TabIndex = 2;
            lblTipoCodigo.Text = "Tipo:";
            lblTipoCodigo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboTipoCodigo
            // 
            cboTipoCodigo.Dock = DockStyle.Fill;
            cboTipoCodigo.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipoCodigo.Font = new Font("Segoe UI", 9F);
            cboTipoCodigo.FormattingEnabled = true;
            cboTipoCodigo.Location = new Point(494, 7);
            cboTipoCodigo.Name = "cboTipoCodigo";
            cboTipoCodigo.Size = new Size(365, 23);
            cboTipoCodigo.TabIndex = 2;
            // 
            // lblPrincipal
            // 
            lblPrincipal.Dock = DockStyle.Fill;
            lblPrincipal.Font = new Font("Segoe UI", 9F);
            lblPrincipal.ForeColor = Color.FromArgb(96, 94, 92);
            lblPrincipal.Location = new Point(7, 40);
            lblPrincipal.Name = "lblPrincipal";
            lblPrincipal.Size = new Size(114, 36);
            lblPrincipal.TabIndex = 3;
            lblPrincipal.Text = "Principal:";
            lblPrincipal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // chkPrincipal
            // 
            chkPrincipal.AutoSize = true;
            chkPrincipal.Dock = DockStyle.Fill;
            chkPrincipal.Font = new Font("Segoe UI", 9F);
            chkPrincipal.Location = new Point(127, 43);
            chkPrincipal.Name = "chkPrincipal";
            chkPrincipal.Size = new Size(241, 30);
            chkPrincipal.TabIndex = 3;
            // 
            // lblId
            // 
            lblId.Dock = DockStyle.Fill;
            lblId.Font = new Font("Segoe UI", 9F);
            lblId.ForeColor = Color.FromArgb(96, 94, 92);
            lblId.Location = new Point(374, 40);
            lblId.Name = "lblId";
            lblId.Size = new Size(114, 36);
            lblId.TabIndex = 4;
            lblId.Text = "ID:";
            lblId.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblIdValor
            // 
            lblIdValor.Dock = DockStyle.Fill;
            lblIdValor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblIdValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblIdValor.Location = new Point(494, 40);
            lblIdValor.Name = "lblIdValor";
            lblIdValor.Size = new Size(365, 36);
            lblIdValor.TabIndex = 5;
            lblIdValor.Text = "<< Nuevo >>";
            lblIdValor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPresentacionFija
            // 
            lblPresentacionFija.Dock = DockStyle.Fill;
            lblPresentacionFija.Font = new Font("Segoe UI", 9F);
            lblPresentacionFija.ForeColor = Color.FromArgb(96, 94, 92);
            lblPresentacionFija.Location = new Point(7, 76);
            lblPresentacionFija.Name = "lblPresentacionFija";
            lblPresentacionFija.Size = new Size(114, 36);
            lblPresentacionFija.TabIndex = 6;
            lblPresentacionFija.Text = "Presentacion:";
            lblPresentacionFija.TextAlign = ContentAlignment.MiddleRight;
            lblPresentacionFija.Visible = false;
            // 
            // lblPresProdValor
            // 
            lblPresProdValor.Dock = DockStyle.Fill;
            lblPresProdValor.Font = new Font("Segoe UI", 9F);
            lblPresProdValor.ForeColor = Color.FromArgb(50, 49, 48);
            lblPresProdValor.Location = new Point(127, 76);
            lblPresProdValor.Name = "lblPresProdValor";
            lblPresProdValor.Size = new Size(241, 36);
            lblPresProdValor.TabIndex = 7;
            lblPresProdValor.Text = "-";
            lblPresProdValor.TextAlign = ContentAlignment.MiddleLeft;
            lblPresProdValor.Visible = false;
            // 
            // lblPresentacionSelector
            // 
            lblPresentacionSelector.Dock = DockStyle.Fill;
            lblPresentacionSelector.Font = new Font("Segoe UI", 9F);
            lblPresentacionSelector.ForeColor = Color.FromArgb(96, 94, 92);
            lblPresentacionSelector.Location = new Point(374, 76);
            lblPresentacionSelector.Name = "lblPresentacionSelector";
            lblPresentacionSelector.Size = new Size(114, 36);
            lblPresentacionSelector.TabIndex = 8;
            lblPresentacionSelector.Text = "Presentacion:";
            lblPresentacionSelector.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cboPresentacion
            // 
            cboPresentacion.Dock = DockStyle.Fill;
            cboPresentacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPresentacion.Font = new Font("Segoe UI", 9F);
            cboPresentacion.FormattingEnabled = true;
            cboPresentacion.Location = new Point(494, 79);
            cboPresentacion.Name = "cboPresentacion";
            cboPresentacion.Size = new Size(365, 23);
            cboPresentacion.TabIndex = 4;
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
            // frm_CodigoBarra
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 560);
            Controls.Add(dgvCodigos);
            Controls.Add(tsCodigos);
            Controls.Add(panelEdicion);
            Controls.Add(panelBotones);
            Controls.Add(panelTitulo);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(640, 480);
            Name = "frm_CodigoBarra";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Codigos de Barra";
            Load += Frm_CodigoBarra_Load;
            tsCodigos.ResumeLayout(false);
            tsCodigos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCodigos).EndInit();
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