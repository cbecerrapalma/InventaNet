using CBP.Data.Abstractions;
using CBP.MultiTenant.Abstractions;
using CBP.MultiTenant.Extensions;
using CBP.WinFrms.Notification;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using System.ComponentModel;

namespace Inventa.Forms.Mantenedores
{
    public partial class frm_Bodega : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IUnitOfWorkSync _unitOfWork;

        // ── Repositorio (lazy) ────────────────────────────────────────────────
        private IBodegaRepository? _bodegaRepo;

        // ── Estado del formulario ─────────────────────────────────────────────
        /// <summary>true = creando nueva bodega; false = editando existente.</summary>
        private bool _esNuevo = true;

        /// <summary>Bodega actualmente en edición. null cuando _esNuevo=true.</summary>
        private Bodega? _bodegaEnEdicion;

        // ── Snapshot para detección de cambios ────────────────────────────────
        private string? _snapCodigo;
        private string? _snapNombre;
        private bool _snapActivo;

        // ── Propiedad pública: IdTenant activo ────────────────────────────────
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IdTenant { get; private set; }

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_Bodega(
            IUnitOfWorkSync unitOfWork,
            ITenantContext tenantContext)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;

            if (!CBP.WinFrms.Theming.DesignModeHelper.IsDesignTime())
                CBP.WinFrms.Theming.ThemeInjector.Attach(this);

            this.KeyPreview = true;
            this.KeyDown += Frm_Bodega_KeyDown;

            ConfigurarColumnasGrilla();
            ConfigurarEventos();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarEventos()
        {
            txtCodigo.TextChanged += (s, e) => { if (!_cargando) ValidarCodigo(); };
            txtCodigo.Leave += TxtCodigo_Leave;
            txtNombre.TextChanged += (s, e) => { if (!_cargando) ValidarNombre(); };
        }

        private void ConfigurarColumnasGrilla()
        {
            dgvBodegas.AutoGenerateColumns = false;
            dgvBodegas.Columns.Clear();

            dgvBodegas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "ID",
                DataPropertyName = nameof(Bodega.IdBodega),
                Visible = false
            });
            dgvBodegas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCodigo",
                HeaderText = "Codigo",
                DataPropertyName = nameof(Bodega.Codigo),
                Width = 120
            });
            dgvBodegas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNombre",
                HeaderText = "Nombre",
                DataPropertyName = nameof(Bodega.Nombre),
                Width = 300,
                // Última columna en Fill — aprovecha el ancho disponible
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dgvBodegas.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colActivo",
                HeaderText = "Activo",
                DataPropertyName = nameof(Bodega.Activo),
                Width = 70
            });
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private bool _cargando;

        private void Frm_Bodega_Load(object sender, EventArgs e)
        {
            var tenantResult = _tenantContext.TryGetCurrent();
            if (tenantResult.IsFailure)
            {
                NotificationManager.Error("No hay empresa activa. Reinicie la aplicacion.");
                Close();
                return;
            }

            IdTenant = tenantResult.Value.Id;
            lblTenantValor.Text = tenantResult.Value.Nombre;

            CargarBodegas();
            LimpiarFormulario();
            ActualizarEstadoBotones();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA DE DATOS
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Carga bodegas del tenant activo y las asigna al DataSource del grid.
        /// </summary>
        private void CargarBodegas()
        {
            _bodegaRepo ??= _unitOfWork.GetCustomRepository<IBodegaRepository>();

            var result = _bodegaRepo.GetByTenant(IdTenant, soloActivas: false);
            if (!result.IsSuccess)
            {
                NotificationManager.Error($"Error al cargar bodegas: {result.Error?.Message}");
                return;
            }

            var lista = result.Value.ToList();
            dgvBodegas.DataSource = lista;
            ActualizarContador(lista.Count);
        }

        private void ActualizarContador(int total)
        {
            tsLblContador.Text = total == 1 ? "1 bodega" : $"{total} bodegas";
        }

        // ═════════════════════════════════════════════════════════════════════
        // ATAJOS DE TECLADO
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_Bodega_KeyDown(object? sender, KeyEventArgs e)
        {
            // Ctrl+S — Guardar
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                BtnGuardar_Click(sender!, EventArgs.Empty);
                return;
            }
            // F5 — Refrescar
            if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                BtnRefrescar_Click(sender!, EventArgs.Empty);
                return;
            }
            // Escape — Cancelar
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                BtnCancelar_Click(sender!, EventArgs.Empty);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // VALIDACIONES
        // ═════════════════════════════════════════════════════════════════════

        private bool ValidarCodigo()
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                txtCodigo.BackColor = Color.LightPink;
                return false;
            }
            if (txtCodigo.Text.Length < 3)
            {
                txtCodigo.BackColor = Color.LightYellow;
                return false;
            }
            txtCodigo.BackColor = Color.White;
            return true;
        }

        private bool ValidarNombre()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                txtNombre.BackColor = Color.LightPink;
                return false;
            }
            if (txtNombre.Text.Length < 5)
            {
                txtNombre.BackColor = Color.LightYellow;
                return false;
            }
            txtNombre.BackColor = Color.White;
            return true;
        }

        /// <summary>
        /// Verificación de unicidad de código al perder el foco.
        /// Da retroalimentación inmediata sin esperar a "Guardar".
        /// </summary>
        private void TxtCodigo_Leave(object? sender, EventArgs e)
        {
            if (_cargando || !ValidarCodigo()) return;

            _bodegaRepo ??= _unitOfWork.GetCustomRepository<IBodegaRepository>();

            var existsResult = _bodegaRepo.ExistsByCodigo(IdTenant, txtCodigo.Text.Trim());
            if (!existsResult.IsSuccess || !existsResult.Value) return;

            if (_esNuevo)
            {
                // Nuevo: cualquier coincidencia es un conflicto
                txtCodigo.BackColor = Color.LightPink;
                return;
            }

            // Edición: conflicto solo si el código pertenece a OTRA bodega
            var bodegaResult = _bodegaRepo.GetByCodigo(IdTenant, txtCodigo.Text.Trim());
            if (bodegaResult.IsSuccess &&
                bodegaResult.Value != null &&
                _bodegaEnEdicion != null &&
                bodegaResult.Value.IdBodega != _bodegaEnEdicion.IdBodega)
            {
                txtCodigo.BackColor = Color.LightPink;
            }
        }

        /// <summary>
        /// Valida los campos del formulario. Incluye verificación de unicidad de código.
        /// </summary>
        private bool ValidarDatos()
        {
            bool esValido = true;
            var errores = new System.Text.StringBuilder();

            if (!ValidarCodigo())
            {
                errores.AppendLine("• El codigo es obligatorio y debe tener al menos 3 caracteres.");
                esValido = false;
            }
            else
            {
                _bodegaRepo ??= _unitOfWork.GetCustomRepository<IBodegaRepository>();

                var existsResult = _bodegaRepo.ExistsByCodigo(IdTenant, txtCodigo.Text.Trim());
                if (existsResult.IsSuccess && existsResult.Value)
                {
                    if (_esNuevo)
                    {
                        errores.AppendLine("• Ya existe una bodega con este codigo en el tenant.");
                        txtCodigo.BackColor = Color.LightPink;
                        esValido = false;
                    }
                    else
                    {
                        // La entidad en edición está disponible directamente — sin round-trip adicional
                        var bodegaResult = _bodegaRepo.GetByCodigo(IdTenant, txtCodigo.Text.Trim());
                        if (bodegaResult.IsSuccess &&
                            bodegaResult.Value != null &&
                            _bodegaEnEdicion != null &&
                            bodegaResult.Value.IdBodega != _bodegaEnEdicion.IdBodega)
                        {
                            errores.AppendLine("• Ya existe otra bodega con este codigo en el tenant.");
                            txtCodigo.BackColor = Color.LightPink;
                            esValido = false;
                        }
                    }
                }
                else if (existsResult.IsFailure)
                {
                    NotificationManager.Warning($"No se pudo verificar unicidad del codigo: {existsResult.Error?.Message}");
                }
            }

            if (!ValidarNombre())
            {
                errores.AppendLine("• El nombre es obligatorio y debe tener al menos 5 caracteres.");
                esValido = false;
            }

            if (!esValido)
                NotificationManager.Warning($"Por favor corrija los siguientes errores:\n\n{errores}");

            return esValido;
        }

        // ═════════════════════════════════════════════════════════════════════
        // OPERACIONES CRUD
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifica cambios pendientes antes de limpiar, igual que BtnCancelar_Click.
        /// </summary>
        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos y crear una nueva bodega?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            LimpiarFormulario();
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (dgvBodegas.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar una bodega para editar.");
                return;
            }

            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos y editar la bodega seleccionada?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var bodega = (Bodega)dgvBodegas.SelectedRows[0].DataBoundItem;
            CargarBodegaEnEdicion(bodega);
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvBodegas.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar una bodega para eliminar.");
                return;
            }

            var bodega = (Bodega)dgvBodegas.SelectedRows[0].DataBoundItem;
            if (MessageBox.Show($"¿Esta seguro de eliminar la bodega '{bodega.Nombre}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _bodegaRepo ??= _unitOfWork.GetCustomRepository<IBodegaRepository>();
            var removeResult = _bodegaRepo.Remove(bodega);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar: {removeResult.Error?.Message}");
                return;
            }

            try
            {

                _unitOfWork.SaveChanges();
                NotificationManager.Success("Bodega eliminada correctamente.");
                CargarBodegas();
                LimpiarFormulario();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al guardar cambios: {ex.Message}");
            }
        }

        private void BtnRefrescar_Click(object sender, EventArgs e) => CargarBodegas();

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;

            _bodegaRepo ??= _unitOfWork.GetCustomRepository<IBodegaRepository>();

            try
            {
                if (_esNuevo)
                {
                    var nueva = new Bodega
                    {
                        IdTenant = IdTenant,

                        Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                        Nombre = txtNombre.Text.Trim().ToUpper(),
                        Activo = chkActivo.Checked
                    };
                    var addResult = _bodegaRepo.Add(nueva);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al crear bodega: {addResult.Error?.Message}");
                        return;
                    }
                }
                else
                {
                    // Usar la entidad vinculada directamente — sin round-trip GetById
                    if (_bodegaEnEdicion == null)
                    {
                        NotificationManager.Error("No hay bodega en edicion.");
                        return;
                    }

                    _bodegaEnEdicion.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                    _bodegaEnEdicion.Nombre = txtNombre.Text.Trim().ToUpper();
                    _bodegaEnEdicion.Activo = chkActivo.Checked;

                    var updateResult = _bodegaRepo.Update(_bodegaEnEdicion);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar bodega: {updateResult.Error?.Message}");
                        return;
                    }
                }

                _unitOfWork.SaveChanges();
                NotificationManager.Success(_esNuevo ? "Bodega creada exitosamente." : "Bodega actualizada exitosamente.");

                CargarBodegas();
                LimpiarFormulario();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Ha ocurrido un error inesperado: {ex.Message}");
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (HayCambios() &&
                MessageBox.Show("¿Esta seguro que desea cancelar? Los cambios no guardados se perderan.", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            LimpiarFormulario();
            ActualizarEstadoBotones();
            dgvBodegas.ClearSelection();
        }

        // ═════════════════════════════════════════════════════════════════════
        // SELECCIÓN EN GRILLA
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Actualiza el estado de botones Editar/Eliminar según la selección.
        /// </summary>
        private void DgvBodegas_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarEstadoBotones();
        }

        // ═════════════════════════════════════════════════════════════════════
        // MÉTODOS AUXILIARES
        // ═════════════════════════════════════════════════════════════════════

        private void LimpiarFormulario()
        {
            _cargando = true;
            try
            {
                txtCodigo.Clear();
                txtNombre.Clear();
                txtCodigo.BackColor = Color.White;
                txtNombre.BackColor = Color.White;
                chkActivo.Checked = true;
                lblIdValor.Text = "<< Nuevo >>";
                lblIdTenantValor.Text = IdTenant.ToString();

                _esNuevo = true;
                _bodegaEnEdicion = null;

                GuardarSnapshot();
            }
            finally
            {
                _cargando = false;
            }
        }

        /// <summary>
        /// Carga la entidad seleccionada en el formulario para su edición.
        /// </summary>
        private void CargarBodegaEnEdicion(Bodega bodega)
        {
            _cargando = true;
            try
            {
                _bodegaEnEdicion = bodega;
                _esNuevo = false;

                txtCodigo.Text = bodega.Codigo;
                txtNombre.Text = bodega.Nombre;
                chkActivo.Checked = bodega.Activo;
                lblIdValor.Text = bodega.IdBodega.ToString();
                lblIdTenantValor.Text = IdTenant.ToString();

                GuardarSnapshot();
            }
            finally
            {
                _cargando = false;
            }
        }

        /// <summary>Guarda los valores actuales del formulario para comparación posterior.</summary>
        private void GuardarSnapshot()
        {
            _snapCodigo = txtCodigo.Text;
            _snapNombre = txtNombre.Text;
            _snapActivo = chkActivo.Checked;
        }

        /// <summary>
        /// Detecta si hay cambios sin guardar comparando con el snapshot guardado al cargar.
        /// </summary>
        private bool HayCambios()
        {
            if (_esNuevo)
                return !string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                       !string.IsNullOrWhiteSpace(txtNombre.Text);

            return txtCodigo.Text != _snapCodigo ||
                   txtNombre.Text != _snapNombre ||
                   chkActivo.Checked != _snapActivo;
        }

        /// <summary>
        /// Centraliza el estado habilitado/deshabilitado de todos los controles.
        /// Centraliza el estado habilitado/deshabilitado de todos los controles.
        /// </summary>
        private void ActualizarEstadoBotones()
        {
            bool haySeleccion = dgvBodegas.SelectedRows.Count > 0;

            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;

            // Guardar y Cancelar siempre disponibles cuando hay edición activa
            // (el formulario siempre permite crear/editar)
            btnGuardar.Enabled = true;
            btnCancelar.Enabled = true;
        }
    }
}


