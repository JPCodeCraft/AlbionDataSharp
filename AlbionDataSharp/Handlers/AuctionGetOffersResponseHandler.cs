using Albion.Network;
using AlbionData.Models;
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
            if (value.marketUpload.Orders.Count > 0)
            {
                NatsManager natsManager = new();
                natsManager.Upload(value.marketUpload);
            }
            await Task.CompletedTask;
        }
    }
}
