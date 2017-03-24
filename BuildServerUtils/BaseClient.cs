using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerUtils
{
    /// <summary>
    /// A base client class which connects and communicates with an http server
    /// </summary>
    public class BaseClient
    {
        #region Properties and Fields

        private Comms Comms { get; set; } = new Comms();

        public event ResponseHandler ResponseReceived;

        #endregion

        public BaseClient(string ipAddress, int portNumber = 1490)
        {
            Comms.Connect(ipAddress, portNumber);

            Comms.ResponseReceived += Comms_ResponseReceived;
        }

        private void Comms_ResponseReceived()
        {
            ResponseReceived?.Invoke();
        }

        public void Get(string uri, params KeyValuePair<string, string>[] parameters)
        {
            Send(HttpMethod.Get, uri, parameters);
        }

        public void Post(string uri, params KeyValuePair<string, string>[] parameters)
        {
            Send(HttpMethod.Post, uri, parameters);
        }

        private void Send(HttpMethod method, string uri, params KeyValuePair<string, string>[] parameters)
        {
            StringBuilder fullRequest = new StringBuilder(uri);

            for (int i = 0; i < parameters.Length; ++i)
            {
                if (i == 0)
                {
                    fullRequest.Append("?");
                }
                else
                {
                    fullRequest.Append("&");
                }

                fullRequest.Append(parameters[i].Key + "=" + parameters[i].Value);
            }

            Comms.Send(method, fullRequest.ToString());
        }
    }
}