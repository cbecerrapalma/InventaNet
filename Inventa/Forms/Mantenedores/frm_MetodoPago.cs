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
    public partial class frm_MetodoPago : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly IServiceScopeFactory _scopeFactory;

        // ── Scope y UnitOfWork propios ────────────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorio (lazy) ────────────────────────────────────────────────
        private IMetodoPagoRepository? _metodoPagoRepo;

        // ── Estado interno ────────────────────────────────────────────────────
        private bool _esNuevo = true;
        private bool _isLoading;
        private bool _huboCambios;

        // Snapshots para detección de cambios
        private string _snapCodigo = string.Empty;
        private string _snapNombre = string.Empty;
        private bool _snapActivo = true;

        // ── DTO aplanado para la grilla ───────────────────────────────────────
        // IdMetodoPago es byte (PK de la tabla MetodoPago)
        private sealed record MetodoPagoItem(
            byte IdMetodoPago,
            string Codigo,
            string Nombre,
            bool Activo
        );

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_MetodoPago(
            IServiceScopeFactory scopeFactory)
        {
            InitializeComponent();
            _scopeFactory = scopeFactory;

            if (!CBP.WinFrms.Theming.DesignModeHelper.IsDesignTime())
                CBP.WinFrms.Theming.ThemeInjector.Attach(this);

            this.KeyPreview = true;
            this.KeyDown += Frm_MetodoPago_KeyDown;

            ConfigurarGrid();
            ConfigurarEventos();

            this.FormClosing += Frm_MetodoPago_FormClosing;
            this.FormClosed += (s, e) => _formScope?.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarGrid()
        {
            dgvMetodosPago.AutoGenerateColumns = false;
            dgvMetodosPago.Columns.Clear();

            dgvMetodosPago.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "ID",
                DataPropertyName = nameof(MetodoPagoItem.IdMetodoPago),
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });
            dgvMetodosPago.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCodigo",
                HeaderText = "Codigo",
                DataPropertyName = nameof(MetodoPagoItem.Codigo),
                Width = 160
            });
            dgvMetodosPago.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNombre",
                HeaderText = "Nombre",
                DataPropertyName = nameof(MetodoPagoItem.Nombre),
                Width = 300
            });
            dgvMetodosPago.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colActivo",
                HeaderText = "Activo",
                DataPropertyName = nameof(MetodoPagoItem.Activo),
                Width = 62,
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvMetodosPago.RowPrePaint += DgvMetodosPago_RowPrePaint;
            dgvMetodosPago.SelectionChanged += DgvMetodosPago_SelectionChanged;
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

        private void Frm_MetodoPago_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                _formScope = _scopeFactory.CreateScope();
                _unitOfWork = _formScope.ServiceProvider
                    .GetRequiredService<IUnitOfWorkSync>();
                _metodoPagoRepo = _unitOfWork.GetCustomRepository<IMetodoPagoRepository>();

                CargarMetodosPago();
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

        private void Frm_MetodoPago_FormClosing(object sender, FormClosingEventArgs e)
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

        private void Frm_MetodoPago_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                BtnGuardar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                CargarMetodosPago();
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

        private void CargarMetodosPago()
        {
            // GetAll(asNoTracking:true) no existe en IMetodoPagoRepository.
            // Se usa GetActivos() — único método de lista disponible.
            // MetodoPago es una tabla de catálogo pequeña (< 10 registros);
            // los inactivos no requieren administración frecuente.
            var result = _metodoPagoRepo!.GetActivos();
            if (!result.IsSuccess)
            {
                NotificationManager.Error($"Error al cargar metodos de pago: {result.Error?.Message}");
                return;
            }

            var items = result.Value
                .Select(m => new MetodoPagoItem(m.IdMetodoPago, m.Codigo, m.Nombre, m.Activo))
                .ToList();

            dgvMetodosPago.DataSource = null;
            dgvMetodosPago.DataSource = items;

            tsLblContador.Text = $"{items.Count} metodo(s) activo(s)";
        }

        // ═════════════════════════════════════════════════════════════════════
        // COLOREADO DE FILAS
        // ═════════════════════════════════════════════════════════════════════

        private void DgvMetodosPago_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvMetodosPago.Rows.Count) return;
            var item = dgvMetodosPago.Rows[e.RowIndex].DataBoundItem as MetodoPagoItem;
            if (item == null) return;

            var row = dgvMetodosPago.Rows[e.RowIndex];
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

        private void DgvMetodosPago_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            ActualizarEstadoBotones();
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

            var existe = _metodoPagoRepo!.ExistsByCodigo(txtCodigo.Text.Trim());
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
                var existe = _metodoPagoRepo!.ExistsByCodigo(txtCodigo.Text.Trim());
                if (existe.IsSuccess && existe.Value)
                {
                    bool mismoRegistro = !_esNuevo &&
                        string.Equals(_snapCodigo, txtCodigo.Text.Trim(),
                                      System.StringComparison.OrdinalIgnoreCase);
                    if (!mismoRegistro)
                    {
                        errores.AppendLine("• Ya existe un metodo de pago con este codigo.");
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
            if (_isLoading || dgvMetodosPago.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un metodo de pago para editar.");
                return;
            }

            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var item = dgvMetodosPago.SelectedRows[0].DataBoundItem as MetodoPagoItem;
            if (item == null) return;

            CargarEnEdicion(item);
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_isLoading || dgvMetodosPago.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un metodo de pago para eliminar.");
                return;
            }

            var item = dgvMetodosPago.SelectedRows[0].DataBoundItem as MetodoPagoItem;
            if (item == null) return;

            if (MessageBox.Show(
                    $"¿Desea eliminar el metodo de pago '{item.Nombre}'?\n\n" +
                    "Verifique que no existan ventas que lo utilisent.", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var getResult = _metodoPagoRepo!.GetById(item.IdMetodoPago);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                NotificationManager.Error("No se pudo obtener el metodo de pago para eliminar.");
                return;
            }

            var removeResult = _metodoPagoRepo.Remove(getResult.Value);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar: {removeResult.Error?.Message}");
                return;
            }

            try
            {
                _unitOfWork!.SaveChanges();
                NotificationManager.Success("Metodo de pago eliminado correctamente.");
                _huboCambios = true;
                CargarMetodosPago();
                LimpiarFormulario();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                // FK violation: la tabla VentaPago referencia IdMetodoPago.
                // Si hay ventas que usan este metodo, la BD rechazará la eliminación.
                NotificationManager.Error(
                    $"No se pudo eliminar. Es posible que existan ventas que utilizan " +
                    $"este metodo de pago.\nDetalle: {ex.Message}");
            }
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            CargarMetodosPago();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            GuardarMetodoPago();
        }

        private void GuardarMetodoPago()
        {
            if (!ValidarDatos()) return;

            try
            {
                if (_esNuevo)
                {
                    var nuevo = new MetodoPago
                    {
                        Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                        Nombre = txtNombre.Text.Trim().ToUpperInvariant(),
                        Activo = chkActivo.Checked
                    };

                    var addResult = _metodoPagoRepo!.Add(nuevo);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al crear: {addResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Metodo de pago creado exitosamente.");
                    _huboCambios = true;
                    CargarMetodosPago();
                    LimpiarFormulario();
                }
                else
                {
                    // IdMetodoPago es byte — la PK de esta tabla de catálogo es un byte
                    if (!byte.TryParse(lblIdValor.Text, out byte idMetodoPago))
                    {
                        NotificationManager.Error("ID de metodo de pago invalido.");
                        return;
                    }

                    var getResult = _metodoPagoRepo!.GetById(idMetodoPago);
                    if (!getResult.IsSuccess || getResult.Value == null)
                    {
                        NotificationManager.Error($"Error al obtener metodo de pago: {getResult.Error?.Message}");
                        return;
                    }

                    var entidad = getResult.Value;
                    entidad.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                    entidad.Nombre = txtNombre.Text.Trim().ToUpperInvariant();
                    entidad.Activo = chkActivo.Checked;

                    var updateResult = _metodoPagoRepo.Update(entidad);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar: {updateResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Metodo de pago actualizado exitosamente.");
                    _huboCambios = true;
                    CargarMetodosPago();
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
            dgvMetodosPago.ClearSelection();
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
                _esNuevo = true;
                GuardarSnapshot();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void CargarEnEdicion(MetodoPagoItem item)
        {
            _isLoading = true;
            try
            {
                txtCodigo.Text = item.Codigo;
                txtNombre.Text = item.Nombre;
                chkActivo.Checked = item.Activo;
                lblIdValor.Text = item.IdMetodoPago.ToString();
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
            bool haySeleccion = dgvMetodosPago.SelectedRows.Count > 0;
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
        }
    }
}


