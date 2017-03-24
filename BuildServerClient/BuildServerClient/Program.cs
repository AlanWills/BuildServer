using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace BuildServerClient
{
    class Program
    {
        private static bool Running = true;

        public static Dictionary<string, IClientCommand> CommandRegistry { get; private set; } = new Dictionary<string, IClientCommand>();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ClientSettings.ReadFile();
            BaseClient client = new BaseClient(ClientSettings.ServerIP, ClientSettings.ServerPort);
            client.ResponseReceived += Comms_ResponseReceived;

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetCustomAttribute<CommandAttribute>() != null))
            {
                CommandRegistry.Add(type.GetCustomAttribute<CommandAttribute>().Token, Activator.CreateInstance(type) as IClientCommand);

                // Debugging registered commands
                //Console.WriteLine("Found command " + type.GetCustomAttribute<CommandAttribute>().Token);
            }

            if (args.Length > 0)
            {
                List<string> argsList = args.ToList();

                bool found = false;

                Debug.Assert(argsList.Count > 0);

                foreach (KeyValuePair<string, IClientCommand> commandPair in CommandRegistry)
                {
                    if (argsList[0] == commandPair.Key)
                    {
                        argsList.RemoveAt(0);
                        commandPair.Value.Execute(client, argsList);
                        found = true;

                        break;
                    }
                }

                if (!found)
                {
                    Console.WriteLine("Unrecognized command: '" + argsList[0] + "'");
                }
                else
                {
                    while (Running)
                    {
                    }
                }
            }
            else
            {
                Console.WriteLine("No arguments passed to exe");
            }
        }

        private static void Comms_ResponseReceived()
        {
            Running = false;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Exception occurred with message: " + (e.ExceptionObject as Exception).Message);
        }
    }
}