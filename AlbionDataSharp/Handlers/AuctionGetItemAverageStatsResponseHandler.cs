using Albion.Network;
using AlbionDataSharp.Nats;
using AlbionDataSharp.Responses;

namespace AlbionDataSharp.Handlers
{
    public class AuctionGetItemAverageStatsResponseHandler : ResponsePacketHandler<AuctionGetItemAverageStatsResponse>
    {
        private readonly INatsManager _natsManager;
        public AuctionGetItemAverageStatsResponseHandler(INatsManager natsManager) : base((int)OperationCodes.AuctionGetItemAverageStats)
        {
            _natsManager = natsManager;
        }

        protected override async Task OnActionAsync(AuctionGetItemAverageStatsResponse value)
        {
            _natsManager.Upload(value.marketHistoriesUpload);
            await Task.CompletedTask;
        }
    }
}
