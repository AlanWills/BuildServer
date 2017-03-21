using BuildServerUtils;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerClient
{
    [Command(CommandStrings.Help)]
    public class HelpCommand : IClientCommand
    {
        public string Description
        {
            get { return "List all of the available commands"; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            foreach (KeyValuePair<string, IClientCommand> command in Program.CommandRegistry)
            {
                Console.WriteLine("Command: " + command.Key + " - " + command.Value.Description);
            }
        }
    }
}
