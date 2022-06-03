using System;

using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Grpc.Net.Client;
using Microsoft.Win32;
using MVVM.Core;
using NTRUEncrypt;
using SHACAL;
using GrpcClient;
using System.Net.Http;
using GrpcServer;
using Key = NTRUEncrypt.Key;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.IO;
using System.Collections.ObjectModel;

namespace KryptographyKP.Client.ViewModels
{
    internal class MainWindowViewModel:ViewModelBase
    {
        #region Fields
        private ICommand _push;
        private ICommand _download;
        private ICommand _update;
        private ICommand _checkShacal;

        public ObservableCollection<string> Filenames { get; set; } = new ObservableCollection<string>();

        public ClientServerCommunication clientServerCommunication = new ClientServerCommunication();
        private CancellationToken token=new CancellationToken();
        private string _Username;
        private string _Description;
        private int _selectedIndex;
        private string _btVisibility= "Visible";
        private int _ProgrProperty;
        byte[] _SHACALKey;
        private string _name_of_another;
        private byte[] _NtrueOtherkey;
        public Key key = new Key();
        CipherContext cipherContext;

        public  void SetUser(string name)
        {
            Username = name;
        }

        #endregion

        public MainWindowViewModel()
        {
            string a="";
            foreach (Window window in App.Current.Windows)
            {
                // если окно - объект TaskWindow
                if (window is AutorizeWindow)
                    a = (window as AutorizeWindow).textbox.Text;
            }

            SetUser(a);
            Start();

            ChangeButtonVisibility();

        }

        #region Properties
        public string Description
        {
            get =>
                _Description;

            private set
            {
                _Description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
        public string Username
        {
            get =>
                _Username;

            private set
            {
                _Username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string BtVisibility
        {
            get =>
                _btVisibility;

            private set
            {
                _btVisibility = value;
                OnPropertyChanged(nameof(BtVisibility));
            }
        }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }

            set
            {
                if (_selectedIndex == value||value==-1)
                {
                    return;
                }
                _selectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
            }
        }

        public int ProgrProperty
        {
            get
            {
                return _selectedIndex;
            }

            set
            {
                _ProgrProperty = value;
                OnPropertyChanged(nameof(ProgrProperty));
            }
        }

        #endregion

        #region Commands

        public ICommand PushCommand =>
            _push ??= new RelayCommand(_ => Push());

        public ICommand DownloadCommand =>
           _download ??= new RelayCommand(_ => Get());

        public ICommand UpdateCommand =>
           _update ??= new RelayCommand(_ => Update());

        public ICommand CheckShacalCommand =>
           _checkShacal ??= new RelayCommand(_ => CheckShacal());

        public async void OnWindowClosing(object sender, CancelEventArgs e)
        {
            await clientServerCommunication.Saybye(Username);
        }

        public async void Push()
        {
            ProgrProperty = 0;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All files(*.*)|*.*";
            dialog.InitialDirectory = Directory.GetCurrentDirectory();

            if (dialog.ShowDialog() == false)
                return;
            ProgrProperty = 20;
            string filename = Path.GetFileNameWithoutExtension(dialog.FileName);
            byte[] fileTextByte = System.IO.File.ReadAllBytes(dialog.FileName);
            if (fileTextByte.Length == 0)
            {
                MessageBox.Show("Вы выбрали пустой файл. Выберите другой");
                return;
            }
            ProgrProperty = 40;
            Description = "Файл: " + filename + " открыт и готов для шифрования.";
            ShacalService shacal = new ShacalService(_SHACALKey);
            cipherContext = new CipherContext(shacal, 20, CipherContext.Mode.ECB, new byte[64]);
            byte[] encryptedFile = EncryptFile(fileTextByte);
            ProgrProperty = 60;
            if (encryptedFile is null)
            {
                MessageBox.Show("У вас нет необходимых ключей для шифрования.");
                return;
            }
            Description = "Файл: " + filename + " зашифрован.";
            //И передача его серваку
            try
            {
                ProgrProperty = 80;
                var sending = await clientServerCommunication.SendFile(filename,encryptedFile,token);
                Description = "Файл: " + filename + " зашифрован и отправлен на сервер.";
                ProgrProperty = 0;
            }
            catch
            {
                MessageBox.Show("Файл слишком велик или неподходящего формата");
            }
        }
        
        public async void Get() 
        {

            if (SelectedIndex == -1)
            {
                MessageBox.Show("Обновите список файлов в листбоксе и выберите файл содержащий \"Mess\" - файл сообщения.");
                return;
            }
            try
            {
                string filename = Filenames[SelectedIndex];
                if (filename.Contains(".Mess"))
                {
                    //List<byte> tmp = new List<byte>;
                    byte[] IsSend = await clientServerCommunication.TakeFileAsync(filename, token);
                    ShacalService shacal = new ShacalService(_SHACALKey);
                    cipherContext = new CipherContext(shacal, 20, CipherContext.Mode.ECB, new byte[64]);
                    var decryptedText = DecryptFile(IsSend);
                    string path = Directory.GetCurrentDirectory();
                    if (!Directory.Exists(path + "/Messages/"))
                    {
                        Directory.CreateDirectory(path + "/Messages/");
                    }
                    if (decryptedText is null)
                    {
                        MessageBox.Show("Отсутствуют необходимые ключи для расшифровки. Попробуйте загрузить с сервера ключ Shacal");
                        return;
                    }
                    File.WriteAllBytes(path + "/Messages/" + filename + ".txt", decryptedText);
                    Description = "Файл: " + filename + " сохранён в папку:\n" + path + "\\Messages\\";
                }
                else
                {
                    MessageBox.Show("Вы выбрали не файл сообщения, выберите файл содержащий Mess");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при попытке расшифровать файл. " + ex.Message);
            }
        }
        
        public async void Update()
        {
            ProgrProperty = 0;
            HttpClientHandler unsafeHandler = new HttpClientHandler();
            GrpcChannel _channel;
            CryptographyServer.CryptographyServerClient _client;
            _channel = GrpcChannel.ForAddress("http://localhost:5055", new GrpcChannelOptions
            {
                HttpHandler = unsafeHandler
            });
            ProgrProperty = 20;
            _client = new CryptographyServer.CryptographyServerClient(_channel);

            Filenames.Clear();
            ProgrProperty = 60;
            List<string> files = new List<string>();
            var taking = (await _client.TakeAllFileNamesAsync(new HelloRequest { Name = Username })).Files.Split(',');
            ProgrProperty = 80;
            foreach (var f in taking)
                files.Add(f);

            foreach (var file in files)
            {
                Filenames.Add(file);
            }
            ProgrProperty = 0;

        }

        //nTRUE асимметричный
        //Shacal симметричный
        
        public async void ChangeButtonVisibility()
        {
            var others = await clientServerCommunication.IsAnyoneHere(token);
            if (others.Length != 1)
            {
                BtVisibility = "Visible";
            }
            else
            {
                BtVisibility = "Collapsed";
                
            }
        }

        public async void CheckShacal()
        {
            try
            {
                ProgrProperty = 0;
                var others = await clientServerCommunication.IsAnyoneHere(token);
                if (others.Length == 0)
                {
                    MessageBox.Show("Вы одни в сети или зашифрованный ключ Shacal еще не сформирован другой стороной... Попробуйте позже");
                    return;
                }
                _name_of_another = (others[0] == Username) ? others[1] : others[0]; ;
                ProgrProperty = 10;
                var takeShacalKey = await clientServerCommunication.TakeShacalKeyAsync(_name_of_another, token);
                if (takeShacalKey == null)
                    Description = $"Не нашли файла {_name_of_another}.Shacal.txt";
                else
                {
                    //Расшифровка ключа
                    ProgrProperty = 20;
                    byte[] Shacalkey = NTRUEncyptService.Decrypt(key, takeShacalKey);
                    ProgrProperty = 80;
                    _SHACALKey = Shacalkey;
                    Description = $"Нашли файл {_name_of_another}.Shacal.txt, \nВыберите файл для зашифровки и отправки выше.";
                    ProgrProperty = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка получения ключа Shacal - " + ex.Message);
            }
        }

        private async Task Start()
        {
            ProgrProperty = 0;
            try
            {
                var others = await clientServerCommunication.Registration(Username, token);

                if (others.Length == 0)
                {
                    Description = "Пока вы на сервере один, ждите появления другого пользователя.";

                    var isClean = await clientServerCommunication.CleanDirectory(token);
                    ProgrProperty = 20;
                    if (isClean == -1)
                    {
                        MessageBox.Show("Сервер не готов к работе...");
                        Application.Current.Shutdown();
                    }
                    ProgrProperty = 40;
                    byte[] NtrueKey = NTRUEncyptService.ConvModQToBytes(key.PublicKey());
                    ProgrProperty = 80;
                    int IsWrittenOnServer = await clientServerCommunication.SendNtrueKeyAsync(NtrueKey, token);
                    if (IsWrittenOnServer == -1)
                        Description = "Не удалось отослать файл на сервер";
                    ProgrProperty = 0;
                }
                else
                {
                    ProgrProperty = 20;
                    Key key = new Key();
                    byte[] NtrueKey = NTRUEncyptService.ConvModQToBytes(key.PublicKey());
                    int IsWrittenOnServer = await clientServerCommunication.SendNtrueKeyAsync(NtrueKey, token);
                    if (IsWrittenOnServer == -1)
                        Description = "Не удалось отослать файл на сервер";
                    ProgrProperty = 40;
                    _name_of_another = others[0];
                    byte[] IsTaken = await clientServerCommunication.TakeNtrueKeyAsync(_name_of_another, token);
                    ProgrProperty = 60;
                    if (IsTaken == null)
                    {
                        Description = $"Не нашли файла ${_name_of_another}.NtrueKey.txt";
                        return;//
                    }
                    else
                    {
                        _NtrueOtherkey = IsTaken;
                    }
                    ProgrProperty = 80;
                    Random rnd = new Random();
                    byte[] Shacalkey = new byte[64];
                    _SHACALKey = Shacalkey;
                    rnd.NextBytes(Shacalkey);
                    ProgrProperty = 85;
                    byte[] encryptedShacalKey = NTRUEncyptService.Encrypt(NTRUEncyptService.BytesToConvModQ(_NtrueOtherkey),Shacalkey);
                    ProgrProperty = 95;
                    int IsSend = await clientServerCommunication.SendShacalKeyAsync(encryptedShacalKey, token);
                    if (IsSend == -1)
                        Description = "Не удалось записать файл на сервер...";
                    ProgrProperty = 0;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Произошла ошибка при регистрации." + e.Message);
                return;
            }
        }

        public byte[] EncryptFile(byte[] fileInfo)
        {
            byte[] filePart = cipherContext.Encrypt(fileInfo);
            return filePart;
        }
        public byte[] DecryptFile(byte[] fileInfo)
        {
            byte[] filePart = cipherContext.Decrypt(fileInfo);
            return filePart;
        }

        #endregion 



    }
}

