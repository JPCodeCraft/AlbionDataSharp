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
            try
            {
                var data = JsonSerializer.SerializeToUtf8Bytes(marketUpload, new JsonSerializerOptions { IncludeFields = true });
                PrivateOutgoingNatsConnection.Publish(marketOrdersIngest, data);
                Console.WriteLine($"Published {marketUpload.Orders.Count} market offers to NATS.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
