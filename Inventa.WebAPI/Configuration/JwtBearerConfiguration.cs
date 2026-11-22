// S57.40: composición JWT centralizada. Sin environment, sin fallback dev, sin duplicación.
// Fuentes soportadas para Jwt:SecretKey (en orden de precedencia del builder):
//  1. UserSecrets (solo Development; requiere <UserSecretsId> en el csproj).
//  2. secrets.json machine-local (%PROGRAMDATA%\InventaNet\secrets.json), fuera del repo.
// appsettings.json conserva "SecretKey": "" (exigido por specs cerrados S56.31/33/34/35).
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Inventa.WebAPI.Configuration;

public static class JwtBearerConfiguration
{
    /// <summary>
    /// Archivo machine-local, fuera del repo, protegido por ACL del SO.
    /// Formato: { "Jwt": { "SecretKey": "...32+ caracteres..." } }
    /// </summary>
    public static string MachineSecretsFile => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "InventaNet", "secrets.json");

    public static void AddMachineSecretsFile(ConfigurationManager cfg)
    {
        ArgumentNullException.ThrowIfNull(cfg);
        cfg.AddJsonFile(MachineSecretsFile, optional: true, reloadOnChange: false);
    }

    /// <summary>
    /// Environment NO es fuente soportada para Jwt:SecretKey (S57.40). Si el valor
    /// resolviera desde un provider de variables de entorno => fail-fast.
    /// No toca otros providers (host sigue leyendo ASPNETCORE_*/DOTNET_* directo).
    /// </summary>
    public static void RejectEnvironmentSecret(ConfigurationManager cfg)
    {
        ArgumentNullException.ThrowIfNull(cfg);
        if (cfg is not IConfigurationRoot root) return;
        foreach (var p in root.Providers.OfType<EnvironmentVariablesConfigurationProvider>())
        {
            if (p.TryGet("Jwt:SecretKey", out var v) && !string.IsNullOrEmpty(v))
                throw new InvalidOperationException(
                    "Jwt:SecretKey por environment no soportado (S57.40). " +
                    "Use UserSecrets (dev) o secrets.json machine-local (prod).");
        }
    }

    /// <summary>
    /// Lee, bindea y valida JwtOptions con mensaje accionable (mirror PassPlat fail-fast).
    /// </summary>
    public static JwtOptions ReadAndValidate(IConfiguration cfg)
    {
        ArgumentNullException.ThrowIfNull(cfg);
        var options = new JwtOptions();
        cfg.GetSection(JwtOptions.SectionName).Bind(options);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true))
            throw new InvalidOperationException(
                "Jwt:SecretKey ausente o menor a 32 caracteres. Desarrollo: "
                + "dotnet user-secrets set \"Jwt:SecretKey\" \"<32+ caracteres>\" --project Inventa.WebAPI. "
                + "Produccion: archivo " + MachineSecretsFile
                + " con { \"Jwt\": { \"SecretKey\": \"...\" } }. Sin environment (S57.40).");
        return options;
    }

    /// <summary>UNICO punto de construcción de parámetros de validación (incl. /debug/*).</summary>
    public static TokenValidationParameters BuildValidationParameters(JwtOptions o)
    {
        ArgumentNullException.ThrowIfNull(o);
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = o.Issuer,
            ValidateAudience = true,
            ValidAudiences = o.ValidAudiences,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(o.ClockSkewMinutes)
        };
    }
}
