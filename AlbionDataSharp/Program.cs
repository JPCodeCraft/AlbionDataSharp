using AlbionDataSharp.Config;
using AlbionDataSharp.Network;
using AlbionDataSharp.State;
using Autofac;
using Autofac.Extensions.DependencyInjection;
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
            var builder = Host.CreateDefaultBuilder(args)
                //AUTOFAC CONFIG
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterType<PlayerStatus>().SingleInstance();
                })
                //SERVICES CONFIG
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<NetworkListener>();
                    services.AddSerilog(config =>
                    {
                        config.ReadFrom.Configuration(hostContext.Configuration);

                    });
                });

            IHost host = builder.Build();

            ConfigurationHelper.Initialize(host.Services.GetRequiredService<IConfiguration>());

            host.Run();
        }

    }
}

