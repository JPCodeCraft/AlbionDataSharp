using AlbionData.Models;
using NATS.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlbionDataSharp
{
    internal static class NatsManager
    {
        private static string PrivateNatsUrl { get; set; } = "nats://localhost:4222";
        private static string PublicNatsUrlEast { get; set; } = "nats://public:thenewalbiondata@nats.albion-online-data.com:24222";
        private static string PublicNatsUrlWest { get; set; } = "nats://public:thenewalbiondata@nats.albion-online-data.com:4222";
        private const string marketOrdersIngest = "marketorders.ingest";
        private const string marketHistoriesIngest = "markethistories.ingest";
        private const string goldDataIngest = "goldprices.ingest";

        private static readonly Lazy<IConnection> PrivateLazyOutgoingNats = new Lazy<IConnection>(() =>
        {
            var natsFactory = new ConnectionFactory();
            return natsFactory.CreateConnection(PrivateNatsUrl);
        });

        private static IConnection PrivateOutgoingNatsConnection
        {
            get
            {
                return PrivateLazyOutgoingNats.Value;
            }
        }

        public static void Upload(MarketUpload marketUpload)
        {
            marketUpload.Orders.RemoveAll(x => !PlayerStatus.CheckLocationIDIsSet());
            try
            {
                var data = JsonSerializer.SerializeToUtf8Bytes(marketUpload, new JsonSerializerOptions { IncludeFields = true });
                PrivateOutgoingNatsConnection.Publish(marketOrdersIngest, data);
                var offers = marketUpload.Orders.Where(x => x.AuctionType == "offer").Count();
                var requests = marketUpload.Orders.Where(x => x.AuctionType == "request").Count();
                var text = "";
                if (offers > 0 && requests == 0) text = $"Published {offers} offers to NATS.";
                else if (offers == 0 && requests > 0) text = $"Published {requests} requests to NATS.";
                else if (offers == 0 && requests == 0) text = $"Published nothing to NATS.";
                else text = $"Published {offers} offers and {requests} requests to NATS.";
                Console.WriteLine(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
