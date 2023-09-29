using AlbionDataSharp.Config;
using AlbionDataSharp.Network;
using AlbionDataSharp.Network.Pow;
using AlbionDataSharp.State;
using AlbionDataSharp.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Squirrel;

namespace AlbionDataSharp
{
    public class Program
    {

        private static async Task Main(string[] args)
        {
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

            // run Squirrel first, as the app may exit after these run
            SquirrelAwareApp.HandleEvents(
                onInitialInstall: OnAppInstall,
                onAppUninstall: OnAppUninstall,
                onEveryRun: OnAppRun);

            await UpdateMyApp();

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddSingleton<ConsoleManager>();
            builder.Services.AddHostedService(x => x.GetRequiredService<ConsoleManager>()); //this makes sure we are using the same instance of ConsoleManager
            builder.Services.AddHostedService<NetworkListener>();
            builder.Services.AddSingleton<ConnectionService>();
            builder.Services.AddHostedService(x => x.GetRequiredService<ConnectionService>());
            builder.Services.AddSingleton<ConfigurationService>();
            builder.Services.AddSingleton<Uploader>();
            builder.Services.AddSingleton<PlayerState>();
            builder.Services.AddTransient<PowSolver>();
            builder.Services.AddSerilog();
            IHost host = builder.Build();

            var consoleManager = host.Services.GetRequiredService<ConsoleManager>();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .WriteTo.Sink(new DelegatingSink(consoleManager.AddStateUpdate), restrictedToMinimumLevel: host.Services.GetRequiredService<ConfigurationService>().UiSettings.ConsoleLogLevel)
                .CreateLogger();

            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            AppDomain.CurrentDomain.ProcessExit += async (s, e) =>
            {
                lifetime.StopApplication();
                await host.StopAsync();
            };

            host.Run();
        }

        static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Log.Error("GlobalExceptionHandler caught : " + e.Message);
        }
        private static void OnAppInstall(SemanticVersion version, IAppTools tools)
        {
            tools.CreateShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
        }

        private static void OnAppUninstall(SemanticVersion version, IAppTools tools)
        {
            tools.RemoveShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
        }

        private static void OnAppRun(SemanticVersion version, IAppTools tools, bool firstRun)
        {
            tools.SetProcessAppUserModelId();
        }
        private static async Task UpdateMyApp()
        {
            try
            {
                Console.WriteLine("Checking for updates... Please wait, this won't take longer than a minute.");
                await Task.Delay(TimeSpan.FromSeconds(0.5));
                ReleaseEntry newVersion;

                using (var mgr = new GithubUpdateManager("https://github.com/augusto501/AlbionDataSharp"))
                {
                    newVersion = await mgr.UpdateApp();
                    Console.WriteLine("Done checking for updates...");
                    await Task.Delay(TimeSpan.FromSeconds(0.5));
                }

                // optionally restart the app automatically, or ask the user if/when they want to restart
                if (newVersion != null)
                {
                    Console.WriteLine("Found a new version! Restarting the client...");
                    await Task.Delay(TimeSpan.FromSeconds(0.5));
                    UpdateManager.RestartApp();
                }
                else
                {
                    Console.WriteLine("Your client is the latest version! Enjoy your silver!");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
            catch
            {
            }
        }

    }
}

