using Google.Protobuf;
using Grpc.Core;
using GrpcServer;
using System.Text;

namespace GrpcServer.Services
{
    public class CryptographyServerService : CryptographyServer.CryptographyServerBase
    {
        private readonly ILogger<CryptographyServerService> _logger;
        private static readonly List<string> _users = new List<string>();
        private string _defaultPath = "/Tmp/";
        public CryptographyServerService(ILogger<CryptographyServerService> logger)
        {
            _logger = logger;
            string path = Directory.GetCurrentDirectory();
            if (!Directory.Exists(path + _defaultPath))
            {
                Directory.CreateDirectory(path + _defaultPath);
            }
            _defaultPath = path + _defaultPath;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            try
            {
                if (_users.Count > 1)
                {
                    return Task.FromResult(new HelloReply
                    {
                        Message = "More than two"
                    }); ;
                }
                if (!_users.Contains(request.Name))
                {
                    _users.Add(request.Name);
                    return Task.FromResult(new HelloReply
                    {
                        Message = "Hello " + request.Name
                    });
                }
                else
                {
                    return Task.FromResult(new HelloReply
                    {
                        Message = "Wrong name!"
                    });
                }
            }
            catch
            {
                throw new ArgumentException();
            }
        }

        public override Task<HelloReply> SayBye(HelloRequest request, ServerCallContext context)
        {
            try
            {
                _users.Remove(request.Name);
                return Task.FromResult(new HelloReply
                {
                    Message = "Bye " + request.Name
                });
            }
            catch
            {
                throw new ArgumentException();
            }
        }

        public override Task<UserList> WhoAtServer(HelloRequest request, ServerCallContext context)
        {
            StringBuilder all = new StringBuilder("");
            try
            {
                foreach (var user in _users)
                {
                    if (user != request.Name)
                    {
                        all.Append(user);
                        all.Append(",");
                    }
                }
                return Task.FromResult(new UserList
                {
                    Users = all.ToString()
                });
            }
            catch
            {
                throw new ArgumentException();
            }
        }

        public override Task<ClearAll> ClearDir(HelloRequest request, ServerCallContext context)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(_defaultPath);

            try
            {
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete();
                }
                return Task.FromResult(new ClearAll
                {
                    IsClear = true
                });
            }
            catch
            {
                return Task.FromResult(new ClearAll
                {
                    IsClear = false
                });
            }
        }

        public override Task<IsWritten> SendFile(FileBuffer request, ServerCallContext context)
        {
            try
            {
                File.WriteAllBytesAsync(_defaultPath + request.Filename + ".txt", request.Info.ToByteArray());
            }
            catch (Exception ex)
            {
                //Если файл не записался
                return Task.FromResult(new IsWritten
                {
                    IsWrittenInServer = false
                });
            }
            //Если файл успешно записался на серваке
            return Task.FromResult(new IsWritten
            {
                IsWrittenInServer = true
            });
        }

        public override Task<FileBuffer> TakeFile(WhatFile request, ServerCallContext context)
        {
            byte[] bytes_from_file;
            try
            {
                bytes_from_file = File.ReadAllBytes(_defaultPath + request.Filename + ".txt");
            }
            catch
            {
                return Task.FromResult(new FileBuffer
                {
                    Filename = ""
                });
            }
            return Task.FromResult(new FileBuffer
            {
                Filename = request.Filename,
                Info = ByteString.CopyFrom(bytes_from_file)
            });
        }

        public override Task<FileList> TakeAllFileNames(HelloRequest request, ServerCallContext context)
        {
            try
            {
                List<string> allfiles = (from a in Directory.GetFiles(_defaultPath)
                                         select Path.GetFileName(a)).ToList();

                StringBuilder allfilenames_str = new StringBuilder();
                foreach (var file in allfiles)
                {
                    allfilenames_str.Append(Path.GetFileNameWithoutExtension(file)).Append(",");
                }
                return Task.FromResult(new FileList
                {
                    Files = allfilenames_str.ToString()
                });
            }
            catch
            {
                throw new ArgumentException();
            }
        }

    }
}