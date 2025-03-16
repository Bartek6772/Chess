using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChessUI
{
    partial class ChessboardView
    {
        public ObservableCollection<HistoryObject> MoveHistory { get; set; }

        public class HistoryObject : INotifyPropertyChanged
        {
            private int _moveNumber;
            private string _moveWhite;
            private string _moveBlack;

            public int MoveNumber {
                get => _moveNumber;
                set {
                    _moveNumber = value;
                    OnPropertyChanged(nameof(MoveNumber));
                }
            }

            public string MoveBlack {
                get => _moveBlack;
                set {
                    _moveBlack = value;
                    OnPropertyChanged(nameof(MoveBlack));
                }
            }

            public string MoveWhite {
                get => _moveWhite;
                set {
                    _moveWhite = value;
                    OnPropertyChanged(nameof(MoveWhite));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ListView_ScrollToBottom(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add) // When a new item is added
            {
                if (HistoryListView.Items.Count == 0) return;

                Dispatcher.InvokeAsync(() =>
                {
                    HistoryListView.ScrollIntoView(HistoryListView.Items[HistoryListView.Items.Count - 1]);

                }, System.Windows.Threading.DispatcherPriority.Background);

            }
        }
    }
}
