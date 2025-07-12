using System;
using System.Runtime.InteropServices;

namespace DynamicWin.Interop;

public class MediaController
{
    private const ushort VK_MEDIA_PLAY_PAUSE = 0xB3;
    private const ushort VK_MEDIA_NEXT_TRACK = 0xB0;
    private const ushort VK_MEDIA_PREV_TRACK = 0xB1;
    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public uint type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct InputUnion
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
    private static extern void Keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private void SimulateMediaKey(ushort keyCode)
    {
        try
        {
            INPUT[] inputs = new INPUT[]
            {
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = keyCode,
                            dwFlags = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                },
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = keyCode,
                            dwFlags = KEYEVENTF_KEYUP,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                }
            };

            var result = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            if (result == 0)
                throw new Exception("SendInput failed");
        }
        catch
        {
            // Fallback to keybd_event
            Keybd_event((byte)keyCode, 0, 0, UIntPtr.Zero);
            Keybd_event((byte)keyCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }

    public void PlayPause() => SimulateMediaKey(VK_MEDIA_PLAY_PAUSE);
    public void Next() => SimulateMediaKey(VK_MEDIA_NEXT_TRACK);
    public void Previous() => SimulateMediaKey(VK_MEDIA_PREV_TRACK);
}
