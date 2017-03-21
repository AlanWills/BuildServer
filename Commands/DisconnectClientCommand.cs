using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServer
{
    [Command(CommandStrings.DisconnectClient)]
    public class DisconnectClientCommand : IServerCommand
    {
        public void Execute(BaseServer server)
        {
            server.ClientComms.Disconnect();
        }
    }
}
