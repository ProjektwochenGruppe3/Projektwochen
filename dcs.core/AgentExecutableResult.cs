using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network;

namespace dcs.core
{
    [Serializable]
    public class AgentExecutableResult
    {
        public AgentExecutableResult(IEnumerable<object> results, JobState state)
        {
            this.JobState = state;
            this.Results = results;
        }

        public IEnumerable<object> Results { get; private set; }

        public JobState JobState { get; private set; }
    }
}
