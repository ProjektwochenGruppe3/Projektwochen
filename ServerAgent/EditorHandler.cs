using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Network;

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
        }

        private List<Editor> ConnectedEditors { get; set; }

        private Thread ListenerThread { get; set; }

        private List<Component> ComponentList { get; set; }
    }
}
