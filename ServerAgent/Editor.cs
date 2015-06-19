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
        public Editor(TcpClient tcp, Thread thread)
        {
            this.TcpClient = tcp;
            this.EditorThread = thread;
        }

        public event EventHandler EditorDisconnected;

        public TcpClient TcpClient { get; set; }

        public Thread EditorThread { get; set; }

        public void OnEditorDisconnected()
        {
            if (this.EditorDisconnected != null)
            {
                this.EditorDisconnected(this, new EventArgs());
            }
        }
    }
}
