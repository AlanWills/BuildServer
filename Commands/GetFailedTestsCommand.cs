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

            if (branches == null)
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
            List<string> historyFiles = server.Branches[branchName].OrderedHistoryFiles;
            string dir = (dirs == null || dirs[0] == CommandStrings.Latest) ? Directory.GetParent(historyFiles[0]).Name : dirs[0];

            if (!historyFiles.Exists(x => Directory.GetParent(x).Name == dir))
            {
                return "Build with inputted directory does not exist";
            }

            HistoryFile file = new HistoryFile(historyFiles.Find(x => Directory.GetParent(x).Name == dir));
            file.Load();

            StringBuilder str = new StringBuilder("<h2>Build Information</h2>");
            str.AppendLine("<h3>Log Files</h3>");
            str.AppendLine("<p><a href=\"" + server.BaseAddress + CommandStrings.GetLog + "?logtype=" + CommandStrings.BuildLog + "&" + CommandStrings.Branch + "=" + branchName + "\">Build Log</a></p>");
            str.AppendLine("<p><a href=\"" + server.BaseAddress + CommandStrings.GetLog + "?logtype=" + CommandStrings.TestLog + "&" + CommandStrings.Branch + "=" + branchName + "\">Test Log</a></p>");

            List<string> failedTests = file.FailedTests;
            if (failedTests.Count == 0)
            {
                str.AppendLine("<p>Branch passed successfully</p>");
            }

            str.AppendLine("<h3>Failed Tests</h3>");

            foreach (string testName in file.FailedTests)
            {
                str.AppendLine("<p>" + testName + "</p>");
            }

            return str.ToString();
        }
    }
}
