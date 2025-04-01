using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    static class Evaluation
    {
        const int pawnValue = 100;
        const int knightValue = 300;
        const int bishopValue = 300;
        const int rookValue = 500;
        const int queenValue = 900;

        const float endgameMaterialStart = rookValue * 2 + bishopValue + knightValue;

        public static int Evaluate(Board board)
        {
            int whiteEval = 0;
            int blackEval = 0;

            int whiteMaterial = CountMaterial(board, Piece.White);
            int blackMaterial = CountMaterial(board, Piece.Black);

            int whiteMaterialWithoutPawns = whiteMaterial - board.pieceList[Piece.WhitePawn].Count * pawnValue;
            int blackMaterialWithoutPawns = blackMaterial - board.pieceList[Piece.BlackPawn].Count * pawnValue;
            float whiteEndgamePhaseWeight = EndgamePhaseWeight(whiteMaterialWithoutPawns);
            float blackEndgamePhaseWeight = EndgamePhaseWeight(blackMaterialWithoutPawns);

            whiteEval += whiteMaterial;
            blackEval += blackMaterial;

            whiteEval += EndGameEval(board, whiteMaterial, blackMaterial, blackEndgamePhaseWeight);
            blackEval += EndGameEval(board, blackMaterial, whiteMaterial, whiteEndgamePhaseWeight);

            whiteEval += CountPosition(board, Piece.White, blackEndgamePhaseWeight);
            blackEval += CountPosition(board, Piece.Black, whiteEndgamePhaseWeight);

            int evaluation = whiteEval - blackEval;

            return evaluation;
        }

        public static int CountMaterial(Board board, int color)
        {
            int material = 0;
            material += board.pieceList[Piece.Pawn | color].Count * pawnValue;
            material += board.pieceList[Piece.Knight | color].Count * knightValue;
            material += board.pieceList[Piece.Bishop | color].Count * bishopValue;
            material += board.pieceList[Piece.Rook | color].Count * rookValue;
            material += board.pieceList[Piece.Queen | color].Count * queenValue;
            return material;
        }

        private static int CountPosition(Board board, int color, float endgamePhaseWeight)
        {
            int position = 0;

            position += EvaluatePieceList(board, Piece.Pawn, color);
            position += EvaluatePieceList(board, Piece.Knight, color);
            position += EvaluatePieceList(board, Piece.Bishop, color);
            position += EvaluatePieceList(board, Piece.Rook, color);
            position += EvaluatePieceList(board, Piece.Queen, color);

            int kingSquare = board.pieceList[Piece.King | color][0];

            int kingEarlyPhase = PieceSquareTables.Read(board[kingSquare], kingSquare);
            position += (int)(kingEarlyPhase * (1 - endgamePhaseWeight));

            return position;
        }

        private static int EvaluatePieceList(Board board, int piece, int color)
        {
            int position = 0;

            foreach (var square in board.pieceList[piece | color]) {
                position += PieceSquareTables.Read(board[square], square);
            }

            return position;
        }

        static float EndgamePhaseWeight(int materialCountWithoutPawns)
        {
            const float multiplier = 1 / endgameMaterialStart;
            return 1 - Math.Min(1, materialCountWithoutPawns * multiplier);
        }

        static int EndGameEval(Board board, int myMaterial, int opponentMaterial, float endgameWeight)
        {
            int mopUpScore = 0;

            if (myMaterial > opponentMaterial + pawnValue * 2 && endgameWeight > 0) {
                int friendlyKingSquare = board.pieceList[Piece.King | (board.colorToMove * Piece.Black)][0];
                int opponentKingSquare = board.pieceList[Piece.King | ((1 - board.colorToMove) * Piece.Black)][0];
                mopUpScore += PrecomputedMoveData.centreManhattanDistance[opponentKingSquare] * 10;
                mopUpScore += (14 - PrecomputedMoveData.NumRookMovesToReachSquare(friendlyKingSquare, opponentKingSquare)) * 4;

                return (int)(mopUpScore * endgameWeight);
            }
            return 0;
        }

        public static int GetPieceValue(int piece)
        {
            return piece switch {
                Piece.Bishop => bishopValue,
                Piece.Knight => knightValue,
                Piece.Rook => rookValue,
                Piece.Queen => queenValue,
                Piece.Pawn => pawnValue,
                _ => 0,
            };
        }
    }
}
