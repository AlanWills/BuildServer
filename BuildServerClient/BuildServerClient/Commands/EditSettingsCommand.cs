using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BuildServerClient.ClientSettings;

namespace BuildServerClient
{
    [Command(CommandStrings.Settings)]
    public class EditSettingsCommand : IClientCommand
    {
        public string Description
        {
            get { return "Show or edit the Settings xml file"; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            if (parameters.Count == 0)
            {
                Console.WriteLine(ServerIPName + ": " + ServerIP);
                Console.WriteLine(ServerPortName + ": " + ServerPort);
                Console.WriteLine(EmailName + ": " + Email);
                Console.WriteLine(NotifySettingName + ": " + NotifySetting);
            }

            foreach (string parameter in parameters)
            {
                if (parameter == ServerIPName)
                {
                    // Edit
                }
                else if (parameter == ServerPortName)
                {
                    // Edit
                }
                else if (parameter == EmailName)
                {
                    // Edit
                }
                else if (parameter == NotifySettingName)
                {
                    // Edit
                }
            }
        }
    }
}
