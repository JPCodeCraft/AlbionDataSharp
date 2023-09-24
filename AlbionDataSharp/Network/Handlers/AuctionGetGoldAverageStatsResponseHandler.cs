using Albion.Network;
using AlbionDataSharp.Network.Responses;

namespace AlbionDataSharp.Network.Handlers
{
    public class AuctionGetGoldAverageStatsResponseHandler : ResponsePacketHandler<AuctionGetGoldAverageStatsResponse>
    {
        public AuctionGetGoldAverageStatsResponseHandler() : base((int)OperationCodes.GoldMarketGetAverageInfo)
        {
        }

        protected override async Task OnActionAsync(AuctionGetGoldAverageStatsResponse value)
        {
            if (value.goldHistoriesUpload.Prices.Count() > 0)
            {
                Uploader uploader = new Uploader();
                await uploader.Upload(value.goldHistoriesUpload);
            }
            await Task.CompletedTask;
        }
    }
}
