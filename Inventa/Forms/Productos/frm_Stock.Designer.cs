namespace Inventa.Forms.Productos
{
    partial class frm_Stock
    {
        private System.ComponentModel.IContainer components = null;

        // ── Panel de contexto (header) ────────────────────────────────────────
        private System.Windows.Forms.Panel panelContexto;
        private System.Windows.Forms.Label lblProducto;

        // ── Layout principal ──────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpMain;

        // ── ToolStrip ─────────────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip tsStock;
        private System.Windows.Forms.ToolStripButton btnAjustar;
        private System.Windows.Forms.ToolStripButton btnTransferir;
        private System.Windows.Forms.ToolStripSeparator tsSep;
        private System.Windows.Forms.ToolStripButton btnRefrescar;
        private System.Windows.Forms.ToolStripLabel tsLblLeyenda;

        // ── Grilla ────────────────────────────────────────────────────────────
        private System.Windows.Forms.DataGridView dgvStock;

        // ── Footer de totales ─────────────────────────────────────────────────
        private System.Windows.Forms.Label lblTotales;

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
            panelContexto = new Panel();
            lblProducto = new Label();
            tlpMain = new TableLayoutPanel();
            tsStock = new ToolStrip();
            btnAjustar = new ToolStripButton();
            btnTransferir = new ToolStripButton();
            tsSep = new ToolStripSeparator();
            btnRefrescar = new ToolStripButton();
            tsLblLeyenda = new ToolStripLabel();
            dgvStock = new DataGridView();
            lblTotales = new Label();
            panelContexto.SuspendLayout();
            tlpMain.SuspendLayout();
            tsStock.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStock).BeginInit();
            SuspendLayout();
            // 
            // panelContexto
            // 
            panelContexto.BackColor = Color.FromArgb(30, 60, 114);
            panelContexto.Controls.Add(lblProducto);
            panelContexto.Dock = DockStyle.Top;
            panelContexto.Location = new Point(0, 0);
            panelContexto.Name = "panelContexto";
            panelContexto.Padding = new Padding(16, 0, 16, 0);
            panelContexto.Size = new Size(884, 44);
            panelContexto.TabIndex = 0;
            // 
            // lblProducto
            // 
            lblProducto.Dock = DockStyle.Fill;
            lblProducto.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblProducto.ForeColor = Color.White;
            lblProducto.Location = new Point(16, 0);
            lblProducto.Name = "lblProducto";
            lblProducto.Size = new Size(852, 44);
            lblProducto.TabIndex = 0;
            lblProducto.Text = "Stock del Producto";
            lblProducto.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(tsStock, 0, 0);
            tlpMain.Controls.Add(dgvStock, 0, 1);
            tlpMain.Controls.Add(lblTotales, 0, 2);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 44);
            tlpMain.Name = "tlpMain";
            tlpMain.Padding = new Padding(8);
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpMain.Size = new Size(884, 417);
            tlpMain.TabIndex = 1;
            // 
            // tsStock
            // 
            tsStock.Dock = DockStyle.Fill;
            tsStock.GripStyle = ToolStripGripStyle.Hidden;
            tsStock.Items.AddRange(new ToolStripItem[] { btnAjustar, btnTransferir, tsSep, btnRefrescar, tsLblLeyenda });
            tsStock.Location = new Point(8, 8);
            tsStock.Name = "tsStock";
            tsStock.RenderMode = ToolStripRenderMode.System;
            tsStock.Size = new Size(868, 30);
            tsStock.TabIndex = 0;
            // 
            // btnAjustar
            // 
            btnAjustar.Image = Properties.Resources.adjust_50;
            btnAjustar.ImageTransparentColor = Color.Magenta;
            btnAjustar.Name = "btnAjustar";
            btnAjustar.Size = new Size(64, 27);
            btnAjustar.Text = "Ajustar";
            btnAjustar.Click += BtnAjustar_Click;
            // 
            // btnTransferir
            // 
            btnTransferir.Image = Properties.Resources.delivery;
            btnTransferir.ImageTransparentColor = Color.Magenta;
            btnTransferir.Name = "btnTransferir";
            btnTransferir.Size = new Size(75, 27);
            btnTransferir.Text = "Transferir";
            btnTransferir.Click += BtnTransferir_Click;
            // 
            // tsSep
            // 
            tsSep.Name = "tsSep";
            tsSep.Size = new Size(6, 30);
            // 
            // btnRefrescar
            // 
            btnRefrescar.Image = Properties.Resources.refresh;
            btnRefrescar.ImageTransparentColor = Color.Magenta;
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Size = new Size(98, 27);
            btnRefrescar.Text = "Refrescar (F5)";
            btnRefrescar.Click += BtnRefrescar_Click;
            // 
            // tsLblLeyenda
            // 
            tsLblLeyenda.Alignment = ToolStripItemAlignment.Right;
            tsLblLeyenda.Font = new Font("Segoe UI", 7.5F);
            tsLblLeyenda.ForeColor = Color.FromArgb(96, 94, 92);
            tsLblLeyenda.Name = "tsLblLeyenda";
            tsLblLeyenda.Size = new Size(116, 27);
            tsLblLeyenda.Text = "■ Negativo   ■ Sin stock";
            // 
            // dgvStock
            // 
            dgvStock.AllowUserToAddRows = false;
            dgvStock.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(250, 249, 248);
            dgvStock.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvStock.BackgroundColor = Color.White;
            dgvStock.BorderStyle = BorderStyle.None;
            dgvStock.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(243, 242, 241);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle2.Padding = new Padding(8, 0, 0, 0);
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvStock.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvStock.ColumnHeadersHeight = 36;
            dgvStock.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(96, 94, 92);
            dataGridViewCellStyle3.Padding = new Padding(8, 2, 8, 2);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(229, 243, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(50, 49, 48);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvStock.DefaultCellStyle = dataGridViewCellStyle3;
            dgvStock.Dock = DockStyle.Fill;
            dgvStock.EnableHeadersVisualStyles = false;
            dgvStock.GridColor = Color.FromArgb(237, 235, 233);
            dgvStock.Location = new Point(11, 41);
            dgvStock.MultiSelect = false;
            dgvStock.Name = "dgvStock";
            dgvStock.ReadOnly = true;
            dgvStock.RowHeadersVisible = false;
            dgvStock.RowTemplate.Height = 32;
            dgvStock.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvStock.Size = new Size(862, 335);
            dgvStock.TabIndex = 1;
            // 
            // lblTotales
            // 
            lblTotales.BackColor = Color.FromArgb(243, 242, 241);
            lblTotales.BorderStyle = BorderStyle.FixedSingle;
            lblTotales.Dock = DockStyle.Fill;
            lblTotales.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotales.ForeColor = Color.FromArgb(50, 49, 48);
            lblTotales.Location = new Point(11, 379);
            lblTotales.Name = "lblTotales";
            lblTotales.Padding = new Padding(8, 0, 8, 0);
            lblTotales.Size = new Size(862, 30);
            lblTotales.TabIndex = 2;
            lblTotales.Text = "Total unidades: 0.0000   |   Valorizado: $0.00";
            lblTotales.TextAlign = ContentAlignment.MiddleRight;
            // 
            // frm_Stock
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(884, 461);
            Controls.Add(tlpMain);
            Controls.Add(panelContexto);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(760, 460);
            Name = "frm_Stock";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Stock del Producto";
            Load += Frm_Stock_Load;
            panelContexto.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            tsStock.ResumeLayout(false);
            tsStock.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStock).EndInit();
            ResumeLayout(false);
        }
    }
}