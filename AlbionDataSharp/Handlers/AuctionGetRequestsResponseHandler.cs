using Albion.Network;
using AlbionData.Models;
using AlbionDataSharp.Responses;
using System.Text;
using System.Text.Json;

namespace AlbionDataSharp.Handlers
{
    public class AuctionGetRequestsResponseHandler : ResponsePacketHandler<AuctionGetRequestsResponse>
    {
        public AuctionGetRequestsResponseHandler() : base((int)OperationCodes.AuctionGetRequests)
        {
        }

        protected override async Task OnActionAsync(AuctionGetRequestsResponse value)
        {
            NatsManager natsManager = new();
            natsManager.Upload(value.marketUpload);
            await Task.CompletedTask;
        }
    }
}
