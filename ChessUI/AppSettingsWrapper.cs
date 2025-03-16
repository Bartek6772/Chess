using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessUI
{
    class AppSettingsWrapper
    {
        public bool AIEnabled {
            get => AppSettings.AIEnabled;
            set => AppSettings.AIEnabled = value;
        }
    }
}
