using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using Inventa.Store.Services;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S68 pruebas puras: StoreJwt parseo/display (nunca validacion).</summary>
public sealed class StoreJwtTests
{
    // Helper: crea JWT sin firma (none alg) para testing display
    private static string CreateToken(params (string type, string value)[] claims)
    {
        var handler = new JwtSecurityTokenHandler();
        var claimList = claims.Select(c => new Claim(c.type, c.value)).ToList();
        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claimList,
            notBefore: null,
            expires: null,
            signingCredentials: null
        );
        return handler.WriteToken(token);
    }

    [Fact]
    public void Parse_Null_ReturnsNull() =>
        Assert.Null(StoreJwt.Parse(null));

    [Fact]
    public void Parse_Empty_ReturnsNull() =>
        Assert.Null(StoreJwt.Parse(""));

    [Fact]
    public void Parse_Whitespace_ReturnsNull() =>
        Assert.Null(StoreJwt.Parse("   "));

    [Fact]
    public void Parse_InvalidFormat_ReturnsNull() =>
        Assert.Null(StoreJwt.Parse("not.a.jwt"));

    [Fact]
    public void Parse_ValidToken_ReturnsPrincipal()
    {
        var token = CreateToken(("name", "Juan"), ("email", "juan@test.cl"), ("sub", "123"));
        var p = StoreJwt.Parse(token);
        Assert.NotNull(p);
        Assert.Equal("Juan", p.GetName());
    }

    [Fact]
    public void GetName_Priority_NameClaim()
    {
        var token = CreateToken(("name", "DisplayName"), (ClaimTypes.NameIdentifier, "sub-val"));
        var p = StoreJwt.Parse(token);
        Assert.Equal("DisplayName", p.GetName());
    }

    [Fact]
    public void GetName_Fallback_NameIdentifier()
    {
        var token = CreateToken((ClaimTypes.NameIdentifier, "sub-456"));
        var p = StoreJwt.Parse(token);
        Assert.Equal("sub-456", p.GetName());
    }

    [Fact]
    public void GetName_Fallback_Sub()
    {
        var token = CreateToken(("sub", "sub-789"));
        var p = StoreJwt.Parse(token);
        Assert.Equal("sub-789", p.GetName());
    }

    [Fact]
    public void GetName_Fallback_PreferredUsername()
    {
        var token = CreateToken(("preferred_username", "user123"));
        var p = StoreJwt.Parse(token);
        Assert.Equal("user123", p.GetName());
    }

    [Fact]
    public void GetName_Fallback_Email()
    {
        var token = CreateToken((ClaimTypes.Email, "email@test.cl"));
        var p = StoreJwt.Parse(token);
        Assert.Equal("email@test.cl", p.GetName());
    }

    [Fact]
    public void GetName_NoClaims_ReturnsNull()
    {
        var token = CreateToken();
        var p = StoreJwt.Parse(token);
        Assert.Null(p.GetName());
    }

    [Fact]
    public void GetExpiration_ValidExp_ReturnsDateTime()
    {
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString();
        var token = CreateToken(("exp", exp));
        var p = StoreJwt.Parse(token);
        var dt = p.GetExpiration();
        Assert.NotNull(dt);
        Assert.True(dt.Value > DateTimeOffset.UtcNow);
    }

    [Fact]
    public void GetExpiration_NoExp_ReturnsNull()
    {
        var token = CreateToken(("name", "test"));
        var p = StoreJwt.Parse(token);
        Assert.Null(p.GetExpiration());
    }

    [Fact]
    public void IsExpired_Expired_ReturnsTrue()
    {
        var exp = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds().ToString();
        var token = CreateToken(("exp", exp));
        var p = StoreJwt.Parse(token);
        Assert.True(p.IsExpired());
    }

    [Fact]
    public void IsExpired_NotExpired_ReturnsFalse()
    {
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString();
        var token = CreateToken(("exp", exp));
        var p = StoreJwt.Parse(token);
        Assert.False(p.IsExpired());
    }

    [Fact]
    public void IsExpired_NoExp_ReturnsTrue()
    {
        var token = CreateToken(("name", "test"));
        var p = StoreJwt.Parse(token);
        Assert.True(p.IsExpired());
    }

    [Fact]
    public void IsExpired_WithSkew_RespectsSkew()
    {
        var exp = DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeSeconds().ToString();
        var token = CreateToken(("exp", exp));
        var p = StoreJwt.Parse(token);
        // 5 min skew -> expirado
        Assert.True(p.IsExpired(TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void GetClaim_ReturnsValue()
    {
        var token = CreateToken(("custom_claim", "custom-value"));
        var p = StoreJwt.Parse(token);
        Assert.Equal("custom-value", p.GetClaim("custom_claim"));
    }

    [Fact]
    public void GetClaim_Missing_ReturnsNull()
    {
        var token = CreateToken(("name", "test"));
        var p = StoreJwt.Parse(token);
        Assert.Null(p.GetClaim("missing"));
    }

    // S96.1 D5: el Id numérico de PassPlat no se muestra como nombre.
    [Fact]
    public void GetName_IgnoraIdNumerico_DevuelveNull()
    {
        var token = CreateToken((ClaimTypes.NameIdentifier, "13"), ("sub", "13"));
        var p = StoreJwt.Parse(token);
        Assert.Null(p.GetName());
    }

    [Fact]
    public void GetName_IgnoraIdNumerico_UsaCorreo()
    {
        var token = CreateToken((ClaimTypes.NameIdentifier, "13"), (ClaimTypes.Email, "ana@x.cl"));
        var p = StoreJwt.Parse(token);
        Assert.Equal("ana@x.cl", p.GetName());
    }
}
