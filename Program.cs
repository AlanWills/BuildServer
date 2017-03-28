using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using static BuildServer.BuildServerSettings;

namespace BuildServer
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Console.Title = "Build Server";

            ReadSettingsFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.xml"));

            using (Server server = new Server(ServerIP, ServerPort, ClientPort))
            {
                Console.WriteLine("Ready");
                Console.ReadKey(true);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Exception occurred with message " + (e.ExceptionObject as Exception).Message);
            Thread.Sleep(2);
            return;
        }
    }
}