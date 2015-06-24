using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Network;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class RemoteServer
    {
        public RemoteServer(Guid guid, string friendlyname, List<Component> comps, List<ClientInfo> clients)
        {
            this.ServerGuid = guid;
            this.FriendlyName = friendlyname;
            this.RemoteClients = clients;
            this.RemoteComponents = comps;
        }

        public Guid ServerGuid { get; set; }

        public string FriendlyName { get; set; }

        public List<Component> RemoteComponents { get; set; }

        public List<ClientInfo> RemoteClients { get; set; }
    }
}
