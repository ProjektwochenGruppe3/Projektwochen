using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientAgent_PW_Cerima_Koini_V1
{
    class Program
    {
        static void Main(string[] args)
        {
            Client c = new Client();
            c.Connect("127.0.0.1", 1337);
            while (true)
            {
                c.SendMessageToServer();
            }
            Console.ReadKey();
        }
    }
}
