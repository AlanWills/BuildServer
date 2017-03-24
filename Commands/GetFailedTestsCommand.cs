using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServer.Commands
{
    [Command(CommandStrings.GetFailedTests)]
    public class GetFailedTestsCommand : IServerCommand
    {
        public void Execute(BaseServer baseServer, List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                baseServer.Send("No branch passed to " + CommandStrings.GetFailedTests);
                return;
            }

            Server server = baseServer as Server;
            string branchName = arguments[0];
            
            if (!server.Branches.ContainsKey(branchName))
            {
                baseServer.Send("Branch " + branchName + " does not exist on the build server");
                return;
            }

            if (server.Branches[branchName].TestingState == Branch.TestState.kUntested)
            {
                baseServer.Send("Branch " + branchName + " has not been tested yet");
                return;
            }

            HistoryFile file = new HistoryFile(server.Branches[branchName].OrderedHistoryFiles.First());
            file.Load();

            List<string> failedTests = file.FailedTests;
            if (failedTests.Count == 0)
            {
                baseServer.Send("Branch " + branchName + " has no failed tests");
                return;
            }

            StringBuilder str = new StringBuilder();

            foreach (string testName in file.FailedTests)
            {
                str.AppendLine(testName);
            }

            baseServer.Send(str.ToString());
        }
    }
}
