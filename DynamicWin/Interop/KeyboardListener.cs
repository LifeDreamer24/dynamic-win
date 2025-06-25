using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DynamicWin.Interop.DllImports;
// ReSharper disable InconsistentNaming

namespace DynamicWin.Interop;

public static class KeyboardListener
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    public static void Start()
    {
        _hookID = SetHook(_proc);
    }

    public static void Stop()
    {
        User32.UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return User32.SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                User32.GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    internal delegate IntPtr LowLevelKeyboardProc(
        int nCode, IntPtr wParam, IntPtr lParam);

    public static List<Keys> KeyDown = [];
    public static Action<Keys, KeyModifier> OnKeyDown;

    private static IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
    {
        int vkCode = Marshal.ReadInt32(lParam);
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN && !KeyDown.Contains((Keys)vkCode))
        {
            KeyDown.Add((Keys)vkCode);

            var keyModi = new KeyModifier();
            keyModi.IsShiftDown = KeyDown.Contains(Keys.LShiftKey) || KeyDown.Contains(Keys.RShiftKey);
            keyModi.IsCtrlDown = KeyDown.Contains(Keys.LControlKey) || KeyDown.Contains(Keys.RControlKey);

            OnKeyDown?.Invoke((Keys)vkCode, keyModi);
        }
        else if(nCode >= 0 && wParam == (IntPtr)WM_KEYUP && KeyDown.Contains((Keys)vkCode))
        {
            KeyDown.Remove((Keys)vkCode);
        }
        return User32.CallNextHookEx(_hookID, nCode, wParam, lParam);
    }
}

public struct KeyModifier
{
    public bool IsCtrlDown = false;
    public bool IsShiftDown = false;

    public KeyModifier()
    {
    }
}