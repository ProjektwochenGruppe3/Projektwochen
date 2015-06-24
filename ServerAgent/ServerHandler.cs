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
            this.Timer.AutoReset = true;
            this.Timer.Elapsed += this.OnTimerElapsed;
        }

        public Guid MyGuid { get; set; }

        public string MyFriendlyName { get; set; }

        public List<RemoteServer> RemoteServers { get; set; }

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
                        this.HandlePackage(pack, ns);
                    }
                }
                catch
                {
                }

                Thread.Sleep(42);
            }
        }

        private void HandlePackage(ServerPackage pack, NetworkStream ns)
        {
            switch (pack.MessageCode)
            {
                case MessageCode.ClientUpdate:
                    break;
                case MessageCode.ComponentSubmit:
                    break;
                case MessageCode.JobRequest:
                    break;
                case MessageCode.JobResult:
                    break;
                case MessageCode.KeepAlive:
                    break;
                case MessageCode.Logon:
                    this.NewServerLogon(pack.Payload, ns);
                    break;
                case MessageCode.RequestAssembly:
                    break;
                default:
                    return;
            }
        }

        private void NewServerLogon(string json, NetworkStream ns)
        {
            LogonRequest request = JsonConvert.DeserializeObject<LogonRequest>(json);

            RemoteServer server = new RemoteServer(request.ServerGuid, request.FriendlyName, request.AvailableComponents.ToList(), request.AvailableClients.ToList());
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

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
