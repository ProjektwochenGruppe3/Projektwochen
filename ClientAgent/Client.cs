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
using System.IO;

namespace ClientAgent
{
    class Client
    {

        public Client(IPAddress ip, int inputPort, string myName)
        {
            this.ipAdress = ip;
            this.port = inputPort;
            this.MyGuid = Guid.NewGuid();
            this.MyName = myName;
            this.Listener = new TcpListener(IPAddress.Any, 47474);
            this.timer = new System.Timers.Timer();
            this.timer.AutoReset = true;
            this.TaskList = new List<AtomicJob>();
            this.ExecutableJobs = new List<AtomicJob>();
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

        private IPAddress ipAdress;

        private int port;

        public List<AtomicJob> TaskList { get; set; }

        public bool SendDataAvailable { get; private set; }

        public string Message { get; private set; }

        public bool Alive { get; private set; }

        public ClientState State { get; private set; }

        public bool Waiting { get; private set; }

        public List<AtomicJob> ExecutableJobs { get; set; }

        public void SendKeepAliveResponse()
        {
            try
            {
                byte[] buffer = new byte[100];
                NetworkStream netStream = this.ClientTCP.GetStream();
                while (this.Alive)
                {
                    Networking.RecievePackage(netStream);
                    AgentStatus response = new AgentStatus(this.firstKeepAliveGuid, this.MyGuid, this.MyName, CPU_Diagnostic.GetCPUusage());
                    Networking.SendPackage(response, netStream);
                    Thread.Sleep(5000);
                }
            }
            catch
            {
                Console.WriteLine("Server has disconnected!");
                this.State = ClientState.connecting;
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
                        this.ClientTCP.Connect(this.ipAdress, this.port);
                        netStream = this.ClientTCP.GetStream();
                    }
                    else if (netStream.DataAvailable)
                    {
                        object receivedObj = Networking.RecievePackage(netStream);
                        AgentStatusRequest request = (AgentStatusRequest)receivedObj;
                        this.firstKeepAliveGuid = request.KeepAliveRequestGuid;
                        this.MyName = request.KeepAliveRequestGuid.ToString() + "_Agent";
                        AgentStatus response = new AgentStatus(request.KeepAliveRequestGuid, this.MyGuid, this.MyName, CPU_Diagnostic.GetCPUusage());
                        Networking.SendPackage(response, netStream);
                        this.Waiting = false;
                        this.State = ClientState.connected;
                    }
                }
                catch
                {
                }

                Thread.Sleep(10);
            }
        }

        public void ListenerWorker()
        {
            Thread executionThread = new Thread(new ThreadStart(ExecutionWorker));
            executionThread.Start();
            while (this.ListenerActive)
            {
                if (this.Listener.Pending())
                {
                    Thread DLL_Loading_Thread = new Thread(new ParameterizedThreadStart(DLL_Worker));
                    AtomicJob atjob = new AtomicJob(DLL_Loading_Thread, this.Listener.AcceptTcpClient());
                    this.TaskList.Add(atjob);
                    atjob.ExecutableThread.Start(atjob);
                }
                Thread.Sleep(70);
            }
        }

        public void DLL_Worker(object atomicJob)
        {
            AtomicJob job = (AtomicJob)atomicJob;
            ExecutableHandler handler = new ExecutableHandler();
            NetworkStream dllStream = job.Server.GetStream();
            string jobDllPath = Path.Combine(Environment.CurrentDirectory, job.AtJobGuid.ToString() + ".dll");
            job.OnExecutableResultsReady += handler.WriteResultToStream;
            bool inExecutableList = false;
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
                            catch (Exception e)
                            {
                                job.State = Core.Network.JobState.Exception;
                                job.Result = new List<string>() { e.Message };
                                job.FireOnExecutableResultsReady(dllStream);
                            }
                        }
                        else if (para != null && !inExecutableList)
                        {
                            job.Params = para.Parameters;
                            this.ExecutableJobs.Add(job);
                            inExecutableList = true;
                        }
                    }
                }
                catch
                {
                    job.InProgress = false;
                }
                Thread.Sleep(50);
            }
            try
            {
                Console.Clear();
                handler.DeleteFile(jobDllPath);
            }
            catch
            {

            }
        }

        public void ExecutionWorker()
        {
            while (true)
            {
                if (this.ExecutableJobs.Count != 0)
                {
                    try
                    {
                        this.ExecutableJobs[0].Result = ComponentExecuter.InvokeMethod(this.ExecutableJobs[0]);
                        this.ExecutableJobs[0].State = Core.Network.JobState.Ok;
                        this.ExecutableJobs[0].FireOnExecutableResultsReady(this.ExecutableJobs[0].Server.GetStream());
                        this.ExecutableJobs.Remove(this.ExecutableJobs[0]);
                    }
                    catch (Exception e)
                    {
                        this.ExecutableJobs[0].State = Core.Network.JobState.Exception;
                        this.ExecutableJobs[0].Result = new List<string>() { e.Message };
                        this.ExecutableJobs[0].FireOnExecutableResultsReady(this.ExecutableJobs[0].Server.GetStream());
                        this.ExecutableJobs.Remove(this.ExecutableJobs[0]);
                    }
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
            Console.WriteLine("Trying to connect...");
            this.ClientThread = new Thread(new ThreadStart(FirstConnect));
            this.ClientThread.Start();
            this.ClientThread.Join();
            if (this.State == ClientState.connected)
            {
                Console.WriteLine("Connected...");
                this.ClientThread = new Thread(new ThreadStart(SendKeepAliveResponse));
                this.ClientThread.Start();
                this.Listener.Start();
                this.ListenerActive = true;
                this.ListenerThread = new Thread(new ThreadStart(ListenerWorker));
                this.ListenerThread.Start();
            }
            else
            {
                this.ListenerActive = false;
                this.ListenerThread.Join();
                Environment.Exit(0);
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
