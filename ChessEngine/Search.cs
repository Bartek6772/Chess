﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public class Search
    {
        Board board;

        public Search(Board board)
        {
            this.board = board;
        }

        public async Task<Move?> FindBestMoveAsync(int depth)
        {
            return await Task.Run(() => FindBestMove(depth));
        }

        public Move? FindBestMove(int depth)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int bestMoveValue = int.MaxValue;

            List<Move> moves = board.GenerateMoves();
            moves = moves.OrderByDescending(move => MoveHeuristic(move)).ToList();

            if (moves.Count == 0) {
                return null;
            }

            Move bestMove = moves[0];

            bool maximizing = (board.colorToMove == Board.WhiteIndex);

            foreach (var move in moves) {

                board.MakeMove(move);
                int moveValue = Minimax(depth - 1, int.MinValue, int.MaxValue, true);
                board.UnmakeMove();

                if (moveValue < bestMoveValue) {
                    bestMoveValue = moveValue;
                    bestMove = move;
                }
            }

            stopwatch.Stop();
            Debug.WriteLine("Search time: " + stopwatch.ElapsedMilliseconds);

            if(bestMove.StartSquare == -1) {
                return null;
            }

            return bestMove;
        }

        public int Minimax(int depth, int alpha, int beta, bool maximizing)
        {
            if (depth == 0 || IsTerminal(board)) {
                return Evaluation.Evaluate(board);
            }

            List<Move> moves = board.GenerateMoves();

            if (maximizing) {
                int maxVal = int.MinValue;

                foreach (Move move in moves) {
                    board.MakeMove(move);
                    int evaluation = Minimax(depth - 1, alpha, beta, false);
                    maxVal = int.Max(evaluation, maxVal);
                    board.UnmakeMove();

                    alpha = int.Max(alpha, evaluation);
                    if (beta <= alpha) break;
                }

                return maxVal;
            }
            else {
                int minVal = int.MaxValue;

                foreach (Move move in moves) {
                    board.MakeMove(move);
                    int evaluation = Minimax(depth - 1, alpha, beta, true);
                    minVal = int.Min(evaluation, minVal);
                    board.UnmakeMove();

                    beta = int.Min(beta, evaluation);
                    if (beta <= alpha) break;
                }

                return minVal;
            }
        }

        private bool IsTerminal(Board board)
        {
            //return board.IsCheckmate() || board.IsStalemate() || board.IsDraw();
            return false;
        }

        public int MoveHeuristic(Move move)
        {
            int score = 0;

            int piece = board[move.StartSquare];
            int capturePiece = board[move.TargetSquare];

            if(capturePiece != Piece.None) {
                score = 10 * Evaluation.GetPieceValue(capturePiece) - Evaluation.GetPieceValue(piece);
            }

            if (move.IsPromotion()) {
                score += 500;
            }

            return score;
        }

    }
}
