using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using dcs.core;
using Core.Component;
using Core.Network;

namespace EditorNetwork
{
    class EditorClientFrame
    {
        public void ConnecttoServer(EditorClient client)
        {
            client.TCPClientEditor.Connect(client.IpAddress, client.Port);
        }


        public void GetComponent(EditorClient client, JobRequest response)
        {
            NetworkStream stream = client.TCPClientEditor.GetStream();        
            
            while (client.IsWaiting)
            {
                if (stream.DataAvailable)
                {                  
                    object receiveddata = Networking.RecievePackage(stream);  
                    IEnumerable<Component> comp = (IEnumerable<Component>)receiveddata;                 
                    Networking.SendPackage(response, stream);                 
                }
            }
        }
        
    }
}
