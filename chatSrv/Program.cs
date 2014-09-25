using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;



namespace chatSrv
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Server srv = new Server();
            srv.Run();           
        }
    }


    class Server
    {
        int port = 12312;
        TcpListener listener;
        List<TcpClient> Clients = new List<TcpClient>();


        public Server()
        {
            listener = new TcpListener( IPAddress.Any, port);
        }

        public void Run()
        {
            Console.WriteLine(">> waiting for clients");
            listener.Start();
            while (true)
            {
               TcpClient client = listener.AcceptTcpClient();
               Clients.Add(client);
               Thread clientThread = new Thread( new ParameterizedThreadStart( ClientThreadBody ) );
               clientThread.Start(client);
               Console.WriteLine(">> client connected");
            }
        }

        void ClientThreadBody(object Client)
        {
            TcpClient CurrentClient = (TcpClient)Client;
            NetworkStream stream = CurrentClient.GetStream();

            SendToAll("Кто-то приконнектился! " + CurrentClient.Client.RemoteEndPoint.ToString());

            while(CurrentClient.Connected)
            {
                try {
                    if (stream.DataAvailable)
                    {
                        byte[] buff = new byte[4096];
                        int read = stream.Read(buff, 0, 4096);
                        string input = UTF8Encoding.UTF8.GetString(buff,0, read);
                        Console.WriteLine( CurrentClient.Client.RemoteEndPoint.ToString() + ": " + input);

                        SendToAll(CurrentClient.Client.RemoteEndPoint.ToString() + ": " + input);
                    }
                }
                catch {                    
                    return;
                }
                /***/
                Thread.Sleep(1);
            }
            
        }

        public void SendToAll(string data)
        {
            byte[] buffer;
            buffer = UTF8Encoding.UTF8.GetBytes(data);
            foreach (TcpClient client in Clients)
            { 
                try
                {
                    client.GetStream().Write(buffer, 0, buffer.Length);
                }
                catch
                {
                    lock (Clients) { 
                        Clients.Remove(client);                         
                    }
                }
            }
        }

    }
}
