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

namespace Inventa.Forms.Mantenedores
{
    public partial class frm_Marca : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IServiceScopeFactory _scopeFactory;

        // ── Scope y UnitOfWork propios ────────────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorios (lazy) ───────────────────────────────────────────────
        private IMarcaRepository? _marcaRepo;
        private IProdRepository? _prodRepo;

        // ── Parámetro de tenant ───────────────────────────────────────────────
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

        // ── DTO aplanado para la grilla ───────────────────────────────────────
        private sealed record MarcaItem(
            int IdMarca,
            string Codigo,
            string Nombre,
            bool Activo,
            DateTime FecCrea
        );

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_Marca(
            IServiceScopeFactory scopeFactory,
            ITenantContext tenantContext)
        {
            InitializeComponent();
            _scopeFactory = scopeFactory;
            _tenantContext = tenantContext;

            if (!CBP.WinFrms.Theming.DesignModeHelper.IsDesignTime())
                CBP.WinFrms.Theming.ThemeInjector.Attach(this);

            this.KeyPreview = true;
            this.KeyDown += Frm_Marca_KeyDown;

            ConfigurarGrid();
            ConfigurarEventos();

            this.FormClosing += Frm_Marca_FormClosing;
            this.FormClosed += (s, e) => _formScope?.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarGrid()
        {
            dgvMarcas.AutoGenerateColumns = false;
            dgvMarcas.Columns.Clear();

            dgvMarcas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                DataPropertyName = nameof(MarcaItem.IdMarca),
                Visible = false
            });
            dgvMarcas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCodigo",
                HeaderText = "Codigo",
                DataPropertyName = nameof(MarcaItem.Codigo),
                Width = 160
            });
            dgvMarcas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNombre",
                HeaderText = "Nombre",
                DataPropertyName = nameof(MarcaItem.Nombre),
                Width = 280
            });
            dgvMarcas.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colActivo",
                HeaderText = "Activo",
                DataPropertyName = nameof(MarcaItem.Activo),
                Width = 62,
                ReadOnly = true
            });
            dgvMarcas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFecCrea",
                HeaderText = "F. Creacion",
                DataPropertyName = nameof(MarcaItem.FecCrea),
                Width = 130,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });

            dgvMarcas.RowPrePaint += DgvMarcas_RowPrePaint;
            dgvMarcas.SelectionChanged += DgvMarcas_SelectionChanged;
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

        private void Frm_Marca_Load(object sender, EventArgs e)
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
                _marcaRepo = _unitOfWork.GetCustomRepository<IMarcaRepository>();
                _prodRepo = _unitOfWork.GetCustomRepository<IProdRepository>();

                CargarMarcas();
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
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_Marca_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea cerrar de todas formas?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            if (DialogResult == DialogResult.None)
                DialogResult = _huboCambios ? DialogResult.OK : DialogResult.Cancel;
        }

        // ═════════════════════════════════════════════════════════════════════
        // ATAJOS DE TECLADO
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_Marca_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                BtnGuardar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                CargarMarcas();
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

        private void CargarMarcas()
        {
            var result = _marcaRepo!.GetByTenant(IdTenant, soloActivas: false);
            if (!result.IsSuccess)
            {
                NotificationManager.Error($"Error al cargar marcas: {result.Error?.Message}");
                return;
            }

            var items = result.Value
                .OrderBy(m => m.Nombre)
                .Select(m => new MarcaItem(m.IdMarca, m.Codigo, m.Nombre, m.Activo, m.FecCrea))
                .ToList();

            dgvMarcas.DataSource = null;
            dgvMarcas.DataSource = items;

            ActualizarContador(items.Count);
        }

        private void ActualizarContador(int total)
        {
            int activas = dgvMarcas.Rows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as MarcaItem)
                .Count(m => m?.Activo == true);
            tsLblContador.Text = $"{activas} activas / {total} total";
        }

        // ═════════════════════════════════════════════════════════════════════
        // COLOREADO DE FILAS
        // ═════════════════════════════════════════════════════════════════════

        private void DgvMarcas_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvMarcas.Rows.Count) return;
            var item = dgvMarcas.Rows[e.RowIndex].DataBoundItem as MarcaItem;
            if (item == null) return;

            var row = dgvMarcas.Rows[e.RowIndex];
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

        private void DgvMarcas_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            ActualizarEstadoBotones();

            if (dgvMarcas.SelectedRows.Count > 0)
            {
                var item = dgvMarcas.SelectedRows[0].DataBoundItem as MarcaItem;
                if (item != null)
                {
                    lblFechaCreacionValor.Text = item.FecCrea.ToString("dd/MM/yyyy HH:mm");
                    return;
                }
            }

            lblFechaCreacionValor.Text = "-";
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

            var existe = _marcaRepo!.ExistsByCodigo(IdTenant, txtCodigo.Text.Trim());
            if (!existe.IsSuccess || !existe.Value) return;

            bool mismoRegistro = !_esNuevo && _snapCodigo != null &&
                string.Equals(_snapCodigo, txtCodigo.Text.Trim(),
                              System.StringComparison.OrdinalIgnoreCase);
            if (!mismoRegistro)
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
                var existe = _marcaRepo!.ExistsByCodigo(IdTenant, txtCodigo.Text.Trim());
                if (existe.IsSuccess && existe.Value)
                {
                    bool mismoRegistro = !_esNuevo && _snapCodigo != null &&
                        string.Equals(_snapCodigo, txtCodigo.Text.Trim(),
                                      System.StringComparison.OrdinalIgnoreCase);
                    if (!mismoRegistro)
                    {
                        errores.AppendLine("• Ya existe una marca con este codigo en la empresa.");
                        txtCodigo.BackColor = System.Drawing.Color.LightPink;
                        esValido = false;
                    }
                }
                else if (existe.IsFailure)
                    NotificationManager.Warning($"No se pudo verificar unicidad del codigo: {existe.Error?.Message}");
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
            if (_isLoading || dgvMarcas.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar una marca para editar.");
                return;
            }

            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var item = dgvMarcas.SelectedRows[0].DataBoundItem as MarcaItem;
            if (item == null) return;

            CargarEnEdicion(item);
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_isLoading || dgvMarcas.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar una marca para eliminar.");
                return;
            }

            var item = dgvMarcas.SelectedRows[0].DataBoundItem as MarcaItem;
            if (item == null) return;

            // Guard: verificar si la marca tiene productos asociados
            var prodResult = _prodRepo!.GetByMarca(IdTenant, item.IdMarca, soloActivos: false);
            if (prodResult.IsSuccess && prodResult.Value.Count > 0)
            {
                NotificationManager.Warning(
                    $"No se puede eliminar '{item.Nombre}' porque tiene " +
                    $"{prodResult.Value.Count} producto(s) asociado(s).\n" +
                    "Puede desactivarla en su lugar.");
                return;
            }

            if (MessageBox.Show($"¿Desea eliminar la marca '{item.Nombre}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var getResult = _marcaRepo!.GetById(item.IdMarca);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                NotificationManager.Error("No se pudo obtener la marca para eliminar.");
                return;
            }

            var removeResult = _marcaRepo.Remove(getResult.Value);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar: {removeResult.Error?.Message}");
                return;
            }

            try
            {
                _unitOfWork!.SaveChanges();
                NotificationManager.Success("Marca eliminada correctamente.");
                _huboCambios = true;
                CargarMarcas();
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
            CargarMarcas();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            GuardarMarca();
        }

        private void GuardarMarca()
        {
            if (!ValidarDatos()) return;

            try
            {
                if (_esNuevo)
                {
                    var nueva = new Marca
                    {
                        IdTenant = IdTenant,
                        Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                        Nombre = txtNombre.Text.Trim().ToUpperInvariant(),
                        Activo = chkActivo.Checked,
                        FecCrea = DateTime.UtcNow
                    };

                    var addResult = _marcaRepo!.Add(nueva);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al crear marca: {addResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Marca creada exitosamente.");
                    _huboCambios = true;
                    CargarMarcas();
                    LimpiarFormulario();
                }
                else
                {
                    if (!int.TryParse(lblIdValor.Text, out int idMarca))
                    {
                        NotificationManager.Error("ID de marca invalido.");
                        return;
                    }

                    var getResult = _marcaRepo!.GetById(idMarca);
                    if (!getResult.IsSuccess || getResult.Value == null)
                    {
                        NotificationManager.Error($"Error al obtener marca: {getResult.Error?.Message}");
                        return;
                    }

                    var entidad = getResult.Value;
                    entidad.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                    entidad.Nombre = txtNombre.Text.Trim().ToUpperInvariant();
                    entidad.Activo = chkActivo.Checked;

                    var updateResult = _marcaRepo.Update(entidad);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar: {updateResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Marca actualizada exitosamente.");
                    _huboCambios = true;
                    CargarMarcas();
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
            dgvMarcas.ClearSelection();
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
                lblFechaCreacionValor.Text = "-";
                lblIdTenantValor.Text = IdTenant.ToString();
                _esNuevo = true;
                GuardarSnapshot();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void CargarEnEdicion(MarcaItem item)
        {
            _isLoading = true;
            try
            {
                txtCodigo.Text = item.Codigo;
                txtNombre.Text = item.Nombre;
                chkActivo.Checked = item.Activo;
                lblIdValor.Text = item.IdMarca.ToString();
                lblFechaCreacionValor.Text = item.FecCrea.ToString("dd/MM/yyyy HH:mm");
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
            bool haySeleccion = dgvMarcas.SelectedRows.Count > 0;
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
        }
    }
}


