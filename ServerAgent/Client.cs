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
        }

        public bool SendDataToClient { get; set; }

        //public string MessageToClient { get; set; }

        public Thread ClientThread { get; set; }

        public TcpClient ClientTcp { get; set; }

        public bool ClientAlive { get; set; }
    }
}
