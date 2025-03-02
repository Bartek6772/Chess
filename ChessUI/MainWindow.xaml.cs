using ChessEngine;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

                image.Height = 44;
                image.RenderTransform = new TranslateTransform();

                image.MouseLeftButtonDown += Piece_MouseLeftButtonDown;

                ChessPieces.Children.Add(image);
            }
        }
        Chessboard.MouseMove += Chessboard_MouseMove;
        Chessboard.MouseLeftButtonUp += Chessboard_MouseLeftButtonUp;
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

    #region Handling Moves
    private Image draggedPiece = null;  // Original chess piece
    private Image dragOverlay = null;   // Floating piece
    private Point mouseOffset;
    private Point mouseDownPosition;    // Tracks the click position
    private bool isDragging = false;    // Flag to check if dragging is in progress
    private const double DragThreshold = 5; // Minimum movement to start drag

    private void Piece_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        draggedPiece = sender as Image;

        if (draggedPiece != null) {

            mouseDownPosition = e.GetPosition(Chessboard);
            mouseOffset = e.GetPosition(draggedPiece);
            isDragging = false;

            Chessboard.CaptureMouse();
        }
    }

    private void StartDragging()
    {

        if (draggedPiece == null) return;

        isDragging = true;
        mouseOffset = new Point(draggedPiece.ActualWidth / 2, draggedPiece.ActualHeight / 2);

        dragOverlay = new Image {
            Source = draggedPiece.Source,
            Width = draggedPiece.Width,
            Height = draggedPiece.Height,
            Opacity = 0.7,
            IsHitTestVisible = false
        };

        DragLayer.Children.Add(dragOverlay);
        UpdateOverlayPosition(mouseDownPosition);
        draggedPiece.Visibility = Visibility.Hidden;
    }

    private void Chessboard_MouseMove(object sender, MouseEventArgs e)
    {
        if (draggedPiece == null || e.LeftButton != MouseButtonState.Pressed) return;

        Point currentPosition = e.GetPosition(Chessboard);
        double distance = Math.Abs(currentPosition.X - mouseDownPosition.X) +
                          Math.Abs(currentPosition.Y - mouseDownPosition.Y);

        if (!isDragging && distance > DragThreshold) {
            StartDragging();
        }

        if (isDragging) {
            UpdateOverlayPosition(e.GetPosition(DragLayer));
        }
    }

    private void Chessboard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (draggedPiece == null) return;

        Point dropPosition = e.GetPosition(Chessboard);
        int row = (int)(dropPosition.Y / 64);
        int column = (int)(dropPosition.X / 64);

        if (isDragging) {
            draggedPiece.Visibility = Visibility.Visible;
            Debug.WriteLine($"Dropped on ({column}, {row})");

            DragLayer.Children.Remove(dragOverlay);
            dragOverlay = null;
        }
        else {
            MessageBox.Show($"Clicked on ({column}, {row})");
        }

        draggedPiece = null;
        isDragging = false;
        Chessboard.ReleaseMouseCapture();
    }

    private void UpdateOverlayPosition(Point mousePosition)
    {
        if (dragOverlay != null) {
            Canvas.SetLeft(dragOverlay, mousePosition.X - mouseOffset.X);
            Canvas.SetTop(dragOverlay, mousePosition.Y - mouseOffset.Y);
        }
    }
    #endregion

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