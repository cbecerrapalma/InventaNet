using System.Text.RegularExpressions;
using Xunit;

namespace Inventa.Web.Tests;

/// <summary>
/// S93.3 (C5): convencion de formato es-CL como prueba C# (la guardia PS no la reemplaza).
/// Todo *.razor de Inventa.Web debe cumplir:
/// montos -&gt; FormatoCl.Moneda, cantidades -&gt; Cantidad, enteros/conteos -&gt; Miles,
/// porcentajes -&gt; Porcentaje; prohibidos ToString("N/C/F"), interpolaciones {x:N/C/F},
/// FormatoCl.Numero( (ambiguo), conteos crudos (@X.Count) y valores con % crudo (-@pct%).
/// Identificadores (Ids, filas de archivo, pasos) quedan fuera: no son conteos.
/// </summary>
public sealed class S933_FormatoConvencionTests
{
    private static readonly (string Nombre, string Patron)[] Reglas =
    [
        ("ToString-N-C-F", @"\.ToString\(\s*""[NCF]"),
        ("Interpolacion-N-C-F", @"\{[^{}]*:[NCF]\d*\}"),
        ("FormatoCl.Numero-ambiguo", @"FormatoCl\.Numero\("),
        ("Conteo-crudo", @"\(@(?!FormatoCl\.Miles\()[^)]*\.(Count|Total)\b"),
        ("Porcentaje-crudo", @"@[\w.)\]]+%"),
        ("Cantidad-como-porcentaje", @"FormatoCl\.Cantidad\([^)]*\)\s*%"),
    ];

    private static string DirApp()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "InventaNet.slnx")))
            dir = dir.Parent;
        Assert.True(dir is not null, "No se encontro InventaNet.slnx hacia arriba desde " + AppContext.BaseDirectory);
        return Path.Combine(dir!.FullName, "Inventa.Web");
    }

    [Fact]
    public void Razor_CumpleConvencionFormatoEsCl()
    {
        var raiz = DirApp();
        var archivos = Directory.GetFiles(raiz, "*.razor", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .OrderBy(f => f)
            .ToList();
        Assert.NotEmpty(archivos);

        var fallos = new List<string>();
        foreach (var archivo in archivos)
        {
            var lineas = File.ReadAllLines(archivo);
            for (var i = 0; i < lineas.Length; i++)
            {
                foreach (var (nombre, patron) in Reglas)
                {
                    if (Regex.IsMatch(lineas[i], patron))
                        fallos.Add($"{Path.GetRelativePath(raiz, archivo)}:{i + 1} [{nombre}] {lineas[i].Trim()}");
                }
            }
        }

        Assert.True(fallos.Count == 0,
            "Convencion es-CL violada en Inventa.Web:" + Environment.NewLine + string.Join(Environment.NewLine, fallos));
    }

    [Theory]
    [InlineData(12990, "12.990")]
    [InlineData(0, "0")]
    [InlineData(1234567, "1.234.567")]
    public void Miles_FormatoEsperado(int valor, string esperado) =>
        Assert.Equal(esperado, CBP.Blazor.Formatting.FormatoCl.Miles(valor));

    [Theory]
    [InlineData(19.0, 0, "19 %")]
    [InlineData(42.5, 1, "42,5 %")]
    public void Porcentaje_FormatoEsperado(double valor, int decimales, string esperado) =>
        Assert.Equal(esperado, CBP.Blazor.Formatting.FormatoCl.Porcentaje((decimal)valor, decimales));

    [Fact]
    public void TasaPct_19_ViajaComoFraccion019() =>
        Assert.Equal(0.19m, Math.Round(19m / 100m, 4, MidpointRounding.AwayFromZero));
}
