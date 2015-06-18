using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class Editor
    {
        public Editor(TcpClient client, Thread thread)
        {
            this.TcpClient = client;
            this.EditorThread = thread;
        }

        public TcpClient TcpClient { get; set; }

        public Thread EditorThread { get; set; }


    }
}
