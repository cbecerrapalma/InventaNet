using CBP.Data.Abstractions;
using CBP.MultiTenant.Abstractions;
using CBP.MultiTenant.Extensions;
using CBP.WinFrms.Notification;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.UI;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Inventa.Forms.Productos
{
    public partial class frm_Presentacion : Form
    {
        // ── Dependencias inyectadas ──────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IServiceScopeFactory _scopeFactory;

        // ── Ámbito y UnitOfWork del formulario ───────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorios ─────────────────────────────────────────────────────────
        private IPresProdRepository? _presProdRepo;
        private IUnidadRepository? _unidadRepo;
        private IVarProdRepository? _varProdRepo;

        // ── Parámetros de entrada ────────────────────────────────────────────────
        /// <summary>ID de la variante (para nueva presentación).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long IdVarProd { get; set; }

        /// <summary>ID de la presentación (para edición).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long? IdPresProd { get; set; }

        // ── Estado interno ───────────────────────────────────────────────────────
        private PresProd? _presentacionActual;
        private bool _isLoading;

        // ── Constructor ───────────────────────────────────────────────────────────
        public frm_Presentacion(
            ITenantContext tenantContext,
            IServiceScopeFactory scopeFactory)
        {
            InitializeComponent();
            _tenantContext = tenantContext;
            _scopeFactory = scopeFactory;

            this.FormClosing += (s, e) => _formScope?.Dispose();
        }

        // ── Carga del formulario ─────────────────────────────────────────────────
        private void Frm_Presentacion_Load(object sender, EventArgs e)
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

                CargarUnidades();
                CargarInfoProducto();

                if (IdPresProd.HasValue)
                    CargarPresentacion(IdPresProd.Value);
                else
                    LimpiarFormulario();
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

        // ── Cargar combo de unidades ──────────────────────────────────────────────
        private void CargarUnidades()
        {
            _unidadRepo ??= _unitOfWork!.GetCustomRepository<IUnidadRepository>();
            var result = _unidadRepo.GetAllOrdenado();
            if (result.IsSuccess)
            {
                var unidades = result.Value.ToList();
                cboUnidad.DataSource = unidades;
                cboUnidad.DisplayMember = nameof(Unidad.Nombre);
                cboUnidad.ValueMember = nameof(Unidad.IdUnidad);
            }
            else
            {
                NotificationManager.Warning($"Error cargando unidades: {result.Error?.Message}");
            }
        }

        // ── Cargar información del producto/variante ──────────────────────────────
        private void CargarInfoProducto()
        {
            if (IdVarProd > 0)
            {
                _varProdRepo ??= _unitOfWork!.GetCustomRepository<IVarProdRepository>();
                var varResult = _varProdRepo.GetById(IdVarProd);
                if (varResult.IsSuccess && varResult.Value != null)
                {
                    var variante = varResult.Value;
                    txtProductoInfo.Text = $"{variante.Prod?.Nombre} - {variante.Nombre}";
                }
            }
            else if (IdPresProd.HasValue)
            {
                _presProdRepo ??= _unitOfWork!.GetCustomRepository<IPresProdRepository>();
                var presResult = _presProdRepo.GetById(IdPresProd.Value);
                if (presResult.IsSuccess && presResult.Value != null)
                {
                    var pres = presResult.Value;
                    txtProductoInfo.Text = $"{pres.VarProd?.Prod?.Nombre} - {pres.VarProd?.Nombre}";
                }
            }
        }

        // ── Cargar presentación existente para edición ────────────────────────────
        private void CargarPresentacion(long idPresProd)
        {
            _presProdRepo ??= _unitOfWork!.GetCustomRepository<IPresProdRepository>();
            var result = _presProdRepo.GetById(idPresProd);
            if (!result.IsSuccess || result.Value == null)
            {
                NotificationManager.Error("No se pudo cargar la presentación.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _presentacionActual = result.Value;

            txtDescrip.Text = _presentacionActual.Descrip;
            cboUnidad.SelectedValue = _presentacionActual.IdUnidad;
            chkEsVenta.Checked = _presentacionActual.EsVenta;
            chkEsCompra.Checked = _presentacionActual.EsCompra;
            chkActivo.Checked = _presentacionActual.Activo;
        }

        // ── Limpiar formulario para nueva presentación ────────────────────────────
        private void LimpiarFormulario()
        {
            _presentacionActual = null;
            txtDescrip.Clear();
            if (cboUnidad.Items.Count > 0)
                cboUnidad.SelectedIndex = -1;
            chkEsVenta.Checked = true;
            chkEsCompra.Checked = true;
            chkActivo.Checked = true;
        }

        // ── Validación de datos ───────────────────────────────────────────────────
        private bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(txtDescrip.Text))
            {
                NotificationManager.Warning("La descripción es obligatoria.");
                return false;
            }

            if (cboUnidad.SelectedValue == null)
            {
                NotificationManager.Warning("Debe seleccionar una unidad.");
                return false;
            }

            return true;
        }

        // ── Verificar unicidad (IdVarProd + IdUnidad) ─────────────────────────────
        private async Task<bool> EsCombinacionUnica(long idVarProd, int idUnidad)
        {
            _presProdRepo ??= _unitOfWork!.GetCustomRepository<IPresProdRepository>();
            var result = _presProdRepo.ExistsByVarianteUnidad(idVarProd, idUnidad);
            if (result.IsFailure)
            {
                NotificationManager.Warning($"Error al verificar unicidad: {result.Error?.Message}");
                return false;
            }

            // En edición, si la combinación es la misma que la actual, es válida
            if (result.Value && _presentacionActual != null)
                return _presentacionActual.IdUnidad == idUnidad; // Misma unidad, ok
            return !result.Value;
        }

        // ── Guardar (crear o actualizar) ─────────────────────────────────────────
        private async void BtnOK_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;

            var tenantResult = _tenantContext.TryGetCurrent();
            if (tenantResult.IsFailure) return;

            int idUnidad = (int)cboUnidad.SelectedValue!;
            long idVar = IdVarProd > 0 ? IdVarProd : (_presentacionActual?.IdVarProd ?? 0);

            if (idVar == 0)
            {
                NotificationManager.Error("No se puede determinar la variante.");
                return;
            }

            // Verificar unicidad (IdVarProd + IdUnidad)
            if (!await EsCombinacionUnica(idVar, idUnidad))
            {
                NotificationManager.Warning("Ya existe una presentación con la misma unidad para esta variante.");
                return;
            }

            _presProdRepo ??= _unitOfWork!.GetCustomRepository<IPresProdRepository>();

            try
            {
                if (_presentacionActual == null)
                {
                    // Nuevo
                    var nueva = new PresProd
                    {
                        IdVarProd = idVar,
                        IdUnidad = idUnidad,
                        Descrip = txtDescrip.Text.Trim(),
                        EsVenta = chkEsVenta.Checked,
                        EsCompra = chkEsCompra.Checked,
                        Activo = chkActivo.Checked
                    };

                    var addResult = _presProdRepo.Add(nueva);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al agregar: {addResult.Error?.Message}");
                        return;
                    }
                    _presentacionActual = nueva;
                }
                else
                {
                    // Actualizar
                    _presentacionActual.Descrip = txtDescrip.Text.Trim();
                    _presentacionActual.IdUnidad = idUnidad;
                    _presentacionActual.EsVenta = chkEsVenta.Checked;
                    _presentacionActual.EsCompra = chkEsCompra.Checked;
                    _presentacionActual.Activo = chkActivo.Checked;

                    var updateResult = _presProdRepo.Update(_presentacionActual);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar: {updateResult.Error?.Message}");
                        return;
                    }
                }

                _unitOfWork!.SaveChanges();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al guardar: {ex.Message}");
            }
        }

        // ── Cancelar ─────────────────────────────────────────────────────────────
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}


