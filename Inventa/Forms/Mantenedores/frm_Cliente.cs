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
    public partial class frm_Cliente : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IServiceScopeFactory _scopeFactory;

        // ── Scope y UnitOfWork propios ────────────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorio (lazy) ────────────────────────────────────────────────
        private IClienteRepository? _clienteRepo;

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
        private string? _snapRUT;
        private string? _snapTelefono;
        private string? _snapEmail;
        private string? _snapDireccion;
        private byte _snapTipoCliente;
        private bool _snapActivo;

        // ── Tipo fuertemente tipado para cboTipoCliente — elimina dynamic ─────
        private sealed record TipoClienteItem(byte Valor, string Texto)
        {
            public override string ToString() => Texto;
        }

        // ── DTO aplanado para la grilla — desacopla de la entidad EF ─────────
        private sealed record ClienteItem(
            int IdCliente,
            string Codigo,
            string Nombre,
            string? RUT,
            string? Telefono,
            string? Email,
            byte TipoCliente,
            bool Activo,
            DateTime FecCrea
        );

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_Cliente(
            IServiceScopeFactory scopeFactory,
            ITenantContext tenantContext)
        {
            InitializeComponent();
            _scopeFactory = scopeFactory;
            _tenantContext = tenantContext;

            if (!CBP.WinFrms.Theming.DesignModeHelper.IsDesignTime())
                CBP.WinFrms.Theming.ThemeInjector.Attach(this);

            this.KeyPreview = true;
            this.KeyDown += Frm_Cliente_KeyDown;

            ConfigurarComboTipo();
            ConfigurarGrid();
            ConfigurarEventos();

            this.FormClosing += Frm_Cliente_FormClosing;
            this.FormClosed += (s, e) => _formScope?.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarComboTipo()
        {
            // TipoClienteItem record — elimina los 4 accesos dynamic
            cboTipoCliente.DataSource = new List<TipoClienteItem>
            {
                new(1, "Publico General"),
                new(2, "Frecuente"),
                new(3, "Mayorista"),
                new(4, "Corporativo")
            };
            cboTipoCliente.DisplayMember = nameof(TipoClienteItem.Texto);
            cboTipoCliente.ValueMember = nameof(TipoClienteItem.Valor);
            cboTipoCliente.SelectedIndex = 0;
        }

        private void ConfigurarGrid()
        {
            dgvClientes.AutoGenerateColumns = false;
            dgvClientes.Columns.Clear();

            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                DataPropertyName = nameof(ClienteItem.IdCliente),
                Visible = false
            });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCodigo",
                HeaderText = "Codigo",
                DataPropertyName = nameof(ClienteItem.Codigo),
                Width = 100
            });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNombre",
                HeaderText = "Nombre",
                DataPropertyName = nameof(ClienteItem.Nombre),
                Width = 250
            });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colRUT",
                HeaderText = "RUT",
                DataPropertyName = nameof(ClienteItem.RUT),
                Width = 120
            });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTelefono",
                HeaderText = "Telefono",
                DataPropertyName = nameof(ClienteItem.Telefono),
                Width = 110
            });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colEmail",
                HeaderText = "Email",
                DataPropertyName = nameof(ClienteItem.Email),
                Width = 180
            });
            dgvClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTipoCliente",
                HeaderText = "Tipo",
                DataPropertyName = nameof(ClienteItem.TipoCliente),
                Width = 120
            });
            dgvClientes.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colActivo",
                HeaderText = "Activo",
                DataPropertyName = nameof(ClienteItem.Activo),
                Width = 62,
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvClientes.CellFormatting += DgvClientes_CellFormatting;
            dgvClientes.RowPrePaint += DgvClientes_RowPrePaint;
            dgvClientes.SelectionChanged += DgvClientes_SelectionChanged;
        }

        private void ConfigurarEventos()
        {
            txtCodigo.TextChanged += (s, e) => { if (!_isLoading) ValidarCodigo(); };
            txtNombre.TextChanged += (s, e) => { if (!_isLoading) ValidarNombre(); };
            txtEmail.Leave += TxtEmail_Leave;
            txtCodigo.Leave += TxtCodigo_Leave;
            txtRUT.Leave += TxtRUT_Leave;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_Cliente_Load(object sender, EventArgs e)
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
                _clienteRepo = _unitOfWork.GetCustomRepository<IClienteRepository>();

                CargarClientes();
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

        private void Frm_Cliente_FormClosing(object sender, FormClosingEventArgs e)
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

        private void Frm_Cliente_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                BtnGuardar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                CargarClientes();
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

        private void CargarClientes()
        {
            var result = _clienteRepo!.GetByTenant(IdTenant, soloActivos: false);
            if (!result.IsSuccess)
            {
                NotificationManager.Error($"Error al cargar clientes: {result.Error?.Message}");
                return;
            }

            var items = result.Value
                .OrderBy(c => c.Nombre)
                .Select(c => new ClienteItem(
                    c.IdCliente, c.Codigo, c.Nombre,
                    c.RUT, c.Telefono, c.Email,
                    c.TipoCliente, c.Activo, c.FecCrea))
                .ToList();

            dgvClientes.DataSource = null;
            dgvClientes.DataSource = items;

            ActualizarContador(items.Count);
        }

        private void ActualizarContador(int total)
        {
            int activos = dgvClientes.Rows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as ClienteItem)
                .Count(c => c?.Activo == true);
            tsLblContador.Text = $"{activos} activos / {total} total";
        }

        // ═════════════════════════════════════════════════════════════════════
        // EVENTOS DE GRILLA
        // ═════════════════════════════════════════════════════════════════════

        private void DgvClientes_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dgvClientes.Columns[e.ColumnIndex].Name != "colTipoCliente") return;
            if (e.Value == null) return;

            // DTO almacena TipoCliente como byte — formatear como texto sin dynamic
            byte tipo = Convert.ToByte(e.Value);
            e.Value = tipo switch
            {
                1 => "Publico General",
                2 => "Frecuente",
                3 => "Mayorista",
                4 => "Corporativo",
                _ => "Desconocido"
            };
            e.FormattingApplied = true;
        }

        private void DgvClientes_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvClientes.Rows.Count) return;
            var item = dgvClientes.Rows[e.RowIndex].DataBoundItem as ClienteItem;
            if (item == null) return;

            var row = dgvClientes.Rows[e.RowIndex];
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

        private void DgvClientes_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            ActualizarEstadoBotones();

            if (dgvClientes.SelectedRows.Count > 0)
            {
                var item = dgvClientes.SelectedRows[0].DataBoundItem as ClienteItem;
                if (item != null)
                {
                    lblIdClienteValor.Text = item.IdCliente.ToString();
                    lblFechaCreacionValor.Text = item.FecCrea.ToString("dd/MM/yyyy HH:mm");
                    return;
                }
            }

            lblIdClienteValor.Text = "-";
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

        private bool ValidarEmail()
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtEmail.BackColor = System.Drawing.Color.White;
                return true; // Email es opcional
            }

            // FIX: validación correcta — MailAddress normaliza la cadena,
            // por eso la comparación de igualdad con el input era incorrecta.
            // Ahora simplemente intentamos parsear y atrapamos la excepción.
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

        private void TxtEmail_Leave(object? sender, EventArgs e) => ValidarEmail();

        private void TxtCodigo_Leave(object? sender, EventArgs e)
        {
            if (_isLoading || !ValidarCodigo()) return;

            var existe = _clienteRepo!.ExistsByCodigo(IdTenant, txtCodigo.Text.Trim());
            if (!existe.IsSuccess || !existe.Value) return;

            bool mismoRegistro = !_esNuevo && _snapCodigo != null &&
                string.Equals(_snapCodigo, txtCodigo.Text.Trim(),
                              System.StringComparison.OrdinalIgnoreCase);
            if (!mismoRegistro)
                txtCodigo.BackColor = System.Drawing.Color.LightPink;
        }

        private void TxtRUT_Leave(object? sender, EventArgs e)
        {
            if (_isLoading || string.IsNullOrWhiteSpace(txtRUT.Text)) return;

            var existe = _clienteRepo!.ExistsByRUT(IdTenant, txtRUT.Text.Trim());
            if (!existe.IsSuccess || !existe.Value) return;

            bool mismoRegistro = !_esNuevo && _snapRUT != null &&
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
                var existe = _clienteRepo!.ExistsByCodigo(IdTenant, txtCodigo.Text.Trim());
                if (existe.IsSuccess && existe.Value)
                {
                    bool mismoRegistro = !_esNuevo && _snapCodigo != null &&
                        string.Equals(_snapCodigo, txtCodigo.Text.Trim(),
                                      System.StringComparison.OrdinalIgnoreCase);
                    if (!mismoRegistro)
                    {
                        errores.AppendLine("• Ya existe un cliente con este codigo en la empresa.");
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

            // Validación de RUT si fue ingresado
            if (!string.IsNullOrWhiteSpace(txtRUT.Text))
            {
                var existeRut = _clienteRepo!.ExistsByRUT(IdTenant, txtRUT.Text.Trim());
                if (existeRut.IsSuccess && existeRut.Value)
                {
                    bool mismoRUT = !_esNuevo && _snapRUT != null &&
                        string.Equals(_snapRUT, txtRUT.Text.Trim(),
                                      System.StringComparison.OrdinalIgnoreCase);
                    if (!mismoRUT)
                    {
                        errores.AppendLine("• Ya existe otro cliente con este RUT en la empresa.");
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
            if (_isLoading || dgvClientes.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un cliente para editar.");
                return;
            }

            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var item = dgvClientes.SelectedRows[0].DataBoundItem as ClienteItem;
            if (item == null) return;

            CargarEnEdicion(item);
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_isLoading || dgvClientes.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un cliente para eliminar.");
                return;
            }

            var item = dgvClientes.SelectedRows[0].DataBoundItem as ClienteItem;
            if (item == null) return;

            if (MessageBox.Show($"¿Desea eliminar el cliente '{item.Nombre}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var getResult = _clienteRepo!.GetById(item.IdCliente);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                NotificationManager.Error("No se pudo obtener el cliente para eliminar.");
                return;
            }

            var removeResult = _clienteRepo.Remove(getResult.Value);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar: {removeResult.Error?.Message}");
                return;
            }

            try
            {
                _unitOfWork!.SaveChanges();
                NotificationManager.Success("Cliente eliminado correctamente.");
                _huboCambios = true;
                CargarClientes();
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
            CargarClientes();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            GuardarCliente();
        }

        private void GuardarCliente()
        {
            if (!ValidarDatos()) return;

            // TipoCliente leído con tipo fuertemente tipado — sin dynamic
            byte tipoCliente = ObtenerTipoSeleccionado();

            try
            {
                if (_esNuevo)
                {
                    var nuevo = new Cliente
                    {
                        IdTenant = IdTenant,
                        Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                        Nombre = txtNombre.Text.Trim().ToUpperInvariant(),
                        RUT = string.IsNullOrWhiteSpace(txtRUT.Text) ? null : txtRUT.Text.Trim(),
                        Direccion = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text.Trim(),
                        Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim(),
                        Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim().ToLowerInvariant(),
                        TipoCliente = tipoCliente,
                        Activo = chkActivo.Checked,
                        FecCrea = DateTime.UtcNow
                    };

                    var addResult = _clienteRepo!.Add(nuevo);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al crear cliente: {addResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Cliente creado exitosamente.");
                    _huboCambios = true;
                    CargarClientes();
                    LimpiarFormulario();
                }
                else
                {
                    if (!int.TryParse(lblIdValor.Text, out int idCliente))
                    {
                        NotificationManager.Error("ID de cliente invalido.");
                        return;
                    }

                    var getResult = _clienteRepo!.GetById(idCliente);
                    if (!getResult.IsSuccess || getResult.Value == null)
                    {
                        NotificationManager.Error($"Error al obtener cliente: {getResult.Error?.Message}");
                        return;
                    }

                    var entidad = getResult.Value;
                    entidad.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                    entidad.Nombre = txtNombre.Text.Trim().ToUpperInvariant();
                    entidad.RUT = string.IsNullOrWhiteSpace(txtRUT.Text) ? null : txtRUT.Text.Trim();
                    entidad.Direccion = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text.Trim();
                    entidad.Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim();
                    entidad.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim().ToLowerInvariant();
                    entidad.TipoCliente = tipoCliente;
                    entidad.Activo = chkActivo.Checked;

                    var updateResult = _clienteRepo.Update(entidad);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar: {updateResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Cliente actualizado exitosamente.");
                    _huboCambios = true;
                    CargarClientes();
                    GuardarSnapshot(); // Actualiza snapshot para que HayCambios vuelva a false
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
            dgvClientes.ClearSelection();
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
                txtRUT.Clear();
                txtTelefono.Clear();
                txtEmail.Clear();
                txtDireccion.Clear();
                cboTipoCliente.SelectedIndex = 0;
                chkActivo.Checked = true;

                txtCodigo.BackColor = System.Drawing.Color.White;
                txtNombre.BackColor = System.Drawing.Color.White;
                txtRUT.BackColor = System.Drawing.Color.White;
                txtEmail.BackColor = System.Drawing.Color.White;

                lblIdValor.Text = "<< Nuevo >>";
                lblIdClienteValor.Text = "-";
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

        private void CargarEnEdicion(ClienteItem item)
        {
            _isLoading = true;
            try
            {
                txtCodigo.Text = item.Codigo;
                txtNombre.Text = item.Nombre;
                txtRUT.Text = item.RUT ?? string.Empty;
                txtTelefono.Text = item.Telefono ?? string.Empty;
                txtEmail.Text = item.Email ?? string.Empty;

                // Cargar Direccion desde la entidad real (no está en el DTO — campo largo)
                var getResult = _clienteRepo!.GetById(item.IdCliente);
                txtDireccion.Text = getResult.IsSuccess && getResult.Value != null
                    ? getResult.Value.Direccion ?? string.Empty
                    : string.Empty;

                // Seleccionar tipo usando DataSource tipado — sin dynamic ni foreach
                if (cboTipoCliente.DataSource is List<TipoClienteItem> tipos)
                {
                    var tipoItem = tipos.FirstOrDefault(t => t.Valor == item.TipoCliente);
                    if (tipoItem != null)
                        cboTipoCliente.SelectedValue = tipoItem.Valor;
                    else
                        cboTipoCliente.SelectedIndex = 0;
                }

                chkActivo.Checked = item.Activo;
                lblIdValor.Text = item.IdCliente.ToString();

                lblIdClienteValor.Text = item.IdCliente.ToString();
                lblFechaCreacionValor.Text = item.FecCrea.ToString("dd/MM/yyyy HH:mm");

                txtCodigo.BackColor = System.Drawing.Color.White;
                txtNombre.BackColor = System.Drawing.Color.White;
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
            _snapNombre = txtNombre.Text;
            _snapRUT = txtRUT.Text;
            _snapTelefono = txtTelefono.Text;
            _snapEmail = txtEmail.Text;
            _snapDireccion = txtDireccion.Text;
            _snapTipoCliente = ObtenerTipoSeleccionado();
            _snapActivo = chkActivo.Checked;
        }

        private bool HayCambios()
        {
            if (_esNuevo)
                return !string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                       !string.IsNullOrWhiteSpace(txtNombre.Text) ||
                       !string.IsNullOrWhiteSpace(txtRUT.Text) ||
                       !string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                       !string.IsNullOrWhiteSpace(txtEmail.Text) ||
                       !string.IsNullOrWhiteSpace(txtDireccion.Text);

            return txtCodigo.Text != _snapCodigo ||
                   txtNombre.Text != _snapNombre ||
                   txtRUT.Text != _snapRUT ||
                   txtTelefono.Text != _snapTelefono ||
                   txtEmail.Text != _snapEmail ||
                   txtDireccion.Text != _snapDireccion ||
                   ObtenerTipoSeleccionado() != _snapTipoCliente ||
                   chkActivo.Checked != _snapActivo;
        }

        /// <summary>
        /// Lee el tipo de cliente seleccionado como byte.
        /// Elimina los 4 accesos dynamic que usaba el código original.
        /// </summary>
        private byte ObtenerTipoSeleccionado()
        {
            if (cboTipoCliente.SelectedItem is TipoClienteItem item)
                return item.Valor;
            return 1; // Publico General como fallback
        }

        private void ActualizarEstadoBotones()
        {
            bool haySeleccion = dgvClientes.SelectedRows.Count > 0;
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
        }
    }
}


