namespace Inventa.WebAPI.Tests;

/// <summary>
/// S89.2-G2: credenciales dummy para pruebas. Valores con prefijo "dummy-",
/// nunca secretos reales (ver tools/Shared/Assert-No-SecretLeakage.ps1).
/// </summary>
public static class TestDummies
{
    public const string Password = "dummy-test-password";
}

