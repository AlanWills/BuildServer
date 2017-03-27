using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
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

            StringBuilder historyInfo = new StringBuilder("<h2>" + branchName + " Build History</h2>");

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
                historyInfo.AppendLine(quantityString + " is not a valid quantity");
            }

            for (int i = 0, n = Math.Min(quantity, historyFiles.Count); i < n; ++i)
            {
                GetHistoryFileInfo(historyInfo, server, branchName, historyFiles[i]);
            }

            return historyInfo.ToString();
        }

        private void GetHistoryFileInfo(StringBuilder builder, Server server, string branchName, string filePath)
        {
            string parentDirName = Directory.GetParent(filePath).Name;

            HistoryFile historyFile = new HistoryFile(filePath);
            historyFile.Load();

            builder.Append("<pre><a href=\"");
            builder.Append(server.BaseAddress + CommandStrings.GetFailedTests + "?branch=" + branchName + "&dir=" + parentDirName);
            builder.Append("\">");
            builder.Append(parentDirName);
            builder.Append("</a>:  ");
            builder.Append(historyFile.Status.DisplayString());
            builder.AppendLine("</pre>");;
        }
    }
}
