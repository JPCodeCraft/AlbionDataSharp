using Albion.Network;
using AlbionData.Models;
using AlbionDataSharp.Requests;
using AlbionDataSharp.Responses;
using System.Text;
using System.Text.Json;

namespace AlbionDataSharp.Handlers
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
