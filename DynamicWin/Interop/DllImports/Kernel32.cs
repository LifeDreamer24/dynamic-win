using System.Runtime.InteropServices;
using DynamicWin.Interop.DllImports.Data;

namespace DynamicWin.Interop.DllImports;

internal static class Kernel32
{
    [DllImport("kernel32.dll")]
    internal static extern bool GetSystemPowerStatus(out SystemPowerStatus lpSystemPowerStatus);
}