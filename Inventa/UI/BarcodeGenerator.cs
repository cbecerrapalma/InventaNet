using System.Drawing;
using ZXing;
using ZXing.Common;

namespace Inventa.UI;

/// <summary>
/// Helper class para generar códigos de barras usando ZXing.Net
/// Soporta múltiples formatos: CODE128, CODE39, EAN13, etc.
/// </summary>
public static class BarcodeGenerator
{
    /// <summary>
    /// Formato de código de barras predeterminado (CODE128)
    /// </summary>
    private const BarcodeFormat DEFAULT_FORMAT = BarcodeFormat.CODE_128;

    /// <summary>
    /// Anchura predeterminada de la imagen generada
    /// </summary>
    private const int DEFAULT_WIDTH = 600;

    /// <summary>
    /// Altura predeterminada de la imagen generada
    /// </summary>
    private const int DEFAULT_HEIGHT = 150;

    /// <summary>
    /// Genera una imagen de código de barras a partir de un valor
    /// </summary>
    /// <param name="codigo">Valor del código de barras</param>
    /// <param name="formato">Formato del código (predeterminado: CODE128)</param>
    /// <param name="mostrarTexto">Si true, muestra el valor debajo del código</param>
    /// <returns>Imagen generada, o null si falla</returns>
    public static Bitmap? GenerarCodigoBarras(
        string codigo,
        BarcodeFormat formato = DEFAULT_FORMAT,
        bool mostrarTexto = true)
    {
        return GenerarCodigoBarras(codigo, formato, DEFAULT_WIDTH, DEFAULT_HEIGHT, mostrarTexto);
    }

    /// <summary>
    /// Genera una imagen de código de barras con tamaño personalizado
    /// </summary>
    /// <param name="codigo">Valor del código de barras</param>
    /// <param name="formato">Formato del código</param>
    /// <param name="ancho">Ancho de la imagen en pixels</param>
    /// <param name="alto">Alto de la imagen en pixels</param>
    /// <param name="mostrarTexto">Si true, muestra el valor debajo del código</param>
    /// <returns>Imagen generada, o null si falla</returns>
    public static Bitmap? GenerarCodigoBarras(
        string codigo,
        BarcodeFormat formato,
        int ancho,
        int alto,
        bool mostrarTexto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            // ✅ Validar longitud según formato
            if (!ValidarLongitud(codigo, formato))
                return null;

            // ✅ Crear el escritor de códigos de barras con ZXing.Net
            var writer = new BarcodeWriter<BitMatrix>
            {
                Format = formato,
                Options = new EncodingOptions
                {
                    Width = ancho,
                    Height = mostrarTexto ? alto - 40 : alto,
                    Margin = 10,
                    PureBarcode = false
                }
            };

            // Generar bitmap con código de barras
            var bitArray = writer.Encode(codigo);
            var barcodeBitmap = ToBitmap(bitArray);

            if (!mostrarTexto)
                return barcodeBitmap;

            // ✅ Agregar texto debajo del código
            return AgregarTextoDebajo(barcodeBitmap, codigo, ancho, alto);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Genera un código QR
    /// </summary>
    /// <param name="contenido">Contenido del QR</param>
    /// <param name="mostrarTexto">Si true, muestra el contenido debajo</param>
    /// <returns>Imagen del QR generada</returns>
    public static Bitmap? GenerarCodigoQR(string contenido, bool mostrarTexto = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contenido))
                return null;

            // ✅ Los QRs típicamente son cuadrados
            return GenerarCodigoBarras(contenido, BarcodeFormat.QR_CODE, 250, 250, mostrarTexto);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Obtiene el nombre legible del formato de código
    /// </summary>
    /// <param name="formato">Formato de código</param>
    /// <returns>Nombre del formato (ej: "CODE128", "QR Code")</returns>
    public static string ObtenerNombreFormato(BarcodeFormat formato)
    {
        return formato switch
        {
            BarcodeFormat.CODE_128 => "CODE 128",
            BarcodeFormat.CODE_39 => "CODE 39",
            BarcodeFormat.EAN_13 => "EAN-13",
            BarcodeFormat.EAN_8 => "EAN-8",
            BarcodeFormat.QR_CODE => "QR Code",
            BarcodeFormat.CODABAR => "Codabar",
            BarcodeFormat.ITF => "ITF",
            BarcodeFormat.RSS_14 => "RSS 14",
            _ => "Código de barras"
        };
    }

    /// <summary>
    /// Valida si el código cumple con los requisitos de longitud del formato
    /// </summary>
    /// <param name="codigo">Código a validar</param>
    /// <param name="formato">Formato esperado</param>
    /// <returns>true si es válido, false si no</returns>
    private static bool ValidarLongitud(string codigo, BarcodeFormat formato)
    {
        var longitud = codigo.Length;

        return formato switch
        {
            // EAN-13 debe tener exactamente 13 dígitos
            BarcodeFormat.EAN_13 => longitud == 13 && codigo.All(char.IsDigit),
            // EAN-8 debe tener exactamente 8 dígitos
            BarcodeFormat.EAN_8 => longitud == 8 && codigo.All(char.IsDigit),
            // CODE128, CODE39, QR: sin restricciones de longitud estrictas
            _ => longitud > 0
        };
    }

    /// <summary>
    /// Convierte un BitMatrix de ZXing a un Bitmap de Windows Forms
    /// </summary>
    /// <param name="bitMatrix">BitMatrix generado por ZXing</param>
    /// <returns>Bitmap listo para mostrar</returns>
    private static Bitmap ToBitmap(BitMatrix bitMatrix)
    {
        var bitmap = new Bitmap(bitMatrix.Width, bitMatrix.Height);

        for (int x = 0; x < bitMatrix.Width; x++)
        {
            for (int y = 0; y < bitMatrix.Height; y++)
            {
                bitmap.SetPixel(x, y, bitMatrix[x, y] ? Color.Black : Color.White);
            }
        }

        return bitmap;
    }

    /// <summary>
    /// Agrega texto debajo de la imagen del código de barras
    /// </summary>
    /// <param name="codigoBitmap">Bitmap del código de barras</param>
    /// <param name="texto">Texto a mostrar</param>
    /// <param name="anchoTotal">Ancho total de la imagen final</param>
    /// <param name="altoTotal">Alto total de la imagen final</param>
    /// <returns>Nueva imagen con el texto agregado</returns>
    private static Bitmap AgregarTextoDebajo(Bitmap codigoBitmap, string texto, int anchoTotal, int altoTotal)
    {
        var imagenFinal = new Bitmap(anchoTotal, altoTotal);

        try
        {
            using (var graphics = Graphics.FromImage(imagenFinal))
            {
                graphics.Clear(Color.White);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // Dibujar el código de barras centrado en la parte superior
                var altoCodigo = codigoBitmap.Height;
                var xCodigo = (anchoTotal - codigoBitmap.Width) / 2;
                graphics.DrawImage(codigoBitmap, xCodigo, 10);

                // Dibujar el texto debajo
                using (var fuente = new Font("Courier New", 11, FontStyle.Bold))
                using (var pincel = new SolidBrush(Color.Black))
                using (var formato = new StringFormat { Alignment = StringAlignment.Center })
                {
                    var rectanguloTexto = new RectangleF(0, altoCodigo + 15, anchoTotal, 35);
                    graphics.DrawString(texto, fuente, pincel, rectanguloTexto, formato);
                }

                // Agregar borde
                using (var bolígrafo = new Pen(Color.Black, 2))
                {
                    graphics.DrawRectangle(bolígrafo, 0, 0, anchoTotal - 1, altoTotal - 1);
                }
            }

            codigoBitmap.Dispose();
            return imagenFinal;
        }
        catch
        {
            imagenFinal.Dispose();
            return codigoBitmap;
        }
    }
}

