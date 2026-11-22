using CBP.Data.Abstractions;
using CBP.WinFrms.Notification;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.UC.Producto.Base;
using Inventa.UC.Producto.DTOs;
using Inventa.UC.Producto.Interfaces;
using Inventa.UI;
using System.ComponentModel;

namespace Inventa.UC.Producto;

/// <summary>
/// UserControl para gestión de códigos de barras (CodBar) asociados a una presentación de producto (PresProd).
/// 
/// ESTRUCTURA DE CONTROLES:
/// ═══════════════════════════════════════════════════════════════════════════════════════════════════════════
/// 
/// 🔍 CONTROLES DE FILTRADO/VISUALIZACIÓN (Grilla):
/// ─────────────────────────────────────────────
/// • cboTipoCod + chkSoloPrincipales → Filtran la VISUALIZACIÓN de códigos en la grilla
///   - No guardan datos en la BBDD
///   - Solo cambian lo que ves en la pantalla
///   - Ejemplo: Si seleccionas "Pesable" y "Solo Principales", solo ve esos códigos
/// 
/// 🖊️  CONTROLES DE CREACIÓN/EDICIÓN (Panel):
/// ────────────────────────────────────────────
/// • cboTipoCodEdicion + chkPrincipal → Se usan cuando CREAS o EDITAS un código
///   - Son los valores que se GUARDAN en la BBDD
///   - Están en el panel de edición (pnlEdicion) que aparece al clickear Nuevo o Editar
///   - Estos valores se persisten cuando haces click en "Guardar"
/// 
/// FLUJO DE CREACIÓN:
/// 1. Click en "Nuevo" → NuevoCodigo() → Muestra pnlEdicion con cboTipoCodEdición y chkPrincipal vacíos
/// 2. Llenas los datos (txtCodigo, cboTipoCodEdicion, chkPrincipal)
/// 3. Click en "Guardar" → GuardarCodigo() → ADD a repository + SaveChanges() a BBDD
/// 4. Recargas la grilla con FiltrarCodigos()
/// 5. Los filtros (cboTipoCod, chkSoloPrincipales) aplican de nuevo a la nueva lista
/// ═══════════════════════════════════════════════════════════════════════════════════════════════════════════
/// </summary>
public partial class ProdCodBarUC : BaseProductoUC
{
    private IProductoContainer? _container;
    private Prod? _productoActual;
    private VarProd? _varianteActual;
    private PresProd? _presentacionActual;
    private List<CodigoBarraItem>? _codigosPorPresentacion;
    private bool _isLoading;
    private bool _isEditing;
    private CodigoBarraItem? _codigoEnEdicion;

    private ICodBarRepository? _codBarRepo;
    private IPresProdRepository? _presProdRepo;

    private static readonly string[] TiposCodigo = { "Normal", "Pesable", "Interno", "Proveedor" };

    public override bool HasChanges => _isEditing;
    public override bool IsEditing => _isEditing;
    public override bool IsValid => ValidarDatos(silent: true);

    public bool Guardar() => true;

    public ProdCodBarUC()
    {
        InitializeComponent();
        ConfigurarGrilla();
        ConfigurarEventos();
    }

    private void ConfigurarGrilla()
    {
        dgvCodigos.AutoGenerateColumns = false;
        dgvCodigos.AllowUserToAddRows = false;
        dgvCodigos.ReadOnly = true;
        dgvCodigos.MultiSelect = false;
        dgvCodigos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        // ✅ CRÍTICO: Columna oculta para IdCodBarra (necesaria para editar/eliminar)
        dgvCodigos.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IdCodBarra", HeaderText = "ID", Width = 0, Visible = false, ReadOnly = true });

        dgvCodigos.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo", HeaderText = "Código", Width = 150, ReadOnly = true });
        dgvCodigos.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Tipo", HeaderText = "Tipo", Width = 100, ReadOnly = true });
        dgvCodigos.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "EsPrincipal", HeaderText = "Principal", Width = 80, ReadOnly = true });
        dgvCodigos.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "Activo", HeaderText = "Activo", Width = 80, ReadOnly = true });
        dgvCodigos.ApplyModernStyle();
    }

    private void ConfigurarEventos()
    {
        cboTipoCod.SelectedIndexChanged += (s, e) => FiltrarCodigos();
        chkSoloPrincipales.CheckedChanged += (s, e) => FiltrarCodigos();
        txtCodigo.TextChanged += TxtCodigo_TextChanged;

        // ✅ PATRÓN: Usar CellClick para detectar selección incluso con 1 registro
        dgvCodigos.CellClick += DgvCodigos_CellClick;
        dgvCodigos.DoubleClick += (s, e) => EditarCodigo();

        tsbNuevo.Click += (s, e) => NuevoCodigo();
        tsbEditar.Click += (s, e) => EditarCodigo();
        tsbEliminar.Click += (s, e) => EliminarCodigo();
        btnGuardar.Click += (s, e) => GuardarCodigo();
        btnCancelar.Click += (s, e) => CancelarEdicion();
        btnCerrarEdicion.Click += (s, e) => CancelarEdicion();
    }

    /// <summary>
    /// ✅ PATRÓN: Usar CellClick para actualizar tslInfo incluso con 1 solo registro
    /// </summary>
    private void DgvCodigos_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (_isLoading || e.RowIndex < 0) return;

        try
        {
            var row = dgvCodigos.Rows[e.RowIndex];
            if (row.DataBoundItem != null)
            {
                dynamic item = row.DataBoundItem;
                var codigo = item.Codigo ?? "---";
                var tipo = item.Tipo ?? "---";  // ✅ item.Tipo ya es string (no int)
                var principal = item.EsPrincipal ? "Principal" : "Secundario";

                tslInfo.Text = $"{codigo} ({tipo}) | {principal}";
                row.Selected = true;  // Seleccionar fila
            }
            else
            {
                tslInfo.Text = "Código seleccionado";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en DgvCodigos_CellClick: {ex.Message}");
            tslInfo.Text = $"{dgvCodigos.Rows.Count} código(s)";
        }

        ActualizarEstadoEdicion(_isEditing);
    }

public override void SetContainer(IProductoContainer container)
     {
         _container = container ?? throw new ArgumentNullException(nameof(container));
         base.SetContainer(container);
     }

    public void InitializeData()
    {
        if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            return;

        CargarCombosTipoCodigo();

        // ✅ Accessibility setup
        SetAccessible(cboTipoCod, "Tipo de Código", "Filtrar códigos por tipo (Normal, Pesable, Interno, Proveedor)");
        SetAccessible(chkSoloPrincipales, "Solo Principales", "Mostrar solo códigos principales");
        SetAccessible(txtCodigo, "Código de Barras", "Ingrese el código de barras");
        SetAccessible(cboTipoCodEdicion, "Tipo de Código", "Seleccione el tipo de código");
        SetAccessible(chkPrincipal, "Principal", "Marque si este es el código principal");
    }

    public override void RefreshData()
    {
        if (_container?.ProductoActual == null)
            Clear();
        else
            _productoActual = _container.ProductoActual;
    }

    public override void Clear()
    {
        _productoActual = null;
        _varianteActual = null;
        _presentacionActual = null;
        dgvCodigos.DataSource = null;
        CancelarEdicion();
    }

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
         CancelarEdicion();
         base.CancelEdit();
     }

    public override bool ValidarDatos(bool silent = false) => true;

    public override void Limpiar() => Clear();

    public void SetPresentacion(PresProd presentacion)
    {
        _presentacionActual = presentacion ?? throw new ArgumentNullException(nameof(presentacion));
        CargarCodigosPorPresentacion();
    }

    private void CargarCombosTipoCodigo()
    {
        cboTipoCod.DataSource = TiposCodigo.Select((t, i) => new { Tipo = t, Valor = i + 1 }).ToList();
        cboTipoCod.DisplayMember = "Tipo";
        cboTipoCod.ValueMember = "Valor";
        cboTipoCod.SelectedIndex = 0;

        cboTipoCodEdicion.DataSource = TiposCodigo.Select((t, i) => new { Tipo = t, Valor = i + 1 }).ToList();
        cboTipoCodEdicion.DisplayMember = "Tipo";
        cboTipoCodEdicion.ValueMember = "Valor";
    }

    private void CargarCodigosPorPresentacion()
    {
        if (_presentacionActual == null) return;

        var idPresProd = _presentacionActual.IdPresProd;
        _isLoading = true;
        try
        {
            _codBarRepo ??= _container!.UnitOfWork.GetCustomRepository<ICodBarRepository>();
            var codResult = _codBarRepo.GetByPresentacion(idPresProd);
            if (codResult.IsSuccess)
            {
                _codigosPorPresentacion = codResult.Value
                    .Select(c => new CodigoBarraItem(c.IdCodBar, c.IdPresProd, c.Codigo, c.TipoCod, c.Principal, true))
                    .ToList();
                FiltrarCodigos();
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void FiltrarCodigos()
    {
        if (_codigosPorPresentacion == null) return;

        var filtrados = _codigosPorPresentacion;

        if (cboTipoCod.SelectedValue != null && int.TryParse(cboTipoCod.SelectedValue.ToString(), out int tipoCod))
            filtrados = filtrados.Where(c => c.Tipo == tipoCod).ToList();

        if (chkSoloPrincipales.Checked)
            filtrados = filtrados.Where(c => c.EsPrincipal).ToList();

        dgvCodigos.DataSource = filtrados.Select(c => new
        {
            c.IdCodBarra,
            Codigo = c.Codigo,
            Tipo = TiposCodigo[c.Tipo - 1],
            c.EsPrincipal,
            c.Activo
        }).ToList();

        // ✅ PATRÓN: Mostrar cantidad de registros en tslInfo
        tslInfo.Text = $"{dgvCodigos.Rows.Count} código(s)";
    }

    private void NuevoCodigo()
    {
        if (_presentacionActual == null)
        {
            NotificationManager.Warning("Seleccione una presentación primero.");
            return;
        }

        _codigoEnEdicion = new CodigoBarraItem(0, _presentacionActual.IdPresProd, "", 1, false, true);
        _isEditing = true;
        MostrarPanelEdicion(true);
        ActualizarEstadoEdicion(true);
    }

    private void EditarCodigo()
    {
        if (dgvCodigos.SelectedRows.Count == 0) return;

        var row = dgvCodigos.SelectedRows[0];
        if (row.Cells[0].Value is not long idCod)
        {
            NotificationManager.Error("No se pudo obtener el ID del código. Intente seleccionar de nuevo.");
            return;
        }

        var codigo = _codigosPorPresentacion?.FirstOrDefault(c => c.IdCodBarra == idCod);
        if (codigo == null)
        {
            NotificationManager.Error("No se encontró el código en memoria. Intente recargar.");
            return;
        }

        _codigoEnEdicion = codigo;
        _isEditing = true;
        MostrarPanelEdicion(true);
        ActualizarEstadoEdicion(true);
    }

    private void GuardarCodigo()
    {
        if (_codigoEnEdicion == null || _container == null) return;

        // ✅ Validación
        if (string.IsNullOrWhiteSpace(txtCodigo.Text))
        {
            SetError(txtCodigo, "Código obligatorio");
            NotificationManager.Warning("Ingrese un código de barras.");
            return;
        }
        else
        {
            ClearError(txtCodigo);
        }

        _isLoading = true;
        try
        {
            _codBarRepo ??= _container.UnitOfWork.GetCustomRepository<ICodBarRepository>();

            if (_codigoEnEdicion.IdCodBarra == 0)
            {
                var nuevo = new CodBar
                {
                    IdPresProd = _codigoEnEdicion.IdPresProd,
                    Codigo = txtCodigo.Text.Trim(),
                    TipoCod = (byte)(cboTipoCodEdicion.SelectedValue != null ? int.Parse(cboTipoCodEdicion.SelectedValue.ToString()!) : 1),
                    Principal = chkPrincipal.Checked
                };
                var addResult = _codBarRepo.Add(nuevo);
                if (addResult.IsFailure)
                {
                    NotificationManager.Error($"Error al crear código: {addResult.Error?.Message}");
                    return;
                }
                NotificationManager.Success("✅ Código creado en memoria.");
            }
            else
            {
                var existente = _codBarRepo.GetById(_codigoEnEdicion.IdCodBarra);
                if (existente.IsSuccess && existente.Value != null)
                {
                    existente.Value.Codigo = txtCodigo.Text.Trim();
                    existente.Value.TipoCod = (byte)(cboTipoCodEdicion.SelectedValue != null ? int.Parse(cboTipoCodEdicion.SelectedValue.ToString()!) : 1);
                    existente.Value.Principal = chkPrincipal.Checked;

                    var updateResult = _codBarRepo.Update(existente.Value);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar código: {updateResult.Error?.Message}");
                        return;
                    }
                    NotificationManager.Success("✅ Código actualizado en memoria.");
                }
            }

            // ✅ CRÍTICO: Guardar los cambios en la BBDD (este paso faltaba)
            var savedCount = _container.UnitOfWork.SaveChanges();
            if (savedCount > 0)
            {
                NotificationManager.Success($"✅ Código guardado en BBDD ({savedCount} registro(s)).");
                CargarCodigosPorPresentacion();
                CancelarEdicion();
            }
            else
            {
                NotificationManager.Warning("⚠️ No se detectaron cambios para guardar.");
            }
        }
        catch (Exception ex)
        {
            NotificationManager.Error($"❌ Error al guardar código: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void EliminarCodigo()
    {
        if (dgvCodigos.SelectedRows.Count == 0) return;

        if (MessageBox.Show("¿Está seguro de que desea eliminar este código?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var row = dgvCodigos.SelectedRows[0];
        if (row.Cells[0].Value is not long idCod) return;

        _isLoading = true;
        try
        {
            _codBarRepo ??= _container!.UnitOfWork.GetCustomRepository<ICodBarRepository>();
            var getResult = _codBarRepo.GetById(idCod);
            if (getResult.IsSuccess && getResult.Value != null)
            {
                var deleteResult = _codBarRepo.Remove(getResult.Value);
                if (deleteResult.IsFailure)
                {
                    NotificationManager.Error($"Error al eliminar código: {deleteResult.Error?.Message}");
                    return;
                }

                // ✅ CRÍTICO: Guardar los cambios en la BBDD después de eliminar
                var savedCount = _container.UnitOfWork.SaveChanges();
                if (savedCount > 0)
                {
                    NotificationManager.Success($"✅ Código eliminado ({savedCount} registro(s)).");
                    CargarCodigosPorPresentacion();
                }
                else
                {
                    NotificationManager.Warning("⚠️ No se detectaron cambios para guardar.");
                }
            }
        }
        catch (Exception ex)
        {
            NotificationManager.Error($"❌ Error al eliminar código: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void MostrarPanelEdicion(bool mostrar)
    {
        if (mostrar && _codigoEnEdicion != null)
        {
            txtCodigo.Text = _codigoEnEdicion.Codigo;
            cboTipoCodEdicion.SelectedValue = _codigoEnEdicion.Tipo;
            chkPrincipal.Checked = _codigoEnEdicion.EsPrincipal;

            // ✅ Generar vista previa del código de barras
            GenerarVistaPrevia(_codigoEnEdicion.Codigo);

            pnlEdicion.Visible = true;
            txtCodigo.Focus();
        }
        else
        {
            pnlEdicion.Visible = false;
            LimpiarVistaPrevia();
        }
    }

    private void GenerarVistaPrevia(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                LimpiarVistaPrevia();
                return;
            }

            // ✅ Generar barcode real con ZXing.Net (formato CODE128)
            var imagenBarcode = CrearImagenCodigoBarras(codigo);

            if (imagenBarcode != null)
            {
                picBarcode.Image = imagenBarcode;
                lblPreview.Text = $"📊 Vista previa - Código: {codigo} (CODE 128)";
            }
            else
            {
                LimpiarVistaPrevia();
                lblPreview.Text = "⚠️ No se pudo generar la vista previa";
            }
        }
        catch (Exception ex)
        {
            LimpiarVistaPrevia();
            lblPreview.Text = $"❌ Error: {ex.Message}";
        }
    }

    private void LimpiarVistaPrevia()
    {
        picBarcode.Image?.Dispose();
        picBarcode.Image = null;
    }

    private Image? CrearImagenCodigoBarras(string codigo)
    {
        try
        {
            // ✅ Usar ZXing.Net para generar código de barras real (CODE128)
            // Se muestra el código debajo del barcode
            return BarcodeGenerator.GenerarCodigoBarras(codigo, mostrarTexto: true);
        }
        catch (Exception ex)
        {
            NotificationManager.Warning($"⚠️ No se pudo generar la vista previa del código de barras: {ex.Message}");
            return null;
        }
    }

    private void TxtCodigo_TextChanged(object sender, EventArgs e)
    {
        if (_isEditing && !_isLoading)
        {
            ValidationDebouncer.StartDebounce(() =>
            {
                var codigo = txtCodigo.Text.Trim();
                if (string.IsNullOrWhiteSpace(codigo))
                {
                    SetError(txtCodigo, "C\u00f3digo obligatorio");
                    LimpiarVistaPrevia();
                }
                else
                {
                    ClearError(txtCodigo);
                    // ✅ Actualizar preview en tiempo real mientras escribes
                    GenerarVistaPrevia(codigo);
                }
            }, 300);
        }
    }

    private void CancelarEdicion()
    {
        _codigoEnEdicion = null;
        MostrarPanelEdicion(false);
        _isEditing = false;
        ActualizarEstadoEdicion(false);
    }

    private void ActualizarEstadoEdicion(bool habilitado)
    {
        // ✅ Cuando estás en modo edición (habilitado=true), deshabilitas los botones de acción
        // Cuando salgas del modo edición (habilitado=false), habilitas los botones SI hay fila seleccionada
        var haySeleccion = dgvCodigos.SelectedRows.Count > 0;

        tsbNuevo.Enabled = !habilitado;
        tsbEditar.Enabled = !habilitado && haySeleccion;
        tsbEliminar.Enabled = !habilitado && haySeleccion;
    }
}
