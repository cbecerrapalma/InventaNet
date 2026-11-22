using CBP.Data.Abstractions;
using CBP.MultiTenant.Abstractions;
using CBP.MultiTenant.Extensions;
using CBP.WinFrms.Notification;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using System.ComponentModel;

namespace Inventa.Forms.Productos
{
    public partial class frm_CodigoBarra : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IUnitOfWorkSync _unitOfWork;

        // ── Repositorios (lazy) ───────────────────────────────────────────────
        private ICodBarRepository? _codBarRepo;
        private IPresProdRepository? _presProdRepo;

        // ── Tipo fuertemente tipado para cboTipoCodigo — reemplaza anonymous+dynamic ──
        private sealed record TipoCodItem(byte Valor, string Texto)
        {
            public override string ToString() => Texto;
        }

        // ── DTO aplanado para el grid — reemplaza DataPropertyName sobre objeto complejo ──
        // GetByPresentacion no incluye la cadena PresProd.VarProd.Prod; el DTO evita
        // NullReferenceException en CellFormatting al acceder a propiedades de navegación.
        private sealed record CodBarItem(
            long IdCodBar,
            string Codigo,
            byte TipoCod,
            bool Principal,
            long IdPresProd,
            string DescripcionPresentacion);

        // ── Lista interna tipada ──────────────────────────────────────────────────
        private List<CodBarItem> _codigosLista = new();

        // ── Estado del formulario ─────────────────────────────────────────────
        /// <summary>true = creando nuevo código; false = editando existente.</summary>
        private bool _esNuevo = true;

        /// <summary>Código actualmente en edición. null cuando _esNuevo = true.</summary>
        private CodBarItem? _codBarEnEdicion;

        /// <summary>
        /// true cuando el formulario se abrió desde una presentación concreta
        /// (IdPresProd > 0). El selector de presentación se oculta.
        /// </summary>
        private bool EsModal => IdPresProd > 0;

        // ── Snapshot para detección de cambios ────────────────────────────────
        private string? _snapCodigo;
        private byte _snapTipoCod;
        private bool _snapPrincipal;
        private long _snapIdPresProd;

        // ── Guard de carga ────────────────────────────────────────────────────
        private bool _cargando;

        // ── Propiedades públicas establecidas por el formulario padre ─────────
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long IdPresProd { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long? IdCodBar { get; set; }

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_CodigoBarra(
            IUnitOfWorkSync unitOfWork,
            ITenantContext tenantContext)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;

            this.KeyPreview = true;
            this.KeyDown += Frm_CodigoBarra_KeyDown;

            ConfigurarComboTipo();
            ConfigurarColumnasGrilla();
            ConfigurarEventos();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarComboTipo()
        {
            // Tipo fuertemente tipado — elimina todos los accesos dynamic
            cboTipoCodigo.DataSource = new List<TipoCodItem>
            {
                new(1, "Normal"),
                new(2, "Pesable"),
                new(3, "Interno"),
                new(4, "Proveedor")
            };
            cboTipoCodigo.DisplayMember = nameof(TipoCodItem.Texto);
            cboTipoCodigo.ValueMember = nameof(TipoCodItem.Valor);
            cboTipoCodigo.SelectedIndex = 0;

            // Combo presentación (solo visible en modo standalone)
            cboPresentacion.DisplayMember = nameof(PresItem.Descripcion);
            cboPresentacion.ValueMember = nameof(PresItem.IdPresProd);
        }

        private void ConfigurarColumnasGrilla()
        {
            dgvCodigos.AutoGenerateColumns = false;
            dgvCodigos.Columns.Clear();

            dgvCodigos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "ID",
                DataPropertyName = nameof(CodBarItem.IdCodBar),
                Visible = false
            });
            dgvCodigos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCodigo",
                HeaderText = "Codigo",
                DataPropertyName = nameof(CodBarItem.Codigo),
                Width = 200
            });
            dgvCodigos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTipo",
                HeaderText = "Tipo",
                DataPropertyName = nameof(CodBarItem.TipoCod),
                Width = 100
            });
            dgvCodigos.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colPrincipal",
                HeaderText = "Principal",
                DataPropertyName = nameof(CodBarItem.Principal),
                Width = 80
            });
            // Columna presentación — solo visible en modo standalone.
            // Usa la propiedad aplanada del DTO — no depende de navegación EF.
            dgvCodigos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colPresentacion",
                HeaderText = "Presentacion",
                DataPropertyName = nameof(CodBarItem.DescripcionPresentacion),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Formatear TipoCod (byte) como texto sin CellFormatting sobre objetos complejos
            dgvCodigos.CellFormatting += DgvCodigos_CellFormatting;
        }

        private void ConfigurarEventos()
        {
            txtCodigo.TextChanged += (s, e) => { if (!_cargando) ValidarCodigo(); };
            txtCodigo.Leave += TxtCodigo_Leave;
            dgvCodigos.SelectionChanged += DgvCodigos_SelectionChanged;
        }

        // ── Tipo auxiliar para cboPresentacion ────────────────────────────────
        private sealed record PresItem(long IdPresProd, string Descripcion)
        {
            public override string ToString() => Descripcion;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_CodigoBarra_Load(object sender, EventArgs e)
        {
            var tenantResult = _tenantContext.TryGetCurrent();
            if (tenantResult.IsFailure)
            {
                NotificationManager.Error("No hay empresa activa. Reinicie la aplicacion.");
                Close();
                return;
            }

            int idTenant = tenantResult.Value.Id;

            if (EsModal)
            {
                // Modo modal: la presentación es fija — ocultar selector
                lblPresentacionSelector.Visible = false;
                cboPresentacion.Visible = false;
                lblPresProdValor.Text = IdPresProd.ToString();
                lblPresProdValor.Visible = true;
                lblPresentacionFija.Visible = true;
                // Ocultar columna presentación — es siempre la misma
                if (dgvCodigos.Columns["colPresentacion"] != null)
                    dgvCodigos.Columns["colPresentacion"]!.Visible = false;

                CargarCodigosPorPresentacion(IdPresProd);
            }
            else
            {
                // Modo standalone: mostrar selector de presentación
                lblPresentacionSelector.Visible = true;
                cboPresentacion.Visible = true;
                lblPresProdValor.Visible = false;
                lblPresentacionFija.Visible = false;

                CargarPresentaciones(idTenant);
                CargarCodigosPorTenant(idTenant);
            }

            if (IdCodBar.HasValue)
            {
                // Se abrió directamente en modo edición
                CargarCodigoEnEdicion(IdCodBar.Value);
                ActualizarEstadoBotones();
            }
            else
            {
                LimpiarFormulario();
                ActualizarEstadoBotones();
                txtCodigo.Focus();
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA DE DATOS
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Carga las presentaciones activas del tenant en cboPresentacion.
        /// GetByTenant incluye VarProd.Prod y Unidad — suficiente para el texto descriptivo.
        /// </summary>
        private void CargarPresentaciones(int idTenant)
        {
            _presProdRepo ??= _unitOfWork.GetCustomRepository<IPresProdRepository>();
            var result = _presProdRepo.GetByTenant(idTenant, soloActivas: true);
            if (!result.IsSuccess)
            {
                NotificationManager.Error($"Error al cargar presentaciones: {result.Error?.Message}");
                return;
            }

            var items = result.Value
                .Select(p => new PresItem(
                    p.IdPresProd,
                    $"{p.VarProd.Prod.Nombre} - {p.VarProd.Nombre} - {p.Descrip} ({p.Unidad.Codigo})"))
                .ToList();

            cboPresentacion.DataSource = items;
            if (cboPresentacion.Items.Count > 0)
                cboPresentacion.SelectedIndex = 0;
        }

        /// <summary>
        /// Carga códigos de una presentación concreta (modo modal).
        /// GetByPresentacion NO incluye navegación — el DTO proyecta
        /// DescripcionPresentacion vacío porque no es necesario en este modo.
        /// </summary>
        private void CargarCodigosPorPresentacion(long idPresProd)
        {
            _codBarRepo ??= _unitOfWork.GetCustomRepository<ICodBarRepository>();
            var result = _codBarRepo.GetByPresentacion(idPresProd);
            if (!result.IsSuccess)
            {
                NotificationManager.Error($"Error al cargar codigos de barra: {result.Error?.Message}");
                return;
            }

            _codigosLista = result.Value.Select(c => new CodBarItem(
                c.IdCodBar,
                c.Codigo,
                c.TipoCod,
                c.Principal,
                c.IdPresProd,
                string.Empty   // no se muestra la columna en modo modal
            )).ToList();

            BindGrid();
        }

        /// <summary>
        /// Carga todos los códigos del tenant (modo standalone).
        /// GetByTenant incluye PresProd.VarProd.Prod — el DTO puede construir
        /// DescripcionPresentacion correctamente.
        /// </summary>
        private void CargarCodigosPorTenant(int idTenant)
        {
            _codBarRepo ??= _unitOfWork.GetCustomRepository<ICodBarRepository>();
            var result = _codBarRepo.GetByTenant(idTenant);
            if (!result.IsSuccess)
            {
                NotificationManager.Error($"Error al cargar codigos de barra: {result.Error?.Message}");
                return;
            }

            _codigosLista = result.Value.Select(c => new CodBarItem(
                c.IdCodBar,
                c.Codigo,
                c.TipoCod,
                c.Principal,
                c.IdPresProd,
                c.PresProd != null
                    ? $"{c.PresProd.VarProd?.Prod?.Nombre} - {c.PresProd.VarProd?.Nombre} - {c.PresProd.Descrip}"
                    : string.Empty
            )).ToList();

            BindGrid();
        }

        private void BindGrid()
        {
            dgvCodigos.DataSource = null;
            dgvCodigos.DataSource = _codigosLista;
        }

        /// <summary>
        /// Carga un código directamente desde la lista interna (evita round-trip GetById).
        /// Si no está en la lista — por ejemplo al abrir con IdCodBar desde exterior — hace GetById.
        /// </summary>
        private void CargarCodigoEnEdicion(long idCodBar)
        {
            // Buscar primero en la lista ya cargada
            var item = _codigosLista.FirstOrDefault(c => c.IdCodBar == idCodBar);

            if (item == null)
            {
                // Carga directa desde BD (solo cuando se abre el formulario con IdCodBar)
                _codBarRepo ??= _unitOfWork.GetCustomRepository<ICodBarRepository>();
                var result = _codBarRepo.GetById(idCodBar);
                if (!result.IsSuccess || result.Value == null)
                {
                    NotificationManager.Error("No se pudo cargar el codigo de barra para edicion.");
                    Close();
                    return;
                }

                var c = result.Value;
                item = new CodBarItem(c.IdCodBar, c.Codigo, c.TipoCod, c.Principal, c.IdPresProd, string.Empty);
            }

            _cargando = true;
            try
            {
                _codBarEnEdicion = item;
                _esNuevo = false;

                txtCodigo.Text = item.Codigo;
                txtCodigo.BackColor = Color.White;

                // Seleccionar tipo en cboTipoCodigo usando el ValueMember (byte Valor)
                cboTipoCodigo.SelectedValue = item.TipoCod;
                if (cboTipoCodigo.SelectedValue == null)
                    cboTipoCodigo.SelectedIndex = 0;

                chkPrincipal.Checked = item.Principal;
                lblIdValor.Text = item.IdCodBar.ToString();

                // Seleccionar presentación en cboPresentacion (solo modo standalone)
                if (!EsModal && cboPresentacion.DataSource is List<PresItem> presItems)
                {
                    var pres = presItems.FirstOrDefault(p => p.IdPresProd == item.IdPresProd);
                    if (pres != null)
                        cboPresentacion.SelectedValue = pres.IdPresProd;
                }

                GuardarSnapshot();
            }
            finally
            {
                _cargando = false;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // ATAJOS DE TECLADO
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_CodigoBarra_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                BtnGuardar_Click(sender!, EventArgs.Empty);
                return;
            }
            if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                BtnRefrescar_Click(sender!, EventArgs.Empty);
                return;
            }
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
            if (txtCodigo.Text.Length < 5)
            {
                txtCodigo.BackColor = Color.LightYellow;
                return false;
            }
            txtCodigo.BackColor = Color.White;
            return true;
        }

        /// <summary>
        /// Verifica unicidad global del código al perder el foco.
        /// Los códigos de barra son únicos a nivel global (índice UQ_CBar).
        /// </summary>
        private void TxtCodigo_Leave(object? sender, EventArgs e)
        {
            if (_cargando || !ValidarCodigo()) return;

            _codBarRepo ??= _unitOfWork.GetCustomRepository<ICodBarRepository>();
            var existsResult = _codBarRepo.ExistsByCodigo(txtCodigo.Text.Trim());
            if (!existsResult.IsSuccess || !existsResult.Value) return;

            if (_esNuevo)
            {
                txtCodigo.BackColor = Color.LightPink;
                return;
            }

            // Edición: conflicto solo si pertenece a otro código
            var codResult = _codBarRepo.GetByCodigo(txtCodigo.Text.Trim());
            if (codResult.IsSuccess &&
                codResult.Value != null &&
                _codBarEnEdicion != null &&
                codResult.Value.IdCodBar != _codBarEnEdicion.IdCodBar)
            {
                txtCodigo.BackColor = Color.LightPink;
            }
        }

        /// <summary>
        /// Validación completa antes de persistir.
        /// Usa repos síncronos y campos fuertemente tipados — sin dynamic ni await.
        /// </summary>
        private bool ValidarDatos()
        {
            bool esValido = true;
            var errores = new System.Text.StringBuilder();

            if (!ValidarCodigo())
            {
                errores.AppendLine("• El codigo es obligatorio y debe tener al menos 5 caracteres.");
                esValido = false;
            }
            else
            {
                _codBarRepo ??= _unitOfWork.GetCustomRepository<ICodBarRepository>();
                var existsResult = _codBarRepo.ExistsByCodigo(txtCodigo.Text.Trim());
                if (existsResult.IsSuccess && existsResult.Value)
                {
                    if (_esNuevo)
                    {
                        errores.AppendLine("• Ya existe un codigo de barra con este valor (debe ser unico global).");
                        txtCodigo.BackColor = Color.LightPink;
                        esValido = false;
                    }
                    else
                    {
                        var codResult = _codBarRepo.GetByCodigo(txtCodigo.Text.Trim());
                        if (codResult.IsSuccess &&
                            codResult.Value != null &&
                            _codBarEnEdicion != null &&
                            codResult.Value.IdCodBar != _codBarEnEdicion.IdCodBar)
                        {
                            errores.AppendLine("• Ya existe otro codigo de barra con este valor (debe ser unico global).");
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

            // Presentación obligatoria en modo standalone
            if (!EsModal && ObtenerIdPresProdSeleccionado() == 0)
            {
                errores.AppendLine("• Debe seleccionar una presentacion.");
                esValido = false;
            }

            // Solo un código puede ser Principal por presentación
            if (chkPrincipal.Checked)
            {
                long idPresProdActual = EsModal
                    ? IdPresProd
                    : ObtenerIdPresProdSeleccionado();

                if (idPresProdActual > 0)
                {
                    long idActual = _codBarEnEdicion?.IdCodBar ?? 0;
                    bool hayOtroPrincipal = _codigosLista.Any(c =>
                        c.Principal &&
                        c.IdPresProd == idPresProdActual &&
                        c.IdCodBar != idActual);

                    if (hayOtroPrincipal)
                    {
                        errores.AppendLine("• Ya existe otro codigo de barra marcado como principal para esta presentacion. Solo uno puede ser principal.");
                        esValido = false;
                    }
                }
            }

            if (!esValido)
                NotificationManager.Warning($"Por favor corrija los siguientes errores:\n\n{errores}");

            return esValido;
        }

        // ═════════════════════════════════════════════════════════════════════
        // OPERACIONES CRUD
        // ═════════════════════════════════════════════════════════════════════

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            LimpiarFormulario();
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (dgvCodigos.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un codigo de barra para editar.");
                return;
            }

            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos y editar el codigo seleccionado?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            // Entidad disponible directamente del DTO en el grid — sin GetById adicional
            var item = (CodBarItem)dgvCodigos.SelectedRows[0].DataBoundItem;
            CargarCodigoEnEdicion(item.IdCodBar);
            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvCodigos.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un codigo de barra para eliminar.");
                return;
            }

            var item = (CodBarItem)dgvCodigos.SelectedRows[0].DataBoundItem;
            if (MessageBox.Show($"¿Esta seguro de eliminar el codigo '{item.Codigo}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _codBarRepo ??= _unitOfWork.GetCustomRepository<ICodBarRepository>();

            // Obtener la entidad real para Remove (el DTO no es una entidad EF)
            var getResult = _codBarRepo.GetById(item.IdCodBar);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                NotificationManager.Error("No se pudo obtener el codigo de barra para eliminar.");
                return;
            }

            var removeResult = _codBarRepo.Remove(getResult.Value);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar: {removeResult.Error?.Message}");
                return;
            }

            try
            {
                _unitOfWork.SaveChanges();
                NotificationManager.Success("Codigo de barra eliminado correctamente.");
                RecargarSegunModo();
                LimpiarFormulario();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al guardar cambios: {ex.Message}");
            }
        }

        private void BtnRefrescar_Click(object sender, EventArgs e) => RecargarSegunModo();

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;

            _codBarRepo ??= _unitOfWork.GetCustomRepository<ICodBarRepository>();

            try
            {
                if (_esNuevo)
                {
                    long idPresProdDestino = EsModal
                        ? IdPresProd
                        : ObtenerIdPresProdSeleccionado();

                    var nuevo = new CodBar
                    {
                        IdPresProd = idPresProdDestino,
                        Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                        TipoCod = ObtenerTipoSeleccionado(),
                        Principal = chkPrincipal.Checked
                    };

                    var addResult = _codBarRepo.Add(nuevo);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al crear codigo de barra: {addResult.Error?.Message}");
                        return;
                    }
                }
                else
                {
                    // Usar entidad real desde BD — el DTO es inmutable y no es entidad EF
                    if (_codBarEnEdicion == null)
                    {
                        NotificationManager.Error("No hay codigo de barra en edicion.");
                        return;
                    }

                    var getResult = _codBarRepo.GetById(_codBarEnEdicion.IdCodBar);
                    if (!getResult.IsSuccess || getResult.Value == null)
                    {
                        NotificationManager.Error($"Error al obtener codigo para actualizar: {getResult.Error?.Message}");
                        return;
                    }

                    var entidad = getResult.Value;
                    entidad.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                    entidad.TipoCod = ObtenerTipoSeleccionado();
                    entidad.Principal = chkPrincipal.Checked;

                    // En modo standalone se puede cambiar la presentación
                    if (!EsModal)
                    {
                        long nuevaPres = ObtenerIdPresProdSeleccionado();
                        if (nuevaPres > 0)
                            entidad.IdPresProd = nuevaPres;
                    }

                    var updateResult = _codBarRepo.Update(entidad);
                    if (updateResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar codigo de barra: {updateResult.Error?.Message}");
                        return;
                    }
                }

                _unitOfWork.SaveChanges();
                NotificationManager.Success(_esNuevo
                    ? "Codigo de barra creado exitosamente."
                    : "Codigo de barra actualizado exitosamente.");

                RecargarSegunModo();
                LimpiarFormulario();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Ha ocurrido un error inesperado: {ex.Message}");
            }
        }

        /// <summary>
        /// BtnCancelar corregido:
        /// — Las dos ramas anteriores eran idénticas (código duplicado).
        /// — La rama "else / cerrar" nunca se alcanzaba si IdCodBar tenía valor.
        /// Ahora: si hay cambios, confirmar → limpiar. Sin cambios → limpiar directamente.
        /// </summary>
        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (HayCambios() &&
                MessageBox.Show("¿Esta seguro que desea cancelar? Los cambios no guardados se perderan.", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            LimpiarFormulario();
            ActualizarEstadoBotones();
            dgvCodigos.ClearSelection();
        }

        // ═════════════════════════════════════════════════════════════════════
        // SELECCIÓN EN GRILLA
        // ═════════════════════════════════════════════════════════════════════

        private void DgvCodigos_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarEstadoBotones();
        }

        private void DgvCodigos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvCodigos.Columns[e.ColumnIndex].Name != "colTipo" || e.Value == null) return;

            // El DTO almacena TipoCod como byte — convertir a texto legible
            byte tipo = Convert.ToByte(e.Value);
            e.Value = tipo switch
            {
                1 => "Normal",
                2 => "Pesable",
                3 => "Interno",
                4 => "Proveedor",
                _ => "Desconocido"
            };
            e.FormattingApplied = true;
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
                txtCodigo.BackColor = Color.White;
                cboTipoCodigo.SelectedIndex = 0;
                chkPrincipal.Checked = false;
                lblIdValor.Text = "<< Nuevo >>";

                _esNuevo = true;
                _codBarEnEdicion = null;

                GuardarSnapshot();
            }
            finally
            {
                _cargando = false;
            }
        }

        private void GuardarSnapshot()
        {
            _snapCodigo = txtCodigo.Text;
            _snapTipoCod = ObtenerTipoSeleccionado();
            _snapPrincipal = chkPrincipal.Checked;
            _snapIdPresProd = EsModal ? IdPresProd : ObtenerIdPresProdSeleccionado();
        }

        private bool HayCambios()
        {
            if (_esNuevo)
                return !string.IsNullOrWhiteSpace(txtCodigo.Text);

            return txtCodigo.Text != _snapCodigo ||
                   ObtenerTipoSeleccionado() != _snapTipoCod ||
                   chkPrincipal.Checked != _snapPrincipal ||
                   (!EsModal && ObtenerIdPresProdSeleccionado() != _snapIdPresProd);
        }

        private void ActualizarEstadoBotones()
        {
            bool haySeleccion = dgvCodigos.SelectedRows.Count > 0;
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
            btnGuardar.Enabled = true;
            btnCancelar.Enabled = true;
        }

        private void RecargarSegunModo()
        {
            var tenantResult = _tenantContext.TryGetCurrent();
            if (tenantResult.IsFailure) return;

            if (EsModal)
                CargarCodigosPorPresentacion(IdPresProd);
            else
                CargarCodigosPorTenant(tenantResult.Value.Id);
        }

        /// <summary>
        /// Lee el tipo seleccionado en cboTipoCodigo de forma fuertemente tipada.
        /// Encapsula el acceso tipado al item seleccionado en cboTipoCodigo.
        /// </summary>
        private byte ObtenerTipoSeleccionado()
        {
            if (cboTipoCodigo.SelectedItem is TipoCodItem item)
                return item.Valor;
            return 1; // Normal como fallback
        }

        /// <summary>
        /// Lee el IdPresProd seleccionado en cboPresentacion de forma fuertemente tipada.
        /// Encapsula el acceso tipado al item seleccionado en cboPresentacion.
        /// </summary>
        private long ObtenerIdPresProdSeleccionado()
        {
            if (cboPresentacion.SelectedItem is PresItem item)
                return item.IdPresProd;
            return 0;
        }
    }
}


