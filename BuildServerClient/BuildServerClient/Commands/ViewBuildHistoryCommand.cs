using BuildServerUtils;
using System.Collections.Generic;

namespace BuildServerClient
{
    [Command(CommandStrings.ViewBuildHistory)]
    public class ViewBuildHistoryCommand : IClientCommand
    {
        public string Description
        {
            get { return "View the testing state of previous builds for a branch"; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            string branchName = (parameters.Count == 0 || parameters[0] == CommandStrings.CurrentBranch) ? ClientSettings.CurrentBranch : parameters[0];
            string quantityString = parameters.Count > 1 ? parameters[1] : "10";

            client.Send(
                CommandStrings.ViewBuildHistory + " " +
                branchName + " " +
                quantityString);
        }
    }
}
