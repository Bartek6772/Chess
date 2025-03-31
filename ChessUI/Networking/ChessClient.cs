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
    public class ChessClient : NetworkBase
    {
        private TcpClient client;

        public void ConnectToServer(string ipAddress)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try {
                client = new TcpClient(ipAddress, 5000);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                Debug.WriteLine("Connected to server!");

                stream = client.GetStream();
                Thread receiveThread = new Thread(ReceiveMessages);
                receiveThread.Start();
            }
            catch (Exception ex) {
                MessageBox.Show("Error connecting to server: " + ex.Message);
            }
        }
    }
}
