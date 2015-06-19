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
        public static IEnumerable<object> InvokeMethod(Type type, IEnumerable<object> values)
        {
            IEnumerable<object> result = null;
            if (type != null)
            {
                MethodInfo method = type.GetMethod("Evaluate");
                if (method != null)
                {
                    object classInstance = Activator.CreateInstance(type, null);
                    object[] parameters = new object[] { values };
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

        public static Assembly GetAssemblyFromDll()
        {
            Assembly asbly = Assembly.LoadFile("C:\\Users\\Josef\\Desktop\\Add_Test.dll");
            return asbly;
        }
    }
}
