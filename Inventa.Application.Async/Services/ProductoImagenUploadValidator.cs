using CBP.Results.Errors;
using CBP.Storage;

namespace Inventa.Application.Async.Services;

/// <summary>
/// Validación de imágenes de producto (S57.71). Desde S104 es una fachada de CBP.Storage:
/// <see cref="UploadPolicy.Images"/> (nombre, tipo y tamaño) y <see cref="FileSignature"/> (firma binaria).
/// Aquí solo queda lo propio de Inventa: la extensión del nombre debe corresponder al tipo declarado.
/// Null = válido.
/// </summary>
public static class ProductoImagenUploadValidator
{
    /// <summary>Largo máximo del nombre (columnas <c>NombreArchivo</c>).</summary>
    public const int MaxNombre = 255;

    /// <summary>Política de las imágenes de producto: JPEG, PNG y WebP de hasta 5 MB.</summary>
    public static UploadPolicy Policy => UploadPolicy.Images;

    public static long MaxBytes => Policy.MaxBytes;

    /// <summary>Valida lo que el cliente declara antes de subir: nombre, tipo y tamaño.</summary>
    public static Error? ValidateDeclared(string? nombreArchivo, string? contentType, long tamanoBytes)
    {
        var invalid = Policy.Validate(nombreArchivo, contentType, tamanoBytes);
        if (invalid is not null) return Fail(invalid.Message);
        if (nombreArchivo!.Length > MaxNombre) return Fail("Nombre de archivo demasiado largo.");

        var ext = Path.GetExtension(nombreArchivo);
        var esperada = FileSignature.ExtensionFor(contentType);
        var coincide = string.Equals(ext, esperada, StringComparison.OrdinalIgnoreCase)
            || (esperada == ".jpg" && string.Equals(ext, ".jpeg", StringComparison.OrdinalIgnoreCase));
        return coincide ? null : Fail($"La extensión del archivo ({ext}) no corresponde al tipo {contentType}.");
    }

    /// <summary>Valida lo declarado y, además, que el contenido tenga la firma binaria de su tipo.</summary>
    public static Error? Validate(string? nombreArchivo, string? contentType, byte[]? content)
    {
        var invalid = ValidateDeclared(nombreArchivo, contentType, content?.LongLength ?? 0);
        if (invalid is not null) return invalid;
        var header = content.AsSpan(0, Math.Min(content!.Length, FileSignature.HeaderLength));
        return FileSignature.Matches(contentType, header) ? null : Fail("El contenido no corresponde a una imagen válida.");
    }

    private static Error Fail(string msg) =>
        new Error("INVALID_IMAGE", msg).WithType(ErrorType.Validation).WithHttpStatus(422);
}
