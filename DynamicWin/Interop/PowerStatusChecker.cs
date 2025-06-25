using DynamicWin.Interop.DllImports;
using DynamicWin.Interop.DllImports.Data;

namespace DynamicWin.Interop;

internal class PowerStatusChecker
{
    public static SystemPowerStatus GetPowerStatus()
    {
        if (Kernel32.GetSystemPowerStatus(out SystemPowerStatus status))
        {
            return status;
        }

        throw new Exception("Unable to get power status.");
    }
}