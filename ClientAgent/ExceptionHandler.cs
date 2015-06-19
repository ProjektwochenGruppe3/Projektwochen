using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientAgent
{
    public static class ExceptionHandler
    {
        public static event EventHandler<OnExecutableFailedEventArgs> OnExecutableFailed;

        public static event EventHandler<OnExecutableInterfaceProblemEventArgs> OnExecutableInterfaceProblem;

        public static void FireOnExecutableInterfaceProblem(object sender, OnExecutableInterfaceProblemEventArgs args)
        {
            if (OnExecutableInterfaceProblem != null)
            {
                OnExecutableInterfaceProblem(sender, args);
            }
        }

    }
}
