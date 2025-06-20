using SkiaSharp;

namespace DynamicWin.WPFBinders;

public class SKPaintSurfaceEventArgs(SKSurface surface) : EventArgs
{
    public SKSurface Surface { get; } = surface;
}