using Albion.Network;
using AlbionDataSharp.Handlers;
using AlbionDataSharp.Requests;
using Microsoft.Extensions.Logging;
using PacketDotNet;
using SharpPcap;
using Serilog;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

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

