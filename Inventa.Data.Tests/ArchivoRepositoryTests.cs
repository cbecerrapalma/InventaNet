using CBP.Storage;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S104 K4: el registro de subidas (tabla Archivo) contra SQL real (localhost/Inventa).
/// La prueba abre una transacción y la revierte: no deja filas.
/// </summary>
[Collection("OfficialLocal")]
public sealed class ArchivoRepositoryTests(OfficialLocalFixture fx)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static PendingUpload Pendiente(int tenant, long bytes) =>
        new(tenant.ToString(), "Local", new StorageObjectRef(StorageContainers.Public, $"t{tenant}/inventa/producto/1/{Guid.NewGuid():N}.png"),
            "producto", "1", "foto.png", "image/png", bytes);

    [Fact]
    public async Task Pendiente_confirmado_y_uso_del_tenant_filtran_por_tenant()
    {
        using var ctx = fx.NewContext();
        await using var tx = await ctx.Database.BeginTransactionAsync(Ct);
        var repo = new ArchivoRepositoryAsync(ctx);
        var propio = OfficialLocalFixture.MasterIdTenant.ToString();
        var ajeno = OfficialLocalFixture.OtroTenant.ToString();
        var usoPropio = (await repo.GetTenantUsageAsync(propio, Ct)).Value;
        var usoAjeno = (await repo.GetTenantUsageAsync(ajeno, Ct)).Value;

        var subida = Pendiente(OfficialLocalFixture.MasterIdTenant, 100);
        var id = (await repo.RegisterPendingAsync(subida, Ct)).Value;
        var rechazada = (await repo.RegisterPendingAsync(Pendiente(OfficialLocalFixture.MasterIdTenant, 40), Ct)).Value;

        var pendiente = (await repo.GetAsync(id, propio, Ct)).Value;
        Assert.NotNull(pendiente);
        Assert.Equal((UploadState.Pending, subida.Object, 100L, "foto.png"), (pendiente.State, pendiente.Object, pendiente.ContentLength, pendiente.FileName));
        Assert.Null((await repo.GetAsync(id, ajeno, Ct)).Value);
        Assert.Null(await repo.GetByIdAsync(long.Parse(id), OfficialLocalFixture.OtroTenant, Ct));
        Assert.Equal(usoPropio + 140, (await repo.GetTenantUsageAsync(propio, Ct)).Value);
        Assert.Equal(usoAjeno, (await repo.GetTenantUsageAsync(ajeno, Ct)).Value);

        var info = new StorageObjectInfo(subida.Object.Container, subida.Object.Key, 100, "image/png", "etag-s104", DateTimeOffset.UtcNow);
        Assert.True((await repo.MarkConfirmedAsync(id, info, Ct)).IsSuccess);
        Assert.True((await repo.MarkRejectedAsync(rechazada, Ct)).IsSuccess);
        Assert.True((await repo.MarkRejectedAsync(id, Ct)).IsFailure);

        var confirmado = (await repo.GetAsync(id, propio, Ct)).Value;
        Assert.Equal((UploadState.Confirmed, "etag-s104"), (confirmado!.State, confirmado.ETag));
        Assert.Equal(UploadState.Rejected, (await repo.GetAsync(rechazada, propio, Ct)).Value!.State);
        Assert.Equal(usoPropio + 100, (await repo.GetTenantUsageAsync(propio, Ct)).Value);
        Assert.Contains(subida.Object.Key, await repo.GetLiveKeysAsync(StorageContainers.Public, OfficialLocalFixture.MasterIdTenant, Ct));
        Assert.DoesNotContain(subida.Object.Key, await repo.GetLiveKeysAsync(StorageContainers.Public, OfficialLocalFixture.OtroTenant, Ct));

        await repo.MarkDeletedAsync(long.Parse(id), OfficialLocalFixture.OtroTenant, Ct);
        Assert.Equal(ArchivoEstado.Confirmado, (await repo.GetByIdAsync(long.Parse(id), OfficialLocalFixture.MasterIdTenant, Ct))!.Estado);
        await repo.MarkDeletedAsync(long.Parse(id), OfficialLocalFixture.MasterIdTenant, Ct);
        Assert.Equal(usoPropio, (await repo.GetTenantUsageAsync(propio, Ct)).Value);
        Assert.Empty(await repo.GetByIdsAsync([long.Parse(id)], OfficialLocalFixture.OtroTenant, Ct));

        await tx.RollbackAsync(Ct);
        Assert.False(await ctx.Archivos.AsNoTracking().AnyAsync(a => a.Clave == subida.Object.Key, Ct));
    }

    [Fact]
    public async Task Barrido_lista_y_borra_solo_vencidos_del_proveedor_sin_imagen()
    {
        var idProd = await fx.MasterIdProdAsync();
        using var ctx = fx.NewContext();
        await using var tx = await ctx.Database.BeginTransactionAsync(Ct);
        var repo = new ArchivoRepositoryAsync(ctx);
        const int tenant = OfficialLocalFixture.MasterIdTenant;

        var pendiente = long.Parse((await repo.RegisterPendingAsync(Pendiente(tenant, 10), Ct)).Value);
        var otroProveedor = long.Parse((await repo.RegisterPendingAsync(Pendiente(tenant, 10) with { Provider = "Otro" }, Ct)).Value);
        var rechazado = (await repo.RegisterPendingAsync(Pendiente(tenant, 10), Ct)).Value;
        Assert.True((await repo.MarkRejectedAsync(rechazado, Ct)).IsSuccess);
        var conImagen = (await repo.RegisterPendingAsync(Pendiente(tenant, 10), Ct)).Value;
        Assert.True((await repo.MarkRejectedAsync(conImagen, Ct)).IsSuccess);
        await new ProductoImagenRepositoryAsync(ctx).CreateAsync(idProd, "uploads/s105.png", "s105.png", "image/png", 10, long.Parse(conImagen), Ct);
        var subidaConfirmada = Pendiente(tenant, 10);
        var confirmado = (await repo.RegisterPendingAsync(subidaConfirmada, Ct)).Value;
        var info = new StorageObjectInfo(subidaConfirmada.Object.Container, subidaConfirmada.Object.Key, 10, "image/png", "etag-s105", DateTimeOffset.UtcNow);
        Assert.True((await repo.MarkConfirmedAsync(confirmado, info, Ct)).IsSuccess);

        var vencidos = (await repo.GetPurgablesAsync("Local", DateTime.Now.AddMinutes(1), 1000, Ct)).Select(a => a.IdArchivo).ToList();
        Assert.Contains(pendiente, vencidos);
        Assert.Contains(long.Parse(rechazado), vencidos);
        Assert.DoesNotContain(otroProveedor, vencidos);
        Assert.DoesNotContain(long.Parse(conImagen), vencidos);
        Assert.DoesNotContain(long.Parse(confirmado), vencidos);

        // Dentro de la gracia no se purga nada recién creado o cerrado.
        var dentroDeLaGracia = (await repo.GetPurgablesAsync("Local", DateTime.Now.AddMinutes(-60), 1000, Ct)).Select(a => a.IdArchivo).ToList();
        Assert.DoesNotContain(pendiente, dentroDeLaGracia);
        Assert.DoesNotContain(long.Parse(rechazado), dentroDeLaGracia);

        Assert.False(await repo.DeletePurgedAsync(long.Parse(confirmado), ArchivoEstado.Confirmado, Ct));
        Assert.False(await repo.DeletePurgedAsync(long.Parse(conImagen), ArchivoEstado.Rechazado, Ct));
        Assert.False(await repo.DeletePurgedAsync(pendiente, ArchivoEstado.Rechazado, Ct));
        Assert.True(await repo.DeletePurgedAsync(pendiente, ArchivoEstado.Pendiente, Ct));
        Assert.Null(await repo.GetByIdAsync(pendiente, tenant, Ct));
        Assert.NotNull(await repo.GetByIdAsync(long.Parse(conImagen), tenant, Ct));
        Assert.NotNull(await repo.GetByIdAsync(long.Parse(confirmado), tenant, Ct));

        await tx.RollbackAsync(Ct);
    }

    [Fact]
    public async Task Barrido_cierra_solo_confirmados_vencidos_sin_imagen()
    {
        var idProd = await fx.MasterIdProdAsync();
        using var ctx = fx.NewContext();
        await using var tx = await ctx.Database.BeginTransactionAsync(Ct);
        var repo = new ArchivoRepositoryAsync(ctx);
        const int tenant = OfficialLocalFixture.MasterIdTenant;

        async Task<long> ConfirmarAsync(PendingUpload subida)
        {
            var id = (await repo.RegisterPendingAsync(subida, Ct)).Value;
            var info = new StorageObjectInfo(subida.Object.Container, subida.Object.Key, 10, "image/png", "etag-s132", DateTimeOffset.UtcNow);
            Assert.True((await repo.MarkConfirmedAsync(id, info, Ct)).IsSuccess);
            return long.Parse(id);
        }

        var sinImagen = await ConfirmarAsync(Pendiente(tenant, 10));
        var conImagen = await ConfirmarAsync(Pendiente(tenant, 10));
        await new ProductoImagenRepositoryAsync(ctx).CreateAsync(idProd, "uploads/s132.png", "s132.png", "image/png", 10, conImagen, Ct);
        var otraEntidad = await ConfirmarAsync(Pendiente(tenant, 10) with { Entity = "otra" });
        var pendiente = long.Parse((await repo.RegisterPendingAsync(Pendiente(tenant, 10), Ct)).Value);

        // Dentro de la gracia no se cierra nada recién confirmado.
        await repo.CerrarConfirmadosSinImagenAsync("producto", DateTime.Now.AddMinutes(-60), 1000, Ct);
        Assert.Equal(ArchivoEstado.Confirmado, (await repo.GetByIdAsync(sinImagen, tenant, Ct))!.Estado);

        // Vencidos: solo el confirmado de la entidad y sin imagen.
        var cerrados = await repo.CerrarConfirmadosSinImagenAsync("producto", DateTime.Now.AddMinutes(1), 1000, Ct);
        Assert.True(cerrados >= 1);
        Assert.Equal(ArchivoEstado.Eliminado, (await repo.GetByIdAsync(sinImagen, tenant, Ct))!.Estado);
        Assert.Equal(ArchivoEstado.Confirmado, (await repo.GetByIdAsync(conImagen, tenant, Ct))!.Estado);
        Assert.Equal(ArchivoEstado.Confirmado, (await repo.GetByIdAsync(otraEntidad, tenant, Ct))!.Estado);
        Assert.Equal(ArchivoEstado.Pendiente, (await repo.GetByIdAsync(pendiente, tenant, Ct))!.Estado);

        await tx.RollbackAsync(Ct);
    }
}
