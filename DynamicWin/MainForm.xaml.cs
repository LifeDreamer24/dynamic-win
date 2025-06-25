using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using DynamicWin.Interop;
using DynamicWin.Rendering;
using DynamicWin.Resources;
using DynamicWin.Shortcuts;
using DynamicWin.UI.Menu;
using DynamicWin.UI.Menu.Menus;
using DynamicWin.UserSettings;
using DataFormats = System.Windows.DataFormats;
using DataObject = System.Windows.DataObject;
using DragAction = System.Windows.DragAction;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;
using QueryContinueDragEventArgs = System.Windows.QueryContinueDragEventArgs;

namespace DynamicWin;

public partial class MainForm
{
    private readonly NotifyIcon _trayIcon;
    private readonly Theme _theme;
    private DynamicWinRenderer _dynamicWinRenderer; 
    private readonly List<IShortcut> _shortcuts;
    
    private bool _isLocalDrag;
    
    public bool IsDragging { get; private set; }
    
    public static MainForm? Instance { get; private set; }

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
        NotifyIcon trayIcon,
        DynamicWinRenderer renderer,
        IEnumerable<IShortcut> shortcuts)
    {
        InitializeComponent();
        Instance = this;
        
        _trayIcon = trayIcon;
        _theme = theme;
        _shortcuts = shortcuts.ToList();
        _dynamicWinRenderer = renderer;
        
        var parent = new Grid();
        parent.Children.Add(_dynamicWinRenderer);

        Content = parent;
        
        WindowStyle = style;
        WindowState = state;
        ResizeMode = resizeMode;
        Topmost = topmost;
        AllowsTransparency = allowsTransparency;
        ShowInTaskbar = showInTaskbar;
        Title = title;
        
        Loaded += (_, _) => ExtendedWindowStyles.ChangeToToolWindow(this);
        
        KeyboardListener.OnKeyDown += (key, modifier) =>
        {
            _shortcuts.FirstOrDefault(shortcut => shortcut.Intended(key, modifier))?.Execute();
        };

        SetMonitor(Settings.ScreenIndex);

        FileResources.extensions.ForEach(x => x.LoadExtension());
        Instance.AllowDrop = true;

        _trayIcon.ContextMenuStrip = new ContextMenuStrip();
        _trayIcon.ContextMenuStrip.Items.Add("Restart Control", null, (x, y) =>
        {
            if (DynamicWinRenderer.Instance != null) DynamicWinRenderer.Instance.Dispose();
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
        
        DragEnter += MainForm_DragEnter;
        DragLeave += MainForm_DragLeave;
        Drop += OnDrop;
        MouseWheel += OnScroll;
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

    private void OnScroll(object? sender, MouseWheelEventArgs e)
    {
        OnScrollEvent?.Invoke(e);
    }

    public void AddRenderer()
    {
        if (DynamicWinRenderer.Instance != null) DynamicWinRenderer.Instance.Dispose();

        _dynamicWinRenderer = new DynamicWinRenderer();
        
        var parent = new Grid();
        parent.Children.Add(_dynamicWinRenderer);

        Content = parent;
    }

    private void MainForm_DragEnter(object? sender, DragEventArgs e)
    {
        IsDragging = true;
        e.Effects = DragDropEffects.Copy;

        if (MenuManager.Instance.ActiveMenu is not DropFileMenu
            && MenuManager.Instance.ActiveMenu is not ConfigureShortcutMenu)
        {
            MenuManager.OpenMenu(new DropFileMenu());
        }
    }

    private void MainForm_DragLeave(object? sender, EventArgs e)
    {
        IsDragging = false;

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

            if (DynamicWinRenderer.Instance != null) DynamicWinRenderer.Instance.Dispose();
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
        IsDragging = false;

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
            MenuManager.Instance.QueueOpenMenu(FileResources.HomeMenu);
            FileResources.HomeMenu.isWidgetMode = false;
        }
        else if (e.Data.GetDataPresent(DataFormats.Text))
        {
            var imageUrl = e.Data.GetData(DataFormats.Text) as string;
            if (string.IsNullOrEmpty(imageUrl))
            {
                MessageBox.Show("Invalid image URL.");
                return;
            }
            
            using var client = new HttpClient();
            
            var file = client.GetByteArrayAsync(imageUrl).GetAwaiter().GetResult();
            var fileName = imageUrl.Split('/').Last();
            
            var fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures), fileName);
            
            File.WriteAllBytes(fullPath, file);
        }
    }

    internal void DisposeTrayIcon()
    {
        _trayIcon.Dispose();
    }
}