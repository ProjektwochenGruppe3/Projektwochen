using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ClientAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            bool inputOkay = false;
            string IPinput;
            string portInput;
            int port = 0;
            string path = Path.Combine(Environment.CurrentDirectory, "DLLs");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (var item in Directory.GetFiles(path, "*.dll"))
            {
                try
                {
                    File.Delete(item);
                }
                catch
                {
                }
            }
            IPAddress ip = null;
            Console.WriteLine("Please enter a name for the client: ");
            string name = Console.ReadLine();
            Console.Clear();
            while (!inputOkay)
            {
                Console.WriteLine("Please enter an IP-Adress: ");
                IPinput = Console.ReadLine();
                try
                {
                    ip = IPAddress.Parse(IPinput);
                    inputOkay = true;
                }
                catch
                {
                }
                Console.Clear();
            }
            inputOkay = false;
            while (!inputOkay)
            {
                Console.WriteLine("Please enter a port: ");
                portInput = Console.ReadLine();
                try
                {
                    port = int.Parse(portInput);
                    inputOkay = true;
                }
                catch
                {
                }
                Console.Clear();
            }
            Client c = new Client(ip,port,name);
        }
    }
}
