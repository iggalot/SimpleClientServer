using ClientServerLibrary;
using System;
using System.Net.Sockets;
using System.Windows;

namespace ClientWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient _client = null;
        string server = "10.211.55.5";
        Int32 port = 13000;

        bool isConnected = false;

        // Get a client stream for reading and writing.
        NetworkStream stream;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_ConnectClick(object sender, RoutedEventArgs e)
        {
            string[] arg = null;

            try
            {
                _client = Client.Connect(server, port);
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

            // Get a client stream for reading and writing.
            stream = _client.GetStream();
            isConnected = true;
            spConnectionActive.Visibility = Visibility.Visible;
            btnConnect.Visibility=Visibility.Collapsed;
        }

        // Actions for when the disconnect button is clicked
        private void Button_DisconnectClick(object sender, RoutedEventArgs e)
        {
            if (_client.Connected)
            {
                // Disconnect from the stream and the socket
                Client.Disconnect(stream, _client);
                isConnected = false;
                spConnectionActive.Visibility = Visibility.Collapsed;
                btnConnect.Visibility = Visibility.Visible;
            }
        }

        // Actions for when the send button is clicked.
        private void Button_SendClick(object sender, RoutedEventArgs e)
        {
            if (_client.Connected)
            {
                // Send the message
                Client.Send(stream, SendMessage.Text);

                // Receive the response and display in the appropriate location
                ReceiveMessage.Text = Client.Receive(stream);
            } else
            {
                ReceiveMessage.Text = "Unable to deliver message. Client is not connected.";
                isConnected = false;
            }
        }
    }
}
