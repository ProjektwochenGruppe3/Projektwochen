using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientAgent
{
    public class ExecutableInterfaceProblemException : ArgumentException
    {
        public ExecutableInterfaceProblemException(string variable)
            : base(variable)
        {
        }
    }
}
