using ChessUI.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChessUI.Dialogs.Alert
{
    class AlertDialogViewModel : DialogViewModelBase<DialogResults>
    {
        public ICommand OKCommand { get; private set; }
        public AlertDialogViewModel(string title, string message) : base(title, message)
        {
            OKCommand = new RelayCommand<IDialogWindow>(OK);
        }

        public void OK(IDialogWindow window)
        {
            CloseDialogWithResult(window, DialogResults.None);
        }
    }
}
