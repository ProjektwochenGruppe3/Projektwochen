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
            this.timer = new System.Timers.Timer();
            this.timer.AutoReset = true;
        }

        private System.Timers.Timer timer;

        private Guid firstKeepAliveGuid;

        private string MyName;

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

        public void SendKeepAliveResponse()
        {
            NetworkStream netStream = this.ClientTCP.GetStream();
            while (this.Alive)
            {
                AgentKeepAliveResponse response = new AgentKeepAliveResponse(this.firstKeepAliveGuid, this.MyGuid, request.KeepAliveRequestGuid.ToString() + "_Agent", 75);
                Networking.SendPackage(response, netStream);
                Thread.Sleep(3000);
            }
        }

        public void FirstConnect()
        {
            NetworkStream netStream = null;

            while (this.Waiting)
            {
                try
                {
                    if (netStream == null)
                    {
                        Console.WriteLine("Trying to connect...");
                        this.ClientTCP.Connect(IPAddress.Parse(this.ipAdress), this.port);
                        netStream = this.ClientTCP.GetStream();
                    }
                    else if (netStream.DataAvailable)
                    {
                        Console.WriteLine("Data available...");
                        object receivedObj = Networking.RecievePackage(netStream);
                        Console.WriteLine("Data received...");
                        AgentKeepAliveRequest request = (AgentKeepAliveRequest)receivedObj;
                        this.firstKeepAliveGuid = request.KeepAliveRequestGuid;
                        this.MyName = request.KeepAliveRequestGuid.ToString() + "_Agent";
                        AgentKeepAliveResponse response = new AgentKeepAliveResponse(request.KeepAliveRequestGuid, this.MyGuid, this.MyName, 75);
                        Networking.SendPackage(response, netStream);
                        Console.WriteLine("Data sent...");
                        this.Waiting = false;
                        this.State = ClientState.connected;
                    }
                }
                catch
                {
                    Console.WriteLine("Connection Failed!");
                }

                Thread.Sleep(10);
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
            this.Waiting = true;
            this.ClientTCP = new TcpClient();
            this.State = ClientState.connecting;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000;
            this.ClientThread = new Thread(new ThreadStart(FirstConnect));
            this.ClientThread.Start();
            this.ClientThread.Join();
            if (this.State == ClientState.connected)
            {
                this.ClientThread = new Thread(new ThreadStart(SendKeepAliveResponse));
                this.ClientThread.Start();
            }
            else
            {
                Console.WriteLine("Error: Connection timer ran out!");
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
