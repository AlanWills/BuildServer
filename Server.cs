using BuildServerUtils;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
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

        public Server(string ip, int serverPort, int clientPort) : 
            base(ip, clientPort)
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetCustomAttribute<CommandAttribute>() != null))
            {
                Listener.Prefixes.Add("http://*:" + serverPort.ToString() + type.GetCustomAttribute<CommandAttribute>().Token + "/");
                CommandRegistry.Add(type.GetCustomAttribute<CommandAttribute>().Token, Activator.CreateInstance(type) as IServerCommand);
            }

            TimeSpan timeUntilMidnight = (DateTime.Today + TimeSpan.FromDays(1)) - DateTime.Now;
            Timer = new Timer(NightlyBuild_DoWork, null, timeUntilMidnight, TimeSpan.FromDays(1));

            LoadBranches();
        }
        
        private void NightlyBuild_DoWork(object state)
        {
            // Build each branch synchronously as they all interfere with certain universal export folders
            foreach (Branch branch in Branches.Values)
            {
                branch.Build().Wait();
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
        
        protected override void ProcessMessage(HttpListenerContext requestContext)
        {
            base.ProcessMessage(requestContext);

            StringBuilder builder = new StringBuilder("<html><body>").AppendLine();

            string url = requestContext.Request.RawUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                // Ignore cruft (although it should never actually get here
                return;
            }

            Console.WriteLine("Command received: " + url);

            foreach (KeyValuePair<string, IServerCommand> command in CommandRegistry)
            {
                // Match url to a command
                if (url.StartsWith(command.Key))
                {
                    string commandResponse = command.Value.Execute(this, requestContext.Request.QueryString);
                    builder.Append(commandResponse);

                    break;
                }
            }

            builder.AppendLine("</body></html>");

            byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());

            requestContext.Response.ContentLength64 = bytes.Length;

            Stream output = requestContext.Response.OutputStream;
            output.Write(bytes, 0, bytes.Length);
            output.Close();
        }
    }
}