﻿using System;
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

        static PrecomputedMoveData()
        {
            NumSquaresToEdge = new int[64][];
            PrecomputeSlidingMoves();

            KnightJumps = new int[64][];
            PrecomputeKnightJumps();
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

        //private static void PrecomputePawnAttacks(ref PawnData data, int[] jumpX, int [] jumpY)
        //{
        //    for (int row = 0; row < 8; row++) {
        //        for (int col = 0; col < 8; col++) {

        //            List<int> moves = new List<int>(8);

        //            for (int i = 0; i < 2; i++) {
        //                int targetX = col + jumpX[i];
        //                int targetY = row + jumpY[i];

        //                if (targetX >= 0 && targetX < 8 && targetY >= 0 && targetY < 8) {
        //                    moves.Add(targetX + targetY * 8);
        //                }
        //            }

        //            data.attacks[col + row * 8] = moves.ToArray();
        //        }
        //    }
        //}
    }
}
