using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public class Piece
    {
        public const int None = 0;
        public const int Pawn = 1;
        public const int Rook = 2;
        public const int Knight = 3;
        public const int Bishop = 4;
        public const int Queen = 5;
        public const int King = 6;

        public const int White = 0;
        public const int Black = 8;

        public const int WhitePawn = Pawn | White;
        public const int WhiteKnight = Knight | White;
        public const int WhiteBishop = Bishop | White;
        public const int WhiteRook = Rook | White;
        public const int WhiteQueen = Queen | White;
        public const int WhiteKing = King | White;

        public const int BlackPawn = Pawn | Black;
        public const int BlackKnight = Knight | Black;
        public const int BlackBishop = Bishop | Black;
        public const int BlackRook = Rook | Black;
        public const int BlackQueen = Queen | Black;
        public const int BlackKing = King | Black;

        public const int MaxPieceIndex = BlackKing;

        const int typeMask = 0b0111;
        const int colorMask = 0b1000;

        public static int PieceColor(int piece) => piece & colorMask;
        public static int PieceType(int piece) => piece & typeMask;
        public static int OppositeColor(int color) => color == White ? Black : White;

        public static bool IsColor(int piece, int color) => piece != None && (piece & colorMask) == color;
    }
}
