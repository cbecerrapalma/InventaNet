using CBP.Data.Abstractions;
using CBP.MultiTenant.Abstractions;
using CBP.MultiTenant.Extensions;
using CBP.WinFrms.Notification;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.Infrastructure;
using Inventa.UI;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Inventa.Forms.Productos
{
    public partial class frm_ConfigAtributos : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly ITenantContext _tenantContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFormFactory _formFactory;

        // ── Scope y UnitOfWork propios ────────────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorios (lazy) ───────────────────────────────────────────────
        // ICategoriaRepository eliminado — era código muerto; IdCategoria
        // se obtiene directamente de Prod.IdCategoria sin consulta adicional.
        private IVarProdRepository? _varProdRepo;
        private IProdRepository? _prodRepo;
        private IAtriDefRepository? _atriDefRepo;
        private IAtriValRepository? _atriValRepo;
        private IVarAtriRepository? _varAtriRepo;

        // ── Parámetro de entrada ──────────────────────────────────────────────
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long IdVarProd { get; set; }

        // ── Estado interno ────────────────────────────────────────────────────
        private VarProd? _variante;
        private Prod? _producto;
        private long _idCategoria;
        private bool _isLoading; // FIX: campo faltante — el código original no compilaba
        private List<AtributoItem> _atributos = new();

        // ── DTO aplanado para la grilla ───────────────────────────────────────
        private sealed record AtributoItem(
            long IdAtriDef,
            string Codigo,
            string Nombre,
            bool EsOblig,
            bool Cumplido,           // true si tiene ≥1 valor asignado (o no es obligatorio)
            string ValoresAsignados,
            List<long> IdsValoresAsignados
        );

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_ConfigAtributos(
            ITenantContext tenantContext,
            IServiceScopeFactory scopeFactory,
            IFormFactory formFactory)
        {
            InitializeComponent();
            _tenantContext = tenantContext;
            _scopeFactory = scopeFactory;
            _formFactory = formFactory;

            this.KeyPreview = true;
            this.KeyDown += Frm_ConfigAtributos_KeyDown;

            ConfigurarGrid();
            this.FormClosing += (s, e) => _formScope?.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN DE COLUMNAS
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarGrid()
        {
            dgvAtributos.AutoGenerateColumns = false;
            dgvAtributos.Columns.Clear();

            dgvAtributos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IdAtriDef",
                HeaderText = "ID",
                DataPropertyName = nameof(AtributoItem.IdAtriDef),
                Visible = false
            });

            dgvAtributos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Codigo",
                HeaderText = "Codigo",
                DataPropertyName = nameof(AtributoItem.Codigo),
                Width = 90
            });

            dgvAtributos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nombre",
                HeaderText = "Atributo",
                DataPropertyName = nameof(AtributoItem.Nombre),
                Width = 200
            });

            dgvAtributos.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "EsOblig",
                HeaderText = "Oblig.",
                DataPropertyName = nameof(AtributoItem.EsOblig),
                Width = 58,
                ReadOnly = true
            });

            // Columna visual: atributo obligatorio cumplido o no
            dgvAtributos.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Cumplido",
                HeaderText = "OK",
                DataPropertyName = nameof(AtributoItem.Cumplido),
                Width = 42,
                ReadOnly = true,
                ToolTipText = "Indica si el atributo obligatorio tiene al menos un valor asignado"
            });

            dgvAtributos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ValoresAsignados",
                HeaderText = "Valores asignados",
                DataPropertyName = nameof(AtributoItem.ValoresAsignados),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvAtributos.RowPrePaint += DgvAtributos_RowPrePaint;
            dgvAtributos.CellDoubleClick += DgvAtributos_CellDoubleClick;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_ConfigAtributos_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                var tenantResult = _tenantContext.TryGetCurrent();
                if (tenantResult.IsFailure)
                {
                    NotificationManager.Error("No hay empresa activa.");
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                if (IdVarProd <= 0)
                {
                    NotificationManager.Error("No se especifico una variante valida.");
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                _formScope = _scopeFactory.CreateScope();
                _unitOfWork = _formScope.ServiceProvider
                    .GetRequiredService<IUnitOfWorkSync>();

                if (!CargarDatosVariante())
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                CargarAtributos();
                ActualizarTitulo();
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
        // ATAJOS DE TECLADO
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_ConfigAtributos_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                BtnRefrescar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                BtnCerrar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Enter && !_isLoading && dgvAtributos.SelectedRows.Count > 0)
            {
                e.SuppressKeyPress = true;
                BtnEditarValores_Click(sender!, EventArgs.Empty);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA DE DATOS
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Carga variante y producto. IdCategoria viene de Prod.IdCategoria
        /// directamente — sin ICategoriaRepository (campo eliminado).
        /// </summary>
        private bool CargarDatosVariante()
        {
            _varProdRepo ??= _unitOfWork!.GetCustomRepository<IVarProdRepository>();
            var varResult = _varProdRepo.GetById(IdVarProd);
            if (!varResult.IsSuccess || varResult.Value == null)
            {
                NotificationManager.Error("No se pudo cargar la variante.");
                return false;
            }
            _variante = varResult.Value;

            _prodRepo ??= _unitOfWork!.GetCustomRepository<IProdRepository>();
            var prodResult = _prodRepo.GetById(_variante.IdProd);
            if (!prodResult.IsSuccess || prodResult.Value == null)
            {
                NotificationManager.Error("No se pudo cargar el producto asociado.");
                return false;
            }
            _producto = prodResult.Value;
            _idCategoria = _producto.IdCategoria;
            return true;
        }

        /// <summary>
        /// Carga todos los AtriDef activos de la categoría y cruza con las
        /// asignaciones de la variante.
        ///
        /// FIX: usa GetByVarianteConDetalle (incluye AtriVal → AtriDef).
        /// GetByVariante NO incluye navegación — va.AtriVal siempre null
        /// en el código original, por lo que el filtro nunca funcionaba.
        /// </summary>
        private void CargarAtributos()
        {
            _atriDefRepo ??= _unitOfWork!.GetCustomRepository<IAtriDefRepository>();
            var defResult = _atriDefRepo.GetByCategoria(_idCategoria, soloActivos: true);
            if (!defResult.IsSuccess)
            {
                NotificationManager.Warning($"Error al cargar atributos: {defResult.Error?.Message}");
                return;
            }

            _varAtriRepo ??= _unitOfWork!.GetCustomRepository<IVarAtriRepository>();
            var varAtriResult = _varAtriRepo.GetByVarianteConDetalle(IdVarProd);
            if (!varAtriResult.IsSuccess)
            {
                NotificationManager.Warning($"Error al cargar asignaciones: {varAtriResult.Error?.Message}");
                return;
            }

            var asignaciones = varAtriResult.Value;

            _atributos = defResult.Value.Select(def =>
            {
                var valsAsignados = asignaciones
                    .Where(va => va.AtriVal?.IdAtriDef == def.IdAtriDef)
                    .Select(va => va.AtriVal!)
                    .ToList();

                var texto = string.Join(", ", valsAsignados.Select(v => v.Nombre));
                var ids = valsAsignados.Select(v => v.IdAtriVal).ToList();
                bool cumplido = !def.EsOblig || ids.Count > 0;

                return new AtributoItem(
                    IdAtriDef: def.IdAtriDef,
                    Codigo: def.Codigo,
                    Nombre: def.Nombre,
                    EsOblig: def.EsOblig,
                    Cumplido: cumplido,
                    ValoresAsignados: texto,
                    IdsValoresAsignados: ids
                );
            }).ToList();

            dgvAtributos.DataSource = null;
            dgvAtributos.DataSource = _atributos;

            ActualizarBarraEstado();
        }

        // ═════════════════════════════════════════════════════════════════════
        // COLOREADO DE FILAS
        // ═════════════════════════════════════════════════════════════════════

        private void DgvAtributos_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvAtributos.Rows.Count) return;
            var item = dgvAtributos.Rows[e.RowIndex].DataBoundItem as AtributoItem;
            if (item == null) return;

            var row = dgvAtributos.Rows[e.RowIndex];

            if (item.EsOblig && !item.Cumplido)
            {
                row.DefaultCellStyle.BackColor = AppColors.WarningLight;
                row.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
                row.DefaultCellStyle.SelectionBackColor = AppColors.Warning;
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            else if (item.EsOblig && item.Cumplido)
            {
                row.DefaultCellStyle.BackColor = AppColors.SuccessLight;
                row.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
                row.DefaultCellStyle.SelectionBackColor = AppColors.Success;
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = AppColors.TextSecondary;
                row.DefaultCellStyle.SelectionBackColor = AppColors.PrimaryLight;
                row.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // ESTADO DE INTERFAZ
        // ═════════════════════════════════════════════════════════════════════

        private void ActualizarTitulo()
        {
            if (_variante != null && _producto != null)
                lblTitulo.Text = $"{_producto.Nombre}  —  Variante: {_variante.Nombre}";
        }

        private void ActualizarEstadoBotones()
        {
            bool haySeleccion = dgvAtributos.SelectedRows.Count > 0;
            btnEditarValores.Enabled = haySeleccion;
            btnGestionarValores.Enabled = haySeleccion;
        }

        /// <summary>
        /// Actualiza la barra de estado con el resumen de atributos obligatorios
        /// cumplidos y pendientes.
        /// </summary>
        private void ActualizarBarraEstado()
        {
            int total = _atributos.Count;
            int obligNoCumplidos = _atributos.Count(a => a.EsOblig && !a.Cumplido);
            int conValores = _atributos.Count(a => a.IdsValoresAsignados.Count > 0);

            if (obligNoCumplidos > 0)
            {
                lblEstado.Text = $"  {obligNoCumplidos} atributo(s) obligatorio(s) sin valor   |   " +
                                      $"{conValores} de {total} con valores asignados";
                lblEstado.ForeColor = AppColors.Warning;
            }
            else
            {
                lblEstado.Text = $"  Todos los obligatorios cubiertos   |   " +
                                      $"{conValores} de {total} con valores asignados";
                lblEstado.ForeColor = AppColors.Success;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // EVENTOS DE GRILLA
        // ═════════════════════════════════════════════════════════════════════

        private void DgvAtributos_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarEstadoBotones();
        }

        private void DgvAtributos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                BtnEditarValores_Click(sender, EventArgs.Empty);
        }

        // ═════════════════════════════════════════════════════════════════════
        // EDITAR VALORES DEL ATRIBUTO SELECCIONADO
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Abre un diálogo inline de selección múltiple de valores para el atributo.
        /// Reemplaza frm_SeleccionarValoresAtributo (formulario inexistente en el proyecto)
        /// con un diálogo construido en código que usa _formFactory para los mantenedores
        /// externos, consistente con el resto del proyecto.
        /// </summary>
        private void BtnEditarValores_Click(object sender, EventArgs e)
        {
            if (_isLoading || dgvAtributos.SelectedRows.Count == 0) return;

            var item = dgvAtributos.SelectedRows[0].DataBoundItem as AtributoItem;
            if (item == null) return;

            _atriValRepo ??= _unitOfWork!.GetCustomRepository<IAtriValRepository>();
            var valsResult = _atriValRepo.GetByAtriDef(item.IdAtriDef, soloActivos: true);
            if (!valsResult.IsSuccess || valsResult.Value.Count == 0)
            {
                NotificationManager.Warning(
                    $"El atributo '{item.Nombre}' no tiene valores activos definidos.\n" +
                    "Use 'Gestionar valores' para crear valores primero.");
                return;
            }

            var nuevosIds = MostrarSelectorValores(
                item.Nombre,
                valsResult.Value,
                new HashSet<long>(item.IdsValoresAsignados));

            if (nuevosIds != null)
                GuardarAsignaciones(item.IdAtriDef, nuevosIds);
        }

        /// <summary>
        /// Diálogo modal de checkboxes para selección múltiple de AtriVal.
        /// Devuelve null si el usuario cancela.
        /// </summary>
        private List<long>? MostrarSelectorValores(
            string nombreAtributo,
            IReadOnlyList<AtriVal> disponibles,
            HashSet<long> seleccionados)
        {
            int alturaPanel = Math.Min(400, disponibles.Count * 28 + 20);

            using var dlg = new Form
            {
                Text = $"Valores para: {nombreAtributo}",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Width = 380,
                Height = alturaPanel + 110,
                Font = this.Font,
                BackColor = Color.White
            };

            var panelChecks = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                Padding = new Padding(12, 8, 12, 4),
                WrapContents = false
            };

            var checks = new List<CheckBox>();
            foreach (var val in disponibles)
            {
                var chk = new CheckBox
                {
                    Text = val.Nombre,
                    Tag = val.IdAtriVal,
                    Checked = seleccionados.Contains(val.IdAtriVal),
                    Width = 320,
                    Height = 24,
                    Font = this.Font
                };
                checks.Add(chk);
                panelChecks.Controls.Add(chk);
            }

            var panelBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 52,
                BackColor = Color.FromArgb(243, 242, 241),
                Padding = new Padding(8, 8, 8, 8),
                WrapContents = false
            };

            var btnOk = new Button
            {
                Text = "Aceptar",
                DialogResult = DialogResult.OK,
                Size = new Size(100, 34),
                BackColor = Color.FromArgb(30, 60, 114),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(this.Font, FontStyle.Bold),
                Margin = new Padding(6, 0, 0, 0)
            };
            btnOk.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Size = new Size(100, 34),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderColor = Color.FromArgb(138, 136, 134) },
                Margin = new Padding(6, 0, 0, 0)
            };

            panelBtns.Controls.Add(btnOk);
            panelBtns.Controls.Add(btnCancel);
            dlg.Controls.Add(panelChecks);
            dlg.Controls.Add(panelBtns);
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            return dlg.ShowDialog(this) == DialogResult.OK
                ? checks.Where(c => c.Checked).Select(c => (long)c.Tag!).ToList()
                : null;
        }

        // ═════════════════════════════════════════════════════════════════════
        // PERSISTENCIA DE ASIGNACIONES (VarAtri)
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Sincroniza las filas VarAtri para un AtriDef concreto.
        ///
        /// FIX CRÍTICO: el código original usaba GetByVariante() sin navegación
        /// y filtraba va.AtriVal?.IdAtriDef — siempre null → nunca eliminaba nada.
        /// Ahora usa GetByVarianteConDetalle() para leer el estado actual y
        /// VarAtriRepo.Get(IdVarProd, idAtriVal) para recuperar la entidad antes
        /// de pasarla a Remove(), que requiere una entidad con PK válida.
        /// </summary>
        private void GuardarAsignaciones(long idAtriDef, List<long> nuevosIds)
        {
            try
            {
                _varAtriRepo ??= _unitOfWork!.GetCustomRepository<IVarAtriRepository>();

                var varAtriResult = _varAtriRepo.GetByVarianteConDetalle(IdVarProd);
                if (!varAtriResult.IsSuccess)
                {
                    NotificationManager.Error($"Error al leer asignaciones: {varAtriResult.Error?.Message}");
                    return;
                }

                // Solo las asignaciones del AtriDef en cuestión
                var actualesDef = varAtriResult.Value
                    .Where(va => va.AtriVal?.IdAtriDef == idAtriDef)
                    .ToList();

                var idsActuales = actualesDef.Select(va => va.IdAtriVal).ToList();
                var idsEliminar = idsActuales.Except(nuevosIds).ToList();
                var idsAgregar = nuevosIds.Except(idsActuales).ToList();

                // Eliminar: Get(idVarProd, idAtriVal) recupera entidad con PK
                // correcta para que Remove() funcione con EF Core.
                foreach (var idEliminar in idsEliminar)
                {
                    var getResult = _varAtriRepo.Get(IdVarProd, idEliminar);
                    if (getResult.IsSuccess && getResult.Value != null)
                    {
                        var removeResult = _varAtriRepo.Remove(getResult.Value);
                        if (removeResult.IsFailure)
                            NotificationManager.Warning($"Error al quitar valor {idEliminar}: {removeResult.Error?.Message}");
                    }
                }

                // Agregar nuevas asignaciones
                foreach (var idAgregar in idsAgregar)
                {
                    // Defensivo: evitar duplicados
                    var existsResult = _varAtriRepo.Exists(IdVarProd, idAgregar);
                    if (existsResult.IsSuccess && existsResult.Value) continue;

                    var addResult = _varAtriRepo.Add(new VarAtri
                    {
                        IdVarProd = IdVarProd,
                        IdAtriVal = idAgregar
                    });
                    if (addResult.IsFailure)
                        NotificationManager.Warning($"Error al agregar valor {idAgregar}: {addResult.Error?.Message}");
                }

                _unitOfWork!.SaveChanges();

                if (idsEliminar.Count > 0 || idsAgregar.Count > 0)
                    NotificationManager.Success($"Asignaciones actualizadas: +{idsAgregar.Count} / -{idsEliminar.Count}.");

                CargarAtributos();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al guardar asignaciones: {ex.Message}");
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // MANTENEDORES EXTERNOS
        // ═════════════════════════════════════════════════════════════════════

        private void BtnGestionarDefiniciones_Click(object sender, EventArgs e)
        {
            var result = _formFactory.ShowDialog<frm_AtributoDef>(form =>
            {
                form.IdCategoria = _idCategoria;
            });

            if (result == DialogResult.OK)
                CargarAtributos();
        }

        private void BtnGestionarValores_Click(object sender, EventArgs e)
        {
            if (dgvAtributos.SelectedRows.Count == 0) return;

            var item = dgvAtributos.SelectedRows[0].DataBoundItem as AtributoItem;
            if (item == null) return;

            var result = _formFactory.ShowDialog<frm_AtributoValor>(form =>
            {
                form.IdAtriDef = item.IdAtriDef;
            });

            if (result == DialogResult.OK)
                CargarAtributos();
        }

        // ═════════════════════════════════════════════════════════════════════
        // BOTONES GENERALES
        // ═════════════════════════════════════════════════════════════════════

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            if (_isLoading) return;
            CargarAtributos();
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            int sinCubrir = _atributos.Count(a => a.EsOblig && !a.Cumplido);
            if (sinCubrir > 0 &&
                MessageBox.Show(
                    $"Hay {sinCubrir} atributo(s) obligatorio(s) sin valor asignado.\n" +
                    "¿Desea cerrar de todas formas?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}


