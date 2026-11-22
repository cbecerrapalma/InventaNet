using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Models;

/// <summary>
/// S57.34: datos crudos para preview de publicación (el controller mapea a DTOs públicos).
/// El servicio nunca referencia tipos de WebAPI.
/// </summary>
public sealed record ProductPreviewHeader(
    long IdProd,
    string Codigo,
    string Nombre,
    long IdCategoria,
    string CatNombre,
    int? IdMarca,
    string? MarcaNombre);

public sealed record ProductPreview(
    ProductPreviewHeader Header,
    IReadOnlyList<VarProd> Variantes,
    IReadOnlyList<PresProd> Presentaciones,
    IReadOnlyList<Precio> Precios,
    IReadOnlyDictionary<int, string> Unidades,
    string? ImagenPrincipalUrl);
