using Albion.Network;
using AlbionDataSharp.Network.Requests;
using AlbionDataSharp.State;

namespace AlbionDataSharp.Network.Handlers
{
    public class AuctionGetItemAverageStatsRequestHandler : RequestPacketHandler<AuctionGetItemAverageStatsRequest>
    {
        PlayerState playerStatus;
        public AuctionGetItemAverageStatsRequestHandler(PlayerState playerStatus) : base((int)OperationCodes.AuctionGetItemAverageStats)
        {
            this.playerStatus = playerStatus;
        }

        protected override async Task OnActionAsync(AuctionGetItemAverageStatsRequest value)
        {
            if (!playerStatus.CheckLocationIDIsSet()) return;

            MarketHistoryInfo info = new MarketHistoryInfo();
            playerStatus.MarketHistoryIDLookup[value.messageID % playerStatus.CacheSize] = info;

            info.Quality = value.quality;
            info.Timescale = value.timescale;
            info.AlbionId = value.albionId;
            info.LocationID = ((int)playerStatus.Location).ToString();

            await Task.CompletedTask;
        }
    }
}
