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
            CPU_Diagnostic.InitialisierePerformanceCounter();
            this.Connect();
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
            try
            {
                NetworkStream netStream = this.ClientTCP.GetStream();
                while (this.Alive)
                {
                    AgentStatus response = new AgentStatus(this.firstKeepAliveGuid, this.MyGuid, this.MyName, CPU_Diagnostic.GetCPUusage());
                    Networking.SendPackage(response, netStream);
                    Thread.Sleep(3000);
                }
            }
            catch
            {
                Console.WriteLine("Server has disconnected!");
                this.Connect();
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
                        AgentStatusRequest request = (AgentStatusRequest)receivedObj;
                        this.firstKeepAliveGuid = request.KeepAliveRequestGuid;
                        this.MyName = request.KeepAliveRequestGuid.ToString() + "_Agent";
                        AgentStatus response = new AgentStatus(request.KeepAliveRequestGuid, this.MyGuid, this.MyName, CPU_Diagnostic.GetCPUusage());
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
                    AtomicJob atjob = new AtomicJob(DLL_Loading_Thread, this.Listener.AcceptTcpClient());
                    this.TaskList.Add(atjob);
                    atjob.ExecutableThread.Start(atjob);
                }
            }
        }

        public void DLL_Worker(object atomicJob)
        {
            AtomicJob job = (AtomicJob)atomicJob;
            ExecutableHandler handler = new ExecutableHandler();
            NetworkStream dllStream = job.Server.GetStream();
            string jobDllPath = Environment.CurrentDirectory + "\\Job_Dlls\\" + job.AtJobGuid + ".dll";
            job.OnExecutableResultsReady += handler.WriteResultToStream;
            while (job.InProgress)
            {
                try
                {
                    if (dllStream.DataAvailable)
                    {
                        object data = Networking.RecievePackage(dllStream);
                        AgentExecutable exe = data as AgentExecutable;
                        AgentExecutableParameters para = data as AgentExecutableParameters;
                        if (exe != null)
                        {
                            try
                            {
                                handler.WriteToFile(jobDllPath, exe.Assembly);
                                job.ExecutableType = ComponentExecuter.GetTypeFromAssembly(ComponentExecuter.GetAssemblyFromDll(jobDllPath));
                            }
                            catch
                            {
                                job.State = Core.Network.JobState.Exception;
                                job.Result = null;
                                job.FireOnExecutableResultsReady(dllStream);
                            }
                        }
                        else if (para != null)
                        {
                            try
                            {
                                job.Params = para.Parameters;
                                job.Result = ComponentExecuter.InvokeMethod(job);
                                job.FireOnExecutableResultsReady(dllStream);
                            }
                            catch
                            {
                                job.State = Core.Network.JobState.Exception;
                                job.Result = null;
                                job.FireOnExecutableResultsReady(dllStream);
                            }
                        }
                    }
                }
                catch
                {
                    job.InProgress = false;
                    handler.DeleteFile(jobDllPath);
                }
                Thread.Sleep(50);
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
