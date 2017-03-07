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
            string settingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Dev Tools", "Git Hooks", "Build Server", "Settings.xml");
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
            }
        }
    }
}