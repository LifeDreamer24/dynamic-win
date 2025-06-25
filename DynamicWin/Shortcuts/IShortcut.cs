using System.Windows.Forms;
using DynamicWin.Interop;

namespace DynamicWin.Shortcuts;

public interface IShortcut
{
    internal bool Intended(Keys key, KeyModifier modifier);
    
    internal void Execute();
}