using Albion.Network;
using System.Text.Json;
using AlbionData.Models;
using AlbionDataSharp.Models;
using Microsoft.Extensions.Logging;

namespace AlbionDataSharp.Requests
{
    public class AuctionGetItemAverageStatsRequest : BaseOperation
    {
        ILogger<AuctionGetItemAverageStatsRequest> logger;
        public AuctionGetItemAverageStatsRequest(Dictionary<byte, object> parameters) : base(parameters)
        {
            logger = Logger.New<AuctionGetItemAverageStatsRequest>();
            MarketHistoryInfo info = new MarketHistoryInfo();

            logger.LogDebug($"Got {GetType().ToString()} packet.");

            if (!PlayerStatus.CheckLocationIDIsSet()) return;
            info.LocationID = PlayerStatus.LocationID;

            try
            {
                if (parameters.TryGetValue(1, out object itemID))
                {
                    info.AlbionId = Convert.ToUInt32(itemID);
                }
                if (parameters.TryGetValue(2, out object quality))
                {
                    info.Quality = Convert.ToUInt16(quality);
                }
                if (parameters.TryGetValue(3, out object timescale))
                {
                    info.Timescale = (Timescale)Convert.ToInt32(timescale);
                }
                if (parameters.TryGetValue(255, out object messageID))
                {
                    PlayerStatus.MarketHistoryIDLookup[Convert.ToUInt32(messageID) % PlayerStatus.CacheSize] = info;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }

        }
    }
}
