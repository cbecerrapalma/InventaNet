using System.Runtime.CompilerServices;

// S74: expone internals (SpPublicCardRow, MapRow) a la prueba de equivalencia.
// Segundo archivo de producto del presupuesto S74 (reserva justificada en S74-RESULT.md).
[assembly: InternalsVisibleTo("Inventa.Data.Tests")]

