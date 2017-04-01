using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace BuildServer
{
    public static class BuildServerSettings
    {
        #region Properties and Fields

        public static string RepositoryURL { get; private set; }

        public static string ServerIP { get; private set; }

        public static int ServerPort { get; private set; }

        public static int ClientPort { get; private set; }

        public static string CompileScriptRelativePath { get; private set; }

        public static string RunTestsScriptRelativePath { get; private set; }

        public static string ServerEmail { get; private set; }

        public static string ServerEmailUsername { get; private set; }

        public static string ServerEmailPassword { get; private set; }

        #endregion

        public static void ReadSettingsFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("No Settings.xml File found in " + filePath);
                Thread.Sleep(2000);
                return;
            }

            Console.WriteLine("Reading settings file");

            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            // Repository URL
            {
                XmlNodeList repoURL = document.GetElementsByTagName("RepositoryURL");
                if (repoURL.Count != 1 || string.IsNullOrEmpty(repoURL[0].InnerText))
                {
                    Console.WriteLine("No Repository URL in Settings File.");
                }
                else
                {
                    RepositoryURL = repoURL[0].InnerText;
                    Console.WriteLine("RepositoryURL = " + RepositoryURL);
                }
            }

            // Server IP
            {
                XmlNodeList serverIP = document.GetElementsByTagName("ServerIP");
                if (serverIP.Count != 1 || string.IsNullOrEmpty(serverIP[0].InnerText))
                {
                    Console.WriteLine("No Server IP in Settings File.");
                }
                else
                {
                    ServerIP = serverIP[0].InnerText;
                    Console.WriteLine("ServerIP = " + ServerIP);
                }
            }

            // Server port
            {
                XmlNodeList serverPort = document.GetElementsByTagName("ServerPort");
                if (serverPort.Count != 1 || string.IsNullOrEmpty(serverPort[0].InnerText))
                {
                    Console.WriteLine("No Server Port in Settings File.");
                }
                else
                {
                    int port = -1;
                    if (!int.TryParse(serverPort[0].InnerText, out port))
                    {
                        Console.WriteLine("Server Port is not a number.");
                    }
                    else
                    {
                        ServerPort = port;
                        Console.WriteLine("ServerPort = " + port);
                    }
                }
            }

            // Client port
            {
                XmlNodeList clientPort = document.GetElementsByTagName("ClientPort");
                if (clientPort.Count != 1 || string.IsNullOrEmpty(clientPort[0].InnerText))
                {
                    ClientPort = ServerPort;
                    Console.WriteLine("ClientPort = " + ClientPort);
                }
                else
                {
                    int port = -1;
                    if (!int.TryParse(clientPort[0].InnerText, out port))
                    {
                        Console.WriteLine("Client Port is not a number.");
                    }
                    else
                    {
                        ClientPort = port;
                        Console.WriteLine("ClientPort = " + port);
                    }
                }
            }

            // Compile script relative path
            {
                XmlNodeList compileScript = document.GetElementsByTagName("CompileScriptRelativePath");
                if (compileScript.Count != 1 || string.IsNullOrEmpty(compileScript[0].InnerText))
                {
                    CompileScriptRelativePath = "compile.bat";
                }
                else
                {
                    CompileScriptRelativePath = compileScript[0].InnerText;
                }

                Console.WriteLine("CompileScriptRelativePath = " + CompileScriptRelativePath);
            }

            // Run tests script relative path
            {
                XmlNodeList runTestsScript = document.GetElementsByTagName("RunTestsScriptRelativePath");
                if (runTestsScript.Count != 1 || string.IsNullOrEmpty(runTestsScript[0].InnerText))
                {
                    RunTestsScriptRelativePath = "run_tests.bat";
                }
                else
                {
                    RunTestsScriptRelativePath = runTestsScript[0].InnerText;
                }

                Console.WriteLine("RunTestsScriptRelativePath = " + RunTestsScriptRelativePath);
            }

            // Server email
            {
                XmlNodeList serverEmail = document.GetElementsByTagName("ServerEmail");
                if (serverEmail.Count != 1 || string.IsNullOrEmpty(serverEmail[0].InnerText))
                {
                    Console.WriteLine("No Server Email in Settings File.");
                }
                else
                {
                    ServerEmail = serverEmail[0].InnerText;
                    Console.WriteLine("ServerEmail = " + ServerEmail);
                }
            }

            // Server email username
            {
                XmlNodeList emailUsername = document.GetElementsByTagName("ServerEmailUsername");
                if (emailUsername.Count != 1 || string.IsNullOrEmpty(emailUsername[0].InnerText))
                {
                    Console.WriteLine("No Server Email Username in Settings File.");
                }
                else
                {
                    ServerEmailUsername = emailUsername[0].InnerText;
                    Console.WriteLine("ServerEmailUsername = " + ServerEmailUsername);
                }
            }

            // Server email password
            {
                XmlNodeList emailPassword = document.GetElementsByTagName("ServerEmailPassword");
                if (emailPassword.Count != 1 || string.IsNullOrEmpty(emailPassword[0].InnerText))
                {
                    Console.WriteLine("No Server Email Password in Settings File.");
                }
                else
                {
                    ServerEmailPassword = emailPassword[0].InnerText;
                    Console.WriteLine("ServerEmailPassword = " + ServerEmailPassword);
                }
            }
        }
    }
}
