using BuildServerUtils;
using System;
using System.Collections.Generic;
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

        #endregion

        public Server(int port = 1490) : 
            base(port)
        {
            TimeSpan timeUntilMidnight = (DateTime.Today + TimeSpan.FromDays(1)) - DateTime.Now;
            Timer = new Timer(NightlyBuild_DoWork, null, timeUntilMidnight, TimeSpan.FromDays(1));
        }

        private void NightlyBuild_DoWork(object state)
        {
            foreach (Branch branch in Branches.Values)
            {
                branch.Build();
            }
        }
        
        protected override void ProcessMessage(byte[] data)
        {
            base.ProcessMessage(data);

            string[] strings = data.ConvertToString().Split(',');

            if (strings.Length == 4 && strings[0] == "Request Build")
            {
                Console.WriteLine("Request Received for " + strings[1]);

                TestProject("GrowDesktop", strings[1], strings[2], strings[3]);
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
