using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BuildServerClient
{
    [Command(CommandStrings.Quit)]
    public class QuitCommand : IClientCommand
    {
        public void Execute(BaseClient client, List<string> parameters)
        {
            client.ServerComms.Send(CommandStrings.Quit);
            Thread.Sleep(1000);
            Program.Running = false;
        }
    }
}