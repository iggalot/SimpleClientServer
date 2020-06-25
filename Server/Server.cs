using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    /// <summary>
    /// Our primary server class that handles multiple client connections
    /// </summary>
    public class Server
    {
        // Store our list of client sockets.
        public static List<TcpClient> clientSocketList { get; set; }

        static void Main(string[] args)
        {
            // Initialize our cliemnt socket list.
            clientSocketList = new List<TcpClient>();

            // Set the TcpListener on port 13000.
            Int32 port = 13000;
            //IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            IPAddress localAddr = IPAddress.Parse("10.211.55.5");

            // TcpListener -- create the server socket
            TcpListener serverSocket = new TcpListener(localAddr, port);

            // Create a default client socket to be used by each thread.
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            // Start listening for client requests.
            serverSocket.Start();
            Console.WriteLine(" >> " + "Server Started");

            counter = 0;   // for counting our connections.
            // Enter the listening loop.
            while (true)
            {
                counter += 1;
                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                clientSocket = serverSocket.AcceptTcpClient();

                // Store the client socket in the connected socket list.
                clientSocketList.Add(clientSocket);

                Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started!");
                handleClient client = new handleClient();
                client.startClient(clientSocket, Convert.ToString(counter));
            }
            //// Shutdown and end connection
            serverSocket.Stop();
            Console.WriteLine(" >> " + "exit");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Class to handle each client request separately.
    /// </summary>
    public class handleClient
    {
        TcpClient clientSocket;
        string clientNum;

        /// <summary>
        /// The function to start the client thread.
        /// </summary>
        /// <param name="inClientSocket">The associated client socket</param>
        /// <param name="strClientNum">String containing the clients number</param>
        public void startClient(TcpClient inClientSocket, string strClientNum)
        {
            this.clientSocket = inClientSocket;
            this.clientNum = strClientNum;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        /// <summary>
        /// The main thread function for each connected client.
        /// </summary>
        private void doChat()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[65536];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;

            while((true))
            {
                try
                {
                    // increment our response counter
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine(" >> " + "From client-" + clientNum + ": " + dataFromClient);


                    rCount = Convert.ToString(requestCount);

                    // Respond back to the client
                    serverResponse = "Server to client(" + clientNum + ") " + "#" + rCount + ": " + dataFromClient;
                    sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.Flush();
                    Console.WriteLine(" >> " + serverResponse);
                }

                catch (System.IO.IOException ex)
                {
                    Console.WriteLine(" >> Client #" + clientNum + " has disconnected.");
                    // Close the socket
                    clientSocket.Close();
                    break;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                    break;
                }
            }

        }
    }
}
