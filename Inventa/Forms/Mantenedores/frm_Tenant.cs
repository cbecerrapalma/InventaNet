using CBP.Data.Abstractions;
using CBP.WinFrms.Notification;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.UI;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Inventa.Forms.Mantenedores
{
    public partial class frm_Tenant : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly IServiceScopeFactory _scopeFactory;

        // ── Scope y UnitOfWork propios ────────────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorio (lazy) ────────────────────────────────────────────────
        private ITenantRepository? _tenantRepo;

        // ── Estado interno ────────────────────────────────────────────────────
        private bool _esNuevo = true;
        private bool _isLoading;
        private bool _huboCambios;

        // Snapshots para detección de cambios
        private string _snapCodigo = string.Empty;
        private string _snapNombre = string.Empty;
        private bool _snapActivo = true;

        // ── DTO aplanado para la grilla ───────────────────────────────────────
        private sealed record TenantItem(
            int IdTenant,
            string Codigo,
            string Nombre,
            bool Activo,
            DateTime FecCrea
        );

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_Tenant(
            IServiceScopeFactory scopeFactory)
        {
            InitializeComponent();
            _scopeFactory = scopeFactory;

            if (!CBP.WinFrms.Theming.DesignModeHelper.IsDesignTime())
                CBP.WinFrms.Theming.ThemeInjector.Attach(this);

            this.KeyPreview = true;
            this.KeyDown += Frm_Tenant_KeyDown;

            ConfigurarGrid();
            ConfigurarEventos();

            this.FormClosing += Frm_Tenant_FormClosing;
            this.FormClosed += (s, e) => _formScope?.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarGrid()
        {
            dgvTenants.AutoGenerateColumns = false;
            dgvTenants.Columns.Clear();

            dgvTenants.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "ID",
                DataPropertyName = nameof(TenantItem.IdTenant),
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });
            dgvTenants.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCodigo",
                HeaderText = "Codigo",
                DataPropertyName = nameof(TenantItem.Codigo),
                Width = 110
            });
            dgvTenants.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNombre",
                HeaderText = "Nombre",
                DataPropertyName = nameof(TenantItem.Nombre),
                Width = 280
            });
            dgvTenants.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colActivo",
                HeaderText = "Activo",
                DataPropertyName = nameof(TenantItem.Activo),
                Width = 62,
                ReadOnly = true
            });
            dgvTenants.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFecCrea",
                HeaderText = "F. Creacion",
                DataPropertyName = nameof(TenantItem.FecCrea),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });

            dgvTenants.RowPrePaint += DgvTenants_RowPrePaint;
            dgvTenants.SelectionChanged += DgvTenants_SelectionChanged;
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

        private void Frm_Tenant_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                _formScope = _scopeFactory.CreateScope();
                _unitOfWork = _formScope.ServiceProvider
                    .GetRequiredService<IUnitOfWorkSync>();
                _tenantRepo = _unitOfWork.GetCustomRepository<ITenantRepository>();

                CargarTenants();
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

        private void Frm_Tenant_FormClosing(object sender, FormClosingEventArgs e)
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

        private void Frm_Tenant_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                BtnGuardar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                CargarTenants();
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

        private void CargarTenants()
        {
            // ITenantRepository expone GetActivos() como única consulta de lista.
            // Para un mantenedor de tenants (superadmin) esto es suficiente:
            // los tenants inactivos son entidades archivadas que no requieren
            // administración frecuente. Si se necesita listar inactivos,
            var result = _tenantRepo!.GetActivos();
            if (!result.IsSuccess)
            {
                NotificationManager.Error($"Error al cargar tenants: {result.Error?.Message}");
                return;
            }

            var items = result.Value
                .Select(t => new TenantItem(t.IdTenant, t.Codigo, t.Nombre, t.Activo, t.FecCrea))
                .ToList();

            dgvTenants.DataSource = null;
            dgvTenants.DataSource = items;

            tsLblContador.Text = $"{items.Count} tenant(s) activo(s)";
        }

        // ═════════════════════════════════════════════════════════════════════
        // COLOREADO DE FILAS
        // ═════════════════════════════════════════════════════════════════════

        private void DgvTenants_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvTenants.Rows.Count) return;
            var item = dgvTenants.Rows[e.RowIndex].DataBoundItem as TenantItem;
            if (item == null) return;

            var row = dgvTenants.Rows[e.RowIndex];
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

        private void DgvTenants_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            ActualizarEstadoBotones();

            if (dgvTenants.SelectedRows.Count > 0)
            {
                var item = dgvTenants.SelectedRows[0].DataBoundItem as TenantItem;
                if (item != null)
                {
                    lblFechaValor.Text = item.FecCrea.ToString("dd/MM/yyyy HH:mm");
                    return;
                }
            }

            lblFechaValor.Text = "-";
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
            if (txtNombre.Text.Length < 4)
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

            var existe = _tenantRepo!.ExistsByCodigo(txtCodigo.Text.Trim());
            if (!existe.IsSuccess || !existe.Value) return;

            bool mismoRegistro = !_esNuevo &&
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
                var existe = _tenantRepo!.ExistsByCodigo(txtCodigo.Text.Trim());
                if (existe.IsSuccess && existe.Value)
                {
                    bool mismoRegistro = !_esNuevo &&
                        string.Equals(_snapCodigo, txtCodigo.Text.Trim(),
                                      System.StringComparison.OrdinalIgnoreCase);
                    if (!mismoRegistro)
                    {
                        errores.AppendLine("• Ya existe un tenant con este codigo.");
                        txtCodigo.BackColor = System.Drawing.Color.LightPink;
                        esValido = false;
                    }
                }
                else if (existe.IsFailure)
                    NotificationManager.Warning($"No se pudo verificar unicidad del codigo: {existe.Error?.Message}");
            }

            if (!ValidarNombre())
            {
                errores.AppendLine("• El nombre es obligatorio (minimo 4 caracteres).");
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
            if (_isLoading || dgvTenants.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un tenant para editar.");
                return;
            }

            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var item = dgvTenants.SelectedRows[0].DataBoundItem as TenantItem;
            if (item == null) return;

            CargarEnEdicion(item);
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_isLoading || dgvTenants.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un tenant para eliminar.");
                return;
            }

            var item = dgvTenants.SelectedRows[0].DataBoundItem as TenantItem;
            if (item == null) return;

            if (MessageBox.Show(
                $"¿Desea eliminar el tenant '{item.Nombre}'?\n\n" +
                "Esta accion eliminara todos los datos asociados a esta empresa.", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var getResult = _tenantRepo!.GetById(item.IdTenant);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                NotificationManager.Error("No se pudo obtener el tenant para eliminar.");
                return;
            }

            var removeResult = _tenantRepo.Remove(getResult.Value);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar: {removeResult.Error?.Message}");
                return;
            }

            try
            {
                _unitOfWork!.SaveChanges();
                NotificationManager.Success("Tenant eliminado correctamente.");
                _huboCambios = true;
                CargarTenants();
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
            CargarTenants();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            GuardarTenant();
        }

        private void GuardarTenant()
        {
            if (!ValidarDatos()) return;

            try
            {
                if (_esNuevo)
                {
                    var nuevo = new Tenant
                    {
                        Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                        Nombre = txtNombre.Text.Trim().ToUpperInvariant(),
                        Activo = chkActivo.Checked,
                        FecCrea = DateTime.UtcNow
                    };

                    var addResult = _tenantRepo!.Add(nuevo);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al crear tenant: {addResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Tenant creado exitosamente.");
                    _huboCambios = true;
                    CargarTenants();
                    LimpiarFormulario();
                }
                else
                {
                    if (!int.TryParse(lblIdValor.Text, out int idTenant))
                    {
                        NotificationManager.Error("ID de tenant invalido.");
                        return;
                    }

                    var getResult = _tenantRepo!.GetById(idTenant);
                    if (!getResult.IsSuccess || getResult.Value == null)
                    {
                        NotificationManager.Error($"Error al obtener tenant: {getResult.Error?.Message}");
                        return;
                    }

                    var entidad = getResult.Value;
                    entidad.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                    entidad.Nombre = txtNombre.Text.Trim().ToUpperInvariant();
                    entidad.Activo = chkActivo.Checked;

                    var updateResult = _tenantRepo.Update(entidad);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar: {updateResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Tenant actualizado exitosamente.");
                    _huboCambios = true;
                    CargarTenants();
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
            dgvTenants.ClearSelection();
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
                lblFechaValor.Text = "-";
                _esNuevo = true;
                GuardarSnapshot();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void CargarEnEdicion(TenantItem item)
        {
            _isLoading = true;
            try
            {
                txtCodigo.Text = item.Codigo;
                txtNombre.Text = item.Nombre;
                chkActivo.Checked = item.Activo;
                lblIdValor.Text = item.IdTenant.ToString();
                lblFechaValor.Text = item.FecCrea.ToString("dd/MM/yyyy HH:mm");
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
            bool haySeleccion = dgvTenants.SelectedRows.Count > 0;
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
        }
    }
}


