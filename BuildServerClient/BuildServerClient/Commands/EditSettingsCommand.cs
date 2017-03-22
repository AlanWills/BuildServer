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

            // Do -1 so we ensure that if we enter the for loop we have the data parameter too
            for (int i = 0; i < parameters.Count - 1; i += 2)
            {
                string parameter = parameters[i];

                if (parameter == ServerIPName)
                {
                    ServerIP = parameters[i + 1];
                }
                else if (parameter == ServerPortName)
                {
                    int port = 0;
                    if (int.TryParse(parameters[i + 1], out port))
                    {
                        ServerPort = port;
                    }
                    else
                    {
                        Console.WriteLine("Port number was invalid");
                    }
                }
                else if (parameter == EmailName)
                {
                    Email = parameters[i + 1];
                }
                else if (parameter == NotifySettingName)
                {
                    Email = parameters[i + 1];
                }
            }

            SaveFile();
        }
    }
}
