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

            // This will change directory into the checked out branch
            CheckoutBranch(projectGithubRepoName, branchName);
            
            CmdLineUtils.PerformCommand("\"" + Path.Combine("Dev Tools", "Git Hooks", "Build Server", "compile.bat") + "\"", "");
            CmdLineUtils.PerformCommand("\"" + Path.Combine("Dev Tools", "Git Hooks", "Build Server", "run_tests.bat") + "\"", "");

            ReadFilesAndSendMessage(email, notifySetting);

            lock (branchesLock)
            {
                // Now set the branch to be not building
                Branches[branchName] = BuildState.kNotBuilding;
            }

            // Move back out of the checked out branch
            Environment.CurrentDirectory = Directory.GetParent(Environment.CurrentDirectory).FullName;
        }

        /// <summary>
        /// We check to see if there is a directory already with the branch name.
        /// If not we clone the branch into a directory with it's name
        /// </summary>
        /// <param name="projectGithubRepoName"></param>
        /// <param name="branchName"></param>
        private void CheckoutBranch(string projectGithubRepoName, string branchName)
        {
            string repoDir = Path.Combine(Environment.CurrentDirectory, branchName);

            if (!Directory.Exists(repoDir))
            {
                // Clone the branch if we do not have it checked out
                CmdLineUtils.PerformCommand(CmdLineUtils.Git, "clone -b " + branchName + " --single-branch https://github.com/GrowSoftware/" + projectGithubRepoName + ".git " + branchName);
            }

            // Change current directory and update the branch
            Environment.CurrentDirectory = repoDir;
            CmdLineUtils.PerformCommand(CmdLineUtils.Git, "pull");

            Console.WriteLine("Checkout of " + branchName + " completed");
        }

        /// <summary>
        /// Reads the log file for the Celeste Engine and either sends back the result or emails the result
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadFilesAndSendMessage(string email, string notifySetting)
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

            // "Y" means only email on fail
            if (!passed || notifySetting != "Y")
            {
                Message(messageContents, email, passed);
            }

            Console.WriteLine("Testing run complete");
        }

        /// <summary>
        /// Either sends back via comms or emails the inputted string in the string builder to me if there is no connection
        /// </summary>
        /// <param name="testRunInformation"></param>
        private void Message(StringBuilder testRunInformation, string email, bool passed)
        {
            DateTime buildCompleteTime = DateTime.Now;

            testRunInformation.AppendLine();
            testRunInformation.Append("Build Request completed at " + buildCompleteTime.ToShortTimeString());

            MailMessage mail = new MailMessage("alawills@googlemail.com", email, passed ? "Build Passed" : "Build Failed", testRunInformation.ToString());
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            
            client.Port = 587;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("alawills", "favouriteprimes111929");
            client.EnableSsl = true;

            client.Send(mail);
        }
    }
}
