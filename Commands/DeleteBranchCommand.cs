using BuildServerUtils;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace BuildServer
{
    [Command(CommandStrings.DeleteBranch)]
    public class DeleteBranchCommand : IServerCommand
    {
        public string Execute(BaseServer baseServer, NameValueCollection arguments)
        {
            Server server = baseServer as Server;
            
            string branchName = arguments.GetValues(CommandStrings.Branch)?[0];
            
            if (!server.Branches.ContainsKey(branchName))
            {
                return branchName + " does not exist on build server";
            }

            Branch branch = server.Branches[branchName];
            if (branch.BuildingState == Branch.BuildState.Building)
            {
                return "Cannot delete a branch that is currently building";
            }

            server.Branches.Remove(branchName);

            // A branch that is never built will not have a directory created
            string directory = Path.Combine(Directory.GetCurrentDirectory(), branchName);
            if (Directory.Exists(directory))
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Couldn't delete " + branchName + " directory because " + e.Message);
                }

                try
                {
                    // Handles git files
                    DeleteDirectory(directory);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Couldn't delete " + branchName + " directory because " + e.Message);
                }
            }

            return "Branch removed from build server";
        }

        private void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
    }
}