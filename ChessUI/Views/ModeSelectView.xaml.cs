using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChessUI
{
    /// <summary>
    /// Logika interakcji dla klasy ModeSelectView.xaml
    /// </summary>
    public partial class ModeSelectView : UserControl
    {
        public ModeSelectView()
        {
            InitializeComponent();
        }

        private void Button_PP_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow) {
                mainWindow.SwitchToGame(GameMode.TwoPlayers);
            }
        }

        private void Button_PM_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow) {
                mainWindow.SwitchToGame(GameMode.PlayerMinimax);
            }
        }
    }
}
