using System.Windows;
using System.Windows.Input;
using DynamicWin.Rendering.Primitives;
using DynamicWin.Resources;
using DynamicWin.UI;
using DynamicWin.UI.Menu;
using DynamicWin.UI.Menu.Menus;
using DynamicWin.UI.UIElements;
using DynamicWin.UserSettings;
using SkiaSharp;

namespace DynamicWin.Rendering;

public class DynamicWinRenderer : WpfCanvasRenderer
{
    private readonly bool _isInitialized;
    
    private int _initialScreenBrightness;
    private int _canvasWithoutClip;

    public static Vec2 ScreenDimensions => new(MainForm.Instance.Width, MainForm.Instance.Height);
    public static Vec2 CursorPosition => new(Mouse.GetPosition(MainForm.Instance).X, Mouse.GetPosition(MainForm.Instance).Y);
    public static DynamicWinRenderer? Instance { get; private set; }
    
    public IslandObject MainIsland { get; }
    public Vec2 ScaleOffset { get; set; } = Vec2.one;
    public float BlurOverride { get; set; } 
    public float AlphaOverride { get; set; } = 1f;
    public float DeltaTime { get; private set; }

    public DynamicWinRenderer()
    {
        var menuManager = new MenuManager();
        Instance = this;
        MainIsland = new IslandObject();
        MainIsland.NeedsRendering += NeedsRendering;
        menuManager.Init();

        _initialScreenBrightness = BrightnessAdjustMenu.GetBrightness();
        _isInitialized = true;
    }
    
    private void NeedsRendering(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(InvalidateVisual);
    }
    
    protected override void RenderCanvas(SKCanvas canvas)
    {
        if (!_isInitialized) return;

        canvas.Clear(SKColors.Transparent);
        
        var dpiFactor = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
        canvas.Scale((float)dpiFactor, (float)dpiFactor);

        _canvasWithoutClip = canvas.Save();

        // if (true) 
        //     Mask(canvas);
        
        MainIsland.DrawCall(canvas);

        if (MainIsland.Hidden) return;

        bool hasContextMenu = false;
        foreach (var menuChild in MenuManager.Instance.ActiveMenu.Children)
        {
            canvas.RestoreToCount(_canvasWithoutClip);
            _canvasWithoutClip = canvas.Save();

            if (menuChild.IsHovering && menuChild.GetContextMenu() != null)
            {
                hasContextMenu = true;
                ContextMenu = menuChild.GetContextMenu();
            }

            foreach (UIObject obj in menuChild.LocalObjects)
            {
                if (obj.IsHovering && obj.GetContextMenu() != null)
                {
                    hasContextMenu = true;
                    ContextMenu = obj.GetContextMenu();
                }
            }

            if (menuChild.maskInToIsland)
            {
                MainIsland.Mask(canvas);
            }

            var renderOffset = Vec2.zero;
            
            canvas.Scale(ScaleOffset.X, ScaleOffset.Y, MainIsland.Position.X + MainIsland.Size.X / 2, MainIsland.Position.Y + MainIsland.Size.Y / 2);
            canvas.Translate(renderOffset.X, renderOffset.Y);

            menuChild.DrawCall(canvas);
        }

        if (!hasContextMenu) ContextMenu = null;

        canvas.Flush();
    }

    protected override void RenderFrame(TimeSpan renderingTime)
    {
        DeltaTime = renderingTime.Milliseconds / 1000f;
        
        if (BrightnessAdjustMenu.GetBrightness() != _initialScreenBrightness && PopupOptions.SaveData.brightnessPopup)
        {
            _initialScreenBrightness = BrightnessAdjustMenu.GetBrightness();
            if (MenuManager.Instance.ActiveMenu is HomeMenu)
            {
                MenuManager.OpenOverlayMenu(new BrightnessAdjustMenu(), 100f);
            }
            else if (BrightnessAdjustMenu.timerUntilClose != null)
            {
                BrightnessAdjustMenu.PressBK();
                BrightnessAdjustMenu.timerUntilClose = 0f;
            }
        }

        MenuManager.Instance.Update(DeltaTime);

        if (MenuManager.Instance.ActiveMenu != null)
        {
            MenuManager.Instance.ActiveMenu.Update();

            if (MenuManager.Instance.ActiveMenu is DropFileMenu && !MainForm.Instance.IsDragging)
                MenuManager.OpenMenu(FileResources.HomeMenu);
        }

        MainIsland.UpdateCall(DeltaTime);

        if (MainIsland.Hidden) return;

        foreach (var menuChild in MenuManager.Instance.ActiveMenu.Children)
        {
            menuChild.UpdateCall(DeltaTime);
        }
    }
}