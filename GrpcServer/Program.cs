using GrpcServer.Services;
using System.Net;
var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.WebHost.ConfigureKestrel((webHostBuilderContext, kestrelServerOptions) =>
{
    // TODO: этих челов надо доставать из конфига, конечно же
    kestrelServerOptions.Listen(IPAddress.Parse("127.0.0.1"), 5055, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        
        // TODO: Тебе наверное пока не нужен хттпс, если хошь потом читани
        // listenOptions.UseHttps();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CryptographyServerService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
