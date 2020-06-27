using System;
using System.Net.Sockets;

namespace ClientServerLibrary
{
    /// <summary>
    /// A generic client class utilizing the TcpClient protocol.  Requires a TcpListener 
    /// on the other end to create the socket before communications can start.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Receives data from the connected socket and stores it in a NetworkStream.
        /// The byte stream information is then encoded to an ASCII string and returned
        /// to the calling function.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string Receive(NetworkStream stream)
        {
            // Receive the TcpServer response.
            // Buffer to store the response bytes.
            Byte[] receivedata = new byte[256];

            // String to store the response ASCII representation
            String responseData = String.Empty;

            // Read the first batch of the Tcpserver response bytes.
            Int32 bytes = stream.Read(receivedata, 0, receivedata.Length);
            responseData = System.Text.Encoding.ASCII.GetString(receivedata, 0, bytes);
            Console.WriteLine("Received: {0}", responseData);

            return responseData;
        }

        /// <summary>
        /// Connects our client to a tcpip socket
        /// </summary>
        /// <param name="v">Address to connect to</param>
        /// <param name="port">Port number to connect to</param>
        /// <returns></returns>
        public static TcpClient Connect(string v, Int32 port)
        {
            return new TcpClient(v, port);
        }

        /// <summary>
        /// Disconnects the client and associated network stream
        /// </summary>
        /// <param name="stream">The network stream to be used</param>
        /// <param name="client">The client that should be disconnected.</param>
        public static void Disconnect(NetworkStream stream, TcpClient client)
        {
            stream.Close();
            client.Close();
        }

        /// <summary>
        /// Sends a string message across the socket.  Also appends a "$" terminator
        /// to the end of the string for the server to signal that the end of line has been 
        /// received.
        /// </summary>
        /// <param name="stream">The network stream to be used in the transmission</param>
        /// <param name="message">The string message to send</param>
        public static void Send(NetworkStream stream, string message)
        {
            // Translate the passed message into ASCII and store it as a Byte array
            Byte[] senddata = System.Text.Encoding.ASCII.GetBytes(message + "$");

            // Send the message to the connected TcpServer.
            stream.Write(senddata, 0, senddata.Length);

            Console.WriteLine("Sent: {0}", message);
        }
    }
}

