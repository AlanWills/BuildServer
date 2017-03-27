using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace BuildServerUtils
{
    /// <summary>
    /// A base server which handles listening for client connections and has simple API to communicate back and forth
    /// </summary>
    public abstract class BaseServer : IDisposable
    {
        #region Properties and Fields

        /// <summary>
        /// The listener we can use to detect incoming connections from clients to the server
        /// </summary>
        protected HttpListener Listener { get; private set; }

        public string BaseAddress { get; private set; }
        
        #endregion

        public BaseServer(string ip, int port)
        {
            BaseAddress = "http://" + ip + ":" + port.ToString();

            Listener = new HttpListener();
            Listener.Start();

            ListenForMessages();
        }
        
        public void Dispose()
        {
            Listener.Close();
        }
        
        private async void ListenForMessages()
        {
            HttpListenerContext context = await Listener.GetContextAsync();
            ProcessMessage(context);

            ListenForMessages();
        }
        
        #region Message Callbacks

        /// <summary>
        /// A function which is called when a message is sent to the server.
        /// Override to perform custom message handling
        /// </summary>
        /// <param name="data"></param>
        protected virtual void ProcessMessage(HttpListenerContext requestContext) { }

        #endregion
    }
}