using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public static class Evaluation
    {
        const int pawnValue = 100;
        const int knightValue = 300;
        const int bishopValue = 300;
        const int rookValue = 500;
        const int queenValue = 900;

        public static int Evaluate(Board board)
        {
            int whiteMaterial = CountMaterial(board, Piece.White);
            int blackMaterial = CountMaterial(board, Piece.Black);

            int evaluation = whiteMaterial - blackMaterial;
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
    }
}
