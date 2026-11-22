using Inventa.Store.Models;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S68 pruebas puras: StoreAuthMessages tabla unica es-CL.</summary>
public sealed class StoreAuthMessagesTests
{
    [Fact]
    public void GetMessage_UnknownCode_ReturnsGeneric() =>
        Assert.Equal(StoreAuthMessages.Generic, StoreAuthMessages.GetMessage("UNKNOWN_CODE"));

    [Fact]
    public void GetMessage_NullCode_ReturnsGeneric() =>
        Assert.Equal(StoreAuthMessages.Generic, StoreAuthMessages.GetMessage(null));

    [Theory]
    [InlineData(StoreAuthCodes.Validation, "Ingresa tu correo o usuario y tu contraseña.")]
    [InlineData(StoreAuthCodes.InvalidCredentials, "Usuario o contraseña incorrectos.")]
    [InlineData(StoreAuthCodes.Forbidden, "Tu cuenta no tiene acceso a esta tienda o está bloqueada. Contacta a la tienda.")]
    [InlineData(StoreAuthCodes.MfaRequired, "Tu cuenta requiere verificación en dos pasos, que aún no está disponible aquí. Puedes ingresar por el panel de administración.")]
    [InlineData(StoreAuthCodes.PasswordChangeRequired, "Debes cambiar tu contraseña antes de ingresar. Hazlo desde el panel de administración.")]
    [InlineData(StoreAuthCodes.RateLimited, "Demasiados intentos. Espera unos segundos e inténtalo de nuevo.")]
    [InlineData(StoreAuthCodes.NotConfigured, "No pudimos procesar el ingreso. Inténtalo de nuevo en unos minutos.")]
    [InlineData(StoreAuthCodes.Upstream, "No pudimos procesar el ingreso. Inténtalo de nuevo en unos minutos.")]
    [InlineData(StoreAuthCodes.RefreshInvalid, "No pudimos procesar el ingreso. Inténtalo de nuevo en unos minutos.")]
    public void GetMessage_KnownCode_ReturnsSpecific(string code, string expected) =>
        Assert.Equal(expected, StoreAuthMessages.GetMessage(code));

    [Fact]
    public void Generic_NoRevelaExistenciaUsuario()
    {
        var msg = StoreAuthMessages.GetMessage(StoreAuthCodes.InvalidCredentials);
        Assert.DoesNotContain("no existe", msg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("no encontrado", msg, StringComparison.OrdinalIgnoreCase);
        // El mensaje genérico "Usuario o contraseña incorrectos" no revela si el usuario existe
        Assert.DoesNotContain("no existe", msg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("no encontrado", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MfaRequired_MencionaPanelAdministracion()
    {
        var msg = StoreAuthMessages.GetMessage(StoreAuthCodes.MfaRequired);
        Assert.Contains("panel de administración", msg);
    }

    [Fact]
    public void PasswordChangeRequired_MencionaPanelAdministracion()
    {
        var msg = StoreAuthMessages.GetMessage(StoreAuthCodes.PasswordChangeRequired);
        Assert.Contains("panel de administración", msg);
    }

    [Fact]
    public void Forbidden_MencionaContactarTienda()
    {
        var msg = StoreAuthMessages.GetMessage(StoreAuthCodes.Forbidden);
        Assert.Contains("Contacta a la tienda", msg);
    }
}
