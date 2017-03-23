﻿using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public List<string> OrderedHistoryFiles
        {
            get
            {
                // Use current directory here as we will be modifying the Environment directory when building
                return Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), BranchName, "Build Server"), "*History.xml", SearchOption.AllDirectories).
                                        OrderByDescending(x => new FileInfo(x).LastWriteTime).ToList();
            }
        }

        public TestState TestingState
        {
            get
            {
                string mostRecentHistoryFile = OrderedHistoryFiles.FirstOrDefault();

                if (string.IsNullOrEmpty(mostRecentHistoryFile))
                {
                    return TestState.kUntested;
                }

                HistoryFile file = new HistoryFile(mostRecentHistoryFile);
                file.Load();

                return file.Status;
            }
        }

        private string BranchName { get; set; }

        public Dictionary<string, User> Notifiers { get; private set; } = new Dictionary<string, User>();

        private const string ProjectGithubRepoName = "GrowDesktop";

        #endregion

        public Branch(string branchName)
        {
            BranchName = branchName;
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
                // This will change directory into the checked out branch
                Checkout();

                // Create directory for this build
                DirectoryInfo thisBuildDirectory = Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Build Server", DateTime.Now.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture)));

                // Redirect output into file
                string logFilePath = Path.Combine(thisBuildDirectory.FullName, "BuildLog.txt");
                using (FileStream stream = File.Open(logFilePath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.AutoFlush = true;

                        CmdLineUtils.PerformCommand("\"" + Path.Combine("Dev Tools", "Git Hooks", "Build Server", "compile.bat") + "\"");
                        Console.WriteLine("Build of " + BranchName + " completed");

                        CmdLineUtils.PerformCommand("\"" + Path.Combine("Dev Tools", "Git Hooks", "Build Server", "run_tests.bat") + "\"", outputWriter: writer);
                        Console.WriteLine("Testing of " + BranchName + " completed");
                    }
                }

                // This will change directory out of checked out branch again
                ReadFilesAndSendMessage(logFilePath);

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
        private void Checkout()
        {
            string repoDir = Path.Combine(Environment.CurrentDirectory, BranchName);

            if (!Directory.Exists(Path.Combine(repoDir, ".git")))
            {
                Console.WriteLine("Cloning " + ProjectGithubRepoName + " into " + repoDir);
                // Clone the branch if we do not have it checked out
                CmdLineUtils.PerformCommand(CmdLineUtils.Git, "clone -b " + BranchName + " --single-branch https://github.com/GrowSoftware/" + ProjectGithubRepoName + ".git " + BranchName);
            }

            // Change current directory and update the branch
            Environment.CurrentDirectory = repoDir;
            Console.WriteLine("Making " + BranchName + " up to date");
            CmdLineUtils.PerformCommand(CmdLineUtils.Git, "pull");

            Console.WriteLine("Checkout of " + BranchName + " completed");
        }


        /// <summary>
        /// Reads the log file for the Celeste Engine and either sends back the result or emails the result
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadFilesAndSendMessage(string logFilePath)
        {
            bool passed = true;
            StringBuilder messageContents = new StringBuilder();
            string testResults = Path.Combine(Environment.CurrentDirectory, "TestResults");
            List<string> failedTestNames = new List<string>();

            if (!Directory.Exists(testResults))
            {
                // Need to improve this to send back a failure message
                passed = false;
                messageContents.AppendLine("No Test Results could be found.  It is likely the code failed to compile so the tests could not be run.");
            }
            else
            {
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
                            if (passed)
                            {
                                // If this is the first to fail, we write this line before changing the flag
                                messageContents.AppendLine("The following tests failed:\n");
                            }

                            passed = false;
                            messageContents.AppendLine(name);
                            failedTestNames.Add(name);
                        }
                    }
                }

                Directory.Delete(testResults, true);
            }

            // Write the history file
            WriteHistoryFile(Directory.GetParent(logFilePath).FullName, passed, failedTestNames);

            // Move back out of the checked out branch
            Console.WriteLine("Moving out of " + BranchName);
            Environment.CurrentDirectory = Directory.GetParent(Environment.CurrentDirectory).FullName;
            Console.WriteLine("Current directory now " + Environment.CurrentDirectory);

            foreach (User user in Notifiers.Values)
            {
                user.Message(logFilePath, messageContents, BranchName, passed);
            }

            Console.WriteLine("Testing run complete");
        }

        /// <summary>
        /// Writes an xml file containing extra information about this particular build
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="passed"></param>
        private void WriteHistoryFile(string directory, bool passed, List<string> failedTestNames)
        {
            HistoryFile file = new HistoryFile(Path.Combine(directory, "History.xml"));
            file.Save(passed, failedTestNames);
        }
    }
}
