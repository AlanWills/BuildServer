using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerClient
{
    [Command(CommandStrings.GetLog)]
    public class GetBranchLogFileCommand : IClientCommand
    {
        public string Description
        {
            get { return "Retrieve the contents of the inputted log type file for the latest build of the inputted branch"; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        { 
            if (parameters.Count == 0 ||
                (parameters[0] != ParameterStrings.BuildLog && parameters[0] != ParameterStrings.TestLog))
            {
                Console.WriteLine("Invalid input for log type.  Please enter either '" + ParameterStrings.BuildLog + "' or '" + ParameterStrings.TestLog + "'");
                return;
            }

            string logType = parameters[0];
            string branchName = (parameters.Count == 1 || parameters[1] == ParameterStrings.CurrentBranch) ? ClientSettings.CurrentBranch : parameters[1];

            client.Get(
                CommandStrings.GetLog,
                new KeyValuePair<string, string>("logtype", logType),
                new KeyValuePair<string, string>(ParameterStrings.Branch, branchName));
        }
    }
}
