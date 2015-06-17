using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Timers;
using dcs.core;

namespace ClientAgent
{
    class Client
    {

        public Client(string ip, int inputPort)
        {
            this.ipAdress = ip;
            this.port = inputPort;
        }

        public TcpClient ClientTCP { get; set; }

        private Thread ClientThread;

        private string ipAdress;

        private int port;

        public bool SendDataAvailable { get; set; }

        public string Message { get; set; }

        public bool Alive { get; set; }

        public ClientState State { get; set; }

        public bool Waiting { get; set; }

        public void ClientWorker()
        {
            NetworkStream netStream = this.ClientTCP.GetStream();
            byte[] receiveBuffer = new byte[500];
            byte[] sendBuffer = new byte[500];
            while (this.Alive)
            {
                if (netStream.DataAvailable)
                {
                }
                if (this.SendDataAvailable)
                {
                }
                Thread.Sleep(50);
            }
        }

        public void FirstConnect()
        {
            NetworkStream netStream = this.ClientTCP.GetStream();
            while (this.Waiting)
            {
                if (netStream.DataAvailable)
                {
                    object receivedObj = Networking.RecievePackage(netStream);
                    AgentKeepAliveRequest request = (AgentKeepAliveRequest)receivedObj;
                    
                }
            }
        }

        public object GetObject(NetworkStream netStream)
        {
            object obj = new object();
            return obj;
        }

        public void Connect()
        {
            this.Alive = true;
            this.ClientTCP = new TcpClient();
            this.ClientTCP.Connect(IPAddress.Parse(this.ipAdress), this.port);
            this.State = ClientState.connecting;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000;
            this.ClientThread = new Thread(new ThreadStart(FirstConnect));
            this.ClientThread.Start();
            this.ClientThread.Join();
            if (this.State == ClientState.connected)
            {
                this.ClientThread = new Thread(new ThreadStart(ClientWorker));
                this.ClientThread.Start();
            }
            else
            {
                Console.WriteLine("Error while first connect to server!");
            }
        }

        public void TimerCallback()
        {
            if (this.Waiting)
            {
                this.Waiting = false;
            }
        }
    }
}
