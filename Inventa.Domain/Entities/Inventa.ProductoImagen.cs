namespace Inventa.Domain.Entities;

public class ProductoImagen
{
    public int IdProductoImagen { get; set; }
    public long IdProd { get; set; }
    public string Url { get; set; } = null!;
    public string NombreArchivo { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long TamanoBytes { get; set; }
    public int Orden { get; set; } = 0;
    public bool EsPrincipal { get; set; } = false;
    public bool Activo { get; set; } = true;
    public DateTime FecCrea { get; set; } = DateTime.UtcNow;
    /// <summary>Archivo del storage; <c>null</c> en filas anteriores a S104.</summary>
    public long? IdArchivo { get; set; }

    /// <summary>Variante de listado (400 px) generada por CBP.Storage.Imaging; <c>null</c> = sin generar (S106).</summary>
    public string? UrlMiniatura { get; set; }

    /// <summary>Variante de ficha (1200 px) generada por CBP.Storage.Imaging; <c>null</c> = sin generar (S106).</summary>
    public string? UrlFicha { get; set; }

    public Prod Prod { get; set; } = null!;
    public Archivo? Archivo { get; set; }
}
