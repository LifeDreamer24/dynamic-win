namespace DynamicWin.UI.Widgets;

public interface IDynamicWinExtension
{
    public string AuthorName { get; }
    public string ExtensionName { get; }
    public string ExtensionID { get; }
    public void LoadExtension();
    public List<IRegisterableWidget> GetExtensionWidgets();
}