using System.Runtime.InteropServices;
using DynamicWin.Interop.ComImports.Data;

namespace DynamicWin.Interop.ComImports;

[ComImport]
[Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IShellItemImageFactory
{
    [PreserveSig]
    HResult GetImage(
        [In, MarshalAs(UnmanagedType.Struct)] NativeSize size,
        [In] ThumbnailOptions flags,
        [Out] out IntPtr phbm);
}