using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.State;
using AlbionDataSharp.UI;
using NATS.Client;
using Serilog;
using System.Text.Json;

namespace AlbionDataSharp.Network.Nats
{
    internal class NatsManager
    {
        Options opts = ConnectionFactory.GetDefaultOptions();

        public NatsManager()
        {
            //hacks so nats won't log it's default event to console
            opts.DisconnectedEventHandler = (sender, args) => { };
            opts.ClosedEventHandler = (sender, args) => { };
        }

        public void Upload(MarketUpload marketUpload)
        {
            var offers = marketUpload.Orders.Where(x => x.AuctionType == "offer").Count();
            var requests = marketUpload.Orders.Where(x => x.AuctionType == "request").Count();

            try
            {
                var data = JsonSerializer.SerializeToUtf8Bytes(marketUpload, new JsonSerializerOptions { IncludeFields = true });

                ServerInfo[] privateServers = Array.Empty<ServerInfo>();

                //private servers
                switch (PlayerStatus.Server)
                {
                    case Server.Unknown:
                        Log.Warning("Server has not been set. Can't upload to NATS. Please change maps.");
                        return;
                    case Server.East:
                        privateServers = ConfigurationHelper.networkSettings.PrivateEastServers;
                        break;
                    case Server.West:
                        privateServers = ConfigurationHelper.networkSettings.PrivateWestServers;
                        break;
                };
                foreach (var serverInfo in privateServers)
                {
                    opts.Url = serverInfo.Url;

                    using (IConnection c = new ConnectionFactory().CreateConnection(opts))
                    {
                        c.Publish(ConfigurationHelper.networkSettings.MarketOrdersIngestSubject, data);
                        c.Flush(10000);
                    }

                    ConsoleManager.IncrementOffersSent(serverInfo.Name, offers);
                    ConsoleManager.IncrementRequestsSent(serverInfo.Name, requests);

                    //logging
                    if (offers > 0 && requests == 0) Log.Information("Published {amount} offers to {server}.", offers, serverInfo.Name);
                    else if (offers == 0 && requests > 0) Log.Information("Published {amount} requests to {server}.", requests, serverInfo.Name);
                    else if (offers == 0 && requests == 0) Log.Debug("Published nothing to {server}.", serverInfo.Name);
                    else Log.Information("Published {amount} offers and {amount} requests to {server}.", offers, requests, serverInfo.Name);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.ToString());
            }
        }
        public void Upload(MarketHistoriesUpload marketHistoriesUpload)
        {
            try
            {
                var data = JsonSerializer.SerializeToUtf8Bytes(marketHistoriesUpload, new JsonSerializerOptions { IncludeFields = true });

                ServerInfo[] privateServers = Array.Empty<ServerInfo>();

                //private servers
                switch (PlayerStatus.Server)
                {
                    case Server.Unknown:
                        Log.Warning("Server has not been set. Can't upload to NATS. Please change maps.");
                        return;
                    case Server.East:
                        privateServers = ConfigurationHelper.networkSettings.PrivateEastServers;
                        break;
                    case Server.West:
                        privateServers = ConfigurationHelper.networkSettings.PrivateWestServers;
                        break;
                };
                foreach (var serverInfo in privateServers)
                {
                    opts.Url = serverInfo.Url;

                    using (IConnection c = new ConnectionFactory().CreateConnection(opts))
                    {
                        c.Publish(ConfigurationHelper.networkSettings.MarketHistoriesIngestSubject, data);
                        c.Flush(10000);
                    }
                    ConsoleManager.IncrementHistoriesSent(serverInfo.Name, marketHistoriesUpload.MarketHistories.Count, marketHistoriesUpload.Timescale);

                    //logging
                    Log.Information("Published {Amount} histories in timescale {Timescale} to {server}.",
                        marketHistoriesUpload.MarketHistories.Count, marketHistoriesUpload.Timescale, serverInfo.Name);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.ToString());
            }
        }

    }
}
