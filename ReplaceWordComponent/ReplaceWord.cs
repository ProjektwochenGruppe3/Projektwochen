using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPrime
{
    public class ReplaceWord
    {
        public ReplaceWord()
        {
            this.ComponentGuid = new Guid();
            this.FriendlyName = "3_ReplaceWord";
            this.InputHints = new string[] { "string", "string", "string" };
            this.OutputHints = new string[] { "string" };
        }

        public Guid ComponentGuid
        {
            get;
            private set;
        }

        public IEnumerable<object> Evaluate(IEnumerable<object> values)
        {
            string text;
            string oldVal;
            string newVal;

            List<object> stringList = new List<object>();
            stringList = values.ToList();
            text = (string)stringList[0];
            oldVal = (string)stringList[1];
            newVal = (string)stringList[2];

            List<string> resultList = new List<string>();
            string newtext = text.Replace(oldVal, newVal);
            resultList.Add(newtext);
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