using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services;
using Inventa.Domain.Entities;
using Xunit;

namespace Inventa.Application.Tests;

// S58.03 P0-A: tenant-scoping a nivel servicio + mapeo de errores de negocio.
// Garantía migrada: PLAYWRIGHT s56.25/s57.70 (tenant isolation) → C# COVERAGE
// (los specs Playwright NO se eliminan en este sprint).
public sealed class ProductoServiceTenantTests
{
    private static Prod ProdT6() => new()
    {
        IdProd = 9001, IdTenant = 6, IdCategoria = 1, IdUniBase = 1,
        Codigo = "S5803-TENANT-9001", Nombre = "S5803 Tenant",
    };

    private static ProductoServiceAsync Sut(StubProdRepositoryAsync repo) =>
        new(repo, new StubImagenStorage());

    [Fact]
    public async Task GetByIdInTenant_DevuelveProductoDelTenant()
    {
        var prod = ProdT6();
        var repo = new StubProdRepositoryAsync
        {
            GetByIdInTenant = (id, tenant, _) =>
                Task.FromResult<Prod?>(
                    id == prod.IdProd && tenant == prod.IdTenant ? prod : null),
        };

        var r = await Sut(repo).GetByIdInTenantAsync(prod.IdProd, prod.IdTenant, Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Value);
        Assert.Equal(prod.IdProd, r.Value!.IdProd);
    }

    [Fact]
    public async Task GetByIdInTenant_OtroTenant_RetornaNull_Para404DelController()
    {
        // Contrato S57.88: el servicio retorna success-with-null y el controller
        // mapea a 404 PRODUCTO_NO_ENCONTRADO. Cross-tenant nunca filtra datos.
        var repo = new StubProdRepositoryAsync
        {
            GetByIdInTenant = (_, _, _) => Task.FromResult<Prod?>(null),
        };

        var r = await Sut(repo).GetByIdInTenantAsync(9001, 999, Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsSuccess);
        Assert.Null(r.Value);
    }

    [Fact]
    public async Task SetVisibleStore_FalloRelocate_Mapea500SinRevertirFlag()
    {
        // Contrato S57.83-G3: flag aplicado (fuente de verdad), archivos después;
        // fallo de IO => 500 IMAGEN_RELOCATE_ERROR explícito, nunca revierte el flag.
        var prod = ProdT6();
        var repo = new StubProdRepositoryAsync
        {
            SetVisibleStore = (_, _, _, _) => Task.FromResult<Prod?>(prod),
        };
        var storage = new StubImagenStorage
        {
            OnRelocate = (_, _) => throw new IOException("disco simulado"),
        };

        var r = await new ProductoServiceAsync(repo, storage)
            .SetVisibleStoreAsync(prod.IdProd, prod.IdTenant, true, Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsFailure);
        Assert.Equal("IMAGEN_RELOCATE_ERROR", r.Error!.Code);
        Assert.Equal(500, r.Error.HttpStatusCode);
    }

    [Fact]
    public async Task SetVisibleStore_RelocateOk_RetornaProducto()
    {
        var prod = ProdT6();
        var repo = new StubProdRepositoryAsync
        {
            SetVisibleStore = (_, _, _, _) => Task.FromResult<Prod?>(prod),
        };
        var relocated = new List<(long IdProd, bool Public)>();
        var storage = new StubImagenStorage
        {
            OnRelocate = (id, pub) =>
            {
                relocated.Add((id, pub));
                return Task.FromResult(Result<int>.Success(0));
            },
        };

        var r = await new ProductoServiceAsync(repo, storage)
            .SetVisibleStoreAsync(prod.IdProd, prod.IdTenant, true, Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Value);
        Assert.Single(relocated);
        Assert.Equal((prod.IdProd, true), relocated[0]);
    }

    private sealed class StubImagenStorage : IProductoImagenStorageServiceAsync
    {
        public Func<long, bool, Task<Result<int>>>? OnRelocate { get; set; }

        public Task<Result<int>> RelocateAsync(long idProd, int idTenant, bool makePublic, CancellationToken ct = default) =>
            OnRelocate is null
                ? throw new NotImplementedException(nameof(RelocateAsync))
                : OnRelocate(idProd, makePublic);

        public Task<Result<ProductoImagenSubida>> IniciarSubidaAsync(long idProd, int idTenant, string? nombreArchivo, string? contentType, long tamanoBytes, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task<Result<ProductoImagen>> ConfirmarSubidaAsync(long idProd, int idTenant, long idArchivo, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task<Result<IReadOnlyList<ProductoImagen>>> GetActivesAsync(long idProd, int idTenant, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task<Result> DeleteAsync(long idProd, int idTenant, int idProductoImagen, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task<Result<IReadOnlyList<string>>> FindOrphanFilesAsync(int idTenant, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task<Result> GenerarVariantesAsync(int idProductoImagen, long idProd, int idTenant, CancellationToken ct = default) =>
            throw new NotImplementedException();
    }
}

