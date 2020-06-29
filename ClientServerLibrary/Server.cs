using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static Hashtable ClientSocketList { get; set; }

        // The socket for our server
        public static TcpListener ServerSocket { get; set; }

        // Tells our server that it should shutdown
        public static bool ShouldShutdownNow { get; set; } = false;

        public Server(string address, Int32 port)
        {
            // create the list to store all of our connected sockets
            ClientSocketList = new Hashtable();

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

                byte[] bytesFrom = new byte[65536];
                string dataFromClient = null;

                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                // Store the client socket in the connected socket list if it isn't already there.
                if(!ClientSocketList.Contains(dataFromClient))
                    ClientSocketList.Add(dataFromClient, clientSocket);

                // Announce a connection
                BroadcastToAll(dataFromClient + " has joined the chat. ", dataFromClient, false);

                // Once connected, hand off the new connection to client handler class
                Console.WriteLine(" >> " + dataFromClient + " has joined.");
                handleClient client = new handleClient();
                client.startClient(clientSocket, dataFromClient, ClientSocketList);
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
        /// A server routine to broadcast a string message to all clients currently connected to
        /// the server.
        /// </summary>
        /// <param name="message"></param>
        public static void BroadcastToAll(string message, string uName, bool flag)
        {
            foreach(DictionaryEntry client in ClientSocketList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)client.Value;

                if (broadcastSocket == null || !broadcastSocket.Connected)
                {
                    ClientSocketList.Remove(client);
                    continue;
                }

                try
                {
                    NetworkStream broadcastStream = broadcastSocket.GetStream();
                    Byte[] broadcastBytes = null;

                    if (flag == true)
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(uName + " says : " + message);
                    }
                    else
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(message);
                    }

                    broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                    broadcastStream.Flush();
                } 
                catch (System.IO.IOException e)
                {
                    // If our socket has disconnected, remove the client
                    continue;
                }
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
        Hashtable clientsList;
        string clientNum;

        /// <summary>
        /// The function to start the client thread.
        /// </summary>
        /// <param name="inClientSocket">The associated client socket</param>
        /// <param name="strClientNum">String containing the client's number</param>
        public void startClient(TcpClient inClientSocket, string strClientNum, Hashtable clist)
        {
            this.clientSocket = inClientSocket;
            this.clientNum = strClientNum;
            this.clientsList = clist;
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
                if (!clientSocket.Connected)
                    break;

                try
                {
                    // increment our response counter
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine(" >> " + "From client - " + clientNum + ": " + dataFromClient);
                    rCount = Convert.ToString(requestCount);

                    Server.BroadcastToAll(dataFromClient, clientNum, true);
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

