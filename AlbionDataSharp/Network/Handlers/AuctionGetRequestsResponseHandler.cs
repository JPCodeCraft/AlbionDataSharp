using Albion.Network;
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
                Uploader uploader = new Uploader();
                await uploader.Upload(value.marketUpload);
            }
            await Task.CompletedTask;
        }
    }
}
