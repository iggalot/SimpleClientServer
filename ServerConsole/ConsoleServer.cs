using ClientServerLibrary;
using System;


namespace ConsoleServer
{
    /// <summary>
    /// Our console application that controls the creation of the server
    /// for handling multiple TcpClient connections
    /// from our client server library.
    /// </summary>
    public class ConsoleServer
    {
        /// <summary>
        /// The main routine of our console server.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Server server = new Server("10.211.55.5", (Int32)13000);
            
            // Shutdown the server
            server.Shutdown();
        }
    }
}
