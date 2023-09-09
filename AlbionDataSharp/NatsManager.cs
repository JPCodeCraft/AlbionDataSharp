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

namespace AlbionDataSharp
{
    internal class NatsManager
    {
        private static string PrivateNatsUrl { get; set; } = "nats://localhost:4222";
        private static string PublicNatsUrlEast { get; set; } = "nats://public:thenewalbiondata@nats.albion-online-data.com:24222";
        private static string PublicNatsUrlWest { get; set; } = "nats://public:thenewalbiondata@nats.albion-online-data.com:4222";
        private const string marketOrdersIngest = "marketorders.ingest";
        private const string marketHistoriesIngest = "markethistories.ingest";
        private const string goldDataIngest = "goldprices.ingest";

        private readonly Lazy<IConnection> PrivateLazyOutgoingNats = new Lazy<IConnection>(() =>
        {
            var natsFactory = new ConnectionFactory();
            return natsFactory.CreateConnection(PrivateNatsUrl);
        });
        private IConnection PrivateOutgoingNatsConnection
        {
            get
            {
                return PrivateLazyOutgoingNats.Value;
            }
        }
        public void Upload(MarketUpload marketUpload)
        {
            try
            {
                var data = JsonSerializer.SerializeToUtf8Bytes(marketUpload, new JsonSerializerOptions { IncludeFields = true });
                PrivateOutgoingNatsConnection.Publish(marketOrdersIngest, data);
                var offers = marketUpload.Orders.Where(x => x.AuctionType == "offer").Count();
                var requests = marketUpload.Orders.Where(x => x.AuctionType == "request").Count();
                if (offers > 0 && requests == 0) Log.Information("Published {amount} offers to NATS.", offers);
                else if (offers == 0 && requests > 0) Log.Information("Published {amount} requests to NATS.", requests);
                else if (offers == 0 && requests == 0) Log.Information($"Published nothing to NATS.");
                else Log.Information("Published {amount} offers and {amount} requests to NATS.", offers, requests);
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
                PrivateOutgoingNatsConnection.Publish(marketHistoriesIngest, data);
                Log.Information("Published {Amount} histories for {ItemID} quality {Quality} in location {Location} timescale {Timescale}.",
                    marketHistoriesUpload.MarketHistories.Count, marketHistoriesUpload.AlbionId, marketHistoriesUpload.QualityLevel, 
                    marketHistoriesUpload.LocationId, marketHistoriesUpload.Timescale);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.ToString());
            }
        }

    }
}
