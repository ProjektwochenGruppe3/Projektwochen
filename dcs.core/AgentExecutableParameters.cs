using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dcs.core
{
    [Serializable]
    public class AgentExecutableParameters
    {
        public AgentExecutableParameters(IEnumerable<object> paramemters)
        {
            this.Parameters = paramemters;
        }

        public IEnumerable<object> Parameters { get; private set; }
    }
}
