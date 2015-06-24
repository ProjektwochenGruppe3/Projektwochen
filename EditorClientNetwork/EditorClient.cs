using Core.Network;
using dcs.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EditorNetwork
{
    public class EditorClient
    {     
        public EditorClient(IPAddress ip, int port)
        {
            this.IpAddress = ip;
            this.Port = port;
            this.TCPClientEditor = new TcpClient();
            //this.TCPClientEditor = new TcpClient(new IPEndPoint(this.IpAddress, this.Port));
            this.IsWaiting = false;
            this.IsAlive = false;
            this.State = ClientState.Disconnected;
        }
       
        public IPAddress IpAddress { get; private set; }

        public int Port { get; set; }

        public TcpClient TCPClientEditor { get; set; }
        
        public bool IsWaiting { get; set; }

        public bool IsAlive { get; set; }

        public ClientState State {get; set;}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns>Returns true if it was possible to establish a Tcp connection. Returns false if not.</returns>
        public bool ConnecttoServer()
        {            
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000;

            int counter = 0;

            while (counter <= 6000)
            {
                try
                {
                    counter = counter + 60;
                    if (this.IpAddress != null)
                    {
                        this.TCPClientEditor.Connect(this.IpAddress, this.Port);
                        this.State = ClientState.Connected;
                        this.IsAlive = true;
                        this.IsWaiting = true;
                        break;
                    }
                    else
                    {
                        this.State = ClientState.Disconnected;
                    }

                }

                catch (Exception e)
                {
                    this.State = ClientState.Disconnected;
                    Thread.Sleep(60);
                    throw new SocketException();
                    
                }
            }

            if (this.State == ClientState.Connected)
            {
                return true;
            }

            else
            {
                return false;
            }
        }
        /// <summary>
        /// Method for recieving data from the server.
        /// </summary>
        /// <param name="client">Object of type EditorClient.</param>
        /// <param name="response"></param>
        /// <returns>Returns an IEnumberable of type Component if data was successfully recieved. Returns null </returns>
        public ServerComponentList GetComponent()
        {
            NetworkStream stream = this.TCPClientEditor.GetStream();
            ServerComponentList  scl = null;
            while (this.IsWaiting)
            {
                if (stream.DataAvailable)
                {
                    object receiveddata = Networking.RecievePackage(stream);
                    scl = (ServerComponentList)receiveddata;
                    this.IsWaiting = false;
                }
                Thread.Sleep(50);
            }
            if (scl != null)
            {
                return scl;
            }
            else
            {
                return null;
            }

        }

        public void SendJobRequest(EditorJob response)
        {
            NetworkStream stream = this.TCPClientEditor.GetStream();
            Networking.SendPackage(response, stream);
            this.IsWaiting = false;
        }

        public void CloseDown()
        {
            try
            {
                this.TCPClientEditor.GetStream().Close();
                this.TCPClientEditor.Close();
            }
        }
    }
}
