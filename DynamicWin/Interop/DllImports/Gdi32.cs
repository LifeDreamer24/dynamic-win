using System.Runtime.InteropServices;

namespace DynamicWin.Interop.DllImports;

internal static class Gdi32
{
    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DeleteObject(IntPtr hObject);
}