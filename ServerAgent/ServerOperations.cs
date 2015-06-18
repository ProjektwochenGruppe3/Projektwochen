using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Network;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public static class ServerOperations
    {

        internal static List<Component> GetLocalComponents()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Components");
            List<Component> components = new List<Component>();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return components;
            }

            foreach (var item in Directory.GetFiles(path, "*.dll"))
            {
                
            }

            return components;
        }
    }
}
