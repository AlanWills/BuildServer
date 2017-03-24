using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

            HistoryFile file = new HistoryFile(server.Branches[branchName].OrderedHistoryFiles.First());
            file.Load();

            List<string> failedTests = file.FailedTests;
            if (failedTests.Count == 0)
            {
                return "Branch " + branchName + " has no failed tests";
            }

            StringBuilder str = new StringBuilder();

            foreach (string testName in file.FailedTests)
            {
                str.AppendLine("<p>" + testName + "</p>");
            }

            return str.ToString();
        }
    }
}
