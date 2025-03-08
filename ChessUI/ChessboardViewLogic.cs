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
        private Image draggedPiece = null;  // Original chess piece
        private Image dragOverlay = null;   // Floating piece
        private Point mouseOffset;
        private Point mouseDownPosition;    // Tracks the click position
        private bool isDragging = false;    // Flag to check if dragging is in progress
        private const double DragThreshold = 5; // Minimum movement to start drag
        private int selectedSquare = -1;

        public void FindBestMoveInBackground()
        {
            Thread thread = new Thread(() => {
                Move? bestMove = search.FindBestMove(6);

                if (bestMove.HasValue) {
                    this.Dispatcher.Invoke(() => {
                        board.MakeMove(bestMove.Value);
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
            //FindBestMoveInBackground();
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

                        board.MakeMove(moves[i]);
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
