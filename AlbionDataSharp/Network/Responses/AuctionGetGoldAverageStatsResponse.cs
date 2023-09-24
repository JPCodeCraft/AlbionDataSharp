using Albion.Network;
using AlbionData.Models;
using Serilog;
using System.Collections;

namespace AlbionDataSharp.Network.Responses
{
    public class AuctionGetGoldAverageStatsResponse : BaseOperation
    {
        public readonly GoldPriceUpload goldHistoriesUpload = new();

        public AuctionGetGoldAverageStatsResponse(Dictionary<byte, object> parameters) : base(parameters)
        {

            uint[] prices = Array.Empty<uint>();
            long[] timeStamps = Array.Empty<long>();

            Log.Debug("Got {PacketType} packet.", GetType());

            try
            {
                //reads the packet
                if (parameters.TryGetValue(0, out object _prices))
                {
                    prices = ((IEnumerable)_prices).Cast<object>().Select(x => Convert.ToUInt32(x)).ToArray();
                }
                if (parameters.TryGetValue(1, out object _timeStamps))
                {
                    timeStamps = ((IEnumerable)_timeStamps).Cast<object>().Select(x => Convert.ToInt64(x)).ToArray();
                }
                //fill the upload
                goldHistoriesUpload.Prices = prices;
                goldHistoriesUpload.Timestamps = timeStamps;
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }
    }
}
