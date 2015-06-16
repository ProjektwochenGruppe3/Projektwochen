using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ClientAgent_PW_Cerima_Koini_V1
{
    class Client
    {
        public TcpClient ClientTCP { get; set; }

        private Thread ClientThread;

        public bool SendDataAvailable { get; set; }

        public string Message { get; set; }

        public bool Alive { get; set; }

        public void SendMessageToServer()
        {
            this.Message = Console.ReadLine();
            this.SendDataAvailable = true;
        }

        public void Worker()
        {
            NetworkStream netStream = this.ClientTCP.GetStream();
            byte[] receiveBuffer = new byte[500];
            byte[] sendBuffer = new byte[500];
            while (this.Alive)
            {
                if (netStream.DataAvailable)
                {
                    int readLength = netStream.Read(receiveBuffer, 0, receiveBuffer.Length);
                    string message = Encoding.UTF8.GetString(receiveBuffer, 0, readLength);
                    netStream.Flush();
                    Console.WriteLine(message);
                }
                if (this.SendDataAvailable)
                {
                    sendBuffer = Encoding.UTF8.GetBytes(this.Message);
                    netStream.Write(sendBuffer, 0, sendBuffer.Length);
                    this.SendDataAvailable = false;
                }
                Thread.Sleep(50);
            }
        }

        public void Connect(string ip, int port)
        {
            this.Alive = true;
            this.ClientTCP = new TcpClient();
            this.ClientTCP.Connect(IPAddress.Parse(ip), port);
            this.ClientThread = new Thread(new ThreadStart(Worker));
            this.ClientThread.Start();
        }
    }
}
