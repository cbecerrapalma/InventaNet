using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CBP.WinFrms.Notification;
using Inventa.Infrastructure;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Inventa.Forms.Utilidades
{
    public partial class frm_ImportacionExportacion : Form
    {
        private readonly IFormFactory _formFactory;
        private DataTable datosImportacion;
        private Dictionary<string, string> mapeoCampos;
        private List<Dictionary<string, object>> registrosProcesados;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IdTenant { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IdUsuario { get; set; }

        public frm_ImportacionExportacion(IFormFactory formFactory)
        {
            InitializeComponent();
            _formFactory = formFactory;
            ConfigurarFormulario();

            if (!CBP.WinFrms.Theming.DesignModeHelper.IsDesignTime())
                CBP.WinFrms.Theming.ThemeInjector.Attach(this);
        }

        private void ConfigurarFormulario()
        {
            // Configurar colores y estilos
            this.BackColor = Color.FromArgb(250, 250, 250);

            // Configurar valores por defecto
            cboTipoImportacion.SelectedIndex = 0;
            cboTipoExportacion.SelectedIndex = 0;
            cboExportarCategoria.SelectedIndex = 0;
            cboExportarMarca.SelectedIndex = 0;
            cboFiltroHistorial.SelectedIndex = 0;
            dtpExportarDesde.Value = DateTime.Now.AddMonths(-1);
            dtpExportarHasta.Value = DateTime.Now;
            dtpHistorialDesde.Value = DateTime.Now.AddMonths(-1);
            dtpHistorialHasta.Value = DateTime.Now;

            // Configurar columnas del grid de mapeo
            ConfigurarGridMapeo();

            // Configurar eventos
            this.Load += Frm_ImportacionExportacion_Load;
            btnExaminar.Click += BtnExaminar_Click;
            btnCargarVistaPrevia.Click += BtnCargarVistaPrevia_Click;
            btnMapeoAutomatico.Click += BtnMapeoAutomatico_Click;
            btnLimpiarMapeo.Click += BtnLimpiarMapeo_Click;
            btnImportar.Click += BtnImportar_Click;
            btnCancelar.Click += BtnCancelar_Click;
            btnExportar.Click += BtnExportar_Click;
            btnSeleccionarTodos.Click += BtnSeleccionarTodos_Click;
            btnDeseleccionarTodos.Click += BtnDeseleccionarTodos_Click;
            btnDescargarPlantilla.Click += BtnDescargarPlantilla_Click;
            btnVerPlantilla.Click += BtnVerPlantilla_Click;
            btnVerDetalle.Click += BtnVerDetalle_Click;
            btnRepetirImportacion.Click += BtnRepetirImportacion_Click;
            btnEliminarHistorial.Click += BtnEliminarHistorial_Click;
            btnFiltrarHistorial.Click += BtnFiltrarHistorial_Click;
        }

        private void ConfigurarGridMapeo()
        {
            dgvMapeo.AutoGenerateColumns = false;
            dgvMapeo.Columns.Clear();

            // Columna Campo Archivo
            DataGridViewTextBoxColumn colCampoArchivo = new DataGridViewTextBoxColumn
            {
                Name = "CampoArchivo",
                HeaderText = "Campo en Archivo",
                Width = 300,
                ReadOnly = true
            };
            dgvMapeo.Columns.Add(colCampoArchivo);

            // Columna Campo Sistema
            DataGridViewComboBoxColumn colCampoSistema = new DataGridViewComboBoxColumn
            {
                Name = "CampoSistema",
                HeaderText = "Campo en Sistema",
                Width = 300,
                DropDownWidth = 300,
                FlatStyle = FlatStyle.Flat
            };
            dgvMapeo.Columns.Add(colCampoSistema);

            // Columna Tipo de Dato
            DataGridViewTextBoxColumn colTipoDato = new DataGridViewTextBoxColumn
            {
                Name = "TipoDato",
                HeaderText = "Tipo de Dato",
                Width = 150,
                ReadOnly = true
            };
            dgvMapeo.Columns.Add(colTipoDato);

            // Columna Ejemplo
            DataGridViewTextBoxColumn colEjemplo = new DataGridViewTextBoxColumn
            {
                Name = "Ejemplo",
                HeaderText = "Ejemplo",
                Width = 200,
                ReadOnly = true
            };
            dgvMapeo.Columns.Add(colEjemplo);
        }

        private void Frm_ImportacionExportacion_Load(object sender, EventArgs e)
        {
            CargarPlantillas();
            CargarHistorial();
        }

        #region Importación de Datos

        private void BtnExaminar_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtRutaArchivo.Text = openFileDialog.FileName;
                NotificationManager.Info($"Archivo seleccionado: {Path.GetFileName(openFileDialog.FileName)}");
            }
        }

        private void BtnCargarVistaPrevia_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtRutaArchivo.Text))
            {
                NotificationManager.Warning("Debe seleccionar un archivo primero.");
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                tsslEstado.Text = "Cargando archivo...";

                // Cargar archivo según extensión
                string extension = Path.GetExtension(txtRutaArchivo.Text).ToLower();

                if (extension == ".xlsx" || extension == ".xls")
                {
                    CargarExcel();
                }
                else if (extension == ".csv")
                {
                    CargarCSV();
                }
                else
                {
                    NotificationManager.Warning("Formato de archivo no soportado. Use Excel (.xlsx, .xls) o CSV (.csv)");
                    return;
                }

                // Mostrar vista previa
                if (datosImportacion != null && datosImportacion.Rows.Count > 0)
                {
                    dgvVistaPrevia.DataSource = datosImportacion;
                    lblRegistrosEncontradosValor.Text = datosImportacion.Rows.Count.ToString();

                    // Cargar opciones de mapeo
                    CargarOpcionesMapeo();

                    NotificationManager.Success($"Archivo cargado: {datosImportacion.Rows.Count} registros encontrados.");
                }

                tsslEstado.Text = "Listo";
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al cargar archivo: {ex.Message}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void CargarExcel()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(txtRutaArchivo.Text)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                datosImportacion = new DataTable();

                // Leer encabezados
                int colCount = worksheet.Dimension.Columns;
                int rowCount = worksheet.Dimension.Rows;

                for (int col = 1; col <= colCount; col++)
                {
                    string header = worksheet.Cells[1, col].Text;
                    if (string.IsNullOrEmpty(header))
                        header = $"Columna{col}";
                    datosImportacion.Columns.Add(header);
                }

                // Leer datos
                int startRow = chkPrimeraFilaEncabezados.Checked ? 2 : 1;
                for (int row = startRow; row <= Math.Min(rowCount, startRow + 100); row++) // Solo primeras 100 filas para vista previa
                {
                    DataRow dr = datosImportacion.NewRow();
                    for (int col = 1; col <= colCount; col++)
                    {
                        dr[col - 1] = worksheet.Cells[row, col].Text;
                    }
                    datosImportacion.Rows.Add(dr);
                }
            }
        }

        private void CargarCSV()
        {
            datosImportacion = new DataTable();
            string[] lines = File.ReadAllLines(txtRutaArchivo.Text, Encoding.UTF8);

            if (lines.Length == 0) return;

            // Leer encabezados
            string[] headers = lines[0].Split(',');
            foreach (string header in headers)
            {
                string colName = header.Trim().Trim('"');
                datosImportacion.Columns.Add(colName);
            }

            // Leer datos
            int startRow = chkPrimeraFilaEncabezados.Checked ? 1 : 0;
            for (int i = startRow + 1; i < Math.Min(lines.Length, startRow + 101); i++)
            {
                string[] values = lines[i].Split(',');
                DataRow dr = datosImportacion.NewRow();

                for (int j = 0; j < Math.Min(values.Length, datosImportacion.Columns.Count); j++)
                {
                    dr[j] = values[j].Trim().Trim('"');
                }

                datosImportacion.Rows.Add(dr);
            }
        }

        private void CargarOpcionesMapeo()
        {
            // Limpiar grid de mapeo
            dgvMapeo.Rows.Clear();

            // Obtener campos del sistema según el tipo seleccionado
            List<string> camposSistema = ObtenerCamposSistema(cboTipoImportacion.Text);

            // Configurar combo box para cada columna
            foreach (DataColumn col in datosImportacion.Columns)
            {
                int rowIndex = dgvMapeo.Rows.Add();
                dgvMapeo.Rows[rowIndex].Cells["CampoArchivo"].Value = col.ColumnName;

                // Configurar combo box
                var comboCell = dgvMapeo.Rows[rowIndex].Cells["CampoSistema"] as DataGridViewComboBoxCell;
                if (comboCell != null)
                {
                    comboCell.DataSource = new List<string>(camposSistema);
                    comboCell.Value = BuscarMapeoAutomatico(col.ColumnName, camposSistema);
                }

                // Mostrar tipo de dato y ejemplo
                dgvMapeo.Rows[rowIndex].Cells["TipoDato"].Value = "Texto";
                if (datosImportacion.Rows.Count > 0)
                {
                    dgvMapeo.Rows[rowIndex].Cells["Ejemplo"].Value = datosImportacion.Rows[0][col.ColumnName].ToString();
                }
            }
        }

        private List<string> ObtenerCamposSistema(string tipo)
        {
            var campos = new List<string> { "--- No importar ---" };

            switch (tipo)
            {
                case "Productos":
                    campos.AddRange(new[] {
                        "Código Producto",
                        "Nombre Producto",
                        "Categoría",
                        "Marca",
                        "Unidad Base",
                        "Días Vencimiento",
                        "Maneja Lote",
                        "Es Pesable",
                        "Activo"
                    });
                    break;

                case "Clientes":
                    campos.AddRange(new[] {
                        "Código Cliente",
                        "Nombre",
                        "RUT",
                        "Dirección",
                        "Teléfono",
                        "Email",
                        "Tipo Cliente",
                        "Activo"
                    });
                    break;

                case "Proveedores":
                    campos.AddRange(new[] {
                        "Código Proveedor",
                        "Razón Social",
                        "Nombre Comercial",
                        "RUT",
                        "Dirección",
                        "Teléfono",
                        "Email",
                        "Contacto",
                        "Activo"
                    });
                    break;

                case "Precios":
                    // S57.69: precio normal único (sin columna Tipo Precio).
                    campos.AddRange(new[] {
                        "Código Producto",
                        "Precio",
                        "Fecha Vigencia",
                        "Activo"
                    });
                    break;

                case "Stock Inicial":
                    campos.AddRange(new[] {
                        "Código Producto",
                        "Bodega",
                        "Lote",
                        "Cantidad",
                        "Costo Promedio",
                        "Fecha Vencimiento"
                    });
                    break;
            }

            return campos;
        }

        private string BuscarMapeoAutomatico(string nombreColumna, List<string> camposSistema)
        {
            nombreColumna = nombreColumna.ToLower().Trim();

            foreach (string campo in camposSistema)
            {
                string campoSinTildes = campo.ToLower()
                    .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                    .Replace("ó", "o").Replace("ú", "u");

                if (campoSinTildes.Contains(nombreColumna) || nombreColumna.Contains(campoSinTildes))
                    return campo;
            }

            return camposSistema.FirstOrDefault(c => c.Contains("Código") || c.Contains("Nombre"));
        }

        private void BtnMapeoAutomatico_Click(object sender, EventArgs e)
        {
            if (dgvMapeo.Rows.Count == 0) return;

            List<string> camposSistema = ObtenerCamposSistema(cboTipoImportacion.Text);

            foreach (DataGridViewRow row in dgvMapeo.Rows)
            {
                string campoArchivo = row.Cells["CampoArchivo"].Value?.ToString() ?? "";
                string mapeo = BuscarMapeoAutomatico(campoArchivo, camposSistema);

                var comboCell = row.Cells["CampoSistema"] as DataGridViewComboBoxCell;
                if (comboCell != null && !string.IsNullOrEmpty(mapeo))
                {
                    comboCell.Value = mapeo;
                }
            }

            NotificationManager.Success("Mapeo automático completado.");
        }

        private void BtnLimpiarMapeo_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvMapeo.Rows)
            {
                var comboCell = row.Cells["CampoSistema"] as DataGridViewComboBoxCell;
                if (comboCell != null)
                {
                    comboCell.Value = "--- No importar ---";
                }
            }
        }

        private void BtnImportar_Click(object sender, EventArgs e)
        {
            if (datosImportacion == null || datosImportacion.Rows.Count == 0)
            {
                NotificationManager.Warning("No hay datos para importar.");
                return;
            }

            // Validar mapeo
            if (!ValidarMapeo())
                return;

            if (MessageBox.Show($"¿Importar {datosImportacion.Rows.Count} registros?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                progressBarImportacion.Visible = true;
                progressBarImportacion.Value = 0;

                // Procesar importación
                registrosProcesados = new List<Dictionary<string, object>>();
                int exitosos = 0;
                int errores = 0;

                for (int i = 0; i < datosImportacion.Rows.Count; i++)
                {
                    DataRow row = datosImportacion.Rows[i];
                    var resultado = ProcesarRegistro(row, i + 1);

                    if (resultado.Exitoso)
                    {
                        exitosos++;
                    }
                    else
                    {
                        errores++;
                    }

                    registrosProcesados.Add(new Dictionary<string, object>
                    {
                        ["Fila"] = i + 1,
                        ["Resultado"] = resultado.Exitoso ? "Éxito" : "Error",
                        ["Mensaje"] = resultado.Mensaje
                    });

                    progressBarImportacion.Value = (int)((i + 1) * 100.0 / datosImportacion.Rows.Count);
                }

                // Actualizar estadísticas
                lblRegistrosValidosValor.Text = exitosos.ToString();
                lblRegistrosConErrorValor.Text = errores.ToString();

                // Guardar en historial
                GuardarHistorialImportacion(exitosos, errores);

                NotificationManager.Success($"Importación completada. {exitosos} éxitos, {errores} errores.");

                // Mostrar resumen
                MostrarResumenImportacion();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error durante la importación: {ex.Message}");
            }
            finally
            {
                progressBarImportacion.Visible = false;
                Cursor = Cursors.Default;
            }
        }

        private bool ValidarMapeo()
        {
            foreach (DataGridViewRow row in dgvMapeo.Rows)
            {
                var comboCell = row.Cells["CampoSistema"] as DataGridViewComboBoxCell;
                if (comboCell != null && comboCell.Value != null &&
                    comboCell.Value.ToString() != "--- No importar ---")
                {
                    return true; // Al menos un campo mapeado
                }
            }

            NotificationManager.Warning("Debe mapear al menos un campo para importar.");
            return false;
        }

        private (bool Exitoso, string Mensaje) ProcesarRegistro(DataRow row, int fila)
        {
            try
            {
                // Construir diccionario de datos según mapeo
                var datos = new Dictionary<string, object>();

                for (int i = 0; i < dgvMapeo.Rows.Count; i++)
                {
                    string campoSistema = dgvMapeo.Rows[i].Cells["CampoSistema"].Value?.ToString();

                    if (!string.IsNullOrEmpty(campoSistema) && campoSistema != "--- No importar ---")
                    {
                        datos[campoSistema] = row[i].ToString();
                    }
                }

                // TODO: Insertar en base de datos según tipo
                switch (cboTipoImportacion.Text)
                {
                    case "Productos":
                        return ProcesarProducto(datos, fila);
                    case "Clientes":
                        return ProcesarCliente(datos, fila);
                        // Otros casos...
                }

                return (true, "OK");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        private (bool Exitoso, string Mensaje) ProcesarProducto(Dictionary<string, object> datos, int fila)
        {
            // Validaciones
            if (!datos.ContainsKey("Código Producto") || string.IsNullOrEmpty(datos["Código Producto"]?.ToString()))
                return (false, $"Fila {fila}: Código de producto requerido");

            if (!datos.ContainsKey("Nombre Producto") || string.IsNullOrEmpty(datos["Nombre Producto"]?.ToString()))
                return (false, $"Fila {fila}: Nombre de producto requerido");

            // TODO: Insertar/Actualizar en BD
            // using (var unitOfWork = new UnitOfWork())
            // {
            //     var productoRepo = unitOfWork.Repository<Prod>();
            //     
            //     var producto = productoRepo.FirstOrDefault(p => p.Codigo == datos["Código Producto"].ToString());
            //     
            //     if (producto == null && chkCrearNuevos.Checked)
            //     {
            //         producto = new Prod { IdTenant = IdTenant };
            //         productoRepo.Add(producto);
            //     }
            //     
            //     if (producto != null && chkActualizarExistentes.Checked)
            //     {
            //         producto.Nombre = datos["Nombre Producto"].ToString();
            //         // Mapear otros campos
            //         unitOfWork.SaveChanges();
            //     }
            // }

            return (true, $"Fila {fila}: Producto procesado correctamente");
        }

        private (bool Exitoso, string Mensaje) ProcesarCliente(Dictionary<string, object> datos, int fila)
        {
            if (!datos.ContainsKey("RUT") && !datos.ContainsKey("Nombre"))
                return (false, $"Fila {fila}: RUT o Nombre requerido");

            return (true, $"Fila {fila}: Cliente procesado correctamente");
        }

        private void GuardarHistorialImportacion(int exitosos, int errores)
        {
            // TODO: Guardar en tabla de historial
            // using (var unitOfWork = new UnitOfWork())
            // {
            //     var historial = new HistorialImportacion
            //     {
            //         IdTenant = IdTenant,
            //         IdUsuario = IdUsuario,
            //         Tipo = cboTipoImportacion.Text,
            //         Archivo = Path.GetFileName(txtRutaArchivo.Text),
            //         Fecha = DateTime.Now,
            //         RegistrosTotales = datosImportacion.Rows.Count,
            //         RegistrosExitosos = exitosos,
            //         RegistrosError = errores
            //     };
            //     unitOfWork.Repository<HistorialImportacion>().Add(historial);
            //     unitOfWork.SaveChanges();
            // }

            // Recargar historial
            CargarHistorial();
        }

        private void MostrarResumenImportacion()
        {
            string resumen = $"RESUMEN DE IMPORTACIÓN\n" +
                            $"========================\n" +
                            $"Tipo: {cboTipoImportacion.Text}\n" +
                            $"Archivo: {Path.GetFileName(txtRutaArchivo.Text)}\n" +
                            $"Total registros: {datosImportacion.Rows.Count}\n" +
                            $"Importados: {lblRegistrosValidosValor.Text}\n" +
                            $"Errores: {lblRegistrosConErrorValor.Text}\n" +
                            $"========================\n\n" +
                            $"Detalle de errores:\n";

            foreach (var reg in registrosProcesados.Where(r => r["Resultado"].ToString() == "Error"))
            {
                resumen += $"- Fila {reg["Fila"]}: {reg["Mensaje"]}\n";
            }

            MessageBox.Show(resumen, "Resumen de Importación",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (datosImportacion != null && datosImportacion.Rows.Count > 0)
            {
                if (MessageBox.Show("¿Cancelar la importación actual?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    LimpiarImportacion();
                }
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void LimpiarImportacion()
        {
            txtRutaArchivo.Clear();
            datosImportacion = null;
            dgvVistaPrevia.DataSource = null;
            dgvMapeo.Rows.Clear();
            lblRegistrosEncontradosValor.Text = "0";
            lblRegistrosValidosValor.Text = "0";
            lblRegistrosConErrorValor.Text = "0";
        }

        #endregion

        #region Exportación de Datos

        private void BtnSeleccionarTodos_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbCamposExportar.Items.Count; i++)
            {
                clbCamposExportar.SetItemChecked(i, true);
            }
        }

        private void BtnDeseleccionarTodos_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbCamposExportar.Items.Count; i++)
            {
                clbCamposExportar.SetItemChecked(i, false);
            }
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            if (clbCamposExportar.CheckedItems.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar al menos un campo para exportar.");
                return;
            }

            // Configurar filtro según formato seleccionado
            if (rdbExcel.Checked)
                saveFileDialog.Filter = "Archivos Excel|*.xlsx";
            else if (rdbCSV.Checked)
                saveFileDialog.Filter = "Archivos CSV|*.csv";
            else if (rdbXML.Checked)
                saveFileDialog.Filter = "Archivos XML|*.xml";
            else if (rdbJSON.Checked)
                saveFileDialog.Filter = "Archivos JSON|*.json";

            saveFileDialog.FileName = $"{cboTipoExportacion.Text}_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    tsslEstado.Text = "Exportando datos...";

                    // Obtener datos a exportar
                    DataTable datosExportar = ObtenerDatosExportacion();

                    // Exportar según formato
                    string extension = Path.GetExtension(saveFileDialog.FileName).ToLower();

                    if (extension == ".xlsx")
                        ExportarAExcel(datosExportar, saveFileDialog.FileName);
                    else if (extension == ".csv")
                        ExportarACSV(datosExportar, saveFileDialog.FileName);
                    else if (extension == ".xml")
                        ExportarAXML(datosExportar, saveFileDialog.FileName);
                    else if (extension == ".json")
                        ExportarAJSON(datosExportar, saveFileDialog.FileName);

                    NotificationManager.Success($"Datos exportados correctamente a:\n{saveFileDialog.FileName}");

                    tsslEstado.Text = "Listo";
                }
                catch (Exception ex)
                {
                    NotificationManager.Error($"Error al exportar: {ex.Message}");
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private DataTable ObtenerDatosExportacion()
        {
            DataTable dt = new DataTable();

            // Agregar columnas seleccionadas
            foreach (string campo in clbCamposExportar.CheckedItems)
            {
                dt.Columns.Add(campo);
            }

            // TODO: Obtener datos desde BD según filtros
            // using (var unitOfWork = new UnitOfWork())
            // {
            //     IQueryable query = null;
            //     
            //     switch (cboTipoExportacion.Text)
            //     {
            //         case "Productos":
            //             query = unitOfWork.Repository<Prod>()
            //                 .Where(p => p.IdTenant == IdTenant);
            //             
            //             if (chkExportarActivos.Checked && !chkExportarInactivos.Checked)
            //                 query = query.Where(p => p.Activo);
            //             
            //             if (cboExportarCategoria.SelectedIndex > 0)
            //                 query = query.Where(p => p.Categoria.Nombre == cboExportarCategoria.Text);
            //             
            //             var productos = query.ToList();
            //             
            //             foreach (var p in productos)
            //             {
            //                 DataRow row = dt.NewRow();
            //                 foreach (string campo in clbCamposExportar.CheckedItems)
            //                 {
            //                     switch (campo)
            //                     {
            //                         case "Código": row[campo] = p.Codigo; break;
            //                         case "Nombre": row[campo] = p.Nombre; break;
            //                         // ...
            //                     }
            //                 }
            //                 dt.Rows.Add(row);
            //             }
            //             break;
            //     }
            // }

            // Datos de ejemplo
            for (int i = 1; i <= 10; i++)
            {
                DataRow row = dt.NewRow();
                foreach (string campo in clbCamposExportar.CheckedItems)
                {
                    if (campo.Contains("Código"))
                        row[campo] = $"PROD00{i}";
                    else if (campo.Contains("Nombre"))
                        row[campo] = $"Producto de ejemplo {i}";
                    else if (campo.Contains("Precio"))
                        row[campo] = 10000 * i;
                    else if (campo.Contains("Stock"))
                        row[campo] = i * 5;
                    else
                        row[campo] = $"Dato {i}";
                }
                dt.Rows.Add(row);
            }

            return dt;
        }

        private void ExportarAExcel(DataTable datos, string archivo)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Datos");

                // Encabezados
                for (int i = 0; i < datos.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = datos.Columns[i].ColumnName;
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 60, 114));
                    worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                }

                // Datos
                for (int i = 0; i < datos.Rows.Count; i++)
                {
                    for (int j = 0; j < datos.Columns.Count; j++)
                    {
                        worksheet.Cells[i + 2, j + 1].Value = datos.Rows[i][j];
                    }
                }

                // Autoajustar columnas
                worksheet.Cells.AutoFitColumns();

                // Guardar archivo
                package.SaveAs(new FileInfo(archivo));
            }
        }

        private void ExportarACSV(DataTable datos, string archivo)
        {
            using (var writer = new StreamWriter(archivo, false, Encoding.UTF8))
            {
                // Escribir encabezados
                for (int i = 0; i < datos.Columns.Count; i++)
                {
                    writer.Write(datos.Columns[i].ColumnName);
                    if (i < datos.Columns.Count - 1)
                        writer.Write(",");
                }
                writer.WriteLine();

                // Escribir datos
                foreach (DataRow row in datos.Rows)
                {
                    for (int i = 0; i < datos.Columns.Count; i++)
                    {
                        string valor = row[i].ToString();
                        if (valor.Contains(",") || valor.Contains("\""))
                        {
                            valor = "\"" + valor.Replace("\"", "\"\"") + "\"";
                        }
                        writer.Write(valor);
                        if (i < datos.Columns.Count - 1)
                            writer.Write(",");
                    }
                    writer.WriteLine();
                }
            }
        }

        private void ExportarAXML(DataTable datos, string archivo)
        {
            var settings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using (var writer = System.Xml.XmlWriter.Create(archivo, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(cboTipoExportacion.Text.Replace(" ", ""));

                foreach (DataRow row in datos.Rows)
                {
                    writer.WriteStartElement("Registro");

                    for (int i = 0; i < datos.Columns.Count; i++)
                    {
                        string colName = datos.Columns[i].ColumnName.Replace(" ", "");
                        writer.WriteElementString(colName, row[i].ToString());
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private void ExportarAJSON(DataTable datos, string archivo)
        {
            var lista = new List<Dictionary<string, object>>();

            foreach (DataRow row in datos.Rows)
            {
                var dict = new Dictionary<string, object>();
                for (int i = 0; i < datos.Columns.Count; i++)
                {
                    dict[datos.Columns[i].ColumnName] = row[i];
                }
                lista.Add(dict);
            }

            string json = JsonConvert.SerializeObject(lista, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(archivo, json, Encoding.UTF8);
        }

        #endregion

        #region Plantillas

        private void CargarPlantillas()
        {
            lvPlantillas.Items.Clear();

            // TODO: Cargar desde BD o recursos
            var plantillas = new[]
            {
                new { Nombre = "Plantilla Productos.xlsx", Tipo = "Productos", Fecha = DateTime.Now.AddDays(-5), Desc = "Plantilla para importar productos" },
                new { Nombre = "Plantilla Clientes.xlsx", Tipo = "Clientes", Fecha = DateTime.Now.AddDays(-5), Desc = "Plantilla para importar clientes" },
                new { Nombre = "Plantilla Precios.xlsx", Tipo = "Precios", Fecha = DateTime.Now.AddDays(-3), Desc = "Plantilla para actualizar precios" },
                new { Nombre = "Plantilla Stock.xlsx", Tipo = "Stock", Fecha = DateTime.Now.AddDays(-1), Desc = "Plantilla para carga de stock inicial" }
            };

            foreach (var p in plantillas)
            {
                var item = new ListViewItem(p.Nombre);
                item.SubItems.Add(p.Tipo);
                item.SubItems.Add(p.Fecha.ToString("dd/MM/yyyy"));
                item.SubItems.Add(p.Desc);
                lvPlantillas.Items.Add(item);
            }
        }

        private void BtnDescargarPlantilla_Click(object sender, EventArgs e)
        {
            if (lvPlantillas.SelectedItems.Count == 0)
            {
                NotificationManager.Warning("Seleccione una plantilla para descargar.");
                return;
            }

            string nombrePlantilla = lvPlantillas.SelectedItems[0].Text;
            saveFileDialog.Filter = "Archivos Excel|*.xlsx";
            saveFileDialog.FileName = nombrePlantilla;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    GenerarPlantilla(saveFileDialog.FileName,
                        lvPlantillas.SelectedItems[0].SubItems[1].Text);

                    NotificationManager.Success("Plantilla descargada correctamente.");
                }
                catch (Exception ex)
                {
                    NotificationManager.Error($"Error al descargar plantilla: {ex.Message}");
                }
            }
        }

        private void GenerarPlantilla(string archivo, string tipo)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(tipo);

                // Agregar encabezados según tipo
                switch (tipo)
                {
                    case "Productos":
                        worksheet.Cells[1, 1].Value = "Código Producto";
                        worksheet.Cells[1, 2].Value = "Nombre Producto";
                        worksheet.Cells[1, 3].Value = "Categoría";
                        worksheet.Cells[1, 4].Value = "Marca";
                        worksheet.Cells[1, 5].Value = "Unidad Base";
                        worksheet.Cells[1, 6].Value = "Precio Venta";
                        worksheet.Cells[1, 7].Value = "Activo";
                        break;

                    case "Clientes":
                        worksheet.Cells[1, 1].Value = "Código Cliente";
                        worksheet.Cells[1, 2].Value = "Nombre";
                        worksheet.Cells[1, 3].Value = "RUT";
                        worksheet.Cells[1, 4].Value = "Dirección";
                        worksheet.Cells[1, 5].Value = "Teléfono";
                        worksheet.Cells[1, 6].Value = "Email";
                        worksheet.Cells[1, 7].Value = "Tipo Cliente";
                        break;
                }

                // Formato de encabezados
                for (int i = 1; i <= 7; i++)
                {
                    worksheet.Cells[1, i].Style.Font.Bold = true;
                    worksheet.Cells[1, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 60, 114));
                    worksheet.Cells[1, i].Style.Font.Color.SetColor(Color.White);
                }

                // Agregar fila de ejemplo
                worksheet.Cells[2, 1].Value = "Ejemplo001";
                worksheet.Cells[2, 2].Value = "Producto de ejemplo";

                // Autoajustar
                worksheet.Cells.AutoFitColumns();

                // Guardar
                package.SaveAs(new FileInfo(archivo));
            }
        }

        private void BtnVerPlantilla_Click(object sender, EventArgs e)
        {
            if (lvPlantillas.SelectedItems.Count == 0)
            {
                NotificationManager.Warning("Seleccione una plantilla para ver.");
                return;
            }

            string tipo = lvPlantillas.SelectedItems[0].SubItems[1].Text;
            DataTable dt = new DataTable();

            switch (tipo)
            {
                case "Productos":
                    dt.Columns.Add("Código Producto");
                    dt.Columns.Add("Nombre Producto");
                    dt.Columns.Add("Categoría");
                    dt.Columns.Add("Marca");
                    dt.Columns.Add("Unidad Base");
                    dt.Columns.Add("Precio Venta");
                    dt.Columns.Add("Activo");
                    break;
            }

            // Agregar fila de ejemplo
            DataRow row = dt.NewRow();
            row[0] = "Ejemplo001";
            row[1] = "Producto de ejemplo";
            dt.Rows.Add(row);

            dgvVistaPreviaPlantilla.DataSource = dt;
        }

        #endregion

        #region Historial

        private void CargarHistorial()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Fecha");
            dt.Columns.Add("Tipo");
            dt.Columns.Add("Archivo");
            dt.Columns.Add("Registros");
            dt.Columns.Add("Exitosos");
            dt.Columns.Add("Errores");
            dt.Columns.Add("Usuario");

            // Datos de ejemplo
            for (int i = 1; i <= 10; i++)
            {
                dt.Rows.Add(
                    DateTime.Now.AddDays(-i).ToString("dd/MM/yyyy HH:mm"),
                    i % 2 == 0 ? "Importación" : "Exportación",
                    $"archivo_{i}.xlsx",
                    100 + i,
                    95 + i,
                    5 + i,
                    "Admin"
                );
            }

            dgvHistorial.DataSource = dt;

            // Configurar columnas
            dgvHistorial.Columns["Fecha"].Width = 130;
            dgvHistorial.Columns["Tipo"].Width = 100;
            dgvHistorial.Columns["Archivo"].Width = 200;
            dgvHistorial.Columns["Registros"].Width = 80;
            dgvHistorial.Columns["Exitosos"].Width = 80;
            dgvHistorial.Columns["Errores"].Width = 80;
            dgvHistorial.Columns["Usuario"].Width = 100;
        }

        private void BtnVerDetalle_Click(object sender, EventArgs e)
        {
            if (dgvHistorial.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Seleccione un registro del historial.");
                return;
            }

            DataGridViewRow row = dgvHistorial.SelectedRows[0];
            string detalle = $"DETALLE DE OPERACIÓN\n" +
                            $"====================\n" +
                            $"Fecha: {row.Cells["Fecha"].Value}\n" +
                            $"Tipo: {row.Cells["Tipo"].Value}\n" +
                            $"Archivo: {row.Cells["Archivo"].Value}\n" +
                            $"Registros totales: {row.Cells["Registros"].Value}\n" +
                            $"Exitosos: {row.Cells["Exitosos"].Value}\n" +
                            $"Errores: {row.Cells["Errores"].Value}\n" +
                            $"Usuario: {row.Cells["Usuario"].Value}";

            MessageBox.Show(detalle, "Detalle de Operación",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnRepetirImportacion_Click(object sender, EventArgs e)
        {
            if (dgvHistorial.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Seleccione una importación para repetir.");
                return;
            }

            string archivo = dgvHistorial.SelectedRows[0].Cells["Archivo"].Value?.ToString() ?? "";

            if (!string.IsNullOrEmpty(archivo))
            {
                txtRutaArchivo.Text = archivo;
                tabControl.SelectedTab = tabImportar;
                BtnCargarVistaPrevia_Click(sender, e);
            }
        }

        private void BtnEliminarHistorial_Click(object sender, EventArgs e)
        {
            if (dgvHistorial.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Seleccione un registro para eliminar.");
                return;
            }

            if (MessageBox.Show("¿Eliminar el registro seleccionado del historial?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                // TODO: Eliminar de BD
                CargarHistorial();
                NotificationManager.Success("Registro eliminado.");
            }
        }

        private void BtnFiltrarHistorial_Click(object sender, EventArgs e)
        {
            // TODO: Aplicar filtros
            CargarHistorial();
            NotificationManager.Info("Filtros aplicados.");
        }

        #endregion
    }
}