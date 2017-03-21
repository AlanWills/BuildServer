﻿using LibGit2Sharp;
using System;
using System.IO;
using System.Threading;
using System.Xml;

namespace BuildServerClient
{
    public static class ClientSettings
    {
        #region Properties and Fields

        public const string ServerIPName = "ServerIP";
        public static string ServerIP { get; private set; }

        public const string ServerPortName = "ServerPort";
        public static int ServerPort { get; private set; }

        public const string EmailName = "Email";
        public static string Email { get; private set; }

        public const string NotifySettingName = "OnlyEmailOnFail";
        public static string NotifySetting { get; private set; }

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
                catch { return "Not A Git Repo"; }
            }
        }

        #endregion

        public static void ReadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("No Settings File at " + filePath);
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
                    Thread.Sleep(2000);
                    return;
                }

                ServerIP = serverIP[0].InnerText;
                Console.WriteLine("ServerIP = " + ServerIP);
            }

            // Server port
            {
                XmlNodeList serverPort = document.GetElementsByTagName(ServerPortName);
                if (serverPort.Count != 1 || string.IsNullOrEmpty(serverPort[0].InnerText))
                {
                    Console.WriteLine("No Server Port in Settings File.");
                    Thread.Sleep(2000);
                    return;
                }

                int port = -1;
                if (!int.TryParse(serverPort[0].InnerText, out port))
                {
                    Console.WriteLine("Server Port is not a number.");
                    Thread.Sleep(2000);
                    return;
                }

                ServerPort = port;
                Console.WriteLine("ServerPort = " + ServerPort);
            }

            // Email
            {
                XmlNodeList emails = document.GetElementsByTagName(EmailName);
                if (emails.Count != 1 && string.IsNullOrEmpty(emails[0].InnerText))
                {
                    Console.WriteLine("No Email in Settings file.");
                    Thread.Sleep(2000);
                    return;
                }

                Email = emails[0].InnerText;
                Console.WriteLine("Email = " + Email);
            }

            // Notify setting
            {
                XmlNodeList notifySettings = document.GetElementsByTagName(NotifySettingName);
                if (notifySettings.Count != 1 && string.IsNullOrEmpty(notifySettings[0].InnerText))
                {
                    Console.WriteLine("No OnlyEmailOnFail in Settings file.");
                    Thread.Sleep(2000);
                    return;
                }

                NotifySetting = notifySettings[0].InnerText;
                Console.WriteLine("NotifySetting = " + NotifySetting);
            }
        }
    }
}
