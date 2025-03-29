using ChessEngine;
using ChessUI.Dialogs;
using ChessUI.Dialogs.Alert;
using ChessUI.Dialogs.Promotion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ChessUI
{
    /// <summary>
    /// Logika interakcji dla klasy ChessboardView.xaml
    /// </summary>
    public partial class ChessboardView : UserControl, INotifyPropertyChanged
    {
        private readonly Image[,] images = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];

        private Board board;
        private List<Move> moves;

        bool rotated = false;

        SolidColorBrush whiteColor = new SolidColorBrush(Color.FromRgb(121, 72, 57));
        SolidColorBrush blackColor = new SolidColorBrush(Color.FromRgb(93, 50, 49));
        SolidColorBrush highlightColor = new SolidColorBrush(Color.FromArgb(100, 219, 65, 48));
        SolidColorBrush lastMoveColor = new SolidColorBrush(Color.FromArgb(100, 245, 190, 39));

        private DialogService dialogService;

        public ChessboardView()
        {
            InitializeComponent();
            InitializeBoard();
            InitializeTimers();

            board = new Board();
            //board.LoadPositionFromFEN("8/k7/3p4/p2P1p2/P2P1P2/8/8/1K6 w - -");
            moves = board.GenerateMoves();

            DrawBoard();
            UpdateEvaluationBar();

            MoveHistory = new ObservableCollection<HistoryObject>();
            MoveHistory.CollectionChanged += ListView_ScrollToBottom;
            DataContext = this;

            dialogService = new DialogService();
            AppSettings.Instance.ZobristHash = board.GetZobristHash();
        }

        public void Alert(string title, string message)
        {
            var dialog = new AlertDialogViewModel(title, message);
            var result = dialogService.OpenDialog(dialog);
        }

        public void Alert(string message) => Alert("Koniec gry", message);

        public Move.Flags Promotion()
        {
            var dialog = new PromotionDialogViewModel("Promotion", board.colorToMove == Board.WhiteIndex ? "white" : "black");
            return dialogService.OpenDialog(dialog);
        }

        #region Rendering Board
        private void InitializeBoard()
        {
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

        public void UpdateEvaluationBar()
        {
            double eval = (double)board.Evaluate();
            double normalized = Math.Clamp((eval + 1000) / 1000, 0.05f, 1.95f);
            double height = EvaluationBar.ActualHeight;
            double newWhiteHeight = normalized;

            ScaleTransform whiteTransform = (ScaleTransform)WhiteAdvantage.RenderTransform;
            DoubleAnimation whiteAnim = new DoubleAnimation {
                To = newWhiteHeight,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
            };

            whiteTransform.BeginAnimation(ScaleTransform.ScaleYProperty, whiteAnim);
        }

        private void RefreshBoard()
        {
            DrawBoard();
            ClearHighlights();
            DrawLastMove();
            UpdateEvaluationBar();

            moves = board.GenerateMoves();

            if(state != GameState.InProgress) {
                timer.Stop();

                if(state == GameState.WhiteWon) Alert("Białe wygrywają - Mat");
                else if(state == GameState.BlackWon) Alert("Czarne wygrywają - Mat");
                else if(state == GameState.Stalemate) Alert("Remis - Pat");
                else if(state == GameState.DrawRepetition) Alert("Remis przez powtórzenia");
                else if(state == GameState.FiftyMovesRule) Alert("Remis - 50 ruchów bez postępu");
                else if(state == GameState.InsufficientMaterial) Alert("Remis - brak przewagi materiału");
            }
        }

        private void DrawBoard()
        {
            for (int row = 0; row < 8; row++) {
                for (int col = 0; col < 8; col++) {
                    images[col, row].Source = Images.sources[board[GridToBoard(row, col)]];
                }
            }
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
            DrawLastMove();
            for (int i = 0; i < moves.Count; i++) {
                if (moves[i].StartSquare == selectedSquare) {
                    ColorHighlight(moves[i].TargetSquare, highlightColor);
                }
            }

        }

        private void DrawLastMove()
        {
            Move? lastMove = board.LastMove();

            if (lastMove.HasValue) {
                ColorHighlight(lastMove.Value.StartSquare, lastMoveColor);
                ColorHighlight(lastMove.Value.TargetSquare, lastMoveColor);
            }
        }

        private void ColorHighlight(int square, SolidColorBrush brush)
        {
            int x = square % 8;
            int y = square / 8;

            if (rotated) {
                highlights[7 - x, y].Fill = brush;
            }
            else {
                highlights[x, 7 - y].Fill = brush;
            }
        }

        private int GridToBoard(int row, int col)
        {
            if (rotated) {
                return row * 8 + (7 - col);
            }
            return (7 - row) * 8 + col;
        }
        #endregion

        #region Buttons
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if(MoveHistory.Count > 0) {

                if (AppSettings.Instance.AIEnabled) {
                    if (MoveHistory[MoveHistory.Count - 1].MoveBlack == "") {
                        return;
                    }

                    MoveHistory.RemoveAt(MoveHistory.Count - 1);

                    positionHistory[board.GetZobristHash()]--;
                    board.UnmakeMove();
                    moveNumber--;

                    moveRule50 = Math.Clamp(moveRule50, 0, moveRule50 - 1);
                }
                else {

                    if (MoveHistory.Count > 0) {
                        if (MoveHistory[MoveHistory.Count - 1].MoveBlack == "") {
                            MoveHistory.RemoveAt(MoveHistory.Count - 1);
                            moveNumber--;
                        }
                        else {
                            MoveHistory[MoveHistory.Count - 1].MoveBlack = "";
                        }
                    }
                }
            }

            moveRule50 = Math.Clamp(moveRule50, 0, moveRule50 - 1);
            state = GameState.InProgress;
            positionHistory[board.GetZobristHash()]--;
            board.UnmakeMove();

            RefreshBoard();
            AppSettings.Instance.ZobristHash = board.GetZobristHash();
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            rotated = !rotated;
            ClearHighlights();
            DrawHighlights();
            DrawBoard();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow) {
                mainWindow.SwitchToModeSelect();
            }
        }
        #endregion

        #region Timers
        private int gameLengthSeconds = 5 * 60;
        private int timerWhite;
        private int timerBlack;
        private int turn = 0;
        DispatcherTimer timer;

        private void InitializeTimers()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            timerWhite = gameLengthSeconds;
            timerBlack = gameLengthSeconds;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if(turn == 0) {
                timerWhite--;
            }
            else {
                timerBlack--;
            }

            WhiteTimer.Content = GetTimerString(timerWhite);
            BlackTimer.Content = GetTimerString(timerBlack);
        }

        public string GetTimerString(int timer)
        {

            return (timer / 60).ToString() + ":" + (timer % 60 < 10 ? "0" : "") + (timer % 60).ToString();
        }
        #endregion

        public void SetMode(GameMode mode)
        {
            if(mode == GameMode.TwoPlayers) {
                AppSettings.Instance.AIEnabled = false;
            }
            else if(mode == GameMode.PlayerMinimax) {
                AppSettings.Instance.AIEnabled = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
