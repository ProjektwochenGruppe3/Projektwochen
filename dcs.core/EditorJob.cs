using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network;

namespace dcs.core
{
    public class EditorJob : JobRequest
    {
        public JobState JobState { get; set; }
    }
}
