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

        private bool EnableSelecting = true;

        int moveNumber = 1; // Remember to delete when undoing moves
        private void MakeMove(Move move)
        {
            if(board.colorToMove == 1) {
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

            turn = 1 - turn;
            board.MakeMove(move);
        }

        public void FindBestMoveInBackground()
        {
            Thread thread = new Thread(() => {
                Search.Result result = search.FindBestMove(AppSettings.Instance.SearchDepth, AppSettings.Instance.SearchTimeLimit);

                if (result.move.HasValue) {
                    Dispatcher.Invoke(() => {
                        AppSettings.Instance.logs.Add($"Search at depth {result.depth} time {result.time}");
                        MakeMove(result.move.Value);
                        RefreshBoard();
                        EnableSelecting = true;

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
            EnableSelecting = false;
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
            if (!EnableSelecting) return;

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

                            if (moves[i].MoveFlag != promotionFlag) {
                                continue;
                            }

                        }

                        MakeMove(moves[i]);
                        selectedSquare = -1;
                        RefreshBoard();

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
            //else {
            //    MessageBox.Show($"Clicked on ({column}, {row})");
            //}

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
