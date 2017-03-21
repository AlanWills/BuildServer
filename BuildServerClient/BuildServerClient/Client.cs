﻿using BuildServerUtils;
using System;

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
            string dataString = data.ConvertToString();
            if (!string.IsNullOrWhiteSpace(dataString))
            {
                Console.WriteLine(data.ConvertToString());
            }
        }
    }
}
