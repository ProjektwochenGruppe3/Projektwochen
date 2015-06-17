using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            Client c = new Client(args[0],int.Parse(args[1]));
            c.Connect();
            Console.ReadKey();
        }
    }
}
