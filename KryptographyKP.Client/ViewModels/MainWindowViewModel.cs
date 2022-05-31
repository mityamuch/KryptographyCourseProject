using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MVVM.Core;
using NTRUEncrypt;
using SHACAL;

namespace KryptographyKP.Client.ViewModels
{
    internal class MainWindowViewModel:ViewModelBase
    {
        #region Fields
        private ICommand _push;
        private ICommand _download;


        private string _enteredText;
        private string _encryptedText;


        private const string host = "127.0.0.1";
        private const int port = 8888;

        static TcpClient client;
        static NetworkStream stream;

        byte[] _SHACALKey;


        public string InFilePath { get; set; }

        #endregion
        public MainWindowViewModel()
        {
            try
            {
                client = new TcpClient();
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream();
            }
            catch (Exception ex)
            {
               MessageBox.Show(ex.Message);
            }
        }
        #region Properties
        public string EnteredText
        {
            get =>
                _enteredText;

            set
            {
                _enteredText = value;
                OnPropertyChanged(nameof(EnteredText));
            }
        }
        public string EncryptedText
        {
            get =>
                _encryptedText;

            private set
            {
                _encryptedText = value;
                OnPropertyChanged(nameof(EncryptedText));
            }
        }
        #endregion
        #region Commands

        public ICommand PushCommand =>
            _push ??= new RelayCommand(_ => Push());

        public ICommand DownloadCommand =>
           _download ??= new RelayCommand(_ => ReceiveMessage());




        public void Push()
        {
            try
            {
                string message = EnteredText;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); //старт потока
                SendMessage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }
        


        #endregion 

        /*
        private void Autorise()
        {
            _client = new TcpClient(_address, _port);
            _stream = _client.GetStream();
            using (StreamWriter writer = new StreamWriter(_stream))
            {
                using (StreamReader reader = new StreamReader(_stream)) 
                { 
                    byte[] _publicKey = Convert.FromBase64String(reader.ReadLine());
                    _SHACALKey = new byte[64];
                    byte[] vector = new byte[8];
                    Random rnd = new Random();
                    rnd.NextBytes(_SHACALKey);
                    byte[] encrypted = NTRUEncyptService.Encrypt(NTRUEncyptService.BytesToConvModQ(_publicKey), _SHACALKey);
                    writer.WriteLine(Convert.ToBase64String(encrypted));
                    for (; ; )
                    {
                        var l = reader.ReadLine();
                        writer.WriteLine("It's me");
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }

        }
        */
        // отправка сообщений
        private void SendMessage()
        {

           //while (true)
           //{
                //string message = EnteredText;
                //byte[] data = Encoding.Unicode.GetBytes(message);
                //stream.Write(data, 0, data.Length);
            //}
        }
        // получение сообщений
        private void ReceiveMessage()
        {

                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    EncryptedText+= message;//вывод сообщения
                }
                catch
                {
                   MessageBox.Show("Подключение прервано!"); //соединение было прервано
                    Disconnect();
                }
            
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }




    }
}

