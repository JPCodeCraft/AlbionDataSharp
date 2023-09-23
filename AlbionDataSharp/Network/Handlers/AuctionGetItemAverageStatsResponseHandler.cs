using Albion.Network;
using AlbionDataSharp.Network.Responses;

namespace AlbionDataSharp.Network.Handlers
{
    public class AuctionGetItemAverageStatsResponseHandler : ResponsePacketHandler<AuctionGetItemAverageStatsResponse>
    {
        public AuctionGetItemAverageStatsResponseHandler() : base((int)OperationCodes.AuctionGetItemAverageStats)
        {
        }

        protected override async Task OnActionAsync(AuctionGetItemAverageStatsResponse value)
        {
            if (value.marketHistoriesUpload.MarketHistories.Count > 0)
            {
                Uploader uploader = new Uploader();
                await uploader.Upload(value.marketHistoriesUpload);
            }
            await Task.CompletedTask;
        }
    }
}
