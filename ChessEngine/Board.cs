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

        public PieceList[] pieceList;

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

            InitializePieceList();
        }

        public void MakeMove(Move move)
        {
            int capturedPiece = this[move.TargetSquare];
            int piece = this[move.StartSquare];

            pieceList[piece].MovePiece(move.StartSquare, move.TargetSquare);
            if (capturedPiece != Piece.None) pieceList[capturedPiece].RemovePiece(move.TargetSquare);

            this[move.TargetSquare] = piece;
            this[move.StartSquare] = Piece.None;

            colorToMove = 1 - colorToMove;
        }

        private void InitializePieceList()
        {
            // TODO: Handle situation when user load position with more than max amount of pieces
            pieceList = new PieceList[Piece.MaxPieceIndex + 1];
            pieceList[Piece.WhitePawn] = new PieceList(8);
            pieceList[Piece.WhiteKnight] = new PieceList(10);
            pieceList[Piece.WhiteBishop] = new PieceList(10);
            pieceList[Piece.WhiteRook] = new PieceList(10);
            pieceList[Piece.WhiteQueen] = new PieceList(9);
            pieceList[Piece.WhiteKing] = new PieceList(1);

            pieceList[Piece.BlackPawn] = new PieceList(8);
            pieceList[Piece.BlackKnight] = new PieceList(10);
            pieceList[Piece.BlackBishop] = new PieceList(10);
            pieceList[Piece.BlackRook] = new PieceList(10);
            pieceList[Piece.BlackQueen] = new PieceList(9);
            pieceList[Piece.BlackKing] = new PieceList(1);

            for (int i = 0; i < 64; i++) {
                if (this[i] == Piece.None) continue;
                pieceList[this[i]].AddPiece(i);
            }
        }
    }
}
