using SkiaSharp;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace DynamicWin.WPFBinders;

[DefaultEvent("PaintSurface")]
[DefaultProperty("Name")]
public class SKElement : FrameworkElement
{
    private const double BitmapDpi = 96.0;

    private readonly bool _designMode;

    private WriteableBitmap? _bitmap;

    protected SKElement()
    {
        _designMode = DesignerProperties.GetIsInDesignMode(this);
    }

    public SKSize CanvasSize { get; private set; }

    public bool IgnorePixelScaling { get; }

    [Category("Appearance")]
    public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (_designMode)
            return;

        if (Visibility != Visibility.Visible || PresentationSource.FromVisual(this) == null)
            return;

        var size = CreateSize(out var unscaledSize, out var scaleX, out var scaleY);
        var userVisibleSize = IgnorePixelScaling ? unscaledSize : size;

        CanvasSize = userVisibleSize;

        if (size.Width <= 0 || size.Height <= 0)
            return;

        var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

        // reset the bitmap if the size has changed
        if (_bitmap == null || info.Width != _bitmap.PixelWidth || info.Height != _bitmap.PixelHeight)
        {
            _bitmap = new WriteableBitmap(info.Width, size.Height, BitmapDpi * scaleX, BitmapDpi * scaleY, PixelFormats.Pbgra32, null);
        }

        // draw on the bitmap
        _bitmap.Lock();
        using (var surface = SKSurface.Create(info, _bitmap.BackBuffer, _bitmap.BackBufferStride))
        {
            OnPaintSurface(new SKPaintSurfaceEventArgs(surface));
        }

        // draw the bitmap to the screen
        _bitmap.AddDirtyRect(new Int32Rect(0, 0, info.Width, size.Height));
        _bitmap.Unlock();
        drawingContext.DrawImage(_bitmap, new Rect(0, 0, ActualWidth, ActualHeight));
    }

    protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        // invoke the event
        PaintSurface?.Invoke(this, e);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        InvalidateVisual();
    }

    private SKSizeI CreateSize(out SKSizeI unscaledSize, out float scaleX, out float scaleY)
    {
        unscaledSize = SKSizeI.Empty;
        scaleX = 1.0f;
        scaleY = 1.0f;

        var w = ActualWidth;
        var h = ActualHeight;

        if (!IsPositive(w) || !IsPositive(h))
            return SKSizeI.Empty;

        unscaledSize = new SKSizeI((int)w, (int)h);

        var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
        scaleX = (float)m.M11;
        scaleY = (float)m.M22;
        return new SKSizeI((int)(w * scaleX), (int)(h * scaleY));

        bool IsPositive(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
        }
    }
}