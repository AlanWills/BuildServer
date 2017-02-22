using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Build Server";

            Server server = new Server();
            Console.WriteLine("Ready");

            while (true) { }
        }
    }
}