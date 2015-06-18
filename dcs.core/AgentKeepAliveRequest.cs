using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dcs.core
{
    [Serializable]
    public class AgentKeepAliveRequest
    {
        public AgentKeepAliveRequest(Guid guid)
        {
            this.KeepAliveRequestGuid = guid;
        }

        public Guid KeepAliveRequestGuid { get; private set; }
    }
}
