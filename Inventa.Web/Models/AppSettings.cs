namespace Inventa.Web.Models;

public sealed class AppSettings
{
    public string ApiBaseUrl { get; set; } = "https://localhost:5002";
    public string AppCodigo { get; set; } = "INVENTA"; // S57.19: INVENTA=12 (App 2 = APP_CI_232107 inactiva, nunca identidad Inventa)
}
