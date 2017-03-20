using System;
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

            BuildServerSettings.ReadFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.xml"));

            Server server = new Server(BuildServerSettings.ServerPort);
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