namespace ChessEngine
{
    public class Board
    {
        private int[] Squares = new int[64];

        public int this[int square] {
            get { return Squares[square]; }
            set { Squares[square] = value; }
        }

        public int this[int column, int row] {
            get { return Squares[column + row * 8]; }
            set { Squares[column + row * 8] = value; }
        }

        public const string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
        public const string testFEN = "b3R3/8/8/2n5/8/6r1/4N3/8";

        public const int WhiteIndex = 0;
        public const int BlackIndex = 1;

        public int colorToMove = WhiteIndex;

        public Board()
        {
            LoadPositionFromFEN(startFEN);
        }

        public void LoadPositionFromFEN(string fen)
        {
            var pieceTypeFromSymbol = new Dictionary<char, int> {
                ['k'] = Piece.King,
                ['q'] = Piece.Queen,
                ['p'] = Piece.Pawn,
                ['r'] = Piece.Rook,
                ['b'] = Piece.Bishop,
                ['n'] = Piece.Knight,
            };

            string fenBoard = fen.Split(' ')[0];
            int row = 7, col = 0;

            foreach (var symbol in fenBoard) {
                if (symbol == '/') {
                    row--;
                    col = 0;
                }
                else {
                    if (char.IsDigit(symbol)) {
                        col += symbol - '0';
                    }
                    else {
                        int pieceColor = char.IsUpper(symbol) ? Piece.White : Piece.Black;
                        int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                        this[col, row] = pieceColor | pieceType;
                        col++;
                    }
                }
            }

            //InitializePieceList();
        }

        public void MakeMove(Move move)
        {
            int piece = this[move.StartSquare];
            this[move.TargetSquare] = piece;
            this[move.StartSquare] = Piece.None;

            colorToMove = 1 - colorToMove;
        }
    }
}
