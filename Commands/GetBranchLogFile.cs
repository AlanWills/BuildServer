using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.IO;

namespace BuildServer
{
    [Command(CommandStrings.GetLog)]
    public class GetBranchLogFile : IServerCommand
    {
        public string Execute(BaseServer baseServer, NameValueCollection arguments)
        {
            Server server = baseServer as Server;

            string[] logTypes = arguments.GetValues("logtype");
            if (logTypes == null)
            {
                return "Specify a log type using logtype=[branch_name]";
            }

            string logType = logTypes.Length > 0 ? logTypes[0] : "";
            if ((logType != CommandStrings.BuildLog) && (logType != CommandStrings.TestLog))
            {
                return "Inputted log type was invalid.  Please enter '" + CommandStrings.BuildLog + "' or '" + CommandStrings.TestLog + "'";
            }

            string[] branches = arguments.GetValues(CommandStrings.Branch);
            if (branches == null)
            {
                return "Specify a branch using branch=[branch_name]";
            }

            string branchName = branches.Length > 0 ? branches[0] : "";

            if (!server.Branches.ContainsKey(branchName))
            {
                return branchName + " not registered on server";
            }

            if (server.Branches[branchName].TestingState == Branch.TestState.Untested)
            {
                return branchName + " has not been built yet";
            }

            List<string> historyFiles = server.Branches[branchName].OrderedHistoryFiles;
            string fileName = logType == CommandStrings.BuildLog ? "BuildLog.txt" : "TestLog.txt";
            string logFile = Path.Combine(Directory.GetParent(historyFiles[0]).FullName, fileName);

            if (!File.Exists(logFile))
            {
                return "Could not find inputted log type for latest build";
            }

            HTMLWriter writer = new HTMLWriter();
            writer.CreatePreservedParagraph(File.ReadAllText(logFile));

            return writer.ToString();
        }
    }
}
