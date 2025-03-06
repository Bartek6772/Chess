using ChessEngine;
using System.Diagnostics;

long[] correctAnswers = { 1, 20, 400, 8902, 197281, 4865609, 119060324, 3195901860, 84998978956 };

Board board = new Board();
MoveGeneration moveGen = new MoveGeneration(board);

long Perft(int depth)
{
    if (depth == 0)
        return 1;

    long nodes = 0;
    List<Move> moves = moveGen.GenerateMoves();

    foreach (var move in moves) {
        board.MakeMove(move);
        nodes += Perft(depth - 1);
        board.UnmakeMove();
    }

    return nodes;
}

void PerfTest(int depth)
{
    Stopwatch stopwatch = Stopwatch.StartNew();
    long test = Perft(depth);
    stopwatch.Stop();

    Console.Write($"Perft({depth}) Nodes: {test}, Time: {stopwatch.ElapsedMilliseconds}ms ");
    bool ok = test == correctAnswers[depth];
    Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
    Console.WriteLine(ok ? "Correct" : "Failed");
    Console.ForegroundColor = ConsoleColor.White;
}

for (int i = 0; i <= 7; i++) {
    PerfTest(i);
}
