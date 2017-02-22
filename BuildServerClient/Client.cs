using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerClient
{
    public class Client : BaseClient
    {
        public Client(string ipAddress, int port = 1490) :
            base(ipAddress, port)
        {

        }

        protected override void OnMessageReceived(byte[] data)
        {
            Console.WriteLine(data.ConvertToString());
        }
    }
}
