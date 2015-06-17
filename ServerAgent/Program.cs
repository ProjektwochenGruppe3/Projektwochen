using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerAgent_PW_Josef_Benda_V1
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.StartServer();
            Console.ReadKey();
            server.Broadcast("Hi ich bin euer Server!");
            Console.ReadKey();
            server.StopServer();
        }
    }
}
