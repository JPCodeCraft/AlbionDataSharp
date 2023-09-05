using Albion.Network;
using AlbionDataSharp.Responses;
using System.Text;
using System.Text.Json;

namespace AlbionDataSharp.Handlers
{
    public class AuctionGetOffersResponseHandler : ResponsePacketHandler<AuctionGetOffersResponse>
    {
        public AuctionGetOffersResponseHandler() : base((int)OperationCodes.AuctionGetOffers)
        {
        }

        protected override async Task OnActionAsync(AuctionGetOffersResponse value)
        {
            //foreach (var a in value.AuctionEntries)
            {
                var a = (JsonSerializer.Serialize(value.AuctionEntries));
                NatsManager.OutgoingNatsConnection.Publish(NatsManager.marketOrdersIngest, Encoding.UTF8.GetBytes(a));
                Console.WriteLine($"Published {value.AuctionEntries.Count} to NATS.");
            }
            await Task.CompletedTask;
        }
    }
}
