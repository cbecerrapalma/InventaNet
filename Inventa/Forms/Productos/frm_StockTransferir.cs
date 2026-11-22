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
    public partial class frm_StockTransferir : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IServiceScopeFactory _scopeFactory;

        // ── Scope y UnitOfWork propios — el SP escribe en BD; necesita
        // su propio DbContext para no interferir con el scope del padre.
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorios (lazy) ───────────────────────────────────────────────
        private IProdRepository? _prodRepo;
        private IBodegaRepository? _bodegaRepo;
        private IPresProdRepository? _presProdRepo;
        private ILoteRepository? _loteRepo;
        private IStockDispRepository? _stockDispRepo;
        private IStockRepository? _stockRepo;

        // ── Parámetro de entrada ──────────────────────────────────────────────
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long IdProd { get; set; }

        // ── Estado interno ────────────────────────────────────────────────────
        private int _idTenant;
        private Prod? _producto;
        private bool _cargando;

        // ── Tipos fuertemente tipados — eliminan dynamic ──────────────────────
        private sealed record LoteItem(long? IdLote, string Codigo)
        {
            public override string ToString() => Codigo;
        }

        private sealed record PresItem(long IdPresProd, long IdVarProd, string Descrip)
        {
            public override string ToString() => Descrip;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_StockTransferir(
            ITenantContext tenantContext,
            IServiceScopeFactory scopeFactory)
        {
            InitializeComponent();
            _tenantContext = tenantContext;
            _scopeFactory = scopeFactory;

            this.KeyPreview = true;
            this.KeyDown += Frm_StockTransferir_KeyDown;
            this.FormClosing += (s, e) => _formScope?.Dispose();

            ConfigurarEventos();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarEventos()
        {
            cboPresentacion.SelectedIndexChanged += CboPresentacion_SelectedIndexChanged;
            cboOrigen.SelectedIndexChanged += CboOrigen_SelectedIndexChanged;
            numCantidad.ValueChanged += NumCantidad_ValueChanged;
            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += BtnCancelar_Click;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_StockTransferir_Load(object sender, EventArgs e)
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
            this.Text = $"Transferencia de Stock  —  {_producto.Codigo}";
        }

        /// <summary>
        /// Carga bodegas activas del tenant en ambos combos (origen y destino).
        /// Cada combo recibe su propia lista para evitar binding compartido.
        /// Si solo hay una bodega no se puede transferir — se cierra con advertencia.
        /// </summary>
        private void CargarBodegas()
        {
            _bodegaRepo ??= _unitOfWork!.GetCustomRepository<IBodegaRepository>();
            var result = _bodegaRepo.GetByTenant(_idTenant, soloActivas: true);

            if (!result.IsSuccess || result.Value.Count < 2)
            {
                NotificationManager.Error(
                    result.Value?.Count == 1
                        ? "Solo hay una bodega activa. Se necesitan al menos dos para realizar una transferencia."
                        : "No hay bodegas activas para realizar la transferencia.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _cargando = true;
            try
            {
                var bodegas = result.Value.ToList();

                // Listas separadas para evitar que el binding de un combo afecte al otro
                cboOrigen.DataSource = bodegas.ToList();
                cboOrigen.DisplayMember = nameof(Bodega.Nombre);
                cboOrigen.ValueMember = nameof(Bodega.IdBodega);
                cboOrigen.SelectedIndex = 0;

                cboDestino.DataSource = bodegas.ToList();
                cboDestino.DisplayMember = nameof(Bodega.Nombre);
                cboDestino.ValueMember = nameof(Bodega.IdBodega);
                cboDestino.SelectedIndex = 1; // Preseleccionar segunda bodega para evitar mismo origen/destino
            }
            finally
            {
                _cargando = false;
            }
        }

        /// <summary>
        /// Carga presentaciones activas del producto con texto descriptivo completo.
        /// GetByProducto incluye VarProd y Unidad — disponibles para armar el texto.
        /// </summary>
        private void CargarPresentaciones()
        {
            _presProdRepo ??= _unitOfWork!.GetCustomRepository<IPresProdRepository>();
            var result = _presProdRepo.GetByProducto(IdProd, soloActivas: true);

            if (!result.IsSuccess || result.Value.Count == 0)
            {
                NotificationManager.Warning("El producto no tiene presentaciones activas. No se puede transferir stock.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _cargando = true;
            try
            {
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

            CargarLotes();
            ActualizarStockOrigen();
        }

        /// <summary>
        /// Carga lotes activos de la variante seleccionada.
        /// Solo aplica si el producto maneja lotes (Prod.ManejaLot = true).
        /// "— Sin lote —" siempre está disponible como primera opción.
        /// </summary>
        private void CargarLotes()
        {
            if (_producto == null) return;

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
        /// Muestra el stock disponible en bodega origen para la presentación seleccionada.
        /// Usa IStockDispRepository.GetCantidad — consulta directa sobre la vista StockDisp,
        /// una sola query sin joins adicionales.
        /// </summary>
        private void ActualizarStockOrigen()
        {
            int idBodegaOrig = ObtenerIdBodegaOrigen();
            long idPresProd = ObtenerIdPresProdSeleccionado();

            if (idBodegaOrig <= 0 || idPresProd <= 0)
            {
                lblStockOrigenValor.Text = "-";
                lblStockOrigenValor.ForeColor = Color.FromArgb(96, 94, 92);
                return;
            }

            _stockDispRepo ??= _unitOfWork!.GetCustomRepository<IStockDispRepository>();
            var result = _stockDispRepo.GetCantidad(_idTenant, idPresProd, idBodegaOrig);

            if (result.IsSuccess)
            {
                decimal cant = result.Value;
                lblStockOrigenValor.Text = $"{cant:N4}";
                lblStockOrigenValor.ForeColor = cant <= 0
                    ? Color.FromArgb(202, 80, 16)   // naranja — sin stock
                    : Color.FromArgb(16, 124, 16);   // verde — hay stock
            }
            else
            {
                lblStockOrigenValor.Text = "0.0000  (sin registro)";
                lblStockOrigenValor.ForeColor = Color.FromArgb(96, 94, 92);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // ATAJOS DE TECLADO
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_StockTransferir_KeyDown(object? sender, KeyEventArgs e)
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
            ActualizarStockOrigen();
        }

        private void CboOrigen_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cargando) return;
            ActualizarStockOrigen();
        }

        private void NumCantidad_ValueChanged(object? sender, EventArgs e)
        {
            if (_cargando) return;
            numCantidad.BackColor = numCantidad.Value <= 0
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

            int idOrigen = ObtenerIdBodegaOrigen();
            int idDestino = ObtenerIdBodegaDestino();

            if (idOrigen <= 0 || idDestino <= 0)
            {
                errores.AppendLine("• Debe seleccionar las bodegas de origen y destino.");
                esValido = false;
            }
            else if (idOrigen == idDestino)
            {
                errores.AppendLine("• Las bodegas de origen y destino no pueden ser la misma.");
                esValido = false;
            }

            if (cboPresentacion.SelectedItem == null)
            {
                errores.AppendLine("• Debe seleccionar una presentacion.");
                esValido = false;
            }

            if (numCantidad.Value <= 0)
            {
                errores.AppendLine("• La cantidad debe ser mayor a cero.");
                numCantidad.BackColor = Color.LightPink;
                if (esValido) numCantidad.Focus();
                esValido = false;
            }

            if (!esValido)
            {
                NotificationManager.Warning($"Por favor corrija los siguientes errores:\n\n{errores}");
                return false;
            }

            // Verificar stock suficiente en bodega origen antes de llamar al SP
            long idPresProd = ObtenerIdPresProdSeleccionado();
            _stockDispRepo ??= _unitOfWork!.GetCustomRepository<IStockDispRepository>();

            var disponibleResult = _stockDispRepo.TieneStockSuficiente(
                _idTenant, idPresProd, idOrigen, numCantidad.Value);

            if (disponibleResult.IsSuccess && !disponibleResult.Value)
            {
                var cantResult = _stockDispRepo.GetCantidad(_idTenant, idPresProd, idOrigen);
                decimal disponible = cantResult.IsSuccess ? cantResult.Value : 0m;

                NotificationManager.Warning(
                    $"Stock insuficiente en bodega origen.\n" +
                    $"Disponible: {disponible:N4}\n" +
                    $"Requerido:  {numCantidad.Value:N4}");
                return false;
            }

            return true;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CRUD — TRANSFERENCIA
        // ═════════════════════════════════════════════════════════════════════

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;

            try
            {
                btnGuardar.Enabled = false;

                int idOrigen = ObtenerIdBodegaOrigen();
                int idDestino = ObtenerIdBodegaDestino();
                long idPresProd = ObtenerIdPresProdSeleccionado();
                var loteItem = cboLote.SelectedItem as LoteItem;
                long? idLote = loteItem?.IdLote;

                var request = new TransferirEntresBodegasRequest(
                    IdTenant: _idTenant,
                    IdBodegaOrig: idOrigen,
                    IdBodegaDest: idDestino,
                    Items: new List<TransferenciaItemSp>
                    {
                        new TransferenciaItemSp(
                            IdPresProd: idPresProd,
                            IdLote:     idLote,
                            Cantidad:   numCantidad.Value
                        )
                    }.AsReadOnly(),
                    IdUsuario: null
                );

                _stockRepo ??= _unitOfWork!.GetCustomRepository<IStockRepository>();
                var result = _stockRepo.TransferirEntresBodegas(request);

                if (result.IsSuccess)
                {
                    string nombreOrigen = (cboOrigen.SelectedItem as Bodega)?.Nombre ?? "-";
                    string nombreDestino = (cboDestino.SelectedItem as Bodega)?.Nombre ?? "-";
                    string nombrePres = ObtenerNombrePresentacionSeleccionada();

                    NotificationManager.Success(
                        $"Transferencia realizada exitosamente.\n" +
                        $"Presentacion: {nombrePres}\n" +
                        $"Cantidad: {numCantidad.Value:N4}\n" +
                        $"Origen: {nombreOrigen}  →  Destino: {nombreDestino}");

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    NotificationManager.Error($"Error al realizar transferencia: {result.Error?.Message}");
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
        // AUXILIARES — sin dynamic
        // ═════════════════════════════════════════════════════════════════════

        private int ObtenerIdBodegaOrigen()
        {
            if (cboOrigen.SelectedItem is Bodega b) return b.IdBodega;
            return 0;
        }

        private int ObtenerIdBodegaDestino()
        {
            if (cboDestino.SelectedItem is Bodega b) return b.IdBodega;
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


