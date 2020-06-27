using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientServerLibrary
{
    /// <summary>
    /// Our primary server class that handles multiple client connections
    /// </summary>
    public class Server
    {
        // Store our list of client sockets.
        public static List<TcpClient> ClientSocketList { get; set; }

        // The socket for our server
        public static TcpListener ServerSocket { get; set; }

        // Tells our server that it should shutdown
        public static bool ShouldShutdownNow { get; set; } = false;

        public Server(string address, Int32 port)
        {
            // create the list to store all of our connected sockets
            ClientSocketList = new List<TcpClient>();

            // Convert the string address to an IPAddress type
            IPAddress localAddr = IPAddress.Parse(address);

            // TcpListener -- create the server socket
            ServerSocket = new TcpListener(localAddr, port);

            // Start listening for client requests.
            ServerSocket.Start();
            Console.WriteLine(" >> " + "Server Started");

            // Now listen for connections.
            ListenForConnections();
        }

        private void ListenForConnections() { 

            int counter = 0;
            counter = 0;   // for counting our connections.

            // Create a default client socket to be used by each thread.
            TcpClient clientSocket = default(TcpClient);

            // Enter the listening loop for detecting client connections.
            while (true)
            {
                // If we have received a signal that we should shut down break from the loop
                if (ShouldShutdownNow)
                    break;

                // Otherwise continue listening
                counter += 1;
                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                clientSocket = ServerSocket.AcceptTcpClient();

                // Store the client socket in the connected socket list.
                ClientSocketList.Add(clientSocket);

                // Once connected, hand off the new connection to client handler class
                Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started!");
                handleClient client = new handleClient();
                client.startClient(this, clientSocket, Convert.ToString(counter));
            }
        }

        /// <summary>
        /// A function for shutting down the server.
        /// </summary>
        public void Shutdown()
        {
            //// Shutdown and end connection         
            ServerSocket.Stop();
            Console.WriteLine(" >> " + "exit");
            Console.ReadLine();
        }

        /// <summary>
        /// Broadcasts a string message to a single specified network stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        public void BroadcastToSingle(TcpClient client, string message)
        {
            NetworkStream stream = client.GetStream();

            Byte[] sendBytes = Encoding.ASCII.GetBytes(message);
            stream.Write(sendBytes, 0, sendBytes.Length);
            stream.Flush();
            Console.WriteLine(" >> " + message);
        }

        /// <summary>
        /// A server routine to broadcast a string message to all clients currently connected to
        /// the server.
        /// </summary>
        /// <param name="message"></param>
        public void BroadcastToAll(string message)
        {
            foreach(TcpClient client in ClientSocketList)
            {
                BroadcastToSingle(client, message);
            }
        }
    }

    /// <summary>
    /// Class to handle each client request separately.
    /// </summary>
    public class handleClient
    {
        TcpClient clientSocket;
        // The server thart created this client thread
        Server ParentServer;
        string clientNum;

        /// <summary>
        /// The function to start the client thread.
        /// </summary>
        /// <param name="inClientSocket">The associated client socket</param>
        /// <param name="strClientNum">String containing the clients number</param>
        public void startClient(Server server, TcpClient inClientSocket, string strClientNum)
        {
            this.ParentServer = server;
            this.clientSocket = inClientSocket;
            this.clientNum = strClientNum;
            Thread ctThread = new Thread(()=>doChat());
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

            while ((true))
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

                    // Respond back to the client by echoing the message
                    // TODO:  Create a function for how to handle a server response for a speicfic application
                    serverResponse = "Server to client(" + clientNum + ") " + "#" + rCount + ": " + dataFromClient;

                    // Send our response to the clientsocket of this thread.
                    ParentServer.BroadcastToSingle(clientSocket,serverResponse);
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

