namespace Inventa.Forms.Mantenedores
{
    partial class frm_MetodoPago
    {
        private System.ComponentModel.IContainer components = null;

        // ── Panel header ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelTitulo;
        private System.Windows.Forms.Label lblTitulo;

        // ── ToolStrip ─────────────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip tsMetodosPago;
        private System.Windows.Forms.ToolStripButton btnNuevo;
        private System.Windows.Forms.ToolStripButton btnEditar;
        private System.Windows.Forms.ToolStripButton btnEliminar;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton btnRefrescar;
        private System.Windows.Forms.ToolStripLabel tsLblContador;

        // ── Grilla ────────────────────────────────────────────────────────────
        private System.Windows.Forms.DataGridView dgvMetodosPago;

        // ── Panel edición (Dock=Bottom) ───────────────────────────────────────
        private System.Windows.Forms.Panel panelEdicion;
        private System.Windows.Forms.TableLayoutPanel tlpMain;

        // Fila 0: Codigo | ID
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.Label lblIdValor;

        // Fila 1: Nombre | Activo
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblActivo;
        private System.Windows.Forms.CheckBox chkActivo;

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
            panelTitulo = new System.Windows.Forms.Panel();
            lblTitulo = new System.Windows.Forms.Label();
            tsMetodosPago = new System.Windows.Forms.ToolStrip();
            btnNuevo = new System.Windows.Forms.ToolStripButton();
            btnEditar = new System.Windows.Forms.ToolStripButton();
            btnEliminar = new System.Windows.Forms.ToolStripButton();
            tsSep1 = new System.Windows.Forms.ToolStripSeparator();
            btnRefrescar = new System.Windows.Forms.ToolStripButton();
            tsLblContador = new System.Windows.Forms.ToolStripLabel();
            dgvMetodosPago = new System.Windows.Forms.DataGridView();
            panelEdicion = new System.Windows.Forms.Panel();
            tlpMain = new System.Windows.Forms.TableLayoutPanel();
            lblCodigo = new System.Windows.Forms.Label();
            txtCodigo = new System.Windows.Forms.TextBox();
            lblId = new System.Windows.Forms.Label();
            lblIdValor = new System.Windows.Forms.Label();
            lblNombre = new System.Windows.Forms.Label();
            txtNombre = new System.Windows.Forms.TextBox();
            lblActivo = new System.Windows.Forms.Label();
            chkActivo = new System.Windows.Forms.CheckBox();
            panelBotones = new System.Windows.Forms.Panel();
            flpBotones = new System.Windows.Forms.FlowLayoutPanel();
            btnGuardar = new System.Windows.Forms.Button();
            btnCancelar = new System.Windows.Forms.Button();

            panelTitulo.SuspendLayout();
            tsMetodosPago.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMetodosPago).BeginInit();
            panelEdicion.SuspendLayout();
            tlpMain.SuspendLayout();
            panelBotones.SuspendLayout();
            flpBotones.SuspendLayout();
            SuspendLayout();

            // ── Panel header azul ─────────────────────────────────────────────
            panelTitulo.BackColor = System.Drawing.Color.FromArgb(30, 60, 114);
            panelTitulo.Controls.Add(lblTitulo);
            panelTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            panelTitulo.Name = "panelTitulo";
            panelTitulo.Size = new System.Drawing.Size(720, 44);
            panelTitulo.TabIndex = 0;

            lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            lblTitulo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblTitulo.ForeColor = System.Drawing.Color.White;
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Text = "MANTENEDOR DE METODOS DE PAGO";
            lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ── ToolStrip ─────────────────────────────────────────────────────
            tsMetodosPago.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            tsMetodosPago.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            tsMetodosPago.Dock = System.Windows.Forms.DockStyle.Top;
            tsMetodosPago.Location = new System.Drawing.Point(0, 44);
            tsMetodosPago.Name = "tsMetodosPago";
            tsMetodosPago.Size = new System.Drawing.Size(720, 27);
            tsMetodosPago.TabIndex = 1;
            tsMetodosPago.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                btnNuevo, btnEditar, btnEliminar, tsSep1, btnRefrescar, tsLblContador
            });

            btnNuevo.Image = global::Inventa.Properties.Resources.add;
            btnNuevo.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Text = "Nuevo";
            btnNuevo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnNuevo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            btnNuevo.ToolTipText = "Nuevo metodo de pago (Ins)";
            btnNuevo.Click += BtnNuevo_Click;

            btnEditar.Image = global::Inventa.Properties.Resources.edit;
            btnEditar.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnEditar.Name = "btnEditar";
            btnEditar.Text = "Editar";
            btnEditar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnEditar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            btnEditar.Enabled = false;
            btnEditar.Click += BtnEditar_Click;

            btnEliminar.Image = global::Inventa.Properties.Resources.delete;
            btnEliminar.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Text = "Eliminar";
            btnEliminar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnEliminar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            btnEliminar.Enabled = false;
            btnEliminar.Click += BtnEliminar_Click;

            tsSep1.Name = "tsSep1";

            btnRefrescar.Image = global::Inventa.Properties.Resources.refresh;
            btnRefrescar.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Text = "Refrescar (F5)";
            btnRefrescar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnRefrescar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            btnRefrescar.Click += BtnRefrescar_Click;

            tsLblContador.Name = "tsLblContador";
            tsLblContador.Text = "";
            tsLblContador.Font = new System.Drawing.Font("Segoe UI", 8F);
            tsLblContador.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            tsLblContador.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;

            // ── DataGridView ──────────────────────────────────────────────────
            dgvMetodosPago.AllowUserToAddRows = false;
            dgvMetodosPago.AllowUserToDeleteRows = false;
            dgvMetodosPago.AutoGenerateColumns = false;
            dgvMetodosPago.BackgroundColor = System.Drawing.Color.White;
            dgvMetodosPago.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dgvMetodosPago.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dgvMetodosPago.GridColor = System.Drawing.Color.FromArgb(237, 235, 233);
            dgvMetodosPago.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvMetodosPago.ColumnHeadersHeight = 36;
            dgvMetodosPago.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(243, 242, 241);
            dgvMetodosPago.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            dgvMetodosPago.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dgvMetodosPago.ColumnHeadersDefaultCellStyle.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            dgvMetodosPago.EnableHeadersVisualStyles = false;
            dgvMetodosPago.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            dgvMetodosPago.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            dgvMetodosPago.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            dgvMetodosPago.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(229, 243, 255);
            dgvMetodosPago.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            dgvMetodosPago.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            dgvMetodosPago.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(250, 249, 248);
            dgvMetodosPago.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvMetodosPago.MultiSelect = false;
            dgvMetodosPago.Name = "dgvMetodosPago";
            dgvMetodosPago.ReadOnly = true;
            dgvMetodosPago.RowHeadersVisible = false;
            dgvMetodosPago.RowTemplate.Height = 32;
            dgvMetodosPago.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvMetodosPago.TabIndex = 2;

            // ── Panel edición (Dock=Bottom) ───────────────────────────────────
            panelEdicion.BackColor = System.Drawing.Color.White;
            panelEdicion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelEdicion.Controls.Add(tlpMain);
            panelEdicion.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelEdicion.Name = "panelEdicion";
            panelEdicion.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            panelEdicion.Size = new System.Drawing.Size(720, 78);
            panelEdicion.TabIndex = 3;

            // tlpMain — 4 col × 2 filas
            tlpMain.ColumnCount = 4;
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            tlpMain.RowCount = 2;
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tlpMain.Controls.Add(lblCodigo, 0, 0);
            tlpMain.Controls.Add(txtCodigo, 1, 0);
            tlpMain.Controls.Add(lblId, 2, 0);
            tlpMain.Controls.Add(lblIdValor, 3, 0);
            tlpMain.Controls.Add(lblNombre, 0, 1);
            tlpMain.Controls.Add(txtNombre, 1, 1);
            tlpMain.Controls.Add(lblActivo, 2, 1);
            tlpMain.Controls.Add(chkActivo, 3, 1);
            tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpMain.Name = "tlpMain";
            tlpMain.TabIndex = 0;

            // Etiquetas
            lblCodigo.Dock = System.Windows.Forms.DockStyle.Fill;
            lblCodigo.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblCodigo.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblCodigo.Name = "lblCodigo"; lblCodigo.Text = "Codigo:";
            lblCodigo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblId.Dock = System.Windows.Forms.DockStyle.Fill;
            lblId.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblId.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblId.Name = "lblId"; lblId.Text = "ID:";
            lblId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblIdValor.Dock = System.Windows.Forms.DockStyle.Fill;
            lblIdValor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblIdValor.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            lblIdValor.Name = "lblIdValor"; lblIdValor.Text = "<< Nuevo >>";
            lblIdValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblNombre.Dock = System.Windows.Forms.DockStyle.Fill;
            lblNombre.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblNombre.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblNombre.Name = "lblNombre"; lblNombre.Text = "Nombre:";
            lblNombre.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            lblActivo.Dock = System.Windows.Forms.DockStyle.Fill;
            lblActivo.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblActivo.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            lblActivo.Name = "lblActivo"; lblActivo.Text = "Activo:";
            lblActivo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // Controles de entrada
            txtCodigo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            txtCodigo.Dock = System.Windows.Forms.DockStyle.Fill;
            txtCodigo.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtCodigo.MaxLength = 20;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.TabIndex = 1;

            txtNombre.Dock = System.Windows.Forms.DockStyle.Fill;
            txtNombre.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtNombre.MaxLength = 50;
            txtNombre.Name = "txtNombre";
            txtNombre.TabIndex = 2;

            chkActivo.AutoSize = true;
            chkActivo.Checked = true;
            chkActivo.CheckState = System.Windows.Forms.CheckState.Checked;
            chkActivo.Dock = System.Windows.Forms.DockStyle.Fill;
            chkActivo.Font = new System.Drawing.Font("Segoe UI", 9F);
            chkActivo.Name = "chkActivo";
            chkActivo.TabIndex = 3;

            // ── Panel botones ─────────────────────────────────────────────────
            panelBotones.BackColor = System.Drawing.Color.FromArgb(243, 242, 241);
            panelBotones.Controls.Add(flpBotones);
            panelBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new System.Drawing.Size(720, 54);
            panelBotones.TabIndex = 4;

            flpBotones.Controls.Add(btnGuardar);
            flpBotones.Controls.Add(btnCancelar);
            flpBotones.Dock = System.Windows.Forms.DockStyle.Fill;
            flpBotones.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flpBotones.Name = "flpBotones";
            flpBotones.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            flpBotones.WrapContents = false;

            btnGuardar.BackColor = System.Drawing.Color.FromArgb(30, 60, 114);
            btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnGuardar.ForeColor = System.Drawing.Color.White;
            btnGuardar.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new System.Drawing.Size(110, 36);
            btnGuardar.TabIndex = 0;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += BtnGuardar_Click;

            btnCancelar.BackColor = System.Drawing.Color.White;
            btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(138, 136, 134);
            btnCancelar.FlatAppearance.BorderSize = 1;
            btnCancelar.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnCancelar.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            btnCancelar.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new System.Drawing.Size(110, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += BtnCancelar_Click;

            // ── Formulario ────────────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(720, 510);
            Controls.Add(dgvMetodosPago);
            Controls.Add(tsMetodosPago);
            Controls.Add(panelEdicion);
            Controls.Add(panelBotones);
            Controls.Add(panelTitulo);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            MinimumSize = new System.Drawing.Size(580, 440);
            Name = "frm_MetodoPago";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Mantenedor de Metodos de Pago";
            Load += Frm_MetodoPago_Load;

            panelTitulo.ResumeLayout(false);
            tsMetodosPago.ResumeLayout(false);
            tsMetodosPago.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMetodosPago).EndInit();
            panelEdicion.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            panelBotones.ResumeLayout(false);
            flpBotones.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}