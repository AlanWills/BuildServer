using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace BuildServer
{
    [Command(CommandStrings.PauseBranch)]
    public class PauseBranchCommand : IServerCommand
    {
        public string Execute(BaseServer baseServer, NameValueCollection arguments)
        {
            Server server = baseServer as Server;

            string[] branchNames = arguments.GetValues(CommandStrings.Branch);
            if (branchNames == null)
            {
                return "No branch passed to command";
            }

            string branchName = branchNames[0];
            if (!server.Branches.ContainsKey(branchName))
            {
                return "Branch" + branchName + " not registered on Build Server";
            }

            server.Branches[branchName].Pause();

            return CommandStrings.PauseBranch + " Command received by Build Server";
        }
    }
}