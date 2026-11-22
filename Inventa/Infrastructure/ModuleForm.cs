using System.ComponentModel;
using System.Threading;
using CBP.MultiTenant.Abstractions;
using CBP.WinFrms.Base;
using CBP.WinFrms.Theming;
using Microsoft.Extensions.DependencyInjection;

namespace Inventa.Infrastructure;

public class ModuleForm : BaseForm, IModuleForm
{
    private bool _initialized;
    private IServiceScope? _scope;
    private readonly ITenantContext? _tenantContext;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual string ModuleKey => GetType().Name;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int IdTenant { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool HasChanges { get; protected set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsNew { get; protected set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IFormFactory? FormFactory { get; private set; }

    protected IServiceProvider Services =>
        _scope?.ServiceProvider
        ?? throw new InvalidOperationException(
            $"{ModuleKey}: service scope not initialized. Call CreateScope first.");

    // Designer-safe parameterless constructor
    protected ModuleForm()
    {
    }

    // Runtime constructor with DI
    protected ModuleForm(
        ITenantContext tenantContext,
        IFormFactory formFactory)
    {
        _tenantContext = tenantContext;
        FormFactory = formFactory;

        if (!DesignModeHelper.IsDesignTime())
        {
            if (_tenantContext.Current is null)
                throw new InvalidOperationException(
                    $"{ModuleKey}: no tenant initialized. Call InitializeTenantAsync in Program.cs.");

            Initialize(_tenantContext.Current.Id);
        }
    }

    public void Initialize(int tenantId)
    {
        if (_initialized)
            return;

        _initialized = true;
        IdTenant = tenantId;
        OnInitialized();
    }

    protected virtual void OnInitialized()
    {
    }

    protected void CreateScope(IServiceScopeFactory factory)
    {
        if (_scope is not null)
            return;

        _scope = factory.CreateScope();
    }

    protected virtual void DisposeScope()
    {
        var scope = Interlocked.Exchange(ref _scope, null);
        scope?.Dispose();
    }

    public virtual bool CanClose()
    {
        return !HasChanges;
    }

    protected virtual bool DetectChanges()
    {
        return HasChanges;
    }

    protected void RefreshDirtyState()
    {
        HasChanges = DetectChanges();
    }

    protected void MarkDirty()
    {
        HasChanges = true;
    }

    protected void ClearDirty()
    {
        HasChanges = false;
    }

    protected DialogResult ShowDialog<T>() where T : Form
    {
        return FormFactory!.ShowDialog<T>();
    }

    protected DialogResult ShowDialog<T>(Action<T>? configure) where T : Form
    {
        return FormFactory!.ShowDialog(configure);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!DesignModeHelper.IsDesignTime() && !e.Cancel)
        {
            RefreshDirtyState();

            if (!CanClose())
            {
                var result = MessageBox.Show(
                    "Hay cambios sin guardar. ¿Cerrar de todas formas?",
                    "Cambios sin guardar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    base.OnFormClosing(e);
                    return;
                }
            }

            DisposeScope();
        }

        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeScope();
        }

        base.Dispose(disposing);
    }
}
