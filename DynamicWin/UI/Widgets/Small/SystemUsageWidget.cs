using DynamicWin.Interop;
using DynamicWin.Rendering.Primitives;
using DynamicWin.UI.UIElements;
using DynamicWin.UserSettings;
using LibreHardwareMonitor.Hardware;

namespace DynamicWin.UI.Widgets.Small;

class RegisterSystemUsageWidget : IRegisterableWidget
{
    public bool IsSmallWidget => true;

    public string WidgetName => "System Usage Display";

    public WidgetBase CreateWidgetInstance(UIObject? parent, Vec2 position, UIAlignment alignment = UIAlignment.TopCenter)
    {
        return new SystemUsageWidget(parent, position, alignment);
    }
}

public class SystemUsageWidget : SmallWidgetBase
{
    DWText text;

    Computer computer;

    public SystemUsageWidget(UIObject? parent, Vec2 position, UIAlignment alignment = UIAlignment.TopCenter) : base(parent, position, alignment)
    {
        text = new DWText(this, "...", Vec2.zero, UIAlignment.Center);
        text.TextSize = 12;
        text.Color = Theme.TextSecond;
        AddLocalObject(text);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        updateCycle += deltaTime;

        if (updateCycle > 1.5f)
        {
            string usage = GetUsage();
            if (string.IsNullOrWhiteSpace(usage))
                usage = "Polling hardware...";
            text.SilentSetText(usage);
            updateCycle = 0f;
        }
    }

    float updateCycle = 0f;

    string GetUsage()
    {
        return HardwareMonitor.usageString;
    }

    protected override float GetWidgetWidth()
    {
        try
        {
            return Math.Max(225f, text?.TextBounds.X ?? 10f);
        }
        catch
        {
            return 225f;
        }
    }
}