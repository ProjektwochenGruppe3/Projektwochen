using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Core.Network;
using dcs.core;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class JobHandler
    {
        public JobHandler(Server server, List<Component> avComp)
        {
            this.Server = server;
            this.AtomicComponents = new List<Component>();
            this.AvailableComponents = avComp;
            this.JobParts = new List<Component>();
            this.JobParts = null;
        }

        public event EventHandler JobDone;

        private Server Server { get; set; }

        private List<Component> AtomicComponents { get; set; }

        private List<Component> AvailableComponents { get; set; }

        private List<InternalNode> JobParts { get; set; }

        public void NewJob(object jobobj)
        {
            EditorJob job = (EditorJob)jobobj;

            if (job.JobAction == JobAction.Save)
            {
                ServerOperations.SaveComponent(job.JobComponent);
            }
            else
            {
                JobRequest request = this.ConvertToJobRequest(job);
                this.ExecuteJob(request);
            }

            if (job.JobAction == JobAction.SaveAndExecute)
            {
                ServerOperations.SaveComponent(job.JobComponent);
            }
        }

        private void ExecuteJob(JobRequest request)
        {
            this.AnalyzeComponent(request.JobComponent);

            // TODO
            // Target & CalcClient GUID GUID UGIDGUAWIpegjöl


            Component comp = this.Server.LocalComponents.FirstOrDefault(x => x.ComponentGuid == request.JobComponent.ComponentGuid);

            // Not null, if component exists locally
            if (comp != null)
            {
                ClientInfo info = this.FindTargetClient(request);

                this.SendAtomicComponent(request, info);
            }
            else
            {
                // Server handler request component
            }
        }

        private void AnalyzeComponent(Component component)
        {
            if (this.JobParts == null)
            {
                this.JobParts = new List<InternalNode>();
            }

            foreach (var item in component.Edges)
            {
                InternalNode node = null;
                node = this.JobParts.FirstOrDefault(x => x.NodeInputGuids.Contains(item.InternalInputComponentGuid) && item.InternalInputComponentGuid != Guid.Empty);

                if (node == null)
                {
                    node = new InternalNode();
                    node.NodeInputGuids = new List<Guid>();
                    node.TargetGuids = new List<Guid>();
                    node.NodeInputGuids.Add(item.InternalInputComponentGuid);
                    node.TargetGuids.Add(item.InternalOutputComponentGuid);
                    node.Component = this.AvailableComponents.First(x => x.ComponentGuid == item.InputComponentGuid);
                    node.InputParametersForTarget = new List<object>();

                    this.JobParts.Add(node);
                }
                else
                {
                    node.NodeInputGuids.Add(item.InternalInputComponentGuid);
                    node.TargetGuids.Add(item.InternalOutputComponentGuid);
                }
            }

            foreach (var item in this.JobParts)
            {
                if (!item.Component.IsAtomic)
                {
                    this.AnalyzeComponent(item.Component);
                }
            }
        }

        private void SplitComponent(Component component)
        {
            if (component.IsAtomic)
            {
                this.AtomicComponents.Add(component);
                return;
            }


        }

        public void DebugSendAtomicComponent(JobRequest request, ClientInfo info)
        {
            //byte[] comp = ServerOperations.GetComponentBytes(request.JobComponent.ComponentGuid);
            byte[] comp = System.IO.File.ReadAllBytes(System.IO.Path.Combine(Environment.CurrentDirectory, "Components", "ConsoleInput.dll"));
            AgentExecutable package = new AgentExecutable(comp);

            //DEBUG
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 47474));
            NetworkStream ns = client.GetStream();

            Networking.SendPackage(package, ns);

            Networking.SendPackage(new AgentExecutableParameters(null), ns);

            while (true)
            {
                if (ns.DataAvailable)
                {
                    Console.WriteLine(((AgentExecutableResult)Networking.RecievePackage(ns)).Results.ToList()[0].ToString());
                    break;
                }
            }
        }

        private void SendAtomicComponent(JobRequest request, ClientInfo info)
        {
            //byte[] comp = ServerOperations.GetComponentBytes(request.JobComponent.ComponentGuid);
            byte[] comp = System.IO.File.ReadAllBytes(System.IO.Path.Combine(Environment.CurrentDirectory, "Components", "ConsoleInput.dll"));
            AgentExecutable package = new AgentExecutable(comp);


        }

        private ClientInfo FindTargetClient(JobRequest request)
        {
            if (request.TargetCalcClientGuid != null && request.TargetCalcClientGuid != Guid.Empty)
            {
                // Checks if client exits in the global network
                Tuple<Guid, string> clienttuple = this.Server.AvailableClients.FirstOrDefault(x => x.Item1 == request.TargetCalcClientGuid);

                if (clienttuple != null)
                {
                    Client client = this.Server.Clients.FirstOrDefault(x => x.ClientGuid == request.TargetCalcClientGuid);

                    // Not null, if client is in internal network (connected to this server)
                    if (client != null)
                    {
                        IPEndPoint endpt = client.ClientTcp.Client.RemoteEndPoint as IPEndPoint;
                        IPAddress ip = endpt.Address;
                        return new ClientInfo() { ClientGuid = client.ClientGuid, FriendlyName = client.FriendlyName, IpAddress = ip };
                    }
                    else
                    {
                        return this.Server.GetRemoteClient((Guid)request.TargetCalcClientGuid);
                    }
                }
                else
                {
                    // FIND LOWEST USAGE
                }
            }

            return null;
        }

        private JobRequest ConvertToJobRequest(EditorJob job)
        {
            JobRequest req = new JobRequest();

            req.FriendlyName = job.FriendlyName;
            req.HopCount = job.HopCount;
            req.InputData = job.InputData;
            req.JobComponent = job.JobComponent;
            req.JobRequestGuid = job.JobRequestGuid;
            req.JobSourceClientGuid = job.JobSourceClientGuid;
            req.TargetCalcClientGuid = job.TargetCalcClientGuid;
            req.TargetDisplayClient = job.TargetDisplayClient;

            return req;
        }
    }
}
