using Inventa.Data.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

// S58.03 FASE 2: fixture único OfficialLocal (fail fast, sin env, sin fallback).
// Fuente canónica: Inventa.WebAPI/appsettings.json → ConnectionStrings:InventaDb.
// REGLA: estos tests son SOLO LECTURA (SELECT). Ninguno escribe: no hay
// S5803-* ni cleanup porque no se crea nada. Si SQL está caído, fallan con
// SqlException de conectividad (BLOCKED_EXTERNAL), nunca con otro backend.
public sealed class OfficialLocalFixture
{
    public const string ConnectionString =
        "Server=.;Database=Inventa;Trusted_Connection=True;TrustServerCertificate=True;";

    // Master por CODIGO (S86): producto ABA-T0001 del tenant 4 (ABARROTES),
    // primera variante por IdVarProd y su presentacion. Los ids numericos
    // murieron con el vaciado S86; nada aqui fija ids.
    public const string MasterCodProd = "ABA-T0001";
    public const int MasterIdTenant = 4;
    public const int OtroTenant = 5;

    private readonly DbContextOptions<InventaDbContext> _options =
        new DbContextOptionsBuilder<InventaDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

    public InventaDbContext NewContext() => new(_options);

    public async Task<long> MasterIdProdAsync()
    {
        using var ctx = NewContext();
        var p = await ctx.Productos.FirstOrDefaultAsync(p =>
            p.IdTenant == MasterIdTenant && p.Codigo == MasterCodProd);
        if (p is null)
            throw new InvalidOperationException(
                "S86: semilla ABA-T0001 ausente en T4. STOP, no skip.");
        return p.IdProd;
    }

    // Presentaciones del master ordenadas por IdPresProd (S86: 1 por variante).
    public async Task<List<long>> MasterPresIdsAsync()
    {
        using var ctx = NewContext();
        var idProd = await MasterIdProdAsync();
        var varIds = await ctx.Variantes.Where(v => v.IdProd == idProd)
            .Select(v => v.IdVarProd).ToListAsync();
        if (varIds.Count == 0)
            throw new InvalidOperationException("S86: ABA-T0001 sin variantes. STOP.");
        return await ctx.Presentaciones.Where(pp => varIds.Contains(pp.IdVarProd))
            .OrderBy(pp => pp.IdPresProd).Select(pp => pp.IdPresProd).ToListAsync();
    }

    public async Task<(long IdVarProd, long IdPresProd)> MasterVarPresAsync()
    {
        using var ctx = NewContext();
        var idProd = await MasterIdProdAsync();
        var v = await ctx.Variantes.Where(v => v.IdProd == idProd)
            .OrderBy(v => v.IdVarProd).FirstOrDefaultAsync();
        if (v is null)
            throw new InvalidOperationException("S86: ABA-T0001 sin variantes. STOP.");
        var pp = await ctx.Presentaciones.Where(p => p.IdVarProd == v.IdVarProd)
            .OrderBy(p => p.IdPresProd).FirstOrDefaultAsync();
        if (pp is null)
            throw new InvalidOperationException("S86: variante master sin presentación. STOP.");
        return (v.IdVarProd, pp.IdPresProd);
    }
}

[CollectionDefinition("OfficialLocal")]
public sealed class OfficialLocalCollection : ICollectionFixture<OfficialLocalFixture>;

