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
        public EditorHandler(List<Component> localcomponents, List<Component> remotecomponents)
        {
            this.ConnectedEditors = new List<Editor>();
            this.ComponentList = localcomponents;

            foreach (var item in remotecomponents)
            {
                this.ComponentList.Add(item);
            }

            this.Listener = new TcpListener(IPAddress.Any, 30000);
        }

        private List<Editor> ConnectedEditors { get; set; }

        private Thread ListenerThread { get; set; }

        private TcpListener Listener { get; set; }

        private List<Component> ComponentList { get; set; }

        private void ListenerWorker()
        {
            this.Listener.Start();

            while (true)
            {
                TcpClient neweditorTcp = this.Listener.AcceptTcpClient();
                Thread neweditorThread = new Thread(new ParameterizedThreadStart(EditorWorker));
                Editor editor = new Editor(neweditorTcp, neweditorThread);
                this.ConnectedEditors.Add(editor);
                neweditorThread.Start(editor);

                Console.WriteLine("Editor connected");

                Thread.Sleep(10);
            }
        }

        private void EditorWorker(object obj)
        {
            Editor editor = obj as Editor;
            NetworkStream ns = editor.TcpClient.GetStream();
            
            try
            {
                Networking.SendPackage(this.ComponentList, ns);
            }
            catch
            {

            }


            while (true)
            {
                if (ns.DataAvailable)
                {
                    //Networking.RecievePackage(ns);

                    break;
                }

                Thread.Sleep(10);
            }
        }
    }
}
