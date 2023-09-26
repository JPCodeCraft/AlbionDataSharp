using Albion.Network;
using AlbionData.Models;
using AlbionDataSharp.Network.Responses;

namespace AlbionDataSharp.Network.Handlers
{
    public class AuctionGetGoldAverageStatsResponseHandler : ResponsePacketHandler<AuctionGetGoldAverageStatsResponse>
    {
        private readonly Uploader uploader;
        public AuctionGetGoldAverageStatsResponseHandler(Uploader uploader) : base((int)OperationCodes.GoldMarketGetAverageInfo)
        {
            this.uploader = uploader;
        }

        protected override async Task OnActionAsync(AuctionGetGoldAverageStatsResponse value)
        {
            GoldPriceUpload goldHistoriesUpload = new();

            goldHistoriesUpload.Prices = value.prices;
            goldHistoriesUpload.Timestamps = value.timeStamps;

            if (goldHistoriesUpload.Prices.Count() > 0 && goldHistoriesUpload.Prices.Count() == goldHistoriesUpload.Timestamps.Count())
            {
                await uploader.Upload(goldHistoriesUpload);
            }
            await Task.CompletedTask;
        }
    }
}
