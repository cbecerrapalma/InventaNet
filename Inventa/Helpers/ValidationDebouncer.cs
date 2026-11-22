using CBP.WinFrms.Utilities;

namespace Inventa.Helpers;

public class ValidationDebouncer : CBP.WinFrms.Utilities.ValidationDebouncer
{
    public ValidationDebouncer(int delayMs = 300) : base(delayMs) { }
}
