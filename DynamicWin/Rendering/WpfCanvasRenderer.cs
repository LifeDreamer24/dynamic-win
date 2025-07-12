using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace DynamicWin.Rendering;

[DefaultEvent("PaintSurface")]
[DefaultProperty("Name")]
public abstract class WpfCanvasRenderer : FrameworkElement, IDisposable
{
    private const double BitmapDpi = 96.0;
    private readonly Stopwatch _measureFrames = new();

    private readonly bool _designMode;
    private WriteableBitmap? _bitmap;

    protected WpfCanvasRenderer()
    {
        _designMode = DesignerProperties.GetIsInDesignMode(this);
        CompositionTarget.Rendering += OnFrameRendering;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (_designMode)
            return;

        if (Visibility != Visibility.Visible || PresentationSource.FromVisual(this) == null)
            return;
        
        var transformationMatrix = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice
                                   ?? Matrix.Identity;

        var scaleWidth = transformationMatrix.M11;
        var scaleHeight = transformationMatrix.M22;

        var size = new SKSizeI((int)(ActualWidth * scaleWidth), (int)(ActualHeight * scaleHeight));

        if (size.Width <= 0 || size.Height <= 0)
            return;

        var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

        // reset the bitmap if the size has changed
        if (_bitmap == null || info.Width != _bitmap.PixelWidth || info.Height != _bitmap.PixelHeight)
        {
            _bitmap = new WriteableBitmap(
                pixelWidth: info.Width, 
                pixelHeight: size.Height, 
                dpiX: BitmapDpi * scaleWidth, 
                dpiY: BitmapDpi * scaleHeight, 
                pixelFormat: PixelFormats.Pbgra32, 
                palette: null);
        }

        // draw on the bitmap
        _bitmap.Lock();
        using (var surface = SKSurface.Create(info, _bitmap.BackBuffer, _bitmap.BackBufferStride))
        {
            RenderCanvas(surface.Canvas);
        }

        // draw the bitmap to the screen
        _bitmap.AddDirtyRect(new Int32Rect(0, 0, info.Width, size.Height));
        _bitmap.Unlock();
        drawingContext.DrawImage(_bitmap, new Rect(0, 0, ActualWidth, ActualHeight));
    }
    
    protected abstract void RenderCanvas(SKCanvas canvas);

    protected abstract void RenderFrame(TimeSpan renderingTime);
    
    private void OnFrameRendering(object? sender, EventArgs eventArgs)
    {
        if (_designMode)
            return;
        
        var renderingTimeInSeconds = _measureFrames.IsRunning
            ? _measureFrames.Elapsed
            : TimeSpan.FromSeconds(1f / 60f);
        
        _measureFrames.Restart();
            
        RenderFrame(renderingTimeInSeconds);
        Dispatcher.Invoke(InvalidateVisual);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        InvalidateVisual();
    }

    public virtual void Dispose()
    {
        CompositionTarget.Rendering -= OnFrameRendering;
        _bitmap?.Freeze();
        _bitmap = null;
    }
}