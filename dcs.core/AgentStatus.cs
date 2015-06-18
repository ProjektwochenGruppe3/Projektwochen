using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dcs.core
{
    [Serializable]
    public class AgentStatus
    {
        public AgentStatus(Guid requestguid, Guid agentguid, string friendlyname, int cpuload)
        {
            this.AgentStatusRequestGuid = requestguid;
            this.AgentGuid = agentguid;
            this.FriendlyName = friendlyname;
            this.CpuLoad = cpuload;
        }

        public Guid AgentStatusRequestGuid { get; private set; }

        public Guid AgentGuid { get; private set; }

        public string FriendlyName { get; private set; }

        public int CpuLoad { get; private set; }
    }
}
