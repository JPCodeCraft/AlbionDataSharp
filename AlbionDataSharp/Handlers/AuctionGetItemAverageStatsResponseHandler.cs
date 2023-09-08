using Albion.Network;
using AlbionData.Models;
using AlbionDataSharp.Responses;
using System.Text;
using System.Text.Json;

namespace AlbionDataSharp.Handlers
{
    public class AuctionGetItemAverageStatsResponseHandler : ResponsePacketHandler<AuctionGetItemAverageStatsResponse>
    {
        public AuctionGetItemAverageStatsResponseHandler() : base((int)OperationCodes.AuctionGetItemAverageStats)
        {
        }

        protected override async Task OnActionAsync(AuctionGetItemAverageStatsResponse value)
        {
            NatsManager.Upload(value.marketHistoriesUpload);
            await Task.CompletedTask;
        }
    }
}
