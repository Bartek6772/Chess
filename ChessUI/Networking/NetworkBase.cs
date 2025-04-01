using ChessEngine;
using ChessUI.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChessUI.Networking
{
    public class NetworkBase
    {
        protected NetworkStream stream;
        public Action<Move> onMove;
        public Action<GameState> onGameStateUpdated;
        public Action<int, int> onTimeUpdated;

        public void SendMove(Move move)
        {
            GameMessage message = new GameMessage {
                Type = MessageType.Move,
                ChessMove = move
            };
            SendMessage(message);
        }

        public void SendGameState(GameState state)
        {
            GameMessage message = new GameMessage {
                Type = MessageType.GameState,
                GameState = state
            };
            SendMessage(message);
        }

        public void SendTime(int white, int black)
        {
            GameMessage message = new GameMessage {
                Type = MessageType.Time,
                timeWhite = white,
                timeBlack = black
            };
            SendMessage(message);
        }

        protected void SendMessage(GameMessage message)
        {
            if (stream != null) {
                string json = message.Serialize();
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        protected void ReceiveMessages()
        {

            byte[] buffer = new byte[1024];
            while (true) {

                if (stream == null) {
                    Debug.WriteLine("problem");
                    continue;
                }

                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                GameMessage message = GameMessage.Deserialize(json);

                if (message.Type == MessageType.Move && message.ChessMove.HasValue) {
                    Move move = message.ChessMove.Value;
                    onMove?.Invoke(move);
                }
                else if (message.Type == MessageType.GameState) {
                    onGameStateUpdated?.Invoke(message.GameState);
                }
                else if(message.Type == MessageType.Time) {
                    onTimeUpdated?.Invoke(message.timeWhite, message.timeBlack);
                }

            }
        }
    }

    public enum MessageType { 
        Move,
        GameState,
        Time,
    }

    public struct GameMessage
    {
        [JsonInclude] public MessageType Type;
        [JsonInclude] public Move? ChessMove;
        [JsonInclude] public GameState GameState;
        [JsonInclude] public int timeWhite;
        [JsonInclude] public int timeBlack;

        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }

        public static GameMessage Deserialize(string json)
        {
            return JsonSerializer.Deserialize<GameMessage>(json);
        }
    }
}
