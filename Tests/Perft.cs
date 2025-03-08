using ChessEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    static class Perft
    {
        private static void PerftTest(int depth, Results results, Board board)
        {
            List<Move> moves = board.GenerateMoves();

            foreach (var move in moves) {
                board.MakeMove(move);

                if (depth - 1 == 0) {
                    results.nodes++;
                    if (move.MoveFlag == Move.Flags.EnPassant) results.enpassant++;
                    if (move.MoveFlag == Move.Flags.CastlingQueenSide || move.MoveFlag == Move.Flags.CastlingKingSide) results.castling++;
                    if (move.IsPromotion()) results.promotions++;
                }
                else {
                    PerftTest(depth - 1, results, board);
                }

                board.UnmakeMove();
            }
        }

        static long PerftDebug(int depth, int maxDepth, Board board)
        {
            if (depth == 0)
                return 1;

            long nodes = 0;
            List<Move> moves = board.GenerateMoves();

            foreach (var move in moves) {
                board.MakeMove(move);
                long x = PerftDebug(depth - 1, maxDepth, board);
                board.UnmakeMove();

                if (depth == maxDepth) {
                    Console.WriteLine($"\"{move.ToString()}\": {x}, ");
                }

                nodes += x;
            }

            return nodes;
        }

        public static Results Run(Board board, int depth) 
        {
            Results results = new() { depth = depth };
            Stopwatch stopwatch = Stopwatch.StartNew();
            PerftTest(depth, results, board);
            stopwatch.Stop();

            results.time = stopwatch.ElapsedMilliseconds;

            return results;
        }

        public static void RunDebug(Board board, int depth)
        {
            Console.WriteLine("Running debug perft: ");
            long total = PerftDebug(depth, depth, board);
            Console.WriteLine("\nTotal: " + total);
        }


        public static void Print(Results results, long answer)
        {
            Console.Write($"Perft({results.depth}) Nodes: {results.nodes}, Time: {results.time}ms ");

            bool ok = results.nodes == answer;
            Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(ok ? "Correct" : $"Failed ({results.nodes}/{answer})");
            Console.ForegroundColor = ConsoleColor.Gray;

            if (!ok) {
                Console.WriteLine(results.ToString());
            }
        }
        public static void Run(Board board, int depth, long answer) => Print(Run(board, depth), answer);

        public class Results
        {
            public long depth = 0;
            public long nodes = 0;
            public long castling = 0;
            public long enpassant = 0;
            public long promotions = 0;
            public long time = 0;

            public override string ToString()
            {
                return $"Nodes: {nodes}\nCastles: {castling}\nenPassant: {enpassant}\nPromotions: {promotions}\n";
            }
        };
    }
}