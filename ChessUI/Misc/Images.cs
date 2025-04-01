using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChessEngine;
using ChessUI.Misc;

namespace ChessUI.Misc
{
    public static class Images
    {
        private const string path = "/Assets/no_shadow/";

        public static readonly Dictionary<int, ImageSource> sources = new() {
            { Piece.None, null },
            { Piece.WhitePawn, LoadImage("/Assets/no_shadow/w_pawn_png_128px") },
            { Piece.WhiteRook, LoadImage("/Assets/no_shadow/w_rook_png_128px") },
            { Piece.WhiteKnight, LoadImage("/Assets/no_shadow/w_knight_png_128px") },
            { Piece.WhiteBishop, LoadImage("/Assets/no_shadow/w_bishop_png_128px") },
            { Piece.WhiteQueen, LoadImage("/Assets/no_shadow/w_queen_png_128px") },
            { Piece.WhiteKing, LoadImage("/Assets/no_shadow/w_king_png_128px") },

            { Piece.BlackPawn, LoadImage("/Assets/no_shadow/b_pawn_png_128px") },
            { Piece.BlackRook, LoadImage("/Assets/no_shadow/b_rook_png_128px") },
            { Piece.BlackKnight, LoadImage("/Assets/no_shadow/b_knight_png_128px") },
            { Piece.BlackBishop, LoadImage("/Assets/no_shadow/b_bishop_png_128px") },
            { Piece.BlackQueen, LoadImage("/Assets/no_shadow/b_queen_png_128px") },
            { Piece.BlackKing, LoadImage("/Assets/no_shadow/b_king_png_128px") },
        };

        public static readonly Dictionary<int, ImageSource> sources2 = new() {
            { Piece.None, null },
            { Piece.WhitePawn, LoadImage("/Assets/custom/w_pawn2") },
            { Piece.WhiteRook, LoadImage("/Assets/custom/w_rook") },
            { Piece.WhiteKnight, LoadImage("/Assets/custom/w_knight") },
            { Piece.WhiteBishop, LoadImage("/Assets/custom/w_bishop") },
            { Piece.WhiteQueen, LoadImage("/Assets/custom/w_queen") },
            { Piece.WhiteKing, LoadImage("/Assets/custom/w_king") },

            { Piece.BlackPawn, LoadImage("/Assets/custom/b_pawn2") },
            { Piece.BlackRook, LoadImage("/Assets/custom/b_rook") },
            { Piece.BlackKnight, LoadImage("/Assets/custom/b_knight") },
            { Piece.BlackBishop, LoadImage("/Assets/custom/b_bishop") },
            { Piece.BlackQueen, LoadImage("/Assets/custom/b_queen") },
            { Piece.BlackKing, LoadImage("/Assets/custom/b_king") },
        };

        public static Dictionary<int, ImageSource> GetImages(ImagesSet set)
        {
            if (set == ImagesSet.Normal) return sources;
            if (set == ImagesSet.Custom) return sources2;
            return null;
        }


        private static ImageSource LoadImage(string fileName)
        {
            return new BitmapImage(new Uri($"{fileName}.png", UriKind.Relative));
        }
    }
}
