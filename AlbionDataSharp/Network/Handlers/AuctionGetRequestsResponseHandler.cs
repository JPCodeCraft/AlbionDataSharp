using Albion.Network;
using AlbionData.Models;
using AlbionDataSharp.Network.Responses;
using AlbionDataSharp.State;

namespace AlbionDataSharp.Network.Handlers
{
    public class AuctionGetRequestsResponseHandler : ResponsePacketHandler<AuctionGetRequestsResponse>
    {
        private readonly Uploader uploader;
        private readonly PlayerState playerStatus;
        public AuctionGetRequestsResponseHandler(Uploader uploader, PlayerState playerStatus) : base((int)OperationCodes.AuctionGetRequests)
        {
            this.uploader = uploader;
            this.playerStatus = playerStatus;
        }

        protected override async Task OnActionAsync(AuctionGetRequestsResponse value)
        {
            if (!playerStatus.CheckLocationIDIsSet()) return;

            MarketUpload marketUpload = new MarketUpload();

            value.marketOrders.ForEach(x => x.LocationId = (ushort)playerStatus.Location);
            marketUpload.Orders.AddRange(value.marketOrders);

            if (marketUpload.Orders.Count > 0)
            {
                await uploader.Upload(marketUpload);
            }
            await Task.CompletedTask;
        }
    }
}
