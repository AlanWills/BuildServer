using BuildServerUtils;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static BuildServer.Branch;

namespace BuildServer
{
    [Command(CommandStrings.GetBranchStatus)]
    public class GetBranchStatusCommand : IServerCommand
    {
        public string Execute(BaseServer baseServer, NameValueCollection arguments)
        {
            Server server = baseServer as Server;

            List<string> branches = arguments.GetValues(CommandStrings.Branch)?.ToList();

            if (branches.Contains(CommandStrings.All))
            {
                // If the user passes all, we use all the branch statuses
                branches.Clear();
                branches.AddRange(server.Branches.Keys);
            }

            StringBuilder stringBuilder = new StringBuilder();

            foreach (string branchName in branches)
            {
                if (server.Branches.ContainsKey(branchName))
                {
                    GetBranchTestStateString(stringBuilder, server, branchName);
                }

                baseServer.Send(stringBuilder.ToString());
            }

            return stringBuilder.ToString();
        }
        
        private string GetBranchTestStateString(StringBuilder builder, Server server, string branchName)
        {
            Branch branch = server.Branches[branchName];

            builder.Append("<p><a style=\"font-weight:bold\" href=\"");
            builder.Append(server.BaseAddress + CommandStrings.ViewBuildHistory);
            builder.Append("?branch=" + branchName);
            builder.Append("\">");
            builder.Append(branchName);
            builder.Append("</a>");
            builder.Append(" has test status: ");
            builder.Append("<a style=\"color:");
            builder.Append(GetTestStateColour(branch.TestingState));
            builder.Append("\" href=\"");
            builder.Append(server.BaseAddress + CommandStrings.GetFailedTests);
            builder.Append("?branch=" + branchName);
            builder.Append("\">");
            builder.Append(branch.TestingState.DisplayString() + "</a></p>");

            return builder.ToString();
        }

        private string GetTestStateColour(TestState state)
        {
            switch (state)
            {
                case TestState.kPassed:
                    return "green";

                case TestState.kFailed:
                    return "red";

                case TestState.kUntested:
                    return "yellow";

                default:
                    Debug.Fail("Unresolved test state colour");
                    return "black";
            }
        }
    }
}