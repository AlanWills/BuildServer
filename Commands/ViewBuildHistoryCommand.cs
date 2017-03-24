using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServer
{
    [Command(CommandStrings.ViewBuildHistory)]
    public class ViewBuildHistoryCommand : IServerCommand
    {
        public string Execute(BaseServer baseServer, NameValueCollection arguments)
        {
            Server server = baseServer as Server;

            string[] branches = arguments.GetValues(CommandStrings.Branch);
            if (branches == null)
            {
                return "Specify a branch using branch=[branch_name]";
            }

            string branchName = branches.Length > 0 ? branches[0] : "";

            if (!server.Branches.ContainsKey(branchName))
            {
                return "Branch " + branchName + " not registered on server";
            }

            StringBuilder historyInfo = new StringBuilder();

            string[] quantities = arguments.GetValues("quantity");
            string quantityString = quantities?.Length > 0 ? quantities[0] : "10";
            int quantity = 0;

            List<string> historyFiles = server.Branches[branchName].OrderedHistoryFiles;

            if (quantityString == CommandStrings.All)
            {
                quantity = historyFiles.Count;
            }
            else if(!int.TryParse(quantityString, out quantity))
            {
                return quantityString + " is not a valid quantity";
            }

            for (int i = 0, n = Math.Min(quantity, historyFiles.Count); i < n; ++i)
            {
                historyInfo.AppendLine("<p>" + GetHistoryFileInfo(server, historyFiles[i]) + "</p>");
            }

            return historyInfo.ToString();
        }

        private string GetHistoryFileInfo(BaseServer baseServer, string filePath)
        {
            HistoryFile historyFile = new HistoryFile(filePath);
            historyFile.Load();

            return historyFile.Status.DisplayString();
        }
    }
}
