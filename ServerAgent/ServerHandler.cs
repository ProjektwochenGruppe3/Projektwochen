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

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class ServerHandler
    {
        public ServerHandler()
        {
            this.RemoteServers = new List<RemoteServer>();
            this.MyGuid = Guid.NewGuid();
            this.Listener = new TcpListener(IPAddress.Any, 10000);
            this.ListenerThread = new Thread(new ThreadStart(this.ListenerWorker));
            //this.ListenerThread.Start();
            this.ListenerThread.IsBackground = true;
        }

        public Guid MyGuid { get; set; }

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
                        Networking.RecieveServerPackage(ns);
                    }
                }
                catch
                {
                }
            }
        }
    }
}
