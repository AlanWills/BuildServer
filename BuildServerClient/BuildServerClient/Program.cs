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

            ClientSettings.ReadFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.xml"));

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