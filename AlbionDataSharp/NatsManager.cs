using NATS.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbionDataSharp
{
    internal static class NatsManager
    {
        private static string NatsUrl { get; set; } = "nats://localhost:4222";
        public const string marketOrdersIngest = "marketorders.ingest";
        public const string marketHistoriesIngest = "markethistories.ingest";
        public const string goldDataIngest = "goldprices.ingest";

        private static readonly Lazy<IConnection> lazyOutgoingNats = new Lazy<IConnection>(() =>
        {
            var natsFactory = new ConnectionFactory();
            return natsFactory.CreateConnection(NatsUrl);
        });

        public static IConnection OutgoingNatsConnection
        {
            get
            {
                return lazyOutgoingNats.Value;
            }
        }


    }
}
