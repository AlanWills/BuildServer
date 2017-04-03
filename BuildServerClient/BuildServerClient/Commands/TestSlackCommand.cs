using BuildServerUtils;
using System.Collections.Generic;

namespace BuildServerClient
{
    [Command(CommandStrings.TestSlack)]
    public class TestSlackCommand : IClientCommand
    {
        public string Description
        {
            get { return "Send a test slack notification to check the slack configurations are correct"; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            client.Get(CommandStrings.TestSlack);
        }
    }
}
