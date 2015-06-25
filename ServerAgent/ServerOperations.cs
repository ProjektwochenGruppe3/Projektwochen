using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Core.Component;
using Core.Network;

namespace ServerAgent_PW_Josef_Benda_V1
{
    public static class ServerOperations
    {
        public static BinaryFormatter formatter = new BinaryFormatter();

        internal static void SaveComponent(Component component)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Components", component.ComponentGuid.ToString() + ".comp");

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    ServerOperations.formatter.Serialize(fs, component);
                    fs.Flush();
                }
            }
            catch
            {
            }
        }

        internal static byte[] GetComponentBytes(Guid compguid)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Components");

            foreach (var item in Directory.GetFiles(path, "*.dll"))
            {
                try
                {
                    Assembly ass = Assembly.LoadFile(item);

                    Type[] types = ass.GetTypes().Where(x => x.GetInterface("IComponent", true) != null).ToArray();

                    foreach (var type in types)
                    {
                        var instance = Activator.CreateInstance(type) as IComponent;

                        if (instance.ComponentGuid == compguid)
                        {
                            return File.ReadAllBytes(ass.Location);
                        }
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        internal static List<ClientInfo> GetClientInfos(List<Client> clients)
        {
            List<ClientInfo> clientinfos = new List<ClientInfo>();
            foreach (var item in clients)
            {
                ClientInfo ci = new ClientInfo();
                ci.ClientGuid = item.ClientGuid;
                ci.FriendlyName = item.FriendlyName;
                ci.IpAddress = ((IPEndPoint)item.ClientTcp.Client.RemoteEndPoint).Address;

                clientinfos.Add(ci);
            }
            return clientinfos;
        }

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
                try
                {
                    Assembly ass = Assembly.LoadFile(item);

                    foreach (var comp in ServerOperations.ConvertToComponent(ass))
                    {
                        components.Add(comp);
                    }
                }
                catch
                {
                }
            }

            foreach (var item in Directory.GetFiles(path, "*.comp"))
            {
                try
                {
                    using (FileStream fs = new FileStream(item, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        components.Add(ServerOperations.formatter.Deserialize(fs) as Component);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return components;
        }

        private static List<Component> ConvertToComponent(Assembly assembly)
        {
            Type[] types = null;
            List<Component> components = new List<Component>();

            try
            {
                types = assembly.GetTypes().Where(x => x.GetInterface("IComponent", true) != null).ToArray();
            }
            catch
            {
            }

            if (types == null)
            {
                return null;
            }

            foreach (var item in types)
            {
                var instance = Activator.CreateInstance(item) as IComponent;

                if (instance == null)
                {
                    continue;
                }

                try
                {
                    Component comp = new Component();
                    comp.ComponentGuid = instance.ComponentGuid;
                    comp.FriendlyName = instance.FriendlyName;
                    comp.InputHints = instance.InputHints;
                    comp.OutputHints = instance.OutputHints;
                    comp.IsAtomic = true;

                    components.Add(comp);
                }
                catch
                {
                }
            }

            return components;
        }
    }
}
