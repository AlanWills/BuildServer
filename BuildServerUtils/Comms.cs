using System;
using System.Net.Http;

namespace BuildServerUtils
{
    /// <summary>
    /// An interface to a client.
    /// Hides the nuts and bolts and provides a public interface of just data input and output from a data sender/receiver.
    /// </summary>
    public class Comms
    {
        #region Properties and Fields

        private HttpClient Client { get; set; }

        #endregion

        public Comms() { }
        
        public void Connect(string ip, int port)
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri("http://" + ip + ":" + port + "/");
        }

        public void Disconnect()
        {
            Client?.Dispose();
        }

        #region Data Sending Functions

        /// <summary>
        /// Convert a string to a byte array and then send to our client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="str"></param>
        public void Send(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                Client.SendAsync(new HttpRequestMessage(HttpMethod.Post, str));
            }
        }

        #endregion
    }
}
