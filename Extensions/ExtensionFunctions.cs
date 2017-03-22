using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BuildServer.Branch;

namespace BuildServer
{
    public static class ExtensionFunctions
    {
        public static string DisplayString(this TestState testState)
        {
            switch (testState)
            {
                case TestState.kPassed:
                    return "Passed";

                case TestState.kFailed:
                    return "Failed";

                case TestState.kUntested:
                    return "Untested";

                default:
                    return "Unknown State";
            }
        }
    }
}
