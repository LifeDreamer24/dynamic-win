using System.Runtime.InteropServices;
using DynamicWin.Interop.DllImports.Data;

namespace DynamicWin.Interop.DllImports;

internal static class User32
{
    [DllImport("user32.dll")]
    internal static extern int SetWindowLong(IntPtr window, int idx, int val);

    [DllImport("user32.dll")]
    internal static extern int GetWindowLong(IntPtr window, int idx);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DevMode devMode);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr SetWindowsHookEx(int idHook,
        KeyboardListener.LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr GetModuleHandle(string lpModuleName);
    
    [DllImport("user32.dll")]
    internal static extern void Keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
}