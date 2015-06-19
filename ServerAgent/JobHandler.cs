using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network;
using dcs.core;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class JobHandler
    {
        public JobHandler(Server server)
        {
            this.Server = server;
        }

        private Server Server { get; set; }

        private List<JobRequest> ActiveJobs { get; set; }

        public void NewJob(EditorJob job)
        {
            if (job.)
        }
    }
}
