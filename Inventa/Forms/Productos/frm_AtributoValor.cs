using CBP.Data.Abstractions;
using CBP.MultiTenant.Abstractions;
using CBP.WinFrms.Notification;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.UI;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Inventa.Forms.Productos
{
    /// <summary>
    /// Mantenedor CRUD de valores de atributo (AtriVal) para un AtriDef dado.
    /// Se abre desde frm_AtributoDef y frm_ConfigAtributos vía _formFactory.ShowDialog.
    /// Permite crear, editar, reordenar y desactivar AtriVal.
    /// </summary>
    public partial class frm_AtributoValor : Form
    {
        // ── Dependencias ─────────────────────────────────────────────────────────
        private readonly IServiceScopeFactory _scopeFactory;

        // ── Scope y UnitOfWork propios ────────────────────────────────────────
        private IServiceScope? _formScope;
        private IUnitOfWorkSync? _unitOfWork;

        // ── Repositorios (lazy) ───────────────────────────────────────────────
        private IAtriValRepository? _atriValRepo;
        private IAtriDefRepository? _atriDefRepo;

        // ── Parámetro de entrada ──────────────────────────────────────────────
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long IdAtriDef { get; set; }

        // ── Estado interno ────────────────────────────────────────────────────
        private bool _esNuevo = true;
        private AtriVal? _valEnEdicion;   // entidad cacheada — evita GetById extra en Guardar
        private bool _cargando;
        private bool _huboCambios;

        // Snapshots para detección de cambios
        private string? _snapCodigo;
        private string? _snapNombre;
        private int _snapOrden;
        private bool _snapActivo;

        // ── DTO aplanado para la grilla ───────────────────────────────────────
        private sealed record ValItem(
            long IdAtriVal,
            string Codigo,
            string Nombre,
            int Orden,
            bool Activo,
            bool UsadoPorVariantes   // true si alguna VarAtri apunta a este valor
        );

        // ═════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═════════════════════════════════════════════════════════════════════

        public frm_AtributoValor(
            IServiceScopeFactory scopeFactory)
        {
            InitializeComponent();
            _scopeFactory = scopeFactory;

            this.KeyPreview = true;
            this.KeyDown += Frm_AtributoValor_KeyDown;

            ConfigurarGrid();
            ConfigurarEventos();

            this.FormClosing += (s, e) => _formScope?.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CONFIGURACIÓN
        // ═════════════════════════════════════════════════════════════════════

        private void ConfigurarGrid()
        {
            dgvAtriVal.AutoGenerateColumns = false;
            dgvAtriVal.Columns.Clear();

            dgvAtriVal.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IdAtriVal",
                DataPropertyName = nameof(ValItem.IdAtriVal),
                Visible = false
            });
            dgvAtriVal.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Orden",
                HeaderText = "#",
                DataPropertyName = nameof(ValItem.Orden),
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });
            dgvAtriVal.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Codigo",
                HeaderText = "Codigo",
                DataPropertyName = nameof(ValItem.Codigo),
                Width = 100
            });
            dgvAtriVal.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nombre",
                HeaderText = "Nombre / Valor",
                DataPropertyName = nameof(ValItem.Nombre),
                Width = 250
            });
            dgvAtriVal.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Activo",
                HeaderText = "Activo",
                DataPropertyName = nameof(ValItem.Activo),
                Width = 58,
                ReadOnly = true
            });
            dgvAtriVal.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "UsadoPorVariantes",
                HeaderText = "En uso",
                DataPropertyName = nameof(ValItem.UsadoPorVariantes),
                Width = 58,
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ToolTipText = "Indica si alguna variante tiene asignado este valor"
            });

            dgvAtriVal.RowPrePaint += DgvAtriVal_RowPrePaint;
            dgvAtriVal.SelectionChanged += DgvAtriVal_SelectionChanged;
            dgvAtriVal.CellDoubleClick += DgvAtriVal_CellDoubleClick;
        }

        private void ConfigurarEventos()
        {
            txtCodigo.TextChanged += (s, e) => { if (!_cargando) ValidarCodigo(); };
            txtNombre.TextChanged += (s, e) => { if (!_cargando) ValidarNombre(); };
            txtCodigo.Leave += TxtCodigo_Leave;
        }

        // ═════════════════════════════════════════════════════════════════════
        // CARGA INICIAL
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_AtributoValor_Load(object sender, EventArgs e)
        {
            if (IdAtriDef <= 0)
            {
                NotificationManager.Error("No se especifico un atributo valido.");
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            try
            {
                _formScope = _scopeFactory.CreateScope();
                _unitOfWork = _formScope.ServiceProvider
                    .GetRequiredService<IUnitOfWorkSync>();

                CargarTitulo();
                CargarAtriVal();
                LimpiarFormulario();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error al inicializar: {ex.Message}");
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // ATAJOS DE TECLADO
        // ═════════════════════════════════════════════════════════════════════

        private void Frm_AtributoValor_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                BtnGuardar_Click(sender!, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                CargarAtriVal();
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

        private void CargarTitulo()
        {
            _atriDefRepo ??= _unitOfWork!.GetCustomRepository<IAtriDefRepository>();
            var result = _atriDefRepo.GetById(IdAtriDef);
            string nombre = result.IsSuccess && result.Value != null
                ? result.Value.Nombre
                : $"Atributo #{IdAtriDef}";

            lblTitulo.Text = $"Valores de  {nombre}";
            this.Text = $"Valores de Atributo  —  {nombre}";
        }

        private void CargarAtriVal()
        {
            _atriValRepo ??= _unitOfWork!.GetCustomRepository<IAtriValRepository>();

            // Mostrar todos (activos e inactivos) para poder reactivar
            var result = _atriValRepo.GetByAtriDef(IdAtriDef, soloActivos: false);
            if (!result.IsSuccess)
            {
                NotificationManager.Warning($"Error al cargar valores: {result.Error?.Message}");
                return;
            }

            var items = result.Value.Select(v =>
            {
                var usado = _atriValRepo.IsUsedByVariantes(v.IdAtriVal);
                bool enUso = usado.IsSuccess && usado.Value;
                return new ValItem(v.IdAtriVal, v.Codigo, v.Nombre, v.Orden, v.Activo, enUso);
            }).ToList();

            dgvAtriVal.DataSource = null;
            dgvAtriVal.DataSource = items;

            ActualizarContador(items.Count);
        }

        private void ActualizarContador(int total)
        {
            int activos = dgvAtriVal.Rows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as ValItem)
                .Count(v => v?.Activo == true);
            tsLblContador.Text = $"{activos} activos / {total} total";
        }

        // ═════════════════════════════════════════════════════════════════════
        // COLOREADO DE FILAS
        // ═════════════════════════════════════════════════════════════════════

        private void DgvAtriVal_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvAtriVal.Rows.Count) return;
            var item = dgvAtriVal.Rows[e.RowIndex].DataBoundItem as ValItem;
            if (item == null) return;

            var row = dgvAtriVal.Rows[e.RowIndex];
            if (!item.Activo)
            {
                row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
                row.DefaultCellStyle.ForeColor = AppColors.TextDisabled;
                row.DefaultCellStyle.SelectionBackColor = AppColors.BorderLight;
                row.DefaultCellStyle.SelectionForeColor = AppColors.TextSecondary;
            }
            else if (item.UsadoPorVariantes)
            {
                // En uso — fondo verde muy tenue para indicar que está asignado
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

        private void DgvAtriVal_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarEstadoBotones();
        }

        private void DgvAtriVal_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                IniciarEdicion();
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
            txtNombre.BackColor = System.Drawing.Color.White;
            return true;
        }

        private void TxtCodigo_Leave(object? sender, EventArgs e)
        {
            if (_cargando || !ValidarCodigo()) return;

            _atriValRepo ??= _unitOfWork!.GetCustomRepository<IAtriValRepository>();
            var existe = _atriValRepo.ExistsByCodigo(IdAtriDef, txtCodigo.Text.Trim());
            if (!existe.IsSuccess || !existe.Value) return;

            if (_esNuevo)
            {
                txtCodigo.BackColor = System.Drawing.Color.LightPink;
                return;
            }

            var otro = _atriValRepo.GetByCodigo(IdAtriDef, txtCodigo.Text.Trim());
            if (otro.IsSuccess && otro.Value != null &&
                _valEnEdicion != null &&
                otro.Value.IdAtriVal != _valEnEdicion.IdAtriVal)
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
                _atriValRepo ??= _unitOfWork!.GetCustomRepository<IAtriValRepository>();
                var existe = _atriValRepo.ExistsByCodigo(IdAtriDef, txtCodigo.Text.Trim());
                if (existe.IsSuccess && existe.Value)
                {
                    if (_esNuevo)
                    {
                        errores.AppendLine("• Ya existe un valor con ese codigo en este atributo.");
                        txtCodigo.BackColor = System.Drawing.Color.LightPink;
                        esValido = false;
                    }
                    else
                    {
                        var otro = _atriValRepo.GetByCodigo(IdAtriDef, txtCodigo.Text.Trim());
                        if (otro.IsSuccess && otro.Value != null &&
                            _valEnEdicion != null &&
                            otro.Value.IdAtriVal != _valEnEdicion.IdAtriVal)
                        {
                            errores.AppendLine("• Ya existe otro valor con ese codigo en este atributo.");
                            txtCodigo.BackColor = System.Drawing.Color.LightPink;
                            esValido = false;
                        }
                    }
                }
            }

            if (!ValidarNombre())
            {
                errores.AppendLine("• El nombre es obligatorio.");
                esValido = false;
            }

            if ((int)numOrden.Value < 0)
            {
                errores.AppendLine("• El orden debe ser mayor o igual a cero.");
                esValido = false;
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
            if (dgvAtriVal.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un valor para editar.");
                return;
            }

            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            IniciarEdicion();
        }

        private void IniciarEdicion()
        {
            if (dgvAtriVal.SelectedRows.Count == 0) return;

            var item = dgvAtriVal.SelectedRows[0].DataBoundItem as ValItem;
            if (item == null) return;

            _atriValRepo ??= _unitOfWork!.GetCustomRepository<IAtriValRepository>();
            var result = _atriValRepo.GetById(item.IdAtriVal);
            if (!result.IsSuccess || result.Value == null)
            {
                NotificationManager.Error("No se pudo cargar el valor para edicion.");
                return;
            }

            _cargando = true;
            try
            {
                _valEnEdicion = result.Value;
                _esNuevo = false;

                txtCodigo.Text = _valEnEdicion.Codigo;
                txtNombre.Text = _valEnEdicion.Nombre;
                numOrden.Value = _valEnEdicion.Orden;
                chkActivo.Checked = _valEnEdicion.Activo;

                txtCodigo.BackColor = System.Drawing.Color.White;
                txtNombre.BackColor = System.Drawing.Color.White;

                GuardarSnapshot();
            }
            finally
            {
                _cargando = false;
            }

            ActualizarEstadoBotones();
            txtCodigo.Focus();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;

            _atriValRepo ??= _unitOfWork!.GetCustomRepository<IAtriValRepository>();

            try
            {
                if (_esNuevo)
                {
                    var nuevo = new AtriVal
                    {
                        IdAtriDef = IdAtriDef,
                        Codigo = txtCodigo.Text.Trim().ToUpperInvariant(),
                        Nombre = txtNombre.Text.Trim(),
                        Orden = (byte)Math.Min(numOrden.Value, 255),
                        Activo = chkActivo.Checked
                    };

                    var addResult = _atriValRepo.Add(nuevo);
                    if (addResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al crear valor: {addResult.Error?.Message}");
                        return;
                    }
                }
                else
                {
                    if (_valEnEdicion == null)
                    {
                        NotificationManager.Error("No hay valor en edicion.");
                        return;
                    }

                    _valEnEdicion.Codigo = txtCodigo.Text.Trim().ToUpperInvariant();
                    _valEnEdicion.Nombre = txtNombre.Text.Trim();
                    _valEnEdicion.Orden = (byte)Math.Min(numOrden.Value, 255);
                    _valEnEdicion.Activo = chkActivo.Checked;

                    var updResult = _atriValRepo.Update(_valEnEdicion);
                    if (updResult.IsFailure)
                    {
                        NotificationManager.Error($"Error al actualizar valor: {updResult.Error?.Message}");
                        return;
                    }
                }

                _unitOfWork!.SaveChanges();
                NotificationManager.Success(_esNuevo
                    ? "Valor creado exitosamente."
                    : "Valor actualizado exitosamente.");

                _huboCambios = true;
                CargarAtriVal();
                LimpiarFormulario();
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                NotificationManager.Error($"Error inesperado: {ex.Message}");
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvAtriVal.SelectedRows.Count == 0)
            {
                NotificationManager.Warning("Debe seleccionar un valor para eliminar.");
                return;
            }

            var item = dgvAtriVal.SelectedRows[0].DataBoundItem as ValItem;
            if (item == null) return;

            _atriValRepo ??= _unitOfWork!.GetCustomRepository<IAtriValRepository>();

            // Verificar si el valor está asignado a alguna variante
            var usadoResult = _atriValRepo.IsUsedByVariantes(item.IdAtriVal);
            if (usadoResult.IsSuccess && usadoResult.Value)
            {
                if (MessageBox.Show(
                    $"El valor '{item.Nombre}' esta asignado a una o mas variantes.\n" +
                    "Eliminarlo quitara automaticamente esa asignacion en esas variantes.\n\n" +
                    "¿Desea continuar?\n" +
                    "(Alternativa recomendada: desactivarlo con Activo = false)", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }
            else
            {
                if (MessageBox.Show($"¿Desea eliminar el valor '{item.Nombre}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            var getResult = _atriValRepo.GetById(item.IdAtriVal);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                NotificationManager.Error("No se pudo obtener el valor para eliminar.");
                return;
            }

            var removeResult = _atriValRepo.Remove(getResult.Value);
            if (removeResult.IsFailure)
            {
                NotificationManager.Error($"Error al eliminar: {removeResult.Error?.Message}");
                return;
            }

            try
            {
                _unitOfWork!.SaveChanges();
                NotificationManager.Success("Valor eliminado.");
                _huboCambios = true;
                CargarAtriVal();
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
            CargarAtriVal();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (HayCambios() &&
                MessageBox.Show("Hay cambios sin guardar. ¿Desea descartarlos?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            LimpiarFormulario();
            dgvAtriVal.ClearSelection();
            ActualizarEstadoBotones();
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            DialogResult = _huboCambios ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        // ═════════════════════════════════════════════════════════════════════
        // HELPERS DE ESTADO
        // ═════════════════════════════════════════════════════════════════════

        private void LimpiarFormulario()
        {
            _cargando = true;
            try
            {
                txtCodigo.Clear();
                txtNombre.Clear();
                numOrden.Value = SiguienteOrden();
                chkActivo.Checked = true;
                txtCodigo.BackColor = System.Drawing.Color.White;
                txtNombre.BackColor = System.Drawing.Color.White;
                _esNuevo = true;
                _valEnEdicion = null;
                GuardarSnapshot();
            }
            finally
            {
                _cargando = false;
            }
        }

        /// <summary>
        /// Propone el siguiente valor de Orden como max(Orden)+1 de los valores actuales.
        /// Facilita al operador mantener el orden correlativo sin escribirlo manualmente.
        /// </summary>
        private decimal SiguienteOrden()
        {
            if (dgvAtriVal.Rows.Count == 0) return 0;
            int maxOrden = dgvAtriVal.Rows
                .Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as ValItem)
                .Where(v => v != null)
                .Max(v => v!.Orden);
            return maxOrden + 1;
        }

        private void GuardarSnapshot()
        {
            _snapCodigo = txtCodigo.Text;
            _snapNombre = txtNombre.Text;
            _snapOrden = (int)numOrden.Value;
            _snapActivo = chkActivo.Checked;
        }

        private bool HayCambios()
        {
            if (_esNuevo)
                return !string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                       !string.IsNullOrWhiteSpace(txtNombre.Text);

            return txtCodigo.Text != _snapCodigo ||
                   txtNombre.Text != _snapNombre ||
                   (int)numOrden.Value != _snapOrden ||
                   chkActivo.Checked != _snapActivo;
        }

        private void ActualizarEstadoBotones()
        {
            bool haySeleccion = dgvAtriVal.SelectedRows.Count > 0;
            btnEditar.Enabled = haySeleccion;
            btnEliminar.Enabled = haySeleccion;
        }
    }
}


