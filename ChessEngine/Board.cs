using System.Diagnostics;

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
        public Stack<GameState> history;

        public const int WK = 1; // 0001 (K)
        public const int WQ = 2; // 0010 (Q)
        public const int BK = 4; // 0100 (k)
        public const int BQ = 8; // 1000 (q)

        public int castlingRights = 0b1111;
        public int enpassantSquare = -1;

        private MoveGeneration moveGeneration;

        public List<Move> GenerateMoves() => moveGeneration.GenerateMoves();

        public Board()
        {
            history = new Stack<GameState>();
            moveGeneration = new MoveGeneration(this);
            //LoadPositionFromFEN(startFEN);
            LoadPositionFromFEN("r3k2r/p1pp1pb1/bn2pnp1/2qPN3/1p2P3/P1N2Q1p/1PPBBPPP/R3K2R w KQkq");
        }

        public struct GameState
        {
            public Move move;
            public int capturedPiece;
            public int castlingRights;
            public int enpassantSquare;
        }

        public void LoadPositionFromFEN(string fen)
        {
            Squares = new int[64];
            castlingRights = 0b1111;
            enpassantSquare = -1;

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

            string[] f = fen.Split(' ');

            if(f.Length > 1) {
                colorToMove = (f[1] == "w" ? 0 : 1);
            }

            if (f.Length > 2) {
                castlingRights = 0b0000;
                foreach (char c in f[2]) {
                    if (c == 'K') AddCastling(WK);
                    else if (c == 'Q') AddCastling(WQ);
                    else if (c == 'k') AddCastling(BK);
                    else if (c == 'q') AddCastling(BQ);
                }
            }

            InitializePieceList();
        }

        public void MakeMove(Move move)
        {
            GameState newGS = new() { move = move };
            newGS.capturedPiece = this[move.TargetSquare];
            newGS.castlingRights = castlingRights;
            newGS.enpassantSquare = enpassantSquare;

            int piece = this[move.StartSquare];

            try {
                pieceList[piece].MovePiece(move.StartSquare, move.TargetSquare);
            }
            catch (NullReferenceException e) {

                Debug.WriteLine($"{piece} {move.ToString} {move.StartSquare} {move.TargetSquare}");
                //throw;
            }

            if (newGS.capturedPiece != Piece.None) {
                pieceList[newGS.capturedPiece].RemovePiece(move.TargetSquare);
            }

            this[move.TargetSquare] = piece;
            this[move.StartSquare] = Piece.None;

            // Move Flags
            if (move.MoveFlag == Move.Flags.CastlingKingSide) {
                Castle(colorToMove * 2, 0);
            }
            else if (move.MoveFlag == Move.Flags.CastlingQueenSide) {
                Castle(colorToMove * 2 + 1, 0);
            }
            else if (move.MoveFlag == Move.Flags.EnPassant) {
                int dir = PrecomputedMoveData.PawnData[colorToMove].direction;
                int enPassant = move.TargetSquare - PrecomputedMoveData.DirectionOffsets[dir];

                pieceList[this[enPassant]].RemovePiece(enPassant);
                this[enPassant] = Piece.None;
            }
            else if (move.MoveFlag == Move.Flags.PromotionQueen) {
                Promote(Piece.Queen, move);
            }
            else if (move.MoveFlag == Move.Flags.PromotionBishop) {
                Promote(Piece.Bishop, move);
            }
            else if (move.MoveFlag == Move.Flags.PromotionKnight) {
                Promote(Piece.Knight, move);
            }
            else if (move.MoveFlag == Move.Flags.PromotionRook) {
                Promote(Piece.Rook, move);
            }

            if (move.MoveFlag == Move.Flags.DoublePush) {
                int dir = PrecomputedMoveData.PawnData[colorToMove].direction;
                enpassantSquare = move.TargetSquare - PrecomputedMoveData.DirectionOffsets[dir];
            }
            else {
                enpassantSquare = -1;
            }

            // Castling rights
            if (piece == Piece.WhiteKing) {
                RemoveCastling(WK);
                RemoveCastling(WQ);
            }
            else if (piece == Piece.BlackKing) {
                RemoveCastling(BK);
                RemoveCastling(BQ);
            }

            if (move.StartSquare == 0 || move.TargetSquare == 0) {
                RemoveCastling(WQ);
            }
            else if (move.StartSquare == 7 || move.TargetSquare == 7) {
                RemoveCastling(WK);
            }
            else if (move.StartSquare == 63 || move.TargetSquare == 63) {
                RemoveCastling(BK);
            }
            else if (move.StartSquare == 56 || (move.TargetSquare == 56)) {
                RemoveCastling(BQ);
            }


            colorToMove = 1 - colorToMove;
            history.Push(newGS);
        }

        public void UnmakeMove()
        {
            if(history.Count == 0) {
                Debug.WriteLine("Trying to unmake move error");
                return;
            }

            GameState oldGS = history.Peek();
            history.Pop();

            int piece = this[oldGS.move.TargetSquare];

            pieceList[piece].MovePiece(oldGS.move.TargetSquare, oldGS.move.StartSquare);
            if (oldGS.capturedPiece != Piece.None) {
                pieceList[oldGS.capturedPiece].AddPiece(oldGS.move.TargetSquare);
            }

            this[oldGS.move.TargetSquare] = oldGS.capturedPiece;
            this[oldGS.move.StartSquare] = piece;

            colorToMove = 1 - colorToMove;

            if (oldGS.move.MoveFlag == Move.Flags.CastlingKingSide) {
                Castle(colorToMove * 2, 1);
            }
            else if (oldGS.move.MoveFlag == Move.Flags.CastlingQueenSide) {
                Castle(colorToMove * 2 + 1, 1);
            }
            else if (oldGS.move.MoveFlag == Move.Flags.EnPassant) {
                int dir = PrecomputedMoveData.PawnData[colorToMove].direction;
                int enPassant = oldGS.move.TargetSquare - PrecomputedMoveData.DirectionOffsets[dir];

                int p = Piece.Pawn | (colorToMove == 0 ? Piece.Black : Piece.White);
                pieceList[p].AddPiece(enPassant);
                this[enPassant] = p;
            }
            else if (oldGS.move.IsPromotion()) {
                UnPromote(oldGS.move);
            }

            castlingRights = oldGS.castlingRights;
            enpassantSquare = oldGS.enpassantSquare;
        }

        #region Promotion
        private void Promote(int piece, Move move)
        {
            int pawn = Piece.Pawn | (colorToMove * Piece.Black);

            int promotionPiece = piece | (colorToMove * Piece.Black);
            pieceList[pawn].RemovePiece(move.TargetSquare);
            pieceList[promotionPiece].AddPiece(move.TargetSquare);
            this[move.TargetSquare] = promotionPiece;
        }

        private void UnPromote(Move move)
        {
            // Firstly moving piece back, than changing it back
            int pawn = Piece.Pawn | (colorToMove * Piece.Black);
            int promotionPiece = this[move.StartSquare];

            pieceList[pawn].AddPiece(move.StartSquare);
            pieceList[promotionPiece].RemovePiece(move.StartSquare);

            this[move.StartSquare] = pawn;
        }
        #endregion

        #region Castling
        private void Castle(int idx, int b)
        {
            int rook = this[PrecomputedMoveData.RooksCastlingPositions[idx][b]];
            int start = PrecomputedMoveData.RooksCastlingPositions[idx][b];
            int end = PrecomputedMoveData.RooksCastlingPositions[idx][1 - b];

            this[end] = rook;
            this[start] = Piece.None;

            pieceList[rook].MovePiece(start, end);
        }

        public void RemoveCastling(int rights)
        {
            castlingRights &= ~rights;
        }

        public void AddCastling(int rights)
        {
            castlingRights |= rights;
        }

        public bool HasCastlingRight(int rights)
        {
            return (castlingRights & rights) != 0;
        }
        #endregion

        public Move? LastMove()
        {
            if (history.Count == 0) return null;
            return history.Peek().move;
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
