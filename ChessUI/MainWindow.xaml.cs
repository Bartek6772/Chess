using ChessEngine;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ChessUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Image[,] images = new Image[8, 8];
    private readonly Rectangle[,] highlights = new Rectangle[8, 8];

    private Board board;
    private MoveGeneration moveGen;
    private List<Move> moves;

    public MainWindow()
    {
        InitializeComponent();
        InitializeBoard();

        board = new Board();
        moveGen = new MoveGeneration(board);
        moves = moveGen.GenerateMoves();

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

                // Board
                Rectangle rect = new Rectangle();
                rect.Fill = (row + col) % 2 == 0 ? whiteColor : blackColor;
                Background.Children.Add(rect);

                // Highlights
                Rectangle highlight = new Rectangle();
                highlights[col, row] = highlight;
                HighlightGrid.Children.Add(highlight);

                // Pieces
                Image image = new Image();
                images[col, row] = image;
                image.Height = 44;
                image.RenderTransform = new TranslateTransform();
                ChessPieces.Children.Add(image);
            }
        }
        Chessboard.MouseLeftButtonDown += Board_MouseLeftButtonDown;
        Chessboard.MouseMove += Chessboard_MouseMove;
        Chessboard.MouseLeftButtonUp += Chessboard_MouseLeftButtonUp;
    }

    private void DrawBoard()
    {
        for (int row = 0; row < 8; row++) {
            for (int col = 0; col < 8; col++) {
                images[col, row].Source = Images.sources[board[(7 - row) * 8 + col]];
            }
        }
    }

    private int GridToBoard(int row, int col)
    {
        return (7 - row) * 8 + col;
    }

    private void ClearHighlights()
    {
        for (int r = 0; r < 8; r++) {
            for (int c = 0; c < 8; c++) {
                highlights[c, r].Fill = Brushes.Transparent;
            }
        }
    }

    private void DrawHighlights()
    {
        ClearHighlights();

        for (int i = 0; i < moves.Count; i++) {
            if (moves[i].StartSquare == selectedSquare) {

                int x = moves[i].TargetSquare % 8;
                int y = moves[i].TargetSquare / 8;

                highlights[x, 7 - y].Fill = new SolidColorBrush(Color.FromRgb(252, 186, 3));
            }
        }
    }

    private int selectedSquare = -1;
    private void SelectSquare(int row, int col)
    {
        if (selectedSquare != -1) {

            for (int i = 0; i < moves.Count; i++) {
                int target = GridToBoard(row, col);
                if (moves[i].StartSquare == selectedSquare && moves[i].TargetSquare == target) {

                    board.MakeMove(moves[i]);
                    moves = moveGen.GenerateMoves();
                    ClearHighlights();
                    DrawBoard();
                    selectedSquare = -1;
                    return;
                }
            }
        }

        selectedSquare = GridToBoard(row, col);
        DrawHighlights();
    }

    #region Handling Moves
    private Image draggedPiece = null;  // Original chess piece
    private Image dragOverlay = null;   // Floating piece
    private Point mouseOffset;
    private Point mouseDownPosition;    // Tracks the click position
    private bool isDragging = false;    // Flag to check if dragging is in progress
    private const double DragThreshold = 5; // Minimum movement to start drag


    private void Board_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        mouseDownPosition = e.GetPosition(Chessboard);
        int row = (int)(mouseDownPosition.Y / 64);
        int col = (int)(mouseDownPosition.X / 64);
        SelectSquare(row, col);

        draggedPiece = images[col, row];

        if (draggedPiece != null) {
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

            DragLayer.Children.Remove(dragOverlay);
            dragOverlay = null;

            SelectSquare(row, column);
        }
        else {
            //MessageBox.Show($"Clicked on ({column}, {row})");
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