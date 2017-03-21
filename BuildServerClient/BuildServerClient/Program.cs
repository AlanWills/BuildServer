using BuildServerUtils;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace BuildServerClient
{
    class Program
    {
        private static Dictionary<string, IClientCommand> CommandRegistry = new Dictionary<string, IClientCommand>();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (args.Length < 1)
            {
                Console.WriteLine("No settings file relative path passed in to executable.");
                Thread.Sleep(2000);
                return;
            }

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetCustomAttribute<CommandAttribute>() != null))
            {
                CommandRegistry.Add(type.GetCustomAttribute<CommandAttribute>().Token, Activator.CreateInstance(type) as IClientCommand);
            }

            ClientSettings.ReadFile(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, args[0]));
            Client client = new Client(ClientSettings.ServerIP, ClientSettings.ServerPort);
            Console.WriteLine("\nReady");

            while (true)
            {
                string command = Console.ReadLine();
                if (command == "quit")
                {
                    // Move to command
                    client.ServerComms.Send("quit");
                    client.ServerComms.Disconnect();
                    return;
                }

                foreach (KeyValuePair<string, IClientCommand> commandPair in CommandRegistry)
                {
                    if (command.StartsWith(commandPair.Key))
                    {
                        commandPair.Value.Execute(client);
                        break;
                    }

                    Console.WriteLine("Unrecognized command: '" + command + "'");
                }
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Exception occurred with message: " + (e.ExceptionObject as Exception).Message);
        }
    }
}