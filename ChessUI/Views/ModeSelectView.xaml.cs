using ChessUI.Dialogs;
using ChessUI.Dialogs.Settings;
using ChessUI.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private DialogService service;
        public ModeSelectView()
        {
            service = new DialogService();
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

        private void Button_HG_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow) {
                mainWindow.SwitchToGame(GameMode.Server);
            }
        }

        private void Button_JG_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow) {
                mainWindow.SwitchToGame(GameMode.Client);
            }
        }

        private void Button_Custom_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow) {

                Settings settings = service.OpenDialog(new SettingsViewModel("Ustawienia gry"));
                if(settings.mode != GameMode.None) {
                    mainWindow.SwitchToGame(settings.mode, settings.fen);
                }
            }
        }
    }
}
