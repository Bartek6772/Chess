using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    class Search
    {
        Board board;
        TranspositionTable transpositionTable;
        Stopwatch stopwatch;

        const int MinValue = -100000;
        const int MaxValue = 100000;

        public bool moveOrdering = true;
        private bool breaker = false;
        private int limit;

        public Search(Board board, TranspositionTable transpositionTable)
        {
            this.board = board;
            stopwatch = new Stopwatch();
            this.transpositionTable = transpositionTable;
        }

        private Move? bestMoveThisIteration;
        private int bestEvalThisIteration;

        public SearchResult FindBestMove2(int depth, int timeLimit)
        {
            //transpositionTable.Clear();


            stopwatch.Start();
            breaker = false;
            limit = timeLimit;

            SearchResult result = new();

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

                int eval = Minimax(currentDepth, 0, MinValue, MaxValue);

                if (breaker) {
                    //result.time = stopwatch.ElapsedMilliseconds;
                    Debug.WriteLine("TT " + ((float)transpositionTable.records / (float)transpositionTable.size) + " " + ((float)transpositionTable.overwrites / (float)transpositionTable.size));
                    stopwatch.Reset();
                    return result;
                }

                result.move = bestMoveThisIteration;
                result.depth = currentDepth;
                result.time = stopwatch.ElapsedMilliseconds;

                Debug.WriteLine($"Search at depth {currentDepth} time: {stopwatch.ElapsedMilliseconds} ms");
            }
            
            stopwatch.Reset();
            Debug.WriteLine("TT " + ((float)transpositionTable.records / (float)transpositionTable.size) + " " + ((float)transpositionTable.overwrites / (float)transpositionTable.size));
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

            int ttEval = transpositionTable.LookupEvaluation(depth, depthFromRoot, alpha, beta);
            if (ttEval != TranspositionTable.lookupFailed) {
                if(depthFromRoot == 0) {
                    bestMoveThisIteration = transpositionTable.GetStoredMove();
                    bestEvalThisIteration = transpositionTable.entries[transpositionTable.Index].eval;
                }
                return ttEval;
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
            int evaluationBound = TranspositionTable.UpperBound;
            Move bestMoveInThisPosition = Move.Null;

            foreach (Move move in moves) {
                board.MakeMove(move);

                int evaluation = -Minimax(depth - 1, depthFromRoot + 1,  -beta, -alpha);

                maxVal = int.Max(evaluation, maxVal);
                board.UnmakeMove();

                if(evaluation > alpha) {

                    evaluationBound = TranspositionTable.Exact;
                    bestMoveInThisPosition = move;

                    alpha = evaluation;

                    if (depthFromRoot == 0) {
                        bestMoveThisIteration = move;
                    }
                }

                if(evaluation >= beta) {
                    transpositionTable.StoreEvaluation(depth, depthFromRoot, beta, TranspositionTable.LowerBound, move);
                    return beta;
                }
                //alpha = int.Max(alpha, evaluation);

            }

            transpositionTable.StoreEvaluation(depth, depthFromRoot, alpha, evaluationBound, bestMoveInThisPosition);
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

    public struct SearchResult
    {
        public Move? move;
        public long time;
        public int depth;
    }
}
