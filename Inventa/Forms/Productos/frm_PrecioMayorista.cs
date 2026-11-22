using CBP.Data.Abstractions;
using CBP.WinFrms.Notification;
using Inventa.Application.Services;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.Helpers;
using System.ComponentModel;

namespace Inventa.Forms.Productos
{
    // S57.69 F-5 (NEW): superficie WinForms para PrecioMayorista (relación
    // dedicada, sin discriminator). Espejo de frm_Precio en modo normal-only:
    // Valor + FecVig + Activo. Sin combo de tipo (prohibido reintroducirlo).
    public partial class frm_PrecioMayorista : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly IUnitOfWorkSync _unitOfWork;

        // ── Repositorio (lazy) ────────────────────────────────────────────────
        private IPrecioMayoristaRepository? _repo;

        // ── Entidad en edición cacheada — evita segundo GetById en BtnGuardar ──
        private PrecioMayorista? _entidadEnEdicion;

        // ── Snapshot para detección de cambios ────────────────────────────────
        private decimal _snapValor;
        private DateOnly _snapFecha;
        private bool _snapActivo;

        // ── Guard de carga ────────────────────────────────────────────────────
        private bool _cargando;

        // ═════════════════════════════════════════════════════════════════════
        // PROPIEDADES PÚBLICAS — establecidas por el formulario padre
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// IdPresProd al que pertenecerá el nuevo precio mayorista.
        /// Obligatorio en modo creación (debe ser mayor a 0).
        /// En modo edición se sobreescribe con el valor de la entidad cargada.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long IdPresProd { get; set; }

        /// <summary>
        /// null → crear nuevo precio mayorista.
        /// &gt; 0  → editar existente (los datos se cargan desde BD en Load).
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long? IdPrecioMayorista { get; set; }

        /// <summary>
        /// Descripción legible de la presentación, construida por el llamador.
        /// Se muestra en el panel de contexto del formulario.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DescripcionPresentacion { get; set; } = string.Empty;

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_PrecioMayorista(
            IUnitOfWorkSync unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;

            this.KeyPreview = true;
            this.KeyDown += Frm_PrecioMayorista_KeyDown;

            ConfigurarEventos();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarEventos()
        {
            dtpFechaVigencia.Value = DateTime.Today;
            numValor.ValueChanged += NumValor_ValueChanged;
            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += BtnCancelar_Click;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_PrecioMayorista_Load(object sender, EventArgs e)
        {
            // Mostrar contexto de presentación en el header
            ActualizarPanelContexto();

            if (IdPrecioMayorista.HasValue)
            {
                CargarEdicion(IdPrecioMayorista.Value);
            }
            else
            {
                this.Text = "Nuevo Precio Mayorista";
                GuardarSnapshot();
            }
        }

        /// <summary>
        /// Muestra en el panel de contexto la presentación y el precio mayorista
        /// vigente actual. El nombre de la presentación viene del llamador (sin
        /// query extra). El vigente se consulta solo si la presentación es conocida.
        /// </summary>
        private void ActualizarPanelContexto()
        {
            if (!string.IsNullOrWhiteSpace(DescripcionPresentacion))
            {
                lblContextoPresentacion.Text = DescripcionPresentacion;
            }
            else if (IdPrecioMayorista.HasValue)
            {
                // Modo edición sin descripción provista — se actualiza luego al cargar entidad
                lblContextoPresentacion.Text = "Cargando...";
            }
            else
            {
                lblContextoPresentacion.Text = "(Presentacion no identificada)";
            }

            // Precio mayorista vigente actual: se consulta para dar contexto al usuario
            if (IdPresProd > 0)
            {
                _repo ??= _unitOfWork.GetCustomRepository<IPrecioMayoristaRepository>();
                var vigResult = _repo.GetValorVigente(IdPresProd);
                if (vigResult.IsSuccess && vigResult.Value.HasValue)
                    lblContextoPrecioVigente.Text = $"Mayorista vigente: {vigResult.Value:C2}";
                else
                    lblContextoPrecioVigente.Text = "Sin precio mayorista vigente";
            }
            else
            {
                lblContextoPrecioVigente.Text = string.Empty;
            }
        }

        /// <summary>
        /// Carga el precio mayorista existente desde BD, puebla los controles y
        /// cachea la entidad. La entidad se reutiliza en BtnGuardar_Click para
        /// evitar un segundo GetById.
        /// </summary>
        private void CargarEdicion(long idPrecioMayorista)
        {
            _repo ??= _unitOfWork.GetCustomRepository<IPrecioMayoristaRepository>();
            var result = _repo.GetById(idPrecioMayorista);

            if (!result.IsSuccess || result.Value == null)
            {
                NotificationManager.Error("No se pudo cargar el precio mayorista para edicion.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _cargando = true;
            try
            {
                _entidadEnEdicion = result.Value;
                var precio = _entidadEnEdicion;

                numValor.Value = Math.Min(precio.Valor, numValor.Maximum);

                // FecVig es DateOnly — convertir a DateTime para DateTimePicker
                dtpFechaVigencia.Value = precio.FecVig.ToDateTime(TimeOnly.MinValue);

                chkActivo.Checked = precio.Activo;

                // IdPresProd se obtiene de la entidad (más seguro que el parámetro externo)
                IdPresProd = precio.IdPresProd;

                this.Text = "Editar Precio Mayorista";

                GuardarSnapshot();

                // Actualizar vigente ahora que tenemos el IdPresProd correcto
                ActualizarPanelContexto();
            }
            finally
            {
                _cargando = false;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // ATAJOS DE TECLADO
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_PrecioMayorista_KeyDown(object? sender, KeyEventArgs e)
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
        // VALIDACIÓN EN TIEMPO REAL
        // ═════════════════════════════════════════════════════════════════════

        private void NumValor_ValueChanged(object? sender, EventArgs e)
        {
            if (_cargando) return;
            numValor.BackColor = numValor.Value <= 0
                ? Color.LightPink
                : Color.White;
        }

        // ═════════════════════════════════════════════════════════════════════
        // VALIDACIÓN DE NEGOCIO
        // ═════════════════════════════════════════════════════════════════════

        private bool ValidarDatos()
        {
            bool esValido = true;
            var errores = new System.Text.StringBuilder();

            if (!IdPrecioMayorista.HasValue && IdPresProd <= 0)
            {
                NotificationManager.Warning("Presentacion no valida. Seleccione una presentacion antes de agregar un precio mayorista.");
                return false;
            }

            if (numValor.Value <= 0)
            {
                errores.AppendLine("• El precio debe ser mayor a cero.");
                numValor.BackColor = Color.LightPink;
                numValor.Focus();
                esValido = false;
            }

            if (!esValido)
            {
                NotificationManager.Warning($"Por favor corrija los siguientes errores:\n\n{errores}");
                return false;
            }

            // Advertencia de fecha pasada (no bloquea si el usuario confirma)
            if (dtpFechaVigencia.Value.Date < DateTime.Today)
            {
                if (MessageBox.Show("La fecha de vigencia es anterior a hoy. ¿Desea continuar de todas formas?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    dtpFechaVigencia.Focus();
                    return false;
                }
            }

            // Advertencia de sobreescritura: ya existe mayorista vigente
            _repo ??= _unitOfWork.GetCustomRepository<IPrecioMayoristaRepository>();
            var vigResult = _repo.ExisteVigente(IdPresProd);

            if (vigResult.IsSuccess && vigResult.Value)
            {
                // En edición no se advierte (se edita la fila existente); en
                // creación se confirma porque el anterior quedará desplazado.
                if (_entidadEnEdicion == null)
                {
                    if (MessageBox.Show(
                        $"Ya existe un precio mayorista vigente para esta presentacion.\n" +
                        $"¿Desea crear uno nuevo de todas formas? El anterior quedara inactivo cuando este entre en vigencia.", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return false;
                }
            }

            return true;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CRUD
        // ═════════════════════════════════════════════════════════════════════

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;

            _repo ??= _unitOfWork.GetCustomRepository<IPrecioMayoristaRepository>();

            // FecVig es DateOnly en la entidad
            var fechaVig = DateOnly.FromDateTime(dtpFechaVigencia.Value);

            try
            {
                // El servicio controla commit (uno por operación); la UI solo
                // mapea Result (sin cierre ante Failure).
                var writeSvc = new PrecioMayoristaWriteService(_repo, _unitOfWork);
                if (!IdPrecioMayorista.HasValue)
                {
                    // ── CREAR ──────────────────────────────────────────────────
                    var rCreate = writeSvc.Create(
                        IdPresProd,
                        numValor.Value,
                        fechaVig,
                        chkActivo.Checked);
                    if (rCreate.IsFailure)
                    {
                        NotificationManager.Error(PrecioWriteUiMapper.ToMessage(rCreate.Error));
                        return;
                    }
                }
                else
                {
                    // ── EDITAR ─────────────────────────────────────────────────
                    if (_entidadEnEdicion == null)
                    {
                        NotificationManager.Error("No hay precio mayorista en edicion.");
                        return;
                    }

                    var rUpdate = writeSvc.Update(
                        _entidadEnEdicion.IdPrecioMayorista,
                        numValor.Value,
                        fechaVig,
                        chkActivo.Checked);
                    if (rUpdate.IsFailure)
                    {
                        NotificationManager.Error(PrecioWriteUiMapper.ToMessage(rUpdate.Error));
                        return;
                    }
                }

                NotificationManager.Success(IdPrecioMayorista.HasValue
                    ? "Precio mayorista actualizado exitosamente."
                    : "Precio mayorista creado exitosamente.");

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error inesperado: {ex.Message}");
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea cerrar de todas formas?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ═════════════════════════════════════════════════════════════════════
        // DETECCIÓN DE CAMBIOS
        // ═════════════════════════════════════════════════════════════════════

        private void GuardarSnapshot()
        {
            _snapValor = numValor.Value;
            _snapFecha = DateOnly.FromDateTime(dtpFechaVigencia.Value);
            _snapActivo = chkActivo.Checked;
        }

        private bool HayCambios()
        {
            if (!IdPrecioMayorista.HasValue)
                return numValor.Value > 0; // nuevo con precio ingresado = cambio

            return numValor.Value != _snapValor ||
                   DateOnly.FromDateTime(dtpFechaVigencia.Value) != _snapFecha ||
                   chkActivo.Checked != _snapActivo;
        }
    }
}
