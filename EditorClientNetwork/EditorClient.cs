using Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EditorNetwork
{
    public class EditorClient
    {     
        public EditorClient()
        {           
        }
       
        public IPAddress IpAddress { get; private set; }

        public int Port { get; set; }

        public TcpClient TCPClientEditor { get; set; }
        
        public bool IsWaiting { get; set; }

        public bool IsAlive { get; set; }

        public ClientState State {get; set;}
     
    }
}
