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
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns>Returns true if it was possible to establish a Tcp connection. Returns false if not.</returns>
        public bool ConnecttoServer(EditorClient client)
        {
            client.IsAlive = true;
            client.IsWaiting = true;
            client.TCPClientEditor = new TcpClient();
            client.State = ClientState.Disconnected;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000;
            //this.ClientThread = new Thread(new ThreadStart(FirstConnect));
            //this.ClientThread.Start();
            //this.ClientThread.Join();
            
            int counter = 0;

            while (counter <= 6000)
            {
                try
                {
                    counter = counter + 60;
                    if (client.IpAddress != null && client.Port != null)
                    {
                    client.TCPClientEditor.Connect(client.IpAddress, client.Port);
                    client.State = ClientState.Connected;
                    }
                    else
                    {
                        
                    }
                   
                }

                catch (Exception)
                {
                    client.State = ClientState.Disconnected;
                    Thread.Sleep(60);
                }
            }
            
            if (client.State == ClientState.Connected)
            {
                return true;
            }
            
            else 
            {
                return false;
            }           
        }
        /// <summary>
        /// Method for recieving data from 
        /// </summary>
        /// <param name="client">Object of type EditorClient.</param>
        /// <param name="response"></param>
        /// <returns>Returns an IEnumberable of type Component if data was successfully recieved. Returns null </returns>
        public IEnumerable<Component> GetComponent(EditorClient client, JobRequest response)
        {
            NetworkStream stream = client.TCPClientEditor.GetStream();
           IEnumerable<Component> comp = null;
            while (client.IsWaiting)
            {              
                if (stream.DataAvailable)
                {
                    object receiveddata = Networking.RecievePackage(stream);
                    comp = (IEnumerable<Component>)receiveddata;
                    client.IsWaiting = false;
                }
               Thread.Sleep(50);
            }
            if (comp != null)
            { 
                return comp;     
            }
            else
            {
                return null;
            }
           
        }

        public void SendJobRequest(EditorClient client, JobRequest response)
        {
            NetworkStream stream = client.TCPClientEditor.GetStream();
            Networking.SendPackage(response, stream);
            client.IsWaiting = false;
        }

    }
}
