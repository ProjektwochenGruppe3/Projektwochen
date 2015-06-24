using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Network;
using dcs.core;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class EditorHandler
    {
        public EditorHandler(Server server)
        {
            this.ConnectedEditors = new List<Editor>();
            this.Server = server;
            this.Listener = new TcpListener(IPAddress.Any, 30000);
            this.ListenerThread = new Thread(new ThreadStart(this.ListenerWorker));
            this.ListenerThread.Start();
        }

        private Server Server { get; set; }

        private List<Editor> ConnectedEditors { get; set; }

        private Thread ListenerThread { get; set; }

        private TcpListener Listener { get; set; }

        private void ListenerWorker()
        {
            this.Listener.Start();

            while (true)
            {
                TcpClient neweditorTcp = this.Listener.AcceptTcpClient();
                Thread neweditorThread = new Thread(new ParameterizedThreadStart(EditorWorker));
                Editor editor = new Editor(neweditorTcp, neweditorThread);
                this.ConnectedEditors.Add(editor);
                editor.EditorDisconnected += this.OnEditorDisconnected;
                neweditorThread.Start(editor);

                Console.WriteLine("Editor connected");

                Thread.Sleep(50);
            }
        }

        private void EditorWorker(object obj)
        {
            Editor editor = obj as Editor;
            NetworkStream ns = editor.TcpClient.GetStream();

            try
            {
                Networking.SendPackage(new ServerComponentList(this.Server.AvailableComponents, this.Server.AvailableClients), ns);
            }
            catch
            {
                editor.OnEditorDisconnected();
                return;
            }

            while (true)
            {
                try
                {
                    if (ns.DataAvailable)
                    {
                        EditorJob job = Networking.RecievePackage(ns) as EditorJob;

                        if (job != null)
                        {
                            List<Component> components = new List<Component>();

                            foreach (var item in this.Server.AvailableComponents)
                            {
                                components.Add(item);
                            }

                            List<Component> locals = this.Server.LocalComponents.ToList();
                            List<Client> agents = this.Server.Clients.ToList();

                            JobHandler handler = new JobHandler(this.Server, locals, components, agents);
                            Thread t = new Thread(new ParameterizedThreadStart(handler.NewJob));
                            t.IsBackground = true;
                            t.Start(job);
                        }
                    }
                }
                catch
                {
                    editor.OnEditorDisconnected();
                    break;
                }

                Thread.Sleep(42);
            }
        }

        private void OnEditorDisconnected(object sender, EventArgs e)
        {
            Editor editor = sender as Editor;

            this.ConnectedEditors.Remove(editor);
        }
    }
}
