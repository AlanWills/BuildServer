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
            string branchName = parameters.Count == 0 ? ClientSettings.CurrentBranch : parameters[0];

            client.Get(
                CommandStrings.GetBranchStatus,
                new KeyValuePair<string, string>(ParameterStrings.Branch, branchName));
        }
    }
}
