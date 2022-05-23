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
        private static string GetIP(TcpClient client)
        {
                
            IPEndPoint? ep =( client.Client.RemoteEndPoint) as IPEndPoint;
            if (ep == null)
                return "unknown";
            return ep.Address.ToString();
        }


        static void Main(string[] args)
        {

            // получаем адреса для запуска сокета
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                listener.Start();

                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Установлено подключение: "+GetIP(client));
                    NetworkStream stream = client.GetStream();
                    StreamReader reader = new StreamReader(stream);
                    StreamWriter writer = new StreamWriter(stream);

                    Key NtrueKey = new Key();
                    byte[] PublicKey =NTRUEncyptService.ConvModQToBytes( NtrueKey.PublicKey());
                    writer.WriteLine(Convert.ToBase64String(PublicKey));
                    writer.Flush();
                    byte[] SessionKey=NTRUEncyptService.Decrypt(NtrueKey,Convert.FromBase64String(reader.ReadLine()));

                    byte[] vector = Convert.FromBase64String(reader.ReadLine());
                    string messageTypeConnection = reader.ReadLine();
                    List<byte[]> buffer=new List<byte[]>();
                    if (Convert.ToInt32(messageTypeConnection) == 1)
                    {
                        Console.WriteLine("Получение");
                        while (!reader.EndOfStream)
                        {
                            buffer.Add(Convert.FromBase64String(reader.ReadLine()));
                        }
                    }
                    else
                    {
                        Console.WriteLine("Отправление");
                        ShacalService shacal = new ShacalService(SessionKey);
                        CipherContext cipherContext = new CipherContext(shacal, 20, CipherContext.Mode.ECB, vector);
                        foreach (byte[] line in buffer)
                        {
                            byte[] sendmessage = cipherContext.Decrypt(line);
                            writer.WriteLine(Convert.ToBase64String(sendmessage));
                            writer.Flush();
                        }


                    }

                    writer.Close();
                    reader.Close();
                    stream.Close();
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
    }
}
