using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChessUI.Networking
{
    public class ChessServer : NetworkBase
    {
        private TcpListener server;
        private TcpClient client;
        public Action onClientConnected;

        public void StartServer()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try {
                server = new TcpListener(IPAddress.Any, 5000);
                server.Start();
                Debug.WriteLine("Server started. Waiting for connection...");

                Thread acceptThread = new Thread(AcceptClient);
                acceptThread.Start();
            }
            catch (Exception ex) {
                MessageBox.Show("Error starting server: " + ex.Message);
            }
        }

        private void AcceptClient()
        {
            client = server.AcceptTcpClient();
            Debug.WriteLine("Client connected!");
            onClientConnected?.Invoke();

            stream = client.GetStream();
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
        }
    }
}
