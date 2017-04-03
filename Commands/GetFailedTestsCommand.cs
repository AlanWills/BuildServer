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
            string[] branches = arguments.GetValues(ParameterStrings.Branch);

            if (branches == null)
            {
                return "No branch passed to " + CommandStrings.GetFailedTests;
            }

            Server server = baseServer as Server;
            string branchName = branches[0];
            
            if (!server.Branches.ContainsKey(branchName))
            {
                return branchName + " does not exist on the build server";
            }

            if (server.Branches[branchName].TestingState == Branch.TestState.Untested)
            {
                return branchName + " has not been tested yet";
            }

            string[] dirs = arguments.GetValues("dir");
            List<string> historyFiles = server.Branches[branchName].OrderedHistoryFiles;
            string dir = (dirs == null || dirs[0] == ParameterStrings.Latest) ? Directory.GetParent(historyFiles[0]).Name : dirs[0];

            if (!historyFiles.Exists(x => Directory.GetParent(x).Name == dir))
            {
                return "Build with inputted directory does not exist";
            }

            HistoryFile file = new HistoryFile(historyFiles.Find(x => Directory.GetParent(x).Name == dir));
            file.Load();

            HTMLWriter writer = new HTMLWriter();
            writer.CreateLink(server.BaseAddress + CommandStrings.ViewBuildHistory + "?branch=" + branchName, "Build History");
            writer.CreateH2("Build Information");
            writer.CreateH3("LogFiles");
            writer.CreateParagraph("")
                  .CreateLink(server.BaseAddress + CommandStrings.GetLog + "?logtype=" + ParameterStrings.BuildLog + "&" + ParameterStrings.Branch + "=" + branchName, "Build Log");
            writer.CreateParagraph("")
                  .CreateLink(server.BaseAddress + CommandStrings.GetLog + "?logtype=" + ParameterStrings.TestLog + "&" + ParameterStrings.Branch + "=" + branchName, "Test Log");

            List<string> failedTests = file.FailedTests;
            if (failedTests.Count == 0)
            {
                writer.CreateParagraph("Branch passed successfully");
            }

            writer.CreateH3("Failed Tests");

            foreach (string testName in file.FailedTests)
            {
                writer.CreateParagraph(testName);
            }

            return writer.ToString();
        }
    }
}
