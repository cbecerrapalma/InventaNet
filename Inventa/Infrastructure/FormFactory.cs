using CBP.WinFrms.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace Inventa.Infrastructure;

public interface IFormFactory : CBP.WinFrms.Hosting.IFormFactory
{
}

public sealed class FormFactory : IFormFactory
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FormFactory(IServiceScopeFactory scopeFactory)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        _scopeFactory = scopeFactory;
    }

    public TForm Create<TForm>() where TForm : Form
    {
        var guard = new ScopeGuard(_scopeFactory.CreateScope());
        var form = guard.Scope.ServiceProvider.GetRequiredService<TForm>();
        form.Disposed += (_, _) => guard.Dispose();
        return form;
    }

    public DialogResult ShowDialog<TForm>() where TForm : Form
        => ShowDialog<TForm>(null);

    public DialogResult ShowDialog<TForm>(Action<TForm>? configure) where TForm : Form
    {
        using var guard = new ScopeGuard(_scopeFactory.CreateScope());
        var form = guard.Scope.ServiceProvider.GetRequiredService<TForm>();
        form.FormClosed += (_, _) => guard.Dispose();
        configure?.Invoke(form);
        var owner = Form.ActiveForm;
        return owner is not null
            ? form.ShowDialog(owner)
            : form.ShowDialog();
    }

    private sealed class ScopeGuard : IDisposable
    {
        private int _disposed;

        public IServiceScope Scope { get; }

        public ScopeGuard(IServiceScope scope)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            Scope.Dispose();
        }
    }
}
