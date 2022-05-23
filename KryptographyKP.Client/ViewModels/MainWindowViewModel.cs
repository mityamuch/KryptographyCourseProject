using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
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
        private static int _port = 8005; // порт сервера
        private static string _address = "127.0.0.1"; // адресс сервера
        private static TcpClient? _client;
        private NetworkStream _stream;
        byte[] _SHACALKey;


        public string InFilePath { get; set; }

        #endregion
        public MainWindowViewModel()
        {
            try
            {
                _client = new TcpClient(_address, _port);
                _stream = _client.GetStream();
                StreamWriter writer = new StreamWriter(_stream);
                StreamReader reader = new StreamReader(_stream);
                byte[] _publicKey = Convert.FromBase64String(reader.ReadLine());
                _SHACALKey = new byte[64];
                byte[] vector = new byte[8];
                Random rnd = new Random();
                rnd.NextBytes(_SHACALKey);
                byte[] encrypted = NTRUEncyptService.Encrypt(NTRUEncyptService.BytesToConvModQ(_publicKey), _SHACALKey);
                writer.WriteLine(Convert.ToBase64String(encrypted));
                writer.Flush();
                writer.Close();
                reader.Close();

            }
            catch
            {
                MessageBox.Show("Не удалось установить соединение с сервером");
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
           _download ??= new RelayCommand(_ => Download());




        public void Push()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string[] lines=null;
            if (openFileDialog.ShowDialog() == true)
            {
                InFilePath = openFileDialog.FileName;
            }
            if (InFilePath != null)
            {
                lines = File.ReadAllLines(InFilePath);
            }

            try
            {
                ShacalService shacal = new ShacalService(_SHACALKey);
                byte[] vector = new byte[8];
                Random rnd = new Random();
                rnd.NextBytes(vector);
                
                CipherContext cipherContext = new CipherContext(shacal, 20, CipherContext.Mode.ECB, vector);
                StreamWriter writer = new StreamWriter(_stream);
                writer.WriteLine(Convert.ToBase64String(vector));
                writer.Flush();
                writer.WriteLine(1);
                writer.Flush();
                foreach (var line in lines)
                {
                    byte[] sendmessage = cipherContext.Encrypt(Encoding.ASCII.GetBytes(line));
                    writer.WriteLine(Convert.ToBase64String(sendmessage));
                    writer.Flush();
                }
                writer.Close();
                _stream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (_client != null)
                    _client.Close();
            }
        }

        public void Download()
        {
            try
            {
                StreamWriter writer = new StreamWriter(_stream);
                writer.WriteLine(2);
                writer.Flush();
                Console.WriteLine("Получение");
                StreamReader reader = new StreamReader(_stream);
                List<byte[]> buffer = new List<byte[]>();
                var recievemessage = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    buffer.Add(Convert.FromBase64String(reader.ReadLine()));
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == true) 
                {
                    StreamWriter wr = new StreamWriter(saveFileDialog.FileName);
                    foreach (byte[] b in buffer)
                    {
                        wr.WriteLine(Convert.ToBase64String(b));
                    }
                    wr.Close();
                }


            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (_client != null)
                    _client.Close();
            }

        }

        


        #endregion 

    }
}

