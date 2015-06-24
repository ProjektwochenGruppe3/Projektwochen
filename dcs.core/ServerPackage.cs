using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network;

namespace dcs.core
{
    public class ServerPackage
    {
        public ServerPackage(MessageCode code, string payload)
        {
            this.MessageCode = code;
            this.Payload = payload;
        }

        public MessageCode MessageCode { get; set; }

        public string Payload { get; set; }
    }
}
