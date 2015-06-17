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

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class Server
    {
        private Thread listenerThread;

        public TcpListener Listener { get; set; }

        public List<Client> Clients { get; set; }

        public bool ServerAlive { get; set; }

        public System.Timers.Timer KeepAliveTimer { get; set; }

        public void StartServer()
        {
            this.Clients = new List<Client>();
            this.ServerAlive = true;
            this.Listener = new TcpListener(IPAddress.Any, 13370);
            this.Listener.Start();
            this.listenerThread = new Thread(new ThreadStart(ListenerWorker));
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

        public void BroadcastKeepAlive()
        {
            foreach (Client c in this.Clients)
            {
                Task t = Task.Factory.StartNew(
                    () =>
                    { 

                    });
            }
        }

        public void SendToAllOtherClients(Client client, string message)
        {
            foreach (Client c in Clients)
            {
                if (c != client)
                {
                }
            }
        }

        public void ListenerWorker()
        {
            while (this.ServerAlive)
            {
                if (this.Listener.Pending())
                {
                    TcpClient tmpClient = this.Listener.AcceptTcpClient();
                    Thread tmpClientThread = new Thread(new ParameterizedThreadStart(ClientWorker));
                    Client client = new Client(tmpClient, tmpClientThread);
                    this.Clients.Add(client);
                    client.ClientThread.Start(client);
                    Console.WriteLine("A Client has connected!");
                }

                Thread.Sleep(50);
            }
        }

        public void ClientWorker(object args)
        {
            Client client = (Client)args;
            NetworkStream netStream = client.ClientTcp.GetStream();
            byte[] receiveBuffer = new byte[500];
            byte[] sendBuffer = new byte[500];

            while(client.ClientAlive)
            {
                if (netStream.DataAvailable)
                {
                    int readLength = netStream.Read(receiveBuffer, 0, receiveBuffer.Length);
                    string message = Encoding.UTF8.GetString(receiveBuffer, 0, readLength);
                    this.SendToAllOtherClients(client, message);
                    Console.WriteLine(message);
                }
                if (client.SendDataToClient)
                {
                    sendBuffer = Encoding.UTF8.GetBytes(client.MessageToClient);
                    netStream.Write(sendBuffer,0,sendBuffer.Length);
                    client.SendDataToClient = false;
                }

                Thread.Sleep(50);
            }
        }

        private Guid SendKeepAliveRequest(Client c)
        {
            Guid g = Guid.NewGuid();
            AgentKeepAliveRequest req = new AgentKeepAliveRequest(g);

            Networking.SendPackage(g, c.ClientTcp.GetStream());

            return g;
        }

        private bool RecieveKeepAliveResponse(Client c, Guid requestguid)
        {
            AgentKeepAliveResponse res = Networking.RecievePackage(c.ClientTcp.GetStream()) as AgentKeepAliveResponse;

            if (res == null || res.KeepAliveRequestGuid != requestguid)
            {
                return false;
            }

            c.CpuLoad = res.CpuLoad;
            c.FriendlyName = res.FriendlyName;

            return true;
        }
    }
}
