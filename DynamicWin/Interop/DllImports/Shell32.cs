using System.Runtime.InteropServices;
using DynamicWin.Interop.ComImports;

namespace DynamicWin.Interop.DllImports;

internal static class Shell32
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int SHCreateItemFromParsingName(
        [MarshalAs(UnmanagedType.LPWStr)] string path,
        // The following parameter is not used - binding context.
        IntPtr pbc,
        ref Guid riid,
        [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);
}