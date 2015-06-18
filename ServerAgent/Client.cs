using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class Client
    {
        public Client(TcpClient client, Thread clientThread)
        {
            this.ClientTcp = client;
            this.ClientThread = clientThread;
            this.ClientAlive = true;
            this.CpuLoad = 0;
            this.FriendlyName = string.Empty;
        }

        public event EventHandler ClientDisconnected;

        public bool SendDataToClient { get; set; }

        public Thread ClientThread { get; set; }

        public TcpClient ClientTcp { get; set; }

        public bool ClientAlive { get; set; }

        public int CpuLoad { get; set; }

        public string FriendlyName { get; set; }

        public Guid ClientGuid { get; set; }

        public void OnClientDisconnected()
        {
            if (this.ClientDisconnected != null)
            {
                this.ClientDisconnected(this, new EventArgs());
            }
        }
    }
}
