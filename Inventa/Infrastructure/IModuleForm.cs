namespace Inventa.Infrastructure;

public interface IModuleForm
{
    string ModuleKey { get; }
    int IdTenant { get; set; }
    bool HasChanges { get; }
    bool IsNew { get; }
    bool CanClose();
    void Initialize(int tenantId);
}
