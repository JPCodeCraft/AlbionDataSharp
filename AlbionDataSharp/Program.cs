using AlbionDataSharp.Config;
using AlbionDataSharp.Network;
using AlbionDataSharp.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AlbionDataSharp
{
    public class Program
    {

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHostedService<NetworkListener>();
            builder.Services.AddSerilog(config =>
            {
                config.ReadFrom.Configuration(builder.Configuration);
                config.WriteTo.Sink(new DelegatingSink(ConsoleManager.AddStateUpdate), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information);

            });
            IHost host = builder.Build();

            ConfigurationHelper.Initialize(host.Services.GetRequiredService<IConfiguration>());
            ConsoleManager.Initialize();

            host.Run();
        }

        static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Log.Error("GlobalExceptionHandler caught : " + e.Message);
        }

    }
}

