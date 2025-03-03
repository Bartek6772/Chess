using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public class PieceList : IEnumerable<int>
    {
        private int[] occupiedSquares;
        private int[] map;
        private int numPieces = 0;

        public int Count {
            get {
                return numPieces;
            }
        }

        public PieceList(int maxPieceCount = 16)
        {
            occupiedSquares = new int[maxPieceCount];
            map = new int[120];
        }

        public void AddPiece(int square)
        {
            occupiedSquares[numPieces] = square;
            map[square] = numPieces;
            numPieces++;
        }

        public void RemovePiece(int square)
        {
            int pieceIndex = map[square];
            occupiedSquares[pieceIndex] = occupiedSquares[numPieces - 1];
            map[occupiedSquares[pieceIndex]] = pieceIndex;
            numPieces--;
        }

        public void MovePiece(int startSquare, int targetSquare)
        {
            int pieceIndex = map[startSquare]; // get the index of this element in the occupiedSquares array
            occupiedSquares[pieceIndex] = targetSquare;
            map[targetSquare] = pieceIndex;
        }

        public int this[int index] => occupiedSquares[index];

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < numPieces; i++) {
                yield return occupiedSquares[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
