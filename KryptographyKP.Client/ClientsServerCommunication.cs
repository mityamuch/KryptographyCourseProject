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
                throw new NotImplementedException();
                
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
                return 3;
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
                return new byte[2];
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
                return new byte[2];
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
                return new byte[2];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
