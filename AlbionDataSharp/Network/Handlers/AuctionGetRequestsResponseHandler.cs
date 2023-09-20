using Albion.Network;
using AlbionDataSharp.Network.Http;
using AlbionDataSharp.Network.Nats;
using AlbionDataSharp.Network.Responses;

namespace AlbionDataSharp.Network.Handlers
{
    public class AuctionGetRequestsResponseHandler : ResponsePacketHandler<AuctionGetRequestsResponse>
    {
        public AuctionGetRequestsResponseHandler() : base((int)OperationCodes.AuctionGetRequests)
        {
        }

        protected override async Task OnActionAsync(AuctionGetRequestsResponse value)
        {
            if (value.marketUpload.Orders.Count > 0)
            {
                NatsManager natsManager = new();
                PowManager powManager = new();
                natsManager.Upload(value.marketUpload);
                await powManager.Upload(value.marketUpload);
            }
            await Task.CompletedTask;
        }
    }
}
