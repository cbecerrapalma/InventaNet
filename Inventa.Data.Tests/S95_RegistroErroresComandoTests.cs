using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>S95 (diagnóstico): el registro central de comandos SQL fallidos funciona de extremo a extremo.</summary>
[Collection("OfficialLocal")]
public sealed class S95_RegistroErroresComandoTests
{
    /// <summary>Entidad mapeada a una tabla que no existe: EF genera un SELECT válido que SQL Server rechaza (error 208).</summary>
    private sealed class Fantasma
    {
        public int Id { get; set; }
    }

    private sealed class ContextoFantasma(DbContextOptions<ContextoFantasma> o) : DbContext(o)
    {
        public DbSet<Fantasma> Fantasmas => Set<Fantasma>();

        protected override void OnModelCreating(ModelBuilder b) =>
            b.Entity<Fantasma>().ToTable("S95_TablaInexistenteDiagnostico");
    }

    [Fact]
    public async Task Comando_fallido_queda_registrado_con_SQL_y_numero_de_error()
    {
        var opciones = new DbContextOptionsBuilder<ContextoFantasma>().UseSqlServer(OfficialLocalFixture.ConnectionString).Options;
        await using var ctx = new ContextoFantasma(opciones);

        await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(
            () => ctx.Fantasmas.AsNoTracking().CountAsync(TestContext.Current.CancellationToken));

        var entrada = Assert.Single(RegistroErroresComandoEf.EntradasRecientes, e => e.Contains("S95_TablaInexistenteDiagnostico"));
        Assert.Contains("Error SQL: 208", entrada);
        Assert.Contains(nameof(Comando_fallido_queda_registrado_con_SQL_y_numero_de_error), entrada);
        Assert.Contains("S95_TablaInexistenteDiagnostico", File.ReadAllText(RegistroErroresComandoEf.Archivo));
    }

    [Fact]
    public void Error_que_no_es_de_SQL_se_formatea_sin_numero()
    {
        var texto = RegistroErroresComandoEf.Formatear("prueba-x", null, new InvalidOperationException("falla"));
        Assert.Contains("prueba: prueba-x", texto);
        Assert.Contains("Error SQL: n/d | InvalidOperationException: falla", texto);
    }
}
