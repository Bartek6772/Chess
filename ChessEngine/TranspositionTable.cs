﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    class TranspositionTable
    {
        public const int lookupFailed = int.MinValue;

        // The value for this position is the exact evaluation
        public const int Exact = 0;
        // A move was found during the search that was too good, meaning the opponent will play a different move earlier on,
        // not allowing the position where this move was available to be reached. Because the search cuts off at
        // this point (beta cut-off), an even better move may exist. This means that the evaluation for the
        // position could be even higher, making the stored value the lower bound of the actual value.
        public const int LowerBound = 1;
        // No move during the search resulted in a position that was better than the current player could get from playing a
        // different move in an earlier position (i.e eval was <= alpha for all moves in the position).
        // Due to the way alpha-beta search works, the value we get here won't be the exact evaluation of the position,
        // but rather the upper bound of the evaluation. This means that the evaluation is, at most, equal to this value.
        public const int UpperBound = 2;


        public Entry[] entries;
        public readonly ulong size;
        public bool enabled = true;
        private Board board;
        public ulong records = 0;
        public ulong overwrites = 0;


        public TranspositionTable(Board board, int size)
        {
            this.board = board;
            this.size = (ulong)size; // Check is ulong needed as size

            entries = new Entry[size];
        }

        public void Clear()
        {
            for (int i = 0; i < entries.Length; i++) {
                entries[i] = new Entry();
            }
        }

        public ulong Index {
            get {
                return board.GetHash() % size;
            }
        }

        public Move GetStoredMove()
        {
            return entries[Index].move;
        }

        public int LookupEvaluation(int depth, int plyFromRoot, int alpha, int beta)
        {
            if (!enabled) {
                return lookupFailed;
            }
            Entry entry = entries[Index];

            if (entry.key == board.GetHash()) {
                if (entry.depth >= depth) {
                    int correctedScore = CorrectRetrievedMateScore(entry.eval, plyFromRoot);
                    if (entry.nodeType == Exact) {
                        return correctedScore;
                    }
                    if (entry.nodeType == UpperBound && correctedScore <= alpha) {
                        return correctedScore;
                    }
                    if (entry.nodeType == LowerBound && correctedScore >= beta) {
                        return correctedScore;
                    }
                }
            }
            return lookupFailed;
        }

        public void StoreEvaluation(int depth, int numPlySearched, int eval, int evalType, Move move)
        {
            if (!enabled) {
                return;
            }

            if (entries[Index].key == 0)
                records++;
            else
                overwrites++;

            Entry entry = new Entry(board.GetHash(), CorrectMateScoreForStorage(eval, numPlySearched), (byte)depth, (byte)evalType, move);
            entries[Index] = entry;
        }

        int CorrectMateScoreForStorage(int score, int numPlySearched)
        {
            if (Search.IsMateScore(score)) {
                int sign = System.Math.Sign(score);
                return (score * sign + numPlySearched) * sign;
            }
            return score;
        }

        int CorrectRetrievedMateScore(int score, int numPlySearched)
        {
            if (Search.IsMateScore(score)) {
                int sign = System.Math.Sign(score);
                return (score * sign - numPlySearched) * sign;
            }
            return score;
        }

        public struct Entry
        {
            public ulong key;
            public int eval;
            public byte depth;
            public byte nodeType;
            public Move move;

            public Entry(ulong key, int eval, byte depth, byte nodeType, Move move)
            {
                this.key = key;
                this.depth = depth;
                this.eval = eval;
                this.nodeType = nodeType;
                this.move = move;
            }
        }
    }
}
