using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CBP.WinFrms.Notification;
using Inventa.Forms.Compras;
using Inventa.Forms.Mantenedores;
using Inventa.Forms.Productos;
using Inventa.Infrastructure;

namespace Inventa.Forms
{
    public partial class frm_Principal : Form
    {
        private readonly IFormFactory _formFactory;
        private readonly Dictionary<string, Form> _moduleRegistry = new();
        private bool _isClosing;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IdTenant { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TenantNombre { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IdUsuario { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string UsuarioNombre { get; set; }

        // Clase para items del TreeView
        private class MenuItemNode
        {
            public string Nombre { get; set; }
            public string Formulario { get; set; }
            public Image Icono { get; set; }
            public List<MenuItemNode> Hijos { get; set; }

            public MenuItemNode()
            {
                Hijos = new List<MenuItemNode>();
            }
        }

        public frm_Principal(IFormFactory formFactory)
        {
            InitializeComponent();
            _formFactory = formFactory;

            if (!CBP.WinFrms.Theming.DesignModeHelper.IsDesignTime())
            {
                IsMdiContainer = true;
                CBP.WinFrms.Theming.ThemeInjector.Attach(this);
            }

            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            timerReloj.Interval = 1000;
            timerReloj.Tick += TimerReloj_Tick;
            timerReloj.Start();

            this.Load += Frm_Principal_Load;
            this.FormClosing += Frm_Principal_FormClosing;
            this.FormClosed += Frm_Principal_FormClosed;

            ConfigurarEventosMenu();

            treeViewMenu.AfterSelect += TreeViewMenu_AfterSelect;

            btnNuevoProducto.Click += BtnNuevoProducto_Click;
            btnNuevaVenta.Click += BtnNuevaVenta_Click;
            btnNuevaCompra.Click += BtnNuevaCompra_Click;
            btnReporteDiario.Click += BtnReporteDiario_Click;

            dgvProductosCriticos.CellDoubleClick += DgvProductosCriticos_CellDoubleClick;
            dgvUltimosMovimientos.CellDoubleClick += DgvUltimosMovimientos_CellDoubleClick;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isClosing) return;
            _isClosing = true;

            foreach (var entry in _moduleRegistry)
            {
                if (entry.Value is IModuleForm module && module.HasChanges)
                {
                    var result = MessageBox.Show(
                        $"El módulo '{entry.Key}' tiene cambios sin guardar. ¿Cerrar de todas formas?",
                        "Cambios sin guardar",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No)
                    {
                        _isClosing = false;
                        e.Cancel = true;
                        return;
                    }
                }
            }

            base.OnFormClosing(e);
        }

        private void Frm_Principal_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_isClosing)
            {
                OnFormClosing(e);
            }
        }

        private void ConfigurarEventosMenu()
        {
            // Archivo
            mnuSalir.Click += MnuSalir_Click;
            mnuTenant.Click += MnuTenant_Click;
            mnuUsuarios.Click += MnuUsuarios_Click;
            mnuBackup.Click += MnuBackup_Click;

            // Maestros
            mnuProductos.Click += (s, e) => AbrirFormulario<frm_Producto>("Productos");
            mnuCategorias.Click += (s, e) => AbrirFormulario<frm_Categoria>("Categorías");
            mnuMarcas.Click += (s, e) => AbrirFormulario<frm_Marca>("Marcas");
            mnuUnidades.Click += (s, e) => AbrirFormulario<frm_Unidad>("Unidades de Medida");
            mnuProveedores.Click += (s, e) => AbrirFormulario<frm_Proveedor>("Proveedores");
            mnuClientes.Click += (s, e) => AbrirFormulario<frm_Cliente>("Clientes");
            mnuBodegas.Click += (s, e) => AbrirFormulario<frm_Bodega>("Bodegas");

            // Inventario
            //mnuStock.Click += (s, e) => AbrirFormulario<frm_Stock>("Gestión de Stock");
            //mnuAjustes.Click += (s, e) => AbrirFormulario<frm_StockAjuste>("Ajuste de Inventario");
            //mnuTransferencias.Click += (s, e) => NotificationManager.Info("Módulo de transferencias en desarrollo");

            // Compras
            mnuOrdenCompra.Click += (s, e) => AbrirFormulario<frm_OrdenCompra>("Ordenes de Compra");
            mnuRecepcion.Click += (s, e) => NotificationManager.Info("Módulo de recepción en desarrollo");

            // Ventas
            mnuPOS.Click += (s, e) => NotificationManager.Info("Módulo POS en desarrollo");
            mnuClientesVentas.Click += (s, e) => AbrirFormulario<frm_Cliente>("Clientes");

            // Reportes
            mnuReporteStock.Click += (s, e) => NotificationManager.Info("Reporte de stock en desarrollo");
            mnuReporteVentas.Click += (s, e) => NotificationManager.Info("Reporte de ventas en desarrollo");
            mnuReporteCompras.Click += (s, e) => NotificationManager.Info("Reporte de compras en desarrollo");
            mnuReporteUtilidad.Click += (s, e) => NotificationManager.Info("Reporte de utilidad en desarrollo");

            // Ayuda
            mnuAcercaDe.Click += MnuAcercaDe_Click;
        }

        private void Frm_Principal_Load(object sender, EventArgs e)
        {
            try
            {
                CargarDatosIniciales();
                CargarTreeViewMenu();
                CargarDashboard();
                ActualizarInfoUsuario();

                NotificationManager.Success("Bienvenido al Sistema de Inventario");
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al cargar el sistema: {ex.Message}");
            }
        }

        private void Frm_Principal_FormClosed(object sender, FormClosedEventArgs e)
        {
            timerReloj?.Stop();
            timerReloj?.Dispose();

            // Clean up all module scopes
            foreach (var entry in _moduleRegistry)
            {
                entry.Value.Dispose();
            }
            _moduleRegistry.Clear();
        }

        private void CargarDatosIniciales()
        {
            // TODO: Cargar datos del tenant y usuario desde sesión/autenticación
            // Ejemplo de datos por defecto
            IdTenant = 1;
            TenantNombre = "EMPRESA DEMO";
            IdUsuario = 1;
            UsuarioNombre = "ADMIN";
        }

        private void CargarTreeViewMenu()
        {
            treeViewMenu.Nodes.Clear();
            treeViewMenu.ImageList = new ImageList();
            treeViewMenu.ImageList.Images.Add("dashboard", Properties.Resources.dashboard);
            treeViewMenu.ImageList.Images.Add("productos", Properties.Resources.products);
            treeViewMenu.ImageList.Images.Add("compras", Properties.Resources.purchases_50);
            treeViewMenu.ImageList.Images.Add("ventas", Properties.Resources.acquisition);
            treeViewMenu.ImageList.Images.Add("inventario", Properties.Resources.inventory_management);
            treeViewMenu.ImageList.Images.Add("reportes", Properties.Resources.data_analytics);
            treeViewMenu.ImageList.Images.Add("config", Properties.Resources.configuration);

            // Nodo Dashboard
            TreeNode nodoDashboard = new TreeNode("Dashboard")
            {
                ImageKey = "dashboard",
                SelectedImageKey = "dashboard",
                Tag = "DASHBOARD"
            };
            treeViewMenu.Nodes.Add(nodoDashboard);

            // Nodo Maestros
            TreeNode nodoMaestros = new TreeNode("Maestros")
            {
                ImageKey = "productos",
                SelectedImageKey = "productos"
            };

            nodoMaestros.Nodes.Add(CrearNodo("Productos", "productos", "frm_Producto"));
            nodoMaestros.Nodes.Add(CrearNodo("Categorías", "productos", "frm_Categoria"));
            nodoMaestros.Nodes.Add(CrearNodo("Marcas", "productos", "frm_Marca"));
            nodoMaestros.Nodes.Add(CrearNodo("Unidades", "productos", "frm_Unidad"));
            nodoMaestros.Nodes.Add(CrearNodo("Proveedores", "productos", "frm_Proveedor"));
            nodoMaestros.Nodes.Add(CrearNodo("Clientes", "productos", "frm_Cliente"));
            nodoMaestros.Nodes.Add(CrearNodo("Bodegas", "productos", "frm_Bodega"));

            treeViewMenu.Nodes.Add(nodoMaestros);

            // Nodo Inventario
            TreeNode nodoInventario = new TreeNode("Inventario")
            {
                ImageKey = "inventario",
                SelectedImageKey = "inventario"
            };

            nodoInventario.Nodes.Add(CrearNodo("Stock", "inventario", "frm_Stock"));
            nodoInventario.Nodes.Add(CrearNodo("Ajustes", "inventario", "frm_StockAjuste"));
            nodoInventario.Nodes.Add(CrearNodo("Transferencias", "inventario", null));

            treeViewMenu.Nodes.Add(nodoInventario);

            // Nodo Compras
            TreeNode nodoCompras = new TreeNode("Compras")
            {
                ImageKey = "compras",
                SelectedImageKey = "compras"
            };

            nodoCompras.Nodes.Add(CrearNodo("Ordenes de Compra", "compras", "frm_OrdenCompra"));
            nodoCompras.Nodes.Add(CrearNodo("Recepción", "compras", null));

            treeViewMenu.Nodes.Add(nodoCompras);

            // Nodo Ventas
            TreeNode nodoVentas = new TreeNode("Ventas")
            {
                ImageKey = "ventas",
                SelectedImageKey = "ventas"
            };

            nodoVentas.Nodes.Add(CrearNodo("Punto de Venta (POS)", "ventas", null));
            nodoVentas.Nodes.Add(CrearNodo("Clientes", "ventas", "frm_Cliente"));

            treeViewMenu.Nodes.Add(nodoVentas);

            // Nodo Reportes
            TreeNode nodoReportes = new TreeNode("Reportes")
            {
                ImageKey = "reportes",
                SelectedImageKey = "reportes"
            };

            nodoReportes.Nodes.Add(CrearNodo("Stock", "reportes", null));
            nodoReportes.Nodes.Add(CrearNodo("Ventas", "reportes", null));
            nodoReportes.Nodes.Add(CrearNodo("Compras", "reportes", null));
            nodoReportes.Nodes.Add(CrearNodo("Utilidad", "reportes", null));

            treeViewMenu.Nodes.Add(nodoReportes);

            // Nodo Configuración
            TreeNode nodoConfig = new TreeNode("Configuración")
            {
                ImageKey = "config",
                SelectedImageKey = "config"
            };

            nodoConfig.Nodes.Add(CrearNodo("Tenant", "config", null));
            nodoConfig.Nodes.Add(CrearNodo("Usuarios", "config", null));
            nodoConfig.Nodes.Add(CrearNodo("Backup", "config", null));

            treeViewMenu.Nodes.Add(nodoConfig);

            // Expandir nodos principales
            treeViewMenu.ExpandAll();
        }

        private TreeNode CrearNodo(string texto, string imagenKey, string tag)
        {
            return new TreeNode(texto)
            {
                ImageKey = imagenKey,
                SelectedImageKey = imagenKey,
                Tag = tag
            };
        }

        private void CargarDashboard()
        {
            try
            {
                // TODO: Cargar datos reales desde repositorios
                // using (var unitOfWork = new UnitOfWork())
                // {
                //     // Totales
                //     lblTotalProductosValor.Text = unitOfWork.Repository<Prod>()
                //         .Count(p => p.IdTenant == IdTenant).ToString();
                //     
                //     lblClientesValor.Text = unitOfWork.Repository<Cliente>()
                //         .Count(c => c.IdTenant == IdTenant).ToString();
                //     
                //     lblProveedoresValor.Text = unitOfWork.Repository<Proveedor>()
                //         .Count(p => p.IdTenant == IdTenant).ToString();
                //     
                //     // Stock bajo
                //     var stockBajo = unitOfWork.Repository<Stock>()
                //         .Count(s => s.IdTenant == IdTenant && s.CantAct < 10);
                //     lblStockBajoValor.Text = stockBajo.ToString();
                //     
                //     // Ventas hoy
                //     var ventasHoy = unitOfWork.Repository<Venta>()
                //         .Where(v => v.IdTenant == IdTenant && v.FecVenta.Date == DateTime.Today)
                //         .Sum(v => (decimal?)v.Total) ?? 0;
                //     lblVentasHoyValor.Text = ventasHoy.ToString("C0");
                //     
                //     // Compras hoy
                //     var comprasHoy = unitOfWork.Repository<OrdenCompra>()
                //         .Where(o => o.IdTenant == IdTenant && o.FecOrden == DateTime.Today)
                //         .Sum(o => (decimal?)o.Total) ?? 0;
                //     lblComprasHoyValor.Text = comprasHoy.ToString("C0");
                //     
                //     // Alertas
                //     lstAlertas.Items.Clear();
                //     
                //     var productosBajoStock = unitOfWork.Repository<Stock>()
                //         .Where(s => s.IdTenant == IdTenant && s.CantAct < s.StockMinimo)
                //         .Take(5)
                //         .ToList();
                //     
                //     foreach (var stock in productosBajoStock)
                //     {
                //         lstAlertas.Items.Add($"⚠️ Stock bajo: {stock.Producto.Nombre} - {stock.CantAct} unidades");
                //     }
                //     
                //     // Productos críticos
                //     var criticos = unitOfWork.Repository<Stock>()
                //         .Where(s => s.IdTenant == IdTenant && s.CantAct < 5)
                //         .Select(s => new {
                //             Producto = s.Producto.Nombre,
                //             Stock = s.CantAct,
                //             Minimo = s.StockMinimo
                //         })
                //         .Take(10)
                //         .ToList();
                //     
                //     dgvProductosCriticos.DataSource = criticos;
                //     
                //     // Últimos movimientos
                //     var movimientos = unitOfWork.Repository<MovInv>()
                //         .Where(m => m.IdTenant == IdTenant)
                //         .OrderByDescending(m => m.FecMov)
                //         .Take(20)
                //         .Select(m => new {
                //             Fecha = m.FecMov,
                //             Tipo = m.TipoMov == 1 ? "Compra" : 
                //                    m.TipoMov == 2 ? "Venta" : 
                //                    m.TipoMov == 3 ? "Ajuste +" : "Ajuste -",
                //             Producto = m.Presentacion.Descripcion,
                //             Cantidad = m.CantCom,
                //             Usuario = m.Usuario
                //         })
                //         .ToList();
                //     
                //     dgvUltimosMovimientos.DataSource = movimientos;
                // }

                // Datos de ejemplo
                lblTotalProductosValor.Text = "1,234";
                lblClientesValor.Text = "567";
                lblProveedoresValor.Text = "89";
                lblStockBajoValor.Text = "23";
                lblVentasHoyValor.Text = "$ 1,250,000";
                lblComprasHoyValor.Text = "$ 850,000";

                lstAlertas.Items.Clear();
                lstAlertas.Items.Add("⚠️ 12 productos con stock bajo el mínimo");
                lstAlertas.Items.Add("⚠️ 3 productos próximos a vencer");
                lstAlertas.Items.Add("⚠️ 5 órdenes de compra pendientes");
                lstAlertas.Items.Add("⚠️ 2 transferencias por completar");

                // Productos críticos
                var criticos = new[]
                {
                    new { Producto = "NOTEBOOK HP 15", Stock = 3, Minimo = 10 },
                    new { Producto = "MOUSE LOGITECH", Stock = 2, Minimo = 15 },
                    new { Producto = "TECLADO MECÁNICO", Stock = 1, Minimo = 8 },
                    new { Producto = "MONITOR SAMSUNG 24", Stock = 4, Minimo = 6 },
                    new { Producto = "IMPRESORA HP LASER", Stock = 0, Minimo = 3 }
                };
                dgvProductosCriticos.DataSource = criticos.ToList();

                // Últimos movimientos
                var movimientos = new[]
                {
                    new { Fecha = DateTime.Now.AddMinutes(-30), Tipo = "Venta", Producto = "NOTEBOOK HP 15", Cantidad = 2, Usuario = "Admin" },
                    new { Fecha = DateTime.Now.AddHours(-2), Tipo = "Compra", Producto = "MOUSE LOGITECH", Cantidad = 50, Usuario = "Admin" },
                    new { Fecha = DateTime.Now.AddHours(-3), Tipo = "Ajuste +", Producto = "TECLADO MECÁNICO", Cantidad = 5, Usuario = "Admin" },
                    new { Fecha = DateTime.Now.AddHours(-5), Tipo = "Venta", Producto = "MONITOR SAMSUNG 24", Cantidad = 1, Usuario = "Admin" },
                    new { Fecha = DateTime.Now.AddDays(-1), Tipo = "Transferencia", Producto = "IMPRESORA HP LASER", Cantidad = 3, Usuario = "Admin" }
                };
                dgvUltimosMovimientos.DataSource = movimientos.ToList();

                // Configurar columnas de grids
                ConfigurarGrids();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al cargar dashboard: {ex.Message}");
            }
        }

        private void ConfigurarGrids()
        {
            // Configurar grid de productos críticos
            dgvProductosCriticos.AutoGenerateColumns = true;
            dgvProductosCriticos.Columns["Producto"].HeaderText = "Producto";
            dgvProductosCriticos.Columns["Producto"].Width = 200;
            dgvProductosCriticos.Columns["Stock"].HeaderText = "Stock Actual";
            dgvProductosCriticos.Columns["Stock"].Width = 80;
            dgvProductosCriticos.Columns["Stock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProductosCriticos.Columns["Minimo"].HeaderText = "Stock Mínimo";
            dgvProductosCriticos.Columns["Minimo"].Width = 80;
            dgvProductosCriticos.Columns["Minimo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // Colorear filas según nivel de stock
            dgvProductosCriticos.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == dgvProductosCriticos.Columns["Stock"].Index)
                {
                    var stock = Convert.ToDecimal(e.Value);
                    if (stock == 0)
                        e.CellStyle.BackColor = Color.FromArgb(255, 200, 200);
                    else if (stock < 3)
                        e.CellStyle.BackColor = Color.FromArgb(255, 230, 200);
                }
            };

            // Configurar grid de últimos movimientos
            dgvUltimosMovimientos.AutoGenerateColumns = true;
            dgvUltimosMovimientos.Columns["Fecha"].HeaderText = "Fecha";
            dgvUltimosMovimientos.Columns["Fecha"].Width = 130;
            dgvUltimosMovimientos.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dgvUltimosMovimientos.Columns["Tipo"].HeaderText = "Tipo";
            dgvUltimosMovimientos.Columns["Tipo"].Width = 80;
            dgvUltimosMovimientos.Columns["Producto"].HeaderText = "Producto";
            dgvUltimosMovimientos.Columns["Producto"].Width = 150;
            dgvUltimosMovimientos.Columns["Cantidad"].HeaderText = "Cantidad";
            dgvUltimosMovimientos.Columns["Cantidad"].Width = 70;
            dgvUltimosMovimientos.Columns["Cantidad"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvUltimosMovimientos.Columns["Usuario"].HeaderText = "Usuario";
            dgvUltimosMovimientos.Columns["Usuario"].Width = 80;
        }

        private void ActualizarInfoUsuario()
        {
            lblUsuario.Text = $"Usuario: {UsuarioNombre}";
            lblTenant.Text = $"Tenant: {TenantNombre}";
            tsslUsuario.Text = $"Usuario: {UsuarioNombre}";
            tsslTenant.Text = $"Tenant: {TenantNombre}";
            tsslVersion.Text = "Versión 2.0.0";
        }

        private void TimerReloj_Tick(object sender, EventArgs e)
        {
            lblFecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            tsslFecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void TreeViewMenu_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                string tag = e.Node.Tag.ToString();

                if (tag == "DASHBOARD")
                {
                    // Mostrar dashboard (ya está visible)
                    panelDashboard.Visible = true;
                }
                else
                {
                    // Abrir formulario correspondiente
                    AbrirFormularioPorTag(tag);
                }
            }
        }

        private void AbrirFormularioPorTag(string tag)
        {
            switch (tag)
            {
                case "frm_Producto":
                    AbrirFormulario<frm_Producto>("Productos");
                    break;
                case "frm_Categoria":
                    AbrirFormulario<frm_Categoria>("Categorías");
                    break;
                case "frm_Marca":
                    AbrirFormulario<frm_Marca>("Marcas");
                    break;
                case "frm_Unidad":
                    AbrirFormulario<frm_Unidad>("Unidades de Medida");
                    break;
                case "frm_Proveedor":
                    AbrirFormulario<frm_Proveedor>("Proveedores");
                    break;
                case "frm_Cliente":
                    AbrirFormulario<frm_Cliente>("Clientes");
                    break;
                case "frm_Bodega":
                    AbrirFormulario<frm_Bodega>("Bodegas");
                    break;
                //case "frm_Stock":
                //    AbrirFormulario<frm_Stock>("Gestión de Stock");
                //    break;
                //case "frm_StockAjuste":
                //    AbrirFormulario<frm_StockAjuste>("Ajuste de Inventario");
                //    break;
                //case "frm_OrdenCompra":
                //    AbrirFormulario<frm_OrdenCompra>("Ordenes de Compra");
                //    break;
                //    break;
                default:
                    NotificationManager.Info($"Módulo {tag} en desarrollo");
                    break;
            }
        }

        private void AbrirFormulario<T>(string titulo) where T : Form
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var form = _formFactory.Create<T>();

                if (form is IModuleForm module)
                {
                    // MDI child: single-instance, non-modal
                    if (TryActivateExistingModule(module.ModuleKey))
                        return;

                    module.Initialize(IdTenant);
                    form.Text = titulo;
                    form.MdiParent = this;

                    form.Load += (_, _) => _moduleRegistry[module.ModuleKey] = form;
                    form.FormClosed += (_, _) => _moduleRegistry.Remove(module.ModuleKey);

                    form.Show();
                }
                else
                {
                    // Legacy form: modal dialog, existing pattern
                    if (form is frm_Categoria categoriaForm)
                        categoriaForm.IdTenant = IdTenant;
                    else if (form is frm_Marca marcaForm)
                        marcaForm.IdTenant = IdTenant;
                    else if (form is frm_Proveedor proveedorForm)
                        proveedorForm.IdTenant = IdTenant;
                    else if (form is frm_Cliente clienteForm)
                        clienteForm.IdTenant = IdTenant;

                    form.Text = titulo;
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al abrir {titulo}: {ex.Message}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private bool TryActivateExistingModule(string moduleKey)
        {
            if (_moduleRegistry.TryGetValue(moduleKey, out var existing))
            {
                if (!existing.IsDisposed)
                {
                    existing.Activate();
                    return true;
                }
                _moduleRegistry.Remove(moduleKey);
            }
            return false;
        }

        #region Eventos de acciones rápidas

        private void BtnNuevoProducto_Click(object sender, EventArgs e)
        {
            AbrirFormulario<frm_Producto>("Nuevo Producto");
            CargarDashboard(); // Refrescar datos
        }

        private void BtnNuevaVenta_Click(object sender, EventArgs e)
        {
            NotificationManager.Info("Módulo de ventas en desarrollo");
        }

        private void BtnNuevaCompra_Click(object sender, EventArgs e)
        {
            AbrirFormulario<frm_OrdenCompra>("Nueva Orden de Compra");
        }

        //private void BtnAjusteStock_Click(object sender, EventArgs e)
        //{
        //    // Abrir directamente un ajuste rápido
        //    var result = _formFactory.ShowDialog<frm_StockAjuste>(form =>
        //    {
        //        //form.IdTenant = IdTenant;
        //        //form.IdStock = 0; // Nuevo ajuste
        //    });

        //    if (result == DialogResult.OK)
        //    {
        //        CargarDashboard();
        //    }
        //}

        private void BtnReporteDiario_Click(object sender, EventArgs e)
        {
            NotificationManager.Info("Generando reporte diario...");
            // TODO: Implementar generación de reporte
        }

        #endregion

        #region Eventos de doble clic en grids

        private void DgvProductosCriticos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Obtener producto seleccionado y abrir para edición
                var producto = dgvProductosCriticos.Rows[e.RowIndex].Cells["Producto"].Value.ToString();

                // TODO: Buscar ID del producto por nombre y abrir formulario
                NotificationManager.Info($"Editando producto: {producto}");

                // Ejemplo:
                // AbrirFormulario<frm_Producto>(form =>
                // {
                //     form.BuscarProducto(producto);
                // });
            }
        }

        private void DgvUltimosMovimientos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Ver detalle del movimiento
                NotificationManager.Info("Ver detalle del movimiento");
            }
        }

        #endregion

        #region Eventos de menú

        private void MnuSalir_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void MnuTenant_Click(object sender, EventArgs e)
        {
            NotificationManager.Info("Configuración de Tenant en desarrollo");
        }

        private void MnuUsuarios_Click(object sender, EventArgs e)
        {
            NotificationManager.Info("Gestión de usuarios en desarrollo");
        }

        private void MnuBackup_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea realizar una copia de seguridad ahora?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                NotificationManager.Success("Copia de seguridad iniciada");
                // TODO: Implementar backup
            }
        }

        private void MnuAcercaDe_Click(object sender, EventArgs e)
        {
            string acerca = @"
╔══════════════════════════════════════╗
║    SISTEMA DE INVENTARIO v2.0       ║
║    Multi-Tenant Enterprise           ║
╠══════════════════════════════════════╣
║  © 2026 - Todos los derechos         ║
║  reservados                          ║
║                                      ║
║  Desarrollado para:                   ║
║  Gestión de Inventarios Profesional  ║
╚══════════════════════════════════════╝";

            MessageBox.Show(acerca, "Acerca de...",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Métodos públicos para acceso externo

        public void EstablecerTenant(int idTenant, string nombreTenant)
        {
            IdTenant = idTenant;
            TenantNombre = nombreTenant;
            ActualizarInfoUsuario();
        }

        public void EstablecerUsuario(int idUsuario, string nombreUsuario)
        {
            IdUsuario = idUsuario;
            UsuarioNombre = nombreUsuario;
            ActualizarInfoUsuario();
        }

        public void RefrescarDashboard()
        {
            CargarDashboard();
        }

        #endregion

    }
}