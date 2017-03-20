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

        public static int ServerPort { get; private set; }

        public static string ServerEmail { get; private set; }

        public static string ServerEmailUsername { get; private set; }

        public static string ServerEmailPassword { get; private set; }

        #endregion

        public static void ReadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("No Settings.xml File found in " + filePath);
                Thread.Sleep(2);
                return;
            }

            Console.WriteLine("Reading settings file");

            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            // Server port
            {
                XmlNodeList serverPort = document.GetElementsByTagName("ServerPort");
                if (serverPort.Count != 1 || string.IsNullOrEmpty(serverPort[0].InnerText))
                {
                    Console.WriteLine("No Server Port in Settings File.");
                    Thread.Sleep(2);
                    return;
                }

                int port = -1;
                if (!int.TryParse(serverPort[0].InnerText, out port))
                {
                    Console.WriteLine("Server Port is not a number.");
                    Thread.Sleep(2);
                    return;
                }

                ServerPort = port;
                Console.WriteLine("ServerPort = " + port);
            }

            // Server email
            {
                XmlNodeList serverEmail = document.GetElementsByTagName("ServerEmail");
                if (serverEmail.Count != 1 || string.IsNullOrEmpty(serverEmail[0].InnerText))
                {
                    Console.WriteLine("No Server Email in Settings File.");
                    Thread.Sleep(2);
                    return;
                }

                ServerEmail = serverEmail[0].InnerText;
                Console.WriteLine("ServerEmail = " + ServerEmail);
            }

            // Server email username
            {
                XmlNodeList emailUsername = document.GetElementsByTagName("ServerEmailUsername");
                if (emailUsername.Count != 1 || string.IsNullOrEmpty(emailUsername[0].InnerText))
                {
                    Console.WriteLine("No Server Email Username in Settings File.");
                    Thread.Sleep(2);
                    return;
                }

                ServerEmailUsername = emailUsername[0].InnerText;
                Console.WriteLine("ServerEmailUsername = " + ServerEmailUsername);
            }

            // Server email password
            {
                XmlNodeList emailPassword = document.GetElementsByTagName("ServerEmailPassword");
                if (emailPassword.Count != 1 || string.IsNullOrEmpty(emailPassword[0].InnerText))
                {
                    Console.WriteLine("No Server Email Password in Settings File.");
                    Thread.Sleep(2);
                    return;
                }

                ServerEmailPassword = emailPassword[0].InnerText;
                Console.WriteLine("ServerEmailPassword = " + ServerEmailPassword);
            }
        }
    }
}
