using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessUI
{
    public enum GameMode
    {
        TwoPlayers,
        PlayerMinimax,
        Sever,
        Client
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
