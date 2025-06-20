using System.Diagnostics;
using System.Windows;

namespace DynamicWin.Main;

public class ErrorForm : Window
{
    public ErrorForm()
    {
        MessageBox.Show("Only one instance of DynamicWin can run at a time.", "An error occured.");
        Process.GetCurrentProcess().Kill();
    }
}