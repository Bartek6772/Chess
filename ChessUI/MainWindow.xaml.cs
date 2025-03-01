using ChessEngine;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChessUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Image[,] images = new Image[8, 8];

    private Board board;

    public MainWindow()
    {
        InitializeComponent();
        InitializeBoard();

        board = new Board();

        DrawBoard();
    }

    private void InitializeBoard()
    {
        //SolidColorBrush whiteColor = new SolidColorBrush(Color.FromRgb(240, 217, 181));
        //SolidColorBrush blackColor = new SolidColorBrush(Color.FromRgb(181, 136, 99));
        SolidColorBrush whiteColor = new SolidColorBrush(Color.FromRgb(121, 72, 57));
        SolidColorBrush blackColor = new SolidColorBrush(Color.FromRgb(93, 50, 49));

        for (int row = 0; row < 8; row++) {
            for (int col = 0; col < 8; col++) {
                Rectangle rect = new Rectangle();
                rect.Fill = (row + col) % 2 != 0 ? whiteColor : blackColor;
                Chessboard.Children.Add(rect);

                Image image = new Image();
                images[col, row] = image;

                image.Width = 40;
                image.Width = 40;

                ChessPieces.Children.Add(image);
            }
        }
    }

    private void DrawBoard()
    {
        for (int row = 0; row < 8; row++) {
            for (int col = 0; col < 8; col++) {
                images[col, row].Source = Images.sources[board[row * 8 + col]];
                Debug.WriteLine(Images.sources[board[row * 8 + col]]);
            }
        }
    }

    #region Window Buttons
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
    #endregion


}