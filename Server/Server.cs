using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ConsoleApp2
{
    class Server
    {

        private static List<ServerClient> clients;
        private static List<ServerClient> disconnectedClients;

        public static int port = 6543;
        private static TcpListener server;
        private static bool serverStarted;

        private static void Recieve(object scc)
        {
            while (true)
            {
                ServerClient sc = scc as ServerClient;
                NetworkStream s = sc.tcp.GetStream();
                //Console.WriteLine("working...");
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(sc, data);
                    }
                }
                Thread.Sleep(50);
            }
        }
        static void Start()
        {
            clients = new List<ServerClient>();
            disconnectedClients = new List<ServerClient>();

            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();

                StartListening();
                serverStarted = true;
                Console.WriteLine("Server has been started on port: " + port.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Socket error: " + ex.Message);
            }


        }
        private static void OnIncomingData(ServerClient sc, string data)
        {
            //Broadcast(data, clients);
            Console.WriteLine(sc.clientName = " says: " + data);
        }
        private static void Broadcast(string data, List<ServerClient> cl)
        {
            foreach (ServerClient sc in cl)
            {
                try
                {
                    StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                    writer.WriteLine(data);
                    writer.Flush();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Write error: " + ex.Message + " to client" + sc.clientName);
                }
            }
        }
        private static void StartListening()
        {
            server.BeginAcceptTcpClient(AcceptTcpClient, server);
        }
        private static void AcceptTcpClient(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
            //StartListening();

            Broadcast(clients[clients.Count - 1].clientName + " has connected", clients);
        }
        private static bool IsConnected(TcpClient c)
        {
            try
            {
                if (c != null && c.Client != null && c.Client.Connected)
                {

                    if (c.Client.Poll(0, SelectMode.SelectRead))
                    {
                        return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                    }
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        static void Main(string[] args)
        {
            Start();
          
            while (clients.Count ==0)   //warunek ile na ile klientow czeka
            {

                StartListening();
            }

            foreach (ServerClient sc in clients)
            {
                if (!IsConnected(sc.tcp))
                {
                    sc.tcp.Close();
                    disconnectedClients.Add(sc);
                    continue;
                }
                else
                {
                    Thread thr = new Thread(Recieve);
                    thr.Start(sc);

                }
            }
            while (true)
            {
                string data2 = Console.ReadLine();
                Broadcast(data2, clients);
            }
        }
    }
    public class ServerClient
    {
        public TcpClient tcp;
        public string clientName;

        public ServerClient(TcpClient clientSocket)
        {
            clientName = "Guest";
            tcp = clientSocket;
        }
    }
}
