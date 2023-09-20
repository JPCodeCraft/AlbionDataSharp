using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AlbionDataSharp.Network;
using AlbionDataSharp.Config;

namespace AlbionDataSharp
{
    public class Program
    {

        private static void Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHostedService<NetworkListener>();
            builder.Services.AddSerilog(config =>
            {
                config.ReadFrom.Configuration(builder.Configuration);

            });
            IHost host = builder.Build();

            ConfigurationHelper.Initialize(host.Services.GetRequiredService<IConfiguration>());
            
            host.Run();
        }

    }
}

