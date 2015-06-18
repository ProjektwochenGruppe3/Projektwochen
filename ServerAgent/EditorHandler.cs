﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class EditorHandler
    {
        public EditorHandler(TcpClient client, Thread thread)
        {
            this.TcpClient = client;
            this.EditorThread = thread;
        }

        private List<Editor> ConnectedEditors { get; set; }
    }
}
