using ChessEngine;
using ChessUI.Misc;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChessUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainContent.Content = new ModeSelectView();
    }

    public void SwitchToGame(GameMode mode, string fen = "")
    {
        ChessboardView chessboardView = new();
        chessboardView.SetMode(mode, fen);
        MainContent.Content = chessboardView;
    }

    public void SwitchToModeSelect()
    {
        MainContent.Content = new ModeSelectView();
    }

    private void Border_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) {
            DragMove();
        }
    }

    private void ButtonClose_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void ButtonMaximize_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow.WindowState == WindowState.Maximized) {
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        }
        else {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
        }
    }

    private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow.WindowState = WindowState.Minimized;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if(e.Key == Key.F2) {
            DebugWindow secondWindow = new DebugWindow();
            secondWindow.Show();
        }
    }
}