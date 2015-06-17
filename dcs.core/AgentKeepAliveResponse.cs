using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dcs.core
{
    public class AgentKeepAliveResponse
    {
        public Guid KeepAliveRequestGuid { get; private set; }

        public string FriendlyName { get; private set; }

        public int CpuLoad { get; private set; }
    }
}
