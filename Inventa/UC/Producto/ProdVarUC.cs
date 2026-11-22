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
/// UserControl para gestión de variantes de un producto.
/// Permite crear, editar y eliminar variantes con sus atributos dinámicos.
/// PASO 2: Heredado de BaseProductoUC para usar ErrorProvider y validación en tiempo real.
/// </summary>
public partial class ProdVarUC : BaseProductoUC
{
    // ─────────────────────────────────────────────────────────────────────────
    // Campos privados
    // ─────────────────────────────────────────────────────────────────────────
    private IProductoContainer? _container;
    private Prod? _productoActual;
    private VarProd? _varianteActual;
    private bool _isLoading;
    private bool _isEditing;
    private Dictionary<string, object?>? _valoresOriginales;
    private long _idVarianteGuardada; // ✅ PASO 3: Store saved variant ID to restore selection

    // ─────────────────────────────────────────────────────────────────────────
    // Repositorios (lazy)
    // ─────────────────────────────────────────────────────────────────────────
    private IVarProdRepository? _varProdRepo;
    private IVarAtriRepository? _varAtriRepo;
    private IAtriDefRepository? _atriDefRepo;
    private IAtriValRepository? _atriValRepo;

    // ─────────────────────────────────────────────────────────────────────────
    // Controles dinámicos para atributos
    // ─────────────────────────────────────────────────────────────────────────
    private readonly List<Control> _atributoControls = new();
    private readonly Dictionary<Control, long> _controlToAtriDefId = new();
    private readonly Dictionary<Control, List<long>> _controlToSelectedValues = new();

    // ─────────────────────────────────────────────────────────────────────────
    // Evento público para notificar selección de variante
    // ─────────────────────────────────────────────────────────────────────────
    public event EventHandler<long>? VarianteSeleccionada;

    // ─────────────────────────────────────────────────────────────────────────
    // Constructor
    // ─────────────────────────────────────────────────────────────────────────
    public ProdVarUC()
    {
        InitializeComponent();
            AutoScroll = true;

        // Configurar eventos de la grilla y botones
        dgvVariantes.CellClick += DgvVariantes_CellClick;
        dgvVariantes.CellDoubleClick += DgvVariantes_CellDoubleClick;
        tsbNuevo.Click += BtnNuevo_Click;
        tsbEditar.Click += BtnEditar_Click;
        tsbEliminar.Click += BtnEliminar_Click;
        btnGuardar.Click += BtnGuardar_Click;
        btnCancelar.Click += BtnCancelar_Click;
        btnCerrarEdicion.Click += (s, e) => CancelEdit();
        txtCodigo.TextChanged += TxtCodigo_TextChanged;
        txtNombre.TextChanged += TxtNombre_TextChanged;

        // Configurar la grilla
        ConfigurarGrilla();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IRefreshable
    // ─────────────────────────────────────────────────────────────────────────
    public override bool HasChanges => _valoresOriginales != null && HayCambios();

    public override void RefreshData()
    {
        if (_container == null) return;
        _productoActual = _container.ProductoActual;
        CargarVariantes();
    }

    public override void Clear()
    {
        _isLoading = true;
        try
        {
            _productoActual = null;
            _varianteActual = null;
            dgvVariantes.DataSource = null;
            LimpiarPanelEdicion();
            _valoresOriginales = null;
            _isEditing = false;
            ActualizarEstadoEdicion(false);
            tslVarianteSeleccionada.Text = "Sin variante seleccionada";
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// <summary>
    /// Alias para Clear() mantener compatibilidad con código existente.
    /// </summary>
    public override void Limpiar() => Clear();

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
         // ✅ FIX: Don't clear _varianteActual - keep it so Edit/Delete buttons stay enabled
         // Only clear the editing state and form data
         _valoresOriginales = null;
         _isEditing = false;
         ActualizarEstadoEdicion(false);
         lblModoEdicion.Text = "🆕 Nueva Variante";

         // ✅ Reload the selected variant's data in the form to show current state
         if (_varianteActual != null)
         {
             CargarVarianteParaEdicion(_varianteActual.IdVarProd);
         }
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

        if (string.IsNullOrWhiteSpace(txtCodigo.Text))
        {
            SetError(txtCodigo, "Código obligatorio");
            sb.AppendLine("• El código de variante es obligatorio.");
            esValido = false;
        }
        else if (txtCodigo.Text.Length < 3)
        {
            SetError(txtCodigo, "Mín. 3 caracteres");
            sb.AppendLine("• El código debe tener al menos 3 caracteres.");
            esValido = false;
        }
        else
        {
            ClearError(txtCodigo);
        }

        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            SetError(txtNombre, "Nombre obligatorio");
            sb.AppendLine("• El nombre de la variante es obligatorio.");
            esValido = false;
        }
        else
        {
            ClearError(txtNombre);
        }

        // Validar unicidad del código dentro del producto
        if (esValido && _container != null && _productoActual != null)
        {
            var codigo = txtCodigo.Text.Trim().ToUpperInvariant();
            var idVar = _varianteActual?.IdVarProd ?? 0;
            _varProdRepo ??= _container.UnitOfWork.GetCustomRepository<IVarProdRepository>();
            var existsResult = _varProdRepo.ExistsByCodigo(_productoActual.IdProd, codigo);
            if (existsResult.IsSuccess && existsResult.Value)
            {
                if (idVar == 0)
                {
                    SetError(txtCodigo, "Código ya existe");
                    sb.AppendLine("• Ya existe una variante con este código en el producto.");
                    esValido = false;
                }
                else
                {
                    var otraVariante = _varProdRepo.GetByCodigo(_productoActual.IdProd, codigo);
                    if (otraVariante.IsSuccess && otraVariante.Value != null && otraVariante.Value.IdVarProd != idVar)
                    {
                        SetError(txtCodigo, "Código duplicado");
                        sb.AppendLine("• Ya existe otra variante con este código en el producto.");
                        esValido = false;
                    }
                }
            }
        }

        // Validar atributos obligatorios
        if (esValido && _productoActual != null && _atriDefRepo != null)
        {
            var atributosObligatorios = _atriDefRepo.GetObligatoriosByCategoria(_productoActual.IdCategoria);
            if (atributosObligatorios.IsSuccess)
            {
                foreach (var atri in atributosObligatorios.Value)
                {
                    var control = _atributoControls.FirstOrDefault(c => _controlToAtriDefId.ContainsKey(c) && _controlToAtriDefId[c] == atri.IdAtriDef);
                    if (control == null) continue;

                    if (control is ComboBox combo && combo.SelectedIndex <= 0)
                    {
                        sb.AppendLine($"• El atributo '{atri.Nombre}' es obligatorio.");
                        esValido = false;
                    }
                    else if (control is CheckedListBox clb && clb.CheckedItems.Count == 0)
                    {
                        sb.AppendLine($"• Debe seleccionar al menos un valor para '{atri.Nombre}'.");
                        esValido = false;
                    }
                }
            }
        }

        if (!esValido && !silent)
            NotificationManager.Warning($"Por favor corrija los siguientes errores:\n\n{sb}");

        return esValido;
    }

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

        CargarAtributos();

        // ✅ PASO 2: Set accessibility attributes
        SetAccessible(txtCodigo, "Código de Variante",
            "Código único dentro del producto, 3+ caracteres. Requerido.");
        SetAccessible(txtNombre, "Nombre de Variante",
            "Nombre descriptivo de la variante. Requerido.");
        SetAccessible(numCostoProm, "Costo Promedio",
            "Costo promedio de la variante. Opcional.");
        SetAccessible(chkActivo, "Variante Activa",
            "Marca si la variante está activa o inactiva.");

        // ✅ Mark required fields
        MarkRequired(lblCodigo);
        MarkRequired(lblNombre);
    }

    public bool Guardar()
    {
        if (_container == null || _productoActual == null) return false;

        if (!ValidarDatos())
            return false;

        bool esNuevo = (_varianteActual == null);

        _varProdRepo ??= _container.UnitOfWork.GetCustomRepository<IVarProdRepository>();
        _varAtriRepo ??= _container.UnitOfWork.GetCustomRepository<IVarAtriRepository>();

        try
        {
            if (esNuevo)
            {
                var nueva = new VarProd
                {
                    IdProd = _productoActual.IdProd,
                    Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                    Nombre = txtNombre.Text.Trim(),
                    CostoProm = numCostoProm.Value,
                    Activo = chkActivo.Checked
                };
                var addResult = _varProdRepo.Add(nueva);
                if (addResult.IsFailure)
                {
                    NotificationManager.Error($"Error al crear variante: {addResult.Error?.Message}");
                    return false;
                }
                _varianteActual = nueva;
            }
            else
            {
                _varianteActual!.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                _varianteActual.Nombre = txtNombre.Text.Trim();
                _varianteActual.CostoProm = numCostoProm.Value;
                _varianteActual.Activo = chkActivo.Checked;

                var updateResult = _varProdRepo.Update(_varianteActual);
                if (updateResult.IsFailure)
                {
                    NotificationManager.Error($"Error al actualizar variante: {updateResult.Error?.Message}");
                    return false;
                }
            }

            // Guardar atributos
            GuardarAtributos();

            // ✅ PASO 3: Store variant ID before EndEdit clears _varianteActual
            _idVarianteGuardada = _varianteActual?.IdVarProd ?? 0;

            // Commit será manejado por el formulario padre
            GuardarValoresOriginales();
            EndEdit();
            // ✅ FIX: Don't call CargarVariantes() here - it will load BEFORE SaveChanges()
            // CargarVariantes will be called from BtnGuardar_Click AFTER SaveChanges()
            return true;
        }
        catch (Exception ex)
        {
            NotificationManager.Error($"Error al guardar variante: {ex.Message}");
            return false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Métodos privados
    // ─────────────────────────────────────────────────────────────────────────
    private void ConfigurarGrilla()
    {
        dgvVariantes.AutoGenerateColumns = false;
        dgvVariantes.Columns.Clear();

        dgvVariantes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "IdVarProd",
            HeaderText = "ID",
            DataPropertyName = nameof(VarianteItem.IdVarProd),
            Width = 50,
            Visible = false
        });
        dgvVariantes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Codigo",
            HeaderText = "Código",
            DataPropertyName = nameof(VarianteItem.Codigo),
            Width = 100
        });
        dgvVariantes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Nombre",
            HeaderText = "Nombre",
            DataPropertyName = nameof(VarianteItem.Nombre),
            Width = 200
        });
        dgvVariantes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Atributos",
            HeaderText = "Atributos",
            DataPropertyName = nameof(VarianteItem.Atributos),
            Width = 250
        });
        dgvVariantes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CostoProm",
            HeaderText = "Costo Prom.",
            DataPropertyName = nameof(VarianteItem.CostoProm),
            Width = 100,
            DefaultCellStyle = new DataGridViewCellStyle
            { Format = "C4", Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        dgvVariantes.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "Activo",
            HeaderText = "Activo",
            DataPropertyName = nameof(VarianteItem.Activo),
            Width = 60
        });

        // Aplicar estilo moderno (suponiendo que existe un método de extensión)
        dgvVariantes.ApplyModernStyle();
    }

    private void CargarVariantes()
    {
        if (_container == null || _productoActual == null) return;

        _varProdRepo ??= _container.UnitOfWork.GetCustomRepository<IVarProdRepository>();
        _varAtriRepo ??= _container.UnitOfWork.GetCustomRepository<IVarAtriRepository>();

        var result = _varProdRepo.GetByProducto(_productoActual.IdProd, soloActivos: false);
        if (!result.IsSuccess)
        {
            NotificationManager.Error($"Error al cargar variantes: {result.Error?.Message}");
            return;
        }

        var variantes = result.Value.ToList();

        var lista = variantes.Select(v => new VarianteItem(
            v.IdVarProd,
            v.Codigo,
            v.Nombre,
            ObtenerAtributosVariante(v.IdVarProd),
            v.CostoProm,
            v.Activo
        )).ToList();

        dgvVariantes.DataSource = lista;
        if (dgvVariantes.Rows.Count > 0 && _varianteActual != null)
        {
            // Mantener selección si es posible
            var row = dgvVariantes.Rows.Cast<DataGridViewRow>().FirstOrDefault(r =>
                ((VarianteItem)r.DataBoundItem).IdVarProd == _varianteActual.IdVarProd);
            if (row != null)
                row.Selected = true;
        }
    }

    private string ObtenerAtributosVariante(long idVarProd)
    {
        if (_container == null) return string.Empty;
        _varAtriRepo ??= _container.UnitOfWork.GetCustomRepository<IVarAtriRepository>();

        var result = _varAtriRepo.GetByVarianteConDetalle(idVarProd);
        if (!result.IsSuccess) return string.Empty;

        return string.Join(", ", result.Value
            .Where(va => va.AtriVal != null)
            .Select(va => $"{va.AtriVal!.AtriDef?.Nombre}: {va.AtriVal.Nombre}"));
    }

    /// <summary>
    /// Obtiene la cantidad de presentaciones (PresProd) asociadas a una variante.
    /// </summary>
    private int ObtenerCantidadPresentaciones(long idVarProd)
    {
        if (_container == null) return 0;

        try
        {
            var presProdRepo = _container.UnitOfWork.GetCustomRepository<IPresProdRepository>();
            var result = presProdRepo.GetByVariante(idVarProd);

            if (result.IsSuccess && result.Value != null)
            {
                return result.Value.Count();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error obtaining presentation count: {ex.Message}");
        }

        return 0;
    }

    private void CargarAtributos()
    {
        if (_container == null || _productoActual == null) return;

        // Limpiar controles existentes
        foreach (var ctrl in _atributoControls)
            flpAtributos.Controls.Remove(ctrl);
        _atributoControls.Clear();
        _controlToAtriDefId.Clear();
        _controlToSelectedValues.Clear();

        // Obtener definiciones de atributos de la categoría
        _atriDefRepo ??= _container.UnitOfWork.GetCustomRepository<IAtriDefRepository>();
        var atributosResult = _atriDefRepo.GetByCategoria(_productoActual.IdCategoria, soloActivos: true);
        if (!atributosResult.IsSuccess) return;

        _atriValRepo ??= _container.UnitOfWork.GetCustomRepository<IAtriValRepository>();

        foreach (var atri in atributosResult.Value)
        {
            var valoresResult = _atriValRepo.GetByAtriDef(atri.IdAtriDef, soloActivos: true);
            if (!valoresResult.IsSuccess) continue;

            var valores = valoresResult.Value.ToList();
            if (valores.Count == 0) continue;

            // Crear panel para este atributo
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(4)
            };

            var lbl = new Label
            {
                Text = atri.Nombre,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true,
                MinimumSize = new Size(120, 20)
            };
            panel.Controls.Add(lbl);

            Control control;
            if (atri.EsOblig || valores.Count <= 5)
            {
                // ComboBox para selección única
                var combo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Width = 200,
                    Tag = atri
                };
                combo.Items.Add(new KeyValuePair<long, string>(0, "Seleccione..."));
                foreach (var v in valores)
                    combo.Items.Add(new KeyValuePair<long, string>(v.IdAtriVal, v.Nombre));
                combo.DisplayMember = "Value";
                combo.ValueMember = "Key";
                combo.SelectedIndex = 0;
                control = combo;
            }
            else
            {
                // CheckedListBox para múltiples selecciones
                var checkedList = new CheckedListBox
                {
                    Height = 80,
                    Width = 200,
                    Tag = atri
                };
                foreach (var v in valores)
                    checkedList.Items.Add(new KeyValuePair<long, string>(v.IdAtriVal, v.Nombre), false);
                checkedList.DisplayMember = "Value";
                control = checkedList;
            }

            panel.Controls.Add(control);
            flpAtributos.Controls.Add(panel);

            _atributoControls.Add(control);
            _controlToAtriDefId[control] = atri.IdAtriDef;
            _controlToSelectedValues[control] = new List<long>();
        }

        gpbAtributos.Visible = _atributoControls.Count > 0;
    }

    private void CargarAtributosVariante(long idVarProd)
    {
        if (_container == null) return;
        _varAtriRepo ??= _container.UnitOfWork.GetCustomRepository<IVarAtriRepository>();

        var result = _varAtriRepo.GetByVariante(idVarProd);
        if (!result.IsSuccess) return;

        var atributosSeleccionados = result.Value.ToList();

        foreach (var control in _atributoControls)
        {
            if (control is ComboBox combo)
            {
                combo.SelectedIndex = 0;
                foreach (var item in combo.Items)
                {
                    if (item is KeyValuePair<long, string> kvp && atributosSeleccionados.Any(a => a.IdAtriVal == kvp.Key))
                    {
                        combo.SelectedItem = item;
                        break;
                    }
                }
            }
            else if (control is CheckedListBox clb)
            {
                for (int i = 0; i < clb.Items.Count; i++)
                {
                    var item = (KeyValuePair<long, string>)clb.Items[i];
                    clb.SetItemChecked(i, atributosSeleccionados.Any(a => a.IdAtriVal == item.Key));
                }
            }
        }
    }

    private void GuardarAtributos()
    {
        if (_container == null || _varianteActual == null) return;

        _varAtriRepo ??= _container.UnitOfWork.GetCustomRepository<IVarAtriRepository>();

        // Eliminar todos los atributos actuales de la variante
        var existentes = _varAtriRepo.GetByVariante(_varianteActual.IdVarProd);
        if (existentes.IsSuccess)
        {
            foreach (var va in existentes.Value)
            {
                _varAtriRepo.Remove(va);
            }
        }

        // Agregar los nuevos seleccionados
        foreach (var control in _atributoControls)
        {
            List<long> valoresSeleccionados = new();

            if (control is ComboBox combo && combo.SelectedItem is KeyValuePair<long, string> kvp && kvp.Key != 0)
            {
                valoresSeleccionados.Add(kvp.Key);
            }
            else if (control is CheckedListBox clb)
            {
                for (int i = 0; i < clb.Items.Count; i++)
                {
                    if (clb.GetItemChecked(i))
                    {
                        var item = (KeyValuePair<long, string>)clb.Items[i];
                        valoresSeleccionados.Add(item.Key);
                    }
                }
            }

            foreach (var idAtriVal in valoresSeleccionados)
            {
                var va = new VarAtri
                {
                    IdVarProd = _varianteActual.IdVarProd,
                    IdAtriVal = idAtriVal
                };
                _varAtriRepo.Add(va);
            }
        }
    }

    private void IniciarEdicionVariante()
    {
        if (_varianteActual == null) return;
        BeginEdit();
        CargarAtributos();
        CargarAtributosVariante(_varianteActual.IdVarProd);
        lblModoEdicion.Text = "✏️ Editar Variante";
        pnlEdicion.Visible = true;
    }

    private void LimpiarPanelEdicion()
    {
        txtCodigo.Clear();
        txtNombre.Clear();
        numCostoProm.Value = 0;
        chkActivo.Checked = true;

        // ✅ PASO 2: Use unified error clearing
        ClearAllErrors();

        foreach (var control in _atributoControls)
        {
            if (control is ComboBox combo)
                combo.SelectedIndex = 0;
            else if (control is CheckedListBox clb)
            {
                for (int i = 0; i < clb.Items.Count; i++)
                    clb.SetItemChecked(i, false);
            }
        }
    }

    private void GuardarValoresOriginales()
    {
        // ✅ FIX: Always save original values, even for new variants
        _valoresOriginales = new Dictionary<string, object?>
        {
            ["Codigo"] = txtCodigo.Text,
            ["Nombre"] = txtNombre.Text,
            ["CostoProm"] = numCostoProm.Value,
            ["Activo"] = chkActivo.Checked
        };

        // Guardar valores originales de atributos dinámicos
        GuardarAtributosOriginales();
    }

    private void GuardarAtributosOriginales()
    {
        foreach (var control in _atributoControls)
        {
            var key = $"Atri_{_controlToAtriDefId[control]}";
            if (control is ComboBox combo)
            {
                _valoresOriginales?[$"{key}_Combo"] = combo.SelectedItem;
            }
            else if (control is CheckedListBox clb)
            {
                var checkedKeys = new List<long>();
                for (int i = 0; i < clb.Items.Count; i++)
                {
                    if (clb.GetItemChecked(i) && clb.Items[i] is KeyValuePair<long, string> kvp)
                        checkedKeys.Add(kvp.Key);
                }
                _valoresOriginales?[$"{key}_Checked"] = checkedKeys;
            }
        }
    }

    private bool HayCambios()
    {
        if (_valoresOriginales == null) return false;
        try
        {
            // Verificar cambios en campos básicos
            if (!NullSafeEquals(_valoresOriginales["Codigo"], txtCodigo.Text) ||
                !NullSafeEquals(_valoresOriginales["Nombre"], txtNombre.Text) ||
                !NullSafeEquals(_valoresOriginales["CostoProm"], numCostoProm.Value) ||
                !NullSafeEquals(_valoresOriginales["Activo"], chkActivo.Checked))
                return true;

            // Verificar cambios en atributos dinámicos
            foreach (var control in _atributoControls)
            {
                var atriDefId = _controlToAtriDefId[control];
                if (control is ComboBox combo)
                {
                    var key = $"Atri_{atriDefId}_Combo";
                    if (!NullSafeEquals(_valoresOriginales.ContainsKey(key) ? _valoresOriginales[key] : null, combo.SelectedItem))
                        return true;
                }
                else if (control is CheckedListBox clb)
                {
                    var key = $"Atri_{atriDefId}_Checked";
                    var currentChecked = new List<long>();
                    for (int i = 0; i < clb.Items.Count; i++)
                    {
                        if (clb.GetItemChecked(i) && clb.Items[i] is KeyValuePair<long, string> kvp)
                            currentChecked.Add(kvp.Key);
                    }
                    var originalChecked = _valoresOriginales.ContainsKey(key) ? (_valoresOriginales[key] as List<long>) ?? new() : new();
                    if (!currentChecked.SequenceEqual(originalChecked))
                        return true;
                }
            }

            return false;
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
        tsbEditar.Enabled = !habilitado && _varianteActual != null;
        tsbEliminar.Enabled = !habilitado && _varianteActual != null;
        pnlEdicion.Visible = habilitado;
        if (habilitado)
        {
            txtCodigo.Focus();
        }
    }

    private long? GetSelectedVariantId()
    {
        if (dgvVariantes.SelectedRows.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("❌ GetSelectedVariantId: No rows selected");
            return null;
        }

        var row = dgvVariantes.SelectedRows[0];
        System.Diagnostics.Debug.WriteLine($"📍 GetSelectedVariantId: Selected row index={row.Index}");

        if (row.DataBoundItem is VarianteItem item)
        {
            System.Diagnostics.Debug.WriteLine($"✅ GetSelectedVariantId: Found VarianteItem with ID={item.IdVarProd}");
            return item.IdVarProd;
        }

        System.Diagnostics.Debug.WriteLine($"❌ GetSelectedVariantId: DataBoundItem is {row.DataBoundItem?.GetType().Name ?? "NULL"}");
        return null;
    }

    private void CargarVarianteParaEdicion(long idVarProd)
    {
        _varProdRepo ??= _container!.UnitOfWork.GetCustomRepository<IVarProdRepository>();
        var result = _varProdRepo.GetById(idVarProd);
        if (!result.IsSuccess || result.Value == null) return;

        _varianteActual = result.Value;
        txtCodigo.Text = _varianteActual.Codigo;
        txtNombre.Text = _varianteActual.Nombre;
        numCostoProm.Value = _varianteActual.CostoProm;
        chkActivo.Checked = _varianteActual.Activo;
        GuardarValoresOriginales();
    }

    /// <summary>
    /// ✅ PASO 3: Restore grid selection after reload to ensure SelectionChanged event fires.
    /// This is critical because EndEdit() clears _varianteActual, and the new grid items
    /// won't trigger SelectionChanged unless we explicitly select a row.
    /// </summary>
    private void RestaurarSeleccionVariante(long idVariante)
    {
        if (idVariante == 0 || dgvVariantes.Rows.Count == 0) return;

        try
        {
            _isLoading = true; // Prevent SelectionChanged from firing prematurely
            foreach (DataGridViewRow row in dgvVariantes.Rows)
            {
                if (row.DataBoundItem is VarianteItem item && item.IdVarProd == idVariante)
                {
                    dgvVariantes.ClearSelection(); // Clear previous selection
                    row.Selected = true;          // Select the row (triggers SelectionChanged)
                    dgvVariantes.FirstDisplayedScrollingRowIndex = row.Index;
                    System.Diagnostics.Debug.WriteLine($"✅ RestaurarSeleccionVariante: Selected row {row.Index} with variant ID {idVariante}");
                    return;
                }
            }
            System.Diagnostics.Debug.WriteLine($"⚠️ RestaurarSeleccionVariante: Variant ID {idVariante} not found in grid");
        }
        finally
        {
            _isLoading = false; // Re-enable SelectionChanged handling
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Eventos de la grilla y botones
    // ─────────────────────────────────────────────────────────────────────────
    private void DgvVariantes_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return; // Ignore header clicks

        System.Diagnostics.Debug.WriteLine($"🔵 DgvVariantes_CellClick CALLED | _isLoading={_isLoading} | RowIndex={e.RowIndex}");

        if (_isLoading)
        {
            System.Diagnostics.Debug.WriteLine("⏭️ Early return: _isLoading is TRUE");
            return;
        }

        if (dgvVariantes.Rows[e.RowIndex].DataBoundItem is VarianteItem item)
        {
            var id = item.IdVarProd;
            System.Diagnostics.Debug.WriteLine($"✅ Loading variant ID {id}");
            _varianteActual = null;
            CargarVarianteParaEdicion(id);

            // ✅ Load and display presentation count
            var cantPresentaciones = ObtenerCantidadPresentaciones(id);
            tslVarianteSeleccionada.Text = $"{_varianteActual!.Codigo} - {_varianteActual.Nombre} | 📦 {cantPresentaciones} presentación(es)";

            VarianteSeleccionada?.Invoke(this, id);
            tsbEditar.Enabled = !_isEditing;
            tsbEliminar.Enabled = !_isEditing;
            System.Diagnostics.Debug.WriteLine($"✅ DgvVariantes_CellClick completed: {tslVarianteSeleccionada.Text}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("⚠️ No variant found in clicked row");
            tslVarianteSeleccionada.Text = "Sin variante seleccionada";
            tsbEditar.Enabled = false;
            tsbEliminar.Enabled = false;
        }
    }

    private void DgvVariantes_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (dgvVariantes.Rows[e.RowIndex].DataBoundItem is VarianteItem item)
        {
            CargarVarianteParaEdicion(item.IdVarProd);
            IniciarEdicionVariante();
        }
    }

    private void BtnNuevo_Click(object sender, EventArgs e)
    {
        if (_isEditing) return;
        _varianteActual = null;
        LimpiarPanelEdicion();
        // ✅ FIX: Initialize _valoresOriginales for new variant so HasChanges works correctly
        _valoresOriginales = new Dictionary<string, object?>
        {
            ["Codigo"] = "",
            ["Nombre"] = "",
            ["CostoProm"] = 0m,
            ["Activo"] = true
        };
        CargarAtributos();
        BeginEdit();
        lblModoEdicion.Text = "🆕 Nueva Variante";
        pnlEdicion.Visible = true;
    }

    private void BtnEditar_Click(object sender, EventArgs e)
    {
        if (_isEditing) return;
        if (_varianteActual == null) return;
        CargarAtributos();
        CargarAtributosVariante(_varianteActual.IdVarProd);
        BeginEdit();
        lblModoEdicion.Text = "✏️ Editar Variante";
        pnlEdicion.Visible = true;
    }

    private void BtnEliminar_Click(object sender, EventArgs e)
    {
        if (_isEditing || _varianteActual == null) return;
        if (MessageBox.Show($"¿Eliminar la variante {_varianteActual.Codigo} - {_varianteActual.Nombre}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        _varProdRepo ??= _container.UnitOfWork.GetCustomRepository<IVarProdRepository>();

        // Verificar si la variante tiene presentaciones con stock antes de eliminar
        var presProdRepo = _container.UnitOfWork.GetCustomRepository<IPresProdRepository>();
        var presentaciones = presProdRepo.GetByVariante(_varianteActual.IdVarProd);
        if (presentaciones.IsSuccess && presentaciones.Value.Any())
        {
            var stockRepo = _container.UnitOfWork.GetCustomRepository<IStockRepository>();
            var tieneStock = false;
            foreach (var pres in presentaciones.Value)
            {
                var stock = stockRepo.GetByVariante(_container.IdTenant, pres.IdVarProd);
                if (stock.IsSuccess && stock.Value.Any(s => s.CantAct > 0))
                {
                    tieneStock = true;
                    break;
                }
            }
            if (tieneStock)
            {
                NotificationManager.Warning("No se puede eliminar la variante porque tiene stock registrado.");
                return;
            }
        }

        var removeResult = _varProdRepo.Remove(_varianteActual);
        if (removeResult.IsFailure)
        {
            NotificationManager.Error($"Error al eliminar variante: {removeResult.Error?.Message}");
            return;
        }

        try
        {
            _container.UnitOfWork.SaveChanges();
            NotificationManager.Success("Variante eliminada.");
            CargarVariantes();
            _varianteActual = null;
            tslVarianteSeleccionada.Text = "Sin variante seleccionada";
        }
        catch (Exception ex)
        {
            NotificationManager.Error($"Error al eliminar variante: {ex.Message}");
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
                    NotificationManager.Success("✅ Variante guardada correctamente.");
                    System.Diagnostics.Debug.WriteLine($"✅ BtnGuardar_Click: SaveChanges() exitoso, {saved} registros guardados");

                    // ✅ FIX: NOW reload the grid AFTER SaveChanges() so the new variant is visible
                    CargarVariantes();

                    // ✅ PASO 3: Restore selection after grid reload to trigger SelectionChanged
                    if (_idVarianteGuardada > 0)
                    {
                        RestaurarSeleccionVariante(_idVarianteGuardada);
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

    // ─────────────────────────────────────────────────────────────────────────
    // Eventos de cambio en campos de edición (Real-Time Validation con Debounce)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Valida el código de variante en tiempo real (debounced, 300ms).
    /// </summary>
    private void TxtCodigo_TextChanged(object sender, EventArgs e)
    {
        if (_isLoading) return;

        // Detectar cambio
        if (_isEditing && _varianteActual != null && HayCambios())
            _container?.MarcarComoModificado();

        // ✅ PASO 2: Real-time validation with debounce
        if (_isEditing)
        {
            ValidationDebouncer.StartDebounce(() =>
            {
                var codigo = txtCodigo.Text.Trim();

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    SetError(txtCodigo, "Código obligatorio");
                }
                else if (codigo.Length < 3)
                {
                    SetError(txtCodigo, "Mín. 3 caracteres");
                }
                else if (_productoActual != null)
                {
                    // Validar unicidad dentro del producto
                    _varProdRepo ??= _container?.UnitOfWork.GetCustomRepository<IVarProdRepository>();
                    if (_varProdRepo != null)
                    {
                        var codigoUpper = codigo.ToUpperInvariant();
                        var exists = _varProdRepo.ExistsByCodigo(_productoActual.IdProd, codigoUpper);
                        if (exists.IsSuccess && exists.Value)
                        {
                            var idVar = _varianteActual?.IdVarProd ?? 0;
                            if (idVar == 0)
                                SetError(txtCodigo, "Código ya existe");
                            else
                            {
                                var otra = _varProdRepo.GetByCodigo(_productoActual.IdProd, codigoUpper);
                                if (otra.IsSuccess && otra.Value?.IdVarProd != idVar)
                                    SetError(txtCodigo, "Código duplicado");
                                else
                                    ClearError(txtCodigo);
                            }
                        }
                        else
                            ClearError(txtCodigo);
                    }
                    else
                        ClearError(txtCodigo);
                }
                else
                    ClearError(txtCodigo);
            }, 300);
        }
        }

        /// <summary>
        /// Alias para Clear() mantener compatibilidad con código existente.
        /// </summary>
    private void TxtNombre_TextChanged(object sender, EventArgs e)
    {
        if (_isLoading) return;

        // Detectar cambio
        if (_isEditing && _varianteActual != null && HayCambios())
            _container?.MarcarComoModificado();

        // ✅ PASO 2: Real-time validation with debounce
        if (_isEditing)
        {
            ValidationDebouncer.StartDebounce(() =>
            {
                var nombre = txtNombre.Text.Trim();

                if (string.IsNullOrWhiteSpace(nombre))
                    SetError(txtNombre, "Nombre obligatorio");
                else
                    ClearError(txtNombre);
            }, 300);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Evento de cambio de producto desde el contenedor
    // ─────────────────────────────────────────────────────────────────────────
    private void OnProductoChanged(object? sender, ProductoChangedEventArgs e)
    {
        if (_isLoading) return;
        if (e.ProductoNuevo != null)
        {
            _productoActual = e.ProductoNuevo;
            RefreshData();
        }
        else
        {
            Clear();
        }
    }

}
