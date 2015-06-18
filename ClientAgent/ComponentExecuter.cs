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
                    object[] parameters = values.ToArray();
                    object resultObject = method.Invoke(classInstance,parameters);
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

        public static Type GetAssembly(object dll)
        {
            Assembly asbly = (Assembly)dll;
            Type objectType = asbly.GetType("IComponent");
            return objectType;
        }
    }
}
