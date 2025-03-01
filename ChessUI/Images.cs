using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChessEngine;

namespace ChessUI
{
    public static class Images
    {
        private const string path = "/Assets/no_shadow";

        public static readonly Dictionary<int, ImageSource> sources = new() {
            { Piece.None, null },
            { Piece.WhitePawn, LoadImage("w_pawn_png_128px") },
            { Piece.WhiteRook, LoadImage("w_rook_png_128px") },
            { Piece.WhiteKnight, LoadImage("w_knight_png_128px") },
            { Piece.WhiteBishop, LoadImage("w_bishop_png_128px") },
            { Piece.WhiteQueen, LoadImage("w_queen_png_128px") },
            { Piece.WhiteKing, LoadImage("w_king_png_128px") },
        };


        private static ImageSource LoadImage(string fileName)
        {
            return new BitmapImage(new Uri($"{path}/{fileName}.png", UriKind.Relative));
        }
    }
}
