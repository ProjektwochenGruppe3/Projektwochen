using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Component;
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
                try
                {
                    Assembly ass = Assembly.LoadFile(item);

                    if (ass.IsDefined(typeof(IComponent)))
                    {
                        foreach (var comp in ServerOperations.ConvertToComponent(ass))
                        {
                            components.Add(comp);
                        }
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
                    Assembly ass = Assembly.LoadFile(item);

                    if (ass.IsDefined(typeof(Component)))
                    {
                        var instance = Activator.CreateInstance(ass.GetType("Component", true)) as Component;

                        if (instance != null)
                        {
                            components.Add(instance);
                        }
                    }
                }
                catch
                {
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
                types = assembly.GetTypes().Where(x => x.IsDefined(typeof(IComponent))).ToArray();
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
