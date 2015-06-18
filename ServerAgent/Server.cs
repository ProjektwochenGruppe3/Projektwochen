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
            this.LocalComponents = ServerOperations.GetLocalComponents();
            this.ServerHandler = new ServerHandler();
            this.EditorHander = new EditorHandler(this.LocalComponents, this.ServerHandler.GetRemoteComponents());
        }

        public List<Client> Clients { get; set; }

        public List<Component> LocalComponents { get; set; }

        public bool ServerAlive { get; set; }

        private EditorHandler EditorHander { get; set; }

        private ServerHandler ServerHandler { get; set; }

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

        //public async void BroadcastKeepAlive()
        //{
        //    List<Task> tasks = new List<Task>();

        //    foreach (Client c in this.Clients)
        //    {
        //        tasks.Add(this.KeepAliveWorker(c));
        //    }

        //    foreach (var item in tasks)
        //    {
        //        try
        //        {
        //            await item;
        //        }
        //        catch
        //        {
        //        }                
        //    }

        //    foreach (var item in this.Clients.Where(x => x.ClientAlive == false))
        //    {
        //        this.Clients.Remove(item);
        //    }
        //}

        //public void SendToAllOtherClients(Client client, string message)
        //{
        //    foreach (Client c in Clients)
        //    {
        //        if (c != client)
        //        {
        //        }
        //    }
        //}

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
                    Console.WriteLine("CPU-Load of client {0} is {1}", client.FriendlyName, client.CpuLoad);
                }

                Thread.Sleep(50);
            }

            netStream.Close();
            netStream.Dispose();
            client.ClientTcp.Close();
        }

        //private async Task KeepAliveWorker(Client c)
        //{
        //    await Task.Run(() => this.EvaluateKeepAlive(c));
        //}

        private bool EvaluateKeepAlive(Client c)
        {
            Guid requestguid = this.SendKeepAliveRequest(c);

            bool success = this.RecieveKeepAliveResponse(c, requestguid);

            if (!success)
            {
                c.ClientAlive = false;
                return false;
            }

            return true;
        }

        private Guid SendKeepAliveRequest(Client c)
        {
            Guid g = Guid.NewGuid();
            AgentStatusRequest req = new AgentStatusRequest(g);

            Networking.SendPackage(req, c.ClientTcp.GetStream());

            return g;
        }

        private bool RecieveKeepAliveResponse(Client c, Guid requestguid)
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

            this.Clients.Remove(client);
        }
    }
}
