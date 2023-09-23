using Albion.Network;
using AlbionDataSharp.Network.Responses;

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
                Uploader uploader = new Uploader();
                await uploader.Upload(value.marketUpload);
            }
            await Task.CompletedTask;
        }
    }
}
