namespace Inventa.Store.Models;

// S64 espejo del contrato S63 (PublicCatalogController.cs:133-141). Campos nuevos
// con default para tolerar un API sin S63: la deserialización los deja en null/0.
// S69: IdPadre/Orden/ImagenUrl (defaults => API sin S69 deja todo como raíz).
public sealed record PublicProductDto(long IdProd, string Codigo, string Nombre, long IdCategoria, string CategoriaNombre, int? IdMarca, string? MarcaNombre, string? UnidadNombre, string? ImagenUrl, decimal? PrecioDesde, bool TieneOferta, decimal? PrecioFinalDesde = null, decimal? PrecioReferencia = null, int? PctDescuento = null, DateOnly? OfertaHasta = null);
public sealed record PublicProductDetailDto(long IdProd, string Codigo, string Nombre, long IdCategoria, string CategoriaNombre, int? IdMarca, string? MarcaNombre, IReadOnlyList<PublicVarianteDto> Variantes, IReadOnlyList<PublicPriceDto> PreciosVigentes, string? ImagenUrl, IReadOnlyList<PublicImageDto> Imagenes);
public sealed record PublicImageDto(string Url, bool EsPrincipal, int Orden);
public sealed record PublicVarianteDto(long IdVarProd, string Codigo, string Nombre, IReadOnlyList<PublicPresentacionDto> Presentaciones);
public sealed record PublicPresentacionDto(long IdPresProd, string Descrip, string Unidad, IReadOnlyList<PublicPriceDto> Precios, IReadOnlyList<PublicOfertaDto>? Ofertas = null, decimal? PrecioVigente = null, decimal? PrecioFinal = null, int? PctDescuento = null);
public sealed record PublicOfertaDto(long IdOferta, decimal PrecioOferta, DateOnly FechaDesde, DateOnly? FechaHasta, decimal? PrecioReferenciaValor, bool ReferenciaEsVigente, int? PctDescuento = null);
public sealed record PublicPriceDto(long IdPrecio, decimal Valor, DateOnly FecVig, bool Vigente);
public sealed record PublicCategoryDto(long IdCategoria, string Codigo, string Nombre, int Cantidad = 0, long? IdPadre = null, short Orden = 0, string? ImagenUrl = null);
public sealed record PublicBrandDto(int IdMarca, string Codigo, string Nombre, int Cantidad);
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);
public sealed record PublicStorefrontDto(string NombreLegal, string? NombreFantasia, string? DescripcionCorta, string? Descripcion, string? LogoUrl, PublicContactDto Contacto, IReadOnlyList<PublicSocialDto> Redes, IReadOnlyList<PublicHourDto> Horarios, PublicBrandingDto? Branding = null);
// S61.1 espejo del contrato S60 (PublicCatalogController.cs:134). Nullable con default
// para tolerar un API sin S60: la deserialización deja Branding en null.
public sealed record PublicBrandingDto(string? ColorAcento, string? FaviconUrl, string? AnuncioTexto, string PerfilStore, string Moneda);
public sealed record PublicContactDto(string? Telefono, string? WhatsApp, string? Email, string? Direccion, string? Comuna, string? Ciudad, string? Region, string? SitioWeb);
public sealed record PublicSocialDto(string Tipo, string Url);
public sealed record PublicHourDto(byte DiaSemana, string? Apertura1, string? Cierre1, string? Apertura2, string? Cierre2, bool Cerrado);
