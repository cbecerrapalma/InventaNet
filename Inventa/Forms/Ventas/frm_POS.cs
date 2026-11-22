using CBP.WinFrms.Notification;
using Inventa.Forms.Mantenedores;
using Inventa.Infrastructure;
using Inventa.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Inventa.Forms.Ventas
{
    public partial class frm_POS : Form
    {
        private readonly IFormFactory _formFactory;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IdTenant { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IdUsuario { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IdCaja { get; set; }

        // Clase para items del carrito
        private class CarritoItem
        {
            public long IdPresProd { get; set; }
            public string CodigoBarras { get; set; }
            public string Producto { get; set; }
            public decimal Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Descuento { get; set; }
            public decimal Subtotal => Cantidad * PrecioUnitario;
            public decimal Total => Subtotal - Descuento;
        }

        private BindingList<CarritoItem> carrito;
        private System.Windows.Forms.Timer busquedaTimer;
        private int idVentaActual;

        public frm_POS(IFormFactory formFactory)
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
            this.BackColor = AppColors.BackgroundLight;
            this.KeyPreview = true;

            // Configurar timer para búsqueda diferida
            busquedaTimer = new System.Windows.Forms.Timer { Interval = 500 };
            busquedaTimer.Tick += BusquedaTimer_Tick;

            // Inicializar carrito
            carrito = new BindingList<CarritoItem>();
            dgvCarrito.DataSource = carrito;

            // Configurar columnas del DataGridView
            ConfigurarGridCarrito();

            // Configurar eventos
            this.Load += Frm_POS_Load;
            this.KeyDown += Frm_POS_KeyDown;

            // Eventos de búsqueda
            txtCodigoBarra.KeyDown += TxtCodigoBarra_KeyDown;
            txtCodigoBarra.TextChanged += TxtCodigoBarra_TextChanged;
            txtProductoBusqueda.TextChanged += TxtProductoBusqueda_TextChanged;
            btnBuscar.Click += BtnBuscar_Click;
            btnAgregar.Click += BtnAgregar_Click;

            // Eventos de cliente
            cboCliente.SelectedIndexChanged += CboCliente_SelectedIndexChanged;
            btnNuevoCliente.Click += BtnNuevoCliente_Click;

            // Eventos de pago
            numDescuento.ValueChanged += NumDescuento_ValueChanged;
            numMontoRecibido.ValueChanged += NumMontoRecibido_ValueChanged;
            cboMetodoPago.SelectedIndexChanged += CboMetodoPago_SelectedIndexChanged;

            // Eventos de botones
            btnPagar.Click += BtnPagar_Click;
            btnCancelarVenta.Click += BtnCancelarVenta_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
            btnCerrar.Click += BtnCerrar_Click;

            // Eventos del carrito
            dgvCarrito.CellEndEdit += DgvCarrito_CellEndEdit;
            dgvCarrito.UserDeletedRow += DgvCarrito_UserDeletedRow;
        }

        private void ConfigurarGridCarrito()
        {
            dgvCarrito.AutoGenerateColumns = false;
            dgvCarrito.Columns.Clear();

            // Columna Código
            DataGridViewTextBoxColumn colCodigo = new DataGridViewTextBoxColumn
            {
                Name = "CodigoBarras",
                HeaderText = "Código",
                DataPropertyName = "CodigoBarras",
                Width = 100,
                ReadOnly = true
            };
            dgvCarrito.Columns.Add(colCodigo);

            // Columna Producto
            DataGridViewTextBoxColumn colProducto = new DataGridViewTextBoxColumn
            {
                Name = "Producto",
                HeaderText = "Producto",
                DataPropertyName = "Producto",
                Width = 250,
                ReadOnly = true
            };
            dgvCarrito.Columns.Add(colProducto);

            // Columna Cantidad
            DataGridViewTextBoxColumn colCantidad = new DataGridViewTextBoxColumn
            {
                Name = "Cantidad",
                HeaderText = "Cantidad",
                DataPropertyName = "Cantidad",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            };
            dgvCarrito.Columns.Add(colCantidad);

            // Columna Precio Unitario
            DataGridViewTextBoxColumn colPrecio = new DataGridViewTextBoxColumn
            {
                Name = "PrecioUnitario",
                HeaderText = "Precio Unit.",
                DataPropertyName = "PrecioUnitario",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                },
                ReadOnly = true
            };
            dgvCarrito.Columns.Add(colPrecio);

            // Columna Descuento
            DataGridViewTextBoxColumn colDescuento = new DataGridViewTextBoxColumn
            {
                Name = "Descuento",
                HeaderText = "Dto.",
                DataPropertyName = "Descuento",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            };
            dgvCarrito.Columns.Add(colDescuento);

            // Columna Subtotal
            DataGridViewTextBoxColumn colSubtotal = new DataGridViewTextBoxColumn
            {
                Name = "Subtotal",
                HeaderText = "Subtotal",
                DataPropertyName = "Subtotal",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                },
                ReadOnly = true
            };
            dgvCarrito.Columns.Add(colSubtotal);

            // Columna Total
            DataGridViewTextBoxColumn colTotal = new DataGridViewTextBoxColumn
            {
                Name = "Total",
                HeaderText = "Total",
                DataPropertyName = "Total",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = AppColors.Primary
                },
                ReadOnly = true
            };
            dgvCarrito.Columns.Add(colTotal);

            // Columna Id (oculta)
            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn
            {
                Name = "IdPresProd",
                DataPropertyName = "IdPresProd",
                Visible = false
            };
            dgvCarrito.Columns.Add(colId);

            // Permitir edición en cantidad y descuento
            dgvCarrito.ReadOnly = false;
            dgvCarrito.Columns["Cantidad"].ReadOnly = false;
            dgvCarrito.Columns["Descuento"].ReadOnly = false;
        }

        private void Frm_POS_Load(object sender, EventArgs e)
        {
            CargarDatosIniciales();
            ActualizarFechaHora();
            txtCodigoBarra.Focus();
        }

        private void CargarDatosIniciales()
        {
            try
            {
                // TODO: Cargar desde repositorios
                // using (var unitOfWork = new UnitOfWork())
                // {
                //     // Clientes
                //     var clientes = unitOfWork.Repository<Cliente>()
                //         .Find(c => c.IdTenant == IdTenant && c.Activo)
                //         .OrderBy(c => c.Nombre)
                //         .ToList();
                //     
                //     cboCliente.DataSource = clientes;
                //     cboCliente.DisplayMember = "Nombre";
                //     cboCliente.ValueMember = "IdCliente";
                //     
                //     // Métodos de pago
                //     var metodosPago = unitOfWork.Repository<MetodoPago>()
                //         .Where(m => m.Activo)
                //         .ToList();
                //     
                //     cboMetodoPago.DataSource = metodosPago;
                //     cboMetodoPago.DisplayMember = "Nombre";
                //     cboMetodoPago.ValueMember = "IdMetodoPago";
                // }

                // Datos de ejemplo
                var clientes = new[]
                {
                    new { Id = 1, Nombre = "PÚBLICO GENERAL", RUT = "Sin RUT", Telefono = "" },
                    new { Id = 2, Nombre = "JUAN PÉREZ", RUT = "12.345.678-9", Telefono = "+56 9 1234 5678" },
                    new { Id = 3, Nombre = "MARÍA GONZÁLEZ", RUT = "23.456.789-0", Telefono = "+56 9 2345 6789" },
                    new { Id = 4, Nombre = "EMPRESA ABC LTDA", RUT = "76.543.210-8", Telefono = "+56 2 2345 6789" }
                };

                cboCliente.DataSource = clientes.ToList();
                cboCliente.DisplayMember = "Nombre";
                cboCliente.ValueMember = "Id";

                var metodosPago = new[]
                {
                    new { Id = 1, Nombre = "EFECTIVO" },
                    new { Id = 2, Nombre = "TARJETA DÉBITO" },
                    new { Id = 3, Nombre = "TARJETA CRÉDITO" },
                    new { Id = 4, Nombre = "TRANSFERENCIA" }
                };

                cboMetodoPago.DataSource = metodosPago.ToList();
                cboMetodoPago.DisplayMember = "Nombre";
                cboMetodoPago.ValueMember = "Id";

                // Configurar valores por defecto
                cboCliente.SelectedIndex = 0; // Público General
                cboMetodoPago.SelectedIndex = 0; // Efectivo
                numDescuento.Value = 0;
                numMontoRecibido.Value = 0;

                // Información de caja
                IdCaja = 1;
                lblCajaValor.Text = "CAJA 001";
                tsslCaja.Text = "Caja: 001";
                tsslUsuario.Text = $"Usuario: {IdUsuario}";
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al cargar datos: {ex.Message}");
            }
        }

        private void ActualizarFechaHora()
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) =>
            {
                lblFecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            };
            timer.Start();
        }

        #region Búsqueda de Productos

        private void TxtCodigoBarra_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BuscarProductoPorCodigo(txtCodigoBarra.Text);
            }
        }

        private void TxtCodigoBarra_TextChanged(object sender, EventArgs e)
        {
            busquedaTimer.Stop();
            busquedaTimer.Start();
        }

        private void TxtProductoBusqueda_TextChanged(object sender, EventArgs e)
        {
            busquedaTimer.Stop();
            busquedaTimer.Start();
        }

        private void BusquedaTimer_Tick(object sender, EventArgs e)
        {
            busquedaTimer.Stop();

            if (!string.IsNullOrWhiteSpace(txtProductoBusqueda.Text))
            {
                BuscarProductosPorNombre(txtProductoBusqueda.Text);
            }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtCodigoBarra.Text))
            {
                BuscarProductoPorCodigo(txtCodigoBarra.Text);
            }
            else if (!string.IsNullOrWhiteSpace(txtProductoBusqueda.Text))
            {
                BuscarProductosPorNombre(txtProductoBusqueda.Text);
            }
            else
            {
                NotificationManager.Warning("Ingrese un código de barras o nombre de producto.");
            }
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtCodigoBarra.Text))
            {
                AgregarProductoPorCodigo(txtCodigoBarra.Text);
            }
            else if (!string.IsNullOrWhiteSpace(txtProductoBusqueda.Text))
            {
                // Si hay búsqueda por nombre, mostrar selector
                MostrarSelectorProductos(txtProductoBusqueda.Text);
            }
            else
            {
                NotificationManager.Warning("Ingrese un código de barras o nombre de producto.");
            }
        }

        private void BuscarProductoPorCodigo(string codigo)
        {
            try
            {
                // TODO: Buscar en repositorio
                // using (var unitOfWork = new UnitOfWork())
                // {
                //     var producto = unitOfWork.Repository<CodBar>()
                //         .Find(c => c.Codigo == codigo && c.Activo)
                //         .Select(c => new {
                //             c.IdPresProd,
                //             c.Codigo,
                //             Producto = c.Presentacion.Descripcion,
                //             Precio = c.Presentacion.Precios
                //                 .Where(p => p.Activo && p.FecVig <= DateTime.Today)
                //                 .OrderByDescending(p => p.FecVig)
                //                 .Select(p => p.Valor)
                //                 .FirstOrDefault()
                //         })
                //         .FirstOrDefault();
                //     
                //     if (producto != null)
                //     {
                //         MostrarResultadoBusqueda(producto);
                //     }
                //     else
                //     {
                //         NotificationManager.Warning("Producto no encontrado.");
                //     }
                // }

                // Simular búsqueda
                if (codigo == "1234567890123" || codigo == "PROD001")
                {
                    var producto = new
                    {
                        IdPresProd = 1L,
                        Codigo = codigo,
                        Producto = "NOTEBOOK HP 15",
                        Precio = 699990m
                    };
                    MostrarResultadoBusqueda(producto);
                }
                else if (codigo == "2345678901234" || codigo == "PROD002")
                {
                    var producto = new
                    {
                        IdPresProd = 2L,
                        Codigo = codigo,
                        Producto = "MOUSE LOGITECH",
                        Precio = 29990m
                    };
                    MostrarResultadoBusqueda(producto);
                }
                else
                {
                    NotificationManager.Warning("Producto no encontrado.");
                }
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error en búsqueda: {ex.Message}");
            }
        }

        private void BuscarProductosPorNombre(string nombre)
        {
            try
            {
                // TODO: Buscar en repositorio
                // using (var unitOfWork = new UnitOfWork())
                // {
                //     var productos = unitOfWork.Repository<PresProd>()
                //         .Where(p => p.Descripcion.Contains(nombre) && p.Activo)
                //         .Select(p => new {
                //             p.IdPresProd,
                //             Codigo = p.Codigos.FirstOrDefault().Codigo,
                //             Producto = p.Descripcion,
                //             Precio = p.Precios
                //                 .Where(pr => pr.Activo && pr.FecVig <= DateTime.Today)
                //                 .OrderByDescending(pr => pr.FecVig)
                //                 .Select(pr => pr.Valor)
                //                 .FirstOrDefault()
                //         })
                //         .Take(10)
                //         .ToList();
                //     
                //     if (productos.Any())
                //     {
                //         MostrarSelectorProductos(productos);
                //     }
                // }

                // Simular búsqueda
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    NotificationManager.Info($"Se encontraron productos para: {nombre}");
                    // Aquí se mostraría un formulario de selección
                }
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error en búsqueda: {ex.Message}");
            }
        }

        private void MostrarResultadoBusqueda(dynamic producto)
        {
            txtCodigoBarra.Text = producto.Codigo;
            txtProductoBusqueda.Text = producto.Producto;

            // Preguntar si desea agregar
            if (MessageBox.Show($"¿Agregar {producto.Producto} al carrito?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                AgregarProductoAlCarrito(producto.IdPresProd, producto.Codigo, producto.Producto, producto.Precio);
            }
        }

        private void MostrarSelectorProductos(string busqueda)
        {
            // TODO: Implementar formulario de selección de productos
            NotificationManager.Info("Múltiples productos encontrados. Seleccione uno.");

            // Ejemplo de cómo abrir un selector
            // var selector = _formFactory.Create<frm_SelectorProducto>();
            // selector.Busqueda = busqueda;
            // if (selector.ShowDialog() == DialogResult.OK)
            // {
            //     AgregarProductoAlCarrito(selector.IdPresProd, selector.Codigo, selector.Producto, selector.Precio);
            // }
        }

        private void AgregarProductoPorCodigo(string codigo)
        {
            BuscarProductoPorCodigo(codigo);
        }

        private void AgregarProductoAlCarrito(long idPresProd, string codigo, string producto, decimal precio)
        {
            // Verificar si ya existe en el carrito
            var existente = carrito.FirstOrDefault(i => i.IdPresProd == idPresProd);

            if (existente != null)
            {
                existente.Cantidad += 1;
                dgvCarrito.Refresh();
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    IdPresProd = idPresProd,
                    CodigoBarras = codigo,
                    Producto = producto,
                    Cantidad = 1,
                    PrecioUnitario = precio,
                    Descuento = 0
                });
            }

            // Limpiar campo de búsqueda y enfocar
            txtCodigoBarra.Clear();
            txtCodigoBarra.Focus();

            // Actualizar totales
            ActualizarTotales();
        }

        #endregion

        #region Gestión de Clientes

        private void CboCliente_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboCliente.SelectedItem != null)
            {
                dynamic cliente = cboCliente.SelectedItem;
                lblRUTValor.Text = cliente.RUT ?? "-";
                lblTelefonoValor.Text = cliente.Telefono ?? "-";
            }
        }

        private void BtnNuevoCliente_Click(object sender, EventArgs e)
        {
            var result = _formFactory.ShowDialog<frm_Cliente>(form =>
            {
                form.IdTenant = IdTenant;
                //form.EsNuevo = true;
            });

            if (result == DialogResult.OK)
            {
                // Recargar lista de clientes
                CargarDatosIniciales();
                NotificationManager.Success("Cliente creado correctamente.");
            }
        }

        #endregion

        #region Cálculos y Actualizaciones

        private void ActualizarTotales()
        {
            decimal subtotal = carrito.Sum(i => i.Subtotal);
            decimal descuentoGlobal = numDescuento.Value;
            decimal descuentoMonto = subtotal * (descuentoGlobal / 100);
            decimal subtotalConDescuento = subtotal - descuentoMonto;
            decimal impuesto = subtotalConDescuento * 0.19m; // 19% IVA
            decimal total = subtotalConDescuento + impuesto;

            lblSubtotalValor.Text = subtotal.ToString("C2");
            lblImpuestoValor.Text = impuesto.ToString("C2");
            lblTotalValor.Text = total.ToString("C2");

            // Actualizar resumen
            lblItemsValor.Text = carrito.Count.ToString();
            lblCantidadTotalValor.Text = carrito.Sum(i => i.Cantidad).ToString("N0");

            // Calcular cambio
            CalcularCambio();
        }

        private void CalcularCambio()
        {
            decimal total = decimal.Parse(lblTotalValor.Text, System.Globalization.NumberStyles.Currency);
            decimal recibido = numMontoRecibido.Value;
            decimal cambio = recibido - total;

            if (cambio >= 0)
            {
                lblCambioValor.Text = cambio.ToString("C2");
                lblCambioValor.ForeColor = Color.FromArgb(39, 174, 96);
            }
            else
            {
                lblCambioValor.Text = "$ 0.00";
                lblCambioValor.ForeColor = Color.Gray;
            }
        }

        private void NumDescuento_ValueChanged(object sender, EventArgs e)
        {
            ActualizarTotales();
        }

        private void NumMontoRecibido_ValueChanged(object sender, EventArgs e)
        {
            CalcularCambio();
        }

        private void CboMetodoPago_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Habilitar/deshabilitar monto recibido según método
            if (cboMetodoPago.SelectedIndex == 0) // Efectivo
            {
                numMontoRecibido.Enabled = true;
                numMontoRecibido.Value = 0;
            }
            else
            {
                numMontoRecibido.Enabled = false;
                numMontoRecibido.Value = decimal.Parse(lblTotalValor.Text, System.Globalization.NumberStyles.Currency);
            }
        }

        #endregion

        #region Eventos del Carrito

        private void DgvCarrito_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string columnName = dgvCarrito.Columns[e.ColumnIndex].Name;

                if (columnName == "Cantidad" || columnName == "Descuento")
                {
                    var item = carrito[e.RowIndex];

                    if (columnName == "Cantidad" && item.Cantidad <= 0)
                    {
                        NotificationManager.Warning("La cantidad debe ser mayor a cero.");
                        item.Cantidad = 1;
                    }

                    if (columnName == "Descuento" && item.Descuento < 0)
                    {
                        NotificationManager.Warning("El descuento no puede ser negativo.");
                        item.Descuento = 0;
                    }

                    if (item.Descuento > item.Subtotal)
                    {
                        NotificationManager.Warning("El descuento no puede ser mayor al subtotal.");
                        item.Descuento = item.Subtotal;
                    }

                    dgvCarrito.Refresh();
                    ActualizarTotales();
                }
            }
        }

        private void DgvCarrito_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            ActualizarTotales();
        }

        #endregion

        #region Acciones de Venta

        private void BtnPagar_Click(object sender, EventArgs e)
        {
            if (carrito.Count == 0)
            {
                NotificationManager.Warning("No hay productos en el carrito.");
                return;
            }

            decimal total = decimal.Parse(lblTotalValor.Text, System.Globalization.NumberStyles.Currency);
            decimal recibido = numMontoRecibido.Value;

            if (cboMetodoPago.SelectedIndex == 0 && recibido < total)
            {
                NotificationManager.Warning("El monto recibido es insuficiente.");
                numMontoRecibido.Focus();
                return;
            }

            if (MessageBox.Show($"Confirmar venta por {lblTotalValor.Text}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;

                // TODO: Guardar venta en base de datos
                // using (var unitOfWork = new UnitOfWork())
                // {
                //     // Crear cabecera de venta
                //     var venta = new Venta
                //     {
                //         IdTenant = IdTenant,
                //         IdBodega = 1, // Bodega por defecto
                //         NumVenta = GenerarNumeroVenta(),
                //         FecVenta = DateTime.Now,
                //         IdCliente = (int?)cboCliente.SelectedValue > 0 ? (int?)cboCliente.SelectedValue : null,
                //         TipoDoc = 1, // Boleta
                //         IdUsuario = IdUsuario,
                //         SubTotal = decimal.Parse(lblSubtotalValor.Text, System.Globalization.NumberStyles.Currency),
                //         Descuento = total * (numDescuento.Value / 100),
                //         Impuesto = decimal.Parse(lblImpuestoValor.Text, System.Globalization.NumberStyles.Currency),
                //         Total = total,
                //         Estado = 1 // Completada
                //     };
                //     
                //     unitOfWork.Repository<Venta>().Add(venta);
                //     
                //     // Crear detalles
                //     foreach (var item in carrito)
                //     {
                //         var detalle = new VentaDetalle
                //         {
                //             IdVenta = venta.IdVenta,
                //             NumLinea = carrito.IndexOf(item) + 1,
                //             IdPresProd = item.IdPresProd,
                //             Cantidad = item.Cantidad,
                //             PrecioUnitario = item.PrecioUnitario,
                //             Descuento = item.Descuento,
                //             Subtotal = item.Subtotal
                //         };
                //         unitOfWork.Repository<VentaDetalle>().Add(detalle);
                //         
                //         // Registrar movimiento de inventario
                //         var mov = new MovInv
                //         {
                //             IdTenant = IdTenant,
                //             IdBodega = 1,
                //             IdPresProd = item.IdPresProd,
                //             FecMov = DateTime.Now,
                //             TipoMov = 2, // Venta
                //             CantCom = item.Cantidad,
                //             CantBase = item.Cantidad // Asumiendo misma unidad
                //         };
                //         unitOfWork.Repository<MovInv>().Add(mov);
                //     }
                //     
                //     unitOfWork.SaveChanges();
                //     idVentaActual = venta.IdVenta;
                // }

                // Simular venta
                System.Threading.Thread.Sleep(500);
                idVentaActual = new Random().Next(1000, 9999);

                // Mostrar comprobante
                MostrarComprobante();

                NotificationManager.Success($"Venta {idVentaActual} completada exitosamente.");

                // Limpiar carrito para nueva venta
                LimpiarCarrito();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al procesar venta: {ex.Message}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void MostrarComprobante()
        {
            // TODO: Mostrar comprobante de venta
            string comprobante = $@"
═══════════════════════════════
      COMPROBANTE DE VENTA
═══════════════════════════════
Venta N°: {idVentaActual}
Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}
Cliente: {(cboCliente.SelectedItem as dynamic).Nombre}
═══════════════════════════════

{string.Join("\n", carrito.Select(i => $"{i.Producto,-30} x{i.Cantidad,5}\n{i.PrecioUnitario,35:C2}  {i.Total,15:C2}"))}

═══════════════════════════════
Subtotal: {lblSubtotalValor.Text,30}
Dto. {numDescuento.Value}%: {(decimal.Parse(lblSubtotalValor.Text, System.Globalization.NumberStyles.Currency) * numDescuento.Value / 100),30:C2}
IVA 19%: {lblImpuestoValor.Text,30}
TOTAL: {lblTotalValor.Text,30}
═══════════════════════════════
Método Pago: {(cboMetodoPago.SelectedItem as dynamic).Nombre}
{(cboMetodoPago.SelectedIndex == 0 ? $"Recibido: {numMontoRecibido.Value:C2}\nCambio: {lblCambioValor.Text}" : "")}
═══════════════════════════════
";

            MessageBox.Show(comprobante, "Comprobante de Venta",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCancelarVenta_Click(object sender, EventArgs e)
        {
            if (carrito.Count > 0)
            {
                if (MessageBox.Show("¿Está seguro de cancelar la venta actual?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    LimpiarCarrito();
                }
            }
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            if (carrito.Count > 0)
            {
                if (MessageBox.Show("¿Limpiar todos los productos del carrito?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    LimpiarCarrito();
                }
            }
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            if (carrito.Count > 0)
            {
                if (MessageBox.Show("Hay productos en el carrito. ¿Está seguro de cerrar el POS?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }

        private void LimpiarCarrito()
        {
            carrito.Clear();
            txtCodigoBarra.Clear();
            txtProductoBusqueda.Clear();
            numDescuento.Value = 0;
            numMontoRecibido.Value = 0;
            cboCliente.SelectedIndex = 0;
            cboMetodoPago.SelectedIndex = 0;
            ActualizarTotales();
            txtCodigoBarra.Focus();
        }

        #endregion

        #region Atajos de Teclado

        private void Frm_POS_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F2:
                    e.Handled = true;
                    BtnPagar_Click(sender, e);
                    break;

                case Keys.F3:
                    e.Handled = true;
                    BtnCancelarVenta_Click(sender, e);
                    break;

                case Keys.F4:
                    e.Handled = true;
                    BtnLimpiar_Click(sender, e);
                    break;

                case Keys.F5:
                    e.Handled = true;
                    txtCodigoBarra.Focus();
                    break;

                case Keys.F6:
                    e.Handled = true;
                    cboCliente.Focus();
                    break;

                case Keys.F8:
                    e.Handled = true;
                    numDescuento.Focus();
                    break;

                case Keys.F9:
                    e.Handled = true;
                    numMontoRecibido.Focus();
                    break;

                case Keys.Escape:
                    e.Handled = true;
                    if (carrito.Count > 0)
                    {
                        if (MessageBox.Show("¿Cancelar venta actual?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.No)
                        {
                            LimpiarCarrito();
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Métodos Públicos

        public void SetDatosIniciales(int idTenant, int idUsuario, int idCaja = 1)
        {
            IdTenant = idTenant;
            IdUsuario = idUsuario;
            IdCaja = idCaja;
        }

        #endregion
    }
}