using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    static class PrecomputedMoveData
    {
        public readonly static int[] DirectionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 };
        public readonly static int[][] NumSquaresToEdge;

        public readonly static int[][] KnightJumps;


        public readonly static PawnDataStruct[] PawnData = {
            new() { direction = 0, doublePushLine = 1, promotionLine = 7, attacksDirections = [4, 6] },
            new() { direction = 1, doublePushLine = 6, promotionLine = 0, attacksDirections = [5, 7] }
        };

        public struct PawnDataStruct
        {
            public int direction;
            public int doublePushLine;
            public int promotionLine;
            public int[] attacksDirections;
        }

        // Aka manhattan distance (answers how many moves for a rook to get from square a to square b)
        public static int[,] orthogonalDistance;
        // Aka chebyshev distance (answers how many moves for a king to get from square a to square b)
        public static int[,] kingDistance;
        public static int[] centreManhattanDistance;

        static PrecomputedMoveData()
        {
            NumSquaresToEdge = new int[64][];
            PrecomputeSlidingMoves();

            KnightJumps = new int[64][];
            PrecomputeKnightJumps();

            DistanceLookup();
        }

        private static void PrecomputeSlidingMoves()
        {
            for (int row = 0; row < 8; row++) {
                for (int col = 0; col < 8; col++) {
                    int numNorth = 7 - row;
                    int numSouth = row;
                    int numEast = 7 - col;
                    int numWest = col;

                    NumSquaresToEdge[col + row * 8] = new int[8] {
                        numNorth,
                        numSouth,
                        numWest,
                        numEast,
                        Math.Min(numNorth, numWest),
                        Math.Min(numSouth, numEast),
                        Math.Min(numNorth, numEast),
                        Math.Min(numSouth, numWest),
                    };
                }
            }
        }

        private static void PrecomputeKnightJumps()
        {
            int[] jumpX = { -1, 1, 2, 2, 1, -1, -2, -2 };
            int[] jumpY = { 2, 2, 1, -1, -2, -2, -1, 1 };

            for (int row = 0; row < 8; row++) {
                for (int col = 0; col < 8; col++) {

                    List<int> moves = new List<int>(8);

                    for (int i = 0; i < 8; i++) {
                        int targetX = col + jumpX[i];
                        int targetY = row + jumpY[i];

                        if(targetX >= 0 && targetX < 8 && targetY >= 0 && targetY < 8) {
                            moves.Add(targetX + targetY * 8);
                        }
                    }

                    KnightJumps[col + row * 8] = moves.ToArray();
                }
            }
        }

        private static void DistanceLookup()
        {
            // Distance lookup
            orthogonalDistance = new int[64, 64];
            kingDistance = new int[64, 64];
            centreManhattanDistance = new int[64];
            for (int squareA = 0; squareA < 64; squareA++) {
                Coord coordA = CoordFromIndex(squareA);
                int fileDstFromCentre = Math.Max(3 - coordA.fileIndex, coordA.fileIndex - 4);
                int rankDstFromCentre = Math.Max(3 - coordA.rankIndex, coordA.rankIndex - 4);
                centreManhattanDistance[squareA] = fileDstFromCentre + rankDstFromCentre;

                for (int squareB = 0; squareB < 64; squareB++) {

                    Coord coordB = CoordFromIndex(squareB);
                    int rankDistance = Math.Abs(coordA.rankIndex - coordB.rankIndex);
                    int fileDistance = Math.Abs(coordA.fileIndex - coordB.fileIndex);
                    orthogonalDistance[squareA, squareB] = fileDistance + rankDistance;
                    kingDistance[squareA, squareB] = Math.Max(fileDistance, rankDistance);
                }
            }
        }

        public static int NumRookMovesToReachSquare(int startSquare, int targetSquare)
        {
            return orthogonalDistance[startSquare, targetSquare];
        }

        public static int NumKingMovesToReachSquare(int startSquare, int targetSquare)
        {
            return kingDistance[startSquare, targetSquare];
        }

        public static Coord CoordFromIndex(int squareIndex)
        {
            return new Coord(squareIndex % 8, squareIndex / 8);
        }

        public struct Coord
        {
            public readonly int fileIndex;
            public readonly int rankIndex;

            public Coord(int fileIndex, int rankIndex)
            {
                this.fileIndex = fileIndex;
                this.rankIndex = rankIndex;
            }
        }
    }
}
