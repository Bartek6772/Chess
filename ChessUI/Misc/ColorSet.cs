using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChessEngine;
namespace ChessUI.Misc
{
    class ColorSet
    {
        public SolidColorBrush whiteColor;
        public SolidColorBrush blackColor;
        public SolidColorBrush highlightColor;
        public SolidColorBrush lastMoveColor;

        public ColorSet(ColorCode white, ColorCode black, ColorCode highlight, ColorCode lastMove)
        {
            whiteColor = white.GetBrush();
            blackColor = black.GetBrush();
            highlightColor = highlight.GetBrush();
            lastMoveColor = lastMove.GetBrush();
        }

        public static List<ColorSet> colorSets = new() {
            new ColorSet(new(121, 72, 57), new(93, 50, 49), new(100, 219, 65, 48), new(100, 245, 190, 39)),
            new ColorSet(new(235, 236, 208), new(115, 149, 82), new(180, 219, 65, 48), new(149, 237, 209, 26)),
            new ColorSet(new(232, 235, 239), new(125, 135, 150), new(160, 219, 65, 48), new(149, 237, 209, 26)),
        };
    }


    public struct ColorCode
    {
        byte r, g, b, a;

        public ColorCode(byte a, byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public ColorCode(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 255;
        }

        public SolidColorBrush GetBrush() => new SolidColorBrush(Color.FromArgb(a, r, g, b));
    }

    public enum ImagesSet
    {
        Normal,
        Custom
    }
}
