using CBP.Data.Abstractions;
using CBP.MultiTenant.Abstractions;
using CBP.MultiTenant.Extensions;
using CBP.WinFrms.Notification;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.Infrastructure;
using Inventa.UI;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Inventa.Forms.Productos
{
    public partial class frm_Stock : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFormFactory _formFactory;

        // ── Ámbito y UnitOfWork del formulario ───────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorios (lazy) ───────────────────────────────────────────────
        // IBodegaRepository eliminado — era código muerto; GetByVariante
        // ya incluye la navegación Bodega mediante .Include(s => s.Bodega).
        private IStockRepository? _stockRepo;
        private IProdRepository? _prodRepo;
        private IVarProdRepository? _varProdRepo;

        // ── Parámetro de entrada ──────────────────────────────────────────────
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long IdProd { get; set; }

        // ── Estado interno ────────────────────────────────────────────────────
        private Prod? _productoActual;
        private List<StockItem> _stockList = new();
        private bool _isLoading;

        // ── DTO interno aplanado para la grilla ───────────────────────────────
        // Incluye IdVarProd para poder agrupar / filtrar por variante,
        // e IdBodega para el cálculo de costo promedio por bodega.
        private sealed record StockItem(
            long IdStock,
            long IdVarProd,
            string Variante,
            string Bodega,
            string Lote,
            decimal CantAct,
            decimal CostoProm,
            decimal Valorizado,
            DateTime FecAct
        );

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_Stock(
            ITenantContext tenantContext,
            IServiceScopeFactory scopeFactory,
            IFormFactory formFactory)
        {
            InitializeComponent();
            _tenantContext = tenantContext;
            _scopeFactory = scopeFactory;
            _formFactory = formFactory;

            this.KeyPreview = true;
            this.KeyDown += Frm_Stock_KeyDown;

            ConfigurarGrid();

            this.FormClosing += (s, e) => _formScope?.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN DE COLUMNAS
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarGrid()
        {
            dgvStock.AutoGenerateColumns = false;
            dgvStock.Columns.Clear();

            // ID oculto — para operaciones internas
            dgvStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IdStock",
                HeaderText = "ID",
                DataPropertyName = nameof(StockItem.IdStock),
                Visible = false
            });

            // IdVarProd oculto — para coloreado de filas alternas por variante
            dgvStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IdVarProd",
                HeaderText = "IdVar",
                DataPropertyName = nameof(StockItem.IdVarProd),
                Visible = false
            });

            dgvStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Variante",
                HeaderText = "Variante",
                DataPropertyName = nameof(StockItem.Variante),
                Width = 200
            });

            dgvStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Bodega",
                HeaderText = "Bodega",
                DataPropertyName = nameof(StockItem.Bodega),
                Width = 130
            });

            dgvStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Lote",
                HeaderText = "Lote",
                DataPropertyName = nameof(StockItem.Lote),
                Width = 110
            });

            dgvStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CantAct",
                HeaderText = "Cantidad",
                DataPropertyName = nameof(StockItem.CantAct),
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N4",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dgvStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CostoProm",
                HeaderText = "Costo Prom.",
                DataPropertyName = nameof(StockItem.CostoProm),
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C4",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dgvStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Valorizado",
                HeaderText = "Valorizado",
                DataPropertyName = nameof(StockItem.Valorizado),
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dgvStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FecAct",
                HeaderText = "Ult. Actualizacion",
                DataPropertyName = nameof(StockItem.FecAct),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });

            // Coloreado de filas por stock negativo y agrupación visual por variante
            dgvStock.RowPrePaint += DgvStock_RowPrePaint;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_Stock_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                var tenantResult = _tenantContext.TryGetCurrent();
                if (tenantResult.IsFailure)
                {
                    NotificationManager.Error("No hay empresa activa.");
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                _formScope = _scopeFactory.CreateScope();
                _unitOfWork = _formScope.ServiceProvider
                    .GetRequiredService<IUnitOfWorkSync>();

                if (IdProd <= 0)
                {
                    NotificationManager.Error("No se especifico un producto valido.");
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                CargarProducto(IdProd);
                CargarStock(tenantResult.Value.Id);
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al cargar: {ex.Message}");
                DialogResult = DialogResult.Cancel;
                Close();
            }
            finally
            {
                _isLoading = false;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // ATAJOS DE TECLADO
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_Stock_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                BtnRefrescar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                Close();
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA DE DATOS
        // ═════════════════════════════════════════════════════════════════════

        private void CargarProducto(long idProd)
        {
            _prodRepo ??= _unitOfWork!.GetCustomRepository<IProdRepository>();
            var result = _prodRepo.GetById(idProd);
            if (result.IsSuccess && result.Value != null)
            {
                _productoActual = result.Value;
                this.Text = $"Stock  —  {_productoActual.Codigo}  {_productoActual.Nombre}";
                lblProducto.Text = $"{_productoActual.Codigo}  —  {_productoActual.Nombre}";
            }
        }

        /// <summary>
        /// Carga el stock de todas las variantes del producto activo.
        /// Carga todas las variantes del producto y su stock en cada bodega.
        /// GetByVariante ya incluye Bodega y Lote mediante .Include().
        /// Los items con CantAct negativa se marcan visualmente en RowPrePaint.
        /// </summary>
        private void CargarStock(int idTenant)
        {
            try
            {
                _stockRepo ??= _unitOfWork!.GetCustomRepository<IStockRepository>();
                _varProdRepo ??= _unitOfWork!.GetCustomRepository<IVarProdRepository>();

                var variantesResult = _varProdRepo.GetByProducto(IdProd, soloActivos: false);
                if (!variantesResult.IsSuccess)
                {
                    NotificationManager.Warning($"Error al cargar variantes: {variantesResult.Error?.Message}");
                    return;
                }

                var stocks = new List<StockItem>();

                foreach (var varProd in variantesResult.Value)
                {
                    var stockResult = _stockRepo.GetByVariante(idTenant, varProd.IdVarProd);
                    if (!stockResult.IsSuccess) continue;

                    foreach (var s in stockResult.Value)
                    {
                        stocks.Add(new StockItem(
                            IdStock: s.IdStock,
                            IdVarProd: varProd.IdVarProd,
                            Variante: varProd.Nombre,
                            Bodega: s.Bodega?.Nombre ?? string.Empty,
                            Lote: s.Lote?.Codigo ?? string.Empty,
                            CantAct: s.CantAct,
                            CostoProm: s.CostoProm,
                            Valorizado: s.CantAct * s.CostoProm,
                            FecAct: s.FecAct
                        ));
                    }
                }

                _stockList = stocks;
                dgvStock.DataSource = null;
                dgvStock.DataSource = _stockList;

                ActualizarTotales();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al cargar stock: {ex.Message}");
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // COLOREADO DE FILAS
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Marca en rojo las filas con stock negativo (inconsistencia de inventario).
        /// Aplica un fondo alternado tenue por bloque de variante para facilitar
        /// la lectura cuando un producto tiene múltiples variantes.
        /// </summary>
        private void DgvStock_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvStock.Rows.Count) return;

            var row = dgvStock.Rows[e.RowIndex];
            var item = row.DataBoundItem as StockItem;
            if (item == null) return;

            if (item.CantAct < 0)
            {
                // Stock negativo: fondo rojo claro — indica inconsistencia de inventario
                row.DefaultCellStyle.BackColor = AppColors.ErrorLight;
                row.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
                row.DefaultCellStyle.SelectionBackColor = AppColors.Error;
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            else if (item.CantAct == 0)
            {
                // Sin stock: fondo amarillo claro — agotado pero sin inconsistencia
                row.DefaultCellStyle.BackColor = AppColors.WarningLight;
                row.DefaultCellStyle.ForeColor = AppColors.TextSecondary;
                row.DefaultCellStyle.SelectionBackColor = AppColors.Warning;
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            else
            {
                // Stock normal: colores estándar
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = AppColors.TextSecondary;
                row.DefaultCellStyle.SelectionBackColor = AppColors.PrimaryLight;
                row.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // TOTALES Y ESTADO DE BOTONES
        // ═════════════════════════════════════════════════════════════════════

        private void ActualizarTotales()
        {
            decimal totalUnidades = _stockList.Sum(s => s.CantAct);
            decimal totalValorizado = _stockList.Sum(s => s.Valorizado);
            int registros = _stockList.Count;
            int negativos = _stockList.Count(s => s.CantAct < 0);

            string texto = $"Registros: {registros}   |   Total unidades: {totalUnidades:N4}   |   Valorizado: {totalValorizado:C2}";
            if (negativos > 0)
                texto += $"   |   ⚠ {negativos} con stock negativo";

            lblTotales.Text = texto;
            lblTotales.ForeColor = negativos > 0 ? AppColors.Error : AppColors.TextPrimary;
        }

        private void ActualizarEstadoBotones()
        {
            bool tieneProducto = _productoActual != null;
            btnAjustar.Enabled = tieneProducto;
            btnTransferir.Enabled = tieneProducto;
        }

        // ═════════════════════════════════════════════════════════════════════
        // HANDLERS DE BOTONES
        // ═════════════════════════════════════════════════════════════════════

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            var tenantResult = _tenantContext.TryGetCurrent();
            if (tenantResult.IsSuccess)
                CargarStock(tenantResult.Value.Id);
        }

        private void BtnAjustar_Click(object sender, EventArgs e)
        {
            if (_isLoading || _productoActual == null) return;

            var result = _formFactory.ShowDialog<frm_StockAjuste>(form =>
            {
                form.IdProd = _productoActual.IdProd;
            });

            if (result == DialogResult.OK)
                BtnRefrescar_Click(sender, e);
        }

        private void BtnTransferir_Click(object sender, EventArgs e)
        {
            if (_isLoading || _productoActual == null) return;

            var result = _formFactory.ShowDialog<frm_StockTransferir>(form =>
            {
                form.IdProd = _productoActual.IdProd;
            });

            if (result == DialogResult.OK)
                BtnRefrescar_Click(sender, e);
        }

    }
}


