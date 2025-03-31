using ChessUI.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChessUI.Dialogs.Network
{
    class NetworkViewModel : DialogViewModelBase<string>
    {
        public ICommand OKCommand { get; private set; }
        public string IP { get; set; }

        public NetworkViewModel(string title) : base(title, "Podaj adres IP")
        {
            OKCommand = new RelayCommand<IDialogWindow>(OK);
        }

        public void OK(IDialogWindow window)
        {
            CloseDialogWithResult(window, IP);
        }
    }
}
