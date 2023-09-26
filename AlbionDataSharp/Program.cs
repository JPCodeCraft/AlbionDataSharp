using AlbionDataSharp.Config;
using AlbionDataSharp.Network;
using AlbionDataSharp.State;
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
            builder.Services.AddSingleton<ConsoleManager>();
            builder.Services.AddHostedService(x => x.GetRequiredService<ConsoleManager>());
            builder.Services.AddSingleton<Uploader>();
            builder.Services.AddSingleton<PlayerStatus>();
            builder.Services.AddSerilog();
            IHost host = builder.Build();

            ConfigurationHelper.Initialize(host.Services.GetRequiredService<IConfiguration>());

            var consoleManager = host.Services.GetRequiredService<ConsoleManager>();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .WriteTo.Sink(new DelegatingSink(consoleManager.AddStateUpdate), restrictedToMinimumLevel: ConfigurationHelper.uiSettings.ConsoleLogLevel)
                .CreateLogger();

            host.Run();
        }

        static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Log.Error("GlobalExceptionHandler caught : " + e.Message);
        }

    }
}

