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

namespace KryptographyKP.Client.ViewModels
{
    internal class MainWindowViewModel:ViewModelBase
    {
        #region Fields
        private ICommand _push;
        private ICommand _download;


        private string _enteredText;
        private string _encryptedText;
        byte[] _SHACALKey;


        public string InFilePath { get; set; }

        #endregion
        public MainWindowViewModel()
        {
            

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
           _download ??= new RelayCommand(_ => Get());

        public async void Push()
        {
            Key key = new Key();

            ClientServerCommunication com = new ClientServerCommunication();
            byte[] NtrueKey=NTRUEncyptService.ConvModQToBytes(key.PublicKey());





         


        }
        
        public void Get() 
        {


        }
        #endregion 



    }
}

