using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildServer
{
    [Command(CommandStrings.BuildBranch)]
    public class BuildCurrentBranchCommand : IServerCommand
    {
        public void Execute(BaseServer baseServer, string arguments)
        {
            Server server = baseServer as Server;

            List<string> strings = arguments.Split(' ').ToList();
            strings.RemoveAll(x => string.IsNullOrEmpty(x));

            if (strings.Count != 3)
            {
                Console.WriteLine("Not enough parameters to build command");
                return;
            }

            string branchName = strings[0];
            Console.WriteLine("Branch name = " + branchName);

            string email = strings[1];
            Console.WriteLine("Email = " + email);

            string notifySetting = strings[2];
            Console.WriteLine("NotifySetting = " + notifySetting);

            if (!server.Branches.ContainsKey(branchName))
            {
                server.Branches.Add(branchName, new Branch(branchName));
            }

            server.Branches[branchName].Build(email, notifySetting);
        }
    }
}