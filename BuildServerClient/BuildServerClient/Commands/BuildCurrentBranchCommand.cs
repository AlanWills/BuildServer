using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BuildServerClient
{
    [Command("buildcurrent")]
    public class BuildCurrentBranchCommand : IClientCommand
    {
        public string Description
        {
            get { return "Request a build on the Build Server for the current branch"; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            string email = parameters.Count > 0 ? parameters[0] : ClientSettings.Email;
            string notifySetting = parameters.Count > 1 ? parameters[1] : ClientSettings.NotifySetting;

            client.ServerComms.Send(
                CommandStrings.BuildBranch + " " + 
                ClientSettings.CurrentBranch + " " +
                email + " " +
                notifySetting);
        }
    }
}