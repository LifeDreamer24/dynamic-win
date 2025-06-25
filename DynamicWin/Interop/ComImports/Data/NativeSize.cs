

using System.Runtime.InteropServices;

namespace DynamicWin.Interop.ComImports.Data;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeSize
{
    private int width;
    private int height;

    public int Width { set { width = value; } }
    public int Height { set { height = value; } }
};