using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerUtils
{
    public static class CommandStrings
    {
        public const string BuildBranch = "build";
        public const string Quit = "quit";
        public const string GetBranchStatus = "status";
        public const string Help = "help";
        public const string Settings = "settings";
        public const string Reconnect = "reconnect";
        public const string ViewBuildHistory = "history";
        public const string GetFailedTests = "failedtests";
        public const string GetLog = "log";
        public const string PauseBranch = "pause";
        public const string ResumeBranch = "resume";
        public const string DeleteBranch = "delete";
        public const string AddBranch = "add";
        public const string TestEmail = "testemail";
        public const string TestSlack = "testslack";
    }
}
