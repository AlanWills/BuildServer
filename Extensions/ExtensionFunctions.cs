using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                case TestState.Passed:
                    return "Passed";

                case TestState.Failed:
                    return "Failed";

                case TestState.Untested:
                    return "Untested";

                default:
                    return "Unknown State";
            }
        }

        public static string Colour(this TestState state)
        {
            switch (state)
            {
                case TestState.Passed:
                    return "green";

                case TestState.Failed:
                    return "red";

                case TestState.Untested:
                    return "blue";

                default:
                    Debug.Fail("Unresolved test state colour");
                    return "black";
            }
        }
    }
}
