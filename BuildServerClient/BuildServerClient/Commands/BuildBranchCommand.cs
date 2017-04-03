using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValuePair = System.Collections.Generic.KeyValuePair<string, string>;

namespace BuildServerClient
{
    [Command(CommandStrings.BuildBranch)]
    public class BuildBranchCommand : IClientCommand
    {
        public string Description
        {
            get { return "Request a build on the Build Server"; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            string branchName = (parameters.Count == 0 || parameters[0] == ParameterStrings.CurrentBranch) ? ClientSettings.CurrentBranch : parameters[0];
            string email = parameters.Count > 1 ? parameters[1] : ClientSettings.Email;
            string notifySetting = parameters.Count > 2 ? parameters[2] : ClientSettings.NotifySetting;

            client.Post(
                CommandStrings.BuildBranch,
                new ValuePair(ParameterStrings.Branch, branchName),
                new ValuePair("email", email),
                new ValuePair("only_email_on_fail", notifySetting));
        }
    }
}
