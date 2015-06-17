using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Component;

namespace Prim
{
    public class Prim : IComponent
    {
        public Prim()
        {
            this.ComponentGuid = new Guid();
            this.FriendlyName = "3_Prim";
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
            List<object> intList = new List<object>();
            intList = values.ToList();
            number = (int)intList[0];
            List<object> resultList = new List<object>();

            int index = 0;
            int count = 1;

            while(index != number)
            {
                count++;

                if(CheckPrime(count))
                {
                    index++;
                }
            }

            resultList[0] = count;
            return resultList;
        }

        public bool CheckPrime(int number)
        {
            for (int i = 2; i <= number - 1; i++)
            {
                if(number % i == 0)
                {
                    return false;
                }
            }
            
            return true;
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
