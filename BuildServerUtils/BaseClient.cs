using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerUtils
{
    /// <summary>
    /// A base client class which connects and communicates with a remote server
    /// </summary>
    public abstract class BaseClient
    {
        #region Properties and Fields

        /// <summary>
        /// The interface to the server
        /// </summary>
        public Comms ServerComms { get; private set; } = new Comms();

        /// <summary>
        /// Wrapper property for checking that the ServerComms is connected.
        /// </summary>
        public bool IsConnected { get { return ServerComms.IsConnected; } }

        #endregion

        public BaseClient(string ipAddress, int portNumber = 1490)
        {
            // Attempt to connect
            string error = "";
            if (TryConnect(ipAddress, portNumber, ref error))
            {
                Console.WriteLine("Connection succeeded");
            }
            else
            {
                Console.WriteLine("Connection failed");
            }
        }
        
        /// <summary>
        /// Attempts to connect to the inputted ip through the inputted port.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="portNumber"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool TryConnect(string ipAddress, int portNumber, ref string errorMessage)
        {
            try
            {
                ServerComms.Connect(ipAddress, portNumber);
                ServerComms.OnDataReceived += OnMessageReceived;

                return true;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
        }

        #region Callbacks

        /// <summary>
        /// A function which is called when this client receives a message.
        /// Override to perform behaviour when custom messages arrive.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void OnMessageReceived(byte[] data) { }
        
        #endregion
    }
}
