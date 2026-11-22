using CBP.Data.Abstractions;
using CBP.Results;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.UC.Producto.Base;
using Inventa.UC.Producto.Interfaces;
using System.ComponentModel;

namespace Inventa.UC.Producto;

/// <summary>
/// ProdStockUC: Gestión de stock consolidado y movimientos de inventario.
/// Muestra stock actual por bodega/variante e historial de movimientos (últimos 3 meses).
/// Patrón: SetContainer → InitializeData → RefreshData con sincronización vía ProductoChanged.
/// UI generada por Designer.cs con grillas, combos, tabs para ajustes y transferencias.
/// </summary>
public partial class ProdStockUC : BaseProductoUC
{
    private IProductoContainer? _container;
    private Prod? _productoActual;
    private long? _varianteSeleccionada;  // ✅ Variante seleccionada para filtrado

    // Repositorios (lazy loading)
    private IStockRepository? _stockRepo;
    private IStockDispRepository? _stockDispRepo;
    private IMovInvRepository? _movInvRepo;
    private IBodegaRepository? _bodegaRepo;
    private IVarProdRepository? _varProdRepo;
    private IPresProdRepository? _presProdRepo;
    private ILoteRepository? _loteRepo;

    // UI State
    private bool _inicializando;
    private List<Stock>? _stockActual;
    private List<MovInv>? _movimientos;
    private List<Bodega>? _bodegas;

    public override bool HasChanges => false; // Stock es solo lectura
    public override bool IsEditing => false;
    public override bool IsValid => true;

    public bool Guardar() => true;

    /// <summary>
    /// Establece la variante seleccionada y recarga stock filtrado.
    /// </summary>
    public void SetVarianteSeleccionada(long idVarProd)
    {
        _varianteSeleccionada = idVarProd;
        _inicializando = true;
        try
        {
            RefreshData();
        }
        finally
        {
            _inicializando = false;
        }
    }

    public ProdStockUC()
    {
        InitializeComponent();

        // ✅ PATRÓN: Usar CellClick para detectar selección incluso con 1 registro
        dgvStock.CellClick += DgvStock_CellClick;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IProductoContainer integration
    // ─────────────────────────────────────────────────────────────────────────

public override void SetContainer(IProductoContainer container)
     {
         _container = container ?? throw new ArgumentNullException(nameof(container));
         base.SetContainer(container);
     }

    public void InitializeData()
    {
        if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            return;

        if (_container == null || _container.ProductoActual == null) return;

        _productoActual = _container.ProductoActual;

        // Cargar bodegas para combos
        CargarBodegas();
        CargarLotes();

        // Suscribirse a cambios de producto
        _container.ProductoChanged += OnProductoChanged;

        // ✅ Accessibility setup
        SetAccessible(cboBodega, "Bodega", "Seleccione bodega para filtrar stock");
        SetAccessible(cboVariante, "Variante", "Seleccione variante para ver movimientos");
        SetAccessible(cboLoteAjuste, "Lote para Ajuste", "Seleccione lote para registrar ajuste");
        SetAccessible(cboBodegaOrigen, "Bodega Origen", "Bodega origen para transferencia");
        SetAccessible(cboBodegaDestino, "Bodega Destino", "Bodega destino para transferencia");
        SetAccessible(cboLoteTransferencia, "Lote para Transferencia", "Seleccione lote para transferencia");
        SetAccessible(dgvStock, "Stock", "Tabla de stock actual por bodega y variante");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IRefreshable
    // ─────────────────────────────────────────────────────────────────────────

    public override void RefreshData()
    {
        if (_productoActual == null) return;

        CargarStockVariante();
        CargarMovimientosVariante();
    }

    public override void Clear()
    {
        dgvStock.DataSource = null;
        cboBodega.Items.Clear();
        cboVariante.Items.Clear();
        cboLoteAjuste.Items.Clear();
        cboBodegaOrigen.Items.Clear();
        cboBodegaDestino.Items.Clear();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IEditable (no-op: stock es solo lectura)
    // ─────────────────────────────────────────────────────────────────────────

public override void BeginEdit() { base.BeginEdit(); }
     public override void EndEdit() { base.EndEdit(); }
     public override void CancelEdit() { base.CancelEdit(); }

    // ─────────────────────────────────────────────────────────────────────────
    // IValidatable (no-op: stock es solo lectura)
    // ─────────────────────────────────────────────────────────────────────────

    public override bool ValidarDatos(bool silent = false) => true;
    public override void Limpiar() => Clear();

    // ─────────────────────────────────────────────────────────────────────────
    // Carga de datos
    // ─────────────────────────────────────────────────────────────────────────

    private void CargarBodegas()
    {
        if (_container?.UnitOfWork == null) return;

        _bodegaRepo ??= _container.UnitOfWork.GetCustomRepository<IBodegaRepository>();

        var result = _bodegaRepo.GetByTenant(_container.IdTenant, soloActivas: true);
        if (result.IsSuccess)
        {
            _bodegas = result.Value.ToList();

            // Cargar combo principal
            cboBodega.Items.Clear();
            cboBodega.Items.Add(new { Nombre = "-- Todas las bodegas --", IdBodega = 0 });
            foreach (var bodega in _bodegas)
                cboBodega.Items.Add(new { Nombre = bodega.Nombre, IdBodega = bodega.IdBodega });

            cboBodega.DisplayMember = "Nombre";
            cboBodega.ValueMember = "IdBodega";
            if (cboBodega.Items.Count > 0)
                cboBodega.SelectedIndex = 0;

            // Cargar combos de origen/destino para transferencias
            cboBodegaOrigen.Items.Clear();
            cboBodegaDestino.Items.Clear();
            foreach (var bodega in _bodegas)
            {
                cboBodegaOrigen.Items.Add(new { Nombre = bodega.Nombre, IdBodega = bodega.IdBodega });
                cboBodegaDestino.Items.Add(new { Nombre = bodega.Nombre, IdBodega = bodega.IdBodega });
            }
            cboBodegaOrigen.DisplayMember = "Nombre";
            cboBodegaOrigen.ValueMember = "IdBodega";
            cboBodegaDestino.DisplayMember = "Nombre";
            cboBodegaDestino.ValueMember = "IdBodega";

            if (cboBodegaOrigen.Items.Count > 0)
                cboBodegaOrigen.SelectedIndex = 0;
            if (cboBodegaDestino.Items.Count > 1)
                cboBodegaDestino.SelectedIndex = 1;
        }

        // Cargar combo de ajuste de bodega
        cboBodegaAjuste.Items.Clear();
        foreach (var bodega in _bodegas ?? new List<Bodega>())
            cboBodegaAjuste.Items.Add(new { Nombre = bodega.Nombre, IdBodega = bodega.IdBodega });
        cboBodegaAjuste.DisplayMember = "Nombre";
        cboBodegaAjuste.ValueMember = "IdBodega";
        if (cboBodegaAjuste.Items.Count > 0)
            cboBodegaAjuste.SelectedIndex = 0;
    }

    private void CargarStockVariante()
    {
        if (_container?.UnitOfWork == null || _productoActual == null) return;

        _stockRepo ??= _container.UnitOfWork.GetCustomRepository<IStockRepository>();
        _varProdRepo ??= _container.UnitOfWork.GetCustomRepository<IVarProdRepository>();

        // Obtener variantes del producto
        var varResult = _varProdRepo.GetByProducto(_productoActual.IdProd);
        if (!varResult.IsSuccess || varResult.Value.Count == 0)
        {
            dgvStock.DataSource = null;
            return;
        }

        var stockData = new List<dynamic>();

        foreach (var variante in varResult.Value)
        {
            var result = _stockRepo.GetByVariante(_container.IdTenant, variante.IdVarProd);
            if (result.IsSuccess)
            {
                foreach (var stock in result.Value)
                {
                    stockData.Add(new
                    {
                        BodegaNombre = stock.Bodega?.Nombre ?? "---",
                        VarianteDescripcion = variante.Nombre,
                        CantActual = stock.CantAct,
                        CostoPromedio = stock.CostoProm.ToString("C2"),
                        FecActualizacion = stock.FecAct.ToString("g")
                    });
                }
            }
        }

        _stockActual = new List<Stock>(); // Guardar para referencia
        dgvStock.DataSource = stockData.Count > 0 ? stockData : null;

        // ✅ PATRÓN: Mostrar cantidad de registros en tslInfo
        tslInfo.Text = $"{stockData.Count} registro(s)";
    }

    /// <summary>
    /// ✅ PATRÓN: Usar CellClick para actualizar tslInfo incluso con 1 solo registro
    /// </summary>
    private void DgvStock_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (_inicializando || e.RowIndex < 0) return;

        try
        {
            var row = dgvStock.Rows[e.RowIndex];
            if (row.DataBoundItem != null)
            {
                dynamic item = row.DataBoundItem;
                var bodega = item.BodegaNombre ?? "---";
                var variante = item.VarianteDescripcion ?? "---";
                var cantidad = item.CantActual ?? 0;
                var costo = item.CostoPromedio ?? "---";

                tslInfo.Text = $"{bodega} | {variante} | Cantidad: {cantidad} | Costo: {costo}";
                row.Selected = true;  // Seleccionar fila
            }
            else
            {
                tslInfo.Text = $"{dgvStock.Rows.Count} registro(s)";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en DgvStock_CellClick: {ex.Message}");
            tslInfo.Text = $"{dgvStock.Rows.Count} registro(s)";
        }
    }

    private void CargarMovimientosVariante()
    {
        if (_container?.UnitOfWork == null || _productoActual == null) return;

        _varProdRepo ??= _container.UnitOfWork.GetCustomRepository<IVarProdRepository>();
        _movInvRepo ??= _container.UnitOfWork.GetCustomRepository<IMovInvRepository>();
        _presProdRepo ??= _container.UnitOfWork.GetCustomRepository<IPresProdRepository>();

        // Obtener variantes del producto
        var varResult = _varProdRepo.GetByProducto(_productoActual.IdProd);
        if (!varResult.IsSuccess || varResult.Value.Count == 0)
        {
            return;
        }

        var desde = DateTime.UtcNow.AddMonths(-3); // Últimos 3 meses
        var hasta = DateTime.UtcNow;
        _movimientos = new List<MovInv>();

        // Iterar por variantes y luego por presentaciones
        foreach (var variante in varResult.Value)
        {
            var presResult = _presProdRepo.GetByVariante(variante.IdVarProd);
            if (!presResult.IsSuccess) continue;

            foreach (var pres in presResult.Value)
            {
                var result = _movInvRepo.GetByPresentacion(_container.IdTenant, pres.IdPresProd, desde, hasta);
                if (result.IsSuccess)
                {
                    _movimientos.AddRange(result.Value);
                }
            }
        }
    }

    private void CargarLotes()
    {
        if (_container?.UnitOfWork == null || _productoActual == null) return;

        _varProdRepo ??= _container.UnitOfWork.GetCustomRepository<IVarProdRepository>();
        _loteRepo ??= _container.UnitOfWork.GetCustomRepository<ILoteRepository>();

        // Obtener variantes para cargar lotes asociados
        var varResult = _varProdRepo.GetByProducto(_productoActual.IdProd);
        if (!varResult.IsSuccess || varResult.Value.Count == 0)
            return;

        // Cargar lotes activos (para ajustes y transferencias)
        cboLoteAjuste.Items.Clear();
        cboLoteTransferencia.Items.Clear();

        foreach (var variante in varResult.Value)
        {
            var result = _loteRepo.GetByVariante(_container.IdTenant, variante.IdVarProd, soloActivos: true);
            if (!result.IsSuccess) continue;

            foreach (var lote in result.Value)
            {
                cboLoteAjuste.Items.Add(new { Nombre = lote.Codigo, IdLote = lote.IdLote });
                cboLoteTransferencia.Items.Add(new { Nombre = lote.Codigo, IdLote = lote.IdLote });
            }
        }

        cboLoteAjuste.DisplayMember = "Nombre";
        cboLoteAjuste.ValueMember = "IdLote";
        cboLoteTransferencia.DisplayMember = "Nombre";
        cboLoteTransferencia.ValueMember = "IdLote";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Event handlers
    // ─────────────────────────────────────────────────────────────────────────

    private void OnProductoChanged(object? sender, ProductoChangedEventArgs e)
    {
        _productoActual = e.ProductoNuevo;
        RefreshData();
        CargarLotes();
    }

    private void TsbAjustar_Click(object? sender, EventArgs e)
    {
        // Lógica para registrar ajuste de inventario
        // Stub - implementar después
    }

    private void TsbTransferir_Click(object? sender, EventArgs e)
    {
        // Lógica para registrar transferencia entre bodegas
        // Stub - implementar después
    }

    private void TsbVerMovimientos_Click(object? sender, EventArgs e)
    {
        RefreshData();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper methods
    // ─────────────────────────────────────────────────────────────────────────

    private string GetTipoMovimientoTexto(byte tipoMov) => tipoMov switch
    {
        1 => "Compra",
        2 => "Venta",
        3 => "Ajuste Entrada",
        4 => "Ajuste Salida",
        5 => "Transfer. Entrada",
        6 => "Transfer. Salida",
        _ => "Desconocido"
    };

    private void pnlFiltros_Paint(object sender, PaintEventArgs e)
    {

    }
}
