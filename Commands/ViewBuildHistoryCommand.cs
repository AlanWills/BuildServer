using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServer
{
    [Command(CommandStrings.ViewBuildHistory)]
    public class ViewBuildHistoryCommand : IServerCommand
    {
        public void Execute(BaseServer baseServer, List<string> arguments)
        {
            Server server = baseServer as Server;
            string branchName = arguments.Count > 0 ? arguments[0] : "";

            if (!server.Branches.ContainsKey(branchName))
            {
                server.Send("Branch " + branchName + " not registered on server");
                return;
            }

            string quantityString = arguments.Count > 1 ? arguments[1] : "10";

            List<string> historyFiles = server.Branches[branchName].OrderedHistoryFiles;
            if (quantityString == CommandStrings.All)
            {
                foreach (string file in historyFiles)
                {
                    SendHistoryFileInfo(server, file);
                }
            }
            else
            {
                int quantity = 0;
                if (int.TryParse(quantityString, out quantity))
                {
                    for (int i = 0, n = Math.Min(quantity, historyFiles.Count); i < n; ++i)
                    {
                        SendHistoryFileInfo(server, historyFiles[i]);
                    }
                }
                else
                {
                    server.Send(quantityString + " is not a valid quantity");
                }
            }
        }

        private void SendHistoryFileInfo(BaseServer baseServer, string filePath)
        {
            HistoryFile historyFile = new HistoryFile(filePath);
            historyFile.Load();

            baseServer.Send(historyFile.Status.DisplayString());
        }
    }
}
