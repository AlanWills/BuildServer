using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace BuildServerUtils
{
    /// <summary>
    /// A base server which handles listening for client connections and has simple API to communicate back and forth
    /// </summary>
    public abstract class BaseServer
    {
        #region Properties and Fields

        /// <summary>
        /// The listener we can use to detect incoming connections from clients to the server
        /// </summary>
        private TcpListener Listener { get; set; }

        /// <summary>
        /// Our interface to the single client we are supporting for now
        /// </summary>
        private Comms ClientComms { get; set; } = new Comms();

        /// <summary>
        /// Determines whether we have clients connected
        /// </summary>
        public bool IsConnected { get { return ClientComms != null && ClientComms.IsConnected; } }

        #endregion

        public BaseServer(int port = 1490)
        {
            Listener = new TcpListener(IPAddress.Any, port);
            Listener.Start();
            ListenForNewClient();
        }
        
        public void Disconnect()
        {
            DisconnectClient();
            Listener.Stop();
        }

        /// <summary>
        /// Starts an asynchronous check for new connections
        /// </summary>
        private void ListenForNewClient()
        {
            Listener.BeginAcceptTcpClient(AcceptClient, null);
        }

        /// <summary>
        /// Callback for when a new client connects to the server
        /// </summary>
        /// <param name="asyncResult"></param>
        protected virtual void AcceptClient(IAsyncResult asyncResult)
        {
            DisconnectClient();

            ClientComms = new Comms(Listener.EndAcceptTcpClient(asyncResult));
            ClientComms.OnDataReceived += ProcessMessage;

            Console.WriteLine("Client connected");

            ListenForNewClient();
        }

        public void Send(string message)
        {
            if (IsConnected && !string.IsNullOrEmpty(message))
            {
                ClientComms.Send(message);
            }
        }

        public void DisconnectClient()
        {
            ClientComms?.Disconnect();
        }
        
        #region Message Callbacks

        /// <summary>
        /// A function which is called when the Client sends a message to the server.
        /// Override to perform custom message handling
        /// </summary>
        /// <param name="data"></param>
        protected virtual void ProcessMessage(byte[] data) { }

        #endregion
    }
}