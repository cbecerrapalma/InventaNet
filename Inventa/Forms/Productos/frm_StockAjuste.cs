using CBP.Data.Abstractions;
using CBP.MultiTenant.Abstractions;
using CBP.MultiTenant.Extensions;
using CBP.WinFrms.Notification;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Inventa.Forms.Productos
{
    public partial class frm_StockAjuste : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IServiceScopeFactory _scopeFactory;

        // ── Scope y UnitOfWork propios — necesarios porque este form ejecuta un SP
        // que escribe en BD; usar el scope del padre causaría conflictos de tracking.
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorios (lazy) ───────────────────────────────────────────────
        private IProdRepository? _prodRepo;
        private IPresProdRepository? _presProdRepo;
        private ILoteRepository? _loteRepo;
        private IBodegaRepository? _bodegaRepo;
        private IStockRepository? _stockRepo;

        // ── Parámetro de entrada ──────────────────────────────────────────────
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long IdProd { get; set; }

        // ── Estado interno ────────────────────────────────────────────────────
        private int _idTenant;
        private Prod? _producto;
        private bool _cargando;

        // ── Tipos fuertemente tipados — eliminan dynamic ──────────────────────
        private sealed record TipoAjusteItem(byte Valor, string Texto)
        {
            public override string ToString() => Texto;
        }

        private sealed record LoteItem(long? IdLote, string Codigo)
        {
            public override string ToString() => Codigo;
        }

        /// <summary>DTO para cboPresentacion — elimina acceso dynamic sobre tipo anónimo.</summary>
        private sealed record PresItem(long IdPresProd, long IdVarProd, string Descrip)
        {
            public override string ToString() => Descrip;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_StockAjuste(
            ITenantContext tenantContext,
            IServiceScopeFactory scopeFactory)
        {
            InitializeComponent();
            _tenantContext = tenantContext;
            _scopeFactory = scopeFactory;

            this.KeyPreview = true;
            this.KeyDown += Frm_StockAjuste_KeyDown;

            this.FormClosing += (s, e) => _formScope?.Dispose();

            ConfigurarCombos();
            ConfigurarEventos();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarCombos()
        {
            // TipoAjuste — byte coincide con TipoMov del SP (3=Entrada, 4=Salida)
            cboTipoAjuste.DataSource = new List<TipoAjusteItem>
            {
                new(3, "ENTRADA (+)  —  Aumenta stock"),
                new(4, "SALIDA (-)   —  Reduce stock")
            };
            cboTipoAjuste.DisplayMember = nameof(TipoAjusteItem.Texto);
            cboTipoAjuste.ValueMember = nameof(TipoAjusteItem.Valor);
            cboTipoAjuste.SelectedIndex = 0;
        }

        private void ConfigurarEventos()
        {
            cboPresentacion.SelectedIndexChanged += CboPresentacion_SelectedIndexChanged;
            cboBodega.SelectedIndexChanged += CboBodega_SelectedIndexChanged;
            cboTipoAjuste.SelectedIndexChanged += CboTipoAjuste_SelectedIndexChanged;
            numCantidad.ValueChanged += NumCantidad_ValueChanged;
            txtMotivo.TextChanged += TxtMotivo_TextChanged;
            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += BtnCancelar_Click;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_StockAjuste_Load(object sender, EventArgs e)
        {
            if (IdProd <= 0)
            {
                NotificationManager.Error("No se ha especificado un producto valido.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            var tenantResult = _tenantContext.TryGetCurrent();
            if (tenantResult.IsFailure)
            {
                NotificationManager.Error("No hay empresa activa.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _idTenant = tenantResult.Value.Id;

            try
            {
                // Crear scope propio para este formulario
                _formScope = _scopeFactory.CreateScope();
                _unitOfWork = _formScope.ServiceProvider
                    .GetRequiredService<IUnitOfWorkSync>();

                CargarProducto();
                CargarBodegas();
                CargarPresentaciones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al inicializar: {ex.Message}");
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA DE DATOS
        // ═════════════════════════════════════════════════════════════════════

        private void CargarProducto()
        {
            _prodRepo ??= _unitOfWork!.GetCustomRepository<IProdRepository>();
            var result = _prodRepo.GetById(IdProd);

            if (!result.IsSuccess || result.Value == null)
            {
                NotificationManager.Error("No se pudo cargar la informacion del producto.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _producto = result.Value;
            lblProductoValor.Text = $"{_producto.Codigo}  —  {_producto.Nombre}";
            this.Text = $"Ajuste de Stock  —  {_producto.Codigo}";
        }

        /// <summary>
        /// Carga las bodegas activas del tenant.
        /// La bodega es obligatoria para el SP sp_AjustarInventario (@IdBodega != 0).
        /// </summary>
        private void CargarBodegas()
        {
            _bodegaRepo ??= _unitOfWork!.GetCustomRepository<IBodegaRepository>();
            var result = _bodegaRepo.GetByTenant(_idTenant, soloActivas: true);

            if (!result.IsSuccess || result.Value.Count == 0)
            {
                NotificationManager.Error("No hay bodegas activas disponibles. No se puede realizar un ajuste.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _cargando = true;
            try
            {
                cboBodega.DataSource = result.Value.ToList();
                cboBodega.DisplayMember = nameof(Bodega.Nombre);
                cboBodega.ValueMember = nameof(Bodega.IdBodega);
                cboBodega.SelectedIndex = 0;
            }
            finally
            {
                _cargando = false;
            }
        }

        /// <summary>
        /// Carga las presentaciones activas del producto.
        /// GetByProducto incluye VarProd y Unidad — disponibles para el texto descriptivo.
        /// </summary>
        private void CargarPresentaciones()
        {
            _presProdRepo ??= _unitOfWork!.GetCustomRepository<IPresProdRepository>();
            var result = _presProdRepo.GetByProducto(IdProd, soloActivas: true);

            if (!result.IsSuccess || result.Value.Count == 0)
            {
                NotificationManager.Warning("El producto no tiene presentaciones activas. No se puede ajustar stock.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _cargando = true;
            try
            {
                // Proyectar a PresItem fuertemente tipado — sin dynamic
                var items = result.Value
                    .Select(p => new PresItem(
                        p.IdPresProd,
                        p.IdVarProd,
                        $"{p.VarProd.Nombre}  —  {p.Descrip} ({p.Unidad.Codigo})"))
                    .ToList();

                cboPresentacion.DataSource = items;
                cboPresentacion.DisplayMember = nameof(PresItem.Descrip);
                cboPresentacion.ValueMember = nameof(PresItem.IdPresProd);
                cboPresentacion.SelectedIndex = 0;
            }
            finally
            {
                _cargando = false;
            }

            // Disparar carga de lotes para la primera presentación
            CargarLotes();
        }

        /// <summary>
        /// Carga los lotes activos para la variante seleccionada.
        /// Solo aplica si el producto maneja lotes (Prod.ManejaLot = true).
        /// El ítem "Sin Lote" siempre está disponible como primera opción.
        /// </summary>
        private void CargarLotes()
        {
            if (_producto == null) return;

            // Obtener IdVarProd de la presentación seleccionada
            long idVarProd = ObtenerIdVarProdSeleccionado();
            if (idVarProd <= 0) return;

            _cargando = true;
            try
            {
                var lotes = new List<LoteItem>
                {
                    new(null, "— Sin lote —")
                };

                if (_producto.ManejaLot)
                {
                    _loteRepo ??= _unitOfWork!.GetCustomRepository<ILoteRepository>();
                    var result = _loteRepo.GetByVariante(_idTenant, idVarProd, soloActivos: true);

                    if (result.IsSuccess && result.Value.Count > 0)
                    {
                        // Añadir lotes reales al inicio de la lista
                        var lotesReales = result.Value
                            .Select(l => new LoteItem(l.IdLote,
                                l.FecVen.HasValue
                                    ? $"{l.Codigo}  (vence: {l.FecVen:dd/MM/yyyy})"
                                    : l.Codigo))
                            .ToList();

                        lotes.InsertRange(0, lotesReales);
                    }
                }

                cboLote.DataSource = lotes;
                cboLote.DisplayMember = nameof(LoteItem.Codigo);
                cboLote.ValueMember = nameof(LoteItem.IdLote);
                cboLote.SelectedIndex = 0;

                // Ocultar selector si el producto no maneja lotes
                bool manejaLot = _producto.ManejaLot;
                lblLote.Visible = manejaLot;
                cboLote.Visible = manejaLot;
            }
            finally
            {
                _cargando = false;
            }
        }

        /// <summary>
        /// Actualiza el stock actual mostrado cuando cambia la combinación
        /// presentación / bodega — permite al operador ver cuánto hay antes de ajustar.
        /// </summary>
        private void ActualizarStockActual()
        {
            int idBodega = ObtenerIdBodegaSeleccionado();
            long idVarProd = ObtenerIdVarProdSeleccionado();

            if (idBodega <= 0 || idVarProd <= 0 || _stockRepo == null)
            {
                lblStockActualValor.Text = "-";
                return;
            }

            var loteItem = cboLote.SelectedItem as LoteItem;
            long? idLote = loteItem?.IdLote;

            var result = _stockRepo.Get(_idTenant, idBodega, idVarProd, idLote);
            if (result.IsSuccess && result.Value != null)
            {
                decimal cant = result.Value.CantAct;
                lblStockActualValor.Text = $"{cant:N4}";
                lblStockActualValor.ForeColor = cant < 0
                    ? Color.FromArgb(209, 52, 56)   // rojo — negativo
                    : cant == 0
                        ? Color.FromArgb(202, 80, 16) // naranja — agotado
                        : Color.FromArgb(16, 124, 16); // verde — disponible
            }
            else
            {
                lblStockActualValor.Text = "0.0000  (sin registro)";
                lblStockActualValor.ForeColor = Color.FromArgb(96, 94, 92);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // ATAJOS DE TECLADO
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_StockAjuste_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                BtnGuardar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                BtnCancelar_Click(sender!, EventArgs.Empty);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // EVENTOS DE CONTROLES
        // ═════════════════════════════════════════════════════════════════════

        private void CboPresentacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cargando) return;
            CargarLotes();
            _stockRepo ??= _unitOfWork!.GetCustomRepository<IStockRepository>();
            ActualizarStockActual();
        }

        private void CboBodega_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cargando) return;
            _stockRepo ??= _unitOfWork!.GetCustomRepository<IStockRepository>();
            ActualizarStockActual();
        }

        private void CboTipoAjuste_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cargando) return;
            // Colorear el combo según tipo para feedback visual inmediato
            bool esEntrada = ObtenerTipoAjuste() == 3;
            cboTipoAjuste.BackColor = esEntrada
                ? Color.FromArgb(223, 246, 221)  // verde claro
                : Color.FromArgb(253, 231, 233);  // rojo claro
        }

        private void NumCantidad_ValueChanged(object? sender, EventArgs e)
        {
            if (_cargando) return;
            numCantidad.BackColor = numCantidad.Value <= 0
                ? Color.LightPink
                : Color.White;
        }

        private void TxtMotivo_TextChanged(object? sender, EventArgs e)
        {
            if (_cargando) return;
            txtMotivo.BackColor = string.IsNullOrWhiteSpace(txtMotivo.Text)
                ? Color.LightPink
                : Color.White;
        }

        // ═════════════════════════════════════════════════════════════════════
        // VALIDACIÓN
        // ═════════════════════════════════════════════════════════════════════

        private bool ValidarDatos()
        {
            bool esValido = true;
            var errores = new System.Text.StringBuilder();

            if (cboPresentacion.SelectedItem == null)
            {
                errores.AppendLine("• Debe seleccionar una presentacion.");
                esValido = false;
            }

            if (ObtenerIdBodegaSeleccionado() <= 0)
            {
                errores.AppendLine("• Debe seleccionar una bodega.");
                esValido = false;
            }

            if (numCantidad.Value <= 0)
            {
                errores.AppendLine("• La cantidad debe ser mayor a cero.");
                numCantidad.BackColor = Color.LightPink;
                numCantidad.Focus();
                esValido = false;
            }

            if (string.IsNullOrWhiteSpace(txtMotivo.Text))
            {
                errores.AppendLine("• Debe ingresar el motivo del ajuste.");
                txtMotivo.BackColor = Color.LightPink;
                if (esValido) txtMotivo.Focus();
                esValido = false;
            }

            if (!esValido)
            {
                NotificationManager.Warning($"Por favor corrija los siguientes errores:\n\n{errores}");
                return false;
            }

            // Validar stock suficiente para salidas (usando la bodega seleccionada)
            if (ObtenerTipoAjuste() == 4)
            {
                int idBodega = ObtenerIdBodegaSeleccionado();
                long idVarProd = ObtenerIdVarProdSeleccionado();
                var loteItem = cboLote.SelectedItem as LoteItem;
                long? idLote = loteItem?.IdLote;

                _stockRepo ??= _unitOfWork!.GetCustomRepository<IStockRepository>();
                var stockResult = _stockRepo.Get(_idTenant, idBodega, idVarProd, idLote);

                decimal cantDisponible = stockResult.IsSuccess && stockResult.Value != null
                    ? stockResult.Value.CantAct
                    : 0m;

                if (cantDisponible < numCantidad.Value)
                {
                    NotificationManager.Warning(
                        $"Stock insuficiente en la bodega seleccionada.\n" +
                        $"Disponible: {cantDisponible:N4}\n" +
                        $"Requerido:  {numCantidad.Value:N4}");
                    return false;
                }
            }

            return true;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CRUD — AJUSTE
        // ═════════════════════════════════════════════════════════════════════

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;

            try
            {
                btnGuardar.Enabled = false;

                int idBodega = ObtenerIdBodegaSeleccionado();
                byte tipoAjuste = ObtenerTipoAjuste();
                long idPresProd = ObtenerIdPresProdSeleccionado();
                var loteItem = cboLote.SelectedItem as LoteItem;
                long? idLote = loteItem?.IdLote;

                // CantAjuste: positivo = entrada, negativo = salida
                decimal cantAjuste = tipoAjuste == 3
                    ? numCantidad.Value
                    : -numCantidad.Value;

                var request = new AjustarInventarioRequest(
                    IdTenant: _idTenant,
                    IdBodega: idBodega,
                    Items: new List<AjusteItemSp>
                    {
                        new AjusteItemSp(
                            IdPresProd:   idPresProd,
                            IdLote:       idLote,
                            CantAjuste:   cantAjuste,
                            CostoUnitario: 0m
                        )
                    }.AsReadOnly(),
                    IdUsuario: null
                );

                _stockRepo ??= _unitOfWork!.GetCustomRepository<IStockRepository>();
                var result = _stockRepo.AjustarInventario(request);

                if (result.IsSuccess)
                {
                    string dir = tipoAjuste == 3 ? "entrada" : "salida";
                    string nombre = ObtenerNombrePresentacionSeleccionada();
                    NotificationManager.Success(
                        $"Ajuste de {dir} realizado exitosamente.\n" +
                        $"Presentacion: {nombre}\n" +
                        $"Cantidad: {numCantidad.Value:N4}\n" +
                        $"Motivo: {txtMotivo.Text.Trim()}");

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    NotificationManager.Error($"Error al realizar ajuste: {result.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error inesperado: {ex.Message}");
            }
            finally
            {
                btnGuardar.Enabled = true;
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ═════════════════════════════════════════════════════════════════════
        // AUXILIARES — sin dynamic, sin acceso frágil a SelectedItem
        // ═════════════════════════════════════════════════════════════════════

        private byte ObtenerTipoAjuste()
        {
            if (cboTipoAjuste.SelectedItem is TipoAjusteItem item)
                return item.Valor;
            return 3; // Entrada como fallback
        }

        private int ObtenerIdBodegaSeleccionado()
        {
            if (cboBodega.SelectedItem is Bodega b)
                return b.IdBodega;
            return 0;
        }

        private long ObtenerIdPresProdSeleccionado()
        {
            if (cboPresentacion.SelectedItem is PresItem item) return item.IdPresProd;
            return 0;
        }

        private long ObtenerIdVarProdSeleccionado()
        {
            if (cboPresentacion.SelectedItem is PresItem item) return item.IdVarProd;
            return 0;
        }

        private string ObtenerNombrePresentacionSeleccionada()
        {
            if (cboPresentacion.SelectedItem is PresItem item) return item.Descrip;
            return string.Empty;
        }
    }
}


