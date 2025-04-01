using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessUI.Misc
{
    public enum GameMode
    {
        None,
        TwoPlayers,
        PlayerMinimax,
        Server,
        Client,
        Custom,
    }

    public enum GameState
    {
        InProgress,
        WhiteWon,
        BlackWon,
        Stalemate,
        DrawRepetition,
        FiftyMovesRule,
        InsufficientMaterial,
        OutOfTimeWhite,
        OutOfTimeBlack,
    }
}
