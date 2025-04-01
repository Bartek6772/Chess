using ChessUI.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChessUI.Dialogs.Settings
{
    class SettingsViewModel : DialogViewModelBase<Settings>
    {
        public ICommand PlayerPlayer { get; private set; }
        public ICommand PlayerMinimax { get; private set; }
        public ICommand Host { get; private set; }
        public ICommand Client { get; private set; }

        private Settings settings;

        public string FEN {
            get => settings.fen;
            set => settings.fen = value;
        }

        public AppSettings app {
            get => AppSettings.Instance;
        }
        public SettingsViewModel(string title) : base(title)
        {
            settings = new();
            PlayerPlayer = new RelayCommand<IDialogWindow>(PP);
            PlayerMinimax = new RelayCommand<IDialogWindow>(PM);
            Host = new RelayCommand<IDialogWindow>(H);
            Client = new RelayCommand<IDialogWindow>(C);
        }

        public void PP(IDialogWindow window)
        {
            settings.mode = GameMode.TwoPlayers;
            CloseDialogWithResult(window, settings);
        }

        public void PM(IDialogWindow window)
        {
            settings.mode = GameMode.PlayerMinimax;
            CloseDialogWithResult(window, settings);
        }

        public void H(IDialogWindow window)
        {
            settings.mode = GameMode.Server;
            CloseDialogWithResult(window, settings);
        }

        public void C(IDialogWindow window)
        {
            settings.mode = GameMode.Client;
            CloseDialogWithResult(window, settings);
        }
    }

    public struct Settings
    {
        public string fen;
        public GameMode mode;
    }
}
