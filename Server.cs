using BuildServerUtils;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace BuildServer
{
    public class Server : BaseServer
    {
        enum BuildState
        {
            kBuilding,
            kNotBuilding
        }

        #region Properties and Fields

        /// <summary>
        /// A dictionary of all the branches that are available and their current build state.
        /// </summary>
        private object branchesLock = new object();
        private Dictionary<string, BuildState> Branches { get; set; } = new Dictionary<string, BuildState>();

        #endregion

        public Server(int port = 1490) : 
            base(port)
        {
        }

        protected override void ProcessMessage(byte[] data)
        {
            Task.Factory.StartNew(() =>
            {
                base.ProcessMessage(data);

                string[] strings = data.ConvertToString().Split(',');

                if (strings.Length == 4 && strings[0] == "Request Build")
                {
                    Console.WriteLine("Request Received for " + strings[1]);

                    TestProject("GrowDesktop", strings[1], strings[2], strings[3]);
                }
            });
        }

        /// <summary>
        /// Clones the repository, runs the tests and then when the process has finished, reads the log file and emails the results
        /// </summary>
        /// <param name="projectDirectoryPath"></param>
        /// <param name="projectExeName"></param>
        private void TestProject(string projectGithubRepoName, string branchName, string email, string notifySetting)
        {
            lock (branchesLock)
            {
                if (!Branches.ContainsKey(branchName))
                {
                    Branches.Add(branchName, BuildState.kNotBuilding);
                }

                if (Branches[branchName] == BuildState.kBuilding)
                {
                    // If we're already building, don't worry about testing again
                    // Maybe in the future, add the email to a list we notify?
                    return;
                }

                // Now set the branch to be building
                Branches[branchName] = BuildState.kBuilding;
            }

            // Redirect output into file
            string logFilePath = Path.Combine(Environment.CurrentDirectory, branchName, "BuildLog.txt");
            using (FileStream stream = File.Open(logFilePath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.AutoFlush = true;

                    // This will change directory into the checked out branch
                    CheckoutBranch(projectGithubRepoName, branchName, writer);

                    CmdLineUtils.PerformCommand("\"" + Path.Combine("Dev Tools", "Git Hooks", "Build Server", "compile.bat") + "\"");
                    Console.WriteLine("Build of " + branchName + " completed");

                    CmdLineUtils.PerformCommand("\"" + Path.Combine("Dev Tools", "Git Hooks", "Build Server", "run_tests.bat") + "\"", outputWriter:writer);
                    Console.WriteLine("Testing of " + branchName + " completed");
                }
            }

            // This will change directory out of checked out branch again
            ReadFilesAndSendMessage(logFilePath, branchName, email, notifySetting);

            lock (branchesLock)
            {
                // Now set the branch to be not building
                Branches[branchName] = BuildState.kNotBuilding;
            }
        }

        /// <summary>
        /// We check to see if there is a directory already with the branch name.
        /// If not we clone the branch into a directory with it's name
        /// </summary>
        /// <param name="projectGithubRepoName"></param>
        /// <param name="branchName"></param>
        private void CheckoutBranch(string projectGithubRepoName, string branchName, TextWriter writer)
        {
            string repoDir = Path.Combine(Environment.CurrentDirectory, branchName);

            if (!Directory.Exists(repoDir))
            {
                Console.WriteLine("Cloning " + projectGithubRepoName + " into repoDir");
                // Clone the branch if we do not have it checked out
                CmdLineUtils.PerformCommand(CmdLineUtils.Git, "clone -b " + branchName + " --single-branch https://github.com/GrowSoftware/" + projectGithubRepoName + ".git " + branchName, writer);
            }

            // Change current directory and update the branch
            Environment.CurrentDirectory = repoDir;
            Console.WriteLine("Making " + branchName + " up to date");
            CmdLineUtils.PerformCommand(CmdLineUtils.Git, "pull", writer);

            writer.WriteLine("Checkout of " + branchName + " completed");
            Console.WriteLine("Checkout of " + branchName + " completed");
        }

        /// <summary>
        /// Reads the log file for the Celeste Engine and either sends back the result or emails the result
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadFilesAndSendMessage(string logFilePath, string branchName, string email, string notifySetting)
        {
            string testResults = Path.Combine(Environment.CurrentDirectory, "TestResults");
            if (!Directory.Exists(testResults))
            {
                // Need to improve this to send back a failure message
                return;
            }

            bool passed = true;
            StringBuilder messageContents = new StringBuilder();
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

            // Move back out of the checked out branch
            Console.WriteLine("Moving out of " + branchName);
            Environment.CurrentDirectory = Directory.GetParent(Environment.CurrentDirectory).FullName;
            Console.WriteLine("Current directory now " + Environment.CurrentDirectory);

            // "Y" means only email on fail
            if (!passed || notifySetting != "Y")
            {
                Message(logFilePath, messageContents, branchName, email, passed);
            }
            else
            {
                Console.WriteLine("Skipping message because the build passed and the user indicated not to email them.");
            }

            Console.WriteLine("Testing run complete");
        }

        /// <summary>
        /// Either sends back via comms or emails the inputted string in the string builder to me if there is no connection
        /// </summary>
        /// <param name="testRunInformation"></param>
        private void Message(string logFilePath, StringBuilder testRunInformation, string branchName, string email, bool passed)
        {
            Console.WriteLine("Reading settings file");

            string settingsFilePath = Path.Combine(Environment.CurrentDirectory, "Settings.xml");
            if (!File.Exists(settingsFilePath))
            {
                Console.WriteLine("No Settings.xml File found in " + settingsFilePath);
                Thread.Sleep(2);
                return;
            }

            XmlDocument document = new XmlDocument();
            document.Load(settingsFilePath);

            XmlNodeList serverEmail = document.GetElementsByTagName("ServerEmail");
            if (serverEmail.Count != 1 || string.IsNullOrEmpty(serverEmail[0].InnerText))
            {
                Console.WriteLine("No Server Email in Settings File.");
                Thread.Sleep(2);
                return;
            }

            Console.WriteLine("ServerEmail = " + serverEmail[0].InnerText);

            XmlNodeList emailUsername = document.GetElementsByTagName("ServerEmailUsername");
            if (emailUsername.Count != 1 || string.IsNullOrEmpty(emailUsername[0].InnerText))
            {
                Console.WriteLine("No Server Email Username in Settings File.");
                Thread.Sleep(2);
                return;
            }

            Console.WriteLine("ServerEmailUsername = " + emailUsername[0].InnerText);

            XmlNodeList emailPassword = document.GetElementsByTagName("ServerEmailPassword");
            if (emailPassword.Count != 1 || string.IsNullOrEmpty(emailPassword[0].InnerText))
            {
                Console.WriteLine("No Server Email Password in Settings File.");
                Thread.Sleep(2);
                return;
            }

            Console.WriteLine("ServerEmailPassword = " + emailPassword[0].InnerText);
            Console.WriteLine("Creating message");

            DateTime buildCompleteTime = DateTime.Now;

            testRunInformation.AppendLine();
            testRunInformation.Append("Build Request completed at " + buildCompleteTime.ToShortTimeString());

            try
            {
                MailMessage mail = new MailMessage(serverEmail[0].InnerText, email, (branchName + " - ") + (passed ? "Build Passed" : "Build Failed"), testRunInformation.ToString());

                Console.WriteLine("Attaching log from file " + logFilePath);
                mail.Attachments.Add(new Attachment(logFilePath));

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(emailUsername[0].InnerText, emailPassword[0].InnerText);
                client.EnableSsl = true;

                client.Send(mail);
                Console.WriteLine("Message sent to " + email);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occurred in sending email with message: " + e.Message);
            }
        }
    }
}
