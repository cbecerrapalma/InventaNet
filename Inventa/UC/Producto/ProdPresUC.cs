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
using System.ComponentModel;

namespace Inventa.UC.Producto;

/// <summary>
/// UserControl para gestión de presentaciones de una variante.
/// Presenta equivalencias de unidades para compra y venta.
/// </summary>
public partial class ProdPresUC : BaseProductoUC
{
    // ─────────────────────────────────────────────────────────────────────────
    // Campos privados
    // ─────────────────────────────────────────────────────────────────────────
    private IProductoContainer? _container;
    private Prod? _productoActual;
    private VarProd? _varianteActual;
    private PresProd? _presentacionActual;
    private bool _isLoading;
    private bool _isEditing;
    private Dictionary<string, object?>? _valoresOriginales;
    private long _idPresentacionGuardada; // ✅ Store saved presentation ID to restore selection

    // ─────────────────────────────────────────────────────────────────────────
    // Repositorios (lazy)
    // ─────────────────────────────────────────────────────────────────────────
    private IPresProdRepository? _presProdRepo;
    private IUnidadRepository? _unidadRepo;
    private IVarProdRepository? _varProdRepo;

    // ─────────────────────────────────────────────────────────────────────────
    // Evento público para notificar selección de presentación
    // ─────────────────────────────────────────────────────────────────────────
    public event EventHandler<long>? PresentacionSeleccionada;

    // ─────────────────────────────────────────────────────────────────────────
    // Constructor
    // ─────────────────────────────────────────────────────────────────────────
    public ProdPresUC()
    {
        InitializeComponent();
        AutoScroll = true;

        // Configurar eventos
        dgvPresentaciones.CellClick += DgvPresentaciones_CellClick;
        dgvPresentaciones.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) IniciarEdicionPresentacion(); };
        txtDescripcion.TextChanged += TxtDescripcion_TextChanged;
        tsbNuevo.Click += BtnNuevo_Click;
        tsbEditar.Click += BtnEditar_Click;
        tsbEliminar.Click += BtnEliminar_Click;
        btnGuardar.Click += BtnGuardar_Click;
        btnCancelar.Click += BtnCancelar_Click;
        btnCerrarEdicion.Click += (s, e) => CancelEdit();
        btnAgregarUnidad.Click += BtnAgregarUnidad_Click;

        ConfigurarGrilla();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IRefreshable
    // ─────────────────────────────────────────────────────────────────────────
    public override bool HasChanges => _valoresOriginales != null && HayCambios();

    public override void RefreshData()
    {
        if (_container == null || _varianteActual == null) return;
        CargarPresentaciones();
    }

    public override void Clear()
    {
        _isLoading = true;
        try
        {
            _varianteActual = null;
            _presentacionActual = null;
            dgvPresentaciones.DataSource = null;
            LimpiarPanelEdicion();
            _valoresOriginales = null;
            _isEditing = false;
            ActualizarEstadoEdicion(false);
            tslPresentacionSeleccionada.Text = "Sin presentación seleccionada";
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
         _presentacionActual = null;
         _valoresOriginales = null;
         _isEditing = false;
         ActualizarEstadoEdicion(false);
         lblModoEdicion.Text = "🆕 Nueva Presentación";
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

        if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
        {
            SetError(txtDescripcion, "Descripción obligatoria");
            sb.AppendLine("• La descripción es obligatoria.");
            esValido = false;
        }
        else
        {
            ClearError(txtDescripcion);
        }

        if (cboUnidad.SelectedValue == null || (int)cboUnidad.SelectedValue == 0)
        {
            sb.AppendLine("• Debe seleccionar una unidad.");
            esValido = false;
        }

        if (!chkEsVenta.Checked && !chkEsCompra.Checked)
        {
            sb.AppendLine("• Debe marcar si es para Venta o Compra.");
            esValido = false;
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
        SetAccessible(txtDescripcion, "Descripción", "Descripción de la presentación del producto");
        SetAccessible(cboUnidad, "Unidad", "Unidad de medida de la presentación");
        SetAccessible(chkEsVenta, "Es Venta", "Indica si esta presentación se utiliza para ventas");
        SetAccessible(chkEsCompra, "Es Compra", "Indica si esta presentación se utiliza para compras");
        SetAccessible(chkActivo, "Activo", "Indica si esta presentación está activa");
    }

    public void SetVarianteSeleccionada(long idVarProd)
    {
        _isLoading = true;
        try
        {
            if (_varianteActual?.IdVarProd != idVarProd)
            {
                _varianteActual = null;
                _presentacionActual = null;
                CancelEdit();
            }

            if (_container != null)
            {
                _varProdRepo ??= _container.UnitOfWork.GetCustomRepository<IVarProdRepository>();
                var result = _varProdRepo.GetById(idVarProd);
                if (result.IsSuccess && result.Value != null)
                {
                    _varianteActual = result.Value;
                    _productoActual = _container.ProductoActual;
                    CargarPresentaciones();
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

        bool esNuevo = (_presentacionActual == null);

        _presProdRepo ??= _container.UnitOfWork.GetCustomRepository<IPresProdRepository>();

        try
        {
            if (esNuevo)
            {
                var nueva = new PresProd
                {
                    IdVarProd = _varianteActual.IdVarProd,
                    Descrip = txtDescripcion.Text.Trim(),
                    IdUnidad = (int)cboUnidad.SelectedValue!,
                    EsVenta = chkEsVenta.Checked,
                    EsCompra = chkEsCompra.Checked,
                    Activo = chkActivo.Checked
                };
                var addResult = _presProdRepo.Add(nueva);
                if (addResult.IsFailure)
                {
                    NotificationManager.Error($"Error al crear presentación: {addResult.Error?.Message}");
                    return false;
                }
                _presentacionActual = nueva;
            }
            else
            {
                _presentacionActual!.Descrip = txtDescripcion.Text.Trim();
                _presentacionActual.IdUnidad = (int)cboUnidad.SelectedValue!;
                _presentacionActual.EsVenta = chkEsVenta.Checked;
                _presentacionActual.EsCompra = chkEsCompra.Checked;
                _presentacionActual.Activo = chkActivo.Checked;

                var updateResult = _presProdRepo.Update(_presentacionActual);
                if (updateResult.IsFailure)
                {
                    NotificationManager.Error($"Error al actualizar presentación: {updateResult.Error?.Message}");
                    return false;
                }
            }

            // ✅ Store presentation ID before EndEdit clears _presentacionActual
            _idPresentacionGuardada = _presentacionActual?.IdPresProd ?? 0;

            GuardarValoresOriginales();
            EndEdit();
            // ✅ FIX Bug #1: Don't reload grid here - let caller (BtnGuardar_Click) reload after SaveChanges()
            return true;
        }
        catch (Exception ex)
        {
            NotificationManager.Error($"Error al guardar presentación: {ex.Message}");
            return false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Métodos privados
    // ─────────────────────────────────────────────────────────────────────────
    private void ConfigurarGrilla()
    {
        dgvPresentaciones.AutoGenerateColumns = false;
        dgvPresentaciones.Columns.Clear();

        dgvPresentaciones.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "IdPresProd",
            HeaderText = "ID",
            DataPropertyName = nameof(PresentacionItem.IdPresProd),
            Width = 50,
            Visible = false
        });
        dgvPresentaciones.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Descripcion",
            HeaderText = "Descripción",
            DataPropertyName = nameof(PresentacionItem.Descripcion),
            Width = 170  // ✅ Increased from 150 to better accommodate varchar(100)
        });
        dgvPresentaciones.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Unidad",
            HeaderText = "Unidad",
            DataPropertyName = nameof(PresentacionItem.Unidad),
            Width = 80
        });
        dgvPresentaciones.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "EsVenta",
            HeaderText = "Venta",
            DataPropertyName = nameof(PresentacionItem.EsVenta),
            Width = 50
        });
        dgvPresentaciones.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "EsCompra",
            HeaderText = "Compra",
            DataPropertyName = nameof(PresentacionItem.EsCompra),
            Width = 50
        });
        dgvPresentaciones.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "Activo",
            HeaderText = "Activo",
            DataPropertyName = nameof(PresentacionItem.Activo),
            Width = 60
        });

        dgvPresentaciones.ApplyModernStyle();
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
            NotificationManager.Error($"Error al cargar combos: {ex.Message}");
        }
    }

    private void CargarPresentaciones()
    {
        if (_container == null || _varianteActual == null) return;

        _presProdRepo ??= _container.UnitOfWork.GetCustomRepository<IPresProdRepository>();

        var result = _presProdRepo.GetByVariante(_varianteActual.IdVarProd);
        if (!result.IsSuccess)
        {
            NotificationManager.Error($"Error al cargar presentaciones: {result.Error?.Message}");
            return;
        }

        var presentaciones = result.Value.ToList();
        var lista = presentaciones.Select(p => new PresentacionItem(
            p.IdPresProd,
            p.IdVarProd,
            p.Descrip,
            p.Unidad?.Nombre ?? "?",
            p.EsVenta,
            p.EsCompra,
            p.Activo
        )).ToList();

        dgvPresentaciones.DataSource = lista;
        // ✅ PATRÓN: Mostrar cantidad de registros en tslPresentacionSeleccionada
        tslPresentacionSeleccionada.Text = $"{lista.Count} presentación(es)";
    }

    private void LimpiarPanelEdicion()
    {
        txtDescripcion.Clear();
        ClearAllErrors();
        cboUnidad.SelectedIndex = -1;
        chkEsVenta.Checked = true;
        chkEsCompra.Checked = false;
        chkActivo.Checked = true;
        lblUniBaseValor.Text = "-";
        lblEquivalenciaValor.Text = "-";
    }

    private void GuardarValoresOriginales()
    {
        // ✅ FIX: Always save original values, even for new presentations
        _valoresOriginales = new Dictionary<string, object?>
        {
            ["Descripcion"] = txtDescripcion.Text,
            ["IdUnidad"] = cboUnidad.SelectedValue,
            ["EsVenta"] = chkEsVenta.Checked,
            ["EsCompra"] = chkEsCompra.Checked,
            ["Activo"] = chkActivo.Checked
        };
    }

    private bool HayCambios()
    {
        if (_valoresOriginales == null) return false;
        try
        {
            return !NullSafeEquals(_valoresOriginales["Descripcion"], txtDescripcion.Text) ||
                   !NullSafeEquals(_valoresOriginales["IdUnidad"], cboUnidad.SelectedValue) ||
                   !NullSafeEquals(_valoresOriginales["EsVenta"], chkEsVenta.Checked) ||
                   !NullSafeEquals(_valoresOriginales["EsCompra"], chkEsCompra.Checked) ||
                   !NullSafeEquals(_valoresOriginales["Activo"], chkActivo.Checked);
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

    private void ActualizarEstadoEdicion(bool habilitado)
    {
        tsbNuevo.Enabled = !habilitado;
        tsbEditar.Enabled = !habilitado && _presentacionActual != null;
        tsbEliminar.Enabled = !habilitado && _presentacionActual != null;
        pnlEdicion.Visible = habilitado;
        if (habilitado)
        {
            txtDescripcion.Focus();
        }
    }

    private long? GetSelectedPresentacionId()
    {
        if (dgvPresentaciones.SelectedRows.Count == 0) return null;
        var item = dgvPresentaciones.SelectedRows[0].DataBoundItem as PresentacionItem;
        return item?.IdPresProd;
    }

    private void CargarPresentacionParaEdicion(long idPresProd)
    {
        _presProdRepo ??= _container!.UnitOfWork.GetCustomRepository<IPresProdRepository>();
        var result = _presProdRepo.GetById(idPresProd);
        if (!result.IsSuccess || result.Value == null) return;

        _presentacionActual = result.Value;
        txtDescripcion.Text = _presentacionActual.Descrip;
        cboUnidad.SelectedValue = _presentacionActual.IdUnidad;
        chkEsVenta.Checked = _presentacionActual.EsVenta;
        chkEsCompra.Checked = _presentacionActual.EsCompra;
        chkActivo.Checked = _presentacionActual.Activo;

        ActualizarEquivalencia();
        GuardarValoresOriginales();
    }

    private void ActualizarEquivalencia()
    {
        if (_productoActual == null || cboUnidad.SelectedValue == null)
        {
            lblEquivalenciaValor.Text = "-";
            return;
        }

        var uniSeleccionada = (int)cboUnidad.SelectedValue;
        lblUniBaseValor.Text = "Unidad base";

        lblEquivalenciaValor.Text = $"1 {(cboUnidad.SelectedItem as Unidad)?.Nombre ?? "?"} de esta presentación";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Eventos
    // ─────────────────────────────────────────────────────────────────────────
    private void DgvPresentaciones_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (_isLoading || e.RowIndex < 0) return;

        try
        {
            var row = dgvPresentaciones.Rows[e.RowIndex];
            if (row.DataBoundItem is PresentacionItem item)
            {
                var id = item.IdPresProd;
                _presentacionActual = null;
                CargarPresentacionParaEdicion(id);
                tsbEditar.Enabled = !_isEditing;
                tsbEliminar.Enabled = !_isEditing;

                // ✅ PATRÓN: Actualizar tslPresentacionSeleccionada con datos del registro seleccionado
                if (_presentacionActual != null)
                {
                    // ✅ Usar la información del DTO que ya está en memoria si Unidad es nula
                    var unidad = _presentacionActual.Unidad?.Nombre ?? item.Unidad ?? "---";
                    var uso = new List<string>();
                    if (_presentacionActual.EsVenta) uso.Add("Venta");
                    if (_presentacionActual.EsCompra) uso.Add("Compra");
                    var usoStr = uso.Count > 0 ? string.Join(", ", uso) : "Sin uso";

                    tslPresentacionSeleccionada.Text = $"{_presentacionActual.Descrip} | {unidad} | {usoStr}";
                }

                // ✅ Notificar selección de presentación
                PresentacionSeleccionada?.Invoke(this, id);
                row.Selected = true;  // Seleccionar fila
            }
            else
            {
                tsbEditar.Enabled = false;
                tsbEliminar.Enabled = false;
                tslPresentacionSeleccionada.Text = $"{dgvPresentaciones.Rows.Count} presentación(es)";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en DgvPresentaciones_CellClick: {ex.Message}");
            tslPresentacionSeleccionada.Text = $"{dgvPresentaciones.Rows.Count} presentación(es)";
        }
    }

    private void BtnNuevo_Click(object sender, EventArgs e)
    {
        if (_isEditing || _varianteActual == null) return;
        _presentacionActual = null;
        LimpiarPanelEdicion();
        // ✅ FIX: Initialize _valoresOriginales for new presentation so HasChanges works correctly
        _valoresOriginales = new Dictionary<string, object?>
        {
            ["Descripcion"] = "",
            ["IdUnidad"] = DBNull.Value,
            ["EsVenta"] = false,
            ["EsCompra"] = false,
            ["Activo"] = true
        };
        BeginEdit();
        lblModoEdicion.Text = "🆕 Nueva Presentación";
        pnlEdicion.Visible = true;
        txtDescripcion.Focus();
    }

    private void TxtDescripcion_TextChanged(object sender, EventArgs e)
    {
        if (_isEditing && !_isLoading)
        {
            ValidationDebouncer.StartDebounce(() =>
            {
                var descripcion = txtDescripcion.Text.Trim();
                if (string.IsNullOrWhiteSpace(descripcion))
                    SetError(txtDescripcion, "Descripción obligatoria");
                else
                    ClearError(txtDescripcion);
            }, 300);
        }
    }

    private void BtnEditar_Click(object sender, EventArgs e)
    {
        if (_isEditing || _presentacionActual == null) return;
        BeginEdit();
        lblModoEdicion.Text = "✏️ Editar Presentación";
        pnlEdicion.Visible = true;
    }

    private void BtnEliminar_Click(object sender, EventArgs e)
    {
        if (_isEditing || _presentacionActual == null) return;
        if (MessageBox.Show($"¿Eliminar la presentación {_presentacionActual.Descrip}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        _presProdRepo ??= _container.UnitOfWork.GetCustomRepository<IPresProdRepository>();

        var removeResult = _presProdRepo.Remove(_presentacionActual);
        if (removeResult.IsFailure)
        {
            NotificationManager.Error($"Error al eliminar presentación: {removeResult.Error?.Message}");
            return;
        }

        try
        {
            _container.UnitOfWork.SaveChanges();
            NotificationManager.Success("Presentación eliminada.");
            CargarPresentaciones();
            _presentacionActual = null;
        }
        catch (Exception ex)
        {
            NotificationManager.Error($"Error al eliminar presentación: {ex.Message}");
        }
    }

    private void BtnGuardar_Click(object sender, EventArgs e)
    {
        try
        {
            var resultado = Guardar();
            if (resultado)
            {
                System.Diagnostics.Debug.WriteLine("➡️ BtnGuardar_Click: Guardar() exitoso, llamando a SaveChanges()");
                // ✅ FIX: Realmente guardar en la BD, no solo en memoria
                var saved = _container?.UnitOfWork.SaveChanges();
                if (saved > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ BtnGuardar_Click: SaveChanges() exitoso, {saved} registros guardados");

                    // ✅ FIX Bug #1: Reload grid AFTER database persistence
                    CargarPresentaciones();

                    NotificationManager.Success("✅ Presentación guardada correctamente.");

                    // ✅ Restore selection after grid reload to trigger SelectionChanged
                    if (_idPresentacionGuardada > 0)
                    {
                        RestaurarSeleccionPresentacion(_idPresentacionGuardada);
                    }
                }
                else
                {
                    NotificationManager.Warning("⚠️ No se detectaron cambios para guardar.");
                    System.Diagnostics.Debug.WriteLine("⚠️ BtnGuardar_Click: SaveChanges() retornó 0");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ BtnGuardar_Click: Guardar() retornó false");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ EXCEPCIÓN en BtnGuardar_Click: {ex.Message}\n{ex.StackTrace}");
            NotificationManager.Error($"❌ Error al guardar: {ex.Message}");
        }
    }

    private void BtnCancelar_Click(object sender, EventArgs e)
    {
        CancelEdit();
    }

    private void BtnAgregarUnidad_Click(object sender, EventArgs e)
    {
        if (_container == null) return;
        // Nota: frm_Unidad podría no existir. Este es un placeholder para futuro.
        NotificationManager.Info("Agregar unidad no está disponible en esta versión.");
    }

    private void IniciarEdicionPresentacion()
    {
        if (_presentacionActual == null) return;
        BtnEditar_Click(null, EventArgs.Empty);
    }

    /// <summary>
    /// ✅ Restore grid selection after reload to ensure SelectionChanged event fires.
    /// This is critical because EndEdit() clears _presentacionActual, and the new grid items
    /// won't trigger SelectionChanged unless we explicitly select a row.
    /// </summary>
    private void RestaurarSeleccionPresentacion(long idPresentacion)
    {
        if (idPresentacion == 0 || dgvPresentaciones.Rows.Count == 0) return;

        try
        {
            _isLoading = true; // Prevent SelectionChanged from firing prematurely
            foreach (DataGridViewRow row in dgvPresentaciones.Rows)
            {
                if (row.DataBoundItem is PresentacionItem item && item.IdPresProd == idPresentacion)
                {
                    dgvPresentaciones.ClearSelection(); // Clear previous selection
                    row.Selected = true;               // Select the row (triggers SelectionChanged)
                    dgvPresentaciones.FirstDisplayedScrollingRowIndex = row.Index;
                    System.Diagnostics.Debug.WriteLine($"✅ RestaurarSeleccionPresentacion: Selected row {row.Index} with presentation ID {idPresentacion}");
                    return;
                }
            }
            System.Diagnostics.Debug.WriteLine($"⚠️ RestaurarSeleccionPresentacion: Presentation ID {idPresentacion} not found in grid");
        }
        finally
        {
            _isLoading = false; // Re-enable SelectionChanged handling
        }
    }

    private void OnProductoChanged(object? sender, ProductoChangedEventArgs e)
    {
        if (_isLoading) return;

        // ✅ FIX Bug #2: Always clear old variant context when product changes
        // This prevents stale variant data from being used in the new product context
        if (_varianteActual != null)
        {
            Clear();
            _varianteActual = null;
        }

        _productoActual = e.ProductoNuevo;

        // Let ProdVarUC (master) drive variant selection
        // Grid will remain empty until user selects a variant in the variant control
    }
}

