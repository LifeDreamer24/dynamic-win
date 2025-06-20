using NAudio.CoreAudioApi;

namespace DynamicWin.Interop;

internal static class AudioDevices
{
    private static readonly MMDevice? _outputDevice;
    private static readonly MMDevice? _inputDevice;
    
    static AudioDevices()
    {
        using var devEnum = new MMDeviceEnumerator();
        _outputDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        _inputDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
    }

    internal static MMDevice? GetOutputDeviceOrDefault() => _outputDevice;

    internal static MMDevice? GetInputDeviceOrDefault() => _inputDevice;
}