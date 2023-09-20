using AlbionData.Models;
using Microsoft.Extensions.Logging;
using NATS.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Extensions.Configuration;
using System.Net.NetworkInformation;
using System.Reflection;
using AlbionDataSharp.State;

namespace AlbionDataSharp.Network.Nats
{
    internal class NatsManager
    {
        private NatsSettings natsSettings;
        Options opts = ConnectionFactory.GetDefaultOptions();

        public NatsManager()
        {
            natsSettings = ConfigurationHelper.config.GetRequiredSection("Nats").Get<NatsSettings>();
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
                    case Servers.Unknown:
                        Log.Warning("Server has not been set. Can't upload to NATS. Please change maps.");
                        return;
                    case Servers.East:
                        privateServers = natsSettings.PrivateEastServers;
                        break;
                    case Servers.West:
                        privateServers = natsSettings.PrivateWestServers;
                        break;
                };
                foreach (var url in privateServers)
                {
                    opts.Url = url;

                    using (IConnection c = new ConnectionFactory().CreateConnection(opts))
                    {
                        c.Publish(natsSettings.MarketOrdersIngestSubject, data);
                        c.Flush(500);
                    }

                    //logging
                    if (offers > 0 && requests == 0) Log.Information("Published {amount} offers to private NATS [{natsServer}].", offers, url);
                    else if (offers == 0 && requests > 0) Log.Information("Published {amount} requests to private NATS [{natsServer}].", requests, url);
                    else if (offers == 0 && requests == 0) Log.Debug("Published nothing to private NATS [{natsServer}].", url);
                    else Log.Information("Published {amount} offers and {amount} requests to private NATS [{natsServer}].", offers, requests, url);
                }

                //AlbionData servers
                switch (PlayerStatus.Server)
                {
                    case Servers.Unknown:
                        Log.Warning("Server has not been set. Can't upload to NATS. Please change maps.");
                        return;
                    case Servers.East:
                        opts.Url = natsSettings.AlbionDataEastServer;
                        break;
                    case Servers.West:
                        opts.Url = natsSettings.AlbionDataWestServer;
                        break;
                };

                if (!string.IsNullOrEmpty(opts.Url))
                {
                    using (IConnection c = new ConnectionFactory().CreateConnection(opts))
                    {
                        c.Publish(natsSettings.MarketOrdersIngestSubject, data);
                        c.Flush(500);
                    }

                    //logging
                    offers = marketUpload.Orders.Where(x => x.AuctionType == "offer").Count();
                    requests = marketUpload.Orders.Where(x => x.AuctionType == "request").Count();
                    if (offers > 0 && requests == 0) Log.Information("Published {amount} offers to AlbionData NATS [{natsServer}].", offers, opts.Url);
                    else if (offers == 0 && requests > 0) Log.Information("Published {amount} requests to AlbionData NATS [{natsServer}].", requests, opts.Url);
                    else if (offers == 0 && requests == 0) Log.Information("Published nothing to AlbionData NATS [{natsServer}].", opts.Url);
                    else Log.Information("Published {amount} offers and {amount} requests to AlbionData NATS [{natsServer}].", offers, requests, opts.Url);
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
                    case Servers.Unknown:
                        Log.Warning("Server has not been set. Can't upload to NATS. Please change maps.");
                        return;
                    case Servers.East:
                        privateServers = natsSettings.PrivateEastServers;
                        break;
                    case Servers.West:
                        privateServers = natsSettings.PrivateWestServers;
                        break;
                };
                foreach (var url in privateServers)
                {
                    opts.Url = url;

                    using (IConnection c = new ConnectionFactory().CreateConnection(opts))
                    {
                        c.Publish(natsSettings.MarketHistoriesIngestSubject, data);
                        c.Flush(500);
                    }

                    //logging
                    Log.Information("Published {Amount} histories for {ItemID} quality {Quality} in location {Location} timescale {Timescale} to private NATS [{natsServer}].",
                        marketHistoriesUpload.MarketHistories.Count, marketHistoriesUpload.AlbionId, marketHistoriesUpload.QualityLevel,
                        marketHistoriesUpload.LocationId, marketHistoriesUpload.Timescale, url);
                }

                //AlbionData servers
                switch (PlayerStatus.Server)
                {
                    case Servers.Unknown:
                        Log.Warning("Server has not been set. Can't upload to NATS. Please change maps.");
                        return;
                    case Servers.East:
                        opts.Url = natsSettings.AlbionDataEastServer;
                        break;
                    case Servers.West:
                        opts.Url = natsSettings.AlbionDataWestServer;
                        break;
                };

                if (!string.IsNullOrEmpty(opts.Url))
                {
                    using (IConnection c = new ConnectionFactory().CreateConnection(opts))
                    {
                        c.Publish(natsSettings.MarketHistoriesIngestSubject, data);
                        c.Flush(500);
                    }

                    //logging
                    Log.Information("Published {Amount} histories for {ItemID} quality {Quality} in location {Location} timescale {Timescale} to AlbionData NATS [{natsServer}].",
                        marketHistoriesUpload.MarketHistories.Count, marketHistoriesUpload.AlbionId, marketHistoriesUpload.QualityLevel,
                        marketHistoriesUpload.LocationId, marketHistoriesUpload.Timescale, opts.Url);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.ToString());
            }
        }

    }
}
