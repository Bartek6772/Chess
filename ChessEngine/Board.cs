using System.Diagnostics;
using System.Runtime.InteropServices;

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
        public const string testFEN = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - ";

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
        private ulong hash;

        private MoveGeneration moveGeneration;
        private TranspositionTable transpositionTable;
        private Search search;

        public Stack<ulong> repetitionPositionHistory;

        public List<Move> GenerateMoves() => moveGeneration.GenerateMoves();

        public Board()
        {
            history = new Stack<GameState>();
            moveGeneration = new MoveGeneration(this);
            repetitionPositionHistory = new();
            LoadPositionFromFEN(startFEN);

            int sizeMB = 256;
            int sizeBytes = sizeMB * 1024 * 1024;
            int entrySizeBytes = Marshal.SizeOf<TranspositionTable.Entry>();
            int numEntries = sizeBytes / entrySizeBytes;
            transpositionTable = new TranspositionTable(this, numEntries);

            search = new Search(this, transpositionTable);
        }

        private void Initialize()
        {
            Squares = new int[64];
            castlingRights = 0b1111;
            enpassantSquare = -1;
            history.Clear();
            colorToMove = 0;
            hash = ZobristHashing.ComputeZobristHash(this);
            repetitionPositionHistory.Clear();

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

        public struct GameState
        {
            public Move move;
            public int capturedPiece;
            public int castlingRights;
            public int enpassantSquare;
        }

        public void LoadPositionFromFEN(string fen)
        {
            Initialize();
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
                        pieceList[pieceColor | pieceType].AddPiece(col + row * 8);
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
        }
        public string GenerateFEN()
        {
            var pieceSymbolFromType = new Dictionary<int, char> {
                [Piece.King] = 'k',
                [Piece.Queen] = 'q',
                [Piece.Pawn] = 'p',
                [Piece.Rook] = 'r',
                [Piece.Bishop] = 'b',
                [Piece.Knight] = 'n',
            };

            string fen = "";

            for (int y = 7; y >= 0; y--) {
                int empty = 0;

                for (int x = 0; x < 8; x++) {
                    int pos = x + y * 8;
                    if (this[pos] != Piece.None) {

                        if(empty > 0) {
                            fen += empty.ToString();
                        }

                        int type = Piece.PieceType(this[pos]);
                        char symbol = pieceSymbolFromType[type];
                        fen += Piece.IsColor(this[pos], Piece.White) ? symbol.ToString().ToUpper() : symbol;
                    }
                    else {
                        empty++;
                    }
                }

                if (empty > 0) {
                    fen += empty.ToString();
                }
                fen += "/";
            }

            fen += " " + (colorToMove == 0 ? "w" : "b");
            if (HasCastlingRight(WK)) fen += "K";
            if (HasCastlingRight(WQ)) fen += "Q";
            if (HasCastlingRight(BK)) fen += "k";
            if (HasCastlingRight(BQ)) fen += "q";

            return fen;
        }

        public void MakeMove(Move move, bool inSearch = false)
        {
            if(move.StartSquare == Move.Null.StartSquare && move.TargetSquare == Move.Null.TargetSquare) {
                Debug.WriteLine("TRYING TO MAKE NULL MOVE!!!!!!!!!!!!");
                return;
            }

            int piece = this[move.StartSquare];
            int color = colorToMove * Piece.Black;
            int moveTo = move.TargetSquare;
            int moveFrom = move.StartSquare;
            int oldEnpassant = enpassantSquare;
            int originalCastleRights = castlingRights;

            GameState newGS = new() { move = move };
            newGS.capturedPiece = this[move.TargetSquare];
            newGS.castlingRights = castlingRights;
            newGS.enpassantSquare = enpassantSquare;

            // Handle Capture
            if (newGS.capturedPiece != Piece.None) {
                hash ^= ZobristHashing.GetPieceHash(newGS.capturedPiece, moveTo);
                pieceList[newGS.capturedPiece].RemovePiece(move.TargetSquare);
            }

            // Update piece list
            pieceList[piece].MovePiece(move.StartSquare, move.TargetSquare);

            // Handling Move Flags
            int pieceOnTargetSquare = piece;
            if (move.IsPromotion()) {

                int promoteType = move.Flag switch {
                    Move.Flags.PromotionQueen => Piece.Queen,
                    Move.Flags.PromotionRook => Piece.Rook,
                    Move.Flags.PromotionKnight => Piece.Knight,
                    Move.Flags.PromotionBishop => Piece.Bishop,
                    _ => 0
                };

                pieceOnTargetSquare = promoteType | color;
                pieceList[pieceOnTargetSquare].AddPiece(moveTo);
                pieceList[Piece.Pawn | color].RemovePiece(moveTo);
            }
            else if (move.Flag == Move.Flags.Castling) {

                bool kingside = move.TargetSquare % 8 == 6;

                int castlingRookFromIndex = (kingside) ? moveTo + 1 : moveTo - 2;
                int castlingRookToIndex = (kingside) ? moveTo - 1 : moveTo + 1;

                int rook = this[castlingRookFromIndex];
                this[castlingRookToIndex] = rook;
                this[castlingRookFromIndex] = Piece.None;

                pieceList[rook].MovePiece(castlingRookFromIndex, castlingRookToIndex);

                hash ^= ZobristHashing.GetPieceHash(rook, castlingRookFromIndex);
                hash ^= ZobristHashing.GetPieceHash(rook, castlingRookToIndex);
            }
            else if (move.Flag == Move.Flags.EnPassant) {

                int enPassant = moveTo + ((colorToMove == WhiteIndex) ? -8 : 8);
                newGS.capturedPiece = this[enPassant];

                pieceList[this[enPassant]].RemovePiece(enPassant);
                this[enPassant] = Piece.None;

                hash ^= ZobristHashing.GetPieceHash(newGS.capturedPiece, enPassant);
            }

            // Update board
            this[move.TargetSquare] = pieceOnTargetSquare;
            this[move.StartSquare] = Piece.None;

            enpassantSquare = -1;
            if (move.Flag == Move.Flags.DoublePush) {
                enpassantSquare = moveTo + ((colorToMove == WhiteIndex) ? -8 : 8);
                hash ^= ZobristHashing.EnpassantFile(enpassantSquare % 8);
            }

            #region Castling Rights
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
            if (move.StartSquare == 63 || move.TargetSquare == 63) {
                RemoveCastling(BK);
            }
            else if (move.StartSquare == 56 || (move.TargetSquare == 56)) {
                RemoveCastling(BQ);
            }
            #endregion

            hash ^= ZobristHashing.SideToMove();
            hash ^= ZobristHashing.GetPieceHash(piece, moveFrom);
            hash ^= ZobristHashing.GetPieceHash(pieceOnTargetSquare, moveTo);

            if(oldEnpassant != -1) {
                hash ^= ZobristHashing.EnpassantFile(oldEnpassant % 8);
            }

            if(originalCastleRights != castlingRights) {
                hash ^= ZobristHashing.CastlingRights(originalCastleRights);
                hash ^= ZobristHashing.CastlingRights(castlingRights);
            }

            //newGS.enpassantSquare = enpassantSquare;
            colorToMove = 1 - colorToMove;
            history.Push(newGS);

            if (!inSearch) {
                if (piece == Piece.Pawn || newGS.capturedPiece != Piece.None) {
                    repetitionPositionHistory.Clear();
                    //fiftyMoveCounter = 0;
                }
                else {
                    repetitionPositionHistory.Push(hash);
                }
            }
        }
        public void UnmakeMove(bool inSearch = false)
        {
            if(history.Count == 0) {
                Debug.WriteLine("Trying to unmake move error");
                return;
            }

            GameState oldGS = history.Peek();
            history.Pop();

            colorToMove = 1 - colorToMove;

            Move move = oldGS.move;
            int movedFrom = move.StartSquare;
            int movedTo = move.TargetSquare;

            int oldEnpassant = oldGS.enpassantSquare;
            bool isEnpassant = move.Flag == Move.Flags.EnPassant;
            int originalRights = castlingRights;

            int piece = this[movedTo];
            int color = colorToMove * Piece.Black;
            int toSquarePieceType = Piece.PieceType(piece);
            int movedPieceType = (move.IsPromotion()) ? Piece.Pawn : toSquarePieceType;

            int movedPiece = movedPieceType | color;

            hash ^= ZobristHashing.SideToMove();
            hash ^= ZobristHashing.GetPieceHash(movedPieceType | color, movedFrom);
            hash ^= ZobristHashing.GetPieceHash(toSquarePieceType | color, movedTo);

            // Remove current and add old enpassant
            if (enpassantSquare != -1) {
                hash ^= ZobristHashing.EnpassantFile(enpassantSquare % 8);
            }

            if (oldEnpassant != -1) {
                hash ^= ZobristHashing.EnpassantFile(oldEnpassant % 8);
            }

            // Capture without enpassant
            if (oldGS.capturedPiece != Piece.None && !isEnpassant) {
                hash ^= ZobristHashing.GetPieceHash(oldGS.capturedPiece, movedTo);
                pieceList[oldGS.capturedPiece].AddPiece(movedTo);
            }

            if (!move.IsPromotion()) {
                pieceList[movedPiece].MovePiece(movedTo, movedFrom);
            }

            this[movedFrom] = movedPiece;
            this[movedTo] = oldGS.capturedPiece;

            if (move.IsPromotion()) {
                pieceList[Piece.Pawn | color].AddPiece(movedFrom);

                int promoteType = move.Flag switch {
                    Move.Flags.PromotionQueen => Piece.Queen,
                    Move.Flags.PromotionRook => Piece.Rook,
                    Move.Flags.PromotionKnight => Piece.Knight,
                    Move.Flags.PromotionBishop => Piece.Bishop,
                    _ => 0
                };

                pieceList[promoteType | color].RemovePiece(movedTo);
            }
            else if (isEnpassant) {
                int epIndex = movedTo + ((colorToMove == WhiteIndex) ? -8 : 8);
                this[movedTo] = 0; // added at this square (captured piece) before
                this[epIndex] = oldGS.capturedPiece;
                pieceList[oldGS.capturedPiece].AddPiece(epIndex);

                hash ^= ZobristHashing.GetPieceHash(oldGS.capturedPiece, epIndex);
            }
            else if (move.Flag == Move.Flags.Castling) {
                bool kingside = move.TargetSquare % 8 == 6;

                int castlingRookFromIndex = (kingside) ? movedTo + 1 : movedTo - 2;
                int castlingRookToIndex = (kingside) ? movedTo - 1 : movedTo + 1;

                int rook = this[castlingRookToIndex];
                this[castlingRookToIndex] = Piece.None;
                this[castlingRookFromIndex] = rook;

                pieceList[rook].MovePiece(castlingRookToIndex, castlingRookFromIndex);

                hash ^= ZobristHashing.GetPieceHash(rook, castlingRookFromIndex);
                hash ^= ZobristHashing.GetPieceHash(rook, castlingRookToIndex);
            }

            castlingRights = oldGS.castlingRights;
            enpassantSquare = oldGS.enpassantSquare;

            if (originalRights != castlingRights) {
                hash ^= ZobristHashing.CastlingRights(originalRights);
                hash ^= ZobristHashing.CastlingRights(castlingRights);
            }

            if (!inSearch && repetitionPositionHistory.Count > 0) {
                repetitionPositionHistory.Pop();
            }
        }

        #region Castling
        public void RemoveCastling(int rights) => castlingRights &= ~rights;
        public void AddCastling(int rights) => castlingRights |= rights;
        public bool HasCastlingRight(int rights) => (castlingRights & rights) != 0;
        #endregion

        #region Short Methods
        public Move? LastMove()
        {
            if (history.Count == 0) return null;
            return history.Peek().move;
        }
        public string GetMoveLongName(Move move)
        {
            if (move.IsPromotion()) {
                string p2 = move.Flag switch {
                    Move.Flags.PromotionBishop => "B",
                    Move.Flags.PromotionKnight => "N",
                    Move.Flags.PromotionRook => "R",
                    Move.Flags.PromotionQueen => "Q",
                    _ => "",
                };
                return SquareToString(move.StartSquare) + "-" + SquareToString(move.TargetSquare) + "=" + p2;
            }

            if(move.Flag == Move.Flags.Castling) {
                if(move.TargetSquare % 8 == 6) {
                    return "O-O";
                }
                else {
                    return "O-O-O";
                }
            }

            string p = Piece.PieceType(this[move.StartSquare]) switch {
                Piece.Bishop => "B",
                Piece.Knight => "N",
                Piece.Rook => "R",
                Piece.Queen => "Q",
                Piece.King => "K",
                _ => "",
            };

            return p + SquareToString(move.StartSquare) + "-" + SquareToString(move.TargetSquare);
        }
        public bool CanMoveToFrom(int start, int target, out Move.Flags flag) => moveGeneration.CanMoveToFrom(start, target, out flag);
        public string SquareToString(int square) => (char)('a' + square % 8) + (square / 8 + 1).ToString();
        public int KingPosition(int colorIndex)
        {
            return pieceList[Piece.King | Piece.Black * colorIndex][0];
        }

        public bool IsInCheck() => moveGeneration.IsSquareAttacked(KingPosition(colorToMove), (1 - colorToMove) * Piece.Black);
        public bool IsCheckmate() => IsInCheck() && moveGeneration.GenerateMoves().Count == 0;
        public bool IsStalemate() => !IsInCheck() && moveGeneration.GenerateMoves().Count == 0;


        public int WhiteMaterial() => Evaluation.CountMaterial(this, Piece.White);
        public int BlackMaterial() => Evaluation.CountMaterial(this, Piece.Black);
        public int Evaluate() => Evaluation.Evaluate(this);

        public ulong GetHash() => hash;
        public Move? GetBookMove() => PGNReader.GetBookMove(hash);
        public SearchResult FindBestMove(int depth, int timeLimit, bool moveOrdering = true, bool allowBookMoves = true) => search.FindBestMove(depth, timeLimit, moveOrdering, allowBookMoves);

        #endregion
    }
}
