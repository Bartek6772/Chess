namespace ChessEngine
{
    public class Board
    {
        private int[] Squares = new int[64];

        public int this[int square] {
            get { return Squares[square]; }
            set { Squares[square] = value; }
        }

        public Board()
        {
            Squares[0] = Piece.WhiteBishop;
            Squares[34] = Piece.WhiteRook;
        }
    }
}
