using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network;

namespace dcs.core
{
    public class ServerComponentList
    {
        public ServerComponentList(List<Component> components)
        {
            this.ComponentList = components;
        }

        public List<Component> ComponentList { get; set; }
    }
}
