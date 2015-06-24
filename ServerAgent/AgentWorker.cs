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

            if (this.Action.NodeInputGuids.Count() == 0)
            {
                Networking.SendPackage(new AgentExecutableParameters(new List<object>()), ns);
            }
            else
            {
                while (true)
                {
                    int missingArgsCount = this.Action.InputParameters.Where(x => x == null).Count();

                    if (missingArgsCount == 0)
                    {
                        AgentExecutableParameters paramameters = new AgentExecutableParameters(this.Action.InputParameters);

                        Networking.SendPackage(paramameters, ns);
                        break;
                    }

                    Thread.Sleep(42);
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
