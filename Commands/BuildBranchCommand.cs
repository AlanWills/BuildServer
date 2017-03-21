using BuildServerUtils;
using System;

namespace BuildServer
{
    [Command(CommandStrings.BuildBranch)]
    public class BuildCurrentBranchCommand : IServerCommand
    {
        public void Execute(BaseServer server)
        {
            throw new NotImplementedException();
        }
    }
}