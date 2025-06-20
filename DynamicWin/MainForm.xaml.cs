using DynamicWin.Resources;
using DynamicWin.UI.Menu;
using DynamicWin.UI.Menu.Menus;
using DynamicWin.Utils;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using DynamicWin.DllImports;
using DynamicWin.Interop;
using DynamicWin.WPFBinders;
using DataFormats = System.Windows.DataFormats;
using DataObject = System.Windows.DataObject;
using DragAction = System.Windows.DragAction;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;
using QueryContinueDragEventArgs = System.Windows.QueryContinueDragEventArgs;

namespace DynamicWin.Main;

public partial class MainForm : Window
{
    private readonly NotifyIcon _trayIcon;
    private readonly TimeSpan _targetElapsedTime = TimeSpan.FromMilliseconds(16); // ~60 FPS
    private DateTime _lastRenderTime;
    private readonly Theme _theme;
    private RendererMain _rendererMain; 
    
    private bool _isLocalDrag = false;
    
    public static MainForm Instance { get; private set; }

    public static Action<MouseWheelEventArgs>? OnScrollEvent { get; set; }

    public MainForm(
        string title,
        WindowStyle style,
        WindowState state,
        ResizeMode resizeMode,
        bool topmost,
        bool allowsTransparency,
        bool showInTaskbar,
        Theme theme,
        NotifyIcon trayIcon)
    {
        InitializeComponent();

        //CompositionTarget.Rendering += OnRendering;

        Instance = this;
        _trayIcon = trayIcon;
        _theme = theme;
        
        WindowStyle = style;
        WindowState = state;
        ResizeMode = resizeMode;
        Topmost = topmost;
        AllowsTransparency = allowsTransparency;
        ShowInTaskbar = showInTaskbar;
        Title = title;
        
        Loaded += (_, _) => ExtendedWindowStyles.ChangeToToolWindow(this);

        SetMonitor(Settings.ScreenIndex);

        AddRenderer();

        FileResources.extensions.ForEach(x => x.LoadExtension());
        Instance.AllowDrop = true;

        _trayIcon.ContextMenuStrip = new ContextMenuStrip();
        _trayIcon.ContextMenuStrip.Items.Add("Restart Control", null, (x, y) =>
        {
            if (RendererMain.Instance != null) RendererMain.Instance.Destroy();
            Content = new Grid();

            AddRenderer();
        });

        _trayIcon.ContextMenuStrip.Items.Add("Settings", null, (x, y) =>
        {
            MenuManager.OpenMenu(new SettingsMenu());
        });
        
        _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (x, y) =>
        {
            SaveManager.SaveAll();
            Process.GetCurrentProcess().Kill();
        });

        _trayIcon.Visible = true;
    }


    public void SetMonitor(int monitorIndex)
    {
        var screen = Screen.AllScreens[Math.Clamp(monitorIndex, 0, GetMonitorCount() - 1)];
        Settings.ScreenIndex = Math.Clamp(monitorIndex, 0, GetMonitorCount() - 1);
        
        if (!IsLoaded)
            WindowStartupLocation = WindowStartupLocation.Manual;

        WindowState = WindowState.Normal;
        ResizeMode = ResizeMode.CanResize;

        var workingArea = screen.WorkingArea;

        Left = workingArea.Left;
        Top = workingArea.Top;
        Width = workingArea.Width;
        Height = workingArea.Height;

        ResizeMode = ResizeMode.NoResize;
    }

    public static int GetMonitorCount()
    {
        return Screen.AllScreens.Length;
    }

    private void OnRendering(object? sender, SKPaintSurfaceEventArgs e)
    {
        var currentTime = DateTime.Now;
        if (currentTime - _lastRenderTime >= _targetElapsedTime)
        {
            _lastRenderTime = currentTime;
        }
    }

    public bool isDragging { get; set; } = false;

    public void OnScroll(object? sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        OnScrollEvent?.Invoke(e);
    }

    public void AddRenderer()
    {
        if (RendererMain.Instance != null) RendererMain.Instance.Destroy();

        _rendererMain = new RendererMain();
        _rendererMain.PaintSurface += OnRendering;
        
        var parent = new Grid();
        parent.Children.Add(_rendererMain);

        Content = parent;
    }

    public void MainForm_DragEnter(object? sender, DragEventArgs e)
    {
        isDragging = true;
        e.Effects = DragDropEffects.Copy;

        if (MenuManager.Instance.ActiveMenu is not DropFileMenu
            && MenuManager.Instance.ActiveMenu is not ConfigureShortcutMenu)
        {
            MenuManager.OpenMenu(new DropFileMenu());
        }
    }

    public void MainForm_DragLeave(object? sender, EventArgs e)
    {
        isDragging = false;

        if (MenuManager.Instance.ActiveMenu is ConfigureShortcutMenu) return;
        MenuManager.OpenMenu(FileResources.HomeMenu);
    }

    internal void StartDrag(string[] files, Action callback)
    {
        if (_isLocalDrag) return;

        Array.ForEach(files, file => { Debug.WriteLine(file); });

        if (files == null) return;
        else if (files.Length <= 0) return;

        try
        {
            _isLocalDrag = true;

            DataObject dataObject = new DataObject(DataFormats.FileDrop, files);
            var effects = DragDrop.DoDragDrop((DependencyObject)this, dataObject, DragDropEffects.Move | DragDropEffects.Copy);

            if (RendererMain.Instance != null) RendererMain.Instance.Destroy();
            Content = new Grid();
            AddRenderer();

            callback?.Invoke();

            _isLocalDrag = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show("An error occurred: " + ex.Message);
        }
    }

    protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
    {
        if (e.Action == DragAction.Cancel)
        {
            _isLocalDrag = false;
        }
        else if (e.Action == DragAction.Continue)
        {
            _isLocalDrag = true;
        }
        else if (e.Action == DragAction.Drop)
        {
            _isLocalDrag = false;
        }
    }

    protected override void OnDragOver(DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        base.OnDragOver(e);
    }

    public void OnDrop(object sender, DragEventArgs e)
    {
        isDragging = false;

        if(MenuManager.Instance.ActiveMenu is ConfigureShortcutMenu)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                ConfigureShortcutMenu.DropData(e);
            }
        }
        else if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            DropFileMenu.Drop(e);
            MenuManager.Instance.QueueOpenMenu(DynamicWin.Resources.FileResources.HomeMenu);
            DynamicWin.Resources.FileResources.HomeMenu.isWidgetMode = false;
        }
    }

    internal void DisposeTrayIcon()
    {
        _trayIcon.Dispose();
    }
}