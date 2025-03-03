using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChessEngine.PrecomputedMoveData;

namespace ChessEngine
{
    public class MoveGeneration
    {
        Board board;
        List<Move> moves;

        public MoveGeneration(Board board)
        {
            this.board = board;
        }

        public List<Move> GenerateMoves()
        {
            moves = new List<Move>();
            int colorToMove = board.colorToMove == Board.WhiteIndex ? Piece.White : Piece.Black;

            foreach (int square in board.pieceList[Piece.Queen | colorToMove]) {
                GenerateSlidingMoves(square, colorToMove, 0, 8);
            }

            foreach (int square in board.pieceList[Piece.Rook | colorToMove]) {
                GenerateSlidingMoves(square, colorToMove, 0, 4);
            }

            foreach (int square in board.pieceList[Piece.Bishop | colorToMove]) {
                GenerateSlidingMoves(square, colorToMove, 4, 8);
            }

            foreach (int square in board.pieceList[Piece.Knight | colorToMove]) {
                GenerateKnightMoves(square, colorToMove);
            }

            foreach (int square in board.pieceList[Piece.Pawn | colorToMove]) {
                GeneratePawnMoves(square, colorToMove);
            }

            GenerateKingMoves(board.pieceList[Piece.King | colorToMove][0], colorToMove);

            //for (int square = 0; square < 64; square++) {

            //    if (Piece.PieceColor(board[square]) == colorToMove) {

            //        int piece = Piece.PieceType(board[square]);

            //        if (piece == Piece.Queen) GenerateSlidingMoves(square, colorToMove, 0, 8);
            //        else if (piece == Piece.Rook) GenerateSlidingMoves(square, colorToMove, 0, 4);
            //        else if (piece == Piece.Bishop) GenerateSlidingMoves(square, colorToMove, 4, 8);
            //        else if (piece == Piece.Knight) GenerateKnightMoves(square, colorToMove);
            //        else if (piece == Piece.King) GenerateKingMoves(square, colorToMove);
            //        else if (piece == Piece.Pawn) GeneratePawnMoves(square, colorToMove);
            //    }

            //}

            return moves;
        }

        private void GenerateSlidingMoves(int start, int color, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++) {
                for (int j = 1; j <= NumSquaresToEdge[start][i]; j++) {
                    int target = start + DirectionOffsets[i] * j;

                    if (Piece.IsColor(board[target], color)) {
                        break;
                    }

                    moves.Add(new Move(start, target));

                    if (Piece.IsColor(board[target], Piece.OppositeColor(color))) {
                        break;
                    }
                }
            }
        }

        private void GenerateKnightMoves(int start, int color)
        {
            for (int i = 0; i < KnightJumps[start].Length; i++) {
                int target = KnightJumps[start][i];

                if (!Piece.IsColor(board[target], color)) {
                    moves.Add(new Move(start, target));
                }
            }
        }

        private void GenerateKingMoves(int start, int color)
        {
            for (int i = 0; i < 8; i++) {
                if (NumSquaresToEdge[start][i] > 0) {
                    int target = start + DirectionOffsets[i];
                    if (!Piece.IsColor(board[target], color)) {
                        moves.Add(new Move(start, target));
                    }
                }
            }
        }


        private void GeneratePawnMoves(int start, int colorToMove)
        {
            PawnData data = colorToMove == Piece.White ? WhitePawnData : BlackPawnData;

            // Push
            int target = start + DirectionOffsets[data.direction];
            if (board[target] == Piece.None) {
                moves.Add(new Move(start, target));

                // Double Push
                if (start / 8 == data.doublePushLine && board[target + DirectionOffsets[data.direction]] == Piece.None) {
                    moves.Add(new Move(start, target + DirectionOffsets[data.direction]));
                }
            }

            // Capture 
            foreach (int attack in data.attacks[start]) {
                if (Piece.IsColor(board[attack], Piece.OppositeColor(colorToMove))) {
                    moves.Add(new Move(start, attack));
                }
            }

            // Promotion
            // En passant
        }


    }
}
