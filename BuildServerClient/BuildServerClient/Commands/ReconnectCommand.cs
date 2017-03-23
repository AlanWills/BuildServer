using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerClient.Commands
{
    [Command(CommandStrings.Reconnect)]
    public class ReconnectCommand : IClientCommand
    {
        public string Description
        {
            get { return "Attempt to reconnect to the Build Server"; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            if (!client.IsConnected)
            {
                string ip = parameters.Count > 0 ? parameters[0] : ClientSettings.ServerIP;
                if (string.IsNullOrEmpty(ip))
                {
                    Console.WriteLine("No Server IP passed in to command and no Settings file loaded");
                    return;
                }

                int port = ClientSettings.ServerPort;
                if (parameters.Count > 1)
                {
                    if (!int.TryParse(parameters[1], out port))
                    {
                        Console.WriteLine("No valid port passed in to command and no Settings file loaded");
                        return;
                    }
                }

                string error = "";
                if (client.TryConnect(ip, port, ref error))
                {
                    Console.WriteLine("Connection successful");
                }
                else
                {
                    Console.WriteLine("Connection was unsuccessful");
                    Console.WriteLine(error);
                }
            }
        }
    }
}
