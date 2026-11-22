namespace Inventa.Forms.Productos
{
    partial class frm_ConfigAtributos
    {
        private System.ComponentModel.IContainer components = null;

        // ── Panel header ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitulo;

        // ── Layout principal ──────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpMain;

        // ── ToolStrip ─────────────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip tsAcciones;
        private System.Windows.Forms.ToolStripButton btnEditarValores;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton btnGestionarDefiniciones;
        private System.Windows.Forms.ToolStripButton btnGestionarValores;
        private System.Windows.Forms.ToolStripSeparator tsSep2;
        private System.Windows.Forms.ToolStripButton btnRefrescar;

        // ── Grilla ────────────────────────────────────────────────────────────
        private System.Windows.Forms.DataGridView dgvAtributos;

        // ── Barra de estado ───────────────────────────────────────────────────
        private System.Windows.Forms.Label lblEstado;

        // ── Panel botones ─────────────────────────────────────────────────────
        private System.Windows.Forms.FlowLayoutPanel flpBotones;
        private System.Windows.Forms.Button btnCerrar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelHeader = new System.Windows.Forms.Panel();
            lblTitulo = new System.Windows.Forms.Label();
            tlpMain = new System.Windows.Forms.TableLayoutPanel();
            tsAcciones = new System.Windows.Forms.ToolStrip();
            btnEditarValores = new System.Windows.Forms.ToolStripButton();
            tsSep1 = new System.Windows.Forms.ToolStripSeparator();
            btnGestionarDefiniciones = new System.Windows.Forms.ToolStripButton();
            btnGestionarValores = new System.Windows.Forms.ToolStripButton();
            tsSep2 = new System.Windows.Forms.ToolStripSeparator();
            btnRefrescar = new System.Windows.Forms.ToolStripButton();
            dgvAtributos = new System.Windows.Forms.DataGridView();
            lblEstado = new System.Windows.Forms.Label();
            flpBotones = new System.Windows.Forms.FlowLayoutPanel();
            btnCerrar = new System.Windows.Forms.Button();

            panelHeader.SuspendLayout();
            tlpMain.SuspendLayout();
            tsAcciones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAtributos).BeginInit();
            flpBotones.SuspendLayout();
            SuspendLayout();

            // ── Panel header azul ─────────────────────────────────────────────
            panelHeader.BackColor = System.Drawing.Color.FromArgb(30, 60, 114);
            panelHeader.Controls.Add(lblTitulo);
            panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            panelHeader.Location = new System.Drawing.Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new System.Windows.Forms.Padding(16, 0, 16, 0);
            panelHeader.Size = new System.Drawing.Size(800, 44);
            panelHeader.TabIndex = 0;

            lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            lblTitulo.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            lblTitulo.ForeColor = System.Drawing.Color.White;
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Text = "Configuracion de Atributos de la Variante";
            lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── TableLayoutPanel principal ────────────────────────────────────
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpMain.RowCount = 4;
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));   // ToolStrip
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));   // Grid
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));   // Estado
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));   // Botones
            tlpMain.Controls.Add(tsAcciones, 0, 0);
            tlpMain.Controls.Add(dgvAtributos, 0, 1);
            tlpMain.Controls.Add(lblEstado, 0, 2);
            tlpMain.Controls.Add(flpBotones, 0, 3);
            tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpMain.Location = new System.Drawing.Point(0, 44);
            tlpMain.Name = "tlpMain";
            tlpMain.Padding = new System.Windows.Forms.Padding(10);
            tlpMain.Size = new System.Drawing.Size(800, 456);
            tlpMain.TabIndex = 1;

            // ── ToolStrip ─────────────────────────────────────────────────────
            tsAcciones.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            tsAcciones.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            tsAcciones.Dock = System.Windows.Forms.DockStyle.Fill;
            tsAcciones.Name = "tsAcciones";
            tsAcciones.TabIndex = 0;
            tsAcciones.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                btnEditarValores, tsSep1,
                btnGestionarDefiniciones, btnGestionarValores,
                tsSep2, btnRefrescar
            });

            btnEditarValores.Image = global::Inventa.Properties.Resources.edit;
            btnEditarValores.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnEditarValores.Name = "btnEditarValores";
            btnEditarValores.Text = "Editar valores";
            btnEditarValores.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnEditarValores.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            btnEditarValores.ToolTipText = "Seleccionar valores para el atributo (Enter / doble clic)";
            btnEditarValores.Enabled = false;
            btnEditarValores.Click += BtnEditarValores_Click;

            tsSep1.Name = "tsSep1";

            btnGestionarDefiniciones.Name = "btnGestionarDefiniciones";
            btnGestionarDefiniciones.Text = "Gestionar definiciones";
            btnGestionarDefiniciones.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnGestionarDefiniciones.ToolTipText = "Administrar atributos de la categoria";
            btnGestionarDefiniciones.Click += BtnGestionarDefiniciones_Click;

            btnGestionarValores.Name = "btnGestionarValores";
            btnGestionarValores.Text = "Gestionar valores";
            btnGestionarValores.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnGestionarValores.ToolTipText = "Administrar valores del atributo seleccionado";
            btnGestionarValores.Enabled = false;
            btnGestionarValores.Click += BtnGestionarValores_Click;

            tsSep2.Name = "tsSep2";

            btnRefrescar.Image = global::Inventa.Properties.Resources.refresh;
            btnRefrescar.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Text = "Refrescar (F5)";
            btnRefrescar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btnRefrescar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            btnRefrescar.Click += BtnRefrescar_Click;

            // ── DataGridView ──────────────────────────────────────────────────
            // FIX: estilos con asignaciones directas — no variables locales DataGridViewCellStyle.
            // FIX: SystemColors.Highlight / ControlText reemplazados por AppColors equivalentes.
            dgvAtributos.AllowUserToAddRows = false;
            dgvAtributos.AllowUserToDeleteRows = false;
            dgvAtributos.AutoGenerateColumns = false;
            dgvAtributos.BackgroundColor = System.Drawing.Color.White;
            dgvAtributos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dgvAtributos.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dgvAtributos.GridColor = System.Drawing.Color.FromArgb(237, 235, 233);
            dgvAtributos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvAtributos.ColumnHeadersHeight = 36;
            dgvAtributos.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(243, 242, 241);
            dgvAtributos.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            dgvAtributos.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dgvAtributos.ColumnHeadersDefaultCellStyle.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            dgvAtributos.EnableHeadersVisualStyles = false;
            dgvAtributos.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            dgvAtributos.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(96, 94, 92);
            dgvAtributos.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            dgvAtributos.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(229, 243, 255);
            dgvAtributos.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            dgvAtributos.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            dgvAtributos.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(250, 249, 248);
            dgvAtributos.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvAtributos.MultiSelect = false;
            dgvAtributos.Name = "dgvAtributos";
            dgvAtributos.ReadOnly = true;
            dgvAtributos.RowHeadersVisible = false;
            dgvAtributos.RowTemplate.Height = 32;
            dgvAtributos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvAtributos.TabIndex = 1;
            dgvAtributos.SelectionChanged += DgvAtributos_SelectionChanged;

            // ── Label de estado ───────────────────────────────────────────────
            lblEstado.BackColor = System.Drawing.Color.FromArgb(243, 242, 241);
            lblEstado.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lblEstado.Dock = System.Windows.Forms.DockStyle.Fill;
            lblEstado.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            lblEstado.ForeColor = System.Drawing.Color.FromArgb(50, 49, 48);
            lblEstado.Name = "lblEstado";
            lblEstado.Text = "";
            lblEstado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── Panel botones ─────────────────────────────────────────────────
            // FIX: btnCerrar original tenía ForeColor=White sin BackColor — texto invisible.
            flpBotones.Controls.Add(btnCerrar);
            flpBotones.Dock = System.Windows.Forms.DockStyle.Fill;
            flpBotones.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flpBotones.Name = "flpBotones";
            flpBotones.Padding = new System.Windows.Forms.Padding(0, 7, 0, 7);
            flpBotones.WrapContents = false;

            btnCerrar.BackColor = System.Drawing.Color.FromArgb(30, 60, 114);
            btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnCerrar.ForeColor = System.Drawing.Color.White;
            btnCerrar.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new System.Drawing.Size(110, 34);
            btnCerrar.TabIndex = 0;
            btnCerrar.Text = "Cerrar";
            btnCerrar.UseVisualStyleBackColor = false;
            btnCerrar.Click += BtnCerrar_Click;

            // ── Formulario ────────────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(800, 500);
            Controls.Add(tlpMain);
            Controls.Add(panelHeader);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            MinimumSize = new System.Drawing.Size(700, 460);
            Name = "frm_ConfigAtributos";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Configuracion de Atributos de Variante";
            Load += Frm_ConfigAtributos_Load;

            panelHeader.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            tsAcciones.ResumeLayout(false);
            tsAcciones.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAtributos).EndInit();
            flpBotones.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}