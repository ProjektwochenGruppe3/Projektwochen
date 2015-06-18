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
            this.Executable = false;
        }

        public Assembly Job_Code { get; set; }

        public IEnumerable<object> Params { get; set; }

        public bool Executable { get; set; }
    }
}
