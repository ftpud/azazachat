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

    // класс сервера
    class Server
    {
        int port = 12312;
        TcpListener listener;
        List<TcpClient> Clients = new List<TcpClient>();


        public Server()
        {
            listener = new TcpListener( IPAddress.Any, port);
        }

        // запуск сервверра
        public void Run()
        {
            Console.WriteLine(">> waiting for clients");
            listener.Start();
            while (true)
            {
               TcpClient client = listener.AcceptTcpClient();
               // записываем клиента в список клиентов
               Clients.Add(client);
               // тред (поток) на каждого клиента
               Thread clientThread = new Thread( new ParameterizedThreadStart( ClientThreadBody ) );
               clientThread.Start(client);

               Console.WriteLine(">> client connected");
            }
        }

        // тело треда клиента
        void ClientThreadBody(object Client)
        {
            TcpClient CurrentClient = (TcpClient)Client;
            NetworkStream stream = CurrentClient.GetStream(); // получаем стрим данных

            SendToAll("Кто-то приконнектился! " + CurrentClient.Client.RemoteEndPoint.ToString());

            // пока клиент приконнекченный 
            while(CurrentClient.Connected)
            {
                try {
                    if (stream.DataAvailable) // если есть что ловить
                    {
                        byte[] buff = new byte[4096];
                        int read = stream.Read(buff, 0, 4096); //ловим до 4кб
                        string input = UTF8Encoding.UTF8.GetString(buff,0, read); // всё что поймали пихаем в стринг
                        Console.WriteLine( CurrentClient.Client.RemoteEndPoint.ToString() + ": " + input);

                        SendToAll(CurrentClient.Client.RemoteEndPoint.ToString() + ": " + input); // транслируем на всех месседж
                    }
                }
                catch {                    
                    return; // если ошибки - вырубаем поток
                }
                /***/
                Thread.Sleep(10); // чтоб поток не сжирал проц
            }
            
        }

        // трансляция всем
        public void SendToAll(string data)
        {
            byte[] buffer;
            buffer = UTF8Encoding.UTF8.GetBytes(data); // конвертим стринг в мсассив байтов
            foreach (TcpClient client in Clients) // перебором клиентов всем отсылаем
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
