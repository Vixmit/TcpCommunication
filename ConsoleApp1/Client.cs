using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace ConsoleApp1
{
    class Client
    {
        private static bool socketReady;
        private static TcpClient socket;
        private static NetworkStream stream;
        private static StreamWriter writer;
        private static StreamReader reader;

        private static void OnIncomingData(string data)
        {
            Console.WriteLine("Server's response: " + data);
        }
        public static void ConnectToServer()
        {
            if (socketReady)
                return;

            string host = "192.168.8.106";
            int port = 6543;

            try
            {
                socket = new TcpClient(host, port);
                stream = socket.GetStream();
                writer = new StreamWriter(stream);
                reader = new StreamReader(stream);
                socketReady = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Socket error: " + ex.Message);
            }
        }

        private static void Recieve()
        {
            while (true)
            {
                if (socketReady)
                {
                   // string data2 = Console.ReadLine();
                    if (stream.DataAvailable)
                    {
                        string data = reader.ReadLine();
                        if (data != null)
                            OnIncomingData(data);
                    }
                }
            }
        }
        static void Main(string[] args)
        {
            socketReady = false;
            ConnectToServer();
            Thread thr = new Thread(Recieve);
            thr.Start();
            while (true)
            {
                //if (socketReady)
                //{
                //    string data2 = Console.ReadLine();
                //    if (stream.DataAvailable)
                //    {
                //        string data = reader.ReadLine();
                //        if (data != null)
                //            OnIncomingData(data);
                //    }

                    string data2 = Console.ReadLine();
                    writer.WriteLine(data2);
                    writer.Flush();
                //}
            }
        }
    }
}
