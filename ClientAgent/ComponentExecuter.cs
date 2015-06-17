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
        public static IEnumerable<object> InvokeMethod(object dll, IEnumerable<object> values)
        {
            Type objectType = dll.GetType();
            Assembly executionable = Assembly.GetAssembly(objectType);
            IComponent tmpClass = (IComponent)executionable;
            IEnumerable<object> result = tmpClass.Evaluate(values);
            return result;
        }
    }
}
