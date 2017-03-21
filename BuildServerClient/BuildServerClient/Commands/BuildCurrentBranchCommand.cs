using BuildServerUtils;
using System;

namespace BuildServerClient
{
    [Command("buildcurrent")]
    public class BuildCurrentBranchCommand : IClientCommand
    {
        public void Execute(BaseClient client)
        {
            client.ServerComms.Send(
                CommandStrings.BuildBranch + " " + 
                ClientSettings.CurrentBranch + " " + 
                ClientSettings.Email + " " + 
                ClientSettings.NotifySetting);
        }
    }
}