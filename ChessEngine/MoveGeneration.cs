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
            List<Move> legalMoves = new List<Move>();
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

            foreach (Move move in moves) {
                board.MakeMove(move);

                int frienldyKingSquare = board.pieceList[Piece.King | colorToMove][0];
                if (!IsSquareAttacked(frienldyKingSquare, Piece.OppositeColor(colorToMove))) {
                    legalMoves.Add(move);
                }
                board.UnmakeMove();
            }

            return legalMoves;
        }

        // try passing freindly color instead
        public bool IsSquareAttacked(int attackSquare, int attackerColour)
        {
            int attackerColourIndex = attackerColour / Piece.Black;
            int friendlyColourIndex = 1 - attackerColourIndex;
            int friendlyColour = friendlyColourIndex * Piece.Black;

            int startDirIndex = 0;
            int endDirIndex = 8;

            int opponentKingSquare = board.pieceList[Piece.King | attackerColour][0];
            for (int i = 0; i < 8; i++) {
                if (NumSquaresToEdge[opponentKingSquare][i] > 0 && opponentKingSquare + DirectionOffsets[i] == attackSquare) {
                    return true;
                }
            }

            if (board.pieceList[Piece.Queen | attackerColour].Count == 0) {
                startDirIndex = (board.pieceList[Piece.Rook | attackerColour].Count > 0) ? 0 : 4;
                endDirIndex = (board.pieceList[Piece.Bishop | attackerColour].Count > 0) ? 8 : 4;
            }

            for (int dir = startDirIndex; dir < endDirIndex; dir++) {
                bool isDiagonal = dir > 3;

                int n = NumSquaresToEdge[attackSquare][dir];
                int directionOffset = DirectionOffsets[dir];

                for (int i = 0; i < n; i++) {
                    int squareIndex = attackSquare + directionOffset * (i + 1);
                    int piece = board[squareIndex];

                    // This square contains a piece
                    if (piece != Piece.None) {
                        if (Piece.IsColor(piece, friendlyColour)) {
                            break;
                        }
                        // This square contains an enemy piece
                        else {
                            int pieceType = Piece.PieceType(piece);

                            // Check if piece is in bitmask of pieces able to move in current direction
                            if (isDiagonal && Piece.IsBishopOrQueen(pieceType) || !isDiagonal && Piece.IsRookOrQueen(pieceType)) {
                                return true;
                            }
                            else {
                                // This enemy piece is not able to move in the current direction, and so is blocking any checks/pins
                                break;
                            }
                        }
                    }
                }
            }

            // Knight attacks
            foreach (int attack in KnightJumps[attackSquare]) {
                if (board[attack] == (Piece.Knight | attackerColour)) {
                    return true;
                }
            }

            // check if enemy pawn is controlling this square
            for (int i = 0; i < 2; i++) {
                // Check if square exists diagonal to friendly king from which enemy pawn could be attacking it
                if (NumSquaresToEdge[attackSquare][PawnData[friendlyColourIndex].attacksDirections[i]] > 0) {
                    // move in direction friendly pawns attack to get square from which enemy pawn would attack
                    int s = attackSquare + DirectionOffsets[PawnData[friendlyColourIndex].attacksDirections[i]];

                    if (board[s] == (Piece.Pawn | attackerColour)) // is enemy pawn
                    {
                        return true;
                    }
                }
            }

            return false;
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

            // Castling

            if (IsSquareAttacked(start, Piece.OppositeColor(color))) return;

            int shortCastle = color == Piece.White ? Board.WK : Board.BK;
            int longCastle = color == Piece.White ? Board.WQ : Board.BQ;

            int index = color / Piece.Black;
            if (board.HasCastlingRight(shortCastle)) {

                bool good = true;
                foreach (int square in CastlingSquares[index * 2]) {
                    if(board[square] != Piece.None || IsSquareAttacked(square, Piece.OppositeColor(color))){
                        good = false;
                        break;
                    }
                }

                if (good) {
                    int dir = DirectionOffsets[3];
                    moves.Add(new Move(start, start + dir * 2, Move.Flags.CastlingKingSide));
                }
            }

            if (board.HasCastlingRight(longCastle)) {

                bool good = true;
                foreach (int square in CastlingSquares[index * 2 + 1]) {
                    if (board[square] != Piece.None || IsSquareAttacked(square, Piece.OppositeColor(color))) {
                        good = false;
                        break;
                    }
                }

                if (good) {
                    int dir = DirectionOffsets[2];
                    moves.Add(new Move(start, start + dir * 2, Move.Flags.CastlingQueenSide));
                }
            }
        }


        private void GeneratePawnMoves(int start, int colorToMove)
        {
            int index = colorToMove / Piece.Black;

            // Push
            int target = start + DirectionOffsets[PawnData[index].direction];
            if (board[target] == Piece.None) {
                moves.Add(new Move(start, target));

                // Double Push
                if (start / 8 == PawnData[index].doublePushLine && board[target + DirectionOffsets[PawnData[index].direction]] == Piece.None) {
                    moves.Add(new Move(start, target + DirectionOffsets[PawnData[index].direction], Move.Flags.DoublePush));
                }
            }

            // Capture 
            for (int i = 0; i < 2; i++) {
                if (NumSquaresToEdge[start][PawnData[index].attacksDirections[i]] > 0) {
                    int attack = start + DirectionOffsets[PawnData[index].attacksDirections[i]];

                    if (Piece.IsColor(board[attack], Piece.OppositeColor(colorToMove))) {
                        moves.Add(new Move(start, attack));
                    }

                    // En passant
                    if (attack == board.enpassantSquare) {
                        moves.Add(new Move(start, attack, Move.Flags.EnPassant));
                    }
                }
            }

            // Promotion
        }


    }
}
