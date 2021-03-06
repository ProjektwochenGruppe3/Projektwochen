﻿using System;
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
        public JobHandler(Server server, List<Component> localComp, List<Component> availableComp, List<Client> agents)
        {
            this.Server = server;
            this.AtomicComponents = new List<Component>();
            this.AvailableComponents = availableComp;
            this.JobParts = null;
            this.LocalComponents = localComp;
            this.LocalAgents = agents;
            this.AgentWorkers = new List<AgentWorker>();
        }

        public event EventHandler ActionDone;

        private Server Server { get; set; }

        private List<Component> AtomicComponents { get; set; }

        private List<Component> AvailableComponents { get; set; }

        private List<Component> LocalComponents { get; set; }

        private List<Client> LocalAgents { get; set; }

        private List<InternalNode> JobParts { get; set; }

        private List<AgentWorker> AgentWorkers { get; set; }

        public void NewJob(object jobobj)
        {
            EditorJob job = (EditorJob)jobobj;

            if (job.JobAction == JobAction.Save)
            {
                ServerOperations.SaveComponent(job.JobComponent);
                this.Server.ServerHandler.SendComponentSubmitRequest(job.JobComponent);
            }
            else
            {
                JobRequest request = this.ConvertToJobRequest(job);
                this.ExecuteJob(request);
            }

            if (job.JobAction == JobAction.SaveAndExecute)
            {
                ServerOperations.SaveComponent(job.JobComponent);
                this.Server.ServerHandler.SendComponentSubmitRequest(job.JobComponent);
            }
        }

        private void ExecuteJob(JobRequest request)
        {
            this.AnalyzeComponent(request.JobComponent);

            if (this.LocalAgents.Count() == 0)
            {
                this.Server.ServerHandler.SendJobRequest(request, null);
                return;
            }

            Tuple<ClientInfo, bool> calc = this.FindClient(request.TargetCalcClientGuid);
            Tuple<ClientInfo, bool> display = this.FindClient(request.TargetDisplayClient);

            if (!calc.Item2 && request.HopCount < this.Server.ServerHandler.RemoteServers.Count())
            {
                if (!this.Server.ServerHandler.SendJobRequest(request, calc.Item1.ClientGuid))
                {
                    Console.WriteLine("The remote server denied the job request!");
                }

                return;
            }
            else if (!display.Item2 && request.HopCount < this.Server.ServerHandler.RemoteServers.Count())
            {
                if(!this.Server.ServerHandler.SendJobRequest(request, display.Item1.ClientGuid))
                {
                    Console.WriteLine("The remote server denied the job request!");
                }

                return;
            }

            // Request all assemblies that are currently not available locally.
            foreach (var item in this.JobParts.Where(x => !this.LocalComponents.Any(y => y.ComponentGuid == x.Component.ComponentGuid)))
            {
                byte[] data = this.Server.ServerHandler.SendAssemblyRequest(item.Component.ComponentGuid);

                if (data == null)
                {
                    Console.WriteLine("The requested assembly could not be retrieved from the source server.");
                    return;
                }

                bool success = ServerOperations.SaveNewAssembly(data, item.Component.ComponentGuid);

                if (!success)
                {
                    Console.WriteLine("The requested assembly could not be saved.");
                    return;
                }
            }            

            this.CreateClientWorkLoads();

            if (calc.Item2 && calc.Item1 != null)
            {
                foreach (var item in this.AgentWorkers.Where(x => x.Action.NodeInputGuids.Count() == 0))
                {
                    item.AgentAddress = calc.Item1.IpAddress;
                }
            }

            if (display.Item2 && display.Item1 != null)
            {
                foreach (var item in this.AgentWorkers.Where(x => x.Action.TargetGuids.Count() == 0))
                {
                    item.AgentAddress = display.Item1.IpAddress;
                }
            }

            foreach (var item in this.AgentWorkers)
            {
                item.ActionCompleted += this.OnActionCompleted;
                item.StartWorker();
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
                if (item.InternalOutputComponentGuid == Guid.Empty || item.InternalInputComponentGuid == Guid.Empty)
                {
                    continue;
                }
                InternalNode node = null;
                node = this.JobParts.FirstOrDefault(x => x.NodeInputGuids.Contains(item.InternalInputComponentGuid) && item.InternalInputComponentGuid != Guid.Empty);

                if (node == null)
                {
                    node = new InternalNode();
                    node.NodeInputGuids = new List<Guid>();
                    node.TargetGuids = new List<Guid>();
                    node.TargetPorts = new List<uint>();
                    node.NodeInputGuids.Add(item.InternalInputComponentGuid);
                    try
                    {
                        node.Component = this.AvailableComponents.First(x => x.ComponentGuid == item.InputComponentGuid);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error occurred while splitting the job: " + e.Message);
                        return;
                    }
                    node.InputParameters = new List<object>();
                    node.InputParameters.Add(null);

                    this.JobParts.Add(node);
                }
                else
                {
                    node.NodeInputGuids.Add(item.InternalInputComponentGuid);
                    node.InputParameters.Add(null);
                }
            }

            foreach (var item in component.Edges)
            {
                if (item.InternalOutputComponentGuid == Guid.Empty || item.InternalInputComponentGuid == Guid.Empty)
                {
                    continue;
                }
                InternalNode node = null;
                node = this.JobParts.FirstOrDefault(x => x.NodeInputGuids.Contains(item.InternalOutputComponentGuid) && item.InternalOutputComponentGuid != Guid.Empty);

                if (node == null)
                {
                    node = new InternalNode();
                    node.NodeInputGuids = new List<Guid>();
                    node.TargetGuids = new List<Guid>();
                    node.TargetPorts = new List<uint>();
                    node.TargetPorts.Add(item.InputValueID);
                    node.TargetGuids.Add(item.InternalInputComponentGuid);
                    try
                    {
                        node.Component = this.AvailableComponents.First(x => x.ComponentGuid == item.OutputComponentGuid);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error occurred while splitting the job: " + e.Message);
                        return;
                    }
                    node.InputParameters = new List<object>();

                    this.JobParts.Add(node);
                }
                else
                {
                    node.TargetGuids.Add(item.InternalInputComponentGuid);
                    node.TargetPorts.Add(item.InputValueID);
                }
            }

            //foreach (var item in this.JobParts)
            //{
            //    if (item.Component.Edges != null)
            //    {
            //        this.AnalyzeComponent(item.Component);
            //    }
            //}
        }

        //public void DebugSendAtomicComponent(JobRequest request, ClientInfo info)
        //{
        //    //byte[] comp = ServerOperations.GetComponentBytes(request.JobComponent.ComponentGuid);
        //    byte[] comp = System.IO.File.ReadAllBytes(System.IO.Path.Combine(Environment.CurrentDirectory, "Components", "ConsoleInput.dll"));
        //    AgentExecutable package = new AgentExecutable(comp);

        //    //DEBUG
        //    TcpClient client = new TcpClient();
        //    client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 47474));
        //    NetworkStream ns = client.GetStream();

        //    Networking.SendPackage(package, ns);

        //    Networking.SendPackage(new AgentExecutableParameters(null), ns);

        //    while (true)
        //    {
        //        if (ns.DataAvailable)
        //        {
        //            Console.WriteLine(((AgentExecutableResult)Networking.RecievePackage(ns)).Results.ToList()[0].ToString());
        //            break;
        //        }
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientguid"></param>
        /// <returns>A tuple containing a client info object and a bool indicating whether the found client is within the local network. Returns NULL if the client was found.</returns>
        private Tuple<ClientInfo, bool> FindClient(Guid? clientguid)
        {
            // Checks if TargetCalcClient is SET
            if (clientguid != null && clientguid != Guid.Empty)
            {
                // Checks if client exits in the global network
                Tuple<Guid, string> clienttuple = this.Server.AvailableClients.FirstOrDefault(x => x.Item1 == clientguid);

                if (clienttuple != null)
                {
                    Client client = this.Server.Clients.FirstOrDefault(x => x.ClientGuid == clientguid);

                    // Not null, if client is in internal network (connected to this server)
                    if (client != null)
                    {
                        IPEndPoint endpt = client.ClientTcp.Client.RemoteEndPoint as IPEndPoint;
                        IPAddress ip = endpt.Address;
                        return new Tuple<ClientInfo, bool>(new ClientInfo() { ClientGuid = client.ClientGuid, FriendlyName = client.FriendlyName, IpAddress = ip }, true);
                    }
                    else
                    {
                        ClientInfo ci = this.Server.GetRemoteClient((Guid)clientguid);
                        return new Tuple<ClientInfo, bool>(ci, false);
                    }
                }
            }

            // Nothing was found: returns null
            return new Tuple<ClientInfo, bool>(null, true);
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

        private void CreateClientWorkLoads()
        {
            for (int i = 0; i < this.JobParts.Count(); i++)
            {
                IPAddress address = ((IPEndPoint)(this.LocalAgents[i % this.LocalAgents.Count()].ClientTcp.Client.RemoteEndPoint)).Address;

                this.AgentWorkers.Add(new AgentWorker(address, this.JobParts[i]));
            }
        }

        private void OnActionCompleted(object sender, ActionCompletedEventArgs e)
        {
            AgentWorker worker = (AgentWorker)sender;
            this.AgentWorkers.Remove(worker);

            if (e.Result.JobState == JobState.Ok)
            {
                InternalNode action = worker.Action;

                if (action.TargetGuids.Count() == 0)
                {
                    return;
                }

                object[] results = e.Result.Results.ToArray();

                for (int i = 0; i < results.Count(); i++)
                {
                    InternalNode nextaction = this.JobParts.FirstOrDefault(x => x.NodeInputGuids.Contains(action.TargetGuids[i]));

                    if (nextaction != null)
                    {
                        nextaction.InputParameters[(int)action.TargetPorts[i] - 1] = results[i];
                    }
                }
            }
        }
    }
}
