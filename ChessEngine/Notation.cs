using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public class Notation
    {
        static Dictionary<char, int> pieceTypeFromSymbol = new Dictionary<char, int> {
            ['k'] = Piece.King,
            ['q'] = Piece.Queen,
            ['p'] = Piece.Pawn,
            ['r'] = Piece.Rook,
            ['b'] = Piece.Bishop,
            ['n'] = Piece.Knight,
        };

        //public static Move GetMove(string move)
        //{
        //    Move m = new();
        //    return m;
        //}

        //public string GetMoveLongString(Move move)
        //{
        //    int x = move.StartSquare % 8;
        //    int y = move.StartSquare / 8;
        //    string result = "";

        //    result += (char)('a' + x) + (y + 1).ToString();
        //    x = move.TargetSquare % 8;
        //    y = move.TargetSquare / 8;
        //    result += (char)('a' + x) + (y + 1).ToString();

        //    return result;
        //}
    }
}
