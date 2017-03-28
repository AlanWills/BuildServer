using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace BuildServer
{ 
    [Command(CommandStrings.AddBranch)]
    public class AddBranchCommand : IServerCommand
    {
        public string Execute(BaseServer baseServer, NameValueCollection arguments)
        {
            Server server = baseServer as Server;
            
            string branchName = arguments.GetValues(CommandStrings.Branch)?[0];
            
            if (server.Branches.ContainsKey(branchName))
            {
                return "Branch already registered on build server";
            }

            server.Branches.Add(branchName, new Branch(branchName));

            return "Branch added to Build Server";
        }
    }
}