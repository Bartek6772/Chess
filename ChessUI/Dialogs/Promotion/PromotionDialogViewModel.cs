using ChessEngine;
using ChessUI.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChessUI.Dialogs.Promotion
{
    class PromotionDialogViewModel : DialogViewModelBase<Move.Flags>
    {
        public ICommand QueenCommand { get; private set; }
        public ICommand BishopCommand { get; private set; }
        public ICommand RookCommand { get; private set; }
        public ICommand KnightCommand { get; private set; }
        public Visibility IsWhite { get; set; }
        public Visibility IsBlack { get; set; }

        public PromotionDialogViewModel(string title, string message) : base(title, message)
        {
            QueenCommand = new RelayCommand<IDialogWindow>(Queen);
            RookCommand = new RelayCommand<IDialogWindow>(Rook);
            BishopCommand = new RelayCommand<IDialogWindow>(Bishop);
            KnightCommand = new RelayCommand<IDialogWindow>(Knight);

            IsWhite = message == "white" ? Visibility.Visible : Visibility.Hidden;
            IsBlack = message != "white" ? Visibility.Visible : Visibility.Hidden;
        }


        public void Queen(IDialogWindow window) => CloseDialogWithResult(window, Move.Flags.PromotionQueen);
        public void Rook(IDialogWindow window) => CloseDialogWithResult(window, Move.Flags.PromotionRook);
        public void Bishop(IDialogWindow window) => CloseDialogWithResult(window, Move.Flags.PromotionBishop);
        public void Knight(IDialogWindow window) => CloseDialogWithResult(window, Move.Flags.PromotionKnight);
    }
}
