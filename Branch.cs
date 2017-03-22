using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BuildServer
{
    public class Branch
    {
        public enum TestState
        {
            kPassed,
            kFailed,
            kUntested
        }

        #region Properties and Fields

        private object branchLock = new object();

        private bool Building { get; set; }

        private bool Queued { get; set; }

        public TestState TestingState { get; private set; } = TestState.kUntested;

        private string BranchName { get; set; }

        private string LogFilePath { get; set; }

        public Dictionary<string, User> Notifiers { get; private set; } = new Dictionary<string, User>();

        private const string ProjectGithubRepoName = "GrowDesktop";

        #endregion

        public Branch(string branchName)
        {
            BranchName = branchName;
            LogFilePath = Path.Combine(Environment.CurrentDirectory, BranchName, "BuildLog.txt");
        }

        public void Build()
        {
            lock (branchLock)
            {
                if (Building)
                {
                    // If we're already building, we don't build again but queue another
                    Queued = true;
                    Console.WriteLine("Build already underway so another will be queued");

                    return;
                }

                // Now set the branch to be building
                Building = true;

                // If we weren't queued, it doesn't matter
                // If we were queued, we are now building so we shouldn't be any more
                Queued = false;
            }

            // Build and test in separate task to not block main build server from testing other projects
            Task.Run(() =>
            {
                // Redirect output into file
                using (FileStream stream = File.Open(LogFilePath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.AutoFlush = true;

                        // This will change directory into the checked out branch
                        Checkout(writer);

                        CmdLineUtils.PerformCommand("\"" + Path.Combine("Dev Tools", "Git Hooks", "Build Server", "compile.bat") + "\"");
                        Console.WriteLine("Build of " + BranchName + " completed");

                        CmdLineUtils.PerformCommand("\"" + Path.Combine("Dev Tools", "Git Hooks", "Build Server", "run_tests.bat") + "\"", outputWriter: writer);
                        Console.WriteLine("Testing of " + BranchName + " completed");
                    }
                }

                // This will change directory out of checked out branch again
                ReadFilesAndSendMessage();

                bool queued = false;
                lock (branchLock)
                {
                    // Now set the branch to be finished building
                    Building = false;
                    queued = Queued;
                }

                if (queued)
                {
                    Build();
                }
            });
        }

        public void Build(string email, string notifySetting)
        {
            lock (branchLock)
            {
                if (!Notifiers.ContainsKey(email))
                {
                    Notifiers.Add(email, new User(email, notifySetting));
                }
                else
                {
                    Notifiers[email].NotifySettingString = notifySetting;
                }
            }

            Build();
        }

        /// <summary>
        /// We check to see if there is a directory already with the branch name.
        /// If not we clone the branch into a directory with it's name
        /// </summary>
        /// <param name="projectGithubRepoName"></param>
        /// <param name="branchName"></param>
        private void Checkout(TextWriter writer)
        {
            string repoDir = Path.Combine(Environment.CurrentDirectory, BranchName);

            if (!Directory.Exists(repoDir))
            {
                Console.WriteLine("Cloning " + ProjectGithubRepoName + " into repoDir");
                // Clone the branch if we do not have it checked out
                CmdLineUtils.PerformCommand(CmdLineUtils.Git, "clone -b " + BranchName + " --single-branch https://github.com/GrowSoftware/" + ProjectGithubRepoName + ".git " + BranchName, writer);
            }

            // Change current directory and update the branch
            Environment.CurrentDirectory = repoDir;
            Console.WriteLine("Making " + BranchName + " up to date");
            CmdLineUtils.PerformCommand(CmdLineUtils.Git, "pull", writer);

            writer.WriteLine("Checkout of " + BranchName + " completed");
            Console.WriteLine("Checkout of " + BranchName + " completed");
        }


        /// <summary>
        /// Reads the log file for the Celeste Engine and either sends back the result or emails the result
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadFilesAndSendMessage()
        {
            bool passed = true;
            StringBuilder messageContents = new StringBuilder();
            string testResults = Path.Combine(Environment.CurrentDirectory, "TestResults");

            if (!Directory.Exists(testResults))
            {
                // Need to improve this to send back a failure message
                passed = false;
                messageContents.AppendLine("No Test Results could be found.  It is likely the code failed to compile so the tests could not be run.");
            }
            else
            {
                messageContents.AppendLine("The following tests failed:\n");

                foreach (string testFile in Directory.EnumerateFiles(testResults, "*.trx"))
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(testFile);

                    foreach (XmlElement resultElement in document.GetElementsByTagName("UnitTestResult"))
                    {
                        string name = resultElement.GetAttribute("testName");
                        string result = resultElement.GetAttribute("outcome");

                        if (result != "Passed")
                        {
                            passed = false;
                            messageContents.AppendLine(name);
                        }
                    }
                }

                Directory.Delete(testResults, true);
            }

            // Set state of branch
            TestingState = passed ? TestState.kPassed : TestState.kFailed;

            // Move back out of the checked out branch
            Console.WriteLine("Moving out of " + BranchName);
            Environment.CurrentDirectory = Directory.GetParent(Environment.CurrentDirectory).FullName;
            Console.WriteLine("Current directory now " + Environment.CurrentDirectory);

            foreach (User user in Notifiers.Values)
            {
                user.Message(LogFilePath, messageContents, BranchName, passed);
            }

            Console.WriteLine("Testing run complete");
        }
    }
}
