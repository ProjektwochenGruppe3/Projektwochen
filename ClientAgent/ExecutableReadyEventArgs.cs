using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ClientAgent
{
    public class ExecutableReadyEventArgs
    {
        public ExecutableReadyEventArgs(AtomicJob job, NetworkStream stream)
        {
            this.Job = job;
            this.Stream = stream;
        }

        public AtomicJob Job { get; set; }

        public NetworkStream Stream { get; set; }
    }
}
