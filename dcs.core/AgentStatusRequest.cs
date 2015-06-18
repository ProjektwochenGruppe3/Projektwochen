using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dcs.core
{
    [Serializable]
    public class AgentStatusRequest
    {
        public AgentStatusRequest(Guid guid)
        {
            this.KeepAliveRequestGuid = guid;
        }

        public Guid KeepAliveRequestGuid { get; private set; }
    }
}
