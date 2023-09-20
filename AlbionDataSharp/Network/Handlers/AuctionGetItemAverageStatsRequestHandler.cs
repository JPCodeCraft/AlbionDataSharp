using Albion.Network;
using AlbionDataSharp.Requests;

namespace AlbionDataSharp.Network.Handlers
{
    public class AuctionGetItemAverageStatsRequestHandler : RequestPacketHandler<AuctionGetItemAverageStatsRequest>
    {
        public AuctionGetItemAverageStatsRequestHandler() : base((int)OperationCodes.AuctionGetItemAverageStats)
        {
        }

        protected override async Task OnActionAsync(AuctionGetItemAverageStatsRequest value)
        {
            await Task.CompletedTask;
        }
    }
}
