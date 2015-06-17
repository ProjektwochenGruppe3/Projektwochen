using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dcs.core
{
    [Serializable]
    public class AgentKeepAliveResponse
    {
        public AgentKeepAliveResponse(Guid requestguid, Guid agentguid, string friendlyname, int cpuload)
        {
            this.KeepAliveRequestGuid = requestguid;
            this.AgentGuid = agentguid;
            this.FriendlyName = friendlyname;
            this.CpuLoad = cpuload;
        }

        public Guid KeepAliveRequestGuid { get; private set; }

        public Guid AgentGuid { get; private set; }

        public string FriendlyName { get; private set; }

        public int CpuLoad { get; private set; }
    }
}
