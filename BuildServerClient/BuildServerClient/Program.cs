using LibGit2Sharp;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace BuildServerClient
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No settings file relative path passed in to executable.");
                Thread.Sleep(2000);
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ClientSettings.ReadFile(Path.Combine(Directory.GetCurrentDirectory(), args[0]));

            Client client = new Client(ClientSettings.ServerIP, ClientSettings.ServerPort);

            string branchName = "";
            using (Repository repo = new Repository(Directory.GetCurrentDirectory()))
            {
                branchName = repo.Head.FriendlyName;
            }

            client.ServerComms.Send("Request Build," + branchName + "," + ClientSettings.Email + "," + ClientSettings.NotifySetting);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Exception occurred with message: " + (e.ExceptionObject as Exception).Message);
        }
    }
}