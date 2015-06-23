using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using dcs.core;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class AgentWorker
    {
        public AgentWorker(IPAddress address, InternalNode node)
        {
            this.AgentAddress = address;
            this.Action = node;
        }

        public event EventHandler<ActionCompletedEventArgs> ActionCompleted;

        public IPAddress AgentAddress { get; set; }

        public InternalNode Action { get; set; }

        public IEnumerable<object> InputParameters { get; set; }

        public void StartWorker()
        {
            Thread t = new Thread(new ThreadStart(Worker));
            t.Start();
        }

        private void Worker()
        {
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(this.AgentAddress, 47474));
            NetworkStream ns = client.GetStream();

            byte[] comp = ServerOperations.GetComponentBytes(this.Action.Component.ComponentGuid);
            AgentExecutable package = new AgentExecutable(comp);

            Networking.SendPackage(package, ns);

            while (this.InputParameters == null)
            {
                if (this.InputParameters != null && this.Action.NodeInputGuids.Count() == this.InputParameters.Count())
                {
                    AgentExecutableParameters paramameters = new AgentExecutableParameters(this.InputParameters);

                    Networking.SendPackage(paramameters, ns);
                }
            }

            while (true)
            {
                if (ns.DataAvailable)
                {
                    AgentExecutableResult res = Networking.RecievePackage(ns) as AgentExecutableResult;

                    if (res != null)
                    {
                        this.FireActionDone(res);
                        break;
                    }
                }

                Thread.Sleep(50);
            }
        }

        private void FireActionDone(AgentExecutableResult result)
        {
            if (this.ActionCompleted != null)
            {
                this.ActionCompleted(this, new ActionCompletedEventArgs(result));
            }
        }
    }
}
