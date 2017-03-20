using LibGit2Sharp;
using System;
using System.IO;
using System.Threading;
using System.Xml;

namespace BuildServerClient
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            string settingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Settings.xml");
            if (!File.Exists(settingsFilePath))
            {
                Console.WriteLine("No Settings File.");
                Thread.Sleep(2);
                return;
            }

            XmlDocument document = new XmlDocument();
            document.Load(settingsFilePath);

            XmlNodeList serverIP = document.GetElementsByTagName("ServerIP");
            if (serverIP.Count != 1 || string.IsNullOrEmpty(serverIP[0].InnerText))
            {
                Console.WriteLine("No Server IP in Settings File.");
                Thread.Sleep(2);
                return;
            }

            Console.WriteLine("ServerIP = " + serverIP[0].InnerText);

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

            Console.WriteLine("ServerPort = " + port);

            Client client = new Client(serverIP[0].InnerText, port);

            string branchName = "";
            using (Repository repo = new Repository(Directory.GetCurrentDirectory()))
            {
                branchName = repo.Head.FriendlyName;
            }

            XmlNodeList emails = document.GetElementsByTagName("Email");
            if (emails.Count == 1 && !string.IsNullOrEmpty(emails[0].InnerText))
            {
                XmlNodeList notifySettings = document.GetElementsByTagName("OnlyEmailOnFail");
                string notifySetting = notifySettings.Count == 1 && !string.IsNullOrEmpty(notifySettings[0].InnerText) ? notifySettings[0].InnerText : "Y";

                client.ServerComms.Send("Request Build," + branchName + "," + emails[0].InnerText + "," + notifySetting);
                Console.WriteLine("Build Request sent");
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Exception occurred with message: " + (e.ExceptionObject as Exception).Message);
        }
    }
}