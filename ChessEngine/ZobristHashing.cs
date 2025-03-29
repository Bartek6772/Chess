using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public static class ZobristHashing
    {
        private static ulong[,] pieceSquareHash = new ulong[12, 64];
        private static ulong[] castlingRightsHash = new ulong[16];
        private static ulong[] enPassantFileHash = new ulong[8];
        private static ulong sideToMoveHash;

        private const string path = "zobrist.txt";

        static ZobristHashing()
        {
            if (File.Exists(path)) {
                LoadZobristTable(path);
            }
            else {
                Generate();
                SaveZobristTable(path);
            }
        }

        private static void Generate()
        {
            // Generate random numbers for piece positions
            for (int piece = 0; piece < 12; piece++) {
                for (int square = 0; square < 64; square++) {
                    pieceSquareHash[piece, square] = Random64();
                }
            }

            // Random number for side to move
            sideToMoveHash = Random64();

            // Castling rights (16 possible states)
            for (int i = 0; i < 16; i++) {
                castlingRightsHash[i] = Random64();
            }

            // En Passant file (8 possible files)
            for (int i = 0; i < 8; i++) {
                enPassantFileHash[i] = Random64();
            }
        }

        public static void SaveZobristTable(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath)) {
                for (int piece = 0; piece < 12; piece++) {
                    for (int square = 0; square < 64; square++) {
                        writer.WriteLine(pieceSquareHash[piece, square]);
                    }
                }
                writer.WriteLine(sideToMoveHash);

                for (int i = 0; i < 16; i++) {
                    writer.WriteLine(castlingRightsHash[i]);
                }

                for (int i = 0; i < 8; i++) {
                    writer.WriteLine(enPassantFileHash[i]);
                }
            }
        }

        public static void LoadZobristTable(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath)) {
                for (int piece = 0; piece < 12; piece++) {
                    for (int square = 0; square < 64; square++) {
                        pieceSquareHash[piece, square] = ulong.Parse(reader.ReadLine());
                    }
                }
                sideToMoveHash = ulong.Parse(reader.ReadLine());

                for (int i = 0; i < 16; i++) {
                    castlingRightsHash[i] = ulong.Parse(reader.ReadLine());
                }

                for (int i = 0; i < 8; i++) {
                    enPassantFileHash[i] = ulong.Parse(reader.ReadLine());
                }
            }
        }

        private static ulong Random64()
        {
            byte[] buffer = new byte[8];
            RandomNumberGenerator.Fill(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static ulong ComputeZobristHash(Board board)
        {
            ulong hash = 0;

            // Iterate over the board and XOR corresponding values
            for (int square = 0; square < 64; square++) {
                int piece = board[square];
                if (piece != Piece.None) {
                    int pieceIndex = GetPieceIndex(piece);
                    hash ^= pieceSquareHash[pieceIndex, square];
                }
            }

            // XOR side to move
            if (board.colorToMove == Board.WhiteIndex) {
                hash ^= sideToMoveHash;
            }

            // XOR castling rights
            hash ^= castlingRightsHash[board.castlingRights];

            // XOR en passant file if available
            int enPassantFile = board.enpassantSquare;
            if (enPassantFile >= 0) {
                hash ^= enPassantFileHash[enPassantFile];
            }

            return hash;
        }

        //public static void UpdateZobristHash(ref ulong hash, Move move, Board board)
        //{
        //    int from = move.StartSquare;
        //    int to = move.TargetSquare;
        //    int movingPiece = board[from];
        //    int pieceIndex = GetPieceIndex(movingPiece);

        //    // XOR out the old piece position
        //    hash ^= pieceSquareHash[pieceIndex, from];

        //    // XOR in the new piece position
        //    hash ^= pieceSquareHash[pieceIndex, to];

        //    // If a capture occurs, remove the captured piece
        //    if (board[to] != Piece.None) {
        //        int capturedPiece = board[to];
        //        int capturedIndex = GetPieceIndex(capturedPiece);
        //        hash ^= pieceSquareHash[capturedIndex, to];
        //    }

        //    // If en passant capture, remove the captured pawn
        //    if (move.MoveFlag == Move.Flags.EnPassant) {
        //        int capturedPawnSquare = to - (board.colorToMove == Board.WhiteIndex ? 8 : -8);
        //        hash ^= pieceSquareHash[GetPieceIndex(Piece.Pawn), capturedPawnSquare];
        //    }

        //    // XOR castling rights if they change
        //    hash ^= castlingRightsHash[board.castlingRights];

        //    // XOR en passant file if a pawn moves two squares
        //    if (move.MoveFlag == Move.Flags.DoublePush) {
        //        hash ^= enPassantFileHash[to % 8];
        //    }

        //    // XOR side to move
        //    hash ^= sideToMoveHash;
        //}

        public static ulong GetPieceHash(int piece, int square) => pieceSquareHash[GetPieceIndex(piece), square];
        public static ulong EnpassantFile(int file) => enPassantFileHash[file];
        public static ulong CastlingRights(int rights) => castlingRightsHash[rights];
        public static ulong SideToMove() => sideToMoveHash;

        private static int GetPieceIndex(int piece)
        {
            return piece switch {
                Piece.WhitePawn => 0,
                Piece.WhiteRook => 1,
                Piece.WhiteKnight => 2,
                Piece.WhiteBishop => 3,
                Piece.WhiteQueen => 4,
                Piece.WhiteKing => 5,
                Piece.BlackPawn => 6,
                Piece.BlackRook => 7,
                Piece.BlackKnight => 8,
                Piece.BlackBishop => 9,
                Piece.BlackQueen => 10,
                Piece.BlackKing => 11,
                _ => -1
            };
        }
    }
}
