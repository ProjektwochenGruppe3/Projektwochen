using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using dcs.core;

namespace ClientAgent
{
    class ExecutableHandler
    {
        public void WriteToFile(string path, byte[] data)
        {
            File.WriteAllBytes(path, data);
        }

        public void WriteResultToStream(object sender, ExecutableReadyEventArgs args)
        {
            Networking.SendPackage(new AgentExecutableResult(args.Job.Result, args.Job.State), args.Stream);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }
    }
}
