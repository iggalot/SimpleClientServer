using ClientServerLibrary;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace ClientWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        NetworkStream serverStream = default(NetworkStream);
        string readData = null;

        string server = "10.211.55.5";
        Int32 port = 13000;

        bool isConnected = false;

        /// <summary>
        /// Constructor for the MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Actions for connecting to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ConnectClick(object sender, RoutedEventArgs e)
        {
            lbConnectStatus.Content = String.Empty;
            lbConnectStatus.Visibility = Visibility.Visible;


            if (String.IsNullOrEmpty(tbLoginName.Text))
            {
                lbConnectStatus.Visibility = Visibility.Visible;
                lbConnectStatus.Content = "You must enter a login name!";               
                return;
            }
                
            // Otherwise try to make the connection
            try
            {
                clientSocket = Client.Connect(server, port);
                serverStream = clientSocket.GetStream();
            }
            catch (ArgumentNullException excep)
            {
                Console.WriteLine("ArgumentNullException: {0}", excep);
                return;
            }
            catch (SocketException excep)
            {
                Console.WriteLine("SocketException: {0}", excep);
                return;
            }

            readData = "Connected to Chat Server...";
            msg();

            // If our socket is not connected, 
            if (!clientSocket.Connected)
            {
                lbConnectStatus.Visibility = Visibility.Visible;
                lbConnectStatus.Content = "Error connected to socket.";
                isConnected = false;
                return;
            } 

            // Send our login name to the server
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(tbLoginName.Text + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            // Start a thread for monitoring for incoming chat messages from the server
            Thread ctThread = new Thread(getMessage);
            ctThread.Start();

            // Update UI visibility features
            isConnected = true;
            spConnect.Visibility = Visibility.Collapsed;
            spConnectionActive.Visibility = Visibility.Visible;
        }

        // Actions for when the disconnect button is clicked
        private void Button_DisconnectClick(object sender, RoutedEventArgs e)
        {
            if (clientSocket.Connected)
            {
                readData = "You have disconnected from the server.";
                msg();

                // Disconnect from the stream and the socket
                Client.Disconnect(serverStream, clientSocket);

                isConnected = false;
                spConnectionActive.Visibility = Visibility.Collapsed;
                spConnect.Visibility = Visibility.Visible;
            }
        }

        // Actions for when the send button is clicked.
        private void Button_SendClick(object sender, RoutedEventArgs e)
        {
            // If there isn't a valid message to send, abort...
            if (String.IsNullOrEmpty(SendMessage.Text))
                return;

            // If the client is connected
            if (clientSocket.Connected)
            {
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(SendMessage.Text + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
            }
            else
            {
                readData = "Unable to deliver message. Client is not connected.";
                msg();
                isConnected = false;
            }
        }

        private void getMessage()
        {
            while (true)
            {

                string returndata="";
                // Ensure the client is stil connected before attempting to read.
                if (!clientSocket.Connected)
                    break;

                serverStream = clientSocket.GetStream();
                int buffSize = 0;
                byte[] inStream = new byte[65536];
                buffSize = clientSocket.ReceiveBufferSize;

                try
                {
                    serverStream.Read(inStream, 0, buffSize);
                    returndata = System.Text.Encoding.ASCII.GetString(inStream);
                    returndata = returndata.Trim('\0');
                    readData = "" + returndata;

                    // Trim the empty bytes at the end of the message

                }
                catch (System.IO.IOException e)
                {
                    // There is an error reading the data, perhaps because server has disconnected
                    // so we catch the error here.
                    readData = "Server has disconnected";   
                }
                finally
                {
                    // Display the message
                    msg();
                }            
            }
        }

        /// <summary>
        /// Send the message to the chat box message field
        /// </summary>
        private void msg()
        {
            Dispatcher.Invoke( () =>
                {
                   // ReceiveMessage.Text = readData;
                   ReceiveMessage.Text = ReceiveMessage.Text + Environment.NewLine + " >> " + readData; 
                }
            );
               
        }
    }
}
