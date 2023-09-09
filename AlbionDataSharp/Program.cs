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

namespace AlbionDataSharp
{
    public class Program
    {

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<NetworkListener>();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger, true);

            IHost host = builder.Build();
            host.Run();
        }

    }
}

