using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerClient.Commands
{
    [Command(CommandStrings.ResumeBranch)]
    public class ResumeBranchCommand : IClientCommand
    {
        public string Description
        {
            get { return "Resume a branch so that subsequent build requests will queue or trigger builds.  If a build was queued before pausing, it will be triggered automatically."; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            string branchName = (parameters.Count == 0 || parameters[0] == ParameterStrings.CurrentBranch) ? ClientSettings.CurrentBranch : parameters[0];

            client.Post(
                CommandStrings.ResumeBranch,
                new KeyValuePair<string, string>(ParameterStrings.Branch, branchName));
        }
    }
}