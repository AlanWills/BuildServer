using System;
using System.Net.Http;

namespace BuildServerUtils
{
    public delegate void ResponseHandler();

    /// <summary>
    /// An interface to a client.
    /// Hides the nuts and bolts and provides a public interface of just data input and output from a data sender/receiver.
    /// </summary>
    public class Comms
    {
        #region Properties and Fields

        private HttpClient Client { get; set; }

        public event ResponseHandler ResponseReceived;

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
        public async void Send(HttpMethod method, string uri)
        {
            if (!string.IsNullOrWhiteSpace(uri))
            {
                HttpResponseMessage message = await Client.SendAsync(new HttpRequestMessage(method, uri));
                string result = await message.Content.ReadAsStringAsync();

                ResponseReceived?.Invoke();

                Console.WriteLine(result);
            }
        }

        #endregion
    }
}
