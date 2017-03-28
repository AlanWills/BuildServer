using BuildServerUtils;
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
        public enum BuildState
        {
            Building,
            Idle,
            Paused
        }

        public enum TestState
        {
            Passed,
            Failed,
            Untested
        }

        #region Properties and Fields

        private object branchLock = new object();

        private object buildingLock = new object();
        private BuildState buildingState = BuildState.Idle;
        public BuildState BuildingState
        {
            get
            {
                lock (buildingLock)
                {
                    return buildingState;
                }
            }
            set
            {
                lock (buildingLock)
                {
                    buildingState = value;
                }
            }
        }

        private object queueLock = new object();
        private bool queued = false;
        public bool Queued
        {
            get
            {
                lock (queueLock)
                {
                    return queued;
                }
            }
            set
            {
                lock (queueLock)
                {
                    queued = value;
                }
            }
        }

        public List<string> OrderedHistoryFiles
        {
            get
            {
                // Use current directory here as we will be modifying the Environment directory when building
                string directory = Path.Combine(Directory.GetCurrentDirectory(), BranchName, "Build Server");
                return (Directory.Exists(directory) ? Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), BranchName, "Build Server"), "*History.xml", SearchOption.AllDirectories).
                                        OrderByDescending(x => new FileInfo(x).LastWriteTime).ToList() : new List<string>());
            }
        }

        public TestState TestingState
        {
            get
            {
                string mostRecentHistoryFile = OrderedHistoryFiles.FirstOrDefault();

                if (string.IsNullOrEmpty(mostRecentHistoryFile))
                {
                    return TestState.Untested;
                }

                HistoryFile file = new HistoryFile(mostRecentHistoryFile);
                file.Load();

                return file.Status;
            }
        }

        private readonly string BranchName;

        public Dictionary<string, User> Notifiers { get; private set; } = new Dictionary<string, User>();

        private const string ProjectGithubRepoName = "GrowDesktop";

        #endregion

        public Branch(string branchName)
        {
            BranchName = branchName;
        }

        public Task Build()
        {
            if (BuildingState == BuildState.Paused)
            {
                // This will still preseve the queued builds so that when we resume we will build automatically
                Console.WriteLine("Build paused so request ignored");
                return Task.Run(() => { });
            }
            else if (BuildingState == BuildState.Building)
            {
                // If we're already building, we don't build again but queue another
                Queued = true;
                Console.WriteLine("Build already underway so another will be queued");

                return Task.Run(() => { });
            }

            // Now set the branch to be building
            BuildingState = BuildState.Building;

            // If we weren't queued, it doesn't matter
            // If we were queued, we are now building so we shouldn't be any more
            Queued = false;

            // Build and test in separate task to not block main build server from testing other projects
            return Task.Run(() =>
            {
                Checkout();

                string repoDir = Path.Combine(Directory.GetCurrentDirectory(), BranchName);

                // Create directory for this build
                DirectoryInfo thisBuildDirectory = Directory.CreateDirectory(Path.Combine(repoDir, "Build Server", DateTime.Now.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture)));

                // Redirect building output into file
                string buildLogFilePath = Path.Combine(thisBuildDirectory.FullName, "BuildLog.txt");
                using (FileStream stream = File.Open(buildLogFilePath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        Console.WriteLine("Starting build of " + BranchName);
                        CmdLineUtils.PerformCommand(Path.Combine(repoDir, "Dev Tools", "Git Hooks", "Build Server", "compile.bat"), repoDir, outputWriter: writer);
                        Console.WriteLine("Build of " + BranchName + " completed");
                    }
                }

                // Redirect testing output into file
                string testLogFilePath = Path.Combine(thisBuildDirectory.FullName, "TestLog.txt");
                using (FileStream stream = File.Open(testLogFilePath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        Console.WriteLine("Starting test run of " + BranchName);

                        // Working directory for testing must absolutely be inside the repo because the test results files are created in the working directory
                        CmdLineUtils.PerformCommand(Path.Combine(repoDir, "Dev Tools", "Git Hooks", "Build Server", "run_tests.bat"), repoDir, outputWriter: writer);
                        Console.WriteLine("Test run of " + BranchName + " completed");
                    }
                }

                ReadFilesAndSendMessage(testLogFilePath);

                if (BuildingState == BuildState.Paused)
                {
                    // If we have paused we should just quit here now
                    return;
                }

                // Now set the branch to be finished building
                BuildingState = BuildState.Idle;

                if (Queued)
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
        /// Sets the build state to paused.
        /// </summary>
        public void Pause()
        {
            BuildingState = BuildState.Paused;
        }

        /// <summary>
        /// Sets the build state to idle and if we had a build queued, we trigger a build.
        /// </summary>
        public void Resume()
        {
            BuildingState = BuildState.Idle;
            if (Queued)
            {
                Build();
            }
        }

        /// <summary>
        /// We check to see if there is a directory already with the branch name.
        /// If not we clone the branch into a directory with it's name
        /// </summary>
        /// <param name="projectGithubRepoName"></param>
        /// <param name="branchName"></param>
        private void Checkout()
        {
            string repoDir = Path.Combine(Directory.GetCurrentDirectory(), BranchName);

            if (!Directory.Exists(repoDir))
            {
                Console.WriteLine("Cloning " + ProjectGithubRepoName + " into " + repoDir);
                // Clone the branch if we do not have it checked out
                CmdLineUtils.PerformCommand(CmdLineUtils.Git, Directory.GetCurrentDirectory(), "clone -b " + BranchName + " --single-branch https://github.com/GrowSoftware/" + ProjectGithubRepoName + ".git " + BranchName);
            }

            Console.WriteLine("Making " + BranchName + " up to date");
            CmdLineUtils.PerformCommand(CmdLineUtils.Git, repoDir, "pull");

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
            string testResults = Path.Combine(Directory.GetCurrentDirectory(), BranchName, "TestResults");
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

            foreach (User user in Notifiers.Values)
            {
                user.Message(messageContents, BranchName, passed);
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
