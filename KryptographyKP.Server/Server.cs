using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using NTRUEncrypt;
using SHACAL;

namespace KryptographyKP.Server
{
    internal class Server
    {
        static int port = 8005;
        static TcpListener listener;
        
        static ServerObject server; // сервер
        static Thread listenThread; // потока для прослушивания

        static void Main(string[] args)
        {
                try
                {
                    server = new ServerObject();
                    listenThread = new Thread(new ThreadStart(server.Listen));
                    listenThread.Start(); //старт потока
                }
                catch (Exception ex)
                {
                    server.Disconnect();
                    Console.WriteLine(ex.Message);
                }
        }
    }
}
