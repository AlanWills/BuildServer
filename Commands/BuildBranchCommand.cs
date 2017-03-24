using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace BuildServer
{
    [Command(CommandStrings.BuildBranch)]
    public class BuildBranchCommand : IServerCommand
    {
        public string Execute(BaseServer baseServer, NameValueCollection arguments)
        {
            Server server = baseServer as Server;

            if (arguments.Count != 3)
            {
                return "Not enough parameters to build command";
            }

            string branchName = arguments.GetValues(CommandStrings.Branch)?[0];
            string email = arguments.GetValues("email")?[0];
            string notifySetting = arguments.GetValues("only_email_on_fail")?[0];

            if (!server.Branches.ContainsKey(branchName))
            {
                server.Branches.Add(branchName, new Branch(branchName));
            }

            server.Branches[branchName].Build(email, notifySetting);

            return "build Command received by Build Server";
        }
    }
}