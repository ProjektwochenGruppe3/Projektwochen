using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dcs.core;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class ActionCompletedEventArgs : EventArgs
    {
        public ActionCompletedEventArgs(AgentExecutableResult res)
        {
            this.Result = res;
        }

        public AgentExecutableResult Result { get; set; }
    }
}
