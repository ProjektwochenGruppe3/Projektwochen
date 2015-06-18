using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ClientAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            //Client c = new Client(args[0],int.Parse(args[1]));

            IEnumerable<object> result = ComponentExecuter.InvokeMethod(ComponentExecuter.GetTypeFromAssembly(ComponentExecuter.GetAssemblyFromDll()), new object[] { 1, 2 });
            foreach (object o in result.ToList())
            {
                Console.WriteLine(o.ToString());
            }
            Console.ReadKey();
        }
    }
}
