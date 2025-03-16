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


        int moveNumber = 1; // Remember to delete when undoing moves
        private void MakeMove(Move move)
        {
            if(board.colorToMove == 1) {
                MoveHistory[MoveHistory.Count - 1].MoveBlack = board.GetMoveLongName(move);
            }
            else {
                HistoryObject ho = new HistoryObject();
                ho.MoveWhite = board.GetMoveLongName(move);
                ho.MoveNumber = moveNumber;
                moveNumber++;

                MoveHistory.Add(ho);
            }

            board.MakeMove(move);
        }

        public void FindBestMoveInBackground()
        {
            Thread thread = new Thread(() => {
                Move? bestMove = search.FindBestMove(5);

                if (bestMove.HasValue) {
                    Dispatcher.Invoke(() => {
                        MakeMove(bestMove.Value);
                        RefreshBoard();
                    });
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void EnemyResponse()
        {
            // if else ... modes
            // do color check: player cant choose enemy pieces when playing with computer (selecting during search)

            if (!AppSettings.AIEnabled) return;
            FindBestMoveInBackground();
        }

        private void SelectSquare(int row, int col)
        {
            if (selectedSquare != -1) {

                for (int i = 0; i < moves.Count; i++) {
                    int target = GridToBoard(row, col);
                    if (moves[i].StartSquare == selectedSquare && moves[i].TargetSquare == target) {

                        if (moves[i].IsPromotion()) {

                            // ask for piece (if not asked before)
                            // if match do move
                            // else continue

                            if (moves[i].MoveFlag != Move.Flags.PromotionQueen) {
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
            Debug.WriteLine("Mouse button down");
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
