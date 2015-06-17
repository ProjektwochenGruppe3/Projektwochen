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
            this.MyGuid = new Guid();
            this.Listener = new TcpListener(IPAddress.Any, 47474);
        }

        public Guid MyGuid { get; private set; }

        public TcpClient ClientTCP { get; private set; }

        public TcpListener Listener { get; set; }

        public bool ListenerActive { get; set; }

        private Thread ClientThread;

        private Thread ListenerThread;

        private string ipAdress;

        private int port;

        public List<AtomicJob> TaskList { get; set; }

        public bool SendDataAvailable { get; private set; }

        public string Message { get; private set; }

        public bool Alive { get; private set; }

        public ClientState State { get; private set; }

        public bool Waiting { get; private set; }

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
            Console.WriteLine("Trying to connect...");
            NetworkStream netStream = this.ClientTCP.GetStream();
            while (this.Waiting)
            {
                if (netStream.DataAvailable)
                {
                    Console.WriteLine("Data available...");
                    object receivedObj = Networking.RecievePackage(netStream);
                    Console.WriteLine("Data received...");
                    AgentKeepAliveRequest request = (AgentKeepAliveRequest)receivedObj;
                    AgentKeepAliveResponse response = new AgentKeepAliveResponse(request.KeepAliveRequestGuid, this.MyGuid,request.KeepAliveRequestGuid.ToString() + "_Agent",CPU_Diagontic.GetCPULoad());
                    Networking.SendPackage(response, netStream);
                    Console.WriteLine("Data sent...");
                }
            }
        }

        public void ListenerWorker()
        {
            while (this.ListenerActive)
            {
                if (this.Listener.Pending())
                {
                    Thread DLL_Loading_Thread = new Thread(new ParameterizedThreadStart(DLL_Worker));
                    DLL_Loading_Thread.Start(this.Listener.AcceptTcpClient());
                }
            }
        }

        public void DLL_Worker(object serverRaw)
        {
            TcpClient server = (TcpClient)serverRaw;
            NetworkStream dllStream = server.GetStream();
            while (this.Waiting)
            {
                if (dllStream.DataAvailable)
                {
                    object data = Networking.RecievePackage(dllStream);
                    AtomicJob job = new AtomicJob(ComponentExecuter.GetAssembly(data));
                    this.TaskList.Add(job);
                }
            }
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
