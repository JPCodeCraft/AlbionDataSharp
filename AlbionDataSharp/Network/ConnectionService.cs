using AlbionDataSharp.Config;
using Microsoft.Extensions.Hosting;
using NATS.Client;
using Serilog;

namespace AlbionDataSharp.Network
{
    public class ConnectionService : BackgroundService
    {
        private ConfigurationService configurationService;

        public readonly HttpClient httpClient = new HttpClient();
        public readonly Dictionary<ServerInfo, IConnection> natsConnections = new Dictionary<ServerInfo, IConnection>();

        public ConnectionService(ConfigurationService configurationService)
        {
            this.configurationService = configurationService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var server in configurationService.NetworkSettings.UploadServers)
            {
                if (server.UploadType == UploadType.Nats)
                {
                    try
                    {
                        var options = ConnectionFactory.GetDefaultOptions();
                        options.Url = server.Url;
                        //hacks so nats won't log it's default event to console
                        options.DisconnectedEventHandler = (sender, args) =>
                        {
                            Log.Information("Nats connection of {server} disconnected with error {error}", server.Name, args.Error);
                        };
                        options.ClosedEventHandler = (sender, args) =>
                        {
                            Log.Information("Nats connection of {server} closed with error {error}", server.Name, args.Error);
                        };
                        options.ReconnectedEventHandler = (sender, args) =>
                        {
                            Log.Information("Nats connection of {server} reconnected with error {error}", server.Name, args.Error);
                        };
                        natsConnections[server] = new ConnectionFactory().CreateConnection(options);
                        Log.Information("Connected to {serverType} server {Server}", server.UploadType, server.Name);
                        server.IsReachable = true;
                    }
                    catch (Exception ex)
                    {
                        server.IsReachable = false;
                        Log.Error(ex, "Failed to connect to {serverType} server {Server}", server.UploadType, server.Name);
                    }
                }
                else if (server.UploadType == UploadType.PoW)
                {
                    HttpResponseMessage response = await httpClient.GetAsync(server.Url + "/pow");
                    if (response.IsSuccessStatusCode)
                    {
                        Log.Information("Connected to {serverType} server {Server}", server.UploadType, server.Name);
                        server.IsReachable = true;
                    }
                    else
                    {
                        server.IsReachable = false;
                        Log.Error("Failed to connect to {serverType} server {Server}", server.UploadType, server.Name);
                    }
                }
            }
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            // Close and flush NATS connections here
            foreach (var connection in natsConnections.Values)
            {
                connection.Drain();
                connection.Close();
            }
            httpClient.Dispose();
            Log.Information("Closed all {type} Connections!", nameof(Uploader));
            await base.StopAsync(stoppingToken);
        }

    }
}
