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
        /// A dictionary of all the commands we have registered
        /// </summary>
        private Dictionary<string, IServerCommand> CommandRegistry { get; set; } = new Dictionary<string, IServerCommand>();

        /// <summary>
        /// A dictionary of all the branches that are available and their current build state.
        /// </summary>
        public Dictionary<string, Branch> Branches { get; private set; } = new Dictionary<string, Branch>();

        private Timer Timer { get; set; }

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

            foreach (KeyValuePair<string, IServerCommand> command in CommandRegistry)
            {
                if (dataString.StartsWith(command.Key))
                {
                    if (ClientComms.IsConnected)
                    {
                        ClientComms.Send("Received command: " + dataString);
                    }

                    // Remove command string
                    command.Value.Execute(this, dataString.Substring(command.Key.Length));
                }
            }
        }
    }
}
