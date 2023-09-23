using Albion.Network;
using AlbionData.Models;
using AlbionDataSharp.Network;
using AlbionDataSharp.State;
using Serilog;

namespace AlbionDataSharp.Requests
{
    public class AuctionGetItemAverageStatsRequest : BaseOperation
    {
        public AuctionGetItemAverageStatsRequest(Dictionary<byte, object> parameters) : base(parameters)
        {
            MarketHistoryInfo info = new MarketHistoryInfo();

            Log.Debug("Got {PacketType} packet.", GetType());

            if (!PlayerStatus.CheckLocationIDIsSet()) return;
            info.LocationID = ((int)PlayerStatus.Location).ToString();

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
                Log.Error(e, e.Message);
            }

        }
    }
}
