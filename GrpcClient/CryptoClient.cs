using System.Net.Http;
using Crypto.Services;
using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace GrpcClient;

public class CryptoClient
{
    private readonly ILogger<CryptoClient> _logger;
    private readonly Crypto.Services.Chat.ChatClient _client;
    private readonly GrpcChannel _channel;

    public CryptoClient(ILogger<CryptoClient> logger = null)
    {
        _logger = logger;

        var httpHandler = new HttpClientHandler();

        httpHandler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        _channel =
            GrpcChannel
                .ForAddress("https://localhost:7173", new GrpcChannelOptions { HttpHandler = httpHandler });

        _client = new Chat.ChatClient(_channel);
    }
    
    // Таких чуваков у Тебя в .proto нету чуть-чуть о_ да все норм меня валера учит)
    // просто до этого он кидал как образецО понел ок
    // давайййй йебашь!спасибор большое!!!!!! завтра уже)к вечеру же сдавать:) окъ, пингуй эсичо
    public StartSessionReply Auth(string Username, string Password)
    {
        var request = new StartSessionRequest
        {
            Username = Username,
            Password = ByteString.CopyFromUtf8(Password)
        };
        return client.Auth(request);
    }
    
    
}