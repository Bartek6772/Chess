using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public static class PGNReader
    {
        private static Board board;
        private static Dictionary<ulong, List<Move>> bookMoves;

        private static int analyzedGames = 0;

        static PGNReader()
        {
            board = new Board();
            bookMoves = new();

            //LoadPGN("Database/test.pgn");
            //LoadPGN("Database/alexander_alekhine.pgn");
            LoadPGN("Database/test.pgn");
            //LoadPGN("Database/bobby_fischer.pgn");
            //LoadPGN("Database/fabiano_caruana.pgn");
            //LoadPGN("Database/garry_kasparov.pgn");
            //LoadPGN("Database/hikaru_nakamura.pgn");

            //string[] txtFiles = Directory.GetFiles("Database/", "*.pgn");
            //foreach (string file in txtFiles) {
            //    int last = analyzedGames;
            //    LoadPGN(file);
            //    Console.WriteLine($"Loaded {analyzedGames - last} games ({file})");
            //}

            Debug.WriteLine(AnalyzedPositions());
        }

        private static void AddBookMove(ulong hash, Move move)
        {
            if (!bookMoves.ContainsKey(hash)) {
                bookMoves[hash] = new List<Move>();
            }

            // deleting this line results in multiple identical moves in the same position, if move is selected randomly this will increase chance (more played - bigger chance)
            if (bookMoves[hash].Contains(move)) return;
            bookMoves[hash].Add(move);
        }

        public static Move? GetBookMove(ulong hash)
        {
            Random rnd = new();
            if (bookMoves.ContainsKey(hash)) {
                int count = bookMoves[hash].Count;
                int idx = rnd.Next(count);
                return bookMoves[hash][idx];
            }
            return null;
        }

        private static void LoadMoves(string moves)
        {
            //Console.WriteLine(moves);
            int colorToMove = 0;

            string[] splitMoves = moves.Split(' ');

            foreach (string moveRaw in splitMoves) {

                string move = moveRaw;
                if (move.Contains(".")) {
                    move = moveRaw.Split(".")[1];
                }

                if (!string.IsNullOrWhiteSpace(move)) {
                    //Debug.WriteLine(move);

                    Debug.WriteLine(board.GetZobristHash());

                    Move m = ParseSan(move.Trim(), colorToMove);

                    AddBookMove(board.GetZobristHash(), m);
                    board.MakeMove(m);
                    colorToMove = 1 - colorToMove;

                    //Console.WriteLine($"{m.StartSquare} -> {m.TargetSquare}");
                    //for (int y = 7; y >= 0; y--) {
                    //    for (int x = 0; x < 8; x++) {
                    //        Console.Write($"| {board[x + y * 8]} ");
                    //    }
                    //    Console.WriteLine("|");
                    //}
                    //Console.WriteLine();
                }
            }

            analyzedGames++;
            board.LoadPositionFromFEN(Board.startFEN);
        }

        public static void LoadPGN(string filePath)
        {
            string pgnText = File.ReadAllText(filePath);
            string[] lines = pgnText.Split('\n');

            string moves = "";

            foreach (string line in lines) {
                string trimmed = line.Trim();

                if (trimmed.StartsWith("[")) {

                    if(moves != "") {
                        LoadMoves(moves);
                        moves = "";
                    }

                    continue;
                }

                string cleanMoves = RemoveNumbersAndResults(trimmed);
                if (!string.IsNullOrEmpty(cleanMoves)) {
                    moves += cleanMoves + " ";
                }
            }

            if (moves != "") {
                LoadMoves(moves);
            }
        }
        public static Move ParseSan(string san, int colorToMove)
        {
            Move move = new ();
            int color = colorToMove * Piece.Black;
            string original = san;

            if (san == "O-O") {
                move.Flag = Move.Flags.Castling;
                move.StartSquare = board.pieceList[Piece.King | color][0];
                move.TargetSquare = board.pieceList[Piece.King | color][0] + PrecomputedMoveData.DirectionOffsets[3] * 2;
                return move;
            }
            if (san == "O-O-O") {
                move.Flag = Move.Flags.Castling;
                move.StartSquare = board.pieceList[Piece.King | color][0];
                move.TargetSquare = board.pieceList[Piece.King | color][0] + PrecomputedMoveData.DirectionOffsets[2] * 2;
                return move;
            }

            if (san.EndsWith("+")) {
                san = san.Substring(0, san.Length - 1);
            }
            else if (san.EndsWith("#")) {
                san = san.Substring(0, san.Length - 1);
            }

            string pieceSymbols = "NBRQK";
            int piece = Piece.Pawn;
            if (pieceSymbols.Contains(san[0].ToString())) {
                piece = pieceTypeFromSymbol[char.ToLower(san[0])];
                san = san.Substring(1);
            }

            // Edge case: fxg8Q+
            if (san.Contains("=")) {
                string[] parts = san.Split('=');
                san = parts[0];
                move.Flag = parts[1] switch {
                    "Q" => Move.Flags.PromotionQueen,
                    "N" => Move.Flags.PromotionKnight,
                    "B" => Move.Flags.PromotionBishop,
                    "R" => Move.Flags.PromotionRook,
                    _ => Move.Flags.None,
                };
            }

            if(pieceSymbols.Contains(san[san.Length - 1].ToString())) {
                move.Flag = san[san.Length - 1].ToString() switch {
                    "Q" => Move.Flags.PromotionQueen,
                    "N" => Move.Flags.PromotionKnight,
                    "B" => Move.Flags.PromotionBishop,
                    "R" => Move.Flags.PromotionRook,
                    _ => Move.Flags.None,
                };
                san = san.Substring(0, san.Length - 1);
            }

            if (san.Contains("x")) {
                san = san.Replace("x", "");
            }

            string target = san.Substring(san.Length - 2);
            move.TargetSquare = (target[0] - 'a') + (target[1] - '1') * 8;
            san = san.Substring(0, san.Length - 2);

            if (piece == Piece.King) {
                move.StartSquare = board.pieceList[piece | color][0];
                return move;
            }

            // Handle disambiguation
            if (san.Length == 1)
            {
                // !!!!!!!!!!!!!!!! FIX ->>>> R1g6+

                foreach (int start in board.pieceList[piece | color]) {

                    if (char.IsDigit(san[0])) {
                        if (start / 8 != san[0] - '1') continue;
                    }
                    else {
                        if (start % 8 != san[0] - 'a') continue;
                    }

                    if (board.CanMoveToFrom(start, move.TargetSquare, out Move.Flags flag)) {
                        if (move.Flag == Move.Flags.None) move.Flag = flag;
                        move.StartSquare = start;
                        return move;
                    }
                }
            }
            else if (san.Length == 2)
            {
                move.StartSquare = (san[0] - 'a') + (san[1] - '1') * 8;
                return move;
            }
            
            foreach (int start in board.pieceList[piece | color]) {
                if (board.CanMoveToFrom(start, move.TargetSquare, out Move.Flags flag)) {
                    if (move.Flag == Move.Flags.None) move.Flag = flag;
                    move.StartSquare = start;
                    return move;
                }
            }


            // SHOULD NOT HAPPEN
            Debug.WriteLine($"Problem with {original} piece: {piece} target: {move.TargetSquare}");
            return move;
        }
        private static string RemoveNumbersAndResults(string line)
        {
            string result = "";
            string[] parts = line.Split(' ');

            foreach (string part in parts) {
                if (part.EndsWith(".") || part == "*" || part == "1-0" || part == "0-1" || part == "1/2-1/2" || part.StartsWith("{") || part.EndsWith("}"))
                    continue;
                result += part + " ";
            }

            return result.Trim();
        }

        static Dictionary<char, int> pieceTypeFromSymbol = new Dictionary<char, int> {
            ['k'] = Piece.King,
            ['q'] = Piece.Queen,
            ['p'] = Piece.Pawn,
            ['r'] = Piece.Rook,
            ['b'] = Piece.Bishop,
            ['n'] = Piece.Knight,
        };
        public static int AnalyzedPositions() => bookMoves.Count;
        public static int AnalyzedGames() => analyzedGames;
        public static int MovesInThisPosition(ulong hash)
        {
            if (bookMoves.ContainsKey(hash)) {
                return bookMoves[hash].Count;
            }
            return 0;
        }
    }
}
