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
                    object[] parameters = new object[] {values};
                    object resultObject = method.Invoke(classInstance,parameters);
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
            Type[] objectType = asbly.GetTypes();
            return objectType[0];
        }

        public static Assembly GetAssemblyFromDll()
        {
            Assembly asbly = Assembly.LoadFile("C:\\Users\\Josef\\Desktop\\Add.dll");
            return asbly;
        }
    }
}
