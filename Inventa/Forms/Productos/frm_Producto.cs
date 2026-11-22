using CBP.Data.Abstractions;
using CBP.MultiTenant.Abstractions;
using CBP.MultiTenant.Extensions;
using CBP.WinFrms.Notification;
using CBP.WinFrms.Theming;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.Forms.Mantenedores;
using Inventa.Infrastructure;
using Inventa.UC.Producto;
using Inventa.UC.Producto.DTOs;
using Inventa.UC.Producto.Interfaces;
using Inventa.UI;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows.Forms;

namespace Inventa.Forms.Productos
{
    public partial class frm_Producto : ModuleForm, IProductoContainer
    {
        public override string ModuleKey => "Productos";

        // ─────────────────────────────────────────────────────────────────────────
        // Dependencias inyectadas
        // ─────────────────────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IServiceScopeFactory _scopeFactory;

        // ─────────────────────────────────────────────────────────────────────────
        // Gestores
        // ─────────────────────────────────────────────────────────────────────────
        private TabLoadingManager _tabLoadingManager = new();
        private PerformanceMetrics _perfMetrics => PerformanceMetrics.Instance;

        // ─────────────────────────────────────────────────────────────────────────
        // Unit of Work y ámbito
        // ─────────────────────────────────────────────────────────────────────────
        private IUnitOfWorkSync? _unitOfWork;

        // ─────────────────────────────────────────────────────────────────────────
        // Repositorios
        // ─────────────────────────────────────────────────────────────────────────
        private IProdRepository? _prodRepo;
        private ICategoriaRepository? _categoriaRepo;
        private IMarcaRepository? _marcaRepo;
        private IPresProdRepository? _presProdRepo;
        private IVarProdRepository? _varProdRepo;

        // ─────────────────────────────────────────────────────────────────────────
        // UserControls
        // ─────────────────────────────────────────────────────────────────────────
        private ProdGeneralUC? ucGeneral;
        private ProdVarUC? ucVariantes;
        private ProdPresUC? ucPresentaciones;
        private ProdConvUC? ucConversiones;
        private ProdCodBarUC? ucCodigos;
        private ProdPreciosVentaUC? ucPreciosVenta;
        private ProdStockUC? ucStock;

        // ─────────────────────────────────────────────────────────────────────────
        // Estado de selección
        // ─────────────────────────────────────────────────────────────────────────
        private long? _varianteSeleccionada;

        // ─────────────────────────────────────────────────────────────────────────
        // Producto actual
        // ─────────────────────────────────────────────────────────────────────────
        private Prod? _productoActual;
        public Prod? ProductoActual => _productoActual;
        public ITenantContext TenantContext => _tenantContext;
        public IUnitOfWorkSync UnitOfWork => _unitOfWork!;

        private Panel? _detalleContentPanel;
        private Button? _activeNavButton;
        private Button? _firstNavButton;
        private Button? _btnToggleExplorador;
        private bool _exploradorVisible = true;
        private static readonly int ANCHO_EXPLORADOR = 280;
        private static readonly Color _navActiveColor = Color.FromArgb(200, 220, 240);
        private static readonly Color _navInactiveColor = Color.Transparent;

        public void NotificarCambioTamaño(ZonaContenedor zona, int alturaActual, int alturaAnterior)
        {
            if (DesignModeHelper.IsDesignTime() || IsDisposed) return;

            var delta = alturaActual - alturaAnterior;
            if (delta == 0) return;

            switch (zona)
            {
                case ZonaContenedor.General:
                    var nuevaGen = splitWorkspace.SplitterDistance + delta;
                    nuevaGen = Math.Max(splitWorkspace.Panel1MinSize,
                             Math.Min(nuevaGen, splitWorkspace.Height - splitWorkspace.Panel2MinSize));
                    splitWorkspace.SplitterDistance = (int)nuevaGen;
                    break;

                case ZonaContenedor.Presentacion:
                    var nuevaPres = splitDetail.SplitterDistance + delta;
                    nuevaPres = Math.Max(splitDetail.Panel1MinSize,
                              Math.Min(nuevaPres, splitDetail.Height - splitDetail.Panel2MinSize));
                    splitDetail.SplitterDistance = (int)nuevaPres;
                    break;
            }
        }

        public event EventHandler<ProductoChangedEventArgs>? ProductoChanged;

        // ─────────────────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────────────────
        public frm_Producto(
            ITenantContext tenantContext,
            IServiceScopeFactory scopeFactory,
            IFormFactory formFactory)
            : base(tenantContext, formFactory)
        {
            InitializeComponent();

            // ── Botón toggle explorador ─────────────────────────────────────
            _btnToggleExplorador = new Button
            {
                Text = "◀",
                Dock = DockStyle.Left,
                Width = 28,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _btnToggleExplorador.Click += BtnToggleExplorador_Click;
            panelHeader.Controls.Add(_btnToggleExplorador);
            _btnToggleExplorador.BringToFront();

            _tenantContext = tenantContext;
            _scopeFactory = scopeFactory;

            ucVariantes = new ProdVarUC();
            ucConversiones = new ProdConvUC();
            ucCodigos = new ProdCodBarUC();
            ucPreciosVenta = new ProdPreciosVentaUC();
            ucStock = new ProdStockUC();

            // Asignar el contenedor (este formulario) a cada UC
            ucVariantes.SetContainer(this);
            ucConversiones.SetContainer(this);
            ucCodigos.SetContainer(this);
            ucPreciosVenta.SetContainer(this);
            ucStock.SetContainer(this);

            // Añadir los UC a los paneles contenedores
            pnlVariantesHost.Controls.Add(ucVariantes);
            pnlConversionesHost.Controls.Add(ucConversiones);
            pnlCodigosHost.Controls.Add(ucCodigos);
            pnlPreciosVentaHost.Controls.Add(ucPreciosVenta);
            pnlStockHost.Controls.Add(ucStock);

            // ── Fase 2: Capturar alturas de diseño antes de Dock=Fill ─────────
            var ucHeights = new Dictionary<UserControl, int>
            {
                [ucConversiones] = ucConversiones.Height,
                [ucCodigos] = ucCodigos.Height,
                [ucPreciosVenta] = ucPreciosVenta.Height,
                [ucStock] = ucStock.Height,
            };

            // Configurar docking (solo UCs que NO están en host con AutoScroll)
            foreach (var uc in new UserControl[]
            {
                ucConversiones, ucCodigos, ucPreciosVenta, ucStock
            })
            {
                uc.Dock = DockStyle.Fill;
            }

            // Suscribirse a eventos de los UC
            ucVariantes.VarianteSeleccionada += OnVarianteSeleccionada;

            _btnLight.Click += BtnLight_Click;
            _btnDark.Click += BtnDark_Click;
            _btnSystem.Click += BtnSystem_Click;
            _btnNotificacion.Click += _btnNotificacion_Click;

            // Inicializar el gestor de estado de secciones
            string[] sectionKeys = { "tabConversiones", "tabCodigos", "tabPreciosVenta", "tabStock" };
            foreach (var key in sectionKeys)
            {
                _tabLoadingManager.SetState(key, LazyLoadState.NotLoaded);
            }

            // ── Progressive enablement inicial ──────────────────────────────────
            splitCol2Col3.Enabled = false;
            splitWorkspace.Enabled = false;
            splitDetail.Enabled = false;

            // ── Fase 2: Reemplazar TabControl por panel con scroll ─────────────
            _detalleContentPanel = CreateDetalleContentPanel(ucHeights);
            splitDetail.Panel2.Controls.Remove(tabDetail);
            splitDetail.Panel2.Controls.Add(_detalleContentPanel);

            // Configurar eventos del formulario
            this.KeyPreview = true;
            this.KeyDown += Frm_Producto_KeyDown;
            dgvProductosLista.CellClick += DgvProductosLista_CellClick;
            dgvProductosLista.SelectionChanged += DgvProductosLista_SelectionChanged;
            btnRefrescar.Click += BtnRefrescar_Click;
            btnLimpiarFiltros.Click += BtnLimpiarFiltros_Click;
            txtBuscarProducto.TextChanged += TxtBuscarProducto_TextChanged;
            cboFiltroCategoria.SelectedIndexChanged += Filtro_Changed;
            cboFiltroMarca.SelectedIndexChanged += Filtro_Changed;
            cboFiltroEstado.SelectedIndexChanged += Filtro_Changed;
            cboFiltroStock.SelectedIndexChanged += Filtro_Changed;
            chkSoloConVariantes.CheckedChanged += Filtro_Changed;

            // Configurar columnas del grid explorer
            ConfigureExplorerGrid();

            // Configurar el timer de búsqueda (si se desea debounce)
            _busquedaTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _busquedaTimer.Tick += OnBusquedaTimerTick;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Implementación de IProductoContainer
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Helper para obtener UCs con tracking de performance (opcional).
        /// Los UCs ya están creados en el constructor, este método es para reutilización.
        /// </summary>
        private T GetOrCreateUC<T>(ref T? field, Func<T> factory) where T : UserControl
        {
            if (field != null)
                return field;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            field = factory();
            sw.Stop();

            _perfMetrics.RecordUcLoad(typeof(T).Name, sw.ElapsedMilliseconds, fromCache: false);
            return field;
        }

        public void MarcarComoModificado()
        {
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Botón toggle explorador (◀/▶)
        // ─────────────────────────────────────────────────────────────────────────
        private void BtnToggleExplorador_Click(object? sender, EventArgs e)
        {
            _exploradorVisible = !_exploradorVisible;
            if (_exploradorVisible)
            {
                splitExplorer.Panel1Collapsed = false;
                splitExplorer.SplitterDistance = ANCHO_EXPLORADOR;
                _btnToggleExplorador.Text = "◀";
            }
            else
            {
                splitExplorer.Panel1Collapsed = true;
                _btnToggleExplorador.Text = "▶";
            }
        }

        public void SolicitarRecarga(TipoDatos tipo)
        {
            switch (tipo)
            {
                case TipoDatos.General:
                    ucGeneral?.RefreshData();
                    break;
                case TipoDatos.Variantes:
                    ucVariantes?.RefreshData();
                    break;
                case TipoDatos.Presentaciones:
                    ucPresentaciones?.RefreshData();
                    break;
                case TipoDatos.Conversiones:
                    ucConversiones?.RefreshData();
                    break;
                case TipoDatos.Codigos:
                    ucCodigos?.RefreshData();
                    break;
                case TipoDatos.PreciosVenta:
                    ucPreciosVenta?.RefreshData();
                    break;
                case TipoDatos.Stock:
                    ucStock?.RefreshData();
                    break;
                case TipoDatos.Todo:
                    ucGeneral?.RefreshData();
                    ucVariantes?.RefreshData();
                    ucPresentaciones?.RefreshData();
                    ucConversiones?.RefreshData();
                    ucCodigos?.RefreshData();
                    ucPreciosVenta?.RefreshData();
                    ucStock?.RefreshData();
                    break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Carga inicial
        // ─────────────────────────────────────────────────────────────────────────

        private void SetTheme(ThemeMode mode)
        {
            ThemeManager.Instance.SetThemeMode(mode);
        }

        private void BtnLight_Click(object? sender, EventArgs e) => SetTheme(ThemeMode.Light);
        private void BtnDark_Click(object? sender, EventArgs e) => SetTheme(ThemeMode.Dark);
        private void BtnSystem_Click(object? sender, EventArgs e) => SetTheme(ThemeMode.System);

        private void OnThemeChanged(object? sender, ThemeModeChangedEventArgs e)
        {
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Configuración del grid explorer (catalog view)
        // ─────────────────────────────────────────────────────────────────────────
        private void ConfigureExplorerGrid()
        {
            dgvProductosLista.AutoGenerateColumns = false;
            dgvProductosLista.Columns.Clear();

            var colNombre = new DataGridViewTextBoxColumn
            {
                Name = "colNombre",
                DataPropertyName = nameof(ProductoListaItem.Nombre),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 10F),
                    Padding = new Padding(8, 4, 8, 4)
                }
            };
            dgvProductosLista.Columns.Add(colNombre);
        }

        private void OnBusquedaTimerTick(object? sender, EventArgs e)
        {
            _busquedaTimer.Stop();
            CargarListaProductos(IdTenant);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignModeHelper.IsDesignTime())
                return;

            ThemeManager.Instance.ThemeChanged += OnThemeChanged;
            InitializeModule();
        }

        private void InitializeModule()
        {
            if (_isLoading) return;
            _isLoading = true;
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 OnLoad: Iniciando carga del formulario");

                dgvProductosLista.Enabled = false;

                CreateScope(_scopeFactory);
                _unitOfWork = Services.GetRequiredService<IUnitOfWorkSync>();

                ucGeneral?.InitializeData();
                ucVariantes?.InitializeData();
                ucPresentaciones?.InitializeData();
                ucConversiones?.InitializeData();
                ucCodigos?.InitializeData();
                ucPreciosVenta?.InitializeData();
                ucStock?.InitializeData();

                CargarCombosFiltros(IdTenant);
                CargarListaProductos(IdTenant);
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al cargar el formulario: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                dgvProductosLista.Enabled = true;
            }
        }

        private void CargarCombosFiltros(int idTenant)
        {
            try
            {
                // Cargar categorías
                _categoriaRepo ??= _unitOfWork!.GetCustomRepository<ICategoriaRepository>();
                var categoriasResult = _categoriaRepo.GetByTenant(idTenant, soloActivas: true);
                if (categoriasResult.IsSuccess)
                {
                    var categorias = categoriasResult.Value.ToList();
                    var catFiltro = new List<Categoria>
                    {
                        new Categoria { IdCategoria = 0, Nombre = "TODAS LAS CATEGORÍAS" }
                    };
                    catFiltro.AddRange(categorias);

                    cboFiltroCategoria.DataSource = catFiltro;
                    cboFiltroCategoria.DisplayMember = nameof(Categoria.Nombre);
                    cboFiltroCategoria.ValueMember = nameof(Categoria.IdCategoria);
                    cboFiltroCategoria.SelectedIndex = 0;
                }

                // Cargar marcas
                _marcaRepo ??= _unitOfWork!.GetCustomRepository<IMarcaRepository>();
                var marcasResult = _marcaRepo.GetByTenant(idTenant, soloActivas: true);
                if (marcasResult.IsSuccess)
                {
                    var marcas = marcasResult.Value.ToList();
                    var marcasFiltro = new List<Marca>
                    {
                        new Marca { IdMarca = 0, Nombre = "TODAS LAS MARCAS" }
                    };
                    marcasFiltro.AddRange(marcas);

                    cboFiltroMarca.DataSource = marcasFiltro;
                    cboFiltroMarca.DisplayMember = nameof(Marca.Nombre);
                    cboFiltroMarca.ValueMember = nameof(Marca.IdMarca);
                    cboFiltroMarca.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al cargar filtros: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Carga de lista de productos con filtros
        // ─────────────────────────────────────────────────────────────────────────
        private void CargarListaProductos(int idTenant)
        {
            try
            {
                _prodRepo ??= _unitOfWork!.GetCustomRepository<IProdRepository>();

                var query = _prodRepo.GetByTenant(idTenant, soloActivos: false);
                if (query.IsFailure) return;

                var productos = query.Value.AsEnumerable();

                // Filtro por estado
                if (cboFiltroEstado.SelectedIndex == 1)          // Solo activos
                    productos = productos.Where(p => p.Activo);
                else if (cboFiltroEstado.SelectedIndex == 2)     // Solo inactivos
                    productos = productos.Where(p => !p.Activo);

                // Filtro por categoría
                if (cboFiltroCategoria.SelectedValue is long catId && catId > 0)
                    productos = productos.Where(p => p.IdCategoria == catId);

                // Filtro por marca
                if (cboFiltroMarca.SelectedValue is int marcaId && marcaId > 0)
                    productos = productos.Where(p => p.IdMarca == marcaId);

                // Búsqueda por texto
                var textoBusqueda = txtBuscarProducto.Text.Trim();
                if (!string.IsNullOrWhiteSpace(textoBusqueda))
                {
                    productos = productos.Where(p =>
                        p.Codigo.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                        p.Nombre.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase));
                }

                // Filtro por existencia de variantes
                if (chkSoloConVariantes.Checked)
                    productos = productos.Where(p => p.Variantes?.Any() == true);

                // Filtro por stock (implementación simplificada; se puede mejorar con una consulta que evalúe stock)
                // Por ahora se omite porque requiere datos de stock que no están disponibles en la lista básica.
                // En una versión completa se debería hacer una consulta con join a Stock para determinar si tiene stock.

                var lista = productos.Select(p => new ProductoListaItem(
                    p.IdProd,
                    p.Codigo,
                    p.Nombre,
                    p.Categoria?.Nombre ?? string.Empty,
                    p.Marca?.Nombre ?? string.Empty,
                    p.Activo,
                    0,  // StockTotal se calculará después si es necesario
                    p.Variantes?.Count ?? 0
                )).ToList();

                _productosLista = new BindingList<ProductoListaItem>(lista);
                dgvProductosLista.ApplyModernStyle();
                dgvProductosLista.DataSource = _productosLista;

                lblContadorProductos.Text = $"Mostrando {lista.Count} productos";
                ActualizarIndicadorFiltros();

                // Disparar manualmente la selección del primer producto
                // WinForms NO dispara SelectionChanged durante la inicialización de DataSource
                // Así que lo hacemos manualmente para cargar el primer producto
                if (dgvProductosLista.Rows.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"➡️ Seleccionando primer producto de la lista");
                    dgvProductosLista.ClearSelection();
                    dgvProductosLista.Rows[0].Selected = true;
                    // Esto dispará el evento SelectionChanged
                }
                else
                {
                    // Si no hay productos, limpiar la selección
                    _productoActual = null;
                    ucGeneral?.Limpiar();
                    tslProductoActual.Text = "Sin producto seleccionado";
                }
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al cargar productos: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Selección de producto
        // ─────────────────────────────────────────────────────────────────────────
        private void DgvProductosLista_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (_isLoading) return;

            var item = dgvProductosLista.Rows[e.RowIndex].DataBoundItem as ProductoListaItem;
            if (item == null) return;

            SeleccionarProducto(item);
        }

        private void DgvProductosLista_SelectionChanged(object? sender, EventArgs e)
        {
            if (_isLoading) return;
            if (dgvProductosLista.SelectedRows.Count == 0) return;

            var item = dgvProductosLista.SelectedRows[0].DataBoundItem as ProductoListaItem;
            if (item == null) return;

            SeleccionarProducto(item);
        }

        private void SeleccionarProducto(ProductoListaItem item)
        {
            try
            {
                if (_productoActual != null && _productoActual.IdProd == item.IdProd)
                    return;

                RefreshDirtyState();
                if (!CanClose())
                {
                    if (MessageBox.Show("Hay cambios sin guardar. ¿Desea guardarlos antes de cambiar de producto?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                }

                _prodRepo ??= _unitOfWork!.GetCustomRepository<IProdRepository>();
                var result = _prodRepo.GetWithVariantes(item.IdProd, IdTenant, ProdRepository.VarianteLoadOption.None);
                if (result.IsFailure || result.Value == null)
                {
                    NotificationManager.Error($"No se pudo cargar el producto: {result.Error?.Message}");
                    return;
                }

                var productoAnterior = _productoActual;
                _productoActual = result.Value;

                MostrarProducto(_productoActual);
                ProductoChanged?.Invoke(this, new ProductoChangedEventArgs(productoAnterior, _productoActual, false));

                _tabLoadingManager.MarkAllForReload();

                tslProductoActual.Text = $"{_productoActual.Codigo} - {_productoActual.Nombre}";
                tslCambiosPendientes.Text = string.Empty;

                splitCol2Col3.Enabled = true;
                splitWorkspace.Enabled = true;
                splitDetail.Enabled = false;

                OnProductoSeleccionado();
                LoadVariantesAsync();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al seleccionar producto: {ex.Message}");
            }
        }

        private void MostrarProducto(Prod producto)
        {
            pnlGeneralHost.SuspendLayout();
            try
            {
                pnlGeneralHost.Controls.Clear();

                if (ucGeneral == null)
                {
                    ucGeneral = new ProdGeneralUC();
                    ucGeneral.SetContainer(this);
                }

                ucGeneral.LoadProducto(producto);
                pnlGeneralHost.Controls.Add(ucGeneral);
                lblEmptyProducto.Visible = false;
            }
            finally
            {
                pnlGeneralHost.ResumeLayout();
            }
        }

        private void LoadVariantesAsync()
        {
            if (_productoActual == null) return;

            try
            {
                ucVariantes?.RefreshData();
                var hasVariantes = _productoActual.Variantes?.Any(v => v.Activo) == true;
                lblEmptyVariante.Visible = !hasVariantes;
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al cargar variantes: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Lazy loading por secciones de detalle (Fase 2)
        // ─────────────────────────────────────────────────────────────────────────
        private void LoadSection(string key)
        {
            if (_productoActual == null) return;
            if (_tabLoadingManager.IsLoaded(key)) return;

            switch (key)
            {
                case "tabConversiones":
                    if (!_varianteSeleccionada.HasValue) return;
                    ucConversiones?.SetVarianteSeleccionada(_varianteSeleccionada.Value);
                    ucConversiones?.RefreshData();
                    break;

                case "tabCodigos":
                    ucCodigos?.RefreshData();
                    break;

                case "tabPreciosVenta":
                    ucPreciosVenta?.RefreshData();
                    break;

                case "tabStock":
                    ucStock?.SetVarianteSeleccionada(_varianteSeleccionada ?? 0);
                    ucStock?.RefreshData();
                    break;
            }
            _tabLoadingManager.SetState(key, LazyLoadState.Loaded);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Fase 2: Panel de detalle con scroll + navegación
        // ─────────────────────────────────────────────────────────────────────────
        private Panel CreateDetalleContentPanel(Dictionary<UserControl, int> ucHeights)
        {
            var pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Name = "pnlDetalleContent"
            };

            // ── Barra de navegación ───────────────────────────────────────────
            var pnlNav = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                BackColor = Color.FromArgb(240, 242, 245)
            };
            var flpNav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(6, 4, 6, 4)
            };

            // ── Separador ─────────────────────────────────────────────────────
            var sep = new Panel
            {
                Dock = DockStyle.Top,
                Height = 2,
                BackColor = Color.FromArgb(210, 210, 210)
            };

            // ── Secciones ─────────────────────────────────────────────────────
            var sections = new (string key, string text, Panel host, UserControl uc)[]
            {
                ("tabConversiones", "Conversiones", pnlConversionesHost, ucConversiones!),
                ("tabCodigos", "Códigos", pnlCodigosHost, ucCodigos!),
                ("tabPreciosVenta", "Precios Venta", pnlPreciosVentaHost, ucPreciosVenta!),
                ("tabStock", "Stock", pnlStockHost, ucStock!),
            };

            var hostPanels = new List<Panel>();

            foreach (var (key, text, host, uc) in sections)
            {
                var height = ucHeights.TryGetValue(uc, out var h) ? h : 500;
                host.Dock = DockStyle.Top;
                host.Height = Math.Max(height, 350);
                host.Visible = false;
                hostPanels.Add(host);

                var btn = new Button
                {
                    Text = text,
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 },
                    Height = 26,
                    AutoSize = true,
                    Padding = new Padding(10, 0, 10, 0),
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    Cursor = Cursors.Hand,
                    BackColor = _navInactiveColor,
                    Tag = key
                };

                btn.Click += (s, e) =>
                {
                    if (_activeNavButton != null && _activeNavButton != btn)
                        _activeNavButton.BackColor = _navInactiveColor;
                    btn.BackColor = _navActiveColor;
                    _activeNavButton = btn;

                    foreach (var h in hostPanels)
                        h.Visible = false;
                    host.Visible = true;

                    LoadSection(key);
                };

                flpNav.Controls.Add(btn);
                pnlContent.Controls.Add(host);

                _firstNavButton ??= btn;
            }

            pnlNav.Controls.Add(flpNav);

            // Z-order: nav on top, separator below, sections below that
            pnlContent.Controls.Add(pnlNav);
            pnlContent.Controls.Add(sep);

            return pnlContent;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Orquestación Explorer + Workspace
        // ─────────────────────────────────────────────────────────────────────────

        private void OnProductoSeleccionado()
        {
            UpdateBreadcrumb();
            OnVarianteSeleccionada(null, 0);
        }

        private void UpdateBreadcrumb()
        {
            if (_productoActual is null)
            {
                lblBreadcrumb.Text = "Productos";
                return;
            }
            lblBreadcrumb.Text = $"Productos  ›  {_productoActual.Codigo} — {_productoActual.Nombre}";
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Evento de selección de variante (desde ProdVariantesUC)
        // ─────────────────────────────────────────────────────────────────────────
        private void OnVarianteSeleccionada(object? sender, long idVarProd)
        {
            _varianteSeleccionada = idVarProd;

            if (idVarProd > 0)
            {
                splitDetail.Enabled = true;
                splitCol2Col3.Enabled = true;
                MostrarPresentacion(idVarProd);
                _firstNavButton?.PerformClick();

                _varProdRepo ??= _unitOfWork?.GetCustomRepository<IVarProdRepository>();
                var varResult = _varProdRepo?.GetById(idVarProd);
                var varNombre = varResult is { IsSuccess: true, Value: not null }
                    ? $"{varResult.Value.Codigo} — {varResult.Value.Nombre}"
                    : $"Variante #{idVarProd}";
                lblBreadcrumb.Text = $"Productos  ›  {_productoActual?.Codigo} — {_productoActual?.Nombre}  ›  {varNombre}";
            }
            else
            {
                splitDetail.Enabled = false;
                UpdateBreadcrumb();
            }
        }

        private void MostrarPresentacion(long idVarProd)
        {
            pnlPresentacionHost.SuspendLayout();
            try
            {
                pnlPresentacionHost.Controls.Clear();

                if (ucPresentaciones == null)
                {
                    ucPresentaciones = new ProdPresUC();
                    ucPresentaciones.SetContainer(this);
                    ucPresentaciones.PresentacionSeleccionada += OnPresentacionSeleccionada;
                }

                ucPresentaciones.SetVarianteSeleccionada(idVarProd);
                ucPresentaciones.RefreshData();
                pnlPresentacionHost.Controls.Add(ucPresentaciones);
                lblEmptyPresentacion.Visible = false;
            }
            finally
            {
                pnlPresentacionHost.ResumeLayout();
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Evento de selección de presentación (desde ProdPresUC)
        // ─────────────────────────────────────────────────────────────────────────
        private void OnPresentacionSeleccionada(object? sender, long idPresProd)
        {
            if (_unitOfWork == null)
                return;

            _presProdRepo ??= _unitOfWork.GetCustomRepository<IPresProdRepository>();
            var result = _presProdRepo.GetById(idPresProd);
            var presDescrip = result is { IsSuccess: true, Value: not null } ? result.Value.Descrip : $"#{idPresProd}";

            // Actualizar el UC de códigos de barras
            if (ucCodigos != null && result is { IsSuccess: true, Value: not null })
            {
                ucCodigos.SetPresentacion(result.Value);
            }

            // Actualizar los UCs de precios con la presentación seleccionada
            ucPreciosVenta?.SetPresentacionSeleccionada(idPresProd);

            // Actualizar breadcrumb con presentación
            _varProdRepo ??= _unitOfWork.GetCustomRepository<IVarProdRepository>();
            var varResult = _varProdRepo?.GetById(_varianteSeleccionada ?? 0);
            var varNombre = varResult is { IsSuccess: true, Value: not null }
                ? $"{varResult.Value.Codigo} — {varResult.Value.Nombre}"
                : $"Variante #{_varianteSeleccionada}";
            if (_productoActual != null)
                lblBreadcrumb.Text = $"Productos  ›  {_productoActual.Codigo} — {_productoActual.Nombre}  ›  {varNombre}  ›  {presDescrip}";
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Botones de filtro y acción
        // ─────────────────────────────────────────────────────────────────────────
        private System.Windows.Forms.Timer _busquedaTimer;
        private bool _isLoading;
        private BindingList<ProductoListaItem> _productosLista = new();

        private void TxtBuscarProducto_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            _busquedaTimer.Stop();
            _busquedaTimer.Start();
        }

        private void Filtro_Changed(object sender, EventArgs e)
        {
            if (_isLoading) return;
            CargarListaProductos(IdTenant);
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            CargarListaProductos(IdTenant);
        }

        public void RefreshProductList()
        {
            CargarListaProductos(IdTenant);
        }

        private void BtnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            txtBuscarProducto.Clear();
            cboFiltroCategoria.SelectedIndex = 0;
            cboFiltroMarca.SelectedIndex = 0;
            cboFiltroEstado.SelectedIndex = 1; // Solo activos (default)
            cboFiltroStock.SelectedIndex = 0;
            chkSoloConVariantes.Checked = false;
            ActualizarIndicadorFiltros();
            BtnRefrescar_Click(sender, e);
        }

        private void ActualizarIndicadorFiltros()
        {
            var filtros = new List<string>();
            if (cboFiltroCategoria.SelectedIndex > 0)
            {
                cboFiltroCategoria.BackColor = Color.FromArgb(255, 249, 230);
                filtros.Add($"Categoría: {cboFiltroCategoria.Text}");
            }
            else
                cboFiltroCategoria.BackColor = Color.White;

            if (cboFiltroMarca.SelectedIndex > 0)
            {
                cboFiltroMarca.BackColor = Color.FromArgb(255, 249, 230);
                filtros.Add($"Marca: {cboFiltroMarca.Text}");
            }
            else
                cboFiltroMarca.BackColor = Color.White;

            if (cboFiltroEstado.SelectedIndex != 1)
            {
                cboFiltroEstado.BackColor = Color.FromArgb(255, 249, 230);
                filtros.Add($"Estado: {cboFiltroEstado.Text}");
            }
            else
                cboFiltroEstado.BackColor = Color.White;

            if (cboFiltroStock.SelectedIndex != 0)
            {
                cboFiltroStock.BackColor = Color.FromArgb(255, 249, 230);
                filtros.Add($"Stock: {cboFiltroStock.Text}");
            }
            else
                cboFiltroStock.BackColor = Color.White;

            if (chkSoloConVariantes.Checked)
            {
                chkSoloConVariantes.BackColor = Color.FromArgb(255, 249, 230);
                filtros.Add("Con variantes");
            }
            else
                chkSoloConVariantes.BackColor = Color.Transparent;

            if (filtros.Count > 0)
            {
                lblFiltrosActivos.Text = $"⚡ Filtros: {string.Join(" • ", filtros)}";
                lblFiltrosActivos.Visible = true;
                btnLimpiarFiltros.Enabled = true;
                btnLimpiarFiltros.BackColor = Color.FromArgb(231, 76, 60);
                btnLimpiarFiltros.ForeColor = Color.White;
            }
            else
            {
                lblFiltrosActivos.Visible = false;
                btnLimpiarFiltros.Enabled = false;
                btnLimpiarFiltros.BackColor = Color.FromArgb(189, 195, 199);
                btnLimpiarFiltros.ForeColor = Color.FromArgb(50, 49, 48);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Botones de CRUD
        // ─────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────
        // Métodos auxiliares
        // ─────────────────────────────────────────────────────────────────────────
        protected override bool DetectChanges()
        {
            return (ucGeneral?.HasChanges == true) ||
                   (ucVariantes?.HasChanges == true) ||
                   (ucPresentaciones?.HasChanges == true) ||
                   (ucConversiones?.HasChanges == true) ||
                   (ucCodigos?.HasChanges == true) ||
                   (ucPreciosVenta?.HasChanges == true) ||
                   (ucStock?.HasChanges == true);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Atajos de teclado
        // ─────────────────────────────────────────────────────────────────────────
        private void Frm_Producto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5) // Refrescar lista
            {
                btnRefrescar.PerformClick();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F2 && _productoActual != null)
            {
                e.SuppressKeyPress = true;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Liberar recursos
        // ─────────────────────────────────────────────────────────────────────────
        private bool _resourcesCleaned;

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // base first: dirty guard + scope dispose
            base.OnFormClosing(e);

            if (e.Cancel)
                return;

            CleanupResources();
        }

        private void CleanupResources()
        {
            if (_resourcesCleaned)
                return;

            _resourcesCleaned = true;

            _tabLoadingManager?.Clear();

            if (_busquedaTimer is not null)
            {
                _busquedaTimer.Tick -= OnBusquedaTimerTick;
                _busquedaTimer.Stop();
                _busquedaTimer.Dispose();
                _busquedaTimer = null;
            }

            ThemeManager.Instance.ThemeChanged -= OnThemeChanged;

            if (_btnToggleExplorador != null)
            {
                _btnToggleExplorador.Click -= BtnToggleExplorador_Click;
                _btnToggleExplorador = null;
            }

            ucVariantes.VarianteSeleccionada -= OnVarianteSeleccionada;
            ucPresentaciones.PresentacionSeleccionada -= OnPresentacionSeleccionada;

            _btnLight.Click -= BtnLight_Click;
            _btnDark.Click -= BtnDark_Click;
            _btnSystem.Click -= BtnSystem_Click;
            _btnNotificacion.Click -= _btnNotificacion_Click;
        }

        private void _btnNotificacion_Click(object sender, EventArgs e)
        {
            NotificationManager.Info("No hay una empresa activa. Reinicie la aplicación.");
            NotificationManager.Warning("No hay una empresa activa. Reinicie la aplicación.");
            NotificationManager.Success("No hay una empresa activa. Reinicie la aplicación.");
            NotificationManager.Error("No hay una empresa activa. Reinicie la aplicación.");
        }
    }
}


