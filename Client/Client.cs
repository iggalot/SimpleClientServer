using System;
using System.Net.Sockets;

namespace Client
{
    public class Client
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Client...\r\n");

            //Connect("127.0.0.1","I'm a test message");
            Connect("10.211.55.5", "I'm a test message");

            Console.WriteLine("Press any key to close window...");
            Console.Read();
        }

        private static void Connect(string server, string message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to havea TcpServer
                // connected to the same address as specified by the server, port
                // combination
                Int32 port = 13000;
                TcpClient client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer response.
                // Buffer to store the response bytes.
                data = new byte[256];

                // String to store the response ASCII representation
                String responseData = String.Empty;

                // Read the first batch of the Tcpserver response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                // Close everything
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
    }
}
