using ChessEngine;
using ChessUI.Dialogs;
using ChessUI.Dialogs.Alert;
using ChessUI.Dialogs.Network;
using ChessUI.Dialogs.Promotion;
using ChessUI.Misc;
using ChessUI.Networking;
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
        private NetworkBase network;

        bool rotated = false;
        bool isMultiplayer = false;

        //SolidColorBrush whiteColor = new SolidColorBrush(Color.FromRgb(121, 72, 57));
        //SolidColorBrush blackColor = new SolidColorBrush(Color.FromRgb(93, 50, 49));
        //SolidColorBrush highlightColor = new SolidColorBrush(Color.FromArgb(100, 219, 65, 48));
        //SolidColorBrush lastMoveColor = new SolidColorBrush(Color.FromArgb(100, 245, 190, 39));

        private ColorSet set;
        private ImagesSet imageSet = ImagesSet.Normal;
        int setIndex = 0;

        private DialogService dialogService;

        public ChessboardView()
        {
            setIndex = AppSettings.Instance.savedColorIndex;
            imageSet = AppSettings.Instance.savedImages;
            set = ColorSet.colorSets[setIndex];

            InitializeComponent();
            InitializeBoard();
            InitializeTimers();

            board = new Board();
            moves = board.GenerateMoves();

            DrawBoard();
            UpdateEvaluationBar();

            MoveHistory = new ObservableCollection<HistoryObject>();
            MoveHistory.CollectionChanged += ListView_ScrollToBottom;
            DataContext = this;

            dialogService = new DialogService();
            AppSettings.Instance.ZobristHash = board.GetHash();
            AppSettings.Instance.logs.Clear();

            whiteCaptures = new List<int>();
            blackCaptures = new List<int>();
        }

        #region Modals
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
        #endregion

        #region Rendering Board
        private void InitializeBoard()
        {
            int height = 44;

            for (int row = 0; row < 8; row++) {
                for (int col = 0; col < 8; col++) {

                    // Board
                    Rectangle rect = new Rectangle();
                    rect.Fill = (row + col) % 2 == 0 ? set.whiteColor : set.blackColor;
                    Background.Children.Add(rect);

                    // Highlights
                    Rectangle highlight = new Rectangle();
                    highlights[col, row] = highlight;
                    HighlightGrid.Children.Add(highlight);

                    // Pieces
                    Image image = new Image();
                    images[col, row] = image;
                    image.Height = height;
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

        bool gameEnded = false;
        private void RefreshBoard()
        {
            DrawBoard();
            ClearHighlights();
            DrawLastMove();
            UpdateEvaluationBar();
            RefreshCaptures();

            moves = board.GenerateMoves();

            if (state != GameState.InProgress && !gameEnded) {
                timer.Stop();

                if(state == GameState.WhiteWon) Alert("Białe wygrywają - Mat");
                else if(state == GameState.BlackWon) Alert("Czarne wygrywają - Mat");
                else if(state == GameState.Stalemate) Alert("Remis - Pat");
                else if(state == GameState.DrawRepetition) Alert("Remis przez powtórzenia");
                else if(state == GameState.FiftyMovesRule) Alert("Remis - 50 ruchów bez postępu");
                else if(state == GameState.InsufficientMaterial) Alert("Remis - brak przewagi materiału");
                else if(state == GameState.OutOfTimeBlack) Alert("Białe wygrywają - koniec czasu");
                else if(state == GameState.OutOfTimeWhite) Alert("Czarne wygrywają - koniec czasu");

                AppSettings.Instance.BookMove = "None";
                gameEnded = true;

                timer.Stop();
            }
            else {

                Move? bookMove = board.GetBookMove();
                if (bookMove.HasValue) {
                    AppSettings.Instance.BookMove = "Book move is " + bookMove.Value.ToString();
                }
                else {
                    AppSettings.Instance.BookMove = "No book move";
                }
            }
        }

        private void DrawBoard()
        {
            int height = 44;
            if (imageSet == ImagesSet.Custom) height = 52;

            for (int row = 0; row < 8; row++) {
                for (int col = 0; col < 8; col++) {
                    images[col, row].Height = height;
                }
            }

            for (int row = 0; row < 8; row++) {
                for (int col = 0; col < 8; col++) {
                    //images[col, row].Source = Images.sources[board[GridToBoard(row, col)]];
                    images[col, row].Source = Images.GetImages(imageSet)[board[GridToBoard(row, col)]];
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
                    ColorHighlight(moves[i].TargetSquare, set.highlightColor);
                }
            }

        }

        private void DrawLastMove()
        {
            Move? lastMove = board.LastMove();

            if (lastMove.HasValue) {
                ColorHighlight(lastMove.Value.StartSquare, set.lastMoveColor);
                ColorHighlight(lastMove.Value.TargetSquare, set.lastMoveColor);
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
            if (isMultiplayer) return;
            if(MoveHistory.Count > 0) {

                if (AppSettings.Instance.AIEnabled) {
                    if (MoveHistory[MoveHistory.Count - 1].MoveBlack == "") {
                        return;
                    }

                    MoveHistory.RemoveAt(MoveHistory.Count - 1);

                    positionHistory[board.GetHash()]--;
                    board.UnmakeMove();
                    moveNumber--;

                    moveRule50 = Math.Min(moveRule50 - 1, 0);
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

                moveRule50 = Math.Min(moveRule50 - 1, 0);
                state = GameState.InProgress;
                positionHistory[board.GetHash()]--;
                board.UnmakeMove();

                RefreshBoard();
                AppSettings.Instance.ZobristHash = board.GetHash();
            }
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            Rotate();
        }

        private void Rotate()
        {
            rotated = !rotated;
            RefreshBoard();
            DrawHighlights();
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
        private bool timersEnabled = true;
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
            if (!timersEnabled) return;

            if(!isMultiplayer || isHost) {
                if (turn == 0) {
                    timerWhite--;
                }
                else {
                    timerBlack--;
                }

                if(isMultiplayer)
                    network.SendTime(timerWhite, timerBlack);
            }

            if(timerWhite <= 0) {
                state = GameState.OutOfTimeWhite;
            }

            if (timerBlack <= 0) {
                state = GameState.OutOfTimeBlack;
            }

            WhiteTimer.Content = GetTimerString(rotated ? timerBlack : timerWhite);
            BlackTimer.Content = GetTimerString(rotated ? timerWhite : timerBlack);
        }

        public string GetTimerString(int timer)
        {
            return (timer / 60).ToString() + ":" + (timer % 60 < 10 ? "0" : "") + (timer % 60).ToString();
        }
        #endregion

        #region Modes And Network
        private int playingAs = 0;
        public bool isHost = false;
        private GameMode mode;
        public void SetMode(GameMode mode, string fen)
        {
            if(fen.Length > 5) {
                Debug.WriteLine(fen);
                board.LoadPositionFromFEN(fen);
                RefreshBoard();
            }

            this.mode = mode;

            if (mode == GameMode.TwoPlayers) {
                AppSettings.Instance.AIEnabled = false;
            }
            else if(mode == GameMode.PlayerMinimax) {
                AppSettings.Instance.AIEnabled = true;
            }
            else if(mode == GameMode.Server) {
                isMultiplayer = true;
                timersEnabled = false;
                isHost = true;
                AppSettings.Instance.AIEnabled = false;

                ChessServer server = new();
                server.onClientConnected += OnClientConnected;
                server.StartServer();
                network = server;
                network.onMove += OnNetworkMove;
                network.onTimeUpdated += OnTimeUpdated;

                playingAs = 0;

                //Alert("Gra się rozpocznie jak drugi gracz dołączy (moment ruszenia zegaru)");
            }
            else if (mode == GameMode.Client) {
                isMultiplayer = true;
                timersEnabled = true;
                AppSettings.Instance.AIEnabled = false;

                string ip = dialogService.OpenDialog(new NetworkViewModel("Dołączanie do gry"));
                ChessClient client = new();
                client.ConnectToServer(ip);
                network = client;
                network.onMove += OnNetworkMove;
                network.onTimeUpdated += OnTimeUpdated;

                playingAs = 1;
                Rotate();
            }
        }

        private void OnNetworkMove(Move move)
        {
            if(board.LastMove().ToString() == move.ToString()) {
                return; // it was my move
            }

            Dispatcher.Invoke(() => {
                MakeMove(move);
            });
        }

        private void OnClientConnected()
        {
            timersEnabled = true;

            Dispatcher.Invoke(() => {
                Alert("Gra rozpoczęta", "Drugi gracz dołączył");
            });
        }

        private void OnTimeUpdated(int white, int black)
        {
            timerWhite = white;
            timerBlack = black;
            Debug.WriteLine(white + " " + black);
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            setIndex++;
            if (setIndex >= ColorSet.colorSets.Count) setIndex = 0;

            set = ColorSet.colorSets[setIndex];

            Background.Children.Clear();
            for (int row = 0; row < 8; row++) {
                for (int col = 0; col < 8; col++) {
                    Rectangle rect = new Rectangle();
                    rect.Fill = (row + col) % 2 == 0 ? set.whiteColor : set.blackColor;
                    Background.Children.Add(rect);
                }
            }

            AppSettings.Instance.savedColorIndex = setIndex;
        }

        private void IconsButton_Click(object sender, RoutedEventArgs e)
        {
            if (imageSet == ImagesSet.Normal) imageSet = ImagesSet.Custom;
            else imageSet = ImagesSet.Normal;

            DrawBoard();
            AppSettings.Instance.savedImages = imageSet;
        }
    }
}
