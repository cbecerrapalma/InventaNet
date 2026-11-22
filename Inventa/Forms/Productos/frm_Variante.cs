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
using System.Windows.Forms;

namespace Inventa.Forms.Productos
{
    public partial class frm_Variante : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFormFactory _formFactory;

        // ── Scope y UnitOfWork propios ────────────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorios (lazy) ───────────────────────────────────────────────
        private IVarProdRepository? _varProdRepo;
        private IVarAtriRepository? _varAtriRepo;
        private IProdRepository? _prodRepo;

        // ── Parámetros de entrada ──────────────────────────────────────────────
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long IdProd { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long? IdVarProd { get; set; }

        // ── Estado interno ────────────────────────────────────────────────────
        private Prod? _producto;
        private VarProd? _varianteActual;
        private bool _isLoading;
        private bool _varianteGuardada; // true cuando se persistió al menos una vez

        // Snapshots para detección de cambios
        private string? _snapCodigo;
        private string? _snapNombre;
        private decimal _snapCostoProm;
        private bool _snapActivo;

        // ── DTO aplanado para la grilla de atributos ──────────────────────────
        // FIX: IdVarAtri eliminado — VarAtri tiene PK compuesta (IdVarProd, IdAtriVal),
        // no existe ningún campo IdVarAtri en la entidad.
        // Almacenamos IdAtriVal para poder llamar a _varAtriRepo.Get(IdVarProd, IdAtriVal).
        private sealed record AtributoItem(
            long IdAtriVal,
            string Atributo,
            string Valor
        );

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_Variante(
            ITenantContext tenantContext,
            IServiceScopeFactory scopeFactory,
            IFormFactory formFactory)
        {
            InitializeComponent();
            _tenantContext = tenantContext;
            _scopeFactory = scopeFactory;
            _formFactory = formFactory;

            this.KeyPreview = true;
            this.KeyDown += Frm_Variante_KeyDown;

            ConfigurarGrid();
            ConfigurarEventos();
            this.FormClosing += (s, e) => _formScope?.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN DE GRILLA
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarGrid()
        {
            dgvAtributos.AutoGenerateColumns = false;
            dgvAtributos.Columns.Clear();

            // IdAtriVal oculto — para operaciones de Delete
            dgvAtributos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IdAtriVal",
                DataPropertyName = nameof(AtributoItem.IdAtriVal),
                Visible = false
            });
            dgvAtributos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Atributo",
                HeaderText = "Atributo",
                DataPropertyName = nameof(AtributoItem.Atributo),
                Width = 220
            });
            dgvAtributos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Valor",
                HeaderText = "Valor asignado",
                DataPropertyName = nameof(AtributoItem.Valor),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvAtributos.SelectionChanged += DgvAtributos_SelectionChanged;
        }

        private void ConfigurarEventos()
        {
            txtCodigo.TextChanged += (s, e) => { if (!_isLoading) ValidarCodigo(); };
            txtNombre.TextChanged += (s, e) => { if (!_isLoading) ValidarNombre(); };
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_Variante_Load(object sender, EventArgs e)
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

                if (IdProd <= 0)
                {
                    NotificationManager.Error("No se especifico un producto valido.");
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                _formScope = _scopeFactory.CreateScope();
                _unitOfWork = _formScope.ServiceProvider
                    .GetRequiredService<IUnitOfWorkSync>();

                _prodRepo ??= _unitOfWork.GetCustomRepository<IProdRepository>();
                var prodResult = _prodRepo.GetById(IdProd);
                if (prodResult.IsSuccess && prodResult.Value != null)
                    _producto = prodResult.Value;

                if (IdVarProd.HasValue)
                {
                    CargarVariante(IdVarProd.Value);
                    _varianteGuardada = true;
                }
                else
                {
                    LimpiarFormulario();
                }

                // FIX: CargarAtributos solo tiene sentido si hay variante persistida
                CargarAtributos();
                ActualizarTitulo();
                ActualizarEstadoBotones();
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

        private void Frm_Variante_KeyDown(object? sender, KeyEventArgs e)
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
        // CARGA DE DATOS
        // ═════════════════════════════════════════════════════════════════════

        private void CargarVariante(long idVarProd)
        {
            _varProdRepo ??= _unitOfWork!.GetCustomRepository<IVarProdRepository>();
            var result = _varProdRepo.GetById(idVarProd);
            if (!result.IsSuccess || result.Value == null)
            {
                NotificationManager.Error("No se pudo cargar la variante.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _varianteActual = result.Value;

            _isLoading = true;
            try
            {
                txtCodigo.Text = _varianteActual.Codigo;
                txtNombre.Text = _varianteActual.Nombre;
                numCostoProm.Value = _varianteActual.CostoProm > numCostoProm.Maximum
                    ? numCostoProm.Maximum
                    : _varianteActual.CostoProm;
                chkActivo.Checked = _varianteActual.Activo;
                txtCodigo.BackColor = System.Drawing.Color.White;
                txtNombre.BackColor = System.Drawing.Color.White;
                GuardarSnapshot();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void LimpiarFormulario()
        {
            _isLoading = true;
            try
            {
                _varianteActual = null;
                txtCodigo.Clear();
                txtNombre.Clear();
                numCostoProm.Value = 0;
                chkActivo.Checked = true;
                txtCodigo.BackColor = System.Drawing.Color.White;
                txtNombre.BackColor = System.Drawing.Color.White;
                GuardarSnapshot();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ActualizarTitulo()
        {
            if (_producto == null) return;
            lblProductoValor.Text = $"{_producto.Codigo}  —  {_producto.Nombre}";
        }

        // ═════════════════════════════════════════════════════════════════════
        // GRILLA DE ATRIBUTOS
        // ═════════════════════════════════════════════════════════════════════

        private void CargarAtributos()
        {
            if (_varianteActual == null)
            {
                dgvAtributos.DataSource = null;
                return;
            }

            _varAtriRepo ??= _unitOfWork!.GetCustomRepository<IVarAtriRepository>();

            // GetByVarianteConDetalle incluye AtriVal → AtriDef (necesario para Nombre)
            var result = _varAtriRepo.GetByVarianteConDetalle(_varianteActual.IdVarProd);
            if (!result.IsSuccess)
            {
                NotificationManager.Warning($"Error al cargar atributos: {result.Error?.Message}");
                return;
            }

            var lista = result.Value
                .Where(va => va.AtriVal != null && va.AtriVal.AtriDef != null)
                .OrderBy(va => va.AtriVal!.AtriDef!.Nombre)
                .ThenBy(va => va.AtriVal!.Nombre)
                .Select(va => new AtributoItem(
                    IdAtriVal: va.IdAtriVal,
                    Atributo: va.AtriVal!.AtriDef!.Nombre,
                    Valor: va.AtriVal!.Nombre
                ))
                .ToList();

            dgvAtributos.DataSource = null;
            dgvAtributos.DataSource = lista;

            ActualizarContadorAtributos(lista.Count);
        }

        private void ActualizarContadorAtributos(int total)
        {
            tsLblAtributos.Text = total > 0
                ? $"{total} atributo(s) asignado(s)"
                : "Sin atributos asignados";
        }

        // ═════════════════════════════════════════════════════════════════════
        // VALIDACIONES EN TIEMPO REAL
        // ═════════════════════════════════════════════════════════════════════

        private bool ValidarCodigo()
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                txtCodigo.BackColor = System.Drawing.Color.LightPink;
                return false;
            }
            txtCodigo.BackColor = System.Drawing.Color.White;
            return true;
        }

        private bool ValidarNombre()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                txtNombre.BackColor = System.Drawing.Color.LightPink;
                return false;
            }
            txtNombre.BackColor = System.Drawing.Color.White;
            return true;
        }

        private void TxtCodigo_Leave(object? sender, EventArgs e)
        {
            if (_isLoading || !ValidarCodigo()) return;

            _varProdRepo ??= _unitOfWork!.GetCustomRepository<IVarProdRepository>();
            var existe = _varProdRepo.ExistsByCodigo(IdProd, txtCodigo.Text.Trim());
            if (!existe.IsSuccess || !existe.Value) return;

            // Solo es conflicto si es otro registro diferente
            if (_varianteActual == null ||
                !string.Equals(_varianteActual.Codigo, txtCodigo.Text.Trim(),
                               System.StringComparison.OrdinalIgnoreCase))
                txtCodigo.BackColor = System.Drawing.Color.LightPink;
        }

        // ═════════════════════════════════════════════════════════════════════
        // VALIDACIÓN COMPLETA
        // ═════════════════════════════════════════════════════════════════════

        private bool ValidarDatos()
        {
            bool esValido = true;
            var errores = new System.Text.StringBuilder();

            if (!ValidarCodigo())
            {
                errores.AppendLine("• El codigo es obligatorio.");
                esValido = false;
            }
            else
            {
                _varProdRepo ??= _unitOfWork!.GetCustomRepository<IVarProdRepository>();
                var existe = _varProdRepo.ExistsByCodigo(IdProd, txtCodigo.Text.Trim());
                if (existe.IsSuccess && existe.Value)
                {
                    bool mismoRegistro = _varianteActual != null &&
                        string.Equals(_varianteActual.Codigo, txtCodigo.Text.Trim(),
                                      System.StringComparison.OrdinalIgnoreCase);
                    if (!mismoRegistro)
                    {
                        errores.AppendLine("• Ya existe una variante con este codigo para el producto.");
                        txtCodigo.BackColor = System.Drawing.Color.LightPink;
                        esValido = false;
                    }
                }
            }

            if (!ValidarNombre())
            {
                errores.AppendLine("• El nombre es obligatorio.");
                esValido = false;
            }

            if (numCostoProm.Value < 0)
            {
                errores.AppendLine("• El costo promedio no puede ser negativo.");
                esValido = false;
            }

            if (!esValido)
                NotificationManager.Warning($"Por favor corrija los siguientes errores:\n\n{errores}");

            return esValido;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CRUD VARIANTE
        // ═════════════════════════════════════════════════════════════════════

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;

            _varProdRepo ??= _unitOfWork!.GetCustomRepository<IVarProdRepository>();

            try
            {
                bool esNueva = _varianteActual == null;

                if (esNueva)
                {
                    var nueva = new VarProd
                    {
                        IdProd = IdProd,
                        Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                        Nombre = txtNombre.Text.Trim(),
                        CostoProm = numCostoProm.Value,
                        Activo = chkActivo.Checked
                    };

                    var addResult = _varProdRepo.Add(nueva);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al agregar variante: {addResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    _varianteActual = nueva;
                    _varianteGuardada = true;

                    // FIX: mensaje correcto — esNueva capturado antes de asignar _varianteActual
                    NotificationManager.Success("Variante creada. Ahora puede gestionar sus atributos.");
                }
                else
                {
                    _varianteActual!.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                    _varianteActual!.Nombre = txtNombre.Text.Trim();
                    _varianteActual!.CostoProm = numCostoProm.Value;
                    _varianteActual!.Activo = chkActivo.Checked;

                    var updateResult = _varProdRepo.Update(_varianteActual);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar variante: {updateResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Variante actualizada.");
                }

                GuardarSnapshot();

                // Recargar atributos y actualizar botones (agregar se habilita tras guardar)
                CargarAtributos();
                ActualizarEstadoBotones();
                ActualizarTitulo();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al guardar: {ex.Message}");
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // GESTIÓN DE ATRIBUTOS
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Abre frm_ConfigAtributos para gestionar todos los atributos de la variante.
        /// FIX: reemplaza el stub con _notification.Info (método inexistente).
        /// </summary>
        private void BtnAgregarAtributo_Click(object sender, EventArgs e)
        {
            if (_varianteActual == null)
            {
                NotificationManager.Warning("Debe guardar la variante antes de gestionar sus atributos.");
                return;
            }

            var result = _formFactory.ShowDialog<frm_ConfigAtributos>(form =>
            {
                form.IdVarProd = _varianteActual.IdVarProd;
            });

            if (result == DialogResult.OK)
                CargarAtributos();
        }

        /// <summary>
        /// Editar atributos — abre frm_ConfigAtributos igual que Agregar.
        /// FIX: reemplaza el stub con _notification.Info (método inexistente).
        /// En frm_ConfigAtributos el usuario puede editar todos los atributos de la variante.
        /// </summary>
        private void BtnEditarAtributo_Click(object sender, EventArgs e)
        {
            if (_varianteActual == null) return;

            var result = _formFactory.ShowDialog<frm_ConfigAtributos>(form =>
            {
                form.IdVarProd = _varianteActual.IdVarProd;
            });

            if (result == DialogResult.OK)
                CargarAtributos();
        }

        /// <summary>
        /// Elimina la asignación VarAtri para el valor seleccionado en la grilla.
        /// FIX: usa _varAtriRepo.Get(IdVarProd, IdAtriVal) en lugar de GetById(IdVarAtri)
        /// porque VarAtri no tiene un campo IdVarAtri — su PK es compuesta (IdVarProd, IdAtriVal).
        /// </summary>
        private void BtnEliminarAtributo_Click(object sender, EventArgs e)
        {
            if (dgvAtributos.SelectedRows.Count == 0) return;
            if (_varianteActual == null) return;

            var item = dgvAtributos.SelectedRows[0].DataBoundItem as AtributoItem;
            if (item == null) return;

            if (MessageBox.Show(
                $"¿Desea quitar la asignacion '{item.Atributo}: {item.Valor}' de esta variante?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _varAtriRepo ??= _unitOfWork!.GetCustomRepository<IVarAtriRepository>();

            // Recuperar la entidad por su PK compuesta
            var getResult = _varAtriRepo.Get(_varianteActual.IdVarProd, item.IdAtriVal);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                NotificationManager.Error("No se pudo obtener la asignacion de atributo.");
                return;
            }

            var removeResult = _varAtriRepo.Remove(getResult.Value);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar asignacion: {removeResult.Error?.Message}");
                return;
            }

            try
            {
                _unitOfWork!.SaveChanges();
                NotificationManager.Success("Asignacion de atributo eliminada.");
                CargarAtributos();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al guardar: {ex.Message}");
            }
        }

        private void BtnRefrescarAtributos_Click(object sender, EventArgs e)
        {
            CargarAtributos();
        }

        // ═════════════════════════════════════════════════════════════════════
        // BOTÓN CERRAR / CANCELAR
        // ═════════════════════════════════════════════════════════════════════

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            // Advertir si hay atributos obligatorios sin cubrir
            if (_varianteActual != null && _producto != null)
            {
                _varAtriRepo ??= _unitOfWork!.GetCustomRepository<IVarAtriRepository>();
                var cumple = _varAtriRepo.VarianteCumpleAtributosObligatorios(
                    _varianteActual.IdVarProd, _producto.IdCategoria);

                if (cumple.IsSuccess && !cumple.Value)
                {
                    if (MessageBox.Show(
                        "Esta variante no tiene todos los atributos obligatorios cubiertos.\n" +
                        "¿Desea cerrar de todas formas?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                }
            }

            DialogResult = _varianteGuardada ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar en los datos de la variante. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            // Si se guardó anteriormente, devolver OK para que el padre recargue
            DialogResult = _varianteGuardada ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        // ═════════════════════════════════════════════════════════════════════
        // EVENTOS DE GRILLA
        // ═════════════════════════════════════════════════════════════════════

        private void DgvAtributos_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarEstadoBotones();
        }

        // ═════════════════════════════════════════════════════════════════════
        // HELPERS
        // ═════════════════════════════════════════════════════════════════════

        private void GuardarSnapshot()
        {
            _snapCodigo = txtCodigo.Text;
            _snapNombre = txtNombre.Text;
            _snapCostoProm = numCostoProm.Value;
            _snapActivo = chkActivo.Checked;
        }

        private bool HayCambios()
        {
            return txtCodigo.Text != _snapCodigo ||
                   txtNombre.Text != _snapNombre ||
                   numCostoProm.Value != _snapCostoProm ||
                   chkActivo.Checked != _snapActivo;
        }

        private void ActualizarEstadoBotones()
        {
            bool hayVariante = _varianteActual != null;
            bool haySeleccion = dgvAtributos.SelectedRows.Count > 0;

            btnAgregarAtributo.Enabled = hayVariante;
            btnEditarAtributo.Enabled = hayVariante;
            btnEliminarAtributo.Enabled = hayVariante && haySeleccion;

            // Actualizar texto del botón Cerrar para claridad
            btnCerrar.Text = _varianteGuardada ? "Cerrar" : "Cerrar sin guardar";
        }
    }
}


