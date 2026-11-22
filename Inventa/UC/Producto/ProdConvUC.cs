using CBP.Data.Abstractions;
using CBP.MultiTenant.Abstractions;
using CBP.WinFrms.Notification;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.Infrastructure;
using Inventa.UC.Producto.Base;
using Inventa.UC.Producto.DTOs;
using Inventa.UC.Producto.Interfaces;
using Inventa.UI;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Inventa.UC.Producto;

/// <summary>
/// UserControl para gestión de factores de conversión de una variante.
/// Define equivalencias de unidades para cálculos de inventario.
/// </summary>
public partial class ProdConvUC : BaseProductoUC
{
    // ─────────────────────────────────────────────────────────────────────────
    // Campos privados
    // ─────────────────────────────────────────────────────────────────────────
    private IProductoContainer? _container;
    private Prod? _productoActual;
    private VarProd? _varianteActual;
    private ProdConv? _conversionActual;
    private bool _isLoading;
    private bool _isEditing;
    private Dictionary<string, object?>? _valoresOriginales;
    private long _idConversionGuardada;

    // ─────────────────────────────────────────────────────────────────────────
    // Repositorios (lazy)
    // ─────────────────────────────────────────────────────────────────────────
    private IProdConvRepository? _prodConvRepo;
    private IUnidadRepository? _unidadRepo;
    private IVarProdRepository? _varProdRepo;

    // ─────────────────────────────────────────────────────────────────────────
    // Constructor
    // ─────────────────────────────────────────────────────────────────────────
    public ProdConvUC()
    {
        InitializeComponent();

        // Configurar eventos
        dgvConversiones.CellClick += DgvConversiones_CellClick;
        dgvConversiones.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) IniciarEdicionConversion(); };
        txtFactor.TextChanged += TxtFactor_TextChanged;
        tsbNuevo.Click += BtnNuevo_Click;
        tsbEditar.Click += BtnEditar_Click;
        tsbEliminar.Click += BtnEliminar_Click;
        btnGuardar.Click += BtnGuardar_Click;
        btnCancelar.Click += BtnCancelar_Click;
        btnCerrarEdicion.Click += (s, e) => CancelEdit();

        ConfigurarGrilla();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IRefreshable
    // ─────────────────────────────────────────────────────────────────────────
    public override bool HasChanges => _valoresOriginales != null && HayCambios();

    public override void RefreshData()
    {
        if (_container == null || _varianteActual == null) return;
        CargarConversiones();
    }

    public override void Clear()
    {
        _isLoading = true;
        try
        {
            _varianteActual = null;
            _conversionActual = null;
            dgvConversiones.DataSource = null;
            LimpiarPanelEdicion();
            _valoresOriginales = null;
            _isEditing = false;
            ActualizarEstadoEdicion(false);
            tslInfo.Text = "Sin variante seleccionada";
        }
        finally
        {
            _isLoading = false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IEditable
    // ─────────────────────────────────────────────────────────────────────────
    public override bool IsEditing => _isEditing;

public override void BeginEdit()
     {
         if (_isEditing) return;
         _isEditing = true;
         ActualizarEstadoEdicion(true);
         base.BeginEdit();
     }

     public override void EndEdit()
     {
         if (!_isEditing) return;
         _isEditing = false;
         ActualizarEstadoEdicion(false);
         base.EndEdit();
     }

     public override void CancelEdit()
     {
         if (!_isEditing) return;
         LimpiarPanelEdicion();
         _conversionActual = null;
         _valoresOriginales = null;
         _isEditing = false;
         ActualizarEstadoEdicion(false);
         lblModoEdicion.Text = "🆕 Nueva Conversión";
         base.CancelEdit();
     }

    // ─────────────────────────────────────────────────────────────────────────
    // IValidatable
    // ─────────────────────────────────────────────────────────────────────────
    public override bool IsValid => ValidarDatos(silent: true);

    public override bool ValidarDatos(bool silent = false)
    {
        bool esValido = true;
        var sb = new System.Text.StringBuilder();

        // Validar que la unidad esté seleccionada
        if (cboUnidad.SelectedValue == null || (int)cboUnidad.SelectedValue == 0)
        {
            sb.AppendLine("• Debe seleccionar una unidad.");
            esValido = false;
        }

        // Validar que el factor sea > 0
        if (!decimal.TryParse(txtFactor.Text, out decimal factor) || factor <= 0)
        {
            SetError(txtFactor, "Factor debe ser mayor a 0");
            sb.AppendLine("• El factor de conversión debe ser mayor a 0.");
            esValido = false;
        }
        else
        {
            ClearError(txtFactor);
        }

        // Validar que no exista duplicado por unidad (excepto si se está editando)
        if (cboUnidad.SelectedValue != null && _varianteActual != null)
        {
            int idUnidad = (int)cboUnidad.SelectedValue;
            _prodConvRepo ??= _container?.UnitOfWork.GetCustomRepository<IProdConvRepository>();

            var existResult = _prodConvRepo?.Exists(_varianteActual.IdVarProd, idUnidad);
            if (existResult?.IsSuccess == true && existResult.Value)
            {
                // Si hay duplicado y no estamos editando la misma conversión, es error
                if (_conversionActual == null || _conversionActual.IdUnidad != idUnidad)
                {
                    sb.AppendLine($"• Ya existe una conversión para esta unidad.");
                    esValido = false;
                }
            }
        }

        if (!esValido && !silent)
            NotificationManager.Warning($"Por favor corrija los siguientes errores:\n\n{sb}");

        return esValido;
    }

    public override void Limpiar() => Clear();

    // ─────────────────────────────────────────────────────────────────────────
    // Métodos públicos
    // ─────────────────────────────────────────────────────────────────────────
public override void SetContainer(IProductoContainer container)
     {
         _container = container ?? throw new ArgumentNullException(nameof(container));
         base.SetContainer(container);
         _container.ProductoChanged += OnProductoChanged;
     }

    /// <summary>
    /// Inicializa los datos del control. Debe llamarse después de que el UnitOfWork esté disponible.
    /// </summary>
    public void InitializeData()
    {
        if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            return;

        CargarCombos();

        // ✅ Accessibility setup
        SetAccessible(txtFactor, "Factor de Conversión", "Factor numérico para convertir entre unidades (ej: 1000 para ml a l)");
        SetAccessible(cboUnidad, "Unidad", "Unidad destino del factor de conversión");
    }

    public void SetVarianteSeleccionada(long idVarProd)
    {
        _isLoading = true;
        try
        {
            if (_varianteActual?.IdVarProd != idVarProd)
            {
                _varianteActual = null;
                _conversionActual = null;
                CancelEdit();
            }

            if (_container != null)
            {
                _varProdRepo ??= _container.UnitOfWork.GetCustomRepository<IVarProdRepository>();
                // ✅ Use GetWithPresentaciones() instead of GetById() to load related Unidad
                var result = _varProdRepo.GetWithPresentaciones(idVarProd);
                if (result.IsSuccess && result.Value != null)
                {
                    _varianteActual = result.Value;
                    _productoActual = _container.ProductoActual;
                    CargarConversiones();
                }
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    public bool Guardar()
    {
        if (_container == null || _varianteActual == null) return false;

        if (!ValidarDatos())
            return false;

        bool esNuevo = (_conversionActual == null);
        decimal factor = decimal.Parse(txtFactor.Text);
        int idUnidad = (int)cboUnidad.SelectedValue!;

        _prodConvRepo ??= _container.UnitOfWork.GetCustomRepository<IProdConvRepository>();

        try
        {
            if (esNuevo)
            {
                var nueva = new ProdConv
                {
                    IdVarProd = _varianteActual.IdVarProd,
                    IdUnidad = idUnidad,
                    Factor = factor
                };
                var addResult = _prodConvRepo.Add(nueva);
                if (addResult.IsFailure)
                {
                    NotificationManager.Error($"Error al crear conversión: {addResult.Error?.Message}");
                    return false;
                }
                _conversionActual = nueva;
            }
            else
            {
                _conversionActual!.Factor = factor;
                _conversionActual.IdUnidad = idUnidad;

                var updateResult = _prodConvRepo.Update(_conversionActual);
                if (updateResult.IsFailure)
                {
                    NotificationManager.Error($"Error al actualizar conversión: {updateResult.Error?.Message}");
                    return false;
                }
            }

            // ✅ Store conversion ID before EndEdit clears _conversionActual
            _idConversionGuardada = _conversionActual?.IdProdConv ?? 0;

            GuardarValoresOriginales();
            EndEdit();
            return true;
        }
        catch (Exception ex)
        {
            NotificationManager.Error($"Error al guardar conversión: {ex.Message}");
            return false;
        }
    }

    public bool Eliminar()
    {
        if (_container == null || _conversionActual == null) return false;

        _prodConvRepo ??= _container.UnitOfWork.GetCustomRepository<IProdConvRepository>();

        try
        {
            var removeResult = _prodConvRepo.Remove(_conversionActual);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar conversión: {removeResult.Error?.Message}");
                return false;
            }

            _container.UnitOfWork.SaveChanges();
            NotificationManager.Success("Conversión eliminada.");
            CargarConversiones();
            _conversionActual = null;
            return true;
        }
        catch (Exception ex)
        {
            NotificationManager.Error($"Error al eliminar conversión: {ex.Message}");
            return false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Métodos privados
    // ─────────────────────────────────────────────────────────────────────────
    private void ConfigurarGrilla()
    {
        dgvConversiones.AutoGenerateColumns = false;
        dgvConversiones.Columns.Clear();

        dgvConversiones.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "IdProdConv",
            HeaderText = "ID",
            DataPropertyName = nameof(ConversionItem.IdProdConv),
            Width = 50,
            Visible = false
        });
        dgvConversiones.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "NombreUnidad",
            HeaderText = "Unidad",
            DataPropertyName = nameof(ConversionItem.NombreUnidad),
            Width = 100
        });
        dgvConversiones.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Factor",
            HeaderText = "Factor",
            DataPropertyName = nameof(ConversionItem.Factor),
            Width = 100
        });
        dgvConversiones.ApplyModernStyle();
    }

    private void CargarCombos()
    {
        if (_container == null) return;

        try
        {
            _unidadRepo ??= _container.UnitOfWork.GetCustomRepository<IUnidadRepository>();
            var unidadesResult = _unidadRepo.GetAll();
            if (unidadesResult.IsSuccess)
            {
                var unidades = unidadesResult.Value.OrderBy(u => u.Nombre).ToList();
                cboUnidad.DataSource = unidades;
                cboUnidad.DisplayMember = nameof(Unidad.Nombre);
                cboUnidad.ValueMember = nameof(Unidad.IdUnidad);
            }
        }
        catch (Exception ex)
        {
            NotificationManager.Error($"Error al cargar unidades: {ex.Message}");
        }
    }

    private void CargarConversiones()
    {
        if (_container == null || _varianteActual == null) return;

        _prodConvRepo ??= _container.UnitOfWork.GetCustomRepository<IProdConvRepository>();

        var result = _prodConvRepo.GetByVariante(_varianteActual.IdVarProd);
        if (!result.IsSuccess)
        {
            NotificationManager.Error($"Error al cargar conversiones: {result.Error?.Message}");
            return;
        }

        var conversiones = result.Value.ToList();
        var lista = conversiones.Select(c => new ConversionItem(
            c.IdProdConv,
            c.IdVarProd,
            c.Unidad?.Nombre ?? "?",
            c.Factor
        )).ToList();

        dgvConversiones.DataSource = lista;
        // ✅ PATRÓN: Mostrar cantidad de registros en tslInfo
        tslInfo.Text = $"{lista.Count} conversión(es)";
    }

    private void LimpiarPanelEdicion()
    {
        txtFactor.Clear();
        ClearAllErrors();
        cboUnidad.SelectedIndex = -1;
    }

    private void GuardarValoresOriginales()
    {
        // ✅ FIX: Always save original values, even for new conversions
        _valoresOriginales = new Dictionary<string, object?>
        {
            [nameof(txtFactor)] = txtFactor.Text,
            [nameof(cboUnidad)] = cboUnidad.SelectedValue
        };
    }

    private bool HayCambios()
    {
        if (_valoresOriginales == null) return false;
        try
        {
            return !NullSafeEquals(_valoresOriginales.TryGetValue(nameof(txtFactor), out var factor) ? factor : null, txtFactor.Text) ||
                   !NullSafeEquals(_valoresOriginales.TryGetValue(nameof(cboUnidad), out var unidad) ? unidad : null, cboUnidad.SelectedValue);
        }
        catch { return true; }
    }

    private static bool NullSafeEquals(object? a, object? b)
    {
        if (a is DBNull) a = null;
        if (b is DBNull) b = null;
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.Equals(b);
    }

    private void ActualizarEstadoEdicion(bool enEdicion)
    {
        pnlEdicion.Visible = enEdicion;
        dgvConversiones.Enabled = !enEdicion;
        tsbNuevo.Enabled = !enEdicion;
        tsbEditar.Enabled = !enEdicion && _conversionActual != null;
        tsbEliminar.Enabled = !enEdicion && _conversionActual != null;

        if (enEdicion)
        {
            txtFactor.Focus();
        }
    }

    private long? GetSelectedConversionId()
    {
        if (dgvConversiones.SelectedRows.Count == 0) return null;
        var item = dgvConversiones.SelectedRows[0].DataBoundItem as ConversionItem;
        return item?.IdProdConv;
    }

    private void IniciarEdicionConversion()
    {
        var idConversion = GetSelectedConversionId();
        if (!idConversion.HasValue) return;

        CargarConversionParaEdicion(idConversion.Value);
    }

    private void CargarConversionParaEdicion(long idProdConv)
    {
        _prodConvRepo ??= _container!.UnitOfWork.GetCustomRepository<IProdConvRepository>();
        var result = _prodConvRepo.GetById(idProdConv);
        if (!result.IsSuccess || result.Value == null) return;

        _conversionActual = result.Value;
        txtFactor.Text = _conversionActual.Factor.ToString();
        cboUnidad.SelectedValue = _conversionActual.IdUnidad;
        GuardarValoresOriginales();
        lblModoEdicion.Text = $"✏️ Editar Conversión - {_conversionActual.Unidad?.Nombre}";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Event handlers
    // ─────────────────────────────────────────────────────────────────────────
    private void DgvConversiones_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (_isLoading || e.RowIndex < 0) return;

        try
        {
            var row = dgvConversiones.Rows[e.RowIndex];
            var conversionItem = row.DataBoundItem as ConversionItem;
            var idConversion = conversionItem?.IdProdConv;

            if (idConversion.HasValue)
            {
                _conversionActual = null;
                CargarConversionParaEdicion(idConversion.Value);
                tsbEditar.Enabled = !_isEditing;
                tsbEliminar.Enabled = !_isEditing;

                // ✅ PATRÓN: Actualizar tslInfo con datos del registro seleccionado
                if (_conversionActual != null)
                {
                    // ✅ Usar la información del DTO que ya está en memoria si Unidad es nula
                    var unidad = _conversionActual.Unidad?.Nombre ?? conversionItem?.NombreUnidad ?? "---";
                    var factor = _conversionActual.Factor;
                    tslInfo.Text = $"Unidad: {unidad} | Factor: {factor}";
                }

                row.Selected = true;  // Seleccionar fila
            }
            else
            {
                tsbEditar.Enabled = false;
                tsbEliminar.Enabled = false;
                tslInfo.Text = $"{dgvConversiones.Rows.Count} conversión(es)";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en DgvConversiones_CellClick: {ex.Message}");
            tslInfo.Text = $"{dgvConversiones.Rows.Count} conversión(es)";
        }
    }

    private void TxtFactor_TextChanged(object? sender, EventArgs e)
    {
        // Para capturar cambios durante edición
        if (_isEditing)
            ValidarDatos(silent: true);
    }

    private void BtnNuevo_Click(object? sender, EventArgs e)
    {
        if (_isEditing || _varianteActual == null) return;

        _conversionActual = null;
        LimpiarPanelEdicion();
        // ✅ FIX: Initialize _valoresOriginales for new conversion so HasChanges works correctly
        _valoresOriginales = new Dictionary<string, object?>
        {
            [nameof(txtFactor)] = "",
            [nameof(cboUnidad)] = DBNull.Value
        };
        BeginEdit();
        lblModoEdicion.Text = "🆕 Nueva Conversión";
        pnlEdicion.Visible = true;
        cboUnidad.Focus();
    }

    private void BtnEditar_Click(object? sender, EventArgs e)
    {
        if (_isEditing || _conversionActual == null) return;

        BeginEdit();
        lblModoEdicion.Text = "✏️ Editar Conversión";
        pnlEdicion.Visible = true;
    }

    private void BtnEliminar_Click(object? sender, EventArgs e)
    {
        if (_isEditing || _conversionActual == null) return;

        var unidadNombre = _conversionActual.Unidad?.Nombre ?? "desconocida";
        if (MessageBox.Show($"¿Eliminar la conversión para {unidadNombre}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        Eliminar();
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        try
        {
            var resultado = Guardar();
            if (resultado)
            {
                // ✅ FIX: Really save to DB, not just in memory
                var saved = _container?.UnitOfWork.SaveChanges();
                if (saved > 0)
                {
                    NotificationManager.Success("✅ Conversión guardada correctamente.");

                    // ✅ Reload grid from DB to reflect changes
                    CargarConversiones();

                    // ✅ Restore selection after grid reload to trigger SelectionChanged
                    if (_idConversionGuardada > 0)
                    {
                        RestaurarSeleccionConversion(_idConversionGuardada);
                    }
                }
                else
                {
                    NotificationManager.Warning("⚠️ No se detectaron cambios para guardar.");
                }
            }
        }
        catch (Exception ex)
        {
            NotificationManager.Error($"❌ Error al guardar: {ex.Message}");
        }
    }

    private void RestaurarSeleccionConversion(long idConversion)
    {
        // Buscar y seleccionar la fila con la conversión guardada
        for (int i = 0; i < dgvConversiones.Rows.Count; i++)
        {
            var item = dgvConversiones.Rows[i].DataBoundItem as ConversionItem;
            if (item?.IdProdConv == idConversion)
            {
                dgvConversiones.ClearSelection();
                dgvConversiones.Rows[i].Selected = true;
                dgvConversiones.FirstDisplayedScrollingRowIndex = i;
break;
             }
         }
     }

     private void BtnCancelar_Click(object? sender, EventArgs e)
    {
        CancelEdit();
    }

    private void OnProductoChanged(object? sender, ProductoChangedEventArgs e)
    {
        // Se dispara cuando cambia el producto en el contenedor
_productoActual = _container?.ProductoActual;
     }

private void pnlVarianteHeader_Paint(object sender, PaintEventArgs e)
     {

     }
 }
