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
        public Guid ComponentGuid
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<object> Evaluate(IEnumerable<object> values)
        {
            throw new NotImplementedException();
        }

        public string FriendlyName
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<string> InputHints
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<string> OutputHints
        {
            get;
        }
    }
}
