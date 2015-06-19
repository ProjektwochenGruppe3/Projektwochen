using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using dcs.core;
using Core.Network;

namespace ClientAgent
{
    public class AtomicJob
    {
        public AtomicJob(Thread thread, TcpClient server)
        {
            this.ExecutableThread = thread;
            this.Server = server;
            this.AtJobGuid = new Guid();
            this.InProgress = true;
        }

        public bool InProgress { get; set; }

        public Guid AtJobGuid { get; private set; }

        public TcpClient Server { get; set; }

        public Type ExecutableType { get; set; }

        public IEnumerable<object> Params { get; set; }

        public Thread ExecutableThread { get; private set; }

        public IEnumerable<object> Result { get; set; }

        public JobState State { get; set; }

        public event EventHandler<AgentExecutable> OnExecutableReceived;

        public event EventHandler<AgentExecutableParameters> OnExecutableParametersRecieved;

        public event EventHandler<ExecutableReadyEventArgs> OnExecutableResultsReady;

        public void FireOnExecutableResultsReady(NetworkStream netStream)
        {
            ExecutableReadyEventArgs args = new ExecutableReadyEventArgs(this, netStream);
            if (OnExecutableResultsReady != null)
            {
                OnExecutableResultsReady(this, args);
            }
        }
    }
}
