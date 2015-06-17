using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ClientAgent
{
    class AtomicJob
    {
        public AtomicJob(Assembly asbly)
        {
            this.Job_Code = asbly;
        }

        public Assembly Job_Code { get; set; }
    }
}
