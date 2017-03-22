using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildServer
{
    [Command(CommandStrings.BuildBranch)]
    public class BuildCurrentBranchCommand : IServerCommand
    {
        public void Execute(BaseServer baseServer, List<string> arguments)
        {
            Server server = baseServer as Server;

            if (arguments.Count != 3)
            {
                Console.WriteLine("Not enough parameters to build command");
                return;
            }

            string branchName = arguments[0];
            Console.WriteLine("Branch name = " + branchName);

            string email = arguments[1];
            Console.WriteLine("Email = " + email);

            string notifySetting = arguments[2];
            Console.WriteLine("NotifySetting = " + notifySetting);

            if (baseServer.IsConnected)
            {
                // Notify the user
                baseServer.Send("Build Command received by Build Server");
            }

            if (!server.Branches.ContainsKey(branchName))
            {
                server.Branches.Add(branchName, new Branch(branchName));
            }

            server.Branches[branchName].Build(email, notifySetting);
        }
    }
}