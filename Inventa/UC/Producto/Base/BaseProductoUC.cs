using System.ComponentModel;
using System.Windows.Forms;
using Inventa.Helpers;
using Inventa.UC.Producto.Interfaces;

namespace Inventa.UC.Producto.Base
{
    public class BaseProductoUC : UserControl, IRefreshable, IEditable, IValidatable
    {
        protected ErrorProvider ErrorProvider { get; private set; }
        protected ValidationDebouncer ValidationDebouncer { get; private set; }
        private IContainer _components;

        protected IProductoContainer? Container { get; private set; }
        private int _lastPreferredHeight;

        protected virtual ZonaContenedor Zona => ZonaContenedor.General;

        protected BaseProductoUC()
        {
            _components = new Container();
            ErrorProvider = new ErrorProvider(_components)
            {
                BlinkStyle = ErrorBlinkStyle.NeverBlink
            };

            ValidationDebouncer = new ValidationDebouncer();
        }

        public virtual void SetContainer(IProductoContainer container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            _lastPreferredHeight = GetPreferredSize(new Size(Width, 0)).Height;
        }

        // ── IRefreshable ────────────────────────────────────────────────────────
        public virtual bool HasChanges => false;
        public virtual void RefreshData() { }
        public virtual void Clear() { }
        public virtual void Limpiar() { }

        // ── IEditable ───────────────────────────────────────────────────────────
        public virtual bool IsEditing => false;

        public virtual void BeginEdit()
        {
            OnEditingChanged(true);
        }

        public virtual void EndEdit()
        {
            OnEditingChanged(false);
        }

        public virtual void CancelEdit()
        {
            OnEditingChanged(false);
        }

        protected virtual void OnEditingChanged(bool editing)
        {
            NotificarCambioTamaño();
        }

        protected void NotificarCambioTamaño()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            if (Container == null) return;

            var altura = GetPreferredSize(new Size(Width, 0)).Height;
            Container.NotificarCambioTamaño(Zona, altura, _lastPreferredHeight);
            _lastPreferredHeight = altura;
        }

        // ── IValidatable ────────────────────────────────────────────────────────
        public virtual bool IsValid => true;
        public virtual bool ValidarDatos(bool silent = false) => true;

        // ── Helpers ─────────────────────────────────────────────────────────────
        protected void SetError(Control control, string message)
        {
            ErrorProvider.SetError(control, message);
            control.BackColor = string.IsNullOrEmpty(message)
                ? SystemColors.Window
                : Color.FromArgb(255, 200, 200);
        }

        protected void ClearError(Control control)
        {
            ErrorProvider.SetError(control, "");
            control.BackColor = SystemColors.Window;
        }

        protected void ClearAllErrors()
        {
            ErrorProvider.Clear();
            foreach (Control control in EnumerateAllControls(this))
            {
                if (control is TextBox or ComboBox or NumericUpDown)
                    control.BackColor = SystemColors.Window;
            }
        }

        protected void SetAccessible(Control control, string name, string description, AccessibleRole role = AccessibleRole.Default)
        {
            control.AccessibleName = name;
            control.AccessibleDescription = description;
            control.AccessibleRole = role;
        }

        protected void MarkRequired(Label label)
        {
            if (!label.Text.EndsWith("*"))
                label.Text += " *";
            label.Font = new Font(label.Font, FontStyle.Bold);
        }

        private static IEnumerable<Control> EnumerateAllControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                yield return control;
                foreach (var child in EnumerateAllControls(control))
                    yield return child;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ValidationDebouncer?.Dispose();
                _components?.Dispose();
                ErrorProvider?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
