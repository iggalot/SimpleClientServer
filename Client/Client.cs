using System;
using System.Net.Sockets;

namespace ConsoleClient
{
    public class Client
    {
        static public void Main(string[] args)
        {
            // Unused main function.  See GitHub initial commit for command line 
            // implementation of this function.
        }

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

        public static TcpClient Connect(string v, Int32 port)
        {
            return new TcpClient(v, port);
        }

        public static void Disconnect(NetworkStream stream, TcpClient client)
        {
            stream.Close();
            client.Close();
        }

        public static void Send(NetworkStream stream, string message)
        {
            // Translate the passed message into ASCII and store it as a Byte array
            Byte[] senddata = System.Text.Encoding.ASCII.GetBytes(message+"$");

            // Send the message to the connected TcpServer.
            stream.Write(senddata, 0, senddata.Length);

            Console.WriteLine("Sent: {0}", message);
        }
    }
}
