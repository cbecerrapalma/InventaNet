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
    public partial class frm_Proveedor : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IServiceScopeFactory _scopeFactory;

        // ── Scope y UnitOfWork propios ────────────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorio (lazy) ────────────────────────────────────────────────
        private IProveedorRepository? _proveedorRepo;

        // ── Parámetro de tenant ───────────────────────────────────────────────
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IdTenant { get; set; }

        // ── Estado interno ────────────────────────────────────────────────────
        private bool _esNuevo = true;
        private bool _isLoading;
        private bool _huboCambios;

        // Snapshots para detección de cambios
        // FIX: campos nullable almacenados como string vacío ("") en lugar de null
        // para que la comparación con TextBox.Text sea siempre string vs string
        private string _snapCodigo = string.Empty;
        private string _snapRazonSocial = string.Empty;
        private string _snapNombreComercial = string.Empty;
        private string _snapRUT = string.Empty;
        private string _snapDireccion = string.Empty;
        private string _snapTelefono = string.Empty;
        private string _snapEmail = string.Empty;
        private string _snapContacto = string.Empty;
        private bool _snapActivo = true;

        // ── DTO aplanado para la grilla ───────────────────────────────────────
        private sealed record ProveedorItem(
            int IdProveedor,
            string Codigo,
            string RazonSocial,
            string? RUT,
            string? Telefono,
            string? Email,
            bool Activo,
            DateTime FecCrea
        );

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_Proveedor(
            IServiceScopeFactory scopeFactory,
            ITenantContext tenantContext)
        {
            InitializeComponent();
            _scopeFactory = scopeFactory;
            _tenantContext = tenantContext;

            if (!CBP.WinFrms.Theming.DesignModeHelper.IsDesignTime())
                CBP.WinFrms.Theming.ThemeInjector.Attach(this);

            this.KeyPreview = true;
            this.KeyDown += Frm_Proveedor_KeyDown;

            ConfigurarGrid();
            ConfigurarEventos();

            this.FormClosing += Frm_Proveedor_FormClosing;
            this.FormClosed += (s, e) => _formScope?.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarGrid()
        {
            dgvProveedores.AutoGenerateColumns = false;
            dgvProveedores.Columns.Clear();

            dgvProveedores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                DataPropertyName = nameof(ProveedorItem.IdProveedor),
                Visible = false
            });
            dgvProveedores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCodigo",
                HeaderText = "Codigo",
                DataPropertyName = nameof(ProveedorItem.Codigo),
                Width = 100
            });
            dgvProveedores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colRazonSocial",
                HeaderText = "Razon Social",
                DataPropertyName = nameof(ProveedorItem.RazonSocial),
                Width = 260
            });
            dgvProveedores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colRUT",
                HeaderText = "RUT",
                DataPropertyName = nameof(ProveedorItem.RUT),
                Width = 120
            });
            dgvProveedores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTelefono",
                HeaderText = "Telefono",
                DataPropertyName = nameof(ProveedorItem.Telefono),
                Width = 120
            });
            dgvProveedores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colEmail",
                HeaderText = "Email",
                DataPropertyName = nameof(ProveedorItem.Email),
                Width = 190
            });
            dgvProveedores.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colActivo",
                HeaderText = "Activo",
                DataPropertyName = nameof(ProveedorItem.Activo),
                Width = 62,
                ReadOnly = true
            });
            dgvProveedores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFecCrea",
                HeaderText = "F. Creacion",
                DataPropertyName = nameof(ProveedorItem.FecCrea),
                Width = 120,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });

            dgvProveedores.RowPrePaint += DgvProveedores_RowPrePaint;
            dgvProveedores.SelectionChanged += DgvProveedores_SelectionChanged;
        }

        private void ConfigurarEventos()
        {
            txtCodigo.TextChanged += (s, e) => { if (!_isLoading) ValidarCodigo(); };
            txtRazonSocial.TextChanged += (s, e) => { if (!_isLoading) ValidarRazonSocial(); };
            txtEmail.Leave += TxtEmail_Leave;
            txtCodigo.Leave += TxtCodigo_Leave;
            txtRUT.Leave += TxtRUT_Leave;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_Proveedor_Load(object sender, EventArgs e)
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
                _proveedorRepo = _unitOfWork.GetCustomRepository<IProveedorRepository>();

                CargarProveedores();
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

        private void Frm_Proveedor_FormClosing(object sender, FormClosingEventArgs e)
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

        private void Frm_Proveedor_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                BtnGuardar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                CargarProveedores();
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

        private void CargarProveedores()
        {
            var result = _proveedorRepo!.GetByTenant(IdTenant, soloActivos: false);
            if (!result.IsSuccess)
            {
                NotificationManager.Error($"Error al cargar proveedores: {result.Error?.Message}");
                return;
            }

            var items = result.Value
                .OrderBy(p => p.RazonSocial)
                .Select(p => new ProveedorItem(
                    p.IdProveedor, p.Codigo, p.RazonSocial,
                    p.RUT, p.Telefono, p.Email, p.Activo, p.FecCrea))
                .ToList();

            dgvProveedores.DataSource = null;
            dgvProveedores.DataSource = items;

            ActualizarContador(items.Count);
        }

        private void ActualizarContador(int total)
        {
            int activos = dgvProveedores.Rows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as ProveedorItem)
                .Count(p => p?.Activo == true);
            tsLblContador.Text = $"{activos} activos / {total} total";
        }

        // ═════════════════════════════════════════════════════════════════════
        // COLOREADO DE FILAS
        // ═════════════════════════════════════════════════════════════════════

        private void DgvProveedores_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvProveedores.Rows.Count) return;
            var item = dgvProveedores.Rows[e.RowIndex].DataBoundItem as ProveedorItem;
            if (item == null) return;

            var row = dgvProveedores.Rows[e.RowIndex];
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

        private void DgvProveedores_SelectionChanged(object sender, EventArgs e)
        {
            // FIX: guard _isLoading faltaba en el original
            if (_isLoading) return;
            ActualizarEstadoBotones();

            if (dgvProveedores.SelectedRows.Count > 0)
            {
                var item = dgvProveedores.SelectedRows[0].DataBoundItem as ProveedorItem;
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

        private bool ValidarRazonSocial()
        {
            if (string.IsNullOrWhiteSpace(txtRazonSocial.Text))
            {
                txtRazonSocial.BackColor = System.Drawing.Color.LightPink;
                return false;
            }
            if (txtRazonSocial.Text.Length < 3)
            {
                txtRazonSocial.BackColor = System.Drawing.Color.LightYellow;
                return false;
            }
            txtRazonSocial.BackColor = System.Drawing.Color.White;
            return true;
        }

        private bool ValidarEmail()
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtEmail.BackColor = System.Drawing.Color.White;
                return true;
            }

            // FIX: MailAddress normaliza la cadena (minúsculas, trim), por lo que
            // la comparación addr.Address != txtEmail.Text fallaba para emails válidos.
            // Ahora solo intentamos parsear y atrapamos la excepción.
            try
            {
                var _ = new System.Net.Mail.MailAddress(txtEmail.Text.Trim());
                txtEmail.BackColor = System.Drawing.Color.White;
                return true;
            }
            catch
            {
                txtEmail.BackColor = System.Drawing.Color.LightPink;
                return false;
            }
        }

        private void TxtEmail_Leave(object? sender, EventArgs e)
        {
            if (!_isLoading) ValidarEmail();
        }

        private void TxtCodigo_Leave(object? sender, EventArgs e)
        {
            if (_isLoading || !ValidarCodigo()) return;

            var existe = _proveedorRepo!.ExistsByCodigo(IdTenant, txtCodigo.Text.Trim());
            if (!existe.IsSuccess || !existe.Value) return;

            bool mismoRegistro = !_esNuevo &&
                string.Equals(_snapCodigo, txtCodigo.Text.Trim(),
                              System.StringComparison.OrdinalIgnoreCase);
            if (!mismoRegistro)
                txtCodigo.BackColor = System.Drawing.Color.LightPink;
        }

        private void TxtRUT_Leave(object? sender, EventArgs e)
        {
            if (_isLoading || string.IsNullOrWhiteSpace(txtRUT.Text)) return;

            var existe = _proveedorRepo!.ExistsByRUT(IdTenant, txtRUT.Text.Trim());
            if (!existe.IsSuccess || !existe.Value) return;

            bool mismoRegistro = !_esNuevo &&
                string.Equals(_snapRUT, txtRUT.Text.Trim(),
                              System.StringComparison.OrdinalIgnoreCase);
            if (!mismoRegistro)
                txtRUT.BackColor = System.Drawing.Color.LightPink;
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
                var existe = _proveedorRepo!.ExistsByCodigo(IdTenant, txtCodigo.Text.Trim());
                if (existe.IsSuccess && existe.Value)
                {
                    bool mismoRegistro = !_esNuevo &&
                        string.Equals(_snapCodigo, txtCodigo.Text.Trim(),
                                      System.StringComparison.OrdinalIgnoreCase);
                    if (!mismoRegistro)
                    {
                        errores.AppendLine("• Ya existe un proveedor con este codigo en la empresa.");
                        txtCodigo.BackColor = System.Drawing.Color.LightPink;
                        esValido = false;
                    }
                }
                else if (existe.IsFailure)
                    NotificationManager.Warning($"No se pudo verificar unicidad del codigo: {existe.Error?.Message}");
            }

            if (!ValidarRazonSocial())
            {
                errores.AppendLine("• La razon social es obligatoria (minimo 3 caracteres).");
                esValido = false;
            }

            if (!string.IsNullOrWhiteSpace(txtRUT.Text))
            {
                var existeRut = _proveedorRepo!.ExistsByRUT(IdTenant, txtRUT.Text.Trim());
                if (existeRut.IsSuccess && existeRut.Value)
                {
                    bool mismoRegistro = !_esNuevo &&
                        string.Equals(_snapRUT, txtRUT.Text.Trim(),
                                      System.StringComparison.OrdinalIgnoreCase);
                    if (!mismoRegistro)
                    {
                        errores.AppendLine("• Ya existe otro proveedor con este RUT en la empresa.");
                        txtRUT.BackColor = System.Drawing.Color.LightPink;
                        esValido = false;
                    }
                }
                else if (existeRut.IsFailure)
                    NotificationManager.Warning($"No se pudo verificar unicidad del RUT: {existeRut.Error?.Message}");
            }

            if (!ValidarEmail())
            {
                errores.AppendLine("• El email no tiene un formato valido.");
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
            if (_isLoading || dgvProveedores.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un proveedor para editar.");
                return;
            }

            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var item = dgvProveedores.SelectedRows[0].DataBoundItem as ProveedorItem;
            if (item == null) return;

            CargarEnEdicion(item);
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_isLoading || dgvProveedores.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un proveedor para eliminar.");
                return;
            }

            var item = dgvProveedores.SelectedRows[0].DataBoundItem as ProveedorItem;
            if (item == null) return;

            if (MessageBox.Show($"¿Desea eliminar el proveedor '{item.RazonSocial}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var getResult = _proveedorRepo!.GetById(item.IdProveedor);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                NotificationManager.Error("No se pudo obtener el proveedor para eliminar.");
                return;
            }

            var removeResult = _proveedorRepo.Remove(getResult.Value);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar: {removeResult.Error?.Message}");
                return;
            }

            try
            {
                _unitOfWork!.SaveChanges();
                NotificationManager.Success("Proveedor eliminado correctamente.");
                _huboCambios = true;
                CargarProveedores();
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
            CargarProveedores();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            GuardarProveedor();
        }

        private void GuardarProveedor()
        {
            if (!ValidarDatos()) return;

            try
            {
                if (_esNuevo)
                {
                    var nuevo = new Proveedor
                    {
                        IdTenant = IdTenant,
                        Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                        RazonSocial = txtRazonSocial.Text.Trim().ToUpperInvariant(),
                        NombreComercial = NullIfEmpty(txtNombreComercial.Text)?.ToUpperInvariant(),
                        RUT = NullIfEmpty(txtRUT.Text),
                        Direccion = NullIfEmpty(txtDireccion.Text),
                        Telefono = NullIfEmpty(txtTelefono.Text),
                        Email = NullIfEmpty(txtEmail.Text)?.ToLowerInvariant(),
                        Contacto = NullIfEmpty(txtContacto.Text),
                        Activo = chkActivo.Checked,
                        FecCrea = DateTime.UtcNow
                    };

                    var addResult = _proveedorRepo!.Add(nuevo);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al crear proveedor: {addResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Proveedor creado exitosamente.");
                    _huboCambios = true;
                    CargarProveedores();
                    LimpiarFormulario();
                }
                else
                {
                    if (!int.TryParse(lblIdValor.Text, out int idProveedor))
                    {
                        NotificationManager.Error("ID de proveedor invalido.");
                        return;
                    }

                    var getResult = _proveedorRepo!.GetById(idProveedor);
                    if (!getResult.IsSuccess || getResult.Value == null)
                    {
                        NotificationManager.Error($"Error al obtener proveedor: {getResult.Error?.Message}");
                        return;
                    }

                    var entidad = getResult.Value;
                    entidad.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                    entidad.RazonSocial = txtRazonSocial.Text.Trim().ToUpperInvariant();
                    entidad.NombreComercial = NullIfEmpty(txtNombreComercial.Text)?.ToUpperInvariant();
                    entidad.RUT = NullIfEmpty(txtRUT.Text);
                    entidad.Direccion = NullIfEmpty(txtDireccion.Text);
                    entidad.Telefono = NullIfEmpty(txtTelefono.Text);
                    entidad.Email = NullIfEmpty(txtEmail.Text)?.ToLowerInvariant();
                    entidad.Contacto = NullIfEmpty(txtContacto.Text);
                    entidad.Activo = chkActivo.Checked;

                    var updateResult = _proveedorRepo.Update(entidad);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar: {updateResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Proveedor actualizado exitosamente.");
                    _huboCambios = true;
                    CargarProveedores();
                    GuardarSnapshot(); // HayCambios vuelve a false tras guardar en edición
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
            dgvProveedores.ClearSelection();
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
                txtRazonSocial.Clear();
                txtNombreComercial.Clear();
                txtRUT.Clear();
                txtDireccion.Clear();
                txtTelefono.Clear();
                txtEmail.Clear();
                txtContacto.Clear();
                chkActivo.Checked = true;

                // FIX: resetear BackColors al limpiar para no mantener estado de error visual
                txtCodigo.BackColor = System.Drawing.Color.White;
                txtRazonSocial.BackColor = System.Drawing.Color.White;
                txtRUT.BackColor = System.Drawing.Color.White;
                txtEmail.BackColor = System.Drawing.Color.White;

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

        private void CargarEnEdicion(ProveedorItem item)
        {
            // Necesitamos los campos opcionales no en el DTO (NombreComercial, Direccion, Contacto)
            // => cargamos la entidad completa desde BD
            var getResult = _proveedorRepo!.GetById(item.IdProveedor);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                NotificationManager.Error("No se pudo cargar el proveedor para edicion.");
                return;
            }

            var p = getResult.Value;

            _isLoading = true;
            try
            {
                txtCodigo.Text = p.Codigo;
                txtRazonSocial.Text = p.RazonSocial;
                txtNombreComercial.Text = p.NombreComercial ?? string.Empty;
                txtRUT.Text = p.RUT ?? string.Empty;
                txtDireccion.Text = p.Direccion ?? string.Empty;
                txtTelefono.Text = p.Telefono ?? string.Empty;
                txtEmail.Text = p.Email ?? string.Empty;
                txtContacto.Text = p.Contacto ?? string.Empty;
                chkActivo.Checked = p.Activo;

                lblIdValor.Text = p.IdProveedor.ToString();
                lblFechaCreacionValor.Text = p.FecCrea.ToString("dd/MM/yyyy HH:mm");

                txtCodigo.BackColor = System.Drawing.Color.White;
                txtRazonSocial.BackColor = System.Drawing.Color.White;
                txtRUT.BackColor = System.Drawing.Color.White;
                txtEmail.BackColor = System.Drawing.Color.White;

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
            _snapRazonSocial = txtRazonSocial.Text;
            _snapNombreComercial = txtNombreComercial.Text;
            _snapRUT = txtRUT.Text;
            _snapDireccion = txtDireccion.Text;
            _snapTelefono = txtTelefono.Text;
            _snapEmail = txtEmail.Text;
            _snapContacto = txtContacto.Text;
            _snapActivo = chkActivo.Checked;
        }

        private bool HayCambios()
        {
            if (_esNuevo)
                return !string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                       !string.IsNullOrWhiteSpace(txtRazonSocial.Text) ||
                       !string.IsNullOrWhiteSpace(txtNombreComercial.Text) ||
                       !string.IsNullOrWhiteSpace(txtRUT.Text) ||
                       !string.IsNullOrWhiteSpace(txtDireccion.Text) ||
                       !string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                       !string.IsNullOrWhiteSpace(txtEmail.Text) ||
                       !string.IsNullOrWhiteSpace(txtContacto.Text);

            // FIX: comparaciones string vs string (ambos son string vacío cuando no hay valor)
            return txtCodigo.Text != _snapCodigo ||
                   txtRazonSocial.Text != _snapRazonSocial ||
                   txtNombreComercial.Text != _snapNombreComercial ||
                   txtRUT.Text != _snapRUT ||
                   txtDireccion.Text != _snapDireccion ||
                   txtTelefono.Text != _snapTelefono ||
                   txtEmail.Text != _snapEmail ||
                   txtContacto.Text != _snapContacto ||
                   chkActivo.Checked != _snapActivo;
        }

        private void ActualizarEstadoBotones()
        {
            bool haySeleccion = dgvProveedores.SelectedRows.Count > 0;
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
        }

        /// <summary>
        /// Devuelve null si el texto está vacío/whitespace, o el texto recortado si tiene contenido.
        /// Centraliza el patrón repetido en GuardarProveedor para campos opcionales.
        /// </summary>
        private static string? NullIfEmpty(string? text) =>
            string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }
}


