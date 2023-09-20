using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.State;
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

                string[] privateServers = Array.Empty<string>();

                //private servers
                switch (PlayerStatus.Server)
                {
                    case Server.Unknown:
                        Log.Warning("Server has not been set. Can't upload to NATS. Please change maps.");
                        return;
                    case Server.East:
                        privateServers = ConfigurationHelper.natsSettings.PrivateEastServers;
                        break;
                    case Server.West:
                        privateServers = ConfigurationHelper.natsSettings.PrivateWestServers;
                        break;
                };
                foreach (var url in privateServers)
                {
                    opts.Url = url;

                    using (IConnection c = new ConnectionFactory().CreateConnection(opts))
                    {
                        c.Publish(ConfigurationHelper.natsSettings.MarketOrdersIngestSubject, data);
                        c.Flush(1000);
                    }

                    //logging
                    if (offers > 0 && requests == 0) Log.Information("Published {amount} offers to private NATS [{natsServer}].", offers, url);
                    else if (offers == 0 && requests > 0) Log.Information("Published {amount} requests to private NATS [{natsServer}].", requests, url);
                    else if (offers == 0 && requests == 0) Log.Debug("Published nothing to private NATS [{natsServer}].", url);
                    else Log.Information("Published {amount} offers and {amount} requests to private NATS [{natsServer}].", offers, requests, url);
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

                string[] privateServers = Array.Empty<string>();

                //private servers
                switch (PlayerStatus.Server)
                {
                    case Server.Unknown:
                        Log.Warning("Server has not been set. Can't upload to NATS. Please change maps.");
                        return;
                    case Server.East:
                        privateServers = ConfigurationHelper.natsSettings.PrivateEastServers;
                        break;
                    case Server.West:
                        privateServers = ConfigurationHelper.natsSettings.PrivateWestServers;
                        break;
                };
                foreach (var url in privateServers)
                {
                    opts.Url = url;

                    using (IConnection c = new ConnectionFactory().CreateConnection(opts))
                    {
                        c.Publish(ConfigurationHelper.natsSettings.MarketHistoriesIngestSubject, data);
                        c.Flush(1000);
                    }

                    //logging
                    Log.Information("Published {Amount} histories for {ItemID} quality {Quality} in location {Location} timescale {Timescale} to private NATS [{natsServer}].",
                        marketHistoriesUpload.MarketHistories.Count, marketHistoriesUpload.AlbionId, marketHistoriesUpload.QualityLevel,
                        marketHistoriesUpload.LocationId, marketHistoriesUpload.Timescale, url);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.ToString());
            }
        }

    }
}
