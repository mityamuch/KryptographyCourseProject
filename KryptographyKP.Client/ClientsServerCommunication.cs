using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Grpc.Net.Client;
using Google.Protobuf;
using System.Threading;
using GrpcServer;
using Grpc.Net.Client.Web;
using Grpc.Core;

namespace KryptographyKP.Client
{
    internal class ClientServerCommunication
    {
        private static HttpClientHandler unsafeHandler = new HttpClientHandler();
        

        private string _myName = "mityamuch";
        private GrpcChannel _channel;
        
        private CryptographyServer.CryptographyServerClient _client;
        

        private string _name_of_another;

        public ClientServerCommunication()
        {
        //unsafeHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        _channel = GrpcChannel.ForAddress("http://localhost:5055", new GrpcChannelOptions
        {
            HttpHandler = unsafeHandler
        });

            _client = new CryptographyServer.CryptographyServerClient(_channel);
        }
        ~ClientServerCommunication()
        {
            _channel.Dispose();
        }
        public async Task<string[]> Registration(string name, CancellationToken token)
        {
            try
            {
                _myName = name;

                var response = await _client.SayHelloAsync(new HelloRequest { Name = _myName });
                Console.WriteLine();
                return await IsAnyoneHere(token);
            }
            catch
            {
                throw new InvalidDataException();
            }
        }

        public async Task Saybye(string name)
        {
            var response = await _client.SayByeAsync(new HelloRequest { Name = _myName });
        }

        public async Task<string[]> IsAnyoneHere(CancellationToken token)
        {
            try
            {
                var others = (await _client.WhoAtServerAsync(new HelloRequest { Name = _myName })).Users.Split(',');
                others = others.Take(others.Length - 1).ToArray();
                return others;
            }
            catch
            {
                throw new InvalidDataException();
            }
        }
        public async Task<int> CleanDirectory(CancellationToken token)
        {
            try
            {
                var clearing = await _client.ClearDirAsync(new HelloRequest { Name = _myName });
                if (clearing.IsClear == false)
                    return -1;
                else return 0;
            }
            catch
            {
                throw new IOException();
            }
        }

        public async Task<int> SendNtrueKeyAsync(byte[] key, CancellationToken token)
        {
            try
            {
                var sendNtrueKey = await _client.SendFileAsync(new FileBuffer
                {
                    Filename = $"{_myName}.NtrueKey",
                    Info = ByteString.CopyFrom(key)
                });
                if (sendNtrueKey.IsWrittenInServer != true)
                    return -1;

                else return 0;
            }
            catch
            {
                throw new InvalidDataException();
            }
        }

        public async Task<int> SendShacalKeyAsync(byte[] toSend, CancellationToken token)
        {
            try
            {
                var sendShacalKey = await _client.SendFileAsync(new FileBuffer
                {
                    Filename = $"{_myName}.ShacalKey",
                    Info = ByteString.CopyFrom(toSend)
                });
                if (sendShacalKey.IsWrittenInServer != true)
                    return -1;
                else return 0;
            }
            catch
            {
                throw new InvalidDataException();
            }
        }

        public async Task<byte[]> TakeNtrueKeyAsync(string another, CancellationToken token)
        {
            try
            {
                _name_of_another = another;
                var takeNtrueKey = await _client.TakeFileAsync(new WhatFile { Filename = $"{_name_of_another}.NtrueKey" });
                if (takeNtrueKey.Filename == "")
                    return null;
                else
                {
                    return takeNtrueKey.Info.ToByteArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<byte[]> TakeShacalKeyAsync(string another, CancellationToken token)
        {
            try
            {
                _name_of_another = another;
                var takeShacalKey = await _client.TakeFileAsync(new WhatFile { Filename = $"{_name_of_another}.ShacalKey" });
                if (takeShacalKey.Filename == "")
                    return null;
                else
                {
                    return takeShacalKey.Info.ToByteArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<byte[]> TakeFileAsync(string name, CancellationToken token)
        {
            try
            {
                var takeFile = await _client.TakeFileAsync(new WhatFile { Filename = name });
                if (takeFile.Filename == "" || String.IsNullOrEmpty(name))
                    return null;
                else
                {
                    return takeFile.Info.ToByteArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> SendFile(string filename,byte[] info,CancellationToken token)
        {
            try
            {
                var sending = await _client.SendFileAsync(new FileBuffer { Filename = $"{_myName}.Mess.{filename}", Info = ByteString.CopyFrom(info) });
                if (sending.IsWrittenInServer != true)
                    return -1;
                else return 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       


    }
}
