using Albion.Network;
using AlbionDataSharp.Nats;
using AlbionDataSharp.Responses;

namespace AlbionDataSharp.Handlers
{
    public class AuctionGetOffersResponseHandler : ResponsePacketHandler<AuctionGetOffersResponse>
    {
        private readonly INatsManager _natsManager;
        public AuctionGetOffersResponseHandler(INatsManager natsManager) : base((int)OperationCodes.AuctionGetOffers)
        {
            _natsManager = natsManager;
        }

        protected override async Task OnActionAsync(AuctionGetOffersResponse value)
        {
            _natsManager.Upload(value.marketUpload);
            await Task.CompletedTask;
        }
    }
}
