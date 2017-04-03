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
        public const string CurrentBranch = "current";
        public const string All = "all";
        public const string Branch = "branch";
        public const string GetFailedTests = "failedtests";
        public const string GetLog = "log";
        public const string BuildLog = "build";
        public const string TestLog = "test";
        public const string Latest = "latest";
        public const string PauseBranch = "pause";
        public const string ResumeBranch = "resume";
        public const string DeleteBranch = "delete";
        public const string AddBranch = "add";
        public const string TestEmail = "testemail";
        public const string TestSlack = "testslack";
        public const string EmailAddress = "email";
        public const string SlackURL = "slackURL";
    }
}
