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
        Stopwatch stopwatch;

        const int MinValue = -100000;
        const int MaxValue = 100000;

        public bool moveOrdering = true;
        private bool breaker = false;
        private int limit;

        public Search(Board board)
        {
            this.board = board;
            stopwatch = new Stopwatch();
        }

        //public async Task<Result> FindBestMoveAsync(int depth, int timeLimit)
        //{
        //    return await Task.Run(() => FindBestMove2(depth, timeLimit));
        //}

        private Move? bestMoveThisIteration;
        private int bestEvalThisIteration;

        public Result FindBestMove2(int depth, int timeLimit)
        {
            stopwatch.Start();
            breaker = false;
            limit = timeLimit;

            Result result = new();

            Move? bookMove = board.GetBookMove();
            if (bookMove.HasValue) {
                result.move = bookMove.Value;
                result.time = stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
                return result;
            }

            for (int currentDepth = 1; currentDepth <= depth; currentDepth++) {

                bestMoveThisIteration = null;
                bestEvalThisIteration = MinValue;

                int eval = Minimax(depth, 0, MinValue, MaxValue);

                if (breaker) {
                    result.time = stopwatch.ElapsedMilliseconds;
                    stopwatch.Reset();
                    return result;
                }

                result.move = bestMoveThisIteration;
                result.depth = currentDepth;

                Debug.WriteLine($"Search at depth {currentDepth} time: {stopwatch.ElapsedMilliseconds} ms");
            }

            result.time = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            return result;
        }

        public int Minimax(int depth, int depthFromRoot, int alpha, int beta)
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
            if(moveOrdering) moves = moves.OrderByDescending(move => MoveHeuristic(move)).ToList();

            if (moves.Count == 0) {
                if (board.IsInCheck()) {
                    return MinValue;
                }
                return 0;
            }

            int maxVal = int.MinValue;

            foreach (Move move in moves) {
                board.MakeMove(move);

                int evaluation = -Minimax(depth - 1, depthFromRoot + 1,  -beta, -alpha);

                maxVal = int.Max(evaluation, maxVal);
                board.UnmakeMove();

                if(evaluation > alpha) {
                    if(depthFromRoot == 0) {
                        bestMoveThisIteration = move;
                    }
                }

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
