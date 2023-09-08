using Albion.Network;
using System.Text.Json;
using AlbionData.Models;
using AlbionDataSharp.Models;

namespace AlbionDataSharp.Requests
{
    public class AuctionGetItemAverageStatsRequest : BaseOperation
    {

        public AuctionGetItemAverageStatsRequest(Dictionary<byte, object> parameters) : base(parameters)
        {
            Console.WriteLine($"Got {GetType().ToString()} packet.");
            if (!PlayerStatus.CheckLocationIDIsSet()) return;
            MarketHistoryInfo info = new MarketHistoryInfo();
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
                Console.WriteLine(e.Message);
            }

        }
    }
}
