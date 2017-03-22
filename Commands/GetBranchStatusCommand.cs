using BuildServerUtils;
using System.Collections.Generic;
using System.Text;

namespace BuildServer
{
    [Command(CommandStrings.GetBranchStatus)]
    public class GetBranchStatusCommand : IServerCommand
    {
        public void Execute(BaseServer baseServer, List<string> arguments)
        {
            Server server = baseServer as Server;

            if (arguments.Contains(CommandStrings.All))
            {
                // If the user passes all, we use all the branch statuses
                arguments.Clear();
                arguments.AddRange(server.Branches.Keys);
            }

            foreach (string branchName in arguments)
            {
                StringBuilder stringBuilder = new StringBuilder("Branch " + branchName + " does not exist on Build Server");
                if (server.Branches.ContainsKey(branchName))
                {
                    stringBuilder.Clear();
                    stringBuilder.Append("Branch " + branchName + " has status: " + server.Branches[branchName].TestingState.DisplayString());
                }

                baseServer.Send(stringBuilder.ToString());
            }
        }
    }
}