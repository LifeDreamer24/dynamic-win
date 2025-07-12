using System.Runtime.InteropServices;

namespace DynamicWin.Interop.ComImports.Data;

[StructLayout(LayoutKind.Sequential)]
internal struct Rgbquad
{
    public byte rgbBlue;
    public byte rgbGreen;
    public byte rgbRed;
    public byte rgbReserved;
}