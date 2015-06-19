using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dcs.core
{
    [Serializable]
    public class AgentExecutable
    {
        public AgentExecutable(byte[] assembly)
        {
            this.Assembly = assembly;
        }

        public byte[] Assembly { get; private set; }
    }
}
