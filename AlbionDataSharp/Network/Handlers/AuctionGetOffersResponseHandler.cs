using Albion.Network;
using AlbionData.Models;
using AlbionDataSharp.Network.Responses;
using AlbionDataSharp.State;

namespace AlbionDataSharp.Network.Handlers
{
    public class AuctionGetOffersResponseHandler : ResponsePacketHandler<AuctionGetOffersResponse>
    {
        private readonly Uploader uploader;
        private readonly PlayerStatus playerStatus;
        public AuctionGetOffersResponseHandler(Uploader uploader, PlayerStatus playerStatus) : base((int)OperationCodes.AuctionGetOffers)
        {
            this.uploader = uploader;
            this.playerStatus = playerStatus;
        }

        protected override async Task OnActionAsync(AuctionGetOffersResponse value)
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
