using BuildServerUtils;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

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
                    stringBuilder.AppendLine("<p>Branch " + branchName + " has status: " + server.Branches[branchName].TestingState.DisplayString() + "</p>");
                }

                baseServer.Send(stringBuilder.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}