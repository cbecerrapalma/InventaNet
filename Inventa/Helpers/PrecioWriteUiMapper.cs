using CBP.Results.Errors;

namespace Inventa.Helpers;

// S57.70: política de tipos retirada en S57.69 (modelo normalizado).
// Este mapper conserva el contrato ToMessage(Error?) usado por 5 call sites;
// la rama especial inalcanzable se elimina. Sin cambios de UX.
public static class PrecioWriteUiMapper
{
    public static string ToMessage(Error? error)
    {
        if (error == null) return "Error desconocido al guardar precio.";
        return error.Message;
    }
}
