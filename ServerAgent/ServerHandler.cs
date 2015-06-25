using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Network;
using dcs.core;
using Newtonsoft.Json;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class ServerHandler
    {
        private static readonly object SyncRoot = new object();

        private List<RemoteServer> remoteservers;

        public ServerHandler(Server server)
        {
            this.RemoteServers = new List<RemoteServer>();
            this.MyGuid = Guid.NewGuid();
            this.Listener = new TcpListener(IPAddress.Any, 10000);
            this.ListenerThread = new Thread(new ThreadStart(this.ListenerWorker));
            //this.ListenerThread.Start();
            this.ListenerThread.IsBackground = true;
            this.MyFriendlyName = "EG42";
            this.Server = server;
            this.Timer = new System.Timers.Timer(30000);
            //this.Timer.Start();
            this.Timer.AutoReset = true;
            this.Timer.Elapsed += this.OnTimerElapsed;
            this.BroadcastTimer = new System.Timers.Timer(180000);
            this.BroadcastTimer.AutoReset = true;
            this.BroadcastTimer.Elapsed += OnBroadcastTimer_Elapsed;
            //this.BroadcastTimer.Start();

            this.UdpListenerThread = new Thread(new ThreadStart(this.UdpListenerWorker));
            this.UdpListenerThread.IsBackground = true;
            this.UdpListenerThread.Start();
        }

        public Guid MyGuid { get; set; }

        public string MyFriendlyName { get; set; }

        public List<RemoteServer> RemoteServers
        {
            get
            {
                lock (SyncRoot)
                {
                    return this.remoteservers;
                }
            }

            set
            {
                lock (SyncRoot)
                {
                    this.remoteservers = value;
                }
            }
        }

        public List<Component> GetRemoteComponents()
        {
            List<Component> components = new List<Component>();

            foreach (var server in this.RemoteServers)
            {
                foreach (var item in server.RemoteComponents)
                {
                    if (!components.Any(x => x.ComponentGuid == item.ComponentGuid))
                    {
                        components.Add(item);
                    }
                }
            }

            return components;
        }

        public List<ClientInfo> GetRemoteClients()
        {
            List<ClientInfo> clients = new List<ClientInfo>();

            foreach (var server in this.RemoteServers)
            {
                foreach (var item in server.RemoteClients)
                {
                    clients.Add(item);
                }
            }

            return clients;
        }

        private TcpListener Listener { get; set; }

        private Thread ListenerThread { get; set; }

        private Thread UdpListenerThread { get; set; }

        private Server Server { get; set; }

        private System.Timers.Timer Timer { get; set; }

        private System.Timers.Timer BroadcastTimer { get; set; }

        private void ListenerWorker()
        {
            this.Listener.Start();

            this.SendBroadcast();

            while (true)
            {
                TcpClient newServer = this.Listener.AcceptTcpClient();

                Thread t = new Thread(new ParameterizedThreadStart(this.RequestWorker));
                t.Start(newServer);

                Thread.Sleep(100);
            }
        }

        private void UdpListenerWorker()
        {
            while (true)
            {
                UdpClient listener = new UdpClient();
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 10001);

                try
                {
                    byte[] data = listener.Receive(ref ip);

                    if (Encoding.UTF8.GetString(data) == "PWSP")
                    {
                        this.SendLogonRequest(ip.Address);
                    }
                }
                catch
                {
                }

                Thread.Sleep(42);
            }
        }

        private void SendBroadcast()
        {
            try
            {
                IPEndPoint broadcastAddress = new IPEndPoint(IPAddress.Broadcast, 10001);
                UdpClient client = new UdpClient(broadcastAddress);
                client.EnableBroadcast = true;

                byte[] pack = Encoding.UTF8.GetBytes("PWSP");
                client.Send(pack, pack.Length);
            }
            catch
            {
            }
        }

        private void RequestWorker(object serverTcp)
        {
            TcpClient newServer = (TcpClient)serverTcp;
            IPAddress serverIP = ((IPEndPoint)newServer.Client.RemoteEndPoint).Address;

            NetworkStream ns = newServer.GetStream();

            while (true)
            {
                try
                {
                    if (ns.DataAvailable)
                    {
                        ServerPackage pack = Networking.RecieveServerPackage(ns);

                        ns.Close();
                        ns.Dispose();
                        newServer.Close();

                        this.HandlePackage(pack, newServer);
                    }
                }
                catch
                {
                }

                Thread.Sleep(42);
            }
        }

        private void HandlePackage(ServerPackage pack, TcpClient senderTcp)
        {
            switch (pack.MessageCode)
            {
                case MessageCode.ClientUpdate:
                    this.RecieveClientUpdateRequest(pack.Payload, senderTcp);
                    break;
                case MessageCode.ComponentSubmit:
                    this.RecieveComponentSubmit(pack.Payload, senderTcp);
                    break;
                case MessageCode.JobRequest:
                    this.RecieveJobRequest(pack.Payload, senderTcp);
                    break;
                case MessageCode.JobResult:

                    break;
                case MessageCode.KeepAlive:
                    this.RecieveKeepAlive(pack.Payload, senderTcp);
                    break;
                case MessageCode.Logon:
                    this.NewServerLogon(pack.Payload, senderTcp);
                    break;
                case MessageCode.RequestAssembly:
                    this.RecieveAssemblyRequest(pack.Payload, senderTcp);
                    break;
                default:
                    return;
            }
        }

        private void SendLogonRequest(IPAddress ip)
        {
            LogonRequest request = new LogonRequest() { ServerGuid = this.MyGuid, FriendlyName = this.MyFriendlyName, LogonRequestGuid = Guid.NewGuid(), AvailableClients = ServerOperations.GetClientInfos(this.Server.Clients.ToList()), AvailableComponents = this.Server.LocalComponents };

            string json = JsonConvert.SerializeObject(request);

            TcpClient client = new TcpClient();

            try
            {
                client.Connect(new IPEndPoint(ip, 10000));
                NetworkStream ns = client.GetStream();

                ServerPackage package = new ServerPackage(MessageCode.Logon, json);
                Networking.SendServerPackage(package, ns);

                int counter = 0;
                while (counter < 30000)
                {
                    if (ns.DataAvailable)
                    {
                        LogonResponse response = JsonConvert.DeserializeObject<LogonResponse>(json);
                        IPAddress serverIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

                        RemoteServer server = new RemoteServer(response.ServerGuid, response.FriendlyName, serverIP, response.AvailableComponents.ToList(), response.AvailableClients.ToList());
                        this.RemoteServers.Add(server);

                        break;
                    }

                    counter += 100;
                    Thread.Sleep(100);
                }

                ns.Close();
                ns.Dispose();
            }
            catch
            {
            }

            client.Close();
        }

        private void NewServerLogon(string json, TcpClient client)
        {
            LogonRequest request = JsonConvert.DeserializeObject<LogonRequest>(json);
            IPAddress serverIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

            RemoteServer server = new RemoteServer(request.ServerGuid, request.FriendlyName, serverIP, request.AvailableComponents.ToList(), request.AvailableClients.ToList());
            this.RemoteServers.Add(server);

            LogonResponse response = new LogonResponse();
            response.ServerGuid = this.MyGuid;
            response.FriendlyName = this.MyFriendlyName;
            response.LogonRequestGuid = request.LogonRequestGuid;

            List<Client> clients = this.Server.Clients.ToList();

            List<ClientInfo> clientinfos = ServerOperations.GetClientInfos(clients);

            response.AvailableClients = clientinfos;
            response.AvailableComponents = this.Server.LocalComponents.ToList();

            // Send response
            try
            {
                NetworkStream ns = client.GetStream();

                string jsonResponse = JsonConvert.SerializeObject(response);
                ServerPackage respPackage = new ServerPackage(MessageCode.Logon, jsonResponse);

                Networking.SendServerPackage(respPackage, ns);

                ns.Close();
                ns.Dispose();
                client.Close();
            }
            catch
            {
            }
        }

        private void SendKeepAlive()
        {
            KeepAliveRequest request = new KeepAliveRequest();
            request.KeepAliveRequestGuid = Guid.NewGuid();
            request.NumberOfClients = this.Server.Clients.Count();
            request.Terminate = false;
            request.CpuLoad = this.Server.TotalCPULoad;

            string json = JsonConvert.SerializeObject(request);
            ServerPackage package = new ServerPackage(MessageCode.KeepAlive, json);

            this.SendPackageToAllServers(package);

            //foreach (var item in this.RemoteServers)
            //{
            //    TcpClient client = new TcpClient();

            //    try
            //    {
            //        client.Connect(new IPEndPoint(item.IP, 10000));

            //        Networking.SendServerPackage(package, client.GetStream());
            //    }
            //    catch
            //    {
            //        item.Responding = false;
            //    }
            //}

            //foreach (var item in this.RemoteServers.Where(x => !x.Responding))
            //{
            //    this.RemoteServers.Remove(item);
            //}
        }

        private void RecieveKeepAlive(string json, TcpClient client)
        {
            JsonSerializerSettings jss = new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error };
            KeepAliveRequest request = null;
            IPAddress serverIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

            try
            {
                request = JsonConvert.DeserializeObject<KeepAliveRequest>(json, jss);
            }
            catch
            {
                return;
            }

            RemoteServer sender = this.RemoteServers.FirstOrDefault(x => x.IP == serverIP);

            if (sender == null)
            {
                return;
            }

            sender.Load = request.NumberOfClients == 0 ? 100 : request.CpuLoad;

            if (request.Terminate)
            {
                sender.Responding = false;
                this.RemoteServers.Remove(sender);
                return;
            }

            // Send response
            try
            {
                NetworkStream ns = client.GetStream();

                string jsonresponse = JsonConvert.SerializeObject(new KeepAliveResponse() { KeepAliveRequestGuid = request.KeepAliveRequestGuid });
                ServerPackage package = new ServerPackage(MessageCode.KeepAlive, jsonresponse);

                Networking.SendServerPackage(package, ns);

                ns.Dispose();
                ns.Close();
                client.Close();
            }
            catch
            {
            }
        }

        internal void SendClientUpdateRequest(Client client, ClientState state)
        {
            ClientInfo ci = new ClientInfo() { ClientGuid = client.ClientGuid, FriendlyName = client.FriendlyName, IpAddress = ((IPEndPoint)client.ClientTcp.Client.RemoteEndPoint).Address };

            ClientUpdateRequest request = new ClientUpdateRequest();
            request.ClientInfo = ci;
            request.ClientState = state;
            request.ClientUpdateRequestGuid = Guid.NewGuid();

            string json = JsonConvert.SerializeObject(request);
            ServerPackage package = new ServerPackage(MessageCode.ClientUpdate, json);

            this.SendPackageToAllServers(package);
        }

        private void RecieveClientUpdateRequest(string json, TcpClient client)
        {
            JsonSerializerSettings jss = new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error };
            ClientUpdateRequest request = null;
            IPAddress serverIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

            try
            {
                request = JsonConvert.DeserializeObject<ClientUpdateRequest>(json, jss);
            }
            catch
            {
                return;
            }

            RemoteServer sender = this.RemoteServers.FirstOrDefault(x => x.IP == serverIP);

            if (sender == null)
            {
                return;
            }

            if (request.ClientState == ClientState.Connected)
            {
                sender.RemoteClients.Add(request.ClientInfo);
            }
            else if (request.ClientState == ClientState.Disconnected)
            {
                ClientInfo toRemove = sender.RemoteClients.FirstOrDefault(x => x.ClientGuid == request.ClientInfo.ClientGuid);

                if (toRemove != null)
                {
                    sender.RemoteClients.Remove(toRemove);
                }
            }
            else
            {
                return;
            }

            // Send resonse
            try
            {
                NetworkStream ns = client.GetStream();

                string jsonresponse = JsonConvert.SerializeObject(new ClientUpdateResponse() { ClientUpdateRequestGuid = request.ClientUpdateRequestGuid });
                ServerPackage package = new ServerPackage(MessageCode.KeepAlive, jsonresponse);
                Networking.SendServerPackage(package, ns);

                ns.Close();
                ns.Dispose();
                client.Close();
            }
            catch
            {
            }
        }

        internal void SendComponentSubmitRequest(Component component)
        {
            ComponentSubmitRequest request = new ComponentSubmitRequest() { Component = component, ComponentSubmitRequestGuid = Guid.NewGuid() };

            string json = JsonConvert.SerializeObject(request);
            ServerPackage package = new ServerPackage(MessageCode.ComponentSubmit, json);
            this.SendPackageToAllServers(package);
        }

        private void RecieveComponentSubmit(string json, TcpClient client)
        {
            JsonSerializerSettings jss = new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error };
            ComponentSubmitRequest request = null;
            IPAddress serverIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

            try
            {
                request = JsonConvert.DeserializeObject<ComponentSubmitRequest>(json, jss);
            }
            catch
            {
                return;
            }

            RemoteServer sender = this.RemoteServers.FirstOrDefault(x => x.IP == serverIP);

            if (sender == null)
            {
                return;
            }

            if (request.Component != null)
            {
                sender.RemoteComponents.Add(request.Component);
            }

            // Send resonse
            try
            {
                NetworkStream ns = client.GetStream();

                string jsonresponse = JsonConvert.SerializeObject(new ComponentSubmitResponse() { ComponentSubmitRequestGuid = request.ComponentSubmitRequestGuid, IsAccepted = true });
                ServerPackage package = new ServerPackage(MessageCode.ComponentSubmit, jsonresponse);

                Networking.SendServerPackage(package, ns);

                ns.Close();
                ns.Dispose();
                client.Close();
            }
            catch
            {
            }
        }

        internal bool SendJobRequest(JobRequest request, Guid? targetClientGuid)
        {
            //JobRequest request = new JobRequest();
            //request.FriendlyName = job.FriendlyName;
            //request.HopCount = job.HopCount;
            //request.InputData = job.InputData;
            //request.JobComponent = job.JobComponent;
            //request.JobGuid = job.JobGuid;
            //request.JobRequestGuid = job.JobRequestGuid;
            //request.JobSourceClientGuid = job.JobSourceClientGuid;
            //request.TargetCalcClientGuid = job.TargetCalcClientGuid;
            //request.TargetDisplayClient = job.TargetDisplayClient;

            RemoteServer targetServer = null;

            if (targetClientGuid == null)
            {
                targetServer = this.RemoteServers.FirstOrDefault(x => x.Load == this.RemoteServers.Min(y => y.Load));
            }
            else
            {
                targetServer = this.RemoteServers.FirstOrDefault(x => x.RemoteClients.Any(y => y.ClientGuid == targetClientGuid));
            }

            if (targetServer == null)
            {
                return false;
            }

            TcpClient client = new TcpClient();

            try
            {
                client.Connect(new IPEndPoint(targetServer.IP, 10000));

                string json = JsonConvert.SerializeObject(request);
                ServerPackage package = new ServerPackage(MessageCode.JobRequest, json);
                Networking.SendServerPackage(package, client.GetStream());
            }
            catch
            {
                return false;
            }

            // Wait for accept
            int counter = 0;

            try
            {
                while (counter < 30000)
                {
                    if (client.GetStream().DataAvailable)
                    {
                        ServerPackage response = Networking.RecieveServerPackage(client.GetStream());

                        JobResponse jobResp = JsonConvert.DeserializeObject<JobResponse>(response.Payload);

                        if (request.JobRequestGuid == jobResp.JobRequestGuid)
                        {
                            client.Close();
                            return jobResp.IsAccepted;
                        }
                    }

                    counter += 500;
                    Thread.Sleep(500);
                }
            }
            catch
            {
            }

            client.Close();

            return false;
        }

        private void RecieveJobRequest(string json, TcpClient client)
        {
            bool success = true;

            EditorJob request = JsonConvert.DeserializeObject<EditorJob>(json);

            if (request == null)
            {
                success = false;
            }

            request.JobAction = JobAction.Execute;

            if (success)
            {
                try
                {
                    List<Component> components = new List<Component>();

                    foreach (var item in this.Server.AvailableComponents)
                    {
                        components.Add(item);
                    }
                    List<Component> locals = this.Server.LocalComponents.ToList();
                    List<Client> agents = this.Server.Clients.ToList();

                    JobHandler handler = new JobHandler(this.Server, locals, components, agents);
                    Thread t = new Thread(new ParameterizedThreadStart(handler.NewJob));
                    t.IsBackground = true;
                    t.Start(request);
                }
                catch
                {
                    success = false;
                }
            }

            // Send resonse
            try
            {
                NetworkStream ns = client.GetStream();

                string jsonresponse = JsonConvert.SerializeObject(new JobResponse() { JobRequestGuid = request.JobRequestGuid, IsAccepted = success });
                ServerPackage package = new ServerPackage(MessageCode.JobRequest, jsonresponse);

                Networking.SendServerPackage(package, ns);

                ns.Close();
                ns.Dispose();
                client.Close();
            }
            catch
            {
            }
        }

        internal byte[] SendAssemblyRequest(Guid componentID)
        {
            RemoteServer target = this.RemoteServers.FirstOrDefault(x => x.RemoteComponents.Any(y => y.ComponentGuid == componentID));

            if (target == null)
            {
                return null;
            }

            AssemblyRequest request = new AssemblyRequest() { AssemblyRequestGuid = Guid.NewGuid(), ComponentGuid = componentID };

            TcpClient client = new TcpClient();

            try
            {
                client.Connect(target.IP, 10000);
                NetworkStream ns = client.GetStream();
                string json = JsonConvert.SerializeObject(request);
                ServerPackage package = new ServerPackage(MessageCode.RequestAssembly, json);

                Networking.SendServerPackage(package, ns);

                int counter = 0;
                byte[] data = null;

                while (counter < 30000)
                {
                    if (ns.DataAvailable)
                    {
                        data = Networking.RecieveRemoteAssembly(ns);
                    }
                    
                    counter += 100;
                    Thread.Sleep(100);
                }

                ns.Close();
                ns.Dispose();
                client.Close();

                return data;
            }
            catch
            {
                return null;
            }

            return null;
        }

        private void RecieveAssemblyRequest(string json, TcpClient client)
        {
            AssemblyRequest request = JsonConvert.DeserializeObject<AssemblyRequest>(json);

            byte[] data = ServerOperations.GetComponentBytes(request.ComponentGuid);

            try
            {
                NetworkStream ns = client.GetStream();

                Networking.SendRemoteAssembly(data, ns);

                ns.Close();
                ns.Dispose();
                client.Close();
            }
            catch
            {
            }
        }

        private void SendPackageToAllServers(ServerPackage package)
        {
            foreach (var item in this.RemoteServers)
            {
                TcpClient client = new TcpClient();

                try
                {
                    client.Connect(new IPEndPoint(item.IP, 10000));
                    NetworkStream ns = client.GetStream();

                    Networking.SendServerPackage(package, ns);

                    this.WaitForResponse(ns);

                    ns.Close();
                    ns.Dispose();
                    client.Close();
                }
                catch
                {
                    item.Responding = false;
                }
            }

            foreach (var item in this.RemoteServers.Where(x => !x.Responding))
            {
                this.RemoteServers.Remove(item);
            }
        }

        private void WaitForResponse(NetworkStream ns)
        {
            int counter = 0;

            while (counter < 10000)
            {
                if (ns.DataAvailable)
                {
                    Networking.RecieveServerPackage(ns);

                    // Recieved package will be discarded, because it's only containing a response GUID.
                }

                counter += 100;
                Thread.Sleep(100);
            }
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.SendKeepAlive();
        }

        private void OnBroadcastTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.SendBroadcast();
        }
    }
}
