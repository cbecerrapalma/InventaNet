using CBP.Data.Abstractions;
using CBP.WinFrms.Notification;
using Inventa.Application.Services;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.Helpers;
using System.ComponentModel;

namespace Inventa.Forms.Productos
{
    public partial class frm_Precio : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly IUnitOfWorkSync _unitOfWork;

        // ── Repositorio (lazy) ────────────────────────────────────────────────
        private IPrecioRepository? _precioRepo;

        // ── Entidad en edición cacheada — evita segundo GetById en BtnGuardar ──
        private Precio? _precioEnEdicion;

        // S57.69: precio normal único — sin selector de tipo (combo oculto,
        // Designer intacto). Mayorista se edita en frm_PrecioMayorista.

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
        /// IdPresProd al que pertenecerá el nuevo precio.
        /// Obligatorio en modo creación (debe ser mayor a 0).
        /// En modo edición se sobreescribe con el valor de la entidad cargada.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long IdPresProd { get; set; }

        /// <summary>
        /// null → crear nuevo precio.
        /// &gt; 0  → editar precio existente (los datos se cargan desde BD en Load).
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long? IdPrecio { get; set; }

        /// <summary>
        /// Descripción legible de la presentación, construida por el llamador.
        /// Se muestra en el panel de contexto del formulario.
        /// Ejemplo: "Agua Mineral - 500ml (UND)"
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DescripcionPresentacion { get; set; } = string.Empty;

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_Precio(
            IUnitOfWorkSync unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;

            this.KeyPreview = true;
            this.KeyDown += Frm_Precio_KeyDown;

            // S57.69: sin selector de tipo (precio normal único).
            cboTipoPrecio.Visible = false;
            lblTipoPrecio.Visible = false;
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

        private void Frm_Precio_Load(object sender, EventArgs e)
        {
            // Mostrar contexto de presentación en el header
            ActualizarPanelContexto();

            if (IdPrecio.HasValue)
            {
                CargarPrecioEdicion(IdPrecio.Value);
            }
            else
            {
                this.Text = "Nuevo Precio de Venta";
                GuardarSnapshot();
            }
        }

        /// <summary>
        /// Muestra en el panel de contexto la presentación y el precio vigente actual.
        /// El nombre de la presentación viene del llamador (sin query extra).
        /// El precio vigente se consulta solo si la presentación es conocida.
        /// </summary>
        private void ActualizarPanelContexto()
        {
            if (!string.IsNullOrWhiteSpace(DescripcionPresentacion))
            {
                lblContextoPresentacion.Text = DescripcionPresentacion;
            }
            else if (IdPrecio.HasValue)
            {
                // Modo edición sin descripción provista — se actualiza luego al cargar entidad
                lblContextoPresentacion.Text = "Cargando...";
            }
            else
            {
                lblContextoPresentacion.Text = "(Presentacion no identificada)";
            }

            // Precio normal vigente actual: se consulta para dar contexto al usuario
            if (IdPresProd > 0)
            {
                _precioRepo ??= _unitOfWork.GetCustomRepository<IPrecioRepository>();
                var vigResult = _precioRepo.GetValorVigente(IdPresProd);
                if (vigResult.IsSuccess && vigResult.Value.HasValue)
                    lblContextoPrecioVigente.Text = $"Precio vigente: {vigResult.Value:C2}";
                else
                    lblContextoPrecioVigente.Text = "Sin precio vigente";
            }
            else
            {
                lblContextoPrecioVigente.Text = string.Empty;
            }
        }

        /// <summary>
        /// Carga el precio existente desde BD, puebla los controles y cachea la entidad.
        /// La entidad se reutiliza en BtnGuardar_Click para evitar un segundo GetById.
        /// </summary>
        private void CargarPrecioEdicion(long idPrecio)
        {
            _precioRepo ??= _unitOfWork.GetCustomRepository<IPrecioRepository>();
            var result = _precioRepo.GetById(idPrecio);

            if (!result.IsSuccess || result.Value == null)
            {
                NotificationManager.Error("No se pudo cargar el precio para edicion.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            _cargando = true;
            try
            {
                _precioEnEdicion = result.Value;
                var precio = _precioEnEdicion;

                numValor.Value = Math.Min(precio.Valor, numValor.Maximum);

                // FecVig es DateOnly — convertir a DateTime para DateTimePicker
                dtpFechaVigencia.Value = precio.FecVig.ToDateTime(TimeOnly.MinValue);

                chkActivo.Checked = precio.Activo;

                // IdPresProd se obtiene de la entidad (más seguro que el parámetro externo)
                IdPresProd = precio.IdPresProd;

                this.Text = "Editar Precio de Venta";

                GuardarSnapshot();

                // Actualizar precio vigente ahora que tenemos el IdPresProd correcto
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

        private void Frm_Precio_KeyDown(object? sender, KeyEventArgs e)
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

            if (!IdPrecio.HasValue && IdPresProd <= 0)
            {
                NotificationManager.Warning("Presentacion no valida. Seleccione una presentacion antes de agregar un precio.");
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

            // Advertencia de sobreescritura: ya existe precio normal vigente
            _precioRepo ??= _unitOfWork.GetCustomRepository<IPrecioRepository>();
            var vigResult = _precioRepo.ExisteVigente(IdPresProd);

            if (vigResult.IsSuccess && vigResult.Value)
            {
                // En edición no se advierte (se edita la fila existente); en
                // creación se confirma porque el anterior quedará desplazado.
                if (_precioEnEdicion == null)
                {
                    if (MessageBox.Show(
                        $"Ya existe un precio vigente para esta presentacion.\n" +
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

            _precioRepo ??= _unitOfWork.GetCustomRepository<IPrecioRepository>();

            // FecVig es DateOnly en la entidad
            var fechaVig = DateOnly.FromDateTime(dtpFechaVigencia.Value);

            try
            {
                // S57.62 M8.1-C: el servicio controla commit (uno por
                // operación); la UI solo mapea Result (sin cierre ante Failure).
                var writeSvc = new PrecioWriteService(_precioRepo, _unitOfWork);
                if (!IdPrecio.HasValue)
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
                    if (_precioEnEdicion == null)
                    {
                        NotificationManager.Error("No hay precio en edicion.");
                        return;
                    }

                    var rUpdate = writeSvc.Update(
                        _precioEnEdicion.IdPrecio,
                        numValor.Value,
                        fechaVig,
                        chkActivo.Checked);
                    if (rUpdate.IsFailure)
                    {
                        NotificationManager.Error(PrecioWriteUiMapper.ToMessage(rUpdate.Error));
                        return;
                    }
                }

                NotificationManager.Success(IdPrecio.HasValue
                    ? "Precio actualizado exitosamente."
                    : "Precio creado exitosamente.");

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
            if (!IdPrecio.HasValue)
                return numValor.Value > 0; // nuevo con precio ingresado = cambio

            return numValor.Value != _snapValor ||
                   DateOnly.FromDateTime(dtpFechaVigencia.Value) != _snapFecha ||
                   chkActivo.Checked != _snapActivo;
        }
    }
}


