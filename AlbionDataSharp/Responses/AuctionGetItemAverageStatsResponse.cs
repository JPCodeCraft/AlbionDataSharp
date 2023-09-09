using Albion.Network;
using System.Text.Json;
using AlbionData.Models;
using AlbionDataSharp.Models;
using System.Collections;
using Microsoft.Extensions.Logging;

namespace AlbionDataSharp.Responses
{
    public class AuctionGetItemAverageStatsResponse : BaseOperation
    {
        public readonly MarketHistoriesUpload marketHistoriesUpload = new();
        ILogger<AuctionGetItemAverageStatsResponse> logger;

        public AuctionGetItemAverageStatsResponse(Dictionary<byte, object> parameters) : base(parameters)
        {
            logger = Logger.New<AuctionGetItemAverageStatsResponse>();

            long[] itemAmounts = Array.Empty<long>();
            ulong[] silverAmounts = Array.Empty<ulong>(); ;
            ulong[] timeStamps = Array.Empty<ulong>(); ;
            ulong messageID = 0;
            MarketHistoryInfo info;

            logger.LogDebug($"Got {GetType().ToString()} packet.");
            if (!PlayerStatus.CheckLocationIDIsSet()) return;

            try
            {
                //reads the packet
                if (parameters.TryGetValue(0, out object amounts))
                {
                    itemAmounts = ((IEnumerable)amounts).Cast<object>().Select(x => Convert.ToInt64(x)).ToArray();
                }
                if (parameters.TryGetValue(1, out object silver))
                {
                    silverAmounts = ((IEnumerable)silver).Cast<object>().Select(x => Convert.ToUInt64(x)).ToArray();
                }
                if (parameters.TryGetValue(2, out object stamps))
                {
                    timeStamps = ((IEnumerable)stamps).Cast<object>().Select(x => Convert.ToUInt64(x)).ToArray();
                }
                if (parameters.TryGetValue(255, out object id))
                {
                    messageID = Convert.ToUInt64(id);
                }
                //load info from history
                info = PlayerStatus.MarketHistoryIDLookup[messageID % PlayerStatus.CacheSize];
                //loops entries to fix amounts
                for (int i = 0; i < itemAmounts.Length; i++)
                {
                    //sometimes opAuctionGetItemAverageStats receives negative item amounts
                    if (itemAmounts[i] < 0)
                    {
                        if (itemAmounts[i] < 124)
                        {
                            // still don't know what to do with these
                            logger.LogCritical($"Market History - Ignoring negative item amount {itemAmounts[i]} for {silverAmounts[i]} silver on {timeStamps[i]}");
                        }
                        // however these can be interpreted by adding them to 256
                        // TODO: make more sense of this, (perhaps there is a better way)
                        logger.LogCritical($"Market History - Interpreting negative item amount {itemAmounts[i]} as {itemAmounts[i]+256} for {silverAmounts[i]} silver on {timeStamps[i]}");
                        itemAmounts[i] = 256 + itemAmounts[i];
                    }
                    marketHistoriesUpload.MarketHistories.Add(new MarketHistory
                    {
                        ItemAmount = (ulong)itemAmounts[i],
                        SilverAmount = silverAmounts[i],
                        Timestamp = timeStamps[i]
                    });
                }
                //fill the upload
                marketHistoriesUpload.AlbionId = info.AlbionId;
                marketHistoriesUpload.LocationId = ushort.Parse(info.LocationID);
                marketHistoriesUpload.QualityLevel = (byte)info.Quality;
                marketHistoriesUpload.Timescale = info.Timescale;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
        }
    }
}
