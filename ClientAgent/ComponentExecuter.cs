using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Core.Component;

namespace ClientAgent
{
    public static class ComponentExecuter
    {
        public static IEnumerable<object> InvokeMethod(AtomicJob job)
        {
            IEnumerable<object> result = null;
            if (job.ExecutableType != null)
            {
                MethodInfo method = job.ExecutableType.GetMethod("Evaluate");
                if (method != null)
                {
                    object classInstance = Activator.CreateInstance(job.ExecutableType, null);
                    object[] parameters = new object[] { job.Params };
                    object resultObject = method.Invoke(classInstance, parameters);
                    result = (IEnumerable<object>)resultObject;
                }
                else
                {
                    throw new ArgumentNullException("method");
                }
            }
            else
            {
                throw new ArgumentNullException("type");
            }
            return (IEnumerable<object>)result;
        }

        public static Type GetTypeFromAssembly(Assembly asbly)
        {
            Type returnType = null;
            Type[] objectTypes = asbly.GetTypes();
            int implementedInterfaces = 0;
            foreach (Type t in objectTypes)
            {
                if (t.GetInterface("IComponent", true) != null)
                {
                    returnType = t;
                    implementedInterfaces++;
                }
            }
            if (implementedInterfaces != 1)
            {
                throw new ExecutableInterfaceProblemException("asbly");
            }
            return returnType;
        }

        public static Assembly GetAssemblyFromDll(string path)
        {
            Assembly asbly = Assembly.LoadFile(path);
            return asbly;
        }
    }
}
