using BuildServerUtils;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace BuildServer
{
    public class Server : BaseServer
    {
        #region Properties and Fields

        /// <summary>
        /// A dictionary of all the branches that are available and their current build state.
        /// </summary>
        private Dictionary<string, Branch> Branches { get; set; } = new Dictionary<string, Branch>();

        private Timer Timer { get; set; }

        /// <summary>
        /// A dictionary of all the commands we have registered
        /// </summary>
        private Dictionary<string, IServerCommand> CommandRegistry { get; set; } = new Dictionary<string, IServerCommand>();

        #endregion

        public Server(int port = 1490) : 
            base(port)
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetCustomAttribute<CommandAttribute>() != null))
            {
                CommandRegistry.Add(type.GetCustomAttribute<CommandAttribute>().Token, Activator.CreateInstance(type) as IServerCommand);
            }

            TimeSpan timeUntilMidnight = (DateTime.Today + TimeSpan.FromDays(1)) - DateTime.Now;
            Timer = new Timer(NightlyBuild_DoWork, null, timeUntilMidnight, TimeSpan.FromDays(1));

            LoadBranches();
        }

        private void NightlyBuild_DoWork(object state)
        {
            foreach (Branch branch in Branches.Values)
            {
                branch.Build();
            }
        }

        private void LoadBranches()
        {
            foreach (string directory in Directory.EnumerateDirectories(Directory.GetCurrentDirectory()))
            {
                if (Directory.Exists(Path.Combine(directory, ".git")))
                {
                    using (Repository repo = new Repository(directory))
                    {
                        Console.WriteLine("Discovered branch " + repo.Head.FriendlyName);
                        Branches.Add(repo.Head.FriendlyName, new Branch(repo.Head.FriendlyName));
                    }
                }
            }
        }
        
        protected override void ProcessMessage(byte[] data)
        {
            base.ProcessMessage(data);

            string dataString = data.ConvertToString();
            Console.WriteLine("Received command: " + dataString);

            if (ClientComms.IsConnected)
            {
                ClientComms.Send("Received command: " + dataString);
            }
        }

        /// <summary>
        /// Clones the repository, runs the tests and then when the process has finished, reads the log file and emails the results
        /// </summary>
        /// <param name="projectDirectoryPath"></param>
        /// <param name="projectExeName"></param>
        private void TestProject(string projectGithubRepoName, string branchName, string email, string notifySetting)
        {
            if (!Branches.ContainsKey(branchName))
            {
                Branches.Add(branchName, new Branch(branchName));
            }

            Branches[branchName].Build(email, notifySetting);
        }
    }
}
