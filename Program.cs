﻿using System;
using System.IO;
using System.Threading;
using System.Xml;

namespace BuildServer
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Console.Title = "Build Server";

            string settingsFilePath = Path.Combine(Environment.CurrentDirectory, "Settings.xml");
            if (!File.Exists(settingsFilePath))
            {
                Console.WriteLine("No Settings.xml File found in " + settingsFilePath);
                Thread.Sleep(2);
                return;
            }

            XmlDocument document = new XmlDocument();
            document.Load(settingsFilePath);

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

            Server server = new Server(port);
            Console.WriteLine("Ready");

            while (true) { }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Exception occurred with message " + (e.ExceptionObject as Exception).Message);
            Thread.Sleep(2);
            return;
        }
    }
}