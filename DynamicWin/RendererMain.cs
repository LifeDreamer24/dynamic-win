using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using DynamicWin.DllImports;
using DynamicWin.Interop.DllImports;
using DynamicWin.Resources;
using DynamicWin.UI;
using DynamicWin.UI.Menu;
using DynamicWin.UI.Menu.Menus;
using DynamicWin.UI.UIElements;
using DynamicWin.Utils;
using DynamicWin.WPFBinders;
using SkiaSharp;

namespace DynamicWin.Main;

public class RendererMain : SKElement
{
    private IslandObject islandObject;
    public IslandObject MainIsland => islandObject;
    private List<UIObject> objects => MenuManager.Instance.ActiveMenu.UiObjects;

    public static Vec2 ScreenDimensions => new Vec2(MainForm.Instance.Width, MainForm.Instance.Height);
    public static Vec2 CursorPosition => new Vec2(Mouse.GetPosition(MainForm.Instance).X, Mouse.GetPosition(MainForm.Instance).Y);

    private static RendererMain instance;
    public static RendererMain Instance => instance;

    public Vec2 renderOffset = Vec2.zero;
    public Vec2 scaleOffset = Vec2.one;
    public float blurOverride { get; set; } = 0f;
    public float alphaOverride { get; set; } = 1f;

    public Action<float> onUpdate { get; set; }
    public Action<SKCanvas> onDraw { get; set; }

    private Stopwatch? updateStopwatch;
    private int initialScreenBrightness = 0;
    private float deltaTime = 0f;
    public float DeltaTime => deltaTime;

    private bool isInitialized = false;
    public int canvasWithoutClip;
    private GRContext Context;

    public RendererMain()
    {
        var menuManager = new MenuManager();
        instance = this;
        islandObject = new IslandObject();
        menuManager.Init();
        
        CompositionTarget.Rendering += OnRendering;
        
        KeyboardListener.onKeyDown += OnKeyRegistered;

        MainForm.Instance.DragEnter += MainForm.Instance.MainForm_DragEnter;
        MainForm.Instance.DragLeave += MainForm.Instance.MainForm_DragLeave;
        MainForm.Instance.Drop += MainForm.Instance.OnDrop;
        MainForm.Instance.MouseWheel += MainForm.Instance.OnScroll;

        initialScreenBrightness = BrightnessAdjustMenu.GetBrightness();

        // Get refresh rate
        int refreshRate = GetRefreshRate();
        Debug.WriteLine($"Monitor Refresh Rate: {refreshRate} Hz");

        isInitialized = true;
    }

    public void Destroy()
    {
        CompositionTarget.Rendering -= OnRendering;
        
        KeyboardListener.onKeyDown -= OnKeyRegistered;
        
        MainForm.Instance.DragEnter -= MainForm.Instance.MainForm_DragEnter;
        MainForm.Instance.DragLeave -= MainForm.Instance.MainForm_DragLeave;
        MainForm.Instance.MouseWheel -= MainForm.Instance.OnScroll;

        instance = null;
    }

    private void OnRendering(object sender, EventArgs e)
    {
        Update();
        Dispatcher.Invoke(InvalidateVisual);
    }

    private void OnKeyRegistered(Keys key, KeyModifier modifier)
    {
        if (key == Keys.LWin && modifier.isCtrlDown)
        {
            islandObject.hidden = !islandObject.hidden;
        }

        if ((key == Keys.VolumeDown || key == Keys.VolumeMute || key == Keys.VolumeUp) && PopupOptions.saveData.volumePopup)
        {
            if (MenuManager.Instance.ActiveMenu is HomeMenu)
            {
                MenuManager.OpenOverlayMenu(new VolumeAdjustMenu(), 100f);
            }
            else if (VolumeAdjustMenu.timerUntilClose != null)
            {
                VolumeAdjustMenu.timerUntilClose = 0f;
            }
        }

        if (key == Keys.MediaNextTrack || key == Keys.MediaPreviousTrack)
        {
            if (MenuManager.Instance.ActiveMenu is HomeMenu)
            {
                if (key == Keys.MediaNextTrack) DynamicWin.Resources.FileResources.HomeMenu.NextSong();
                else DynamicWin.Resources.FileResources.HomeMenu.PrevSong();
            }
        }
    }

    private void Update()
    {
        var fixedDeltaTime = 1f / 60f;
        
        if (updateStopwatch != null)
        {
            updateStopwatch.Stop();
            deltaTime = updateStopwatch.ElapsedMilliseconds / 1000f;
        }
        else
        {
            deltaTime = 1f / 1000f;
        }

        deltaTime = fixedDeltaTime;
        updateStopwatch = Stopwatch.StartNew();

        onUpdate?.Invoke(DeltaTime);

        if (BrightnessAdjustMenu.GetBrightness() != initialScreenBrightness && PopupOptions.saveData.brightnessPopup)
        {
            initialScreenBrightness = BrightnessAdjustMenu.GetBrightness();
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

        MenuManager.Instance.Update(deltaTime);

        if (MenuManager.Instance.ActiveMenu != null)
        {
            MenuManager.Instance.ActiveMenu.Update();

            if (MenuManager.Instance.ActiveMenu is DropFileMenu && !MainForm.Instance.isDragging)
                MenuManager.OpenMenu(DynamicWin.Resources.FileResources.HomeMenu);
        }

        islandObject.UpdateCall(DeltaTime);

        if (MainIsland.hidden) return;

        foreach (UIObject uiObject in objects)
        {
            uiObject.UpdateCall(DeltaTime);
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        if (!isInitialized) return;

        SKSurface surface = e.Surface;
        SKCanvas canvas = surface.Canvas;

        canvas.Clear(SKColors.Transparent);

        var dpiFactor = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
        canvas.Scale((float)dpiFactor, (float)dpiFactor);

        canvasWithoutClip = canvas.Save();

        if (islandObject.maskInToIsland) Mask(canvas);
        islandObject.DrawCall(canvas);

        if (MainIsland.hidden) return;

        bool hasContextMenu = false;
        foreach (UIObject uiObject in objects)
        {
            canvas.RestoreToCount(canvasWithoutClip);
            canvasWithoutClip = canvas.Save();

            if (uiObject.IsHovering && uiObject.GetContextMenu() != null)
            {
                hasContextMenu = true;
                ContextMenu = uiObject.GetContextMenu();
            }

            foreach (UIObject obj in uiObject.LocalObjects)
            {
                if (obj.IsHovering && obj.GetContextMenu() != null)
                {
                    hasContextMenu = true;
                    ContextMenu = obj.GetContextMenu();
                }
            }

            if (uiObject.maskInToIsland)
            {
                Mask(canvas);
            }

            canvas.Scale(scaleOffset.X, scaleOffset.Y, islandObject.Position.X + islandObject.Size.X / 2, islandObject.Position.Y + islandObject.Size.Y / 2);
            canvas.Translate(renderOffset.X, renderOffset.Y);

            uiObject.DrawCall(canvas);
        }

        onDraw?.Invoke(canvas);

        if (!hasContextMenu) ContextMenu = null;

        canvas.Flush();
    }

    private void Mask(SKCanvas canvas)
    {
        var islandMask = GetMask();
        canvas.ClipRoundRect(islandMask);
    }

    public SKRoundRect GetMask()
    {
        var islandMask = islandObject.GetRect();
        islandMask.Deflate(new SKSize(1, 1));
        return islandMask;
    }
        
    private int GetRefreshRate()
    {
        const int ENUM_CURRENT_SETTINGS = -1;
            
        var devMode = new DevMode();
        devMode.dmSize = (ushort)Marshal.SizeOf(typeof(DevMode));
        if (User32.EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode))
            return (int)devMode.dmDisplayFrequency;
        return 60;
    }
}