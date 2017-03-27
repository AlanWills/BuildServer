using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServer.Commands
{
    [Command(CommandStrings.GetFailedTests)]
    public class GetFailedTestsCommand : IServerCommand
    {
        public string Execute(BaseServer baseServer, NameValueCollection arguments)
        {
            string[] branches = arguments.GetValues(CommandStrings.Branch);

            if (branches.Length < 1)
            {
                return "No branch passed to " + CommandStrings.GetFailedTests;
            }

            Server server = baseServer as Server;
            string branchName = branches[0];
            
            if (!server.Branches.ContainsKey(branchName))
            {
                return "Branch " + branchName + " does not exist on the build server";
            }

            if (server.Branches[branchName].TestingState == Branch.TestState.kUntested)
            {
                return "Branch " + branchName + " has not been tested yet";
            }

            string[] dirs = arguments.GetValues("dir");

            if (dirs.Length < 1)
            {
                return "No directory passed to " + CommandStrings.GetFailedTests;
            }

            string dir = dirs[0];

            List<string> historyFiles = server.Branches[branchName].OrderedHistoryFiles;
            if (!historyFiles.Exists(x => Directory.GetParent(x).Name == dir))
            {
                return "Build with inputted directory does not exist";
            }

            HistoryFile file = new HistoryFile(historyFiles.Find(x => Directory.GetParent(x).Name == dir));
            file.Load();

            StringBuilder str = new StringBuilder();
            str.AppendLine("<a href=\"" + server.BaseAddress + CommandStrings.GetLog + "?logtype=" + CommandStrings.BuildLog + "&" + CommandStrings.Branch + "=" + branchName + "\">Build Log</a>");
            str.AppendLine("<a href=\"" + server.BaseAddress + CommandStrings.GetLog + "?logtype=" + CommandStrings.TestLog + "&" + CommandStrings.Branch + "=" + branchName + "\">Test Log</a>");

            List<string> failedTests = file.FailedTests;
            if (failedTests.Count == 0)
            {
                str.AppendLine("<p>Branch " + branchName + " has no failed tests</p>");
            }

            foreach (string testName in file.FailedTests)
            {
                str.AppendLine("<p>" + testName + "</p>");
            }

            return str.ToString();
        }
    }
}
