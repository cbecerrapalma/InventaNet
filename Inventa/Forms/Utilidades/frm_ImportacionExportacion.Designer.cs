namespace Inventa.Forms.Utilidades
{
    partial class frm_ImportacionExportacion
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Panel panelSuperior;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabImportar;
        private System.Windows.Forms.TabPage tabExportar;
        private System.Windows.Forms.TabPage tabPlantillas;
        private System.Windows.Forms.TabPage tabHistorial;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tsslEstado;

        // Controles de Importar
        private System.Windows.Forms.GroupBox gpbTipoImportacion;
        private System.Windows.Forms.ComboBox cboTipoImportacion;
        private System.Windows.Forms.GroupBox gpbArchivo;
        private System.Windows.Forms.TextBox txtRutaArchivo;
        private System.Windows.Forms.Button btnExaminar;
        private System.Windows.Forms.GroupBox gpbOpciones;
        private System.Windows.Forms.CheckBox chkPrimeraFilaEncabezados;
        private System.Windows.Forms.CheckBox chkActualizarExistentes;
        private System.Windows.Forms.CheckBox chkCrearNuevos;
        private System.Windows.Forms.CheckBox chkValidarDatos;
        private System.Windows.Forms.GroupBox gpbMapeoCampos;
        private System.Windows.Forms.DataGridView dgvMapeo;
        private System.Windows.Forms.Button btnMapeoAutomatico;
        private System.Windows.Forms.Button btnLimpiarMapeo;
        private System.Windows.Forms.GroupBox gpbVistaPrevia;
        private System.Windows.Forms.DataGridView dgvVistaPrevia;
        private System.Windows.Forms.Button btnCargarVistaPrevia;
        private System.Windows.Forms.Label lblRegistrosEncontrados;
        private System.Windows.Forms.Label lblRegistrosEncontradosValor;
        private System.Windows.Forms.Label lblRegistrosValidos;
        private System.Windows.Forms.Label lblRegistrosValidosValor;
        private System.Windows.Forms.Label lblRegistrosConError;
        private System.Windows.Forms.Label lblRegistrosConErrorValor;
        private System.Windows.Forms.ProgressBar progressBarImportacion;
        private System.Windows.Forms.Button btnImportar;
        private System.Windows.Forms.Button btnCancelar;

        // Controles de Exportar
        private System.Windows.Forms.GroupBox gpbTipoExportacion;
        private System.Windows.Forms.ComboBox cboTipoExportacion;
        private System.Windows.Forms.GroupBox gpbFormatoExportacion;
        private System.Windows.Forms.RadioButton rdbExcel;
        private System.Windows.Forms.RadioButton rdbCSV;
        private System.Windows.Forms.RadioButton rdbXML;
        private System.Windows.Forms.RadioButton rdbJSON;
        private System.Windows.Forms.GroupBox gpbFiltrosExportacion;
        private System.Windows.Forms.CheckBox chkExportarActivos;
        private System.Windows.Forms.CheckBox chkExportarInactivos;
        private System.Windows.Forms.ComboBox cboExportarCategoria;
        private System.Windows.Forms.ComboBox cboExportarMarca;
        private System.Windows.Forms.DateTimePicker dtpExportarDesde;
        private System.Windows.Forms.DateTimePicker dtpExportarHasta;
        private System.Windows.Forms.Label lblExportarDesde;
        private System.Windows.Forms.Label lblExportarHasta;
        private System.Windows.Forms.GroupBox gpbCamposExportar;
        private System.Windows.Forms.CheckedListBox clbCamposExportar;
        private System.Windows.Forms.Button btnSeleccionarTodos;
        private System.Windows.Forms.Button btnDeseleccionarTodos;
        private System.Windows.Forms.Button btnExportar;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;

        // Controles de Plantillas
        private System.Windows.Forms.ListView lvPlantillas;
        private System.Windows.Forms.ColumnHeader chPlantillaNombre;
        private System.Windows.Forms.ColumnHeader chPlantillaTipo;
        private System.Windows.Forms.ColumnHeader chPlantillaFecha;
        private System.Windows.Forms.ColumnHeader chPlantillaDescripcion;
        private System.Windows.Forms.Button btnDescargarPlantilla;
        private System.Windows.Forms.Button btnVerPlantilla;
        private System.Windows.Forms.GroupBox gbpVistaPreviaPlantilla;
        private System.Windows.Forms.DataGridView dgvVistaPreviaPlantilla;

        // Controles de Historial
        private System.Windows.Forms.DataGridView dgvHistorial;
        private System.Windows.Forms.Button btnVerDetalle;
        private System.Windows.Forms.Button btnRepetirImportacion;
        private System.Windows.Forms.Button btnEliminarHistorial;
        private System.Windows.Forms.ComboBox cboFiltroHistorial;
        private System.Windows.Forms.DateTimePicker dtpHistorialDesde;
        private System.Windows.Forms.DateTimePicker dtpHistorialHasta;
        private System.Windows.Forms.Button btnFiltrarHistorial;

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
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.panelSuperior = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabImportar = new System.Windows.Forms.TabPage();
            this.tabExportar = new System.Windows.Forms.TabPage();
            this.tabPlantillas = new System.Windows.Forms.TabPage();
            this.tabHistorial = new System.Windows.Forms.TabPage();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tsslEstado = new System.Windows.Forms.ToolStripStatusLabel();

            // Controles de Importar
            this.gpbTipoImportacion = new System.Windows.Forms.GroupBox();
            this.cboTipoImportacion = new System.Windows.Forms.ComboBox();
            this.gpbArchivo = new System.Windows.Forms.GroupBox();
            this.txtRutaArchivo = new System.Windows.Forms.TextBox();
            this.btnExaminar = new System.Windows.Forms.Button();
            this.gpbOpciones = new System.Windows.Forms.GroupBox();
            this.chkPrimeraFilaEncabezados = new System.Windows.Forms.CheckBox();
            this.chkActualizarExistentes = new System.Windows.Forms.CheckBox();
            this.chkCrearNuevos = new System.Windows.Forms.CheckBox();
            this.chkValidarDatos = new System.Windows.Forms.CheckBox();
            this.gpbMapeoCampos = new System.Windows.Forms.GroupBox();
            this.dgvMapeo = new System.Windows.Forms.DataGridView();
            this.btnMapeoAutomatico = new System.Windows.Forms.Button();
            this.btnLimpiarMapeo = new System.Windows.Forms.Button();
            this.gpbVistaPrevia = new System.Windows.Forms.GroupBox();
            this.dgvVistaPrevia = new System.Windows.Forms.DataGridView();
            this.btnCargarVistaPrevia = new System.Windows.Forms.Button();
            this.lblRegistrosEncontrados = new System.Windows.Forms.Label();
            this.lblRegistrosEncontradosValor = new System.Windows.Forms.Label();
            this.lblRegistrosValidos = new System.Windows.Forms.Label();
            this.lblRegistrosValidosValor = new System.Windows.Forms.Label();
            this.lblRegistrosConError = new System.Windows.Forms.Label();
            this.lblRegistrosConErrorValor = new System.Windows.Forms.Label();
            this.progressBarImportacion = new System.Windows.Forms.ProgressBar();
            this.btnImportar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();

            // Controles de Exportar
            this.gpbTipoExportacion = new System.Windows.Forms.GroupBox();
            this.cboTipoExportacion = new System.Windows.Forms.ComboBox();
            this.gpbFormatoExportacion = new System.Windows.Forms.GroupBox();
            this.rdbExcel = new System.Windows.Forms.RadioButton();
            this.rdbCSV = new System.Windows.Forms.RadioButton();
            this.rdbXML = new System.Windows.Forms.RadioButton();
            this.rdbJSON = new System.Windows.Forms.RadioButton();
            this.gpbFiltrosExportacion = new System.Windows.Forms.GroupBox();
            this.chkExportarActivos = new System.Windows.Forms.CheckBox();
            this.chkExportarInactivos = new System.Windows.Forms.CheckBox();
            this.cboExportarCategoria = new System.Windows.Forms.ComboBox();
            this.cboExportarMarca = new System.Windows.Forms.ComboBox();
            this.lblExportarDesde = new System.Windows.Forms.Label();
            this.dtpExportarDesde = new System.Windows.Forms.DateTimePicker();
            this.lblExportarHasta = new System.Windows.Forms.Label();
            this.dtpExportarHasta = new System.Windows.Forms.DateTimePicker();
            this.gpbCamposExportar = new System.Windows.Forms.GroupBox();
            this.clbCamposExportar = new System.Windows.Forms.CheckedListBox();
            this.btnSeleccionarTodos = new System.Windows.Forms.Button();
            this.btnDeseleccionarTodos = new System.Windows.Forms.Button();
            this.btnExportar = new System.Windows.Forms.Button();

            // Controles de Plantillas
            this.lvPlantillas = new System.Windows.Forms.ListView();
            this.chPlantillaNombre = new System.Windows.Forms.ColumnHeader();
            this.chPlantillaTipo = new System.Windows.Forms.ColumnHeader();
            this.chPlantillaFecha = new System.Windows.Forms.ColumnHeader();
            this.chPlantillaDescripcion = new System.Windows.Forms.ColumnHeader();
            this.btnDescargarPlantilla = new System.Windows.Forms.Button();
            this.btnVerPlantilla = new System.Windows.Forms.Button();
            this.gbpVistaPreviaPlantilla = new System.Windows.Forms.GroupBox();
            this.dgvVistaPreviaPlantilla = new System.Windows.Forms.DataGridView();

            // Controles de Historial
            this.dgvHistorial = new System.Windows.Forms.DataGridView();
            this.btnVerDetalle = new System.Windows.Forms.Button();
            this.btnRepetirImportacion = new System.Windows.Forms.Button();
            this.btnEliminarHistorial = new System.Windows.Forms.Button();
            this.cboFiltroHistorial = new System.Windows.Forms.ComboBox();
            this.dtpHistorialDesde = new System.Windows.Forms.DateTimePicker();
            this.dtpHistorialHasta = new System.Windows.Forms.DateTimePicker();
            this.btnFiltrarHistorial = new System.Windows.Forms.Button();

            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();

            this.tlpMain.SuspendLayout();
            this.panelSuperior.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabImportar.SuspendLayout();
            this.tabExportar.SuspendLayout();
            this.tabPlantillas.SuspendLayout();
            this.tabHistorial.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();

            // tlpMain
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.panelSuperior, 0, 0);
            this.tlpMain.Controls.Add(this.tabControl, 0, 1);
            this.tlpMain.Controls.Add(this.statusStrip, 0, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpMain.Size = new System.Drawing.Size(1100, 700);
            this.tlpMain.TabIndex = 0;

            // panelSuperior
            this.panelSuperior.BackColor = System.Drawing.Color.FromArgb(30, 60, 114);
            this.panelSuperior.Controls.Add(this.lblTitulo);
            this.panelSuperior.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSuperior.Location = new System.Drawing.Point(3, 3);
            this.panelSuperior.Name = "panelSuperior";
            this.panelSuperior.Size = new System.Drawing.Size(1094, 44);
            this.panelSuperior.TabIndex = 0;

            // lblTitulo
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(1094, 44);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "IMPORTACIÓN / EXPORTACIÓN DE DATOS";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // tabControl
            this.tabControl.Controls.Add(this.tabImportar);
            this.tabControl.Controls.Add(this.tabExportar);
            this.tabControl.Controls.Add(this.tabPlantillas);
            this.tabControl.Controls.Add(this.tabHistorial);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(3, 53);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1094, 619);
            this.tabControl.TabIndex = 1;

            // ==================== TAB IMPORTAR ====================
            this.tabImportar.BackColor = System.Drawing.Color.White;
            this.tabImportar.Controls.Add(this.gpbTipoImportacion);
            this.tabImportar.Controls.Add(this.gpbArchivo);
            this.tabImportar.Controls.Add(this.gpbOpciones);
            this.tabImportar.Controls.Add(this.gpbMapeoCampos);
            this.tabImportar.Controls.Add(this.gpbVistaPrevia);
            this.tabImportar.Controls.Add(this.progressBarImportacion);
            this.tabImportar.Controls.Add(this.btnImportar);
            this.tabImportar.Controls.Add(this.btnCancelar);
            this.tabImportar.Location = new System.Drawing.Point(4, 24);
            this.tabImportar.Name = "tabImportar";
            this.tabImportar.Padding = new System.Windows.Forms.Padding(10);
            this.tabImportar.Size = new System.Drawing.Size(1086, 591);
            this.tabImportar.TabIndex = 0;
            this.tabImportar.Text = "Importar Datos";

            // gpbTipoImportacion
            this.gpbTipoImportacion.Controls.Add(this.cboTipoImportacion);
            this.gpbTipoImportacion.Location = new System.Drawing.Point(13, 13);
            this.gpbTipoImportacion.Name = "gpbTipoImportacion";
            this.gpbTipoImportacion.Size = new System.Drawing.Size(1060, 60);
            this.gpbTipoImportacion.TabIndex = 0;
            this.gpbTipoImportacion.TabStop = false;
            this.gpbTipoImportacion.Text = "Tipo de Datos a Importar";

            this.cboTipoImportacion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboTipoImportacion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTipoImportacion.FormattingEnabled = true;
            this.cboTipoImportacion.Items.AddRange(new object[] {
                "Productos",
                "Clientes",
                "Proveedores",
                "Precios",
                "Stock Inicial",
                "Categorías",
                "Marcas",
                "Unidades de Medida"
            });
            this.cboTipoImportacion.Location = new System.Drawing.Point(3, 19);
            this.cboTipoImportacion.Name = "cboTipoImportacion";
            this.cboTipoImportacion.Size = new System.Drawing.Size(1054, 23);
            this.cboTipoImportacion.TabIndex = 0;

            // gpbArchivo
            this.gpbArchivo.Controls.Add(this.txtRutaArchivo);
            this.gpbArchivo.Controls.Add(this.btnExaminar);
            this.gpbArchivo.Location = new System.Drawing.Point(13, 79);
            this.gpbArchivo.Name = "gpbArchivo";
            this.gpbArchivo.Size = new System.Drawing.Size(1060, 60);
            this.gpbArchivo.TabIndex = 1;
            this.gpbArchivo.TabStop = false;
            this.gpbArchivo.Text = "Archivo de Origen";

            this.txtRutaArchivo.Location = new System.Drawing.Point(6, 22);
            this.txtRutaArchivo.Name = "txtRutaArchivo";
            this.txtRutaArchivo.ReadOnly = true;
            this.txtRutaArchivo.Size = new System.Drawing.Size(940, 23);
            this.txtRutaArchivo.TabIndex = 0;

            this.btnExaminar.Location = new System.Drawing.Point(952, 20);
            this.btnExaminar.Name = "btnExaminar";
            this.btnExaminar.Size = new System.Drawing.Size(100, 27);
            this.btnExaminar.TabIndex = 1;
            this.btnExaminar.Text = "Examinar...";
            this.btnExaminar.UseVisualStyleBackColor = true;

            // gpbOpciones
            this.gpbOpciones.Controls.Add(this.chkPrimeraFilaEncabezados);
            this.gpbOpciones.Controls.Add(this.chkActualizarExistentes);
            this.gpbOpciones.Controls.Add(this.chkCrearNuevos);
            this.gpbOpciones.Controls.Add(this.chkValidarDatos);
            this.gpbOpciones.Location = new System.Drawing.Point(13, 145);
            this.gpbOpciones.Name = "gpbOpciones";
            this.gpbOpciones.Size = new System.Drawing.Size(1060, 60);
            this.gpbOpciones.TabIndex = 2;
            this.gpbOpciones.TabStop = false;
            this.gpbOpciones.Text = "Opciones de Importación";

            this.chkPrimeraFilaEncabezados.AutoSize = true;
            this.chkPrimeraFilaEncabezados.Checked = true;
            this.chkPrimeraFilaEncabezados.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPrimeraFilaEncabezados.Location = new System.Drawing.Point(10, 25);
            this.chkPrimeraFilaEncabezados.Name = "chkPrimeraFilaEncabezados";
            this.chkPrimeraFilaEncabezados.Size = new System.Drawing.Size(168, 19);
            this.chkPrimeraFilaEncabezados.TabIndex = 0;
            this.chkPrimeraFilaEncabezados.Text = "Primera fila son encabezados";
            this.chkPrimeraFilaEncabezados.UseVisualStyleBackColor = true;

            this.chkActualizarExistentes.AutoSize = true;
            this.chkActualizarExistentes.Checked = true;
            this.chkActualizarExistentes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkActualizarExistentes.Location = new System.Drawing.Point(190, 25);
            this.chkActualizarExistentes.Name = "chkActualizarExistentes";
            this.chkActualizarExistentes.Size = new System.Drawing.Size(134, 19);
            this.chkActualizarExistentes.TabIndex = 1;
            this.chkActualizarExistentes.Text = "Actualizar existentes";
            this.chkActualizarExistentes.UseVisualStyleBackColor = true;

            this.chkCrearNuevos.AutoSize = true;
            this.chkCrearNuevos.Checked = true;
            this.chkCrearNuevos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCrearNuevos.Location = new System.Drawing.Point(340, 25);
            this.chkCrearNuevos.Name = "chkCrearNuevos";
            this.chkCrearNuevos.Size = new System.Drawing.Size(101, 19);
            this.chkCrearNuevos.TabIndex = 2;
            this.chkCrearNuevos.Text = "Crear nuevos";
            this.chkCrearNuevos.UseVisualStyleBackColor = true;

            this.chkValidarDatos.AutoSize = true;
            this.chkValidarDatos.Checked = true;
            this.chkValidarDatos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkValidarDatos.Location = new System.Drawing.Point(460, 25);
            this.chkValidarDatos.Name = "chkValidarDatos";
            this.chkValidarDatos.Size = new System.Drawing.Size(102, 19);
            this.chkValidarDatos.TabIndex = 3;
            this.chkValidarDatos.Text = "Validar datos";
            this.chkValidarDatos.UseVisualStyleBackColor = true;

            // gpbMapeoCampos
            this.gpbMapeoCampos.Controls.Add(this.dgvMapeo);
            this.gpbMapeoCampos.Controls.Add(this.btnMapeoAutomatico);
            this.gpbMapeoCampos.Controls.Add(this.btnLimpiarMapeo);
            this.gpbMapeoCampos.Location = new System.Drawing.Point(13, 211);
            this.gpbMapeoCampos.Name = "gpbMapeoCampos";
            this.gpbMapeoCampos.Size = new System.Drawing.Size(1060, 140);
            this.gpbMapeoCampos.TabIndex = 3;
            this.gpbMapeoCampos.TabStop = false;
            this.gpbMapeoCampos.Text = "Mapeo de Campos";

            this.dgvMapeo.AllowUserToAddRows = false;
            this.dgvMapeo.AllowUserToDeleteRows = false;
            this.dgvMapeo.BackgroundColor = System.Drawing.Color.White;
            this.dgvMapeo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMapeo.Location = new System.Drawing.Point(6, 22);
            this.dgvMapeo.Name = "dgvMapeo";
            this.dgvMapeo.RowHeadersVisible = false;
            this.dgvMapeo.Size = new System.Drawing.Size(940, 80);
            this.dgvMapeo.TabIndex = 0;

            this.btnMapeoAutomatico.Location = new System.Drawing.Point(952, 22);
            this.btnMapeoAutomatico.Name = "btnMapeoAutomatico";
            this.btnMapeoAutomatico.Size = new System.Drawing.Size(100, 27);
            this.btnMapeoAutomatico.TabIndex = 1;
            this.btnMapeoAutomatico.Text = "Mapeo Auto";
            this.btnMapeoAutomatico.UseVisualStyleBackColor = true;

            this.btnLimpiarMapeo.Location = new System.Drawing.Point(952, 55);
            this.btnLimpiarMapeo.Name = "btnLimpiarMapeo";
            this.btnLimpiarMapeo.Size = new System.Drawing.Size(100, 27);
            this.btnLimpiarMapeo.TabIndex = 2;
            this.btnLimpiarMapeo.Text = "Limpiar";
            this.btnLimpiarMapeo.UseVisualStyleBackColor = true;

            // gpbVistaPrevia
            this.gpbVistaPrevia.Controls.Add(this.dgvVistaPrevia);
            this.gpbVistaPrevia.Controls.Add(this.btnCargarVistaPrevia);
            this.gpbVistaPrevia.Controls.Add(this.lblRegistrosEncontrados);
            this.gpbVistaPrevia.Controls.Add(this.lblRegistrosEncontradosValor);
            this.gpbVistaPrevia.Controls.Add(this.lblRegistrosValidos);
            this.gpbVistaPrevia.Controls.Add(this.lblRegistrosValidosValor);
            this.gpbVistaPrevia.Controls.Add(this.lblRegistrosConError);
            this.gpbVistaPrevia.Controls.Add(this.lblRegistrosConErrorValor);
            this.gpbVistaPrevia.Location = new System.Drawing.Point(13, 357);
            this.gpbVistaPrevia.Name = "gpbVistaPrevia";
            this.gpbVistaPrevia.Size = new System.Drawing.Size(1060, 150);
            this.gpbVistaPrevia.TabIndex = 4;
            this.gpbVistaPrevia.TabStop = false;
            this.gpbVistaPrevia.Text = "Vista Previa de Datos";

            this.dgvVistaPrevia.AllowUserToAddRows = false;
            this.dgvVistaPrevia.AllowUserToDeleteRows = false;
            this.dgvVistaPrevia.BackgroundColor = System.Drawing.Color.White;
            this.dgvVistaPrevia.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVistaPrevia.Location = new System.Drawing.Point(6, 22);
            this.dgvVistaPrevia.Name = "dgvVistaPrevia";
            this.dgvVistaPrevia.ReadOnly = true;
            this.dgvVistaPrevia.RowHeadersVisible = false;
            this.dgvVistaPrevia.Size = new System.Drawing.Size(780, 90);
            this.dgvVistaPrevia.TabIndex = 0;

            this.btnCargarVistaPrevia.BackColor = System.Drawing.Color.FromArgb(30, 60, 114);
            this.btnCargarVistaPrevia.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargarVistaPrevia.ForeColor = System.Drawing.Color.White;
            this.btnCargarVistaPrevia.Location = new System.Drawing.Point(800, 22);
            this.btnCargarVistaPrevia.Name = "btnCargarVistaPrevia";
            this.btnCargarVistaPrevia.Size = new System.Drawing.Size(120, 30);
            this.btnCargarVistaPrevia.TabIndex = 1;
            this.btnCargarVistaPrevia.Text = "Cargar Vista Previa";
            this.btnCargarVistaPrevia.UseVisualStyleBackColor = false;

            this.lblRegistrosEncontrados.AutoSize = true;
            this.lblRegistrosEncontrados.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRegistrosEncontrados.Location = new System.Drawing.Point(800, 65);
            this.lblRegistrosEncontrados.Name = "lblRegistrosEncontrados";
            this.lblRegistrosEncontrados.Size = new System.Drawing.Size(130, 15);
            this.lblRegistrosEncontrados.TabIndex = 2;
            this.lblRegistrosEncontrados.Text = "Registros encontrados:";

            this.lblRegistrosEncontradosValor.AutoSize = true;
            this.lblRegistrosEncontradosValor.ForeColor = System.Drawing.Color.FromArgb(30, 60, 114);
            this.lblRegistrosEncontradosValor.Location = new System.Drawing.Point(930, 65);
            this.lblRegistrosEncontradosValor.Name = "lblRegistrosEncontradosValor";
            this.lblRegistrosEncontradosValor.Size = new System.Drawing.Size(13, 15);
            this.lblRegistrosEncontradosValor.TabIndex = 3;
            this.lblRegistrosEncontradosValor.Text = "0";

            this.lblRegistrosValidos.AutoSize = true;
            this.lblRegistrosValidos.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRegistrosValidos.Location = new System.Drawing.Point(800, 90);
            this.lblRegistrosValidos.Name = "lblRegistrosValidos";
            this.lblRegistrosValidos.Size = new System.Drawing.Size(94, 15);
            this.lblRegistrosValidos.TabIndex = 4;
            this.lblRegistrosValidos.Text = "Registros válidos:";

            this.lblRegistrosValidosValor.AutoSize = true;
            this.lblRegistrosValidosValor.ForeColor = System.Drawing.Color.Green;
            this.lblRegistrosValidosValor.Location = new System.Drawing.Point(900, 90);
            this.lblRegistrosValidosValor.Name = "lblRegistrosValidosValor";
            this.lblRegistrosValidosValor.Size = new System.Drawing.Size(13, 15);
            this.lblRegistrosValidosValor.TabIndex = 5;
            this.lblRegistrosValidosValor.Text = "0";

            this.lblRegistrosConError.AutoSize = true;
            this.lblRegistrosConError.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRegistrosConError.Location = new System.Drawing.Point(800, 115);
            this.lblRegistrosConError.Name = "lblRegistrosConError";
            this.lblRegistrosConError.Size = new System.Drawing.Size(108, 15);
            this.lblRegistrosConError.TabIndex = 6;
            this.lblRegistrosConError.Text = "Registros con error:";

            this.lblRegistrosConErrorValor.AutoSize = true;
            this.lblRegistrosConErrorValor.ForeColor = System.Drawing.Color.Red;
            this.lblRegistrosConErrorValor.Location = new System.Drawing.Point(910, 115);
            this.lblRegistrosConErrorValor.Name = "lblRegistrosConErrorValor";
            this.lblRegistrosConErrorValor.Size = new System.Drawing.Size(13, 15);
            this.lblRegistrosConErrorValor.TabIndex = 7;
            this.lblRegistrosConErrorValor.Text = "0";

            // progressBarImportacion
            this.progressBarImportacion.Location = new System.Drawing.Point(13, 513);
            this.progressBarImportacion.Name = "progressBarImportacion";
            this.progressBarImportacion.Size = new System.Drawing.Size(860, 23);
            this.progressBarImportacion.TabIndex = 5;
            this.progressBarImportacion.Visible = false;

            // btnImportar
            this.btnImportar.BackColor = System.Drawing.Color.FromArgb(30, 60, 114);
            this.btnImportar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImportar.ForeColor = System.Drawing.Color.White;
            this.btnImportar.Location = new System.Drawing.Point(970, 513);
            this.btnImportar.Name = "btnImportar";
            this.btnImportar.Size = new System.Drawing.Size(100, 35);
            this.btnImportar.TabIndex = 6;
            this.btnImportar.Text = "Importar";
            this.btnImportar.UseVisualStyleBackColor = false;

            // btnCancelar
            this.btnCancelar.Location = new System.Drawing.Point(970, 554);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(100, 27);
            this.btnCancelar.TabIndex = 7;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;

            // ==================== TAB EXPORTAR ====================
            this.tabExportar.BackColor = System.Drawing.Color.White;
            this.tabExportar.Controls.Add(this.gpbTipoExportacion);
            this.tabExportar.Controls.Add(this.gpbFormatoExportacion);
            this.tabExportar.Controls.Add(this.gpbFiltrosExportacion);
            this.tabExportar.Controls.Add(this.gpbCamposExportar);
            this.tabExportar.Controls.Add(this.btnExportar);
            this.tabExportar.Location = new System.Drawing.Point(4, 24);
            this.tabExportar.Name = "tabExportar";
            this.tabExportar.Padding = new System.Windows.Forms.Padding(10);
            this.tabExportar.Size = new System.Drawing.Size(1086, 591);
            this.tabExportar.TabIndex = 1;
            this.tabExportar.Text = "Exportar Datos";

            // gpbTipoExportacion
            this.gpbTipoExportacion.Controls.Add(this.cboTipoExportacion);
            this.gpbTipoExportacion.Location = new System.Drawing.Point(13, 13);
            this.gpbTipoExportacion.Name = "gpbTipoExportacion";
            this.gpbTipoExportacion.Size = new System.Drawing.Size(1060, 60);
            this.gpbTipoExportacion.TabIndex = 0;
            this.gpbTipoExportacion.TabStop = false;
            this.gpbTipoExportacion.Text = "Tipo de Datos a Exportar";

            this.cboTipoExportacion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboTipoExportacion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTipoExportacion.FormattingEnabled = true;
            this.cboTipoExportacion.Items.AddRange(new object[] {
                "Productos",
                "Clientes",
                "Proveedores",
                "Precios",
                "Stock Actual",
                "Movimientos de Inventario",
                "Ventas",
                "Compras"
            });
            this.cboTipoExportacion.Location = new System.Drawing.Point(3, 19);
            this.cboTipoExportacion.Name = "cboTipoExportacion";
            this.cboTipoExportacion.Size = new System.Drawing.Size(1054, 23);
            this.cboTipoExportacion.TabIndex = 0;

            // gpbFormatoExportacion
            this.gpbFormatoExportacion.Controls.Add(this.rdbExcel);
            this.gpbFormatoExportacion.Controls.Add(this.rdbCSV);
            this.gpbFormatoExportacion.Controls.Add(this.rdbXML);
            this.gpbFormatoExportacion.Controls.Add(this.rdbJSON);
            this.gpbFormatoExportacion.Location = new System.Drawing.Point(13, 79);
            this.gpbFormatoExportacion.Name = "gpbFormatoExportacion";
            this.gpbFormatoExportacion.Size = new System.Drawing.Size(1060, 60);
            this.gpbFormatoExportacion.TabIndex = 1;
            this.gpbFormatoExportacion.TabStop = false;
            this.gpbFormatoExportacion.Text = "Formato de Exportación";

            this.rdbExcel.AutoSize = true;
            this.rdbExcel.Checked = true;
            this.rdbExcel.Location = new System.Drawing.Point(20, 25);
            this.rdbExcel.Name = "rdbExcel";
            this.rdbExcel.Size = new System.Drawing.Size(61, 19);
            this.rdbExcel.TabIndex = 0;
            this.rdbExcel.TabStop = true;
            this.rdbExcel.Text = "Excel";
            this.rdbExcel.UseVisualStyleBackColor = true;

            this.rdbCSV.AutoSize = true;
            this.rdbCSV.Location = new System.Drawing.Point(120, 25);
            this.rdbCSV.Name = "rdbCSV";
            this.rdbCSV.Size = new System.Drawing.Size(46, 19);
            this.rdbCSV.TabIndex = 1;
            this.rdbCSV.Text = "CSV";
            this.rdbCSV.UseVisualStyleBackColor = true;

            this.rdbXML.AutoSize = true;
            this.rdbXML.Location = new System.Drawing.Point(210, 25);
            this.rdbXML.Name = "rdbXML";
            this.rdbXML.Size = new System.Drawing.Size(47, 19);
            this.rdbXML.TabIndex = 2;
            this.rdbXML.Text = "XML";
            this.rdbXML.UseVisualStyleBackColor = true;

            this.rdbJSON.AutoSize = true;
            this.rdbJSON.Location = new System.Drawing.Point(300, 25);
            this.rdbJSON.Name = "rdbJSON";
            this.rdbJSON.Size = new System.Drawing.Size(53, 19);
            this.rdbJSON.TabIndex = 3;
            this.rdbJSON.Text = "JSON";
            this.rdbJSON.UseVisualStyleBackColor = true;

            // gpbFiltrosExportacion
            this.gpbFiltrosExportacion.Controls.Add(this.chkExportarActivos);
            this.gpbFiltrosExportacion.Controls.Add(this.chkExportarInactivos);
            this.gpbFiltrosExportacion.Controls.Add(this.cboExportarCategoria);
            this.gpbFiltrosExportacion.Controls.Add(this.cboExportarMarca);
            this.gpbFiltrosExportacion.Controls.Add(this.lblExportarDesde);
            this.gpbFiltrosExportacion.Controls.Add(this.dtpExportarDesde);
            this.gpbFiltrosExportacion.Controls.Add(this.lblExportarHasta);
            this.gpbFiltrosExportacion.Controls.Add(this.dtpExportarHasta);
            this.gpbFiltrosExportacion.Location = new System.Drawing.Point(13, 145);
            this.gpbFiltrosExportacion.Name = "gpbFiltrosExportacion";
            this.gpbFiltrosExportacion.Size = new System.Drawing.Size(1060, 100);
            this.gpbFiltrosExportacion.TabIndex = 2;
            this.gpbFiltrosExportacion.TabStop = false;
            this.gpbFiltrosExportacion.Text = "Filtros";

            this.chkExportarActivos.AutoSize = true;
            this.chkExportarActivos.Checked = true;
            this.chkExportarActivos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportarActivos.Location = new System.Drawing.Point(20, 25);
            this.chkExportarActivos.Name = "chkExportarActivos";
            this.chkExportarActivos.Size = new System.Drawing.Size(96, 19);
            this.chkExportarActivos.TabIndex = 0;
            this.chkExportarActivos.Text = "Solo activos";
            this.chkExportarActivos.UseVisualStyleBackColor = true;

            this.chkExportarInactivos.AutoSize = true;
            this.chkExportarInactivos.Location = new System.Drawing.Point(130, 25);
            this.chkExportarInactivos.Name = "chkExportarInactivos";
            this.chkExportarInactivos.Size = new System.Drawing.Size(106, 19);
            this.chkExportarInactivos.TabIndex = 1;
            this.chkExportarInactivos.Text = "Incluir inactivos";
            this.chkExportarInactivos.UseVisualStyleBackColor = true;

            this.cboExportarCategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboExportarCategoria.FormattingEnabled = true;
            this.cboExportarCategoria.Items.AddRange(new object[] { "Todas las categorías", "Electrónica", "Computación", "Oficina" });
            this.cboExportarCategoria.Location = new System.Drawing.Point(20, 55);
            this.cboExportarCategoria.Name = "cboExportarCategoria";
            this.cboExportarCategoria.Size = new System.Drawing.Size(200, 23);
            this.cboExportarCategoria.TabIndex = 2;

            this.cboExportarMarca.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboExportarMarca.FormattingEnabled = true;
            this.cboExportarMarca.Items.AddRange(new object[] { "Todas las marcas", "Samsung", "HP", "Logitech" });
            this.cboExportarMarca.Location = new System.Drawing.Point(240, 55);
            this.cboExportarMarca.Name = "cboExportarMarca";
            this.cboExportarMarca.Size = new System.Drawing.Size(200, 23);
            this.cboExportarMarca.TabIndex = 3;

            this.lblExportarDesde.AutoSize = true;
            this.lblExportarDesde.Location = new System.Drawing.Point(460, 35);
            this.lblExportarDesde.Name = "lblExportarDesde";
            this.lblExportarDesde.Size = new System.Drawing.Size(41, 15);
            this.lblExportarDesde.TabIndex = 4;
            this.lblExportarDesde.Text = "Desde:";

            this.dtpExportarDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpExportarDesde.Location = new System.Drawing.Point(460, 55);
            this.dtpExportarDesde.Name = "dtpExportarDesde";
            this.dtpExportarDesde.Size = new System.Drawing.Size(120, 23);
            this.dtpExportarDesde.TabIndex = 5;

            this.lblExportarHasta.AutoSize = true;
            this.lblExportarHasta.Location = new System.Drawing.Point(600, 35);
            this.lblExportarHasta.Name = "lblExportarHasta";
            this.lblExportarHasta.Size = new System.Drawing.Size(38, 15);
            this.lblExportarHasta.TabIndex = 6;
            this.lblExportarHasta.Text = "Hasta:";

            this.dtpExportarHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpExportarHasta.Location = new System.Drawing.Point(600, 55);
            this.dtpExportarHasta.Name = "dtpExportarHasta";
            this.dtpExportarHasta.Size = new System.Drawing.Size(120, 23);
            this.dtpExportarHasta.TabIndex = 7;

            // gpbCamposExportar
            this.gpbCamposExportar.Controls.Add(this.clbCamposExportar);
            this.gpbCamposExportar.Controls.Add(this.btnSeleccionarTodos);
            this.gpbCamposExportar.Controls.Add(this.btnDeseleccionarTodos);
            this.gpbCamposExportar.Location = new System.Drawing.Point(13, 251);
            this.gpbCamposExportar.Name = "gpbCamposExportar";
            this.gpbCamposExportar.Size = new System.Drawing.Size(1060, 240);
            this.gpbCamposExportar.TabIndex = 3;
            this.gpbCamposExportar.TabStop = false;
            this.gpbCamposExportar.Text = "Campos a Exportar";

            this.clbCamposExportar.CheckOnClick = true;
            this.clbCamposExportar.FormattingEnabled = true;
            this.clbCamposExportar.Items.AddRange(new object[] {
                "Código",
                "Nombre",
                "Categoría",
                "Marca",
                "Unidad Base",
                "Precio Venta",
                "Precio Compra",
                "Stock Actual",
                "Stock Mínimo",
                "Fecha Creación",
                "Activo"
            });
            this.clbCamposExportar.Location = new System.Drawing.Point(6, 22);
            this.clbCamposExportar.Name = "clbCamposExportar";
            this.clbCamposExportar.Size = new System.Drawing.Size(940, 184);
            this.clbCamposExportar.TabIndex = 0;

            this.btnSeleccionarTodos.Location = new System.Drawing.Point(952, 22);
            this.btnSeleccionarTodos.Name = "btnSeleccionarTodos";
            this.btnSeleccionarTodos.Size = new System.Drawing.Size(100, 27);
            this.btnSeleccionarTodos.TabIndex = 1;
            this.btnSeleccionarTodos.Text = "Seleccionar Todos";
            this.btnSeleccionarTodos.UseVisualStyleBackColor = true;

            this.btnDeseleccionarTodos.Location = new System.Drawing.Point(952, 55);
            this.btnDeseleccionarTodos.Name = "btnDeseleccionarTodos";
            this.btnDeseleccionarTodos.Size = new System.Drawing.Size(100, 27);
            this.btnDeseleccionarTodos.TabIndex = 2;
            this.btnDeseleccionarTodos.Text = "Deseleccionar Todos";
            this.btnDeseleccionarTodos.UseVisualStyleBackColor = true;

            // btnExportar
            this.btnExportar.BackColor = System.Drawing.Color.FromArgb(30, 60, 114);
            this.btnExportar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportar.ForeColor = System.Drawing.Color.White;
            this.btnExportar.Location = new System.Drawing.Point(970, 500);
            this.btnExportar.Name = "btnExportar";
            this.btnExportar.Size = new System.Drawing.Size(100, 35);
            this.btnExportar.TabIndex = 4;
            this.btnExportar.Text = "Exportar";
            this.btnExportar.UseVisualStyleBackColor = false;

            // ==================== TAB PLANTILLAS ====================
            this.tabPlantillas.BackColor = System.Drawing.Color.White;
            this.tabPlantillas.Controls.Add(this.lvPlantillas);
            this.tabPlantillas.Controls.Add(this.btnDescargarPlantilla);
            this.tabPlantillas.Controls.Add(this.btnVerPlantilla);
            this.tabPlantillas.Controls.Add(this.gbpVistaPreviaPlantilla);
            this.tabPlantillas.Location = new System.Drawing.Point(4, 24);
            this.tabPlantillas.Name = "tabPlantillas";
            this.tabPlantillas.Padding = new System.Windows.Forms.Padding(10);
            this.tabPlantillas.Size = new System.Drawing.Size(1086, 591);
            this.tabPlantillas.TabIndex = 2;
            this.tabPlantillas.Text = "Plantillas";

            this.lvPlantillas.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.chPlantillaNombre,
                this.chPlantillaTipo,
                this.chPlantillaFecha,
                this.chPlantillaDescripcion
            });
            this.lvPlantillas.FullRowSelect = true;
            this.lvPlantillas.GridLines = true;
            this.lvPlantillas.Location = new System.Drawing.Point(13, 13);
            this.lvPlantillas.Name = "lvPlantillas";
            this.lvPlantillas.Size = new System.Drawing.Size(800, 200);
            this.lvPlantillas.TabIndex = 0;
            this.lvPlantillas.UseCompatibleStateImageBehavior = false;
            this.lvPlantillas.View = System.Windows.Forms.View.Details;

            this.chPlantillaNombre.Text = "Nombre";
            this.chPlantillaNombre.Width = 200;
            this.chPlantillaTipo.Text = "Tipo";
            this.chPlantillaTipo.Width = 150;
            this.chPlantillaFecha.Text = "Fecha";
            this.chPlantillaFecha.Width = 120;
            this.chPlantillaDescripcion.Text = "Descripción";
            this.chPlantillaDescripcion.Width = 300;

            this.btnDescargarPlantilla.Location = new System.Drawing.Point(830, 13);
            this.btnDescargarPlantilla.Name = "btnDescargarPlantilla";
            this.btnDescargarPlantilla.Size = new System.Drawing.Size(120, 30);
            this.btnDescargarPlantilla.TabIndex = 1;
            this.btnDescargarPlantilla.Text = "Descargar Plantilla";
            this.btnDescargarPlantilla.UseVisualStyleBackColor = true;

            this.btnVerPlantilla.Location = new System.Drawing.Point(830, 49);
            this.btnVerPlantilla.Name = "btnVerPlantilla";
            this.btnVerPlantilla.Size = new System.Drawing.Size(120, 30);
            this.btnVerPlantilla.TabIndex = 2;
            this.btnVerPlantilla.Text = "Ver Plantilla";
            this.btnVerPlantilla.UseVisualStyleBackColor = true;

            this.gbpVistaPreviaPlantilla.Controls.Add(this.dgvVistaPreviaPlantilla);
            this.gbpVistaPreviaPlantilla.Location = new System.Drawing.Point(13, 230);
            this.gbpVistaPreviaPlantilla.Name = "gbpVistaPreviaPlantilla";
            this.gbpVistaPreviaPlantilla.Size = new System.Drawing.Size(1060, 350);
            this.gbpVistaPreviaPlantilla.TabIndex = 3;
            this.gbpVistaPreviaPlantilla.TabStop = false;
            this.gbpVistaPreviaPlantilla.Text = "Vista Previa de la Plantilla";

            this.dgvVistaPreviaPlantilla.AllowUserToAddRows = false;
            this.dgvVistaPreviaPlantilla.AllowUserToDeleteRows = false;
            this.dgvVistaPreviaPlantilla.BackgroundColor = System.Drawing.Color.White;
            this.dgvVistaPreviaPlantilla.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVistaPreviaPlantilla.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvVistaPreviaPlantilla.Location = new System.Drawing.Point(3, 19);
            this.dgvVistaPreviaPlantilla.Name = "dgvVistaPreviaPlantilla";
            this.dgvVistaPreviaPlantilla.ReadOnly = true;
            this.dgvVistaPreviaPlantilla.RowHeadersVisible = false;
            this.dgvVistaPreviaPlantilla.Size = new System.Drawing.Size(1054, 328);
            this.dgvVistaPreviaPlantilla.TabIndex = 0;

            // ==================== TAB HISTORIAL ====================
            this.tabHistorial.BackColor = System.Drawing.Color.White;
            this.tabHistorial.Controls.Add(this.dgvHistorial);
            this.tabHistorial.Controls.Add(this.btnVerDetalle);
            this.tabHistorial.Controls.Add(this.btnRepetirImportacion);
            this.tabHistorial.Controls.Add(this.btnEliminarHistorial);
            this.tabHistorial.Controls.Add(this.cboFiltroHistorial);
            this.tabHistorial.Controls.Add(this.dtpHistorialDesde);
            this.tabHistorial.Controls.Add(this.dtpHistorialHasta);
            this.tabHistorial.Controls.Add(this.btnFiltrarHistorial);
            this.tabHistorial.Location = new System.Drawing.Point(4, 24);
            this.tabHistorial.Name = "tabHistorial";
            this.tabHistorial.Padding = new System.Windows.Forms.Padding(10);
            this.tabHistorial.Size = new System.Drawing.Size(1086, 591);
            this.tabHistorial.TabIndex = 3;
            this.tabHistorial.Text = "Historial";

            this.cboFiltroHistorial.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFiltroHistorial.FormattingEnabled = true;
            this.cboFiltroHistorial.Items.AddRange(new object[] { "Todos", "Importaciones", "Exportaciones" });
            this.cboFiltroHistorial.Location = new System.Drawing.Point(13, 13);
            this.cboFiltroHistorial.Name = "cboFiltroHistorial";
            this.cboFiltroHistorial.Size = new System.Drawing.Size(150, 23);
            this.cboFiltroHistorial.TabIndex = 0;

            this.dtpHistorialDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpHistorialDesde.Location = new System.Drawing.Point(180, 13);
            this.dtpHistorialDesde.Name = "dtpHistorialDesde";
            this.dtpHistorialDesde.Size = new System.Drawing.Size(100, 23);
            this.dtpHistorialDesde.TabIndex = 1;

            this.dtpHistorialHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpHistorialHasta.Location = new System.Drawing.Point(300, 13);
            this.dtpHistorialHasta.Name = "dtpHistorialHasta";
            this.dtpHistorialHasta.Size = new System.Drawing.Size(100, 23);
            this.dtpHistorialHasta.TabIndex = 2;

            this.btnFiltrarHistorial.Location = new System.Drawing.Point(420, 12);
            this.btnFiltrarHistorial.Name = "btnFiltrarHistorial";
            this.btnFiltrarHistorial.Size = new System.Drawing.Size(75, 25);
            this.btnFiltrarHistorial.TabIndex = 3;
            this.btnFiltrarHistorial.Text = "Filtrar";
            this.btnFiltrarHistorial.UseVisualStyleBackColor = true;

            this.dgvHistorial.AllowUserToAddRows = false;
            this.dgvHistorial.AllowUserToDeleteRows = false;
            this.dgvHistorial.BackgroundColor = System.Drawing.Color.White;
            this.dgvHistorial.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHistorial.Location = new System.Drawing.Point(13, 50);
            this.dgvHistorial.Name = "dgvHistorial";
            this.dgvHistorial.ReadOnly = true;
            this.dgvHistorial.RowHeadersVisible = false;
            this.dgvHistorial.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvHistorial.Size = new System.Drawing.Size(1060, 450);
            this.dgvHistorial.TabIndex = 4;

            this.btnVerDetalle.Location = new System.Drawing.Point(13, 510);
            this.btnVerDetalle.Name = "btnVerDetalle";
            this.btnVerDetalle.Size = new System.Drawing.Size(100, 30);
            this.btnVerDetalle.TabIndex = 5;
            this.btnVerDetalle.Text = "Ver Detalle";
            this.btnVerDetalle.UseVisualStyleBackColor = true;

            this.btnRepetirImportacion.Location = new System.Drawing.Point(120, 510);
            this.btnRepetirImportacion.Name = "btnRepetirImportacion";
            this.btnRepetirImportacion.Size = new System.Drawing.Size(130, 30);
            this.btnRepetirImportacion.TabIndex = 6;
            this.btnRepetirImportacion.Text = "Repetir Importación";
            this.btnRepetirImportacion.UseVisualStyleBackColor = true;

            this.btnEliminarHistorial.Location = new System.Drawing.Point(260, 510);
            this.btnEliminarHistorial.Name = "btnEliminarHistorial";
            this.btnEliminarHistorial.Size = new System.Drawing.Size(100, 30);
            this.btnEliminarHistorial.TabIndex = 7;
            this.btnEliminarHistorial.Text = "Eliminar";
            this.btnEliminarHistorial.UseVisualStyleBackColor = true;

            // statusStrip
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.tsslEstado
            });
            this.statusStrip.Location = new System.Drawing.Point(0, 675);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1100, 25);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip1";

            this.tsslEstado.Name = "tsslEstado";
            this.tsslEstado.Size = new System.Drawing.Size(1085, 20);
            this.tsslEstado.Spring = true;
            this.tsslEstado.Text = "Listo";
            this.tsslEstado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // openFileDialog
            this.openFileDialog.Filter = "Archivos Excel|*.xlsx;*.xls|Archivos CSV|*.csv|Todos los archivos|*.*";
            this.openFileDialog.Title = "Seleccionar archivo para importar";

            // saveFileDialog
            this.saveFileDialog.Filter = "Archivos Excel|*.xlsx|Archivos CSV|*.csv|Archivos XML|*.xml|Archivos JSON|*.json";
            this.saveFileDialog.Title = "Guardar archivo exportado";

            // frm_ImportacionExportacion
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.tlpMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "frm_ImportacionExportacion";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Importación / Exportación de Datos";
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.panelSuperior.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabImportar.ResumeLayout(false);
            this.tabExportar.ResumeLayout(false);
            this.tabPlantillas.ResumeLayout(false);
            this.tabHistorial.ResumeLayout(false);
            this.tabHistorial.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}