using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessUI
{
    class AppSettings : INotifyPropertyChanged
    {
        private static readonly Lazy<AppSettings> _instance = new(() => new AppSettings());
        public static AppSettings Instance => _instance.Value;

        private bool _AIEnabled = true;
        private bool _moveOrderingEnabled = true;
        private int _searchTimeLimit = 5000;
        private int _searchDepth = 5;
        private string _bookMove = "Make move to refresh";
        private ulong _zobristHash;

        public bool AIEnabled {
            get => _AIEnabled;
            set {
                _AIEnabled = value;
                OnPropertyChanged(nameof(AIEnabled));
            }
        }

        public bool MoveOrderingEnabled {
            get => _moveOrderingEnabled;
            set {
                _moveOrderingEnabled = value;
                OnPropertyChanged(nameof(MoveOrderingEnabled));
            }
        }

        public int SearchTimeLimit {
            get => _searchTimeLimit;
            set {
                _searchTimeLimit = value;
                OnPropertyChanged(nameof(SearchTimeLimit));
            }
        }

        public int SearchDepth {
            get => _searchDepth;
            set {
                _searchDepth = value;
                OnPropertyChanged(nameof(SearchDepth));
            }
        }

        public string BookMove {
            get => _bookMove;
            set {
                _bookMove = value;
                OnPropertyChanged(nameof(BookMove));
            }
        }

        public ulong ZobristHash {
            get => _zobristHash;
            set {
                _zobristHash = value;
                OnPropertyChanged(nameof(ZobristHash));
            }
        }


        public ObservableCollection<string> logs = new ObservableCollection<string>();


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum GameMode
    {
        TwoPlayers,
        PlayerMinimax
    }
}
