using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public class InternalNode
    {
        /// <summary>
        /// Gets or sets the component that is being used to calculate the input parameters fot the target component.
        /// </summary>
        public Component Component { get; set;}

        /// <summary>
        /// Gets or sets the list of GUIDs of the component sending the Input Parameters to this Component.
        /// </summary>
        public List<Guid> NodeInputGuids { get; set; }

        /// <summary>
        /// Gets or sets the list of GUIDs of the component recieving the Input Parameters.
        /// </summary>
        public List<Guid> TargetGuids { get; set; }

        public List<uint> TargetPorts { get; set; }

        /// <summary>
        /// Gets or sets the input parameters for the target component.
        /// </summary>
        public List<object> InputParametersForTarget { get; set; }
    }
}
