using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public struct Move
    {
        public int StartSquare;
        public int TargetSquare;
        public Flags MoveFlag;

        public Move(int startSquare, int targetSquare, Flags flag = Flags.None)
        {
            StartSquare = startSquare;
            TargetSquare = targetSquare;
            MoveFlag = flag;
        }

        public enum Flags
        {
            None = 0,
            PromotionKnight = 1,
            PromotionBishop = 2,
            PromotionRook = 3,
            PromotionQueen = 4,
            EnPassant = 5,
            CastlingKingSide = 6,
            CastlingQueenSide = 7,
            DoublePush = 8,
        }

        public bool IsPromotion() => MoveFlag is Flags.PromotionRook or Flags.PromotionQueen or Flags.PromotionBishop or Flags.PromotionKnight;
    }
}
