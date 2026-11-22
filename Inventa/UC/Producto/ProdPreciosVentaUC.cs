using CBP.Data.Abstractions;
using CBP.WinFrms.Notification;
using Inventa.Application.Services;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.Helpers;
using Inventa.UC.Producto.Base;
using Inventa.UC.Producto.Interfaces;
using System.ComponentModel;

namespace Inventa.UC.Producto;

/// <summary>
/// UserControl para la gestión de precios de venta por presentación.
/// </summary>
public partial class ProdPreciosVentaUC : BaseProductoUC, IRefreshable, IEditable, IValidatable
{
    private IProductoContainer? _container;
    private Prod? _productoActual;
    private bool _isEditing;
    private bool _isLoading;

    private IPrecioRepository? _precioRepo;
    private IPresProdRepository? _presProdRepo;
    private long? _presentacionSeleccionada;
    private Precio? _precioActual;

    protected override ZonaContenedor Zona => ZonaContenedor.Detalle;

    public override bool HasChanges => false;
    public override bool IsEditing => _isEditing;
    public override bool IsValid => ValidarDatos(silent: true);

    public ProdPreciosVentaUC()
    {
        InitializeComponent();
        cboPresentacion.SelectedIndexChanged += CboPresentacion_SelectedIndexChanged;
        // S57.69: sin filtro/selector por tipo (precio normal único; mayorista
        // vive en superficie dedicada). Controles ocultos, Designer intacto.
        cboTipoPrecio.Visible = false;
        lblTipoPrecio.Visible = false;
        cboTipoPrecioEdicion.Visible = false;
        lblTipoPrecioEdicion.Visible = false;
        dgvPrecios.CellClick += DgvPrecios_CellClick;
        dgvPrecios.CellDoubleClick += DgvPrecios_CellDoubleClick;
        dgvPrecios.SelectionChanged += DgvPrecios_SelectionChanged;
    }

    public override void SetContainer(IProductoContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        base.SetContainer(container);
        _container.ProductoChanged += OnProductoChanged;
    }

    private void OnProductoChanged(object? sender, ProductoChangedEventArgs e)
    {
        RefreshData();
    }

    public void InitializeData()
    {
    }

    public override void RefreshData()
    {
        if (_container?.ProductoActual == null)
            Clear();
        else
        {
            _productoActual = _container.ProductoActual;
            CargarPresentacionesCombo();
            CargarGrilla();
        }
    }

    public override void Clear()
    {
        _productoActual = null;
        if (dgvPrecios != null)
            dgvPrecios.DataSource = null;
        LimpiarCombos();
    }

    public override void BeginEdit()
    {
        if (_isEditing) return;
        base.BeginEdit();
        _isEditing = true;
        ActualizarEstadoBotones();
    }

    public override void EndEdit()
    {
        if (!_isEditing) return;
        base.EndEdit();
        _isEditing = false;
        ActualizarEstadoBotones();
    }

    public override void CancelEdit()
    {
        if (!_isEditing) return;
        base.CancelEdit();
        _isEditing = false;
    }

    public override bool ValidarDatos(bool silent = false) => true;

    public bool Guardar() => true;

    /// <summary>
    /// Establece la presentación seleccionada. Este es el dato maestro del UC.
    /// </summary>
    public void SetPresentacionSeleccionada(long idPresProd)
    {
        if (idPresProd <= 0)
            return;

        _presentacionSeleccionada = idPresProd;
        RefreshData();
    }

    /// <summary>
    /// Método para compatibilidad con SetVarianteSeleccionada (no se usa directamente aquí).
    /// Simplemente limpia la presentación seleccionada.
    /// </summary>
    public void SetVarianteSeleccionada(long idVarProd)
    {
        // Al cambiar variante, limpiar presentación seleccionada
        _presentacionSeleccionada = null;
        Clear();
    }

    private void LimpiarCombos()
    {
        _isLoading = true;
        try
        {
            cboPresentacion.DataSource = null;
            cboPresentacion.Items.Clear();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void CargarPresentacionesCombo()
    {
        // Si no hay presentación seleccionada, el UC está vacío
        if (_presentacionSeleccionada == null)
        {
            cboPresentacion.Enabled = false;
            return;
        }

        // Si hay presentación seleccionada, mostrarla en el combo
        _isLoading = true;
        try
        {
            _presProdRepo ??= _container?.UnitOfWork.GetCustomRepository<IPresProdRepository>();
            if (_presProdRepo == null)
                return;

            var presResult = _presProdRepo.GetById(_presentacionSeleccionada.Value);
            if (!presResult.IsSuccess || presResult.Value == null)
            {
                cboPresentacion.Enabled = false;
                return;
            }

            var pres = presResult.Value;
            var items = new List<PresentacionComboItem>
            {
                new PresentacionComboItem(pres.IdPresProd, $"{pres.Descrip}")
            };

            cboPresentacion.DataSource = items;
            cboPresentacion.DisplayMember = nameof(PresentacionComboItem.Descripcion);
            cboPresentacion.ValueMember = nameof(PresentacionComboItem.IdPresProd);
            cboPresentacion.SelectedIndex = 0;
            cboPresentacion.Enabled = false;  // Solo lectura - no es filtro, es identificador
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en CargarPresentacionesCombo: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void CargarGrilla()
    {
        // El UC requiere una presentación seleccionada
        if (_presentacionSeleccionada == null)
        {
            dgvPrecios.DataSource = null;
            ActualizarEstadoBotones();
            ActualizarResumen();
            return;
        }

        _isLoading = true;
        try
        {
            _precioRepo ??= _container?.UnitOfWork.GetCustomRepository<IPrecioRepository>();
            var precResult = _precioRepo.GetByPresentacion(_presentacionSeleccionada.Value);

            if (!precResult.IsSuccess || precResult.Value == null || !precResult.Value.Any())
            {
                dgvPrecios.DataSource = null;
                tslInfo.Text = "0 precio(s)";
                ActualizarEstadoBotones();
                ActualizarResumen();
                return;
            }

            var items = new List<dynamic>();

            foreach (var precio in precResult.Value)
            {
                // S57.69: grilla = precios normales (post-DROP todo es normal).
                // Obtener nombre de presentación
                _presProdRepo ??= _container?.UnitOfWork.GetCustomRepository<IPresProdRepository>();
                var presResult = _presProdRepo.GetById(_presentacionSeleccionada.Value);
                var presNombre = presResult.IsSuccess ? presResult.Value?.Descrip : "Desconocida";

                items.Add(new
                {
                    IdPrecio = precio.IdPrecio,  // ✅ Agregar ID para poder editar
                    Presentacion = presNombre,
                    IdPresProd = _presentacionSeleccionada,
                    MontoVenta = precio.Valor.ToString("C"),
                    FechaVigencia = precio.FecVig.ToShortDateString(),
                    Activo = precio.Activo ? "Sí" : "No"
                });
            }

            if (dgvPrecios != null)
                dgvPrecios.DataSource = new BindingList<dynamic>(items);

            // ✅ Ocultar columnas ID
            OcultarColumnasID(dgvPrecios);

            // ✅ PATRÓN: Mostrar cantidad de registros en tslInfo
            tslInfo.Text = $"{items.Count} precio(s)";

            // ✅ Actualizar estado de botones después de cargar
            ActualizarEstadoBotones();

            // ✅ Actualizar panel de resumen
            ActualizarResumen();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en CargarGrilla: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Actualiza el estado de los botones Editar y Eliminar basado en la selección en la grilla.
    /// </summary>
    private void ActualizarEstadoBotones()
    {
        bool haySeleccion = (dgvPrecios.SelectedRows.Count > 0) && !_isEditing;
        tsbEditar.Enabled = haySeleccion;
        tsbEliminar.Enabled = haySeleccion;
    }

    /// <summary>
    /// ✅ PATRÓN: Usar CellClick para actualizar tslInfo incluso con 1 solo registro
    /// </summary>
    private void DgvPrecios_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (_isLoading || e.RowIndex < 0) return;

        try
        {
            var row = dgvPrecios.Rows[e.RowIndex];
            if (row.DataBoundItem != null)
            {
                dynamic item = row.DataBoundItem;
                var monto = item.MontoVenta ?? "---";
                var vigencia = $"{item.FechaVigencia}";

                tslInfo.Text = $"{monto} | Vigencia: {vigencia}";
                row.Selected = true;  // Seleccionar fila
            }
            else
            {
                tslInfo.Text = $"{dgvPrecios.Rows.Count} precio(s)";
            }

            // ✅ Actualizar estado de botones
            ActualizarEstadoBotones();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en DgvPrecios_CellClick: {ex.Message}");
            tslInfo.Text = $"{dgvPrecios.Rows.Count} precio(s)";
        }
    }

    private void DgvPrecios_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (_isLoading || e.RowIndex < 0) return;
        TsbEditar_Click(sender, EventArgs.Empty);
    }

    private void DgvPrecios_SelectionChanged(object? sender, EventArgs e)
    {
        if (_isLoading) return;
        ActualizarEstadoBotones();
    }

    private void CboPresentacion_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isLoading)
            return;

        CargarGrilla();
        ActualizarResumen();
    }

    private void TsbNuevo_Click(object? sender, EventArgs e)
    {
        if (_presentacionSeleccionada == null)
        {
            MessageBox.Show("Debe seleccionar una presentación primero.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        BeginEdit();
        _precioActual = null;  // Nuevo precio
        MostrarPanelEdicion();
        LimpiarCamposEdicion();
        lblModoEdicion.Text = "💰 Nuevo Precio de Venta";
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        if (!ValidarCamposEdicion())
            return;

        try
        {
            if (_container == null || _presentacionSeleccionada == null)
            {
                NotificationManager.Error("No hay presentación seleccionada.");
                return;
            }

            _precioRepo ??= _container.UnitOfWork.GetCustomRepository<IPrecioRepository>();
            if (_precioRepo == null)
            {
                NotificationManager.Error("No se pudo obtener el repositorio de precios.");
                return;
            }

            bool esNuevo = (_precioActual == null);

            // S57.62 M8.1-C: el servicio controla commit (uno por operación);
            // la UI solo mapea Result.
            var writeSvc = new PrecioWriteService(_precioRepo, _container.UnitOfWork);
            if (esNuevo)
            {
                var rCreate = writeSvc.Create(
                    _presentacionSeleccionada.Value,
                    numValor.Value,
                    DateOnly.FromDateTime(dtpFecVig.Value),
                    chkActivo.Checked);
                if (rCreate.IsFailure)
                {
                    NotificationManager.Error(PrecioWriteUiMapper.ToMessage(rCreate.Error));
                    return;
                }
            }
            else
            {
                var rUpdate = writeSvc.Update(
                    _precioActual!.IdPrecio,
                    numValor.Value,
                    DateOnly.FromDateTime(dtpFecVig.Value),
                    chkActivo.Checked);
                if (rUpdate.IsFailure)
                {
                    NotificationManager.Error(PrecioWriteUiMapper.ToMessage(rUpdate.Error));
                    return;
                }
            }

            // El servicio ya commiteó; mantener flujo de mensajes.
            {
                var mensaje = esNuevo ? "✅ Precio guardado correctamente." : "✅ Precio actualizado correctamente.";
                NotificationManager.Success(mensaje);

                OcultarPanelEdicion();
                CargarGrilla();
                EndEdit();
                _precioActual = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en BtnGuardar_Click: {ex.Message}");
            NotificationManager.Error($"Error al guardar precio: {ex.Message}");
        }
    }

    private void BtnCancelar_Click(object? sender, EventArgs e)
    {
        OcultarPanelEdicion();
        CancelEdit();
    }

    private void BtnCerrarEdicion_Click(object? sender, EventArgs e)
    {
        OcultarPanelEdicion();
        CancelEdit();
    }

    private void MostrarPanelEdicion()
    {
        pnlEdicion.Visible = true;
        lblModoEdicion.Text = "💰 Nuevo Precio de Venta";
    }

    private void OcultarPanelEdicion()
    {
        pnlEdicion.Visible = false;
    }

    private void LimpiarCamposEdicion()
    {
        numValor.Value = 0;
        dtpFecVig.Value = DateTime.Now;
        chkActivo.Checked = true;
        pnlValidacion.Visible = false;
    }

    private bool ValidarCamposEdicion()
    {
        if (numValor.Value <= 0)
        {
            pnlValidacion.Visible = true;
            lblMensajeValidacion.Text = "⚠️ El valor del precio debe ser mayor a cero.";
            return false;
        }

        pnlValidacion.Visible = false;
        return true;
    }

    /// <summary>
    /// Carga un precio seleccionado en el formulario de edición.
    /// </summary>
    private void CargarPrecioParaEdicion(Precio precio)
    {
        _precioActual = precio;
        numValor.Value = precio.Valor;
        dtpFecVig.Value = precio.FecVig.ToDateTime(TimeOnly.MinValue);
        chkActivo.Checked = precio.Activo;
        pnlValidacion.Visible = false;

        BeginEdit();
        MostrarPanelEdicion();
        lblModoEdicion.Text = "✏️ Editar Precio de Venta";
    }

    /// <summary>
    /// Obtiene el ID del precio seleccionado en la grilla.
    /// </summary>
    private long? GetSelectedPrecioId()
    {
        if (dgvPrecios.SelectedRows.Count == 0)
            return null;

        // Necesitamos obtener el IdPrecio del registro original
        // Como la grilla muestra dynamic objects, necesitamos recuperar del repositorio
        var item = dgvPrecios.SelectedRows[0].DataBoundItem as dynamic;
        if (item == null)
            return null;

        // Buscar el precio por fecha de vigencia (edición por IdPrecio directo).
        _precioRepo ??= _container?.UnitOfWork.GetCustomRepository<IPrecioRepository>();
        if (_precioRepo == null)
            return null;

        // Obtener todos los precios y encontrar el que coincida
        var precResult = _precioRepo.GetByPresentacion(_presentacionSeleccionada.Value);
        if (!precResult.IsSuccess || precResult.Value == null)
            return null;

        var precio = precResult.Value.FirstOrDefault(p =>
            p.FecVig.ToString() == item.FechaVigencia);

        return precio?.IdPrecio;
    }

    /// <summary>
    /// Abre el diálogo de edición para el precio seleccionado.
    /// </summary>
    private void TsbEditar_Click(object? sender, EventArgs e)
    {
        if (dgvPrecios.SelectedRows.Count == 0)
        {
            NotificationManager.Warning("Debe seleccionar un precio para editar.");
            return;
        }

        _precioRepo ??= _container?.UnitOfWork.GetCustomRepository<IPrecioRepository>();
        if (_precioRepo == null)
            return;

        // Obtener el precio seleccionado
        var item = dgvPrecios.SelectedRows[0].DataBoundItem as dynamic;
        if (item == null)
            return;

        // Buscar el precio
        long idPrecio = item.IdPrecio;
        var precResult = _precioRepo.GetById(idPrecio);
        if (!precResult.IsSuccess || precResult.Value == null)
        {
            NotificationManager.Error("No se pudo encontrar el precio.");
            return;
        }

        var precio = precResult.Value;
        if (precio == null)
        {
            NotificationManager.Error("No se pudo encontrar el precio seleccionado.");
            return;
        }

        CargarPrecioParaEdicion(precio);
    }

    /// <summary>
    /// Elimina el precio seleccionado.
    /// </summary>
    private void TsbEliminar_Click(object? sender, EventArgs e)
    {
        if (dgvPrecios.SelectedRows.Count == 0)
        {
            NotificationManager.Warning("Debe seleccionar un precio para eliminar.");
            return;
        }

        var item = dgvPrecios.SelectedRows[0].DataBoundItem as dynamic;
        if (item == null)
            return;

        if (MessageBox.Show($"¿Eliminar el precio de {item.MontoVenta}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            _precioRepo ??= _container.UnitOfWork.GetCustomRepository<IPrecioRepository>();
            if (_precioRepo == null)
            {
                NotificationManager.Error("No se pudo obtener el repositorio de precios.");
                return;
            }

            // Buscar el precio
            long idPrecio = item.IdPrecio;

            // S57.62 M8.1-C: Delete vía servicio (guard + commit único).
            var writeSvcDel = new PrecioWriteService(_precioRepo, _container.UnitOfWork);
            var delResult = writeSvcDel.Delete(idPrecio);
            if (delResult.IsFailure)
            {
                NotificationManager.Error(PrecioWriteUiMapper.ToMessage(delResult.Error));
                return;
            }

            NotificationManager.Success("✅ Precio eliminado correctamente.");
            CargarGrilla();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en TsbEliminar_Click: {ex.Message}");
            NotificationManager.Error($"Error al eliminar precio: {ex.Message}");
        }
    }

    ///// Obtiene valores del formulario de edición (legado TipoPrec retirado en S57.69).
    ///// Convierte a int primero para evitar InvalidCastException.
    ///// </summary>
    //private byte GetTipoPrecioSeguro(ComboBox combo)
    //{
    //    try
    //    {
    //        var value = combo.SelectedValue ?? 0;
    //        var intValue = (int)value;
    //        return (byte)intValue;
    //    }
    //    catch (Exception ex)
    //    {
    //        System.Diagnostics.Debug.WriteLine($"Error en GetTipoPrecioSeguro: {ex.Message}");
    //        return 0;
    //    }
    //}

    /// <summary>
    /// ✅ Ocultar columnas de ID en la grilla
    /// </summary>
    private void OcultarColumnasID(DataGridView dgv)
    {
        try
        {
            if (dgv?.Columns == null)
                return;

            // Ocultar columnas que comienzan con "Id"
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Name.StartsWith("Id", StringComparison.Ordinal))
                {
                    col.Visible = false;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en OcultarColumnasID: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza el panel de resumen (pnlResumen) con el precio normal vigente.
    /// Llama GetVigente para obtener el precio activo actual.
    /// </summary>
    private void ActualizarResumen()
    {
        try
        {
            if (_presentacionSeleccionada == null || _presentacionSeleccionada <= 0)
            {
                lblPrecioActual.Text = "💰 Precio actual: $0.00";
                lblVigencia.Text = "📅 Vigente desde: --/--/----";
                lblProximoCambio.Text = "⏭️ Próximo cambio: --/--/----";
                return;
            }

            _precioRepo ??= _container?.UnitOfWork.GetCustomRepository<IPrecioRepository>();
            if (_precioRepo == null)
            {
                lblPrecioActual.Text = "💰 Precio actual: $0.00";
                lblVigencia.Text = "📅 Vigente desde: --/--/----";
                lblProximoCambio.Text = "⏭️ Próximo cambio: --/--/----";
                return;
            }

            var fechaActual = DateOnly.FromDateTime(DateTime.Now);
            var precioVigente = _precioRepo.GetVigente(_presentacionSeleccionada.Value, fechaActual);
            if (precioVigente.IsFailure || precioVigente.Value == null)
            {
                lblPrecioActual.Text = "💰 Precio actual: --";
                lblVigencia.Text = "📅 Vigente desde: --/--/----";
                lblProximoCambio.Text = "⏭️ Próximo cambio: --/--/----";
                return;
            }

            var precio = precioVigente.Value;
            lblPrecioActual.Text = $"💰 Precio actual: {precio.Valor.ToString("C")}";
            lblVigencia.Text = $"📅 Vigente desde: {precio.FecVig:dd/MM/yyyy}";

            var todosPrecios = _precioRepo.GetByPresentacion(_presentacionSeleccionada.Value);
            var proxPrecio = todosPrecios.Value?.FirstOrDefault(p =>
                p.FecVig > precio.FecVig && p.Activo);

            lblProximoCambio.Text = proxPrecio != null
                ? $"⏭️ Próximo cambio: {proxPrecio.FecVig:dd/MM/yyyy}"
                : "⏭️ Próximo cambio: ---";
        }
        catch (Exception ex)
        {
            lblPrecioActual.Text = "💰 Precio actual: $0.00";
            lblVigencia.Text = "📅 Vigente desde: --/--/----";
            lblProximoCambio.Text = "⏭️ Próximo cambio: --/--/----";
            System.Diagnostics.Debug.WriteLine($"Error en ActualizarResumen: {ex.Message}");
        }
    }

    private void pnlSelector_Paint(object sender, PaintEventArgs e)
    {

    }

    /// <summary>
    /// DTO simple para el combobox de presentaciones.
    /// </summary>
    private record PresentacionComboItem(long IdPresProd, string Descripcion);
}
