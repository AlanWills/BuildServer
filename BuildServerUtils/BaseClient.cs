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
        public Comms ServerComms { get; private set; }

        #endregion

        public BaseClient(string ipAddress, int portNumber = 1490)
        {
            // Attempt to connect
            try
            {
                ServerComms = new Comms(new TcpClient(ipAddress, portNumber));
                ServerComms.OnDataReceived += OnMessageReceived;

                Console.WriteLine("Connection succeeded");
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection failed");
                Console.WriteLine(e.Message);
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
