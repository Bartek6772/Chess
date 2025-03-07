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
        MoveGeneration moveGeneration;

        public Search(Board board)
        {
            this.board = board;
            moveGeneration = new MoveGeneration(board);
        }

        public async Task<Move?> FindBestMoveAsync(int depth)
        {
            return await Task.Run(() => FindBestMove(depth));
        }

        public Move? FindBestMove(int depth)
        {
            int bestMoveValue = int.MaxValue;

            List<Move> moves = moveGeneration.GenerateMoves();

            if(moves.Count == 0) {
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

            List<Move> moves = moveGeneration.GenerateMoves();

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
    }
}
