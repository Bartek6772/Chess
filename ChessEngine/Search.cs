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

        const int MinValue = -MaxValue;
        const int MaxValue = 999999999;
        const int MateScore = 100000;

        private bool moveOrdering = true;
        private bool abortSearch = false;
        private int limit;

        public Search(Board board, TranspositionTable transpositionTable)
        {
            this.board = board;
            stopwatch = new Stopwatch();
            this.transpositionTable = transpositionTable;
        }

        private Move? bestMoveThisIteration;
        private int bestEvalThisIteration;

        public SearchResult FindBestMove(int depth, int timeLimit, bool moveOrdering, bool bookMoves)
        {
            this.moveOrdering = moveOrdering;
            stopwatch.Start();
            abortSearch = false;
            limit = timeLimit;

            SearchResult result = new();

            bestMoveThisIteration = null;
            bestEvalThisIteration = 0;

            if (bookMoves) {
                Move? bookMove = board.GetBookMove();
                if (bookMove.HasValue) {
                    result.move = bookMove.Value;
                    result.time = stopwatch.ElapsedMilliseconds;
                    stopwatch.Reset();
                    result.isBookMove = true;
                    return result;
                }
            }
            result.isBookMove = false;

            for (int currentDepth = 1; currentDepth <= depth; currentDepth++) {

                bestMoveThisIteration = null;
                bestEvalThisIteration = MinValue;

                int eval = Minimax(currentDepth, 0, MinValue, MaxValue);

                if (abortSearch) {
                    // Random move when no move is found
                    if(!result.move.HasValue) {
                        List<Move> moves = board.GenerateMoves();
                        Random rnd = new();
                        result.move = moves[rnd.Next(moves.Count)];
                    }

                    Debug.WriteLine("TT " + ((float)transpositionTable.records / (float)transpositionTable.size) + " " + ((float)transpositionTable.overwrites / (float)transpositionTable.size));
                    stopwatch.Reset();
                    return result;
                }


                result.move = bestMoveThisIteration;
                result.depth = currentDepth;
                result.time = stopwatch.ElapsedMilliseconds;

                if (IsMateScore(eval)) {
                    Debug.WriteLine("Found mate");
                    break;
                }

                Debug.WriteLine($"Search at depth {currentDepth} time: {stopwatch.ElapsedMilliseconds} ms {eval}");
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

            // CHECK THIS
            if (depthFromRoot > 0) {
                if (board.repetitionPositionHistory.Contains(board.GetHash())) {
                    return 0;
                }

                //alpha = Math.Max(alpha, -MateScore + depthFromRoot);
                //beta = Math.Min(beta, MateScore - depthFromRoot);
                //if (alpha >= beta) {
                //    return alpha;
                //}
            }

            if (stopwatch.ElapsedMilliseconds > limit && !abortSearch) {
                abortSearch = true;
            }

            if (abortSearch) {
                return 0;
            }

            int ttEval = transpositionTable.LookupEvaluation(depth, depthFromRoot, alpha, beta);
            if (ttEval != TranspositionTable.lookupFailed) {
                if (depthFromRoot == 0) {
                    bestMoveThisIteration = transpositionTable.GetStoredMove();
                    bestEvalThisIteration = transpositionTable.entries[transpositionTable.Index].eval;
                }
                return ttEval;
            }

            List<Move> moves = board.GenerateMoves();
            if(moveOrdering) moves = moves.OrderByDescending(move => MoveHeuristic(move)).ToList();

            if (moves.Count == 0) {
                if (board.IsInCheck()) {
                    return -(MateScore - depthFromRoot);
                }
                else {
                    return 0;
                }
            }

            int evaluationBound = TranspositionTable.UpperBound;
            Move bestMoveInThisPosition = Move.Null;

            foreach (Move move in moves) {
                board.MakeMove(move, true);

                int evaluation = -Minimax(depth - 1, depthFromRoot + 1,  -beta, -alpha);
                board.UnmakeMove(true);

                if(evaluation > alpha) {

                    evaluationBound = TranspositionTable.Exact;
                    bestMoveInThisPosition = move;

                    alpha = evaluation;

                    if (depthFromRoot == 0) {
                        bestMoveThisIteration = move;
                    }
                }

                if (evaluation >= beta) {
                    transpositionTable.StoreEvaluation(depth, depthFromRoot, beta, TranspositionTable.LowerBound, move);
                    return beta;
                }
            }

            transpositionTable.StoreEvaluation(depth, depthFromRoot, alpha, evaluationBound, bestMoveInThisPosition);
            return alpha;
        }

        public static bool IsMateScore(int score)
        {
            const int maxMateDepth = 1000;
            return System.Math.Abs(score) > MateScore - maxMateDepth;
        }

        public static int NumPlyToMateFromScore(int score)
        {
            return MateScore - Math.Abs(score);

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
        public bool isBookMove;
    }
}
