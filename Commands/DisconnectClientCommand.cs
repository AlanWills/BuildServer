using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServer
{
    [Command(CommandStrings.Quit)]
    public class DisconnectClientCommand : IServerCommand
    {
        public void Execute(BaseServer server, List<string> arguments)
        {
            server.DisconnectClient();
        }
    }
}
