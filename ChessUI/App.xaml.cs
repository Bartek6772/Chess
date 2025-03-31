using System.Configuration;
using System.Data;
using System.Windows;

namespace ChessUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Remove any checks preventing multiple instances
        base.OnStartup(e);
    }
}

