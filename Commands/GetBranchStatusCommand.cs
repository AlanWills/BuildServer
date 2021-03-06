﻿using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static BuildServer.Branch;

namespace BuildServer
{
    [Command(CommandStrings.GetBranchStatus)]
    public class GetBranchStatusCommand : IServerCommand
    {
        public string Execute(BaseServer baseServer, NameValueCollection arguments)
        {
            Server server = baseServer as Server;

            List<string> branches = arguments.GetValues(ParameterStrings.Branch)?.ToList();

            if (branches.Contains(ParameterStrings.All))
            {
                // If the user passes all, we use all the branch statuses
                branches.Clear();
                branches.AddRange(server.Branches.Keys);
            }

            HTMLWriter html = new HTMLWriter();
            html.CreateH2("Branches on Build Server");

            foreach (string branchName in branches)
            {
                if (server.Branches.ContainsKey(branchName))
                {
                    GetBranchTestStateString(html, server, branchName);
                }
            }

            return html.ToString();
        }
        
        private string GetBranchTestStateString(HTMLWriter builder, Server server, string branchName)
        {
            Branch branch = server.Branches[branchName];

            builder.CreateH3("")
                   .CreateLink(server.BaseAddress + CommandStrings.ViewBuildHistory + "?branch=" + branchName, branchName)
                   .AddStyling(new Tuple<string, string>("font-weight", "bold"));

            builder.CreatePreservedParagraph("Latest Build:   ")
                   .CreateLink(server.BaseAddress + CommandStrings.GetFailedTests + "?branch=" + branchName + "&dir=" + ParameterStrings.Latest, branch.TestingState.DisplayString())
                   .AddStyling(new Tuple<string, string>("color", branch.TestingState.Colour()));

            builder.CreatePreservedParagraph("Status:         " + (branch.BuildingState.ToString()));
            builder.CreatePreservedParagraph("Build Queued:   " + (branch.Queued ? "Yes" : "No"));

            if (branch.BuildingState == BuildState.Paused)
            {
                builder.CreateButton("Resume", server.BaseAddress + CommandStrings.ResumeBranch + "?" + ParameterStrings.Branch + "=" + branchName);
            }
            else
            {
                // Should always be able to pause if not paused already
                builder.CreateButton("Pause", server.BaseAddress + CommandStrings.PauseBranch + "?" + ParameterStrings.Branch + "=" + branchName);
            }

            builder.CreateLineBreak();

            return builder.ToString();
        }
    }
}