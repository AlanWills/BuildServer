using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string branchName = parameters.Count > 0 ? parameters[0] : ClientSettings.CurrentBranch;
            string email = parameters.Count > 1 ? parameters[1] : ClientSettings.Email;
            string notifySetting = parameters.Count > 2 ? parameters[2] : ClientSettings.NotifySetting;

            client.ServerComms.Send(
                CommandStrings.BuildBranch + " " +
                branchName + " " +
                email + " " +
                notifySetting);
        }
    }
}
