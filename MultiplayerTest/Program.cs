using ChessUI.Networking;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.WriteLine("Multiplayer Test");

//Test test = new() { a = 5, b = 10 };
//string json = JsonSerializer.Serialize(test);
//Console.WriteLine(json);

ChessServer server = new ChessServer();
server.StartServer();

ChessClient client = new ChessClient();
client.ConnectToServer("192.168.56.1");

//server.SendGameState(ChessUI.GameState.Stalemate);
//server.SendMove(new ChessEngine.Move() { StartSquare = 1, TargetSquare = 18 });
//client.SendMove(new ChessEngine.Move() { StartSquare = 11, TargetSquare = 19, Flag = ChessEngine.Move.Flags.DoublePush });


struct Test
{
    [JsonInclude] public int a;
    [JsonInclude] public int b;
}
