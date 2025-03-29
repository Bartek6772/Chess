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

        Stopwatch stopwatch;

        private bool breaker = false;
        private int limit;

        public Search(Board board)
        {
            this.board = board;
            stopwatch = new Stopwatch();
        }

        public async Task<Result> FindBestMoveAsync(int depth, int timeLimit)
        {
            return await Task.Run(() => FindBestMove(depth, timeLimit));
        }

        public Result FindBestMove(int depth, int timeLimit)
        {
            stopwatch.Start();
            breaker = false;

            limit = timeLimit;

            Move? bestMove = null;

            int bestAlpha = MinValue;
            int bestBeta = MaxValue;
            int bestMoveValue;

            Result result = new();

            Move? bookMove = board.GetBookMove();
            if (bookMove.HasValue) {
                result.move = bookMove.Value;
                result.time = stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
                return result;
            }

            return result;

            for (int currentDepth = 1; currentDepth <= depth; currentDepth++) {

                bestMoveValue = int.MaxValue;

                List<Move> moves = board.GenerateMoves();
                //moves = moves.OrderByDescending(move => MoveHeuristic(move)).ToList();

                if (moves.Count == 0) {
                    return result;
                }

                foreach (var move in moves) {
                    board.MakeMove(move);
                    int moveValue = Minimax(currentDepth - 1, bestAlpha, bestBeta);
                    board.UnmakeMove();

                    if (breaker) {
                        result.time = stopwatch.ElapsedMilliseconds;
                        stopwatch.Reset();
                        return result;
                    }

                    if (moveValue < bestMoveValue) {
                        bestMoveValue = moveValue;
                        bestMove = move;
                    }

                    bestAlpha = int.Min(bestAlpha, moveValue);
                    bestBeta = int.Max(bestBeta, moveValue);
                }

                Debug.WriteLine($"Search at depth {currentDepth} time: {stopwatch.ElapsedMilliseconds} ms");
                result.depth = currentDepth;
                result.move = bestMove;
            }

            result.time = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            return result;
        }

        public int Minimax(int depth, int alpha, int beta)
        {
            if (depth == 0) {
                return Evaluation.Evaluate(board) * (board.colorToMove == Board.WhiteIndex ? 1 : -1);
            }

            if(stopwatch.ElapsedMilliseconds > limit && !breaker) {
                breaker = true;
            }

            if (breaker) {
                return 0;
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

        public struct Result
        {
            public Move? move;
            public long time;
            public int depth;
        }

    }
}
