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
    /// <summary>
    /// Mantenedor CRUD de definiciones de atributo (AtriDef) para una categoría.
    /// Se abre desde frm_ConfigAtributos vía _formFactory.ShowDialog.
    /// Permite crear, editar y desactivar AtriDef, y navegar al mantenedor
    /// de valores (frm_AtributoValor) para el atributo seleccionado.
    /// </summary>
    public partial class frm_AtributoDef : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFormFactory _formFactory;

        // ── Scope y UnitOfWork propios ────────────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorios (lazy) ───────────────────────────────────────────────
        private IAtriDefRepository? _atriDefRepo;
        private ICategoriaRepository? _categoriaRepo;

        // ── Parámetro de entrada ──────────────────────────────────────────────
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long IdCategoria { get; set; }

        // ── Estado interno ────────────────────────────────────────────────────
        private bool _esNuevo = true;
        private AtriDef? _defEnEdicion;   // entidad cacheada — evita GetById extra en Guardar
        private bool _cargando;
        private bool _huboCambios;    // para DialogResult.OK al cerrar

        // Snapshots para detección de cambios
        private string? _snapCodigo;
        private string? _snapNombre;
        private bool _snapEsOblig;
        private bool _snapActivo;

        // ── DTO aplanado para la grilla ───────────────────────────────────────
        private sealed record DefItem(
            long IdAtriDef,
            string Codigo,
            string Nombre,
            bool EsOblig,
            bool Activo,
            int CantValores
        );

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_AtributoDef(
            ITenantContext tenantContext,
            IServiceScopeFactory scopeFactory,
            IFormFactory formFactory)
        {
            InitializeComponent();
            _tenantContext = tenantContext;
            _scopeFactory = scopeFactory;
            _formFactory = formFactory;

            this.KeyPreview = true;
            this.KeyDown += Frm_AtributoDef_KeyDown;

            ConfigurarGrid();
            ConfigurarEventos();

            this.FormClosing += (s, e) => _formScope?.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarGrid()
        {
            dgvAtriDef.AutoGenerateColumns = false;
            dgvAtriDef.Columns.Clear();

            dgvAtriDef.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IdAtriDef",
                DataPropertyName = nameof(DefItem.IdAtriDef),
                Visible = false
            });
            dgvAtriDef.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Codigo",
                HeaderText = "Codigo",
                DataPropertyName = nameof(DefItem.Codigo),
                Width = 100
            });
            dgvAtriDef.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nombre",
                HeaderText = "Atributo",
                DataPropertyName = nameof(DefItem.Nombre),
                Width = 240
            });
            dgvAtriDef.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "EsOblig",
                HeaderText = "Oblig.",
                DataPropertyName = nameof(DefItem.EsOblig),
                Width = 58,
                ReadOnly = true
            });
            dgvAtriDef.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Activo",
                HeaderText = "Activo",
                DataPropertyName = nameof(DefItem.Activo),
                Width = 58,
                ReadOnly = true
            });
            dgvAtriDef.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CantValores",
                HeaderText = "Valores",
                DataPropertyName = nameof(DefItem.CantValores),
                Width = 68,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvAtriDef.RowPrePaint += DgvAtriDef_RowPrePaint;
            dgvAtriDef.SelectionChanged += DgvAtriDef_SelectionChanged;
        }

        private void ConfigurarEventos()
        {
            txtCodigo.Leave += TxtCodigo_Leave;
            txtCodigo.TextChanged += (s, e) => { if (!_cargando) ValidarCodigo(); };
            txtNombre.TextChanged += (s, e) => { if (!_cargando) ValidarNombre(); };
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_AtributoDef_Load(object sender, EventArgs e)
        {
            if (IdCategoria <= 0)
            {
                NotificationManager.Error("No se especifico una categoria valida.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            try
            {
                _formScope = _scopeFactory.CreateScope();
                _unitOfWork = _formScope.ServiceProvider
                    .GetRequiredService<IUnitOfWorkSync>();

                CargarTitulo();
                CargarAtriDef();
                LimpiarFormulario();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al inicializar: {ex.Message}");
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // ATAJOS DE TECLADO
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_AtributoDef_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                BtnGuardar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                CargarAtriDef();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                BtnCancelar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Insert)
            {
                e.SuppressKeyPress = true;
                BtnNuevo_Click(sender!, EventArgs.Empty);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA DE DATOS
        // ═════════════════════════════════════════════════════════════════════

        private void CargarTitulo()
        {
            _categoriaRepo ??= _unitOfWork!.GetCustomRepository<ICategoriaRepository>();
            var result = _categoriaRepo.GetById(IdCategoria);
            string nombreCat = result.IsSuccess && result.Value != null
                ? result.Value.Nombre
                : $"Categoria #{IdCategoria}";

            lblTitulo.Text = $"Atributos de  {nombreCat}";
            this.Text = $"Definicion de Atributos  —  {nombreCat}";
        }

        private void CargarAtriDef()
        {
            _atriDefRepo ??= _unitOfWork!.GetCustomRepository<IAtriDefRepository>();

            // Mostrar todos (activos e inactivos) para poder reactivar
            var result = _atriDefRepo.GetByCategoria(IdCategoria, soloActivos: false);
            if (!result.IsSuccess)
            {
                NotificationManager.Warning($"Error al cargar atributos: {result.Error?.Message}");
                return;
            }

            var items = result.Value.Select(d =>
            {
                var cntResult = _unitOfWork!
                    .GetCustomRepository<IAtriValRepository>()
                    .CountByAtriDef(d.IdAtriDef, soloActivos: false);
                int cnt = cntResult.IsSuccess ? cntResult.Value : 0;

                return new DefItem(
                    d.IdAtriDef, d.Codigo, d.Nombre, d.EsOblig, d.Activo, cnt);
            }).ToList();

            dgvAtriDef.DataSource = null;
            dgvAtriDef.DataSource = items;

            ActualizarContador(items.Count);
        }

        private void ActualizarContador(int total)
        {
            int activos = dgvAtriDef.Rows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as DefItem)
                .Count(d => d?.Activo == true);
            tsLblContador.Text = $"{activos} activos / {total} total";
        }

        // ═════════════════════════════════════════════════════════════════════
        // COLOREADO DE FILAS
        // ═════════════════════════════════════════════════════════════════════

        private void DgvAtriDef_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvAtriDef.Rows.Count) return;
            var item = dgvAtriDef.Rows[e.RowIndex].DataBoundItem as DefItem;
            if (item == null) return;

            var row = dgvAtriDef.Rows[e.RowIndex];
            if (!item.Activo)
            {
                row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
                row.DefaultCellStyle.ForeColor = AppColors.TextDisabled;
                row.DefaultCellStyle.SelectionBackColor = AppColors.BorderLight;
                row.DefaultCellStyle.SelectionForeColor = AppColors.TextSecondary;
            }
            else
            {
                row.DefaultCellStyle.BackColor = System.Drawing.Color.White;
                row.DefaultCellStyle.ForeColor = AppColors.TextSecondary;
                row.DefaultCellStyle.SelectionBackColor = AppColors.PrimaryLight;
                row.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // SELECCIÓN EN GRILLA
        // ═════════════════════════════════════════════════════════════════════

        private void DgvAtriDef_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarEstadoBotones();
        }

        private void DgvAtriDef_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                IniciarEdicion();
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

        /// <summary>
        /// Verifica unicidad del código al perder foco.
        /// </summary>
        private void TxtCodigo_Leave(object? sender, EventArgs e)
        {
            if (_cargando || !ValidarCodigo()) return;

            _atriDefRepo ??= _unitOfWork!.GetCustomRepository<IAtriDefRepository>();
            var existe = _atriDefRepo.ExistsByCodigo(IdCategoria, txtCodigo.Text.Trim());
            if (!existe.IsSuccess || !existe.Value) return;

            if (_esNuevo)
            {
                txtCodigo.BackColor = System.Drawing.Color.LightPink;
                return;
            }

            // Edición: conflicto solo si es otro AtriDef
            var otro = _atriDefRepo.GetByCodigo(IdCategoria, txtCodigo.Text.Trim());
            if (otro.IsSuccess && otro.Value != null &&
                _defEnEdicion != null &&
                otro.Value.IdAtriDef != _defEnEdicion.IdAtriDef)
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
                _atriDefRepo ??= _unitOfWork!.GetCustomRepository<IAtriDefRepository>();
                var existe = _atriDefRepo.ExistsByCodigo(IdCategoria, txtCodigo.Text.Trim());
                if (existe.IsSuccess && existe.Value)
                {
                    if (_esNuevo)
                    {
                        errores.AppendLine("• Ya existe un atributo con ese codigo en esta categoria.");
                        txtCodigo.BackColor = System.Drawing.Color.LightPink;
                        esValido = false;
                    }
                    else
                    {
                        var otro = _atriDefRepo.GetByCodigo(IdCategoria, txtCodigo.Text.Trim());
                        if (otro.IsSuccess && otro.Value != null &&
                            _defEnEdicion != null &&
                            otro.Value.IdAtriDef != _defEnEdicion.IdAtriDef)
                        {
                            errores.AppendLine("• Ya existe otro atributo con ese codigo en esta categoria.");
                            txtCodigo.BackColor = System.Drawing.Color.LightPink;
                            esValido = false;
                        }
                    }
                }
            }

            if (!ValidarNombre())
            {
                errores.AppendLine("• El nombre es obligatorio.");
                esValido = false;
            }

            if (!esValido)
                NotificationManager.Warning($"Por favor corrija los siguientes errores:\n\n{errores}");

            return esValido;
        }

        // ═════════════════════════════════════════════════════════════════════
        // OPERACIONES CRUD
        // ═════════════════════════════════════════════════════════════════════

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            if (HayCambios() &&
                !_notification.Confirm("Hay cambios sin guardar. ¿Desea descartarlos?"))
                return;

            LimpiarFormulario();
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (dgvAtriDef.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un atributo para editar.");
                return;
            }

            if (HayCambios() &&
                !_notification.Confirm("Hay cambios sin guardar. ¿Desea descartarlos?"))
                return;

            IniciarEdicion();
        }

        private void IniciarEdicion()
        {
            if (dgvAtriDef.SelectedRows.Count == 0) return;

            var item = dgvAtriDef.SelectedRows[0].DataBoundItem as DefItem;
            if (item == null) return;

            _atriDefRepo ??= _unitOfWork!.GetCustomRepository<IAtriDefRepository>();
            var result = _atriDefRepo.GetById(item.IdAtriDef);
            if (!result.IsSuccess || result.Value == null)
            {
                NotificationManager.Error("No se pudo cargar el atributo para edicion.");
                return;
            }

            _cargando = true;
            try
            {
                _defEnEdicion = result.Value;
                _esNuevo = false;

                txtCodigo.Text = _defEnEdicion.Codigo;
                txtNombre.Text = _defEnEdicion.Nombre;
                chkEsOblig.Checked = _defEnEdicion.EsOblig;
                chkActivo.Checked = _defEnEdicion.Activo;

                txtCodigo.BackColor = System.Drawing.Color.White;
                txtNombre.BackColor = System.Drawing.Color.White;

                GuardarSnapshot();
            }
            finally
            {
                _cargando = false;
            }

            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;

            _atriDefRepo ??= _unitOfWork!.GetCustomRepository<IAtriDefRepository>();

            try
            {
                if (_esNuevo)
                {
                    var nuevo = new AtriDef
                    {
                        IdCategoria = IdCategoria,
                        Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                        Nombre = txtNombre.Text.Trim(),
                        EsOblig = chkEsOblig.Checked,
                        Activo = chkActivo.Checked
                    };

                    var addResult = _atriDefRepo.Add(nuevo);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al crear atributo: {addResult.Error?.Message}");
                        return;
                    }
                }
                else
                {
                    if (_defEnEdicion == null)
                    {
                        NotificationManager.Error("No hay atributo en edicion.");
                        return;
                    }

                    _defEnEdicion.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                    _defEnEdicion.Nombre = txtNombre.Text.Trim();
                    _defEnEdicion.EsOblig = chkEsOblig.Checked;
                    _defEnEdicion.Activo = chkActivo.Checked;

                    var updResult = _atriDefRepo.Update(_defEnEdicion);
                    if (updResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar atributo: {updResult.Error?.Message}");
                        return;
                    }
                }

                _unitOfWork!.SaveChanges();
                _notification.Success(_esNuevo
                    ? "Atributo creado exitosamente."
                    : "Atributo actualizado exitosamente.");

                _huboCambios = true;
                CargarAtriDef();
                LimpiarFormulario();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error inesperado: {ex.Message}");
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvAtriDef.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un atributo para eliminar.");
                return;
            }

            var item = dgvAtriDef.SelectedRows[0].DataBoundItem as DefItem;
            if (item == null) return;

            _atriDefRepo ??= _unitOfWork!.GetCustomRepository<IAtriDefRepository>();

            // Verificar si tiene valores definidos — eliminación en cascada podría
            // afectar variantes que ya usan esos valores (VarAtri)
            var tieneValores = _atriDefRepo.TieneValores(item.IdAtriDef);
            if (tieneValores.IsSuccess && tieneValores.Value)
            {
                if (!_notification.Confirm(
                    $"El atributo '{item.Nombre}' tiene valores definidos.\n" +
                    "Eliminarlo tambien eliminara todos sus valores y las asignaciones " +
                    "en variantes que los usen.\n\n" +
                    "¿Desea continuar de todas formas?\n" +
                    "(Alternativa: desactivarlo con Activo = false)"))
                    return;
            }
            else
            {
                if (!_notification.Confirm(
                    $"¿Desea eliminar el atributo '{item.Nombre}'?"))
                    return;
            }

            var getResult = _atriDefRepo.GetById(item.IdAtriDef);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                NotificationManager.Error("No se pudo obtener el atributo para eliminar.");
                return;
            }

            var removeResult = _atriDefRepo.Remove(getResult.Value);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar: {removeResult.Error?.Message}");
                return;
            }

            try
            {
                _unitOfWork!.SaveChanges();
                _notification.Success("Atributo eliminado.");
                _huboCambios = true;
                CargarAtriDef();
                LimpiarFormulario();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al guardar: {ex.Message}");
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (HayCambios() &&
                !_notification.Confirm("Hay cambios sin guardar. ¿Desea descartarlos?"))
                return;

            LimpiarFormulario();
            dgvAtriDef.ClearSelection();
            ActualizarEstadoBotones();
        }

        /// <summary>
        /// Abre frm_AtributoValor para gestionar los valores del atributo seleccionado.
        /// </summary>
        private void BtnGestionarValores_Click(object sender, EventArgs e)
        {
            if (dgvAtriDef.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un atributo para gestionar sus valores.");
                return;
            }

            var item = dgvAtriDef.SelectedRows[0].DataBoundItem as DefItem;
            if (item == null) return;

            var result = _formFactory.ShowDialog<frm_AtributoValor>(form =>
            {
                form.IdAtriDef = item.IdAtriDef;
            });

            // Recargar para actualizar el contador de valores
            if (result == DialogResult.OK)
            {
                _huboCambios = true;
                CargarAtriDef();
            }
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            CargarAtriDef();
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            DialogResult = _huboCambios ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        // ═════════════════════════════════════════════════════════════════════
        // HELPERS DE ESTADO
        // ═════════════════════════════════════════════════════════════════════

        private void LimpiarFormulario()
        {
            _cargando = true;
            try
            {
                txtCodigo.Clear();
                txtNombre.Clear();
                chkEsOblig.Checked = false;
                chkActivo.Checked = true;
                txtCodigo.BackColor = System.Drawing.Color.White;
                txtNombre.BackColor = System.Drawing.Color.White;
                _esNuevo = true;
                _defEnEdicion = null;
                GuardarSnapshot();
            }
            finally
            {
                _cargando = false;
            }
        }

        private void GuardarSnapshot()
        {
            _snapCodigo = txtCodigo.Text;
            _snapNombre = txtNombre.Text;
            _snapEsOblig = chkEsOblig.Checked;
            _snapActivo = chkActivo.Checked;
        }

        private bool HayCambios()
        {
            if (_esNuevo)
                return !string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                       !string.IsNullOrWhiteSpace(txtNombre.Text);

            return txtCodigo.Text != _snapCodigo ||
                   txtNombre.Text != _snapNombre ||
                   chkEsOblig.Checked != _snapEsOblig ||
                   chkActivo.Checked != _snapActivo;
        }

        private void ActualizarEstadoBotones()
        {
            bool haySeleccion = dgvAtriDef.SelectedRows.Count > 0;
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
            btnGestionarValores.Enabled = haySeleccion;
        }
    }
}


