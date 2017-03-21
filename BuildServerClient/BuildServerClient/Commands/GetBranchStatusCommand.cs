using BuildServerUtils;
using System.Collections.Generic;
using System.Text;

namespace BuildServerClient
{
    [Command(CommandStrings.GetBranchStatus)]
    public class GetBranchStatusCommand : IClientCommand
    {
        public string Description
        {
            get { return "Return the latest build result of an inputted branch on the Build Server."; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            StringBuilder branchNames = new StringBuilder(1024);
            if (parameters.Count == 0)
            {
                // Use current branch name
                branchNames.Append(ClientSettings.CurrentBranch);
            }
            else
            {
                foreach (string branchName in parameters)
                {
                    branchNames.Append(branchName);
                    branchNames.Append(" ");
                }
            }

            client.ServerComms.Send(
                CommandStrings.GetBranchStatus + " " +
                branchNames.ToString());
        }
    }
}
