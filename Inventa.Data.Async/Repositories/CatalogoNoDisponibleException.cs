namespace Inventa.Data.Async.Repositories;

public sealed class CatalogoNoDisponibleException : Exception
{
    public string Code { get; }

    public CatalogoNoDisponibleException(string code, string? message = null, Exception? innerException = null)
        : base(message ?? $"El catálogo no está disponible actualmente ({code}).", innerException)
    {
        Code = code;
    }
}
