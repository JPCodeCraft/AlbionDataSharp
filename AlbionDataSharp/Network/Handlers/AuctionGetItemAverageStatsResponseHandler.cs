using Albion.Network;
using AlbionDataSharp.Network.Http;
using AlbionDataSharp.Network.Nats;
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
                NatsManager natsManager = new();
                PowManager powManager = new();
                natsManager.Upload(value.marketHistoriesUpload);
                await powManager.Upload(value.marketHistoriesUpload);
            }
            await Task.CompletedTask;
        }
    }
}
