using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerClient.Commands
{
    [Command(CommandStrings.GetFailedTests)]
    public class GetFailedTestsCommand : IClientCommand
    {
        public string Description
        {
            get { return "Obtain all of the names of the tests that failed in the most recent build of a branch"; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            string branchName = (parameters.Count == 0 || parameters[0] == CommandStrings.CurrentBranch) ? ClientSettings.CurrentBranch : parameters[0];

            client.Send(CommandStrings.GetFailedTests + " " + branchName);
        }
    }
}
