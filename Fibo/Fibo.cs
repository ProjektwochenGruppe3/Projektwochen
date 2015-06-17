using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Component;

namespace Fibo
{
    public class Fibo : IComponent
    {
        public Fibo()
        {
            this.ComponentGuid = new Guid();
            this.FriendlyName = "3_Fibo";
            this.InputHints = new string[] { "int" };
            this.OutputHints = new string[] { "int" };

        }

        public Guid ComponentGuid
        {
            get;
            private set;
        }

        public IEnumerable<object> Evaluate(IEnumerable<object> values)
        {
            int number;
            int a = 0;
            int b = 1;
            List<object> intList = new List<object>();
            intList = values.ToList();
            number = (int)intList[0];
            List<object> resultList = new List<object>();

            resultList[0] = a + b;
            return resultList;
        }

        public string FriendlyName
        {
            get;
            private set;
        }

        public IEnumerable<string> InputHints
        {
            get;
            private set;
        }

        public IEnumerable<string> OutputHints
        {
            get;
            private set;
        }
    }
}
