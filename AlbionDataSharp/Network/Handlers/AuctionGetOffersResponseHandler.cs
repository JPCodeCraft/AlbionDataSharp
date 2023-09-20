using Albion.Network;
using AlbionData.Models;
using AlbionDataSharp.Network.Nats;
using AlbionDataSharp.Network.Responses;
using System.Text;
using System.Text.Json;

namespace AlbionDataSharp.Network.Handlers
{
    public class AuctionGetOffersResponseHandler : ResponsePacketHandler<AuctionGetOffersResponse>
    {
        public AuctionGetOffersResponseHandler() : base((int)OperationCodes.AuctionGetOffers)
        {
        }

        protected override async Task OnActionAsync(AuctionGetOffersResponse value)
        {
            if (value.marketUpload.Orders.Count > 0)
            {
                NatsManager natsManager = new();
                natsManager.Upload(value.marketUpload);
            }
            await Task.CompletedTask;
        }
    }
}
