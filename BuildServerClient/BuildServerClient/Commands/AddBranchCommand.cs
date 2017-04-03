using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerClient
{
    [Command(CommandStrings.AddBranch)]
    public class AddBranchCommand : IClientCommand
    {
        public string Description
        {
            get { return "Add a branch to the build server."; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            string branchName = (parameters.Count == 0 || parameters[0] == ParameterStrings.CurrentBranch) ? ClientSettings.CurrentBranch : parameters[0];
           
            client.Post(
                CommandStrings.AddBranch,
                new KeyValuePair<string, string>(ParameterStrings.Branch, branchName));
        }
    }
}
