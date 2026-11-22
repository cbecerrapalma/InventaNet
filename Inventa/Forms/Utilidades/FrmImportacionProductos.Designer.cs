namespace Inventa.Forms.Utilidades
{
    partial class FrmImportacionProductos
    {
        private System.ComponentModel.IContainer components = null;

        // TableLayout principal
        private System.Windows.Forms.TableLayoutPanel tlpPrincipal;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Panel panelFooter;

        // Header
        private FontAwesome.Sharp.IconPictureBox picLogo;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;

        // TableLayout interno para contenido
        private System.Windows.Forms.TableLayoutPanel tlpContenido;

        // GroupBox Archivo
        private System.Windows.Forms.GroupBox gbxArchivo;
        private System.Windows.Forms.Label lblRuta;
        private System.Windows.Forms.TextBox txtRutaArchivo;
        private FontAwesome.Sharp.IconButton btnSeleccionarArchivo;
        private FontAwesome.Sharp.IconButton btnGenerarPlantilla;
        private System.Windows.Forms.Label lblInfoArchivo;

        // GroupBox Modo
        private System.Windows.Forms.GroupBox gbxModo;
        private System.Windows.Forms.ComboBox cmbModo;
        private System.Windows.Forms.Panel panelModoInfo;
        private System.Windows.Forms.Label lblModoIcono;
        private System.Windows.Forms.Label lblModoTitulo;
        private System.Windows.Forms.Label lblModoDescripcion;
        private System.Windows.Forms.ProgressBar progressModoRiesgo;

        // GroupBox Estadísticas
        private System.Windows.Forms.GroupBox gbxEstadisticas;
        private System.Windows.Forms.FlowLayoutPanel flpEstadisticas;
        private System.Windows.Forms.Panel panelProdStats;
        private FontAwesome.Sharp.IconPictureBox iconProd;
        private System.Windows.Forms.Label lblProdTitulo;
        private System.Windows.Forms.Label lblProdValor;
        private System.Windows.Forms.Panel panelVarStats;
        private FontAwesome.Sharp.IconPictureBox iconVar;
        private System.Windows.Forms.Label lblVarTitulo;
        private System.Windows.Forms.Label lblVarValor;
        private System.Windows.Forms.Panel panelPresStats;
        private FontAwesome.Sharp.IconPictureBox iconPres;
        private System.Windows.Forms.Label lblPresTitulo;
        private System.Windows.Forms.Label lblPresValor;
        private System.Windows.Forms.Panel panelErrorStats;
        private FontAwesome.Sharp.IconPictureBox iconError;
        private System.Windows.Forms.Label lblErrorTitulo;
        private System.Windows.Forms.Label lblErrorValor;

        // GroupBox Previsualización
        private System.Windows.Forms.GroupBox gbxPrevisualizacion;
        private System.Windows.Forms.TabControl tabHojas;
        private System.Windows.Forms.TabPage tabProductos;
        private System.Windows.Forms.TabPage tabVariantes;
        private System.Windows.Forms.TabPage tabPresentaciones;
        private System.Windows.Forms.DataGridView dgvProductos;
        private System.Windows.Forms.DataGridView dgvVariantes;
        private System.Windows.Forms.DataGridView dgvPresentaciones;
        private System.Windows.Forms.Panel panelPreviewToolbar;
        private FontAwesome.Sharp.IconButton btnPrimeraPagina;
        private FontAwesome.Sharp.IconButton btnPaginaAnterior;
        private System.Windows.Forms.NumericUpDown numPaginaActual;
        private FontAwesome.Sharp.IconButton btnPaginaSiguiente;
        private FontAwesome.Sharp.IconButton btnUltimaPagina;
        private System.Windows.Forms.Label lblTotalPaginas;
        private System.Windows.Forms.ComboBox cmbFilasPorPagina;

        // GroupBox Validación
        private System.Windows.Forms.GroupBox gbxValidacion;
        private System.Windows.Forms.DataGridView dgvErrores;
        private FontAwesome.Sharp.IconButton btnExportarErrores;
        private FontAwesome.Sharp.IconButton btnIgnorarErrores;

        // GroupBox Progreso
        private System.Windows.Forms.GroupBox gbxProgreso;
        private System.Windows.Forms.ProgressBar progressGeneral;
        private System.Windows.Forms.Label lblProgresoGeneral;
        private System.Windows.Forms.ListView lvPasos;
        private System.Windows.Forms.ColumnHeader colPaso;
        private System.Windows.Forms.ColumnHeader colEstado;
        private System.Windows.Forms.ColumnHeader colRegistros;
        private FontAwesome.Sharp.IconButton btnPausar;
        private FontAwesome.Sharp.IconButton btnCancelar;

        // Botones de acción
        private System.Windows.Forms.FlowLayoutPanel flpAcciones;
        private FontAwesome.Sharp.IconButton btnImportar;
        private FontAwesome.Sharp.IconButton btnValidar;
        private FontAwesome.Sharp.IconButton btnVistaPrevia;
        private FontAwesome.Sharp.IconButton btnHistorial;
        private FontAwesome.Sharp.IconButton btnAyuda;

        // Diálogos y utilidades
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.ContextMenuStrip contextMenuGrid;
        private System.Windows.Forms.ToolStripMenuItem copiarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportarExcelToolStripMenuItem;
        private System.Windows.Forms.Timer timerAnimacion;
        private System.Windows.Forms.ImageList imageListPasos;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmImportacionProductos));

            // ============================================
            // FORMULARIO PRINCIPAL
            // ============================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.ClientSize = new System.Drawing.Size(1400, 900);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1200, 700);
            this.Name = "FrmImportacionProductos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Importación Masiva de Productos - Módulo de Inventario";
            this.Padding = new System.Windows.Forms.Padding(10);

            // ============================================
            // TABLE LAYOUT PRINCIPAL
            // ============================================
            this.tlpPrincipal = new System.Windows.Forms.TableLayoutPanel();
            this.tlpPrincipal.ColumnCount = 1;
            this.tlpPrincipal.RowCount = 3;
            this.tlpPrincipal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPrincipal.Padding = new System.Windows.Forms.Padding(10);
            this.tlpPrincipal.BackColor = System.Drawing.Color.Transparent;

            this.tlpPrincipal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tlpPrincipal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPrincipal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));

            // ============================================
            // PANEL HEADER
            // ============================================
            this.panelHeader = new System.Windows.Forms.Panel();
            this.panelHeader.BackColor = System.Drawing.Color.White;
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelHeader.Padding = new System.Windows.Forms.Padding(15, 5, 15, 5);

            this.picLogo = new FontAwesome.Sharp.IconPictureBox();
            this.picLogo.IconChar = FontAwesome.Sharp.IconChar.BoxOpen;
            this.picLogo.IconColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.picLogo.IconSize = 50;
            this.picLogo.Size = new System.Drawing.Size(50, 50);
            this.picLogo.Location = new System.Drawing.Point(15, 10);

            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblTitulo.Text = "Importación Masiva de Productos";
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(41, 53, 72);
            this.lblTitulo.Location = new System.Drawing.Point(80, 10);
            this.lblTitulo.Size = new System.Drawing.Size(500, 40);

            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.lblSubtitulo.Text = "Gestione la importación de productos con estructura completa";
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Italic);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(120, 130, 140);
            this.lblSubtitulo.Location = new System.Drawing.Point(82, 45);
            this.lblSubtitulo.Size = new System.Drawing.Size(500, 20);

            this.panelHeader.Controls.Add(this.picLogo);
            this.panelHeader.Controls.Add(this.lblTitulo);
            this.panelHeader.Controls.Add(this.lblSubtitulo);

            // ============================================
            // PANEL CONTENT
            // ============================================
            this.panelContent = new System.Windows.Forms.Panel();
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Padding = new System.Windows.Forms.Padding(5);
            this.panelContent.BackColor = System.Drawing.Color.Transparent;

            // ============================================
            // TABLE LAYOUT CONTENIDO (mejora punto 1)
            // ============================================
            this.tlpContenido = new System.Windows.Forms.TableLayoutPanel();
            this.tlpContenido.ColumnCount = 2;
            this.tlpContenido.RowCount = 4;
            this.tlpContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpContenido.Padding = new System.Windows.Forms.Padding(5);
            this.tlpContenido.BackColor = System.Drawing.Color.Transparent;

            this.tlpContenido.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tlpContenido.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));

            this.tlpContenido.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tlpContenido.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tlpContenido.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpContenido.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));

            // ============================================
            // GROUPBOX ARCHIVO
            // ============================================
            this.gbxArchivo = new System.Windows.Forms.GroupBox();
            this.gbxArchivo.Text = " Archivo de Importación ";
            this.gbxArchivo.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbxArchivo.ForeColor = System.Drawing.Color.FromArgb(41, 53, 72);
            this.gbxArchivo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxArchivo.BackColor = System.Drawing.Color.White;

            this.lblRuta = new System.Windows.Forms.Label();
            this.lblRuta.Text = "Ruta del archivo:";
            this.lblRuta.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblRuta.Location = new System.Drawing.Point(20, 35);
            this.lblRuta.Size = new System.Drawing.Size(120, 25);

            this.txtRutaArchivo = new System.Windows.Forms.TextBox();
            this.txtRutaArchivo.Location = new System.Drawing.Point(140, 32);
            this.txtRutaArchivo.Size = new System.Drawing.Size(350, 30);
            this.txtRutaArchivo.ReadOnly = true;
            this.txtRutaArchivo.BackColor = System.Drawing.Color.FromArgb(245, 245, 250);

            this.btnSeleccionarArchivo = new FontAwesome.Sharp.IconButton();
            this.btnSeleccionarArchivo.Text = " Examinar";
            this.btnSeleccionarArchivo.IconChar = FontAwesome.Sharp.IconChar.FolderOpen;
            this.btnSeleccionarArchivo.IconColor = System.Drawing.Color.White;
            this.btnSeleccionarArchivo.IconSize = 20;
            this.btnSeleccionarArchivo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSeleccionarArchivo.Location = new System.Drawing.Point(500, 30);
            this.btnSeleccionarArchivo.Size = new System.Drawing.Size(130, 35);
            this.btnSeleccionarArchivo.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.btnSeleccionarArchivo.ForeColor = System.Drawing.Color.White;
            this.btnSeleccionarArchivo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSeleccionarArchivo.FlatAppearance.BorderSize = 0;
            this.btnSeleccionarArchivo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSeleccionarArchivo.Name = "btnSeleccionarArchivo";
            this.btnSeleccionarArchivo.TabIndex = 1;

            this.btnGenerarPlantilla = new FontAwesome.Sharp.IconButton();
            this.btnGenerarPlantilla.Text = " Generar Plantilla";
            this.btnGenerarPlantilla.IconChar = FontAwesome.Sharp.IconChar.FileExcel;
            this.btnGenerarPlantilla.IconColor = System.Drawing.Color.White;
            this.btnGenerarPlantilla.IconSize = 20;
            this.btnGenerarPlantilla.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnGenerarPlantilla.Location = new System.Drawing.Point(20, 70);
            this.btnGenerarPlantilla.Size = new System.Drawing.Size(150, 35);
            this.btnGenerarPlantilla.BackColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this.btnGenerarPlantilla.ForeColor = System.Drawing.Color.White;
            this.btnGenerarPlantilla.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerarPlantilla.FlatAppearance.BorderSize = 0;
            this.btnGenerarPlantilla.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGenerarPlantilla.Name = "btnGenerarPlantilla";
            this.btnGenerarPlantilla.TabIndex = 2;

            this.lblInfoArchivo = new System.Windows.Forms.Label();
            this.lblInfoArchivo.Text = "Formatos soportados: .xlsx, .xls";
            this.lblInfoArchivo.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
            this.lblInfoArchivo.ForeColor = System.Drawing.Color.FromArgb(150, 150, 150);
            this.lblInfoArchivo.Location = new System.Drawing.Point(180, 78);
            this.lblInfoArchivo.Size = new System.Drawing.Size(200, 20);

            this.gbxArchivo.Controls.Add(this.lblRuta);
            this.gbxArchivo.Controls.Add(this.txtRutaArchivo);
            this.gbxArchivo.Controls.Add(this.btnSeleccionarArchivo);
            this.gbxArchivo.Controls.Add(this.btnGenerarPlantilla);
            this.gbxArchivo.Controls.Add(this.lblInfoArchivo);

            // ============================================
            // GROUPBOX MODO
            // ============================================
            this.gbxModo = new System.Windows.Forms.GroupBox();
            this.gbxModo.Text = " Modo de Importación ";
            this.gbxModo.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbxModo.ForeColor = System.Drawing.Color.FromArgb(41, 53, 72);
            this.gbxModo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxModo.BackColor = System.Drawing.Color.White;

            this.cmbModo = new System.Windows.Forms.ComboBox();
            this.cmbModo.Location = new System.Drawing.Point(20, 32);
            this.cmbModo.Size = new System.Drawing.Size(340, 30);
            this.cmbModo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbModo.Name = "cmbModo";
            this.cmbModo.TabIndex = 3;

            this.panelModoInfo = new System.Windows.Forms.Panel();
            this.panelModoInfo.Location = new System.Drawing.Point(20, 70);
            this.panelModoInfo.Size = new System.Drawing.Size(340, 40);
            this.panelModoInfo.BackColor = System.Drawing.Color.FromArgb(245, 245, 250);

            this.lblModoIcono = new System.Windows.Forms.Label();
            this.lblModoIcono.Text = "🔄";
            this.lblModoIcono.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblModoIcono.Location = new System.Drawing.Point(5, 5);
            this.lblModoIcono.Size = new System.Drawing.Size(40, 30);

            this.lblModoTitulo = new System.Windows.Forms.Label();
            this.lblModoTitulo.Text = "Modo Mixto";
            this.lblModoTitulo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblModoTitulo.Location = new System.Drawing.Point(45, 5);
            this.lblModoTitulo.Size = new System.Drawing.Size(100, 20);

            this.lblModoDescripcion = new System.Windows.Forms.Label();
            this.lblModoDescripcion.Text = "Crea nuevos y actualiza existentes";
            this.lblModoDescripcion.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblModoDescripcion.Location = new System.Drawing.Point(45, 22);
            this.lblModoDescripcion.Size = new System.Drawing.Size(200, 15);

            this.progressModoRiesgo = new System.Windows.Forms.ProgressBar();
            this.progressModoRiesgo.Location = new System.Drawing.Point(250, 10);
            this.progressModoRiesgo.Size = new System.Drawing.Size(80, 20);
            this.progressModoRiesgo.Maximum = 10;
            this.progressModoRiesgo.Visible = false;

            this.panelModoInfo.Controls.Add(this.lblModoIcono);
            this.panelModoInfo.Controls.Add(this.lblModoTitulo);
            this.panelModoInfo.Controls.Add(this.lblModoDescripcion);
            this.panelModoInfo.Controls.Add(this.progressModoRiesgo);

            this.gbxModo.Controls.Add(this.cmbModo);
            this.gbxModo.Controls.Add(this.panelModoInfo);

            // ============================================
            // GROUPBOX ESTADÍSTICAS
            // ============================================
            this.gbxEstadisticas = new System.Windows.Forms.GroupBox();
            this.gbxEstadisticas.Text = " Estadísticas del Archivo ";
            this.gbxEstadisticas.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbxEstadisticas.ForeColor = System.Drawing.Color.FromArgb(41, 53, 72);
            this.gbxEstadisticas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxEstadisticas.BackColor = System.Drawing.Color.White;

            this.flpEstadisticas = new System.Windows.Forms.FlowLayoutPanel();
            this.flpEstadisticas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpEstadisticas.Padding = new System.Windows.Forms.Padding(10);

            // Panel Productos
            this.panelProdStats = new System.Windows.Forms.Panel();
            this.panelProdStats.Size = new System.Drawing.Size(200, 60);
            this.panelProdStats.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);

            this.iconProd = new FontAwesome.Sharp.IconPictureBox();
            this.iconProd.IconChar = FontAwesome.Sharp.IconChar.Box;
            this.iconProd.IconColor = System.Drawing.Color.White;
            this.iconProd.IconSize = 30;
            this.iconProd.Location = new System.Drawing.Point(10, 15);

            this.lblProdTitulo = new System.Windows.Forms.Label();
            this.lblProdTitulo.Text = "Productos";
            this.lblProdTitulo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblProdTitulo.ForeColor = System.Drawing.Color.White;
            this.lblProdTitulo.Location = new System.Drawing.Point(50, 10);
            this.lblProdTitulo.Size = new System.Drawing.Size(80, 20);

            this.lblProdValor = new System.Windows.Forms.Label();
            this.lblProdValor.Text = "0";
            this.lblProdValor.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblProdValor.ForeColor = System.Drawing.Color.White;
            this.lblProdValor.Location = new System.Drawing.Point(50, 25);
            this.lblProdValor.Size = new System.Drawing.Size(100, 25);

            this.panelProdStats.Controls.Add(this.iconProd);
            this.panelProdStats.Controls.Add(this.lblProdTitulo);
            this.panelProdStats.Controls.Add(this.lblProdValor);

            // Panel Variantes (similar)
            this.panelVarStats = new System.Windows.Forms.Panel();
            this.panelVarStats.Size = new System.Drawing.Size(200, 60);
            this.panelVarStats.BackColor = System.Drawing.Color.FromArgb(46, 204, 113);

            this.iconVar = new FontAwesome.Sharp.IconPictureBox();
            this.iconVar.IconChar = FontAwesome.Sharp.IconChar.CodeBranch;
            this.iconVar.IconColor = System.Drawing.Color.White;
            this.iconVar.IconSize = 30;
            this.iconVar.Location = new System.Drawing.Point(10, 15);

            this.lblVarTitulo = new System.Windows.Forms.Label();
            this.lblVarTitulo.Text = "Variantes";
            this.lblVarTitulo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblVarTitulo.ForeColor = System.Drawing.Color.White;
            this.lblVarTitulo.Location = new System.Drawing.Point(50, 10);
            this.lblVarTitulo.Size = new System.Drawing.Size(80, 20);

            this.lblVarValor = new System.Windows.Forms.Label();
            this.lblVarValor.Text = "0";
            this.lblVarValor.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblVarValor.ForeColor = System.Drawing.Color.White;
            this.lblVarValor.Location = new System.Drawing.Point(50, 25);
            this.lblVarValor.Size = new System.Drawing.Size(100, 25);

            this.panelVarStats.Controls.Add(this.iconVar);
            this.panelVarStats.Controls.Add(this.lblVarTitulo);
            this.panelVarStats.Controls.Add(this.lblVarValor);

            // Panel Presentaciones (similar)
            this.panelPresStats = new System.Windows.Forms.Panel();
            this.panelPresStats.Size = new System.Drawing.Size(200, 60);
            this.panelPresStats.BackColor = System.Drawing.Color.FromArgb(155, 89, 182);

            this.iconPres = new FontAwesome.Sharp.IconPictureBox();
            this.iconPres.IconChar = FontAwesome.Sharp.IconChar.BoxesPacking;
            this.iconPres.IconColor = System.Drawing.Color.White;
            this.iconPres.IconSize = 30;
            this.iconPres.Location = new System.Drawing.Point(10, 15);

            this.lblPresTitulo = new System.Windows.Forms.Label();
            this.lblPresTitulo.Text = "Presentaciones";
            this.lblPresTitulo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPresTitulo.ForeColor = System.Drawing.Color.White;
            this.lblPresTitulo.Location = new System.Drawing.Point(50, 10);
            this.lblPresTitulo.Size = new System.Drawing.Size(80, 20);

            this.lblPresValor = new System.Windows.Forms.Label();
            this.lblPresValor.Text = "0";
            this.lblPresValor.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblPresValor.ForeColor = System.Drawing.Color.White;
            this.lblPresValor.Location = new System.Drawing.Point(50, 25);
            this.lblPresValor.Size = new System.Drawing.Size(100, 25);

            this.panelPresStats.Controls.Add(this.iconPres);
            this.panelPresStats.Controls.Add(this.lblPresTitulo);
            this.panelPresStats.Controls.Add(this.lblPresValor);

            // Panel Errores (similar)
            this.panelErrorStats = new System.Windows.Forms.Panel();
            this.panelErrorStats.Size = new System.Drawing.Size(200, 60);
            this.panelErrorStats.BackColor = System.Drawing.Color.FromArgb(231, 76, 60);

            this.iconError = new FontAwesome.Sharp.IconPictureBox();
            this.iconError.IconChar = FontAwesome.Sharp.IconChar.ExclamationTriangle;
            this.iconError.IconColor = System.Drawing.Color.White;
            this.iconError.IconSize = 30;
            this.iconError.Location = new System.Drawing.Point(10, 15);

            this.lblErrorTitulo = new System.Windows.Forms.Label();
            this.lblErrorTitulo.Text = "Errores";
            this.lblErrorTitulo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblErrorTitulo.ForeColor = System.Drawing.Color.White;
            this.lblErrorTitulo.Location = new System.Drawing.Point(50, 10);
            this.lblErrorTitulo.Size = new System.Drawing.Size(80, 20);

            this.lblErrorValor = new System.Windows.Forms.Label();
            this.lblErrorValor.Text = "0";
            this.lblErrorValor.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblErrorValor.ForeColor = System.Drawing.Color.White;
            this.lblErrorValor.Location = new System.Drawing.Point(50, 25);
            this.lblErrorValor.Size = new System.Drawing.Size(100, 25);

            this.panelErrorStats.Controls.Add(this.iconError);
            this.panelErrorStats.Controls.Add(this.lblErrorTitulo);
            this.panelErrorStats.Controls.Add(this.lblErrorValor);

            this.flpEstadisticas.Controls.Add(this.panelProdStats);
            this.flpEstadisticas.Controls.Add(this.panelVarStats);
            this.flpEstadisticas.Controls.Add(this.panelPresStats);
            this.flpEstadisticas.Controls.Add(this.panelErrorStats);

            this.gbxEstadisticas.Controls.Add(this.flpEstadisticas);

            // ============================================
            // GROUPBOX PREVISUALIZACIÓN
            // ============================================
            this.gbxPrevisualizacion = new System.Windows.Forms.GroupBox();
            this.gbxPrevisualizacion.Text = " Previsualización de Datos ";
            this.gbxPrevisualizacion.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbxPrevisualizacion.ForeColor = System.Drawing.Color.FromArgb(41, 53, 72);
            this.gbxPrevisualizacion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxPrevisualizacion.BackColor = System.Drawing.Color.White;

            this.tabHojas = new System.Windows.Forms.TabControl();
            this.tabHojas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabHojas.Font = new System.Drawing.Font("Segoe UI", 9F);

            // Tab Productos
            this.tabProductos = new System.Windows.Forms.TabPage();
            this.tabProductos.Text = "PRODUCTOS";

            this.dgvProductos = new System.Windows.Forms.DataGridView();
            this.dgvProductos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProductos.ReadOnly = true;
            this.dgvProductos.RowHeadersVisible = false;
            this.dgvProductos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProductos.EnableHeadersVisualStyles = false;
            this.dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dgvProductos.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.dgvProductos.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            this.dgvProductos.Name = "dgvProductos";
            this.dgvProductos.TabIndex = 10;

            this.tabProductos.Controls.Add(this.dgvProductos);

            // Tab Variantes (similar)
            this.tabVariantes = new System.Windows.Forms.TabPage();
            this.tabVariantes.Text = "VARIANTES";

            this.dgvVariantes = new System.Windows.Forms.DataGridView();
            this.dgvVariantes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvVariantes.ReadOnly = true;
            this.dgvVariantes.RowHeadersVisible = false;
            this.dgvVariantes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvVariantes.EnableHeadersVisualStyles = false;
            this.dgvVariantes.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.dgvVariantes.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dgvVariantes.Name = "dgvVariantes";
            this.dgvVariantes.TabIndex = 11;

            this.tabVariantes.Controls.Add(this.dgvVariantes);

            // Tab Presentaciones (similar)
            this.tabPresentaciones = new System.Windows.Forms.TabPage();
            this.tabPresentaciones.Text = "PRESENTACIONES";

            this.dgvPresentaciones = new System.Windows.Forms.DataGridView();
            this.dgvPresentaciones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPresentaciones.ReadOnly = true;
            this.dgvPresentaciones.RowHeadersVisible = false;
            this.dgvPresentaciones.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPresentaciones.EnableHeadersVisualStyles = false;
            this.dgvPresentaciones.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.dgvPresentaciones.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dgvPresentaciones.Name = "dgvPresentaciones";
            this.dgvPresentaciones.TabIndex = 12;

            this.tabPresentaciones.Controls.Add(this.dgvPresentaciones);

            this.tabHojas.TabPages.Add(this.tabProductos);
            this.tabHojas.TabPages.Add(this.tabVariantes);
            this.tabHojas.TabPages.Add(this.tabPresentaciones);

            // Toolbar de previsualización
            this.panelPreviewToolbar = new System.Windows.Forms.Panel();
            this.panelPreviewToolbar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelPreviewToolbar.Height = 40;
            this.panelPreviewToolbar.BackColor = System.Drawing.Color.FromArgb(245, 245, 250);

            this.btnPrimeraPagina = new FontAwesome.Sharp.IconButton();
            this.btnPrimeraPagina.IconChar = FontAwesome.Sharp.IconChar.AngleDoubleLeft;
            this.btnPrimeraPagina.IconSize = 16;
            this.btnPrimeraPagina.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrimeraPagina.FlatAppearance.BorderSize = 0;
            this.btnPrimeraPagina.Size = new System.Drawing.Size(30, 30);
            this.btnPrimeraPagina.Location = new System.Drawing.Point(5, 5);
            this.btnPrimeraPagina.Name = "btnPrimeraPagina";
            this.btnPrimeraPagina.TabIndex = 13;

            this.btnPaginaAnterior = new FontAwesome.Sharp.IconButton();
            this.btnPaginaAnterior.IconChar = FontAwesome.Sharp.IconChar.AngleLeft;
            this.btnPaginaAnterior.IconSize = 16;
            this.btnPaginaAnterior.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPaginaAnterior.FlatAppearance.BorderSize = 0;
            this.btnPaginaAnterior.Size = new System.Drawing.Size(30, 30);
            this.btnPaginaAnterior.Location = new System.Drawing.Point(35, 5);
            this.btnPaginaAnterior.Name = "btnPaginaAnterior";
            this.btnPaginaAnterior.TabIndex = 14;

            this.numPaginaActual = new System.Windows.Forms.NumericUpDown();
            this.numPaginaActual.Minimum = 1;
            this.numPaginaActual.Maximum = 1;
            this.numPaginaActual.Value = 1;
            this.numPaginaActual.Location = new System.Drawing.Point(70, 5);
            this.numPaginaActual.Size = new System.Drawing.Size(50, 25);
            this.numPaginaActual.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numPaginaActual.Name = "numPaginaActual";
            this.numPaginaActual.TabIndex = 15;

            this.btnPaginaSiguiente = new FontAwesome.Sharp.IconButton();
            this.btnPaginaSiguiente.IconChar = FontAwesome.Sharp.IconChar.AngleRight;
            this.btnPaginaSiguiente.IconSize = 16;
            this.btnPaginaSiguiente.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPaginaSiguiente.FlatAppearance.BorderSize = 0;
            this.btnPaginaSiguiente.Size = new System.Drawing.Size(30, 30);
            this.btnPaginaSiguiente.Location = new System.Drawing.Point(125, 5);
            this.btnPaginaSiguiente.Name = "btnPaginaSiguiente";
            this.btnPaginaSiguiente.TabIndex = 16;

            this.btnUltimaPagina = new FontAwesome.Sharp.IconButton();
            this.btnUltimaPagina.IconChar = FontAwesome.Sharp.IconChar.AngleDoubleRight;
            this.btnUltimaPagina.IconSize = 16;
            this.btnUltimaPagina.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUltimaPagina.FlatAppearance.BorderSize = 0;
            this.btnUltimaPagina.Size = new System.Drawing.Size(30, 30);
            this.btnUltimaPagina.Location = new System.Drawing.Point(155, 5);
            this.btnUltimaPagina.Name = "btnUltimaPagina";
            this.btnUltimaPagina.TabIndex = 17;

            this.lblTotalPaginas = new System.Windows.Forms.Label();
            this.lblTotalPaginas.Text = "de 1";
            this.lblTotalPaginas.Location = new System.Drawing.Point(190, 10);
            this.lblTotalPaginas.Size = new System.Drawing.Size(50, 20);
            this.lblTotalPaginas.Name = "lblTotalPaginas";

            this.cmbFilasPorPagina = new System.Windows.Forms.ComboBox();
            this.cmbFilasPorPagina.Items.AddRange(new object[] { "10", "25", "50", "100" });
            this.cmbFilasPorPagina.SelectedIndex = 0;
            this.cmbFilasPorPagina.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilasPorPagina.Location = new System.Drawing.Point(500, 5);
            this.cmbFilasPorPagina.Size = new System.Drawing.Size(80, 25);
            this.cmbFilasPorPagina.Name = "cmbFilasPorPagina";
            this.cmbFilasPorPagina.TabIndex = 18;

            this.panelPreviewToolbar.Controls.Add(this.btnPrimeraPagina);
            this.panelPreviewToolbar.Controls.Add(this.btnPaginaAnterior);
            this.panelPreviewToolbar.Controls.Add(this.numPaginaActual);
            this.panelPreviewToolbar.Controls.Add(this.btnPaginaSiguiente);
            this.panelPreviewToolbar.Controls.Add(this.btnUltimaPagina);
            this.panelPreviewToolbar.Controls.Add(this.lblTotalPaginas);
            this.panelPreviewToolbar.Controls.Add(this.cmbFilasPorPagina);

            this.gbxPrevisualizacion.Controls.Add(this.tabHojas);
            this.gbxPrevisualizacion.Controls.Add(this.panelPreviewToolbar);

            // ============================================
            // GROUPBOX VALIDACIÓN
            // ============================================
            this.gbxValidacion = new System.Windows.Forms.GroupBox();
            this.gbxValidacion.Text = " Validación y Errores ";
            this.gbxValidacion.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbxValidacion.ForeColor = System.Drawing.Color.FromArgb(41, 53, 72);
            this.gbxValidacion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxValidacion.BackColor = System.Drawing.Color.White;

            this.dgvErrores = new System.Windows.Forms.DataGridView();
            this.dgvErrores.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvErrores.ReadOnly = true;
            this.dgvErrores.RowHeadersVisible = false;
            this.dgvErrores.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvErrores.EnableHeadersVisualStyles = false;
            this.dgvErrores.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.dgvErrores.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dgvErrores.Name = "dgvErrores";
            this.dgvErrores.TabIndex = 19;

            this.btnExportarErrores = new FontAwesome.Sharp.IconButton();
            this.btnExportarErrores.Text = " Exportar Errores";
            this.btnExportarErrores.IconChar = FontAwesome.Sharp.IconChar.FileExport;
            this.btnExportarErrores.IconSize = 16;
            this.btnExportarErrores.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnExportarErrores.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnExportarErrores.Height = 35;
            this.btnExportarErrores.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.btnExportarErrores.ForeColor = System.Drawing.Color.White;
            this.btnExportarErrores.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportarErrores.FlatAppearance.BorderSize = 0;
            this.btnExportarErrores.Name = "btnExportarErrores";
            this.btnExportarErrores.TabIndex = 20;

            this.btnIgnorarErrores = new FontAwesome.Sharp.IconButton();
            this.btnIgnorarErrores.Text = " Ignorar y Continuar";
            this.btnIgnorarErrores.IconChar = FontAwesome.Sharp.IconChar.ExclamationTriangle;
            this.btnIgnorarErrores.IconSize = 16;
            this.btnIgnorarErrores.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnIgnorarErrores.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnIgnorarErrores.Height = 35;
            this.btnIgnorarErrores.BackColor = System.Drawing.Color.FromArgb(241, 176, 57);
            this.btnIgnorarErrores.ForeColor = System.Drawing.Color.White;
            this.btnIgnorarErrores.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIgnorarErrores.FlatAppearance.BorderSize = 0;
            this.btnIgnorarErrores.Name = "btnIgnorarErrores";
            this.btnIgnorarErrores.TabIndex = 21;

            this.gbxValidacion.Controls.Add(this.dgvErrores);
            this.gbxValidacion.Controls.Add(this.btnExportarErrores);
            this.gbxValidacion.Controls.Add(this.btnIgnorarErrores);

            // ============================================
            // GROUPBOX PROGRESO
            // ============================================
            this.gbxProgreso = new System.Windows.Forms.GroupBox();
            this.gbxProgreso.Text = " Progreso de Importación ";
            this.gbxProgreso.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbxProgreso.ForeColor = System.Drawing.Color.FromArgb(41, 53, 72);
            this.gbxProgreso.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxProgreso.BackColor = System.Drawing.Color.White;
            this.gbxProgreso.Visible = false;

            this.progressGeneral = new System.Windows.Forms.ProgressBar();
            this.progressGeneral.Dock = System.Windows.Forms.DockStyle.Top;
            this.progressGeneral.Height = 30;
            this.progressGeneral.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressGeneral.Name = "progressGeneral";

            this.lblProgresoGeneral = new System.Windows.Forms.Label();
            this.lblProgresoGeneral.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblProgresoGeneral.Height = 25;
            this.lblProgresoGeneral.Text = "Iniciando importación... 0%";
            this.lblProgresoGeneral.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);

            this.lvPasos = new System.Windows.Forms.ListView();
            this.lvPasos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPasos.View = System.Windows.Forms.View.Details;
            this.lvPasos.FullRowSelect = true;
            this.lvPasos.GridLines = true;
            this.lvPasos.Name = "lvPasos";

            this.colPaso = new System.Windows.Forms.ColumnHeader();
            this.colPaso.Text = "Paso";
            this.colPaso.Width = 200;

            this.colEstado = new System.Windows.Forms.ColumnHeader();
            this.colEstado.Text = "Estado";
            this.colEstado.Width = 150;

            this.colRegistros = new System.Windows.Forms.ColumnHeader();
            this.colRegistros.Text = "Registros Procesados";
            this.colRegistros.Width = 150;

            this.lvPasos.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colPaso, this.colEstado, this.colRegistros
            });

            this.btnPausar = new FontAwesome.Sharp.IconButton();
            this.btnPausar.Text = " Pausar";
            this.btnPausar.IconChar = FontAwesome.Sharp.IconChar.Pause;
            this.btnPausar.IconSize = 16;
            this.btnPausar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPausar.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnPausar.Width = 100;
            this.btnPausar.BackColor = System.Drawing.Color.FromArgb(241, 176, 57);
            this.btnPausar.ForeColor = System.Drawing.Color.White;
            this.btnPausar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPausar.FlatAppearance.BorderSize = 0;
            this.btnPausar.Name = "btnPausar";
            this.btnPausar.TabIndex = 22;

            this.btnCancelar = new FontAwesome.Sharp.IconButton();
            this.btnCancelar.Text = " Cancelar";
            this.btnCancelar.IconChar = FontAwesome.Sharp.IconChar.Stop;
            this.btnCancelar.IconSize = 16;
            this.btnCancelar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCancelar.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancelar.Width = 100;
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(231, 76, 60);
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.TabIndex = 23;

            this.gbxProgreso.Controls.Add(this.lvPasos);
            this.gbxProgreso.Controls.Add(this.btnCancelar);
            this.gbxProgreso.Controls.Add(this.btnPausar);
            this.gbxProgreso.Controls.Add(this.lblProgresoGeneral);
            this.gbxProgreso.Controls.Add(this.progressGeneral);

            // ============================================
            // PANEL FOOTER (Botones de acción)
            // ============================================
            this.panelFooter = new System.Windows.Forms.Panel();
            this.panelFooter.BackColor = System.Drawing.Color.White;
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Fill;

            this.flpAcciones = new System.Windows.Forms.FlowLayoutPanel();
            this.flpAcciones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpAcciones.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpAcciones.Padding = new System.Windows.Forms.Padding(10);

            this.btnImportar = new FontAwesome.Sharp.IconButton();
            this.btnImportar.Text = " Iniciar Importación";
            this.btnImportar.IconChar = FontAwesome.Sharp.IconChar.Play;
            this.btnImportar.IconColor = System.Drawing.Color.White;
            this.btnImportar.IconSize = 20;
            this.btnImportar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnImportar.Size = new System.Drawing.Size(180, 45);
            this.btnImportar.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
            this.btnImportar.ForeColor = System.Drawing.Color.White;
            this.btnImportar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImportar.FlatAppearance.BorderSize = 0;
            this.btnImportar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnImportar.Enabled = false;
            this.btnImportar.Name = "btnImportar";
            this.btnImportar.TabIndex = 24;

            this.btnValidar = new FontAwesome.Sharp.IconButton();
            this.btnValidar.Text = " Validar Datos";
            this.btnValidar.IconChar = FontAwesome.Sharp.IconChar.CheckCircle;
            this.btnValidar.IconColor = System.Drawing.Color.White;
            this.btnValidar.IconSize = 20;
            this.btnValidar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnValidar.Size = new System.Drawing.Size(140, 45);
            this.btnValidar.BackColor = System.Drawing.Color.FromArgb(46, 204, 113);
            this.btnValidar.ForeColor = System.Drawing.Color.White;
            this.btnValidar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnValidar.FlatAppearance.BorderSize = 0;
            this.btnValidar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnValidar.Enabled = false;
            this.btnValidar.Name = "btnValidar";
            this.btnValidar.TabIndex = 25;

            this.btnVistaPrevia = new FontAwesome.Sharp.IconButton();
            this.btnVistaPrevia.Text = " Vista Previa";
            this.btnVistaPrevia.IconChar = FontAwesome.Sharp.IconChar.Eye;
            this.btnVistaPrevia.IconColor = System.Drawing.Color.White;
            this.btnVistaPrevia.IconSize = 20;
            this.btnVistaPrevia.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnVistaPrevia.Size = new System.Drawing.Size(130, 45);
            this.btnVistaPrevia.BackColor = System.Drawing.Color.FromArgb(155, 89, 182);
            this.btnVistaPrevia.ForeColor = System.Drawing.Color.White;
            this.btnVistaPrevia.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVistaPrevia.FlatAppearance.BorderSize = 0;
            this.btnVistaPrevia.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnVistaPrevia.Enabled = false;
            this.btnVistaPrevia.Name = "btnVistaPrevia";
            this.btnVistaPrevia.TabIndex = 26;

            this.btnHistorial = new FontAwesome.Sharp.IconButton();
            this.btnHistorial.Text = " Historial";
            this.btnHistorial.IconChar = FontAwesome.Sharp.IconChar.History;
            this.btnHistorial.IconColor = System.Drawing.Color.White;
            this.btnHistorial.IconSize = 20;
            this.btnHistorial.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnHistorial.Size = new System.Drawing.Size(100, 45);
            this.btnHistorial.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.btnHistorial.ForeColor = System.Drawing.Color.White;
            this.btnHistorial.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHistorial.FlatAppearance.BorderSize = 0;
            this.btnHistorial.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHistorial.Name = "btnHistorial";
            this.btnHistorial.TabIndex = 27;

            this.btnAyuda = new FontAwesome.Sharp.IconButton();
            this.btnAyuda.Text = " Ayuda";
            this.btnAyuda.IconChar = FontAwesome.Sharp.IconChar.QuestionCircle;
            this.btnAyuda.IconColor = System.Drawing.Color.White;
            this.btnAyuda.IconSize = 20;
            this.btnAyuda.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAyuda.Size = new System.Drawing.Size(90, 45);
            this.btnAyuda.BackColor = System.Drawing.Color.FromArgb(149, 165, 166);
            this.btnAyuda.ForeColor = System.Drawing.Color.White;
            this.btnAyuda.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAyuda.FlatAppearance.BorderSize = 0;
            this.btnAyuda.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAyuda.Name = "btnAyuda";
            this.btnAyuda.TabIndex = 28;

            this.flpAcciones.Controls.Add(this.btnAyuda);
            this.flpAcciones.Controls.Add(this.btnHistorial);
            this.flpAcciones.Controls.Add(this.btnVistaPrevia);
            this.flpAcciones.Controls.Add(this.btnValidar);
            this.flpAcciones.Controls.Add(this.btnImportar);

            this.panelFooter.Controls.Add(this.flpAcciones);

            // ============================================
            // CONTROLES ADICIONALES
            // ============================================
            this.contextMenuGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copiarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem("Copiar");
            this.exportarExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem("Exportar a Excel");
            this.contextMenuGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.copiarToolStripMenuItem,
                this.exportarExcelToolStripMenuItem
            });

            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialog.Filter = "Archivos Excel|*.xlsx;*.xls";

            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveFileDialog.Filter = "Archivos Excel|*.xlsx";

            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 300;
            this.toolTip.ReshowDelay = 100;
            this.toolTip.ShowAlways = true;

            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);

            this.timerAnimacion = new System.Windows.Forms.Timer(this.components);
            this.timerAnimacion.Interval = 100;

            this.imageListPasos = new System.Windows.Forms.ImageList(this.components);
            // Asignar imágenes (requiere recursos o icons)

            // ============================================
            // ENSAMBLAR TODO
            // ============================================
            this.tlpContenido.Controls.Add(this.gbxArchivo, 0, 0);
            this.tlpContenido.Controls.Add(this.gbxModo, 1, 0);
            this.tlpContenido.Controls.Add(this.gbxEstadisticas, 0, 1);
            this.tlpContenido.SetColumnSpan(this.gbxEstadisticas, 2);
            this.tlpContenido.Controls.Add(this.gbxPrevisualizacion, 0, 2);
            this.tlpContenido.Controls.Add(this.gbxValidacion, 1, 2);
            this.tlpContenido.Controls.Add(this.gbxProgreso, 0, 3);
            this.tlpContenido.SetColumnSpan(this.gbxProgreso, 2);

            this.panelContent.Controls.Add(this.tlpContenido);

            this.tlpPrincipal.Controls.Add(this.panelHeader, 0, 0);
            this.tlpPrincipal.Controls.Add(this.panelContent, 0, 1);
            this.tlpPrincipal.Controls.Add(this.panelFooter, 0, 2);

            this.Controls.Add(this.tlpPrincipal);

            // Configurar orden de tabulación (mejora punto 10)
            // Los TabIndex se han asignado en cada control
            // El orden será: Archivo -> Modo -> Botones de acción -> etc.

            this.ResumeLayout(false);
        }
    }
}