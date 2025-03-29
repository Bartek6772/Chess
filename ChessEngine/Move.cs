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
        public Flags Flag;

        public Move(int startSquare, int targetSquare, Flags flag = Flags.None)
        {
            StartSquare = startSquare;
            TargetSquare = targetSquare;
            Flag = flag;
        }

        public enum Flags
        {
            None = 0,
            PromotionKnight = 1,
            PromotionBishop = 2,
            PromotionRook = 3,
            PromotionQueen = 4,
            EnPassant = 5,
            Castling = 6,
            DoublePush = 7,
        }

        public bool IsPromotion() => Flag is Flags.PromotionRook or Flags.PromotionQueen or Flags.PromotionBishop or Flags.PromotionKnight;

        public override string ToString()
        {
            int x = StartSquare % 8;
            int y = StartSquare / 8;
            string result = "";

            result += (char)('a' + x) + (y + 1).ToString();
            x = TargetSquare % 8;
            y = TargetSquare / 8;
            result += (char)('a' + x) + (y + 1).ToString();

            return result;
        }
    }
}
