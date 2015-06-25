using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using dcs.core;
using Core.Network;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class Server
    {
        private Thread listenerThread;

        public Server()
        {
            this.Clients = new List<Client>();
            this.ServerAlive = true;
            this.Listener = new TcpListener(IPAddress.Any, 13370);
            this.listenerThread = new Thread(new ThreadStart(ListenerWorker));
            this.ServerHandler = new ServerHandler(this);
            this.EditorHander = new EditorHandler(this);
        }

        public List<Client> Clients { get; set; }

        public bool ServerAlive { get; set; }

        public List<Component> AvailableComponents
        {
            get
            {
                List<Component> comp = new List<Component>();

                foreach (var item in this.LocalComponents)
                {
                    comp.Add(item);
                }

                foreach (var item in this.ServerHandler.GetRemoteComponents())
                {
                    comp.Add(item);
                }

                return comp;
            }
        }

        public List<Tuple<Guid, string>> AvailableClients
        {
            get
            {
                List<Tuple<Guid, string>> clients = new List<Tuple<Guid, string>>();

                foreach (var item in this.Clients)
                {
                    clients.Add(new Tuple<Guid, string>(item.ClientGuid, item.FriendlyName));
                }

                foreach (var item in this.ServerHandler.GetRemoteClients())
                {
                    clients.Add(new Tuple<Guid, string>(item.ClientGuid, item.FriendlyName));
                }

                return clients;
            }
        }

        public int TotalCPULoad
        {
            get
            {
                int load = 0;

                if (this.Clients.Count() == 0)
                {
                    return 100;
                }

                foreach (var item in this.Clients)
                {
                    load += item.CpuLoad;
                }

                try
                {
                    load /= this.Clients.Count();
                }
                catch
                {
                    return 100;
                }

                return load;
            }
        }

        internal List<Component> LocalComponents
        {
            get
            {
                return ServerOperations.GetLocalComponents();
            }
        }

        internal ServerHandler ServerHandler { get; set; }

        private EditorHandler EditorHander { get; set; }

        private TcpListener Listener { get; set; }  

        public void StartServer()
        {
            this.listenerThread.Start();
        }

        public void StopServer()
        {
            foreach (Client c in Clients)
            {
                c.ClientThread.Join();
                c.ClientAlive = false;
            }
            this.listenerThread.Join();
            this.ServerAlive = false;
        }

        public void ListenerWorker()
        {
            this.Listener.Start();

            while (this.ServerAlive)
            {
                if (this.Listener.Pending())
                {
                    TcpClient tmpClient = this.Listener.AcceptTcpClient();
                    Thread tmpClientThread = new Thread(new ParameterizedThreadStart(ClientWorker));
                    Client client = new Client(tmpClient, tmpClientThread);
                    Console.WriteLine("Client has connected!");

                    if (this.EvaluateKeepAlive(client))
                    {
                        this.Clients.Add(client);
                        client.ClientThread.Start(client);
                        client.ClientDisconnected += this.OnClientDisconnected;
                        //this.ServerHandler.SendClientUpdateRequest(client, ClientState.Connected);
                    }
                }

                Thread.Sleep(50);
            }
        }

        public void ClientWorker(object args)
        {
            Client client = (Client)args;
            NetworkStream netStream = client.ClientTcp.GetStream();

            while (client.ClientAlive)
            {
                AgentStatus recieved = null;

                try
                {
                    Networking.SendPackage(true, netStream);

                    if (netStream.DataAvailable)
                    {
                        recieved = Networking.RecievePackage(netStream) as AgentStatus;
                    }
                }
                catch
                {
                    client.OnClientDisconnected();
                }

                if (recieved != null)
                {
                    client.CpuLoad = recieved.CpuLoad;
                    client.ClientGuid = recieved.AgentGuid;
                    client.FriendlyName = recieved.FriendlyName;

                    Console.WriteLine("CPU-Load of client {0} is {1}", client.FriendlyName, client.CpuLoad);
                }

                Thread.Sleep(500);
            }

            netStream.Close();
            netStream.Dispose();
            client.ClientTcp.Close();
        }

        //private async Task KeepAliveWorker(Client c)
        //{
        //    await Task.Run(() => this.EvaluateKeepAlive(c));
        //}

        internal ClientInfo GetRemoteClient(Guid clientguid)
        {
            foreach (var item in this.ServerHandler.RemoteServers)
            {
                ClientInfo info = item.RemoteClients.FirstOrDefault(x => x.ClientGuid == clientguid);

                if (info != null)
                {
                    return info;
                }
            }

            return null;
        }
        
        private bool EvaluateKeepAlive(Client c)
        {
            Guid requestguid = this.SendAgentStatusRequest(c);

            bool success = this.RecieveAgentStatusResponse(c, requestguid);

            if (!success)
            {
                c.ClientAlive = false;
                return false;
            }

            return true;
        }

        private Guid SendAgentStatusRequest(Client c)
        {
            Guid g = Guid.NewGuid();
            AgentStatusRequest req = new AgentStatusRequest(g);

            Networking.SendPackage(req, c.ClientTcp.GetStream());

            return g;
        }

        private bool RecieveAgentStatusResponse(Client c, Guid requestguid)
        {
            NetworkStream ns = c.ClientTcp.GetStream();
            AgentStatus res = null;

            int timer = 0;

            while (timer < 5000)
            {
                if (ns.DataAvailable)
                {
                    res = Networking.RecievePackage(ns) as AgentStatus;
                }

                timer += 50;
                Thread.Sleep(50);
            }

            if (res == null || res.AgentStatusRequestGuid != requestguid)
            {
                return false;
            }

            c.CpuLoad = res.CpuLoad;
            c.FriendlyName = res.FriendlyName;

            return true;
        }

        private void OnClientDisconnected(object sender, EventArgs e)
        {
            Client client = sender as Client;

            if (client == null)
            {
                return;
            }

            client.ClientAlive = false;
            this.Clients.Remove(client);
            //this.ServerHandler.SendClientUpdateRequest(client, ClientState.Disconnected);
            Console.WriteLine("Client {0} disconnected", client.FriendlyName);
        }
    }
}
