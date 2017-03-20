using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace BuildServerClient
{
    public static class ClientSettings
    {
        #region Properties and Fields

        public static string ServerIP { get; private set; }

        public static int ServerPort { get; private set; }

        public static string Email { get; private set; }

        public static string NotifySetting { get; private set; }

        #endregion

        public static void ReadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("No Settings File.");
                Thread.Sleep(2);
                return;
            }

            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            // Server IP
            {
                XmlNodeList serverIP = document.GetElementsByTagName("ServerIP");
                if (serverIP.Count != 1 || string.IsNullOrEmpty(serverIP[0].InnerText))
                {
                    Console.WriteLine("No Server IP in Settings File.");
                    Thread.Sleep(2);
                    return;
                }

                ServerIP = serverIP[0].InnerText;
                Console.WriteLine("ServerIP = " + ServerIP);
            }

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
                Console.WriteLine("ServerPort = " + ServerPort);
            }

            // Email
            {
                XmlNodeList emails = document.GetElementsByTagName("Email");
                if (emails.Count != 1 && string.IsNullOrEmpty(emails[0].InnerText))
                {
                    Console.WriteLine("No Email in Settings file.");
                    Thread.Sleep(2);
                    return;
                }

                Email = emails[0].InnerText;
                Console.WriteLine("Email = " + Email);
            }

            // Notify setting
            {
                XmlNodeList notifySettings = document.GetElementsByTagName("OnlyEmailOnFail");
                if (notifySettings.Count != 1 && string.IsNullOrEmpty(notifySettings[0].InnerText))
                {
                    Console.WriteLine("No OnlyEmailOnFail in Settings file.");
                    Thread.Sleep(2);
                    return;
                }

                NotifySetting = notifySettings[0].InnerText;
                Console.WriteLine("NotifySetting = " + NotifySetting);
            }
        }
    }
}
