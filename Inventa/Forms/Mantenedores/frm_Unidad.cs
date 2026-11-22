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
    public partial class frm_Unidad : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly IServiceScopeFactory _scopeFactory;

        // ── Scope y UnitOfWork propios ────────────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorio (lazy) ────────────────────────────────────────────────
        private IUnidadRepository? _unidadRepo;

        // ── Estado interno ────────────────────────────────────────────────────
        private bool _esNuevo = true;
        private bool _isLoading;
        private bool _huboCambios;

        // Snapshots para detección de cambios
        private string? _snapCodigo;
        private string? _snapNombre;

        // ── DTO aplanado para la grilla — desacopla de la entidad EF ─────────
        private sealed record UnidadItem(
            int IdUnidad,
            string Codigo,
            string Nombre,
            bool EnUso
        );

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_Unidad(
            IServiceScopeFactory scopeFactory)
        {
            InitializeComponent();
            _scopeFactory = scopeFactory;

            if (!CBP.WinFrms.Theming.DesignModeHelper.IsDesignTime())
                CBP.WinFrms.Theming.ThemeInjector.Attach(this);

            this.KeyPreview = true;
            this.KeyDown += Frm_Unidad_KeyDown;

            ConfigurarGrid();
            ConfigurarEventos();

            this.FormClosing += Frm_Unidad_FormClosing;
            this.FormClosed += (s, e) => _formScope?.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarGrid()
        {
            dgvUnidades.AutoGenerateColumns = false;
            dgvUnidades.Columns.Clear();

            dgvUnidades.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "ID",
                DataPropertyName = nameof(UnidadItem.IdUnidad),
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });
            dgvUnidades.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCodigo",
                HeaderText = "Codigo",
                DataPropertyName = nameof(UnidadItem.Codigo),
                Width = 150
            });
            dgvUnidades.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNombre",
                HeaderText = "Nombre",
                DataPropertyName = nameof(UnidadItem.Nombre),
                Width = 280
            });
            dgvUnidades.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colEnUso",
                HeaderText = "En uso",
                DataPropertyName = nameof(UnidadItem.EnUso),
                Width = 62,
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ToolTipText = "Indica si la unidad esta siendo utilizada por algun producto o presentacion"
            });

            dgvUnidades.RowPrePaint += DgvUnidades_RowPrePaint;
            dgvUnidades.SelectionChanged += DgvUnidades_SelectionChanged;
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

        private void Frm_Unidad_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                _formScope = _scopeFactory.CreateScope();
                _unitOfWork = _formScope.ServiceProvider
                    .GetRequiredService<IUnitOfWorkSync>();
                _unidadRepo = _unitOfWork.GetCustomRepository<IUnidadRepository>();

                CargarUnidades();
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

        private void Frm_Unidad_FormClosing(object sender, FormClosingEventArgs e)
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

        private void Frm_Unidad_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                BtnGuardar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                CargarUnidades();
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

        private void CargarUnidades()
        {
            var result = _unidadRepo!.GetAllOrdenado();
            if (!result.IsSuccess)
            {
                NotificationManager.Error($"Error al cargar unidades: {result.Error?.Message}");
                return;
            }

            var items = result.Value
                .Select(u =>
                {
                    var uso = _unidadRepo.IsUsedByProductos(u.IdUnidad);
                    return new UnidadItem(u.IdUnidad, u.Codigo, u.Nombre,
                        uso.IsSuccess && uso.Value);
                })
                .ToList();

            dgvUnidades.DataSource = null;
            dgvUnidades.DataSource = items;

            ActualizarContador(items.Count);
        }

        private void ActualizarContador(int total)
        {
            int enUso = dgvUnidades.Rows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as UnidadItem)
                .Count(u => u?.EnUso == true);
            tsLblContador.Text = $"{enUso} en uso / {total} total";
        }

        // ═════════════════════════════════════════════════════════════════════
        // COLOREADO DE FILAS
        // ═════════════════════════════════════════════════════════════════════

        private void DgvUnidades_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvUnidades.Rows.Count) return;
            var item = dgvUnidades.Rows[e.RowIndex].DataBoundItem as UnidadItem;
            if (item == null) return;

            var row = dgvUnidades.Rows[e.RowIndex];
            if (item.EnUso)
            {
                row.DefaultCellStyle.BackColor = AppColors.SuccessLight;
                row.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
                row.DefaultCellStyle.SelectionBackColor = AppColors.Success;
                row.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
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

        private void DgvUnidades_SelectionChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            ActualizarEstadoBotones();

            if (dgvUnidades.SelectedRows.Count > 0)
            {
                var item = dgvUnidades.SelectedRows[0].DataBoundItem as UnidadItem;
                if (item != null)
                {
                    lblIdUnidadValor.Text = item.IdUnidad.ToString();
                    lblUsosValor.Text = item.EnUso ? "Si" : "No";
                    return;
                }
            }

            lblIdUnidadValor.Text = "-";
            lblUsosValor.Text = "-";
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
            if (txtNombre.Text.Length < 2)
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

            var existe = _unidadRepo!.ExistsByCodigo(txtCodigo.Text.Trim());
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
                errores.AppendLine("• El codigo es obligatorio.");
                esValido = false;
            }
            else
            {
                var existe = _unidadRepo!.ExistsByCodigo(txtCodigo.Text.Trim());
                if (existe.IsSuccess && existe.Value)
                {
                    bool mismoRegistro = !_esNuevo && _snapCodigo != null &&
                        string.Equals(_snapCodigo, txtCodigo.Text.Trim(),
                                      System.StringComparison.OrdinalIgnoreCase);
                    if (!mismoRegistro)
                    {
                        errores.AppendLine("• Ya existe una unidad con este codigo.");
                        txtCodigo.BackColor = System.Drawing.Color.LightPink;
                        esValido = false;
                    }
                }
                else if (existe.IsFailure)
                    NotificationManager.Warning($"No se pudo verificar unicidad del codigo: {existe.Error?.Message}");
            }

            if (!ValidarNombre())
            {
                errores.AppendLine("• El nombre es obligatorio (minimo 2 caracteres).");
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
            if (_isLoading || dgvUnidades.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar una unidad para editar.");
                return;
            }

            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var item = dgvUnidades.SelectedRows[0].DataBoundItem as UnidadItem;
            if (item == null) return;

            CargarEnEdicion(item);
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_isLoading || dgvUnidades.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar una unidad para eliminar.");
                return;
            }

            var item = dgvUnidades.SelectedRows[0].DataBoundItem as UnidadItem;
            if (item == null) return;

            // FIX: IsUsedByProductos() síncrono — DTO ya tiene el campo EnUso calculado
            if (item.EnUso)
            {
                NotificationManager.Warning(
                    $"No se puede eliminar '{item.Nombre}' porque esta siendo utilizada " +
                    "por productos o presentaciones.");
                return;
            }

            if (MessageBox.Show($"¿Desea eliminar la unidad '{item.Nombre}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var getResult = _unidadRepo!.GetById(item.IdUnidad);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                NotificationManager.Error("No se pudo obtener la unidad para eliminar.");
                return;
            }

            var removeResult = _unidadRepo.Remove(getResult.Value);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar: {removeResult.Error?.Message}");
                return;
            }

            try
            {
                _unitOfWork!.SaveChanges();
                NotificationManager.Success("Unidad eliminada correctamente.");
                _huboCambios = true;
                CargarUnidades();
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
            CargarUnidades();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            GuardarUnidad();
        }

        private void GuardarUnidad()
        {
            if (!ValidarDatos()) return;

            try
            {
                if (_esNuevo)
                {
                    var nueva = new Unidad
                    {
                        Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                        Nombre = txtNombre.Text.Trim().ToUpperInvariant()
                    };

                    var addResult = _unidadRepo!.Add(nueva);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al crear unidad: {addResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Unidad creada exitosamente.");
                    _huboCambios = true;
                    CargarUnidades();
                    LimpiarFormulario();
                }
                else
                {
                    if (!int.TryParse(lblIdValor.Text, out int idUnidad))
                    {
                        NotificationManager.Error("ID de unidad invalido.");
                        return;
                    }

                    var getResult = _unidadRepo!.GetById(idUnidad);
                    if (!getResult.IsSuccess || getResult.Value == null)
                    {
                        NotificationManager.Error($"Error al obtener unidad: {getResult.Error?.Message}");
                        return;
                    }

                    var entidad = getResult.Value;
                    entidad.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                    entidad.Nombre = txtNombre.Text.Trim().ToUpperInvariant();

                    var updateResult = _unidadRepo.Update(entidad);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar: {updateResult.Error?.Message}");
                        return;
                    }

                    _unitOfWork!.SaveChanges();
                    NotificationManager.Success("Unidad actualizada exitosamente.");
                    _huboCambios = true;
                    CargarUnidades();
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
            dgvUnidades.ClearSelection();
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
                txtCodigo.BackColor = System.Drawing.Color.White;
                txtNombre.BackColor = System.Drawing.Color.White;
                lblIdValor.Text = "<< Nuevo >>";
                lblIdUnidadValor.Text = "-";
                lblUsosValor.Text = "-";
                _esNuevo = true;
                GuardarSnapshot();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void CargarEnEdicion(UnidadItem item)
        {
            _isLoading = true;
            try
            {
                txtCodigo.Text = item.Codigo;
                txtNombre.Text = item.Nombre;
                lblIdValor.Text = item.IdUnidad.ToString();
                lblIdUnidadValor.Text = item.IdUnidad.ToString();
                lblUsosValor.Text = item.EnUso ? "Si" : "No";
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
        }

        private bool HayCambios()
        {
            if (_esNuevo)
                return !string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                       !string.IsNullOrWhiteSpace(txtNombre.Text);

            return txtCodigo.Text != _snapCodigo ||
                   txtNombre.Text != _snapNombre;
        }

        private void ActualizarEstadoBotones()
        {
            bool haySeleccion = dgvUnidades.SelectedRows.Count > 0;
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
        }
    }
}


