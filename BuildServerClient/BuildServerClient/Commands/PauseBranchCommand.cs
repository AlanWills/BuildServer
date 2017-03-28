using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerClient.Commands
{
    [Command(CommandStrings.PauseBranch)]
    public class PauseBranchCommand : IClientCommand
    {
        public string Description
        {
            get { return "Pause a branch so that no subsequent build requests will queue or trigger builds."; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            string branchName = (parameters.Count == 0 || parameters[0] == CommandStrings.CurrentBranch) ? ClientSettings.CurrentBranch : parameters[0];

            client.Post(
                CommandStrings.PauseBranch,
                new KeyValuePair<string, string>(CommandStrings.Branch, branchName));
        }
    }
}