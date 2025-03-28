using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessUI.Dialogs
{
    class DialogService
    {
        public T OpenDialog<T>(DialogViewModelBase<T> viewModel)
        {
            IDialogWindow window = new DialogWindow();
            window.DataContext = viewModel;
            window.ShowDialog();
            return viewModel.DialogResult;
        }
    }

    public interface IDialogWindow
    {
        bool? DialogResult { get; set; }
        object DataContext { get; set; }
        bool? ShowDialog();
    }

    public enum DialogResults
    {
        None,
        Yes,
        No,
    }
}
