using BuildServerUtils;
using System;
using System.Collections.Generic;

namespace BuildServerClient
{
    public class Client : BaseClient
    {
        public Client(string ipAddress, int port = 1490) :
            base(ipAddress, port)
        {

        }

        public override void Disconnect()
        {
            if (IsConnected)
            {
                new QuitCommand().Execute(this, new List<string>());
            }
        }

        protected override void OnMessageReceived(byte[] data)
        {
            string dataString = data.ConvertToString();

            if (!string.IsNullOrWhiteSpace(dataString))
            {
                Console.WriteLine(data.ConvertToString());
            }
        }
    }
}
