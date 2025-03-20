using System;
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

        const int MinValue = -100000;
        const int MaxValue = 100000;

        public Search(Board board)
        {
            this.board = board;
        }

        public async Task<Move?> FindBestMoveAsync(int depth, int timeLimit)
        {
            return await Task.Run(() => FindBestMove(depth, timeLimit));
        }

        public Move? FindBestMove(int depth, int timeLimit)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Move? bestMove = null;
            Move? lastBestMove = null;

            int bestAlpha = MinValue;
            int bestBeta = MaxValue;
            int bestMoveValue;

            for (int currentDepth = 0; currentDepth <= depth; currentDepth++) {

                bestMoveValue = int.MaxValue;

                List<Move> moves = board.GenerateMoves();
                //moves = moves.OrderByDescending(move => MoveHeuristic(move)).ToList();

                if (moves.Count == 0) {
                    return null;
                }

                foreach (var move in moves) {
                    board.MakeMove(move);
                    int moveValue = Minimax(depth - 1, bestAlpha, bestBeta);
                    board.UnmakeMove();

                    if (moveValue < bestMoveValue) {
                        bestMoveValue = moveValue;
                        bestMove = move;
                    }

                    bestAlpha = int.Min(bestAlpha, moveValue);
                    bestBeta = int.Max(bestBeta, moveValue);

                    //if (stopwatch.ElapsedMilliseconds > timeLimit) {
                    //    stopwatch.Stop();
                    //    Debug.WriteLine("Search time: " + stopwatch.ElapsedMilliseconds);
                    //    return lastBestMove;
                    //}
                }

                lastBestMove = bestMove;
            }

            stopwatch.Stop();
            Debug.WriteLine("Search time: " + stopwatch.ElapsedMilliseconds);
            return lastBestMove;
        }

        public int Minimax(int depth, int alpha, int beta)
        {
            if (depth == 0) {
                return Evaluation.Evaluate(board) * (board.colorToMove == Board.WhiteIndex ? 1 : -1);
            }

            List<Move> moves = board.GenerateMoves();

            if (moves.Count == 0) {
                if (board.IsInCheck()) {
                    return MinValue;
                }
                return 0;
            }

            int maxVal = int.MinValue;

            foreach (Move move in moves) {
                board.MakeMove(move);

                int evaluation = -Minimax(depth - 1, -beta, -alpha);

                maxVal = int.Max(evaluation, maxVal);
                board.UnmakeMove();

                if(evaluation >= beta) {
                    return beta;
                }
                alpha = int.Max(alpha, evaluation);
            }

            return alpha;
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
