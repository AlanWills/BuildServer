using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerClient
{
    [Command(CommandStrings.DeleteBranch)]
    public class DeleteBranchCommand : IClientCommand
    {
        public string Description
        {
            get { return "Remove a branch and all history from the build server."; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            string branchName = (parameters.Count == 0 || parameters[0] == CommandStrings.CurrentBranch) ? ClientSettings.CurrentBranch : parameters[0];
           
            client.Post(
                CommandStrings.DeleteBranch,
                new KeyValuePair<string, string>(CommandStrings.Branch, branchName));
        }
    }
}
