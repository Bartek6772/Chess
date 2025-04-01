using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ChessUI.Misc;

namespace ChessUI
{
    partial class ChessboardView
    {
        List<int> whiteCaptures;
        List<int> blackCaptures;

        public void RefreshCaptures()
        {
            whiteCaptures.Sort((a, b) => b.CompareTo(a));
            blackCaptures.Sort((a, b) => b.CompareTo(a));

            WhiteCaptures.Children.Clear();
            BlackCaptures.Children.Clear();

            List<int> list1 = rotated ? blackCaptures : whiteCaptures;
            List<int> list2 = rotated ? whiteCaptures : blackCaptures;

            foreach (var piece in list1) {
                Image image = new Image();
                image.Source = Images.GetImages(imageSet)[piece];
                image.Height = 30;

                WhiteCaptures.Children.Add(image);
            }

            foreach (var piece in list2) {
                Image image = new Image();
                image.Source = Images.GetImages(imageSet)[piece];
                image.Height = 30;

                BlackCaptures.Children.Add(image);
            }
        }
    }
}
