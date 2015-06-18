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
using System.Threading;

namespace EditorNetwork
{
    class EditorClientFrame
    {
        
        /// <summary>
        /// Method for recieving data from 
        /// </summary>
        /// <param name="client">Object of type EditorClient.</param>
        /// <param name="response"></param>
        /// <returns>Returns an IEnumberable of type Component if data was successfully recieved. Returns null </returns>
        

        public void SendJobRequest(EditorClient client, JobRequest response)
        {
            NetworkStream stream = client.TCPClientEditor.GetStream();
            Networking.SendPackage(response, stream);
            client.IsWaiting = false;
        }

    }
}
