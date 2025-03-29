using ChessEngine;
using System.Diagnostics;
using Tests;

long[] correctAnswers = { 1, 20, 400, 8902, 197281, 4865609, 119060324, 3195901860, 84998978956 };
long[] correctAnswers2 = { 1, 48, 2039, 97862, 4085603, 193690690, 8031647685 };
long[] correctAnswers3 = { 1, 14, 191, 2812, 43238, 674624, 11030083, 178633661 };
long[] correctAnswers4 = { 1, 6, 264, 9467, 422333, 15833292, 706045033 };
long[] correctAnswers5 = { 1, 44, 1486, 62379, 2103487, 89941194 };

Board board = new Board();
//Move move = new() { StartSquare = 1, TargetSquare = 18 };

//ulong hash = ZobristHashing.ComputeZobristHash(board);
//Console.WriteLine(hash);

//ZobristHashing.UpdateZobristHash(ref hash, move, board);
//Console.WriteLine(hash);

//ZobristHashing.UpdateZobristHash(ref hash, move, board);
//Console.WriteLine(hash);



//Perft.RunDebug(board, 2);

Console.WriteLine("Running Standard Tests: ");
board.LoadPositionFromFEN(Board.startFEN);
for (int i = 1; i <= 5; i++) {
    Perft.Run(board, i, correctAnswers[i]);
}


Console.WriteLine("\nRunning Position 2 Tests: ");
board.LoadPositionFromFEN("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R");
for (int i = 1; i <= 4; i++) {
    Perft.Run(board, i, correctAnswers2[i]);
}

Console.WriteLine("\nRunning Position 3 Tests: ");
board.LoadPositionFromFEN("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - ");
for (int i = 1; i <= 5; i++) {
    Perft.Run(board, i, correctAnswers3[i]);
}

Console.WriteLine("\nRunning Position 4 Tests: ");
board.LoadPositionFromFEN("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq");
for (int i = 1; i <= 5; i++) {
    Perft.Run(board, i, correctAnswers4[i]);
}

Console.WriteLine("\nRunning Position 5 Tests: ");
board.LoadPositionFromFEN("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ");
for (int i = 1; i <= 4; i++) {
    Perft.Run(board, i, correctAnswers5[i]);
}

//PGNReader reader = new PGNReader();
//var list = reader.LoadPGN("Database/master_games.pgn");
//foreach (var item in list) {
//    Console.WriteLine(item.ToString());
//}

//Console.WriteLine(PGNReader.AnalyzedPositions());


