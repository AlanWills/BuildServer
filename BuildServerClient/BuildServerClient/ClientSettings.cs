using LibGit2Sharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace BuildServerClient
{
    public static class ClientSettings
    {
        #region Properties and Fields

        public const string ServerIPName = "ServerIP";
        public static string ServerIP { get; set; }

        public const string ServerPortName = "ServerPort";
        public static int ServerPort { get; set; }

        public const string EmailName = "Email";
        public static string Email { get; set; }

        public const string NotifySettingName = "OnlyEmailOnFail";
        public static string NotifySetting { get; set; }

        public static string CurrentBranch
        {
            get
            {
                try
                {
                    using (Repository repo = new Repository(Directory.GetCurrentDirectory()))
                    {
                        return repo.Head.FriendlyName;
                    }
                }
                catch { return "Not_A_Repo"; }
            }
        }

        public const string SettingsFileName = "Settings.xml";

        #endregion
        
        public static void ReadFile()
        {
            string filePath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, SettingsFileName);
            if (!File.Exists(filePath))
            {
                Console.WriteLine("No Settings File found.  Auto creating one now which can be configured using the 'settings' command.");
                File.Create(filePath);

                Thread.Sleep(2000);
                return;
            }

            Console.WriteLine("Reading Settings file");

            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            // Server IP
            {
                XmlNodeList serverIP = document.GetElementsByTagName(ServerIPName);
                if (serverIP.Count != 1 || string.IsNullOrEmpty(serverIP[0].InnerText))
                {
                    Console.WriteLine("No Server IP in Settings File.");
                }
                else
                {
                    ServerIP = serverIP[0].InnerText;
                }
            }

            // Server port
            {
                XmlNodeList serverPort = document.GetElementsByTagName(ServerPortName);
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
                    }
                }
            }

            // Email
            {
                XmlNodeList emails = document.GetElementsByTagName(EmailName);
                if (emails.Count != 1 && string.IsNullOrEmpty(emails[0].InnerText))
                {
                    Console.WriteLine("No Email in Settings file.");
                }
                else
                {
                    Email = emails[0].InnerText;
                }
            }

            // Notify setting
            {
                XmlNodeList notifySettings = document.GetElementsByTagName(NotifySettingName);
                if (notifySettings.Count != 1 && string.IsNullOrEmpty(notifySettings[0].InnerText))
                {
                    Console.WriteLine("No OnlyEmailOnFail in Settings file.");
                }
                else
                {
                    NotifySetting = notifySettings[0].InnerText;
                }
            }
        }

        public static void SaveFile()
        {
            string filePath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, SettingsFileName);
            XmlDocument document = new XmlDocument();
            XmlElement root = document.CreateElement("Root");
            document.AppendChild(root);

            // Server IP
            {
                XmlElement serverIPElement = document.CreateElement(ServerIPName);
                serverIPElement.InnerText = string.IsNullOrWhiteSpace(ServerIP) ? "" : ServerIP;
                root.AppendChild(serverIPElement);
            }

            // Server Port
            {
                XmlElement serverPortElement = document.CreateElement(ServerPortName);
                serverPortElement.InnerText = ServerPort.ToString();
                root.AppendChild(serverPortElement);
            }

            // Email
            {
                XmlElement emailElement = document.CreateElement(EmailName);
                emailElement.InnerText = string.IsNullOrWhiteSpace(Email) ? "" : Email;
                root.AppendChild(emailElement);
            }

            // Notify Setting
            {
                XmlElement notifySettingElement = document.CreateElement(NotifySettingName);
                notifySettingElement.InnerText = string.IsNullOrWhiteSpace(NotifySetting) ? "" : NotifySetting;
                root.AppendChild(notifySettingElement);
            }

            Console.WriteLine("Saving Settings file");
            document.Save(filePath);
        }
    }
}
