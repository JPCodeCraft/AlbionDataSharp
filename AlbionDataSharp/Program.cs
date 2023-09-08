using AlbionDataSharp.Handlers;
using AlbionDataSharp.Nats;
using AlbionDataSharp.Status;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AlbionDataSharp
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            builder.Logging.SetMinimumLevel(LogLevel.Trace);
            builder.Services.AddHostedService<ListenerService>();
            builder.Services.AddSingleton<INatsManager, NatsManager>();
            builder.Services.AddSingleton<IPlayerStatus, PlayerStatus>();
            IHost host = builder.Build();
            host.Run();
        }
    }
}

