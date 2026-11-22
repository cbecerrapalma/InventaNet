using CBP.Data.Abstractions;
using CBP.MultiTenant.Abstractions;
using CBP.MultiTenant.Extensions;
using CBP.Services.Sync;
using CBP.WinFrms.Notification;
using Inventa.Application.Dtos;
using Inventa.Application.Services;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.UI;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Inventa.Forms.Mantenedores
{
    public partial class frm_Categoria : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IServiceScopeFactory _scopeFactory;

        // ── Scope y UnitOfWork propios ────────────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Servicio (lazy) ──────────────────────────────────────────────────
        private ICategoriaService? _categoriaService;

        // ── Parámetro/estado de tenant ────────────────────────────────────────
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IdTenant { get; set; }

        // ── Estado interno ────────────────────────────────────────────────────
        private bool _esNuevo = true;
        private bool _isLoading;
        private bool _huboCambios;

        // Snapshots para detección de cambios
        private string? _snapCodigo;
        private string? _snapNombre;
        private bool _snapActivo;

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_Categoria(
            IServiceScopeFactory scopeFactory,
            ITenantContext tenantContext)
        {
            InitializeComponent();
            _scopeFactory = scopeFactory;
            _tenantContext = tenantContext;

            if (!CBP.WinFrms.Theming.DesignModeHelper.IsDesignTime())
                CBP.WinFrms.Theming.ThemeInjector.Attach(this);

            this.KeyPreview = true;
            this.KeyDown += Frm_Categoria_KeyDown;

            ConfigurarGrid();
            ConfigurarEventos();

            // FIX: FormClosing síncrono simple — elimina todo el patrón
            // async/fire-and-forget (_cierreIniciado/_cierreEnCurso/CerrarFormularioAsync)
            // que era innecesario al usar IUnitOfWorkSync.
            this.FormClosing += Frm_Categoria_FormClosing;
            this.FormClosed += (s, e) => _formScope?.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarGrid()
        {
            dgvCategorias.AutoGenerateColumns = false;
            dgvCategorias.Columns.Clear();

            dgvCategorias.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "ID",
                DataPropertyName = nameof(CategoriaDto.IdCategoria),
                Visible = false
            });
            dgvCategorias.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCodigo",
                HeaderText = "Codigo",
                DataPropertyName = nameof(CategoriaDto.Codigo),
                Width = 160
            });
            dgvCategorias.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNombre",
                HeaderText = "Nombre",
                DataPropertyName = nameof(CategoriaDto.Nombre),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dgvCategorias.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colActivo",
                HeaderText = "Activo",
                DataPropertyName = nameof(CategoriaDto.Activo),
                Width = 70,
                ReadOnly = true
            });

            dgvCategorias.RowPrePaint += DgvCategorias_RowPrePaint;
            dgvCategorias.SelectionChanged += DgvCategorias_SelectionChanged;
        }

        private void ConfigurarEventos()
        {
            txtCodigo.TextChanged += (s, e) => { if (!_isLoading) ValidarCodigo(); };
            txtNombre.TextChanged += (s, e) => { if (!_isLoading) ValidarNombre(); };
            txtCodigo.Leave += TxtCodigo_Leave;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_Categoria_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                var tenantResult = _tenantContext.TryGetCurrent();
                if (tenantResult.IsFailure)
                {
                    NotificationManager.Error("No hay empresa activa. Reinicie la aplicacion.");
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                IdTenant = tenantResult.Value.Id;
                lblTenantValor.Text = tenantResult.Value.Nombre;
                lblIdTenantValor.Text = IdTenant.ToString();

                _formScope = _scopeFactory.CreateScope();
                _unitOfWork = _formScope.ServiceProvider
                    .GetRequiredService<IUnitOfWorkSync>();
                _categoriaService = _formScope.GetCustomService<ICategoriaService>();

                CargarCategorias();
                LimpiarFormulario();
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
        // CIERRE DEL FORMULARIO — síncrono simple
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_Categoria_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea cerrar de todas formas?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            // DialogResult ya tiene el valor correcto asignado por cada botón.
            // Si el usuario cierra con la X sin haber guardado nada, forzamos Cancel.
            if (DialogResult == DialogResult.None)
                DialogResult = _huboCambios ? DialogResult.OK : DialogResult.Cancel;
        }

        // ═════════════════════════════════════════════════════════════════════
        // ATAJOS DE TECLADO
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_Categoria_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                BtnGuardar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                CargarCategorias();
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

        private void CargarCategorias()
        {
            var result = _categoriaService!.GetByTenant(IdTenant, soloActivas: false);
            if (!result.IsSuccess)
            {
                NotificationManager.Error($"Error al cargar categorias: {result.Error?.Message}");
                return;
            }

            var items = result.Value
                .OrderBy(c => c.Codigo)
                .ToList();

            dgvCategorias.DataSource = null;
            dgvCategorias.DataSource = items;

            ActualizarContador(items.Count);
        }

        private void ActualizarContador(int total)
        {
            int activas = dgvCategorias.Rows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as CategoriaDto)
                .Count(c => c?.Activo == true);
            tsLblContador.Text = $"{activas} activas / {total} total";
        }

        // ═════════════════════════════════════════════════════════════════════
        // COLOREADO DE FILAS
        // ═════════════════════════════════════════════════════════════════════

        private void DgvCategorias_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvCategorias.Rows.Count) return;
            var item = dgvCategorias.Rows[e.RowIndex].DataBoundItem as CategoriaDto;
            if (item == null) return;

            var row = dgvCategorias.Rows[e.RowIndex];
            if (!item.Activo)
            {
                row.DefaultCellStyle.BackColor = AppColors.InactiveRowBackground;
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

        private void DgvCategorias_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            ActualizarEstadoBotones();

            if (dgvCategorias.SelectedRows.Count > 0)
            {
                var item = dgvCategorias.SelectedRows[0].DataBoundItem as CategoriaDto;
                if (item != null)
                {
                    lblIdCategoriaValor.Text = item.IdCategoria.ToString();
                    ActualizarTotalProductos(item.IdCategoria);
                    return;
                }
            }

            lblIdCategoriaValor.Text = "-";
            lblTotalProductosValor.Text = "-";
        }

        private void ActualizarTotalProductos(long idCategoria)
        {
            var result = _categoriaService!.CountProductosByCategoria(idCategoria);
            lblTotalProductosValor.Text = result.IsSuccess
                ? result.Value.ToString()
                : "Error";
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
            if (txtCodigo.Text.Length < 2)
            {
                txtCodigo.BackColor = System.Drawing.Color.LightYellow;
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
            if (txtNombre.Text.Length < 3)
            {
                txtNombre.BackColor = System.Drawing.Color.LightYellow;
                return false;
            }
            txtNombre.BackColor = System.Drawing.Color.White;
            return true;
        }

        private void TxtCodigo_Leave(object? sender, EventArgs e)
        {
            if (_isLoading || !ValidarCodigo()) return;

            var existe = _categoriaService!.ExistsByCodigo(IdTenant, txtCodigo.Text.Trim());
            if (!existe.IsSuccess || !existe.Value) return;

            // Solo es conflicto si es otro registro
            if (!_esNuevo && _snapCodigo != null &&
                string.Equals(_snapCodigo, txtCodigo.Text.Trim(),
                              System.StringComparison.OrdinalIgnoreCase))
                return;

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
                errores.AppendLine("• El codigo es obligatorio (minimo 2 caracteres).");
                esValido = false;
            }
            else
            {
            var existe = _categoriaService!.ExistsByCodigo(IdTenant, txtCodigo.Text.Trim());
                if (existe.IsSuccess && existe.Value)
                {
                    bool mismoRegistro = !_esNuevo && _snapCodigo != null &&
                        string.Equals(_snapCodigo, txtCodigo.Text.Trim(),
                                      System.StringComparison.OrdinalIgnoreCase);
                    if (!mismoRegistro)
                    {
                        errores.AppendLine("• Ya existe una categoria con este codigo en la empresa.");
                        txtCodigo.BackColor = System.Drawing.Color.LightPink;
                        esValido = false;
                    }
                }
                else if (existe.IsFailure)
                {
                    NotificationManager.Warning($"No se pudo verificar unicidad del codigo: {existe.Error?.Message}");
                }
            }

            if (!ValidarNombre())
            {
                errores.AppendLine("• El nombre es obligatorio (minimo 3 caracteres).");
                esValido = false;
            }

            if (!esValido)
                NotificationManager.Warning($"Por favor corrija los siguientes errores:\n\n{errores}");

            return esValido;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CRUD
        // ═════════════════════════════════════════════════════════════════════

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;

            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            LimpiarFormulario();
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (_isLoading || dgvCategorias.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar una categoria para editar.");
                return;
            }

            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var item = dgvCategorias.SelectedRows[0].DataBoundItem as CategoriaDto;
            if (item == null) return;

            CargarEnEdicion(item);
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_isLoading || dgvCategorias.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar una categoria para eliminar.");
                return;
            }

            var item = dgvCategorias.SelectedRows[0].DataBoundItem as CategoriaDto;
            if (item == null) return;

            // Verificar productos asociados antes de eliminar
            var countResult = _categoriaService!.CountProductosByCategoria(item.IdCategoria);
            if (countResult.IsSuccess && countResult.Value > 0)
            {
                NotificationManager.Warning(
                    $"No se puede eliminar la categoria '{item.Nombre}' porque tiene " +
                    $"{countResult.Value} producto(s) asociado(s).\n" +
                    "Puede desactivarla en su lugar.");
                return;
            }

            if (MessageBox.Show($"¿Desea eliminar la categoria '{item.Nombre}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var dto = new CategoriaDto(item.IdCategoria, IdTenant, item.Codigo, item.Nombre, item.Activo);
            var removeResult = _categoriaService.Delete(dto);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar: {removeResult.Error?.Message}");
                return;
            }

            try
            {
                _unitOfWork!.SaveChanges();
                NotificationManager.Success("Categoria eliminada correctamente.");
                _huboCambios = true;
                CargarCategorias();
                LimpiarFormulario();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al guardar: {ex.Message}");
            }
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            CargarCategorias();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            GuardarCategoria();
        }

        private void GuardarCategoria()
        {
            if (!ValidarDatos()) return;

            try
            {
                var dto = new CategoriaDto(
                    _esNuevo ? 0 : long.Parse(lblIdValor.Text),
                    IdTenant,
                    txtCodigo.Text.Trim().ToUpperInvariant(),
                    txtNombre.Text.Trim().ToUpperInvariant(),
                    chkActivo.Checked);

                if (_esNuevo)
                {
                    var addResult = _categoriaService!.Add(dto);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al crear categoria: {addResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Categoria creada exitosamente.");
                    _huboCambios = true;
                    CargarCategorias();
                    LimpiarFormulario();
                }
                else
                {
                    var updateResult = _categoriaService!.Update(dto);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar: {updateResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Categoria actualizada exitosamente.");
                    _huboCambios = true;
                    CargarCategorias();

                    // En edición: recargar snapshot para que HayCambios vuelva a false
                    GuardarSnapshot();
                }

                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error inesperado: {ex.Message}");
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;

            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            LimpiarFormulario();
            dgvCategorias.ClearSelection();
            ActualizarEstadoBotones();
        }

        // ═════════════════════════════════════════════════════════════════════
        // HELPERS
        // ═════════════════════════════════════════════════════════════════════

        private void LimpiarFormulario()
        {
            _isLoading = true;
            try
            {
                txtCodigo.Clear();
                txtNombre.Clear();
                chkActivo.Checked = true;
                txtCodigo.BackColor = System.Drawing.Color.White;
                txtNombre.BackColor = System.Drawing.Color.White;
                lblIdValor.Text = "<< Nuevo >>";
                lblIdCategoriaValor.Text = "-";
                lblTotalProductosValor.Text = "-";
                lblIdTenantValor.Text = IdTenant.ToString();
                _esNuevo = true;
                GuardarSnapshot();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void CargarEnEdicion(CategoriaDto item)
        {
            _isLoading = true;
            try
            {
                txtCodigo.Text = item.Codigo;
                txtNombre.Text = item.Nombre;
                chkActivo.Checked = item.Activo;
                lblIdValor.Text = item.IdCategoria.ToString();
                txtCodigo.BackColor = System.Drawing.Color.White;
                txtNombre.BackColor = System.Drawing.Color.White;
                _esNuevo = false;
                GuardarSnapshot();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void GuardarSnapshot()
        {
            _snapCodigo = txtCodigo.Text;
            _snapNombre = txtNombre.Text;
            _snapActivo = chkActivo.Checked;
        }

        private bool HayCambios()
        {
            if (_esNuevo)
                return !string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                       !string.IsNullOrWhiteSpace(txtNombre.Text);

            return txtCodigo.Text != _snapCodigo ||
                   txtNombre.Text != _snapNombre ||
                   chkActivo.Checked != _snapActivo;
        }

        private void ActualizarEstadoBotones()
        {
            bool haySeleccion = dgvCategorias.SelectedRows.Count > 0;
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
        }
    }
}


