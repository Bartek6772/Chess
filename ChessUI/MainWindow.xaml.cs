using ChessEngine;
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
        
    }

    //private void AnimatePieceMovement(Image piece, int newRow, int newColumn)
    //{
    //    // Get piece index in UniformGrid
    //    int oldIndex = ChessPieces.Children.IndexOf(piece);
    //    int oldRow = oldIndex / 8;
    //    int oldColumn = oldIndex % 8;

    //    // Calculate movement offsets
    //    double deltaX = (newColumn - oldColumn) * 64;
    //    double deltaY = (newRow - oldRow) * 64;

    //    // Ensure the piece has a transform
    //    TranslateTransform transform = piece.RenderTransform as TranslateTransform;
    //    if (transform == null) {
    //        transform = new TranslateTransform();
    //        piece.RenderTransform = transform;
    //    }

    //    // Animate X movement
    //    DoubleAnimation animX = new DoubleAnimation {
    //        By = deltaX,
    //        Duration = TimeSpan.FromMilliseconds(200),
    //        EasingFunction = new QuadraticEase()
    //    };

    //    // Animate Y movement
    //    DoubleAnimation animY = new DoubleAnimation {
    //        By = deltaY,
    //        Duration = TimeSpan.FromMilliseconds(200),
    //        EasingFunction = new QuadraticEase()
    //    };

    //    // Start animation
    //    transform.BeginAnimation(TranslateTransform.XProperty, animX);
    //    transform.BeginAnimation(TranslateTransform.YProperty, animY);

    //    // Update piece position after animation completes
    //    animY.Completed += (s, e) =>
    //    {
    //        int newIndex = newRow * 8 + newColumn;
    //        ChessPieces.Children.Remove(piece);
    //        ChessPieces.Children.Insert(newIndex, piece);

    //        // Reset translation to avoid stacking movements
    //        transform.X = 0;
    //        transform.Y = 0;
    //    };
    //}

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