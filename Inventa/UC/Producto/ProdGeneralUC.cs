using CBP.Data.Abstractions;
using CBP.MultiTenant.Abstractions;
using CBP.WinFrms.Notification;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.Forms.Mantenedores;
using Inventa.Infrastructure;
using Inventa.UC.Producto.Base;
using Inventa.UC.Producto.Interfaces;
using System.ComponentModel;

namespace Inventa.UC.Producto;

/// <summary>
/// UserControl para la gestión de los datos generales del producto.
/// PASO 2: Heredado de BaseProductoUC para usar ErrorProvider y validación en tiempo real.
/// </summary>
public partial class ProdGeneralUC : BaseProductoUC
    {
        // ─────────────────────────────────────────────────────────────────────────
        // Campos privados
        // ─────────────────────────────────────────────────────────────────────────
        private IProductoContainer? _container;
        private Prod? _productoActual;
        private bool _isLoading;
        private bool _isEditing;
        private Dictionary<string, object?>? _valoresOriginales;

        // ─────────────────────────────────────────────────────────────────────────
        // Repositorios (lazy)
        // ─────────────────────────────────────────────────────────────────────────
        private IProdRepository? _prodRepo;
        private ICategoriaRepository? _categoriaRepo;
        private IMarcaRepository? _marcaRepo;
        private IUnidadRepository? _unidadRepo;

        // ─────────────────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────────────────
        public ProdGeneralUC()
        {
            InitializeComponent();
            AutoScroll = true;

            // Configurar eventos de los campos
            txtCodigo.TextChanged += TxtCodigo_TextChanged;
            txtNombre.TextChanged += TxtNombre_TextChanged;
            txtCodigo.Leave += TxtCodigo_Leave;
            btnAgregarCategoria.Click += BtnAgregarCategoria_Click;
            btnAgregarMarca.Click += BtnAgregarMarca_Click;
            btnAgregarUnidadBase.Click += BtnAgregarUnidadBase_Click;

            // Configurar eventos de los botones de edición
            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += BtnCancelar_Click;

            // Configurar eventos de los botones ToolStrip CRUD
            tsbNuevo.Click += TsbNuevo_Click;
            tsbEditar.Click += TsbEditar_Click;
            tsbEliminar.Click += TsbEliminar_Click;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // IRefreshable
        // ─────────────────────────────────────────────────────────────────────────
        public override bool HasChanges => _valoresOriginales != null && HayCambios();

        public override void RefreshData()
        {
            if (_container == null) return;
            LoadProducto(_container.ProductoActual);
        }

        public override void Clear()
        {
            _isLoading = true;
            try
            {
                _productoActual = null;
                LimpiarCampos();
                _valoresOriginales = null;
                _isEditing = false;
                ActualizarEstadoEdicion(false);
            }
            finally
            {
                _isLoading = false;
            }
        }

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
            if (_container?.ProductoActual != null)
                LoadProducto(_container.ProductoActual);
            else
                LimpiarCampos();
            _isEditing = false;
            ActualizarEstadoEdicion(false);
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

            // Validar código
            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                SetError(txtCodigo, "Código obligatorio");
                sb.AppendLine("• El código es obligatorio.");
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

            // Validar unicidad del código (si es nuevo o cambió)
            if (esValido && _container != null)
            {
                var codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                var idProd = _productoActual?.IdProd ?? 0;
                _prodRepo ??= _container.UnitOfWork.GetCustomRepository<IProdRepository>();
                var existsResult = _prodRepo.ExistsByCodigo(_container.IdTenant, codigo);
                if (existsResult.IsSuccess && existsResult.Value)
                {
                    // Si es el mismo producto que se está editando, permitir
                    if (idProd == 0)
                    {
                        // Nuevo producto con código ya existente
                        SetError(txtCodigo, "Código ya existe");
                        sb.AppendLine("• Ya existe un producto con este código en la empresa.");
                        esValido = false;
                    }
                    else
                    {
                        var otroProducto = _prodRepo.GetByCodigo(_container.IdTenant, codigo);
                        if (otroProducto.IsSuccess && otroProducto.Value != null && otroProducto.Value.IdProd != idProd)
                        {
                            SetError(txtCodigo, "Código duplicado");
                            sb.AppendLine("• Ya existe otro producto con este código en la empresa.");
                            esValido = false;
                        }
                    }
                }
            }

            // Validar nombre
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                SetError(txtNombre, "Nombre obligatorio");
                sb.AppendLine("• El nombre es obligatorio.");
                esValido = false;
            }
            else if (txtNombre.Text.Length < 4)
            {
                SetError(txtNombre, "Mín. 4 caracteres");
                sb.AppendLine("• El nombre debe tener al menos 4 caracteres.");
                esValido = false;
            }
            else
            {
                ClearError(txtNombre);
            }

            // Validar categoría
            if (cboCategoria.SelectedValue == null || (long)cboCategoria.SelectedValue == 0)
            {
                SetError(cboCategoria, "Categoría requerida");
                sb.AppendLine("• Debe seleccionar una categoría.");
                esValido = false;
            }
            else
            {
                ClearError(cboCategoria);
            }

            // Validar unidad base
            if (cboUnidadBase.SelectedValue == null)
            {
                SetError(cboUnidadBase, "Unidad requerida");
                sb.AppendLine("• Debe seleccionar una unidad base.");
                esValido = false;
            }
            else
            {
                ClearError(cboUnidadBase);
            }

            if (!esValido && !silent)
                NotificationManager.Warning($"Por favor corrija los siguientes errores:\n\n{sb}");

            return esValido;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Métodos públicos
        // ─────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Establece el contenedor (formulario padre) y suscribe a eventos.
        /// </summary>
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

            // ✅ PASO 2: Set accessibility attributes for screen readers and keyboard users
            SetAccessible(txtCodigo, "Código del Producto",
                "Código único del producto, 3+ caracteres. Requerido.");
            SetAccessible(txtNombre, "Nombre del Producto",
                "Nombre único del producto, 4+ caracteres. Requerido.");
            SetAccessible(cboCategoria, "Categoría",
                "Categoría del producto. Requerido.");
            SetAccessible(cboMarca, "Marca",
                "Marca del producto. Opcional.");
            SetAccessible(cboUnidadBase, "Unidad Base",
                "Unidad de medida base. Requerido.");
            SetAccessible(numDiasVencimiento, "Días de Vencimiento",
                "Días antes de vencimiento (0 = sin vencimiento). Opcional.");
            SetAccessible(chkManejaLote, "Maneja Lote",
                "Indica si el producto usa lotes de producción.");
            SetAccessible(chkEsPesable, "Es Pesable",
                "Indica si el producto se vende por peso.");
            SetAccessible(chkActivo, "Activo",
                "Marca del producto como activo o inactivo.");

            // ✅ Mark required fields with asterisk + bold
            MarkRequired(lblCodigo);
            MarkRequired(lblNombre);
            MarkRequired(lblCategoria);
            MarkRequired(lblUnidadBase);

            // ✅ Set tab order for keyboard navigation
            SetTabOrder();
        }

        /// <summary>
        /// Establece el orden de tabulación lógico para navegación por teclado.
        /// </summary>
        private void SetTabOrder()
        {
            txtCodigo.TabIndex = 0;
            txtNombre.TabIndex = 1;
            cboCategoria.TabIndex = 2;
            cboMarca.TabIndex = 3;
            cboUnidadBase.TabIndex = 4;
            numDiasVencimiento.TabIndex = 5;
            chkManejaLote.TabIndex = 6;
            chkEsPesable.TabIndex = 7;
            chkActivo.TabIndex = 8;
            btnAgregarCategoria.TabIndex = 9;
            btnAgregarMarca.TabIndex = 10;
            btnAgregarUnidadBase.TabIndex = 11;
        }

        /// <summary>
        /// Carga un producto en el formulario.
        /// </summary>
        public void LoadProducto(Prod? producto)
        {
            _isLoading = true;
            try
            {
                _productoActual = producto;
                if (_productoActual == null)
                {
                    LimpiarCampos();
                    // ✅ FIX: Initialize _valoresOriginales for new products so HasChanges works correctly
                    // Set empty/DBNull values as "original" to detect when user enters data
                    _valoresOriginales = new Dictionary<string, object?>
                    {
                        ["Codigo"] = "",
                        ["Nombre"] = "",
                        ["IdCategoria"] = DBNull.Value,
                        ["IdMarca"] = DBNull.Value,
                        ["IdUnidadBase"] = DBNull.Value,
                        ["DiasVencimiento"] = 0,
                        ["ManejaLote"] = false,
                        ["EsPesable"] = false,
                        ["Activo"] = true
                    };
                    _isEditing = true; // Modo nuevo, habilitado para editar
                    ActualizarEstadoEdicion(true);
                }
                else
                {
                    // Poblar campos
                    txtCodigo.Text = _productoActual.Codigo;
                    txtNombre.Text = _productoActual.Nombre;

                    // ✅ FIX: Use ID-based selection to avoid object reference mismatch
                    // ComboBox items may be different objects than navigation properties
                    SetComboBoxValue(cboCategoria, _productoActual.IdCategoria);
                    SetComboBoxValue(cboMarca, _productoActual.IdMarca ?? 0);
                    SetComboBoxValue(cboUnidadBase, _productoActual.IdUniBase);

                    numDiasVencimiento.Value = _productoActual.DiasVen ?? 0;
                    chkManejaLote.Checked = _productoActual.ManejaLot;
                    chkEsPesable.Checked = _productoActual.EsPesable;
                    chkActivo.Checked = _productoActual.Activo;

                    // Guardar valores originales para detección de cambios
                    GuardarValoresOriginales();
                    _isEditing = false; // No estamos editando hasta que el usuario modifique algo
                    ActualizarEstadoEdicion(false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ EXCEPCIÓN en LoadProducto: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Helper para establecer el valor en ComboBox por comparación de ValueMember.
        /// Evita problemas de referencia de objeto cuando los items vienen de diferentes consultas.
        /// </summary>
        private void SetComboBoxValue(ComboBox combo, object? value)
        {
            if (combo == null) return;
            if (value == null || (value is int && (int)value == 0) || (value is long && (long)value == 0)) return;

            try
            {
                // Si el ValueMember está configurado, buscar por valor en lugar de por referencia
                if (string.IsNullOrEmpty(combo.ValueMember))
                {
                    combo.SelectedValue = value;
                    return;
                }

                // Recorrer items y comparar por ValueMember
                foreach (var item in combo.Items)
                {
                    if (item == null) continue;

                    // Use reflection to get the property value by ValueMember name
                    var prop = item.GetType().GetProperty(combo.ValueMember);
                    if (prop != null)
                    {
                        var propValue = prop.GetValue(item);

                        if (propValue != null && propValue.Equals(value))
                        {
                            combo.SelectedItem = item;
                            return;
                        }
                    }
                }

                // Fallback: try direct assignment
                combo.SelectedValue = value;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error en SetComboBoxValue: {ex.Message}");
            }
        }

        /// <summary>
        /// Guarda los cambios del producto en la base de datos (con commit).
        /// </summary>
        /// <returns>true si se guardó correctamente en la base de datos.</returns>
        public bool Guardar()
        {
            if (_container == null) return false;

            if (!ValidarDatos())
                return false;

            bool esNuevo = (_productoActual == null);

            _prodRepo ??= _container.UnitOfWork.GetCustomRepository<IProdRepository>();

            try
            {
                if (esNuevo)
                {
                    var nuevo = new Prod
                    {
                        IdTenant = _container.IdTenant,
                        Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                        Nombre = txtNombre.Text.Trim().ToUpper(),
                        IdCategoria = (long)cboCategoria.SelectedValue!,
                        IdMarca = cboMarca.SelectedValue is int m && m > 0 ? m : (int?)null,
                        IdUniBase = (int)cboUnidadBase.SelectedValue!,
                        DiasVen = (int)numDiasVencimiento.Value > 0 ? (int)numDiasVencimiento.Value : null,
                        ManejaLot = chkManejaLote.Checked,
                        EsPesable = chkEsPesable.Checked,
                        Activo = chkActivo.Checked
                    };
                    var addResult = _prodRepo.Add(nuevo);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al crear producto: {addResult.Error?.Message}");
                        return false;
                    }
                    _productoActual = nuevo;
                }
                else
                {
                    _productoActual!.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                    _productoActual.Nombre = txtNombre.Text.Trim().ToUpper();
                    _productoActual.IdCategoria = (long)cboCategoria.SelectedValue!;
                    _productoActual.IdMarca = cboMarca.SelectedValue is int m && m > 0 ? m : (int?)null;
                    _productoActual.IdUniBase = (int)cboUnidadBase.SelectedValue!;
                    _productoActual.DiasVen = (int)numDiasVencimiento.Value > 0 ? (int)numDiasVencimiento.Value : null;
                    _productoActual.ManejaLot = chkManejaLote.Checked;
                    _productoActual.EsPesable = chkEsPesable.Checked;
                    _productoActual.Activo = chkActivo.Checked;

                    var updateResult = _prodRepo.Update(_productoActual);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar producto: {updateResult.Error?.Message}");
                        return false;
                    }
                }

                // ✅ CRITICAL FIX: Persist changes to database before returning success
                _container.UnitOfWork.SaveChanges();

                // Actualizar valores originales después de guardar
                GuardarValoresOriginales();
                _isEditing = false;
                ActualizarEstadoEdicion(false);
                return true;
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al guardar producto: {ex.Message}");
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Métodos privados
        // ─────────────────────────────────────────────────────────────────────────
        private void CargarCombos()
        {
            if (_container == null) return;

            try
            {
                // Cargar categorías
                _categoriaRepo ??= _container.UnitOfWork.GetCustomRepository<ICategoriaRepository>();
                var categoriasResult = _categoriaRepo.GetByTenant(_container.IdTenant, soloActivas: true);
                if (categoriasResult.IsSuccess)
                {
                    var categorias = categoriasResult.Value.ToList();
                    cboCategoria.DataSource = categorias;
                    cboCategoria.DisplayMember = nameof(Categoria.Nombre);
                    cboCategoria.ValueMember = nameof(Categoria.IdCategoria);
                }

                // Cargar marcas
                _marcaRepo ??= _container.UnitOfWork.GetCustomRepository<IMarcaRepository>();
                var marcasResult = _marcaRepo.GetByTenant(_container.IdTenant, soloActivas: true);
                if (marcasResult.IsSuccess)
                {
                    var marcas = marcasResult.Value.ToList();
                    cboMarca.DataSource = marcas;
                    cboMarca.DisplayMember = nameof(Marca.Nombre);
                    cboMarca.ValueMember = nameof(Marca.IdMarca);
                }

                // Cargar unidades
                _unidadRepo ??= _container.UnitOfWork.GetCustomRepository<IUnidadRepository>();
                var unidadesResult = _unidadRepo.GetAll();
                if (unidadesResult.IsSuccess)
                {
                    var unidades = unidadesResult.Value.OrderBy(u => u.Nombre).ToList();
                    cboUnidadBase.DataSource = unidades;
                    cboUnidadBase.DisplayMember = nameof(Unidad.Nombre);
                    cboUnidadBase.ValueMember = nameof(Unidad.IdUnidad);
                }
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al cargar combos: {ex.Message}");
            }
        }

        private void LimpiarCampos()
        {
            txtCodigo.Clear();
            txtNombre.Clear();
            cboCategoria.SelectedIndex = -1;
            cboMarca.SelectedIndex = -1;
            cboUnidadBase.SelectedIndex = -1;
            numDiasVencimiento.Value = 0;
            chkManejaLote.Checked = false;
            chkEsPesable.Checked = false;
            chkActivo.Checked = true;

            // ✅ PASO 2: Clear all errors when clearing fields
            ClearAllErrors();
        }

        private void GuardarValoresOriginales()
        {
            _valoresOriginales = new Dictionary<string, object?>
            {
                ["Codigo"] = txtCodigo.Text,
                ["Nombre"] = txtNombre.Text,
                ["IdCategoria"] = cboCategoria.SelectedValue,
                ["IdMarca"] = cboMarca.SelectedValue,
                ["IdUnidadBase"] = cboUnidadBase.SelectedValue,
                ["DiasVencimiento"] = numDiasVencimiento.Value,
                ["ManejaLote"] = chkManejaLote.Checked,
                ["EsPesable"] = chkEsPesable.Checked,
                ["Activo"] = chkActivo.Checked
            };
        }

        private bool HayCambios()
        {
            if (_valoresOriginales == null) return false;
            try
            {
                return !NullSafeEquals(_valoresOriginales["Codigo"], txtCodigo.Text) ||
                       !NullSafeEquals(_valoresOriginales["Nombre"], txtNombre.Text) ||
                       !NullSafeEquals(_valoresOriginales["IdCategoria"], cboCategoria.SelectedValue) ||
                       !NullSafeEquals(_valoresOriginales["IdMarca"], cboMarca.SelectedValue) ||
                       !NullSafeEquals(_valoresOriginales["IdUnidadBase"], cboUnidadBase.SelectedValue) ||
                       !NullSafeEquals(_valoresOriginales["DiasVencimiento"], numDiasVencimiento.Value) ||
                       !NullSafeEquals(_valoresOriginales["ManejaLote"], chkManejaLote.Checked) ||
                       !NullSafeEquals(_valoresOriginales["EsPesable"], chkEsPesable.Checked) ||
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
            // Habilitar/deshabilitar controles según modo edición
            txtCodigo.Enabled = habilitado;
            txtNombre.Enabled = habilitado;
            cboCategoria.Enabled = habilitado;
            cboMarca.Enabled = habilitado;
            cboUnidadBase.Enabled = habilitado;
            numDiasVencimiento.Enabled = habilitado;
            chkManejaLote.Enabled = habilitado;
            chkEsPesable.Enabled = habilitado;
            chkActivo.Enabled = habilitado;
            btnAgregarCategoria.Enabled = habilitado;
            btnAgregarMarca.Enabled = habilitado;
            btnAgregarUnidadBase.Enabled = habilitado;

            // Mostrar/ocultar botones de edición
            pnlAcciones.Visible = habilitado;

            // Mostrar indicador de edición
            lblEstadoEdicion.Text = habilitado ? "✏️ Modo edición activo" : "💡 Doble clic en un producto de la lista para editar";
            lblEstadoEdicion.ForeColor = habilitado ? Color.FromArgb(52, 152, 219) : Color.FromArgb(149, 165, 166);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Eventos de cambios en campos (Real-Time Validation con Debounce)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Valida el código en tiempo real (debounced, 300ms).
        /// </summary>
        private void TxtCodigo_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;

            // Detectar cambio (enter edit mode si es necesario)
            if (!_isEditing && _productoActual != null)
            {
                _isEditing = true;
                ActualizarEstadoEdicion(true);
            }

            // ✅ PASO 2: Real-time validation with debounce (no blocking UI)
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
                else
                {
                    // Validar unicidad
                    _prodRepo ??= _container?.UnitOfWork.GetCustomRepository<IProdRepository>();
                    if (_prodRepo != null && _container != null)
                    {
                        var codigoUpper = codigo.ToUpperInvariant();
                        var exists = _prodRepo.ExistsByCodigo(_container.IdTenant, codigoUpper);
                        if (exists.IsSuccess && exists.Value)
                        {
                            var idProd = _productoActual?.IdProd ?? 0;
                            if (idProd == 0)
                            {
                                SetError(txtCodigo, "Código ya existe");
                            }
                            else
                            {
                                var otro = _prodRepo.GetByCodigo(_container.IdTenant, codigoUpper);
                                if (otro.IsSuccess && otro.Value?.IdProd != idProd)
                                    SetError(txtCodigo, "Código duplicado");
                                else
                                    ClearError(txtCodigo);
                            }
                        }
                        else
                        {
                            ClearError(txtCodigo);
                        }
                    }
                    else
                    {
                        ClearError(txtCodigo);
                    }
                }
            }, 300);  // 300ms debounce window
        }

        /// <summary>
        /// Valida el nombre en tiempo real (debounced, 300ms).
        /// </summary>
        private void TxtNombre_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;

            // Detectar cambio (enter edit mode si es necesario)
            if (!_isEditing && _productoActual != null)
            {
                _isEditing = true;
                ActualizarEstadoEdicion(true);
            }

            // ✅ PASO 2: Real-time validation with debounce
            ValidationDebouncer.StartDebounce(() =>
            {
                var nombre = txtNombre.Text.Trim();

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    SetError(txtNombre, "Nombre obligatorio");
                }
                else if (nombre.Length < 4)
                {
                    SetError(txtNombre, "Mín. 4 caracteres");
                }
                else
                {
                    ClearError(txtNombre);
                }
            }, 300);
        }

        /// <summary>
        /// Validación adicional al perder foco (Leave event).
        /// Complementa validación en tiempo real para casos específicos.
        /// </summary>
        private void TxtCodigo_Leave(object sender, EventArgs e)
        {
            if (_isLoading || _container == null) return;

            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
                return;

            var codigo = txtCodigo.Text.Trim().ToUpperInvariant();
            var idProd = _productoActual?.IdProd ?? 0;

            _prodRepo ??= _container.UnitOfWork.GetCustomRepository<IProdRepository>();
            var existsResult = _prodRepo.ExistsByCodigo(_container.IdTenant, codigo);
            if (existsResult.IsSuccess && existsResult.Value)
            {
                if (idProd == 0)
                {
                    SetError(txtCodigo, "Código ya existe");
                    NotificationManager.Warning("Ya existe un producto con este código.");
                }
                else
                {
                    var otro = _prodRepo.GetByCodigo(_container.IdTenant, codigo);
                    if (otro.IsSuccess && otro.Value != null && otro.Value.IdProd != idProd)
                    {
                        SetError(txtCodigo, "Código duplicado");
                        NotificationManager.Warning("Ya existe otro producto con este código.");
                    }
                    else
                    {
                        ClearError(txtCodigo);
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Botones "+" para abrir mantenedores
        // ─────────────────────────────────────────────────────────────────────────
        private void BtnAgregarCategoria_Click(object sender, EventArgs e)
        {
            if (_container == null) return;
            var result = _container.FormFactory.ShowDialog<frm_Categoria>();
            if (result == DialogResult.OK)
            {
                // Recargar combos y mantener la selección actual si es posible
                var idSeleccionado = cboCategoria.SelectedValue as long?;
                CargarCombos();
                if (idSeleccionado.HasValue && cboCategoria.Items.Cast<Categoria>().Any(c => c.IdCategoria == idSeleccionado.Value))
                    cboCategoria.SelectedValue = idSeleccionado.Value;
            }
        }

        private void BtnAgregarMarca_Click(object sender, EventArgs e)
        {
            if (_container == null) return;
            var result = _container.FormFactory.ShowDialog<frm_Marca>();
            if (result == DialogResult.OK)
            {
                var idSeleccionado = cboMarca.SelectedValue as int?;
                CargarCombos();
                if (idSeleccionado.HasValue && cboMarca.Items.Cast<Marca>().Any(m => m.IdMarca == idSeleccionado.Value))
                    cboMarca.SelectedValue = idSeleccionado.Value;
            }
        }

        private void BtnAgregarUnidadBase_Click(object sender, EventArgs e)
        {
            if (_container == null) return;
            var result = _container.FormFactory.ShowDialog<frm_Unidad>();
            if (result == DialogResult.OK)
            {
                var idSeleccionado = cboUnidadBase.SelectedValue as int?;
                CargarCombos();
                if (idSeleccionado.HasValue && cboUnidadBase.Items.Cast<Unidad>().Any(u => u.IdUnidad == idSeleccionado.Value))
                    cboUnidadBase.SelectedValue = idSeleccionado.Value;
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
                LoadProducto(e.ProductoNuevo);
                tsbEditar.Enabled = true;
                tsbEliminar.Enabled = true;
                tslEstadoProducto.Text = $"Producto: {e.ProductoNuevo.Codigo}";

                pnlAcciones.Visible = false;
            }
            else
            {
                Clear();
                tsbEditar.Enabled = false;
                tsbEliminar.Enabled = false;
                tslEstadoProducto.Text = "Sin producto seleccionado";
                    pnlAcciones.Visible = false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Eventos de botones de edición
        // ─────────────────────────────────────────────────────────────────────────
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!IsValid) return;

            if (Guardar())
            {
                NotificationManager.Success("Producto guardado correctamente.");
                EndEdit();
                pnlAcciones.Visible = false;

                // ✅ Refresh product list after successful save so user sees new/updated product
                if (_container is Inventa.Forms.Productos.frm_Producto form)
                    form.RefreshProductList();
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            CancelEdit();
            pnlAcciones.Visible = false;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Eventos ToolStrip CRUD
        // ─────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Maneja el clic del botón "Nuevo" en la ToolStrip.
        /// Crea un nuevo producto en modo edición.
        /// </summary>
        private void TsbNuevo_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;

            // Verificar si hay cambios sin guardar
            if (HasChanges && _container != null)
            {
                if (MessageBox.Show("Hay cambios sin guardar. ¿Desea guardarlos antes de crear un nuevo producto?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                if (!Guardar())
                    return;
            }

            // Limpiar y preparar para nuevo producto
            LoadProducto(null);
            BeginEdit();
            pnlAcciones.Visible = true;

            // Actualizar estado de botones
            tsbEditar.Enabled = false;
            tsbEliminar.Enabled = false;
            tslEstadoProducto.Text = "Nuevo producto";
        }

        /// <summary>
        /// Maneja el clic del botón "Editar" en la ToolStrip.
        /// Habilita modo edición para el producto actual.
        /// </summary>
        private void TsbEditar_Click(object sender, EventArgs e)
        {
            if (_productoActual == null) return;

            BeginEdit();
            pnlAcciones.Visible = true;
            tslEstadoProducto.Text = $"Editando: {_productoActual.Codigo}";
        }

        /// <summary>
        /// Maneja el clic del botón "Eliminar" en la ToolStrip.
        /// Elimina el producto actual con confirmación.
        /// </summary>
        private void TsbEliminar_Click(object sender, EventArgs e)
        {
            if (_isLoading || _productoActual == null || _container == null) return;

            _prodRepo ??= _container.UnitOfWork.GetCustomRepository<IProdRepository>();

            // Determinar estrategia de eliminación basada en variantes
            var hasVariants = _productoActual.Variantes?.Count > 0;

            var confirmMessage = hasVariants
                ? $"El producto {_productoActual.Codigo} tiene variantes asociadas.\n\n" +
                  "Se marcará como INACTIVO para preservar el historial.\n\n" +
                  "¿Continuar?"
                : $"¿Eliminar el producto {_productoActual.Codigo} - {_productoActual.Nombre} de la base de datos?";

            if (MessageBox.Show(confirmMessage, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                if (hasVariants)
                {
                    // Estrategia 1: Soft delete — marcar como inactivo
                    _productoActual.Activo = false;
                    var updateResult = _prodRepo.Update(_productoActual);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al marcar como inactivo: {updateResult.Error?.Message}");
                        return;
                    }

                    _container.UnitOfWork.SaveChanges();
                    NotificationManager.Info(
                        $"Producto {_productoActual.Codigo} marcado como INACTIVO.\n\n" +
                        "Nota: El producto conserva sus variantes y datos asociados.");
                }
                else
                {
                    // Estrategia 2: Hard delete — eliminar de la base de datos
                    var removeResult = _prodRepo.Remove(_productoActual);
                    if (removeResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al eliminar: {removeResult.Error?.Message}");
                        return;
                    }

                    _container.UnitOfWork.SaveChanges();
                    NotificationManager.Success($"Producto {_productoActual.Codigo} eliminado de la base de datos.");
                }

                // Limpiar y notificar al contenedor
                _productoActual = null;
                Clear();

                // ✅ Refresh product list after successful operation
                if (_container is Inventa.Forms.Productos.frm_Producto form)
                    form.RefreshProductList();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error durante la operación: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var frm = this.ParentForm;
            //AntdUI.Message.success(frm, "This is a success message", Font);
        }
    }
