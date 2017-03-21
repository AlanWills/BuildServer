using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServer
{
    [Command(CommandStrings.GetBranchStatus)]
    public class GetBranchStatusCommand : IServerCommand
    {
        public void Execute(BaseServer baseServer, List<string> arguments)
        {
            if (arguments.Contains("all"))
            {
                // If the user passes all, we use all the branch statuses
                arguments.Clear();
                arguments.AddRange((baseServer as Server).Branches.Keys);
            }

            foreach (string branchName in arguments)
            {
                StringBuilder stringBuilder = new StringBuilder("Branch " + branchName + " does not exist on Build Server");
                if ((baseServer as Server).Branches.ContainsKey(branchName))
                {
                    stringBuilder.Clear();
                    stringBuilder.Append("Branch " + branchName + " has status: ");
                }

                baseServer.ClientComms.Send(stringBuilder.ToString());
            }
        }
    }
}