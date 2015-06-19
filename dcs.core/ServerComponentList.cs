using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network;

namespace dcs.core
{
    [Serializable]
    public class ServerComponentList
    {
        public ServerComponentList(List<Component> components, List<Tuple<Guid, string>> clients)
        {
            this.ComponentList = components;
            this.AvailableClients = clients;
        }

        public List<Component> ComponentList { get; set; }

        public List<Tuple<Guid, string>> AvailableClients { get; set; }
    }
}
