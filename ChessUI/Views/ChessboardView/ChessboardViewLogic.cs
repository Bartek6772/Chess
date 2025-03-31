using ChessEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChessUI
{
    partial class ChessboardView
    {
        private Image draggedPiece = null;
        private Image dragOverlay = null;
        private Point mouseOffset;
        private Point mouseDownPosition;
        private bool isDragging = false;
        private const double DragThreshold = 5;
        private int selectedSquare = -1;

        int moveNumber = 1;

        int moveRule50 = 0;
        private Dictionary<ulong, int> positionHistory = new Dictionary<ulong, int>();
        private GameState state = GameState.InProgress;

        private void MakeMove(Move move)
        {
            moveRule50++;
            if (Piece.PieceType(board[move.StartSquare]) == Piece.Pawn || board[move.TargetSquare] != Piece.None) {
                moveRule50 = 0;
            }

            if (board.colorToMove == 1) {
                MoveHistory[MoveHistory.Count - 1].MoveBlack = board.GetMoveLongName(move);
            }
            else {
                HistoryObject ho = new HistoryObject();
                ho.MoveWhite = board.GetMoveLongName(move);
                ho.MoveBlack = "";
                ho.MoveNumber = moveNumber;
                moveNumber++;

                MoveHistory.Add(ho);
            }

            board.MakeMove(move);

            if (isMultiplayer) {
                if(turn == playingAs) {
                    // I made move
                    network.SendMove(move);
                }
                else {
                    // opponent make move
                }
            }

            turn = 1 - turn;

            AppSettings.Instance.ZobristHash = board.GetZobristHash();

            Debug.WriteLine("Book Moves: " + PGNReader.MovesInThisPosition(board.GetZobristHash()));

            #region Special Rules
            if (moveRule50 > 50) {
                state = GameState.FiftyMovesRule;
            }
            else if (board.IsCheckmate()) {
                state = board.colorToMove == Board.BlackIndex ? GameState.WhiteWon : GameState.BlackWon;
            }
            else if (board.IsStalemate()) {
                state = GameState.Stalemate;
            }

            ulong hash = board.GetZobristHash();
            if (positionHistory.ContainsKey(hash)) {
                positionHistory[hash]++;

                if (positionHistory[hash] == 3) {
                    state = GameState.DrawRepetition;
                }
            }
            else {
                positionHistory[hash] = 1;
            }

            if (board.pieceList[Piece.WhiteRook].Count + board.pieceList[Piece.BlackRook].Count == 0 &&
                board.pieceList[Piece.WhiteQueen].Count + board.pieceList[Piece.BlackQueen].Count == 0) {

                int b_knights = board.pieceList[Piece.BlackKnight].Count;
                int b_bishops = board.pieceList[Piece.BlackBishop].Count;
                int w_knights = board.pieceList[Piece.WhiteKnight].Count;
                int w_bishops = board.pieceList[Piece.WhiteBishop].Count;

                if (board.WhiteMaterial() == 0 && (b_knights + b_bishops) == 1 ) {
                    state = GameState.InsufficientMaterial;
                }
                
                if (board.BlackMaterial() == 0 && (w_bishops + w_knights) == 1) {
                    state = GameState.InsufficientMaterial;
                }

                if (board.WhiteMaterial() == 0 && board.BlackMaterial() == 0) {
                    state = GameState.InsufficientMaterial;
                }

                if (board.WhiteMaterial() == 0 && board.BlackMaterial() == 0) {
                    state = GameState.InsufficientMaterial;
                }

                if((w_knights + b_knights) == 0 && b_bishops == 1 && w_bishops == 1) {
                    bool s1 = IsWhiteSquare(board.pieceList[Piece.BlackBishop][0]);
                    bool s2 = IsWhiteSquare(board.pieceList[Piece.WhiteBishop][0]);

                    if(s1 == s2) {
                        state = GameState.InsufficientMaterial;
                    }
                }
            }

            bool IsWhiteSquare(int square)
            {
                int row = square / 8;
                int col = square % 8;
                return (row + col) % 2 == 0;
            }
            #endregion

            RefreshBoard();
        }

        public void FindBestMoveInBackground()
        {
            Thread thread = new Thread(() => {
                SearchResult result = board.FindBestMove(AppSettings.Instance.SearchDepth, AppSettings.Instance.SearchTimeLimit);

                if (result.move.HasValue) {
                    Dispatcher.Invoke(() => {
                        AppSettings.Instance.logs.Add($"Search at depth {result.depth} time {result.time}");
                        MakeMove(result.move.Value);
                        //RefreshBoard();

                        Move? bookMove = board.GetBookMove();
                        if (bookMove.HasValue) {
                            Debug.WriteLine(bookMove.Value.ToString());
                            AppSettings.Instance.BookMove = "Book move is " + bookMove.Value.ToString();
                        }
                        else {
                            AppSettings.Instance.BookMove = "No book move";
                        }
                    });
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void EnemyResponse()
        {
            if (!AppSettings.Instance.AIEnabled) return;
            FindBestMoveInBackground();
        }

        private void SelectSquare(int row, int col)
        {
            if (!timersEnabled) return; // timers disabled means game is paused
            if (state != GameState.InProgress) return;
            if (playingAs != turn) return;

            if (selectedSquare != -1) {

                Move.Flags promotionFlag = Move.Flags.None;
                bool asked = false;

                for (int i = 0; i < moves.Count; i++) {
                    int target = GridToBoard(row, col);
                    if (moves[i].StartSquare == selectedSquare && moves[i].TargetSquare == target) {

                        if (moves[i].IsPromotion()) {

                            if(!asked) {
                                promotionFlag = Promotion();
                                asked = true;
                            }

                            if (moves[i].Flag != promotionFlag) {
                                continue;
                            }

                        }

                        selectedSquare = -1;
                        MakeMove(moves[i]);
                        //RefreshBoard();

                        EnemyResponse();
                        return;
                    }
                }
            }

            selectedSquare = GridToBoard(row, col);
            ClearHighlights();
            DrawHighlights();
        }

        private void Board_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseDownPosition = e.GetPosition(Chessboard);
            int row = (int)(mouseDownPosition.Y / 64);
            int col = (int)(mouseDownPosition.X / 64);

            if (row < 0 || row > 7 || col < 0 || col > 7) {
                Debug.WriteLine("Col and row outside the board");
                return;
            }

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
            double distance = Math.Sqrt(Math.Abs(currentPosition.X - mouseDownPosition.X) + Math.Abs(currentPosition.Y - mouseDownPosition.Y));

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

                Debug.WriteLine("Mouse button up");
                SelectSquare(row, column);
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
    }
}