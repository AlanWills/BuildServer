using BuildServerUtils;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace BuildServerClient
{
    class Program
    {
        public static bool Running = true;

        private static Dictionary<string, IClientCommand> CommandRegistry = new Dictionary<string, IClientCommand>();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (args.Length < 1)
            {
                Console.WriteLine("No settings file relative path passed in to executable.  You will have to pass settings in to commands manually.");
            }
            else
            {
                ClientSettings.ReadFile(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, args[0]));
            }

            Client client = new Client(ClientSettings.ServerIP, ClientSettings.ServerPort);

            if (client.ServerComms == null || !client.ServerComms.IsConnected)
            {
                Console.WriteLine("Connection to build server failed");
                return;
            }

            Console.WriteLine("\nReady");

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetCustomAttribute<CommandAttribute>() != null))
            {
                CommandRegistry.Add(type.GetCustomAttribute<CommandAttribute>().Token, Activator.CreateInstance(type) as IClientCommand);

                // Debugging registered commands
                //Console.WriteLine("Found command " + type.GetCustomAttribute<CommandAttribute>().Token);
            }

            while (Running)
            {
                // Remove all whitespace
                string commandInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(commandInput))
                {
                    bool found = false;

                    List<string> parameters = commandInput.Split(' ').ToList();
                    parameters.RemoveAll(x => string.IsNullOrWhiteSpace(x));

                    Debug.Assert(parameters.Count > 0);

                    foreach (KeyValuePair<string, IClientCommand> commandPair in CommandRegistry)
                    {
                        if (parameters[0] == commandPair.Key)
                        {
                            parameters.RemoveAt(0);
                            commandPair.Value.Execute(client, parameters);
                            found = true;

                            break;
                        }
                    }

                    if (!found)
                    {
                        Console.WriteLine("Unrecognized command: '" + commandInput + "'");
                    }
                }
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Exception occurred with message: " + (e.ExceptionObject as Exception).Message);
        }
    }
}