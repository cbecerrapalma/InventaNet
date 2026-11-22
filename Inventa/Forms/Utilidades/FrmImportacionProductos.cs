using FontAwesome.Sharp;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Inventa.Forms.Utilidades
{
    public partial class FrmImportacionProductos : Form
    {
        #region Enumeraciones y Clases Anidadas

        private enum ModoImportacion
        {
            Insertar = 1,
            Actualizar = 2,
            Mixto = 3,
            Reemplazar = 4
        }

        private enum EstadoPaso
        {
            Pendiente,
            EnProceso,
            Completado,
            Error,
            Advertencia
        }

        private class ErrorValidacion
        {
            public int Fila { get; set; }
            public string Hoja { get; set; }
            public string Campo { get; set; }
            public string Mensaje { get; set; }
            public string Severidad { get; set; } // "Error", "Advertencia"
        }

        private class PasoImportacion
        {
            public string Nombre { get; set; }
            public EstadoPaso Estado { get; set; }
            public int RegistrosProcesados { get; set; }
            public int TotalRegistros { get; set; }
        }

        private class ModoInfo
        {
            public string Icono { get; set; }
            public string Titulo { get; set; }
            public string Descripcion { get; set; }
            public Color Color { get; set; }
            public int Riesgo { get; set; }
        }

        private enum TipoEstado
        {
            Info,
            Exito,
            Error,
            Advertencia
        }

        private enum TipoNotificacion
        {
            Info,
            Exito,
            Error,
            Advertencia
        }

        #endregion

        #region Variables Privadas

        private readonly Dictionary<ModoImportacion, ModoInfo> _modosInfo;
        private List<ErrorValidacion> _erroresValidacion;
        private Dictionary<string, DataTable> _datosHojas;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _pausaSolicitada;
        private int _paginaActual = 1;
        private int _totalPaginas = 1;
        private int _filasPorPagina = 10;
        private string _archivoActual;
        private readonly int _idTenant;      // Tenant de la sesión actual
        private readonly int _idUsuario;     // Usuario que ejecuta la importación

        #endregion

        #region Constructor

        public FrmImportacionProductos()
        {
            InitializeComponent();
            _modosInfo = InicializarModos();
            _erroresValidacion = new List<ErrorValidacion>();
            _datosHojas = new Dictionary<string, DataTable>();

            // Obtener tenant y usuario desde la sesión actual (clase estática de ejemplo)
            //_idTenant = SesionActual.Instance.IdTenant;
            //_idUsuario = SesionActual.Instance.IdUsuario;

            ConfigurarFormulario();
            CargarModos();
            ConectarEventos();
            ConfigurarToolTips();
            ConfigurarEfectosBotones();
        }

        #endregion

        #region Configuración Inicial

        private Dictionary<ModoImportacion, ModoInfo> InicializarModos()
        {
            return new Dictionary<ModoImportacion, ModoInfo>
            {
                { ModoImportacion.Insertar, new ModoInfo
                    {
                        Icono = "➕",
                        Titulo = "Modo Insertar",
                        Descripcion = "Solo crea nuevos registros (error si existen)",
                        Color = Color.FromArgb(52, 152, 219),
                        Riesgo = 1
                    }
                },
                { ModoImportacion.Actualizar, new ModoInfo
                    {
                        Icono = "🔄",
                        Titulo = "Modo Actualizar",
                        Descripcion = "Actualiza registros existentes por código",
                        Color = Color.FromArgb(46, 204, 113),
                        Riesgo = 2
                    }
                },
                { ModoImportacion.Mixto, new ModoInfo
                    {
                        Icono = "⚡",
                        Titulo = "Modo Mixto",
                        Descripcion = "Crea nuevos y actualiza existentes",
                        Color = Color.FromArgb(155, 89, 182),
                        Riesgo = 5
                    }
                },
                { ModoImportacion.Reemplazar, new ModoInfo
                    {
                        Icono = "⚠️",
                        Titulo = "Modo Reemplazar",
                        Descripcion = "Elimina y recrea todo (requiere confirmación)",
                        Color = Color.FromArgb(231, 76, 60),
                        Riesgo = 10
                    }
                }
            };
        }

        private void ConfigurarFormulario()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.DoubleBuffer |
                         ControlStyles.ResizeRedraw, true);

            lvPasos.View = View.Details;
            lvPasos.FullRowSelect = true;
            lvPasos.GridLines = true;

            cmbFilasPorPagina.SelectedIndex = 0;

            dgvProductos.ContextMenuStrip = contextMenuGrid;
            dgvVariantes.ContextMenuStrip = contextMenuGrid;
            dgvPresentaciones.ContextMenuStrip = contextMenuGrid;
            dgvErrores.ContextMenuStrip = contextMenuGrid;

            // Configurar columnas del DataGridView de errores
            dgvErrores.Columns.Clear();
            dgvErrores.Columns.Add("Fila", "Fila");
            dgvErrores.Columns.Add("Hoja", "Hoja");
            dgvErrores.Columns.Add("Campo", "Campo");
            dgvErrores.Columns.Add("Error", "Error");
            dgvErrores.Columns.Add("Severidad", "Severidad");
        }

        private void ConfigurarToolTips()
        {
            toolTip.SetToolTip(btnSeleccionarArchivo, "Selecciona un archivo Excel con la estructura de productos.");
            toolTip.SetToolTip(btnGenerarPlantilla, "Genera una plantilla Excel con todas las hojas necesarias y ejemplos.");
            toolTip.SetToolTip(btnImportar, "Inicia el proceso de importación según el modo seleccionado.");
            toolTip.SetToolTip(btnValidar, "Valida la estructura y los datos del archivo antes de importar.");
            toolTip.SetToolTip(btnVistaPrevia, "Muestra una vista previa de los datos del archivo.");
            toolTip.SetToolTip(btnHistorial, "Ver el historial de importaciones realizadas.");
            toolTip.SetToolTip(btnAyuda, "Muestra instrucciones detalladas para la importación.");
            toolTip.SetToolTip(btnExportarErrores, "Exporta la lista de errores a un archivo Excel.");
            toolTip.SetToolTip(btnIgnorarErrores, "Continúa con la importación ignorando los errores (los registros con errores se omiten).");
            toolTip.SetToolTip(btnPausar, "Pausa o reanuda el proceso de importación.");
            toolTip.SetToolTip(btnCancelar, "Cancela el proceso de importación en curso.");
        }

        private void ConfigurarEfectosBotones()
        {
            foreach (IconButton btn in new IconButton[] { btnImportar, btnValidar, btnVistaPrevia, btnHistorial, btnAyuda, btnSeleccionarArchivo, btnGenerarPlantilla, btnExportarErrores, btnIgnorarErrores, btnPausar, btnCancelar })
            {
                btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(btn.BackColor);
                btn.MouseLeave += (s, e) =>
                {
                    if (btn == btnImportar) btn.BackColor = Color.FromArgb(52, 152, 219);
                    else if (btn == btnValidar) btn.BackColor = Color.FromArgb(46, 204, 113);
                    else if (btn == btnVistaPrevia) btn.BackColor = Color.FromArgb(155, 89, 182);
                    else if (btn == btnHistorial) btn.BackColor = Color.FromArgb(52, 73, 94);
                    else if (btn == btnAyuda) btn.BackColor = Color.FromArgb(149, 165, 166);
                    else if (btn == btnSeleccionarArchivo) btn.BackColor = Color.FromArgb(52, 73, 94);
                    else if (btn == btnGenerarPlantilla) btn.BackColor = Color.FromArgb(39, 174, 96);
                    else if (btn == btnExportarErrores) btn.BackColor = Color.FromArgb(52, 73, 94);
                    else if (btn == btnIgnorarErrores) btn.BackColor = Color.FromArgb(241, 176, 57);
                    else if (btn == btnPausar) btn.BackColor = Color.FromArgb(241, 176, 57);
                    else if (btn == btnCancelar) btn.BackColor = Color.FromArgb(231, 76, 60);
                };
                btn.MouseDown += (s, e) => btn.Location = new Point(btn.Location.X + 2, btn.Location.Y + 2);
                btn.MouseUp += (s, e) => btn.Location = new Point(btn.Location.X - 2, btn.Location.Y - 2);
            }
        }

        private void CargarModos()
        {
            cmbModo.DataSource = Enum.GetValues(typeof(ModoImportacion))
                .Cast<ModoImportacion>()
                .ToList();
            cmbModo.SelectedItem = ModoImportacion.Mixto;
        }

        private void ConectarEventos()
        {
            this.btnSeleccionarArchivo.Click += new System.EventHandler(this.btnSeleccionarArchivo_Click);
            this.btnGenerarPlantilla.Click += new System.EventHandler(this.btnGenerarPlantilla_Click);
            this.btnImportar.Click += new System.EventHandler(this.btnImportar_Click);
            this.btnValidar.Click += new System.EventHandler(this.btnValidar_Click);
            this.btnVistaPrevia.Click += new System.EventHandler(this.btnVistaPrevia_Click);
            this.btnHistorial.Click += new System.EventHandler(this.btnHistorial_Click);
            this.btnAyuda.Click += new System.EventHandler(this.btnAyuda_Click);

            this.btnExportarErrores.Click += new System.EventHandler(this.btnExportarErrores_Click);
            this.btnIgnorarErrores.Click += new System.EventHandler(this.btnIgnorarErrores_Click);

            this.btnPausar.Click += new System.EventHandler(this.btnPausar_Click);
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            this.btnPrimeraPagina.Click += new System.EventHandler(this.btnPrimeraPagina_Click);
            this.btnPaginaAnterior.Click += new System.EventHandler(this.btnPaginaAnterior_Click);
            this.btnPaginaSiguiente.Click += new System.EventHandler(this.btnPaginaSiguiente_Click);
            this.btnUltimaPagina.Click += new System.EventHandler(this.btnUltimaPagina_Click);
            this.cmbFilasPorPagina.SelectedIndexChanged += new System.EventHandler(this.cmbFilasPorPagina_SelectedIndexChanged);
            this.numPaginaActual.ValueChanged += new System.EventHandler(this.numPaginaActual_ValueChanged);

            this.cmbModo.SelectedIndexChanged += new System.EventHandler(this.cmbModo_SelectedIndexChanged);

            this.copiarToolStripMenuItem.Click += new System.EventHandler(this.copiarToolStripMenuItem_Click);
            this.exportarExcelToolStripMenuItem.Click += new System.EventHandler(this.exportarExcelToolStripMenuItem_Click);

            this.timerAnimacion.Tick += new System.EventHandler(this.timerAnimacion_Tick);
            this.timerAnimacion.Start();
        }

        #endregion

        #region Event Handlers

        private void cmbModo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbModo.SelectedItem is ModoImportacion modo && _modosInfo.ContainsKey(modo))
            {
                var info = _modosInfo[modo];

                lblModoIcono.Text = info.Icono;
                lblModoTitulo.Text = info.Titulo;
                lblModoDescripcion.Text = info.Descripcion;
                panelModoInfo.BackColor = Color.FromArgb(20, info.Color.R, info.Color.G, info.Color.B);

                progressModoRiesgo.Value = info.Riesgo;
                progressModoRiesgo.Visible = true;

                btnImportar.BackColor = info.Color;
            }
        }

        private async void btnSeleccionarArchivo_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _archivoActual = openFileDialog.FileName;
                txtRutaArchivo.Text = _archivoActual;

                await CargarArchivoExcelAsync(_archivoActual);
            }
        }

        private async void btnGenerarPlantilla_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = $"Plantilla_Importacion_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                await GenerarPlantillaExcelAsync(saveFileDialog.FileName);
            }
        }

        private async void btnValidar_Click(object sender, EventArgs e)
        {
            await ValidarDatosAsync();
        }

        private async void btnVistaPrevia_Click(object sender, EventArgs e)
        {
            await CargarVistaPreviaAsync();
        }

        private async void btnImportar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_archivoActual))
            {
                MostrarNotificacion("Debe seleccionar un archivo", TipoNotificacion.Advertencia);
                return;
            }

            var modo = (ModoImportacion)cmbModo.SelectedItem;

            if (modo == ModoImportacion.Reemplazar && !ConfirmarReemplazo())
            {
                return;
            }

            if (_erroresValidacion.Any(e => e.Severidad == "Error") &&
                !ConfirmarContinuarConErrores())
            {
                return;
            }

            await IniciarImportacionAsync();
        }

        private void btnPausar_Click(object sender, EventArgs e)
        {
            _pausaSolicitada = !_pausaSolicitada;
            btnPausar.Text = _pausaSolicitada ? "Reanudar" : "Pausar";
            btnPausar.IconChar = _pausaSolicitada ? IconChar.Play : IconChar.Pause;
            btnPausar.BackColor = _pausaSolicitada ?
                Color.FromArgb(46, 204, 113) :
                Color.FromArgb(241, 176, 57);
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private void btnIgnorarErrores_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "¿Está seguro de continuar con la importación a pesar de los errores?\n\n" +
                "Los registros con errores serán omitidos.",
                "Confirmar Continuación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                btnImportar.Enabled = true;
            }
        }

        private void btnExportarErrores_Click(object? sender, EventArgs e)
        {
            ExportarErroresAExcel();
        }

        private void btnHistorial_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Módulo de historial en desarrollo",
                "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnAyuda_Click(object sender, EventArgs e)
        {
            MostrarAyuda();
        }

        private void btnPrimeraPagina_Click(object sender, EventArgs e)
        {
            _paginaActual = 1;
            ActualizarPagina();
        }

        private void btnPaginaAnterior_Click(object sender, EventArgs e)
        {
            if (_paginaActual > 1)
            {
                _paginaActual--;
                ActualizarPagina();
            }
        }

        private void btnPaginaSiguiente_Click(object sender, EventArgs e)
        {
            if (_paginaActual < _totalPaginas)
            {
                _paginaActual++;
                ActualizarPagina();
            }
        }

        private void btnUltimaPagina_Click(object sender, EventArgs e)
        {
            _paginaActual = _totalPaginas;
            ActualizarPagina();
        }

        private void numPaginaActual_ValueChanged(object sender, EventArgs e)
        {
            if (numPaginaActual.Value >= 1 && numPaginaActual.Value <= _totalPaginas)
            {
                _paginaActual = (int)numPaginaActual.Value;
                ActualizarPagina();
            }
        }

        private void cmbFilasPorPagina_SelectedIndexChanged(object sender, EventArgs e)
        {
            _filasPorPagina = int.Parse(cmbFilasPorPagina.SelectedItem.ToString());
            _paginaActual = 1;
            ActualizarPagina();
        }

        private void copiarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopiarSeleccionGrid();
        }

        private void exportarExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportarGridAExcel();
        }

        private void timerAnimacion_Tick(object sender, EventArgs e)
        {
            // Animación opcional
        }

        #endregion

        #region Métodos de Carga y Validación

        private async Task CargarArchivoExcelAsync(string rutaArchivo)
        {
            try
            {
                Enabled = false;
                MostrarEstado("Cargando archivo Excel...", TipoEstado.Info);

                _datosHojas = await Task.Run(() => LeerArchivoExcel(rutaArchivo));

                ActualizarEstadisticas();
                await ValidarDatosAsync();

                btnValidar.Enabled = true;
                btnVistaPrevia.Enabled = true;

                MostrarEstado("Archivo cargado correctamente", TipoEstado.Exito);
            }
            catch (Exception ex)
            {
                MostrarNotificacion($"Error al cargar archivo: {ex.Message}", TipoNotificacion.Error);
                MostrarEstado("Error al cargar archivo", TipoEstado.Error);
            }
            finally
            {
                Enabled = true;
            }
        }

        private Dictionary<string, DataTable> LeerArchivoExcel(string rutaArchivo)
        {
            var resultado = new Dictionary<string, DataTable>();

            using (var package = new ExcelPackage(new FileInfo(rutaArchivo)))
            {
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    if (worksheet.Dimension == null) continue;

                    var dt = new DataTable(worksheet.Name);

                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                    {
                        var header = worksheet.Cells[1, col].Value?.ToString() ?? $"Columna{col}";
                        dt.Columns.Add(header);
                    }

                    for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                    {
                        var dr = dt.NewRow();
                        bool filaVacia = true;

                        for (int col = 0; col < dt.Columns.Count; col++)
                        {
                            var valor = worksheet.Cells[row, col + 1].Value;
                            if (valor != null)
                            {
                                dr[col] = valor.ToString();
                                filaVacia = false;
                            }
                        }

                        if (!filaVacia)
                        {
                            dt.Rows.Add(dr);
                        }
                    }

                    resultado[worksheet.Name.ToUpper()] = dt;
                }
            }

            return resultado;
        }

        private async Task ValidarDatosAsync()
        {
            try
            {
                Enabled = false;
                MostrarEstado("Validando datos...", TipoEstado.Info);

                _erroresValidacion = await Task.Run(() => RealizarValidaciones());

                ActualizarListaErrores();
                ActualizarEstadisticasErrores();

                bool tieneErroresGraves = _erroresValidacion.Any(e => e.Severidad == "Error");
                btnImportar.Enabled = !tieneErroresGraves;

                if (tieneErroresGraves)
                {
                    MostrarEstado("Se encontraron errores graves", TipoEstado.Error);
                }
                else
                {
                    MostrarEstado("Validación completada sin errores graves", TipoEstado.Exito);
                }
            }
            catch (Exception ex)
            {
                MostrarNotificacion($"Error en validación: {ex.Message}", TipoNotificacion.Error);
            }
            finally
            {
                Enabled = true;
            }
        }

        private List<ErrorValidacion> RealizarValidaciones()
        {
            var errores = new List<ErrorValidacion>();

            var hojasRequeridas = new[] { "PRODUCTOS", "VARIANTES", "PRESENTACIONES" };
            foreach (var hoja in hojasRequeridas)
            {
                if (!_datosHojas.ContainsKey(hoja))
                {
                    errores.Add(new ErrorValidacion
                    {
                        Hoja = hoja,
                        Mensaje = $"Hoja requerida '{hoja}' no encontrada",
                        Severidad = "Error"
                    });
                }
            }

            if (_datosHojas.ContainsKey("PRODUCTOS"))
            {
                var dt = _datosHojas["PRODUCTOS"];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var row = dt.Rows[i];

                    if (string.IsNullOrEmpty(row["CodigoProducto"]?.ToString()))
                    {
                        errores.Add(new ErrorValidacion
                        {
                            Fila = i + 2,
                            Hoja = "PRODUCTOS",
                            Campo = "CodigoProducto",
                            Mensaje = "Código de producto requerido",
                            Severidad = "Error"
                        });
                    }

                    if (string.IsNullOrEmpty(row["NombreProducto"]?.ToString()))
                    {
                        errores.Add(new ErrorValidacion
                        {
                            Fila = i + 2,
                            Hoja = "PRODUCTOS",
                            Campo = "NombreProducto",
                            Mensaje = "Nombre de producto requerido",
                            Severidad = "Error"
                        });
                    }

                    var codigo = row["CodigoProducto"]?.ToString();
                    if (!string.IsNullOrEmpty(codigo))
                    {
                        var duplicados = dt.AsEnumerable()
                            .Select((r, idx) => new { Row = r, Index = idx })
                            .Where(x => x.Row["CodigoProducto"]?.ToString() == codigo && x.Index != i)
                            .ToList();

                        if (duplicados.Any())
                        {
                            errores.Add(new ErrorValidacion
                            {
                                Fila = i + 2,
                                Hoja = "PRODUCTOS",
                                Campo = "CodigoProducto",
                                Mensaje = $"Código duplicado: {codigo}",
                                Severidad = "Error"
                            });
                        }
                    }
                }
            }

            if (_datosHojas.ContainsKey("VARIANTES") && _datosHojas.ContainsKey("PRODUCTOS"))
            {
                var productos = _datosHojas["PRODUCTOS"];
                var variantes = _datosHojas["VARIANTES"];
                var productosExistentes = productos.AsEnumerable()
                    .Select(r => r["CodigoProducto"]?.ToString())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToHashSet();

                for (int i = 0; i < variantes.Rows.Count; i++)
                {
                    var row = variantes.Rows[i];
                    var codigoProducto = row["CodigoProducto"]?.ToString();

                    if (!string.IsNullOrEmpty(codigoProducto) && !productosExistentes.Contains(codigoProducto))
                    {
                        errores.Add(new ErrorValidacion
                        {
                            Fila = i + 2,
                            Hoja = "VARIANTES",
                            Campo = "CodigoProducto",
                            Mensaje = $"Producto padre '{codigoProducto}' no encontrado",
                            Severidad = "Error"
                        });
                    }
                }
            }

            return errores;
        }

        private void ActualizarEstadisticas()
        {
            if (_datosHojas.ContainsKey("PRODUCTOS"))
            {
                lblProdValor.Text = _datosHojas["PRODUCTOS"].Rows.Count.ToString();
            }

            if (_datosHojas.ContainsKey("VARIANTES"))
            {
                lblVarValor.Text = _datosHojas["VARIANTES"].Rows.Count.ToString();
            }

            if (_datosHojas.ContainsKey("PRESENTACIONES"))
            {
                lblPresValor.Text = _datosHojas["PRESENTACIONES"].Rows.Count.ToString();
            }
        }

        private void ActualizarEstadisticasErrores()
        {
            var erroresGraves = _erroresValidacion.Count(e => e.Severidad == "Error");
            lblErrorValor.Text = $"{erroresGraves}";
            panelErrorStats.BackColor = erroresGraves > 0 ?
                Color.FromArgb(231, 76, 60) :
                Color.FromArgb(46, 204, 113);
        }

        private void ActualizarListaErrores()
        {
            dgvErrores.Rows.Clear();

            foreach (var error in _erroresValidacion.OrderBy(e => e.Hoja).ThenBy(e => e.Fila))
            {
                int rowIndex = dgvErrores.Rows.Add(
                    error.Fila.ToString(),
                    error.Hoja,
                    error.Campo,
                    error.Mensaje,
                    error.Severidad
                );

                if (error.Severidad == "Error")
                    dgvErrores.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
                else
                    dgvErrores.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 224);
            }
        }

        #endregion

        #region Métodos de Importación (Simulación con lógica de maestros)

        private async Task IniciarImportacionAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _pausaSolicitada = false;

            try
            {
                gbxProgreso.Visible = true;
                btnImportar.Enabled = false;
                btnValidar.Enabled = false;
                btnVistaPrevia.Enabled = false;
                btnPausar.Enabled = true;
                btnCancelar.Enabled = true;

                // ============================================
                // PASOS DE LA IMPORTACIÓN (incluyendo maestros)
                // ============================================
                var pasos = new List<PasoImportacion>
                {
                    new PasoImportacion { Nombre = "1. Validando estructura", Estado = EstadoPaso.Pendiente, TotalRegistros = 1 },
                    new PasoImportacion { Nombre = "2. Procesando categorías", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("CATEGORIAS") ? _datosHojas["CATEGORIAS"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "3. Procesando marcas", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("MARCAS") ? _datosHojas["MARCAS"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "4. Procesando unidades", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("UNIDADES") ? _datosHojas["UNIDADES"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "5. Procesando proveedores", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("PROVEEDORES") ? _datosHojas["PROVEEDORES"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "6. Procesando bodegas", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("BODEGAS") ? _datosHojas["BODEGAS"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "7. Creando productos", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("PRODUCTOS") ? _datosHojas["PRODUCTOS"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "8. Procesando atributos dinámicos", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("ATRIBUTOS_DINAMICOS") ? _datosHojas["ATRIBUTOS_DINAMICOS"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "9. Creando valores de atributos", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("VALORES_ATRIBUTOS") ? _datosHojas["VALORES_ATRIBUTOS"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "10. Creando variantes", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("VARIANTES") ? _datosHojas["VARIANTES"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "11. Asignando atributos a variantes", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("VARIANTE_ATRIBUTOS") ? _datosHojas["VARIANTE_ATRIBUTOS"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "12. Creando presentaciones", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("PRESENTACIONES") ? _datosHojas["PRESENTACIONES"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "13. Asignando códigos de barra", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("CODIGOS_BARRA") ? _datosHojas["CODIGOS_BARRA"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "14. Configurando precios de venta", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("PRECIOS_VENTA") ? _datosHojas["PRECIOS_VENTA"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "16. Registrando stock inicial", Estado = EstadoPaso.Pendiente, TotalRegistros = _datosHojas.ContainsKey("STOCK_INICIAL") ? _datosHojas["STOCK_INICIAL"].Rows.Count : 0 },
                    new PasoImportacion { Nombre = "17. Generando reporte final", Estado = EstadoPaso.Pendiente, TotalRegistros = 1 }
                };

                ActualizarListaPasos(pasos);

                // Simular la importación paso a paso (en una implementación real, aquí se invocaría al servicio)
                await SimularImportacionAsync(pasos, _cancellationTokenSource.Token);

                MostrarNotificacion("Importación completada exitosamente", TipoNotificacion.Exito);
            }
            catch (OperationCanceledException)
            {
                MostrarNotificacion("Importación cancelada", TipoNotificacion.Advertencia);
            }
            catch (Exception ex)
            {
                MostrarNotificacion($"Error en importación: {ex.Message}", TipoNotificacion.Error);
            }
            finally
            {
                btnImportar.Enabled = true;
                btnValidar.Enabled = true;
                btnVistaPrevia.Enabled = true;
                btnPausar.Enabled = false;
                btnCancelar.Enabled = false;
                btnPausar.Text = "Pausar";
                btnPausar.IconChar = IconChar.Pause;
                _pausaSolicitada = false;
            }
        }

        private int ObtenerTotalMaestros()
        {
            int total = 0;
            if (_datosHojas.ContainsKey("PRODUCTOS"))
            {
                var categorias = _datosHojas["PRODUCTOS"].AsEnumerable()
                    .Select(r => r["CodigoCategoria"]?.ToString())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .Count();
                var marcas = _datosHojas["PRODUCTOS"].AsEnumerable()
                    .Select(r => r["CodigoMarca"]?.ToString())
                    .Where(m => !string.IsNullOrEmpty(m))
                    .Distinct()
                    .Count();
                var unidades = _datosHojas["PRODUCTOS"].AsEnumerable()
                    .Select(r => r["CodigoUnidadBase"]?.ToString())
                    .Where(u => !string.IsNullOrEmpty(u))
                    .Distinct()
                    .Count();
                total = categorias + marcas + unidades;
            }
            return total;
        }

        private async Task SimularImportacionAsync(List<PasoImportacion> pasos, CancellationToken token)
        {
            var random = new Random();

            for (int i = 0; i < pasos.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                while (_pausaSolicitada)
                {
                    await Task.Delay(100);
                    token.ThrowIfCancellationRequested();
                }

                pasos[i].Estado = EstadoPaso.EnProceso;
                ActualizarListaPasos(pasos);

                // Simular progreso dentro del paso
                int totalSubPasos = random.Next(3, 8);
                for (int j = 0; j <= totalSubPasos; j++)
                {
                    token.ThrowIfCancellationRequested();

                    pasos[i].RegistrosProcesados = (j * pasos[i].TotalRegistros) / totalSubPasos;

                    int progresoGeneral = (i * 100 / pasos.Count) + (j * 100 / (pasos.Count * totalSubPasos));
                    progressGeneral.Value = Math.Min(progresoGeneral, 100);
                    lblProgresoGeneral.Text = $"Procesando: {pasos[i].Nombre} - {progresoGeneral}%";

                    ActualizarListaPasos(pasos);

                    await Task.Delay(200);
                }

                pasos[i].RegistrosProcesados = pasos[i].TotalRegistros;
                pasos[i].Estado = random.Next(10) < 8 ? EstadoPaso.Completado : EstadoPaso.Advertencia;
                ActualizarListaPasos(pasos);
            }

            progressGeneral.Value = 100;
            lblProgresoGeneral.Text = "¡Importación completada!";
        }

        private void ActualizarListaPasos(List<PasoImportacion> pasos)
        {
            lvPasos.Items.Clear();

            foreach (var paso in pasos)
            {
                var item = new ListViewItem(paso.Nombre);

                string estadoTexto = paso.Estado switch
                {
                    EstadoPaso.Pendiente => "⏳ Pendiente",
                    EstadoPaso.EnProceso => "⚙️ En proceso",
                    EstadoPaso.Completado => "✅ Completado",
                    EstadoPaso.Error => "❌ Error",
                    EstadoPaso.Advertencia => "⚠️ Advertencia",
                    _ => paso.Estado.ToString()
                };

                item.SubItems.Add(estadoTexto);
                item.SubItems.Add($"{paso.RegistrosProcesados}/{paso.TotalRegistros}");

                item.BackColor = paso.Estado switch
                {
                    EstadoPaso.Completado => Color.FromArgb(232, 245, 233),
                    EstadoPaso.EnProceso => Color.FromArgb(227, 242, 253),
                    EstadoPaso.Error => Color.FromArgb(255, 235, 238),
                    EstadoPaso.Advertencia => Color.FromArgb(255, 243, 224),
                    _ => Color.White
                };

                lvPasos.Items.Add(item);
            }

            lvPasos.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        #endregion

        #region Métodos de Plantilla (con hojas maestras y ejemplos actualizados)

        private async Task GenerarPlantillaExcelAsync(string rutaArchivo)
        {
            try
            {
                Enabled = false;
                MostrarEstado("Generando plantilla...", TipoEstado.Info);

                await Task.Run(() =>
                {
                    using (var package = new ExcelPackage())
                    {
                        // ============================================
                        // HOJA 1: CATEGORIAS (maestro)
                        // ============================================
                        var worksheet = package.Workbook.Worksheets.Add("CATEGORIAS");
                        string[] headersCategorias = new[] { "CodigoCategoria", "Nombre", "Activo" };
                        for (int i = 0; i < headersCategorias.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersCategorias[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }
                        int fila = 2;
                        worksheet.Cells[fila, 1].Value = "CAT-ELECTRO-001";
                        worksheet.Cells[fila, 2].Value = "Electrodomésticos";
                        worksheet.Cells[fila, 3].Value = "1";
                        fila++;
                        worksheet.Cells[fila, 1].Value = "CAT-ROPA-001";
                        worksheet.Cells[fila, 2].Value = "Ropa";
                        worksheet.Cells[fila, 3].Value = "1";
                        fila++;
                        worksheet.Cells[fila, 1].Value = "CAT-JAMONES-001";
                        worksheet.Cells[fila, 2].Value = "Jamones";
                        worksheet.Cells[fila, 3].Value = "1";
                        fila++;
                        worksheet.Cells[fila, 1].Value = "CAT-LICORES-001";
                        worksheet.Cells[fila, 2].Value = "Licores";
                        worksheet.Cells[fila, 3].Value = "1";
                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 2: MARCAS (maestro)
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("MARCAS");
                        string[] headersMarcas = new[] { "CodigoMarca", "Nombre", "Activo" };
                        for (int i = 0; i < headersMarcas.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersMarcas[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }
                        fila = 2;
                        worksheet.Cells[fila, 1].Value = "MAR-LG-001";
                        worksheet.Cells[fila, 2].Value = "LG";
                        worksheet.Cells[fila, 3].Value = "1";
                        fila++;
                        worksheet.Cells[fila, 1].Value = "MAR-ZARA-001";
                        worksheet.Cells[fila, 2].Value = "ZARA";
                        worksheet.Cells[fila, 3].Value = "1";
                        fila++;
                        worksheet.Cells[fila, 1].Value = "MAR-JOSELITO-001";
                        worksheet.Cells[fila, 2].Value = "Joselito";
                        worksheet.Cells[fila, 3].Value = "1";
                        fila++;
                        worksheet.Cells[fila, 1].Value = "MAR-RIOJANA-001";
                        worksheet.Cells[fila, 2].Value = "Bodegas Riojanas";
                        worksheet.Cells[fila, 3].Value = "1";
                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 3: UNIDADES (maestro)
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("UNIDADES");
                        string[] headersUnidades = new[] { "CodigoUnidad", "Nombre", "Activo" };
                        for (int i = 0; i < headersUnidades.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersUnidades[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }
                        fila = 2;
                        worksheet.Cells[fila, 1].Value = "UN";
                        worksheet.Cells[fila, 2].Value = "Unidad";
                        worksheet.Cells[fila, 3].Value = "1";
                        fila++;
                        worksheet.Cells[fila, 1].Value = "KG";
                        worksheet.Cells[fila, 2].Value = "Kilogramo";
                        worksheet.Cells[fila, 3].Value = "1";
                        fila++;
                        worksheet.Cells[fila, 1].Value = "LT";
                        worksheet.Cells[fila, 2].Value = "Litro";
                        worksheet.Cells[fila, 3].Value = "1";
                        fila++;
                        worksheet.Cells[fila, 1].Value = "CAJ";
                        worksheet.Cells[fila, 2].Value = "Caja";
                        worksheet.Cells[fila, 3].Value = "1";
                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 4: PROVEEDORES (maestro)
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("PROVEEDORES");
                        string[] headersProveedores = new[] { "CodigoProveedor", "RazonSocial", "RUT", "Activo" };
                        for (int i = 0; i < headersProveedores.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersProveedores[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }
                        fila = 2;
                        worksheet.Cells[fila, 1].Value = "PROV-DIST-001";
                        worksheet.Cells[fila, 2].Value = "Distribuidora Electrónica S.A.";
                        worksheet.Cells[fila, 3].Value = "76.123.456-7";
                        worksheet.Cells[fila, 4].Value = "1";
                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 5: BODEGAS (maestro)
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("BODEGAS");
                        string[] headersBodegas = new[] { "CodigoBodega", "Nombre", "Activo" };
                        for (int i = 0; i < headersBodegas.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersBodegas[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }
                        fila = 2;
                        worksheet.Cells[fila, 1].Value = "BOD-PPAL-001";
                        worksheet.Cells[fila, 2].Value = "Bodega Principal";
                        worksheet.Cells[fila, 3].Value = "1";
                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 6: PRODUCTOS (con códigos según convención)
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("PRODUCTOS");
                        string[] headersProductos = new[]
                        {
                            "CodigoProducto", "NombreProducto", "CodigoCategoria", "CodigoMarca",
                            "CodigoUnidadBase", "DiasVencimiento", "ManejaLote", "EsPesable", "Activo"
                        };

                        for (int i = 0; i < headersProductos.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersProductos[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }

                        fila = 2;
                        worksheet.Cells[fila, 1].Value = "PROD-ELE-000001";
                        worksheet.Cells[fila, 2].Value = "Refrigerador No Frost";
                        worksheet.Cells[fila, 3].Value = "CAT-ELECTRO-001";
                        worksheet.Cells[fila, 4].Value = "MAR-LG-001";
                        worksheet.Cells[fila, 5].Value = "UN";
                        worksheet.Cells[fila, 6].Value = "0";
                        worksheet.Cells[fila, 7].Value = "0";
                        worksheet.Cells[fila, 8].Value = "0";
                        worksheet.Cells[fila, 9].Value = "1";
                        fila++;

                        worksheet.Cells[fila, 1].Value = "PROD-ROP-000001";
                        worksheet.Cells[fila, 2].Value = "Camisa Manga Larga";
                        worksheet.Cells[fila, 3].Value = "CAT-ROPA-001";
                        worksheet.Cells[fila, 4].Value = "MAR-ZARA-001";
                        worksheet.Cells[fila, 5].Value = "UN";
                        worksheet.Cells[fila, 6].Value = "0";
                        worksheet.Cells[fila, 7].Value = "0";
                        worksheet.Cells[fila, 8].Value = "0";
                        worksheet.Cells[fila, 9].Value = "1";
                        fila++;

                        worksheet.Cells[fila, 1].Value = "PROD-JAM-000001";
                        worksheet.Cells[fila, 2].Value = "Jamón Serrano";
                        worksheet.Cells[fila, 3].Value = "CAT-JAMONES-001";
                        worksheet.Cells[fila, 4].Value = "MAR-JOSELITO-001";
                        worksheet.Cells[fila, 5].Value = "KG";
                        worksheet.Cells[fila, 6].Value = "365";
                        worksheet.Cells[fila, 7].Value = "1";
                        worksheet.Cells[fila, 8].Value = "1";
                        worksheet.Cells[fila, 9].Value = "1";
                        fila++;

                        worksheet.Cells[fila, 1].Value = "PROD-LIC-000001";
                        worksheet.Cells[fila, 2].Value = "Vino Tinto Reserva";
                        worksheet.Cells[fila, 3].Value = "CAT-LICORES-001";
                        worksheet.Cells[fila, 4].Value = "MAR-RIOJANA-001";
                        worksheet.Cells[fila, 5].Value = "UN";
                        worksheet.Cells[fila, 6].Value = "0";
                        worksheet.Cells[fila, 7].Value = "0";
                        worksheet.Cells[fila, 8].Value = "0";
                        worksheet.Cells[fila, 9].Value = "1";

                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 7: VARIANTES (ejemplos)
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("VARIANTES");
                        string[] headersVariantes = new[] { "CodigoProducto", "CodigoVariante", "NombreVariante", "CostoPromedio" };

                        for (int i = 0; i < headersVariantes.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersVariantes[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }

                        fila = 2;
                        worksheet.Cells[fila, 1].Value = "PROD-ELE-000001";
                        worksheet.Cells[fila, 2].Value = "VAR-PROD-ELE-000001-01";
                        worksheet.Cells[fila, 3].Value = "Refrigerador 300L";
                        worksheet.Cells[fila, 4].Value = "450.00";
                        fila++;

                        worksheet.Cells[fila, 1].Value = "PROD-ROP-000001";
                        worksheet.Cells[fila, 2].Value = "VAR-PROD-ROP-000001-01";
                        worksheet.Cells[fila, 3].Value = "Camisa Manga Larga - S";
                        worksheet.Cells[fila, 4].Value = "15.00";
                        fila++;

                        worksheet.Cells[fila, 1].Value = "PROD-JAM-000001";
                        worksheet.Cells[fila, 2].Value = "VAR-PROD-JAM-000001-01";
                        worksheet.Cells[fila, 3].Value = "Jamón Serrano 7kg";
                        worksheet.Cells[fila, 4].Value = "80.00";

                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 8: ATRIBUTOS_DINAMICOS
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("ATRIBUTOS_DINAMICOS");
                        string[] headersAtributos = new[] { "CodigoCategoria", "CodigoAtributo", "NombreAtributo", "EsObligatorio" };

                        for (int i = 0; i < headersAtributos.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersAtributos[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }

                        worksheet.Cells[2, 1].Value = "CAT-ROPA-001";
                        worksheet.Cells[2, 2].Value = "ATR-ROPA-TALLA";
                        worksheet.Cells[2, 3].Value = "Talla";
                        worksheet.Cells[2, 4].Value = "1";

                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 9: VALORES_ATRIBUTOS
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("VALORES_ATRIBUTOS");
                        string[] headersValores = new[] { "CodigoCategoria", "CodigoAtributo", "CodigoValor", "NombreValor", "Orden" };

                        for (int i = 0; i < headersValores.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersValores[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }

                        worksheet.Cells[2, 1].Value = "CAT-ROPA-001";
                        worksheet.Cells[2, 2].Value = "ATR-ROPA-TALLA";
                        worksheet.Cells[2, 3].Value = "VAL-ATR-ROPA-TALLA-S";
                        worksheet.Cells[2, 4].Value = "Small";
                        worksheet.Cells[2, 5].Value = "1";

                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 10: VARIANTE_ATRIBUTOS
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("VARIANTE_ATRIBUTOS");
                        string[] headersVarAtrib = new[] { "CodigoProducto", "CodigoVariante", "CodigoAtributo", "CodigoValor" };

                        for (int i = 0; i < headersVarAtrib.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersVarAtrib[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }

                        worksheet.Cells[2, 1].Value = "PROD-ROP-000001";
                        worksheet.Cells[2, 2].Value = "VAR-PROD-ROP-000001-01";
                        worksheet.Cells[2, 3].Value = "ATR-ROPA-TALLA";
                        worksheet.Cells[2, 4].Value = "VAL-ATR-ROPA-TALLA-S";

                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 11: PRESENTACIONES
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("PRESENTACIONES");
                        string[] headersPresentaciones = new[]
                        {
                            "CodigoProducto", "CodigoVariante", "DescripcionPresentacion", "CodigoUnidad",
                            "FactorConversion", "EsVenta", "EsCompra"
                        };

                        for (int i = 0; i < headersPresentaciones.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersPresentaciones[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }

                        worksheet.Cells[2, 1].Value = "PROD-ELE-000001";
                        worksheet.Cells[2, 2].Value = "VAR-PROD-ELE-000001-01";
                        worksheet.Cells[2, 3].Value = "UNIDAD";
                        worksheet.Cells[2, 4].Value = "UN";
                        worksheet.Cells[2, 5].Value = "1";
                        worksheet.Cells[2, 6].Value = "1";
                        worksheet.Cells[2, 7].Value = "1";

                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 12: CODIGOS_BARRA
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("CODIGOS_BARRA");
                        string[] headersCodBar = new[]
                        {
                            "CodigoProducto", "CodigoVariante", "DescripcionPresentacion",
                            "CodigoBarra", "TipoCodigo", "Principal"
                        };

                        for (int i = 0; i < headersCodBar.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersCodBar[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }

                        worksheet.Cells[2, 1].Value = "PROD-ELE-000001";
                        worksheet.Cells[2, 2].Value = "VAR-PROD-ELE-000001-01";
                        worksheet.Cells[2, 3].Value = "UNIDAD";
                        worksheet.Cells[2, 4].Value = "1234567890123";
                        worksheet.Cells[2, 5].Value = "1";
                        worksheet.Cells[2, 6].Value = "1";

                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 13: PRECIOS_VENTA (S57.69: normales sin TipoPrecio)
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("PRECIOS_VENTA");
                        string[] headersPreciosVenta = new[]
                        {
                            "CodigoProducto", "CodigoVariante", "DescripcionPresentacion",
                            "Precio", "FechaVigencia"
                        };

                        for (int i = 0; i < headersPreciosVenta.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersPreciosVenta[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }

                        worksheet.Cells[2, 1].Value = "PROD-ELE-000001";
                        worksheet.Cells[2, 2].Value = "VAR-PROD-ELE-000001-01";
                        worksheet.Cells[2, 3].Value = "UNIDAD";
                        worksheet.Cells[2, 4].Value = "599.99";
                        worksheet.Cells[2, 5].Value = DateTime.Now.ToString("yyyy-MM-dd");

                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 13b: PRECIOS_MAYORISTA (S57.69: relación normalizada)
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("PRECIOS_MAYORISTA");
                        string[] headersPreciosMayorista = new[]
                        {
                            "CodigoProducto", "CodigoVariante", "DescripcionPresentacion",
                            "Precio", "FechaVigencia"
                        };

                        for (int i = 0; i < headersPreciosMayorista.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersPreciosMayorista[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }

                        worksheet.Cells[2, 1].Value = "PROD-ELE-000001";
                        worksheet.Cells[2, 2].Value = "VAR-PROD-ELE-000001-01";
                        worksheet.Cells[2, 3].Value = "UNIDAD";
                        worksheet.Cells[2, 4].Value = "499.99";
                        worksheet.Cells[2, 5].Value = DateTime.Now.ToString("yyyy-MM-dd");

                        worksheet.Cells.AutoFitColumns();

                        // ============================================
                        // HOJA 15: STOCK_INICIAL
                        // ============================================
                        worksheet = package.Workbook.Worksheets.Add("STOCK_INICIAL");
                        string[] headersStock = new[]
                        {
                            "CodigoProducto", "CodigoVariante", "DescripcionPresentacion",
                            "CodigoBodega", "CodigoLote", "FechaVencimiento", "CantidadInicial", "CostoUnitario"
                        };

                        for (int i = 0; i < headersStock.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headersStock[i];
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                        }

                        worksheet.Cells[2, 1].Value = "PROD-ELE-000001";
                        worksheet.Cells[2, 2].Value = "VAR-PROD-ELE-000001-01";
                        worksheet.Cells[2, 3].Value = "UNIDAD";
                        worksheet.Cells[2, 4].Value = "BOD-PPAL-001";
                        worksheet.Cells[2, 5].Value = "";
                        worksheet.Cells[2, 6].Value = "";
                        worksheet.Cells[2, 7].Value = "50";
                        worksheet.Cells[2, 8].Value = "450.00";

                        worksheet.Cells.AutoFitColumns();

                        // Agregar comentarios de ayuda
                        AgregarComentariosAyuda(package.Workbook.Worksheets);

                        package.SaveAs(new FileInfo(rutaArchivo));
                    }
                });

                MostrarNotificacion("Plantilla generada exitosamente", TipoNotificacion.Exito);

                if (MessageBox.Show("¿Desea abrir la carpeta donde se guardó la plantilla?",
                    "Plantilla Generada", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{rutaArchivo}\"");
                }
            }
            catch (Exception ex)
            {
                MostrarNotificacion($"Error al generar plantilla: {ex.Message}", TipoNotificacion.Error);
            }
            finally
            {
                Enabled = true;
            }
        }

        private void AgregarComentariosAyuda(ExcelWorksheets worksheets)
        {
            var comentarios = new Dictionary<string, string>
            {
                { "CodigoProducto", "Código único del producto según convención: PROD-{CategoriaPrefijo}-{6 dígitos}" },
                { "NombreProducto", "Nombre descriptivo del producto (requerido)" },
                { "CodigoCategoria", "Código de categoría según convención: CAT-{Nombre}-{3 dígitos} (debe existir en hoja CATEGORIAS)" },
                { "CodigoMarca", "Código de marca según convención: MAR-{NombreCorto}-{3 dígitos} (debe existir en hoja MARCAS)" },
                { "CodigoUnidadBase", "Código de unidad estándar: UN, KG, LT, CAJ, PAR (debe existir en hoja UNIDADES)" },
                { "DiasVencimiento", "Días de vida útil del producto (opcional)" },
                { "ManejaLote", "0 = No maneja lote, 1 = Maneja lote" },
                { "EsPesable", "0 = No pesable, 1 = Producto pesable" },
                { "Activo", "0 = Inactivo, 1 = Activo (por defecto 1)" },
                { "CodigoVariante", "Código único de variante según convención: VAR-{CodigoProducto}-{2 dígitos}" },
                { "NombreVariante", "Nombre descriptivo de la variante" },
                { "CostoPromedio", "Costo promedio inicial (opcional)" },
                { "CodigoAtributo", "Código del atributo según convención: ATR-{CategoriaPrefijo}-{Nombre}" },
                { "NombreAtributo", "Nombre del atributo" },
                { "EsObligatorio", "0 = Opcional, 1 = Obligatorio" },
                { "CodigoValor", "Código del valor según convención: VAL-{CodigoAtributo}-{Valor}" },
                { "NombreValor", "Nombre del valor" },
                { "Orden", "Orden de visualización" },
                { "DescripcionPresentacion", "Descripción de la presentación" },
                { "CodigoUnidad", "Código de unidad (debe existir en hoja UNIDADES)" },
                { "FactorConversion", "Factor de conversión respecto a unidad base" },
                { "EsVenta", "0 = No se vende, 1 = Se vende" },
                { "EsCompra", "0 = No se compra, 1 = Se compra" },
                { "CodigoBarra", "Código de barras (único global)" },
                { "TipoCodigo", "1=Normal, 2=Pesable, 3=Interno, 4=Proveedor" },
                { "Principal", "0=Secundario, 1=Principal" },
                { "Precio", "Valor del precio normal (hoja PRECIOS_VENTA) o mayorista (hoja PRECIOS_MAYORISTA)" },
                { "FechaVigencia", "Fecha desde la que aplica el precio" },
                { "CodigoProveedor", "Código del proveedor según convención: PROV-{NombreCorto}-{3 dígitos} (debe existir en hoja PROVEEDORES)" },
                { "CodigoBodega", "Código de bodega según convención: BOD-{Tipo}-{3 dígitos} (debe existir en hoja BODEGAS)" },
                { "CodigoLote", "Código de lote según convención: LOTE-{CodigoProducto}-{Fecha}-{2 dígitos}" },
                { "FechaVencimiento", "Fecha de vencimiento del lote" },
                { "CantidadInicial", "Cantidad inicial en unidades de la presentación" },
                { "CostoUnitario", "Costo unitario para este lote" }
            };

            foreach (var worksheet in worksheets)
            {
                if (worksheet.Dimension == null) continue;

                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    var cell = worksheet.Cells[1, col];
                    if (cell.Value != null && comentarios.ContainsKey(cell.Value.ToString()))
                    {
                        var comment = cell.AddComment(comentarios[cell.Value.ToString()], "Sistema");
                        comment.AutoFit = true;
                    }
                }
            }
        }

        #endregion

        #region Métodos de Vista Previa

        private async Task CargarVistaPreviaAsync()
        {
            if (!_datosHojas.Any())
            {
                MostrarNotificacion("No hay datos para mostrar", TipoNotificacion.Advertencia);
                return;
            }

            try
            {
                Enabled = false;

                await Task.Run(() =>
                {
                    this.Invoke(new Action(() =>
                    {
                        if (_datosHojas.ContainsKey("PRODUCTOS"))
                        {
                            dgvProductos.DataSource = _datosHojas["PRODUCTOS"];
                        }

                        if (_datosHojas.ContainsKey("VARIANTES"))
                        {
                            dgvVariantes.DataSource = _datosHojas["VARIANTES"];
                        }

                        if (_datosHojas.ContainsKey("PRESENTACIONES"))
                        {
                            dgvPresentaciones.DataSource = _datosHojas["PRESENTACIONES"];
                        }

                        if (_datosHojas.ContainsKey("PRODUCTOS"))
                        {
                            _totalPaginas = (int)Math.Ceiling((double)_datosHojas["PRODUCTOS"].Rows.Count / _filasPorPagina);
                            lblTotalPaginas.Text = $"de {_totalPaginas}";
                            numPaginaActual.Maximum = _totalPaginas;
                            numPaginaActual.Value = 1;
                        }
                    }));
                });
            }
            finally
            {
                Enabled = true;
            }
        }

        private void ActualizarPagina()
        {
            numPaginaActual.Value = _paginaActual;
            // Aquí se implementaría la lógica de paginación real si se desea
        }

        #endregion

        #region Métodos de Utilidad

        private void MostrarEstado(string mensaje, TipoEstado tipo)
        {
            System.Diagnostics.Debug.WriteLine($"[{tipo}] {mensaje}");
        }

        private void MostrarNotificacion(string mensaje, TipoNotificacion tipo)
        {
            var icono = tipo switch
            {
                TipoNotificacion.Exito => MessageBoxIcon.Information,
                TipoNotificacion.Error => MessageBoxIcon.Error,
                TipoNotificacion.Advertencia => MessageBoxIcon.Warning,
                _ => MessageBoxIcon.None
            };

            MessageBox.Show(mensaje, "Importación de Productos", MessageBoxButtons.OK, icono);
        }

        private bool ConfirmarReemplazo()
        {
            var result = MessageBox.Show(
                "⚠️ MODO REEMPLAZAR ⚠️\n\n" +
                "Esta operación ELIMINARÁ todos los productos existentes " +
                "y los reemplazará con los datos del archivo.\n\n" +
                "¿Está SEGURO de continuar?",
                "Confirmación de Reemplazo",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            return result == DialogResult.Yes;
        }

        private bool ConfirmarContinuarConErrores()
        {
            var result = MessageBox.Show(
                "Se encontraron errores en los datos.\n\n" +
                "¿Desea continuar con la importación? Los registros con errores serán omitidos.",
                "Errores de Validación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            return result == DialogResult.Yes;
        }

        private void ExportarErroresAExcel()
        {
            if (_erroresValidacion.Count == 0)
            {
                MostrarNotificacion("No hay errores para exportar", TipoNotificacion.Advertencia);
                return;
            }

            saveFileDialog.FileName = $"Errores_Validacion_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Errores");

                        worksheet.Cells[1, 1].Value = "Fila";
                        worksheet.Cells[1, 2].Value = "Hoja";
                        worksheet.Cells[1, 3].Value = "Campo";
                        worksheet.Cells[1, 4].Value = "Error";
                        worksheet.Cells[1, 5].Value = "Severidad";

                        using (var range = worksheet.Cells[1, 1, 1, 5])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 73, 94));
                            range.Style.Font.Color.SetColor(Color.White);
                        }

                        for (int i = 0; i < _erroresValidacion.Count; i++)
                        {
                            var error = _erroresValidacion[i];
                            worksheet.Cells[i + 2, 1].Value = error.Fila;
                            worksheet.Cells[i + 2, 2].Value = error.Hoja;
                            worksheet.Cells[i + 2, 3].Value = error.Campo;
                            worksheet.Cells[i + 2, 4].Value = error.Mensaje;
                            worksheet.Cells[i + 2, 5].Value = error.Severidad;
                        }

                        worksheet.Cells.AutoFitColumns();
                        package.SaveAs(new FileInfo(saveFileDialog.FileName));
                    }

                    MostrarNotificacion("Errores exportados exitosamente", TipoNotificacion.Exito);
                }
                catch (Exception ex)
                {
                    MostrarNotificacion($"Error al exportar: {ex.Message}", TipoNotificacion.Error);
                }
            }
        }

        private void CopiarSeleccionGrid()
        {
            DataGridView grid = null;

            if (tabHojas.SelectedTab == tabProductos)
                grid = dgvProductos;
            else if (tabHojas.SelectedTab == tabVariantes)
                grid = dgvVariantes;
            else if (tabHojas.SelectedTab == tabPresentaciones)
                grid = dgvPresentaciones;
            else
                grid = dgvErrores;

            if (grid != null && grid.SelectedCells.Count > 0)
            {
                var data = grid.GetClipboardContent();
                Clipboard.SetDataObject(data);
            }
        }

        private void ExportarGridAExcel()
        {
            DataGridView grid = null;
            string nombreHoja = "";

            if (tabHojas.SelectedTab == tabProductos)
            {
                grid = dgvProductos;
                nombreHoja = "PRODUCTOS";
            }
            else if (tabHojas.SelectedTab == tabVariantes)
            {
                grid = dgvVariantes;
                nombreHoja = "VARIANTES";
            }
            else if (tabHojas.SelectedTab == tabPresentaciones)
            {
                grid = dgvPresentaciones;
                nombreHoja = "PRESENTACIONES";
            }
            else
            {
                grid = dgvErrores;
                nombreHoja = "ERRORES";
            }

            if (grid == null || grid.Rows.Count == 0) return;

            saveFileDialog.FileName = $"Exportacion_{nombreHoja}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add(nombreHoja);

                        for (int i = 0; i < grid.Columns.Count; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = grid.Columns[i].HeaderText;
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        }

                        for (int i = 0; i < grid.Rows.Count; i++)
                        {
                            for (int j = 0; j < grid.Columns.Count; j++)
                            {
                                var valor = grid.Rows[i].Cells[j].Value;
                                worksheet.Cells[i + 2, j + 1].Value = valor?.ToString();
                            }
                        }

                        worksheet.Cells.AutoFitColumns();
                        package.SaveAs(new FileInfo(saveFileDialog.FileName));
                    }

                    MostrarNotificacion("Datos exportados exitosamente", TipoNotificacion.Exito);
                }
                catch (Exception ex)
                {
                    MostrarNotificacion($"Error al exportar: {ex.Message}", TipoNotificacion.Error);
                }
            }
        }

        private void MostrarAyuda()
        {
            string ayuda = @"INSTRUCCIONES DE IMPORTACIÓN

1. Prepare su archivo Excel con las siguientes hojas (en orden recomendado):
   • CATEGORIAS: Códigos y nombres de categorías
   • MARCAS: Códigos y nombres de marcas
   • UNIDADES: Códigos y nombres de unidades de medida
   • PROVEEDORES: Códigos y datos de proveedores
   • BODEGAS: Códigos y nombres de bodegas
   • PRODUCTOS: Información base de productos
   • VARIANTES: Variantes por producto
   • ATRIBUTOS_DINAMICOS: Definición de atributos
   • VALORES_ATRIBUTOS: Valores posibles
   • VARIANTE_ATRIBUTOS: Asignación de atributos
   • PRESENTACIONES: Unidades de venta/compra
   • CODIGOS_BARRA: Códigos de barra
   • PRECIOS_VENTA: Precios normales de venta
   • PRECIOS_MAYORISTA: Precios mayoristas
   • STOCK_INICIAL: Stock inicial

2. Seleccione el modo de importación:
   • Insertar: Solo crea nuevos registros
   • Actualizar: Actualiza existentes
   • Mixto: Crea nuevos y actualiza existentes
   • Reemplazar: Elimina y recrea todo

3. Valide sus datos antes de importar
4. Revise los errores y advertencias
5. Inicie la importación

Para más información, consulte el manual de usuario.";

            MessageBox.Show(ayuda, "Ayuda - Importación de Productos",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}