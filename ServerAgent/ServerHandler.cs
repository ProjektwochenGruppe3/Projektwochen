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

        private Server Server { get; set; }

        private System.Timers.Timer Timer { get; set; }

        private void ListenerWorker()
        {
            this.Listener.Start();

            this.Broadcast();

            while (true)
            {
                TcpClient newServer = this.Listener.AcceptTcpClient();

                Thread t = new Thread(new ParameterizedThreadStart(this.RequestWorker));
                t.Start(newServer);

                Thread.Sleep(100);
            }
        }

        private void Broadcast()
        {
            // 10001 UDP
        }

        private void RequestWorker(object serverTcp)
        {
            TcpClient newServer = (TcpClient)serverTcp;

            NetworkStream ns = newServer.GetStream();

            while (true)
            {
                try
                {
                    if (ns.DataAvailable)
                    {
                        ServerPackage pack = Networking.RecieveServerPackage(ns);
                        this.HandlePackage(pack, newServer);
                    }
                }
                catch
                {
                }

                Thread.Sleep(42);
            }
        }

        private void HandlePackage(ServerPackage pack, TcpClient serverTcpClient)
        {
            switch (pack.MessageCode)
            {
                case MessageCode.ClientUpdate:
                    this.RecieveClientUpdateRequest(pack.Payload, serverTcpClient);
                    break;
                case MessageCode.ComponentSubmit:
                    break;
                case MessageCode.JobRequest:
                    break;
                case MessageCode.JobResult:
                    break;
                case MessageCode.KeepAlive:
                    this.RecieveKeepAlive(pack.Payload, serverTcpClient);
                    break;
                case MessageCode.Logon:
                    this.NewServerLogon(pack.Payload, serverTcpClient);
                    break;
                case MessageCode.RequestAssembly:
                    break;
                default:
                    return;
            }
        }

        private void NewServerLogon(string json, TcpClient serverTcpClient)
        {
            NetworkStream ns = serverTcpClient.GetStream();
            IPAddress ip = ((IPEndPoint)serverTcpClient.Client.RemoteEndPoint).Address;
            LogonRequest request = JsonConvert.DeserializeObject<LogonRequest>(json);

            RemoteServer server = new RemoteServer(request.ServerGuid, request.FriendlyName, ip, request.AvailableComponents.ToList(), request.AvailableClients.ToList());
            this.RemoteServers.Add(server);

            LogonResponse response = new LogonResponse();
            response.ServerGuid = this.MyGuid;
            response.FriendlyName = this.MyFriendlyName;
            response.LogonRequestGuid = request.LogonRequestGuid;

            List<Client> clients = this.Server.Clients.ToList();
            List<ClientInfo> clientinfos = new List<ClientInfo>();

            foreach (var item in clients)
            {
                ClientInfo ci = new ClientInfo();
                ci.ClientGuid = item.ClientGuid;
                ci.FriendlyName = item.FriendlyName;
                ci.IpAddress = ((IPEndPoint)item.ClientTcp.Client.RemoteEndPoint).Address;

                clientinfos.Add(ci);
            }

            response.AvailableClients = clientinfos;
            response.AvailableComponents = this.Server.LocalComponents.ToList();

            string jsonResponse = JsonConvert.SerializeObject(response);
            ServerPackage respPackage = new ServerPackage(MessageCode.Logon, jsonResponse);

            Networking.SendServerPackage(respPackage, ns);
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

        private void RecieveKeepAlive(string json, TcpClient serverTcpClient)
        {
            JsonSerializerSettings jss = new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error };
            KeepAliveRequest request = null;

            try
            {
                request = JsonConvert.DeserializeObject<KeepAliveRequest>(json, jss);
            }
            catch
            {
                return;
            }

            RemoteServer sender = this.RemoteServers.FirstOrDefault(x => x.IP == ((IPEndPoint)serverTcpClient.Client.RemoteEndPoint).Address);

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
            TcpClient client = new TcpClient();
            
            try
            {
                client.Connect(new IPEndPoint(sender.IP, 10000));

                string jsonresponse = JsonConvert.SerializeObject(new KeepAliveResponse() { KeepAliveRequestGuid = request.KeepAliveRequestGuid });
                ServerPackage package = new ServerPackage(MessageCode.KeepAlive, jsonresponse);
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

        private void RecieveClientUpdateRequest(string json, TcpClient serverTcpClient)
        {
            JsonSerializerSettings jss = new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error };
            ClientUpdateRequest request = null;

            try
            {
                request = JsonConvert.DeserializeObject<ClientUpdateRequest>(json, jss);
            }
            catch
            {
                return;
            }

            RemoteServer sender = this.RemoteServers.FirstOrDefault(x => x.IP == ((IPEndPoint)serverTcpClient.Client.RemoteEndPoint).Address);

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
            TcpClient client = new TcpClient();
            
            try
            {
                client.Connect(new IPEndPoint(sender.IP, 10000));

                string jsonresponse = JsonConvert.SerializeObject(new ClientUpdateResponse() { ClientUpdateRequestGuid = request.ClientUpdateRequestGuid });
                ServerPackage package = new ServerPackage(MessageCode.KeepAlive, jsonresponse);
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

                    Networking.SendServerPackage(package, client.GetStream());
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

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.SendKeepAlive();
        }
    }
}
