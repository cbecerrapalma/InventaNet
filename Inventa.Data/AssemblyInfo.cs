using System.Runtime.CompilerServices;

// S75: expone internals (SpErrorMapper) a la prueba de mapeo de errores.
// Mismo patrón que Inventa.Data.Async (S74). Sin este archivo el test no compila.
[assembly: InternalsVisibleTo("Inventa.Data.Tests")]

