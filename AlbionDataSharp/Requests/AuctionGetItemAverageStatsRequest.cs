using Albion.Network;
using System.Text.Json;
using AlbionData.Models;
using AlbionDataSharp.Models;
using Microsoft.Extensions.Logging;
using AlbionDataSharp.Status;

namespace AlbionDataSharp.Requests
{
    public class AuctionGetItemAverageStatsRequest : BaseOperation
    {
        private readonly ILogger<AuctionGetItemAverageStatsRequest> _logger;
        private readonly IPlayerStatus _playerStatus;

        public AuctionGetItemAverageStatsRequest(ILogger<AuctionGetItemAverageStatsRequest> logger, IPlayerStatus playerStatus, Dictionary<byte, object> parameters) : base(parameters)
        {
            _logger = logger;
            _playerStatus = playerStatus;

            _logger.LogDebug($"Got {GetType().ToString()} packet.");
            if (!_playerStatus.CheckLocationIDIsSet()) return;
            MarketHistoryInfo info = new MarketHistoryInfo();
            info.LocationID = _playerStatus.LocationID;
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
                    _playerStatus.MarketHistoryIDLookup[Convert.ToUInt32(messageID) % _playerStatus.CacheSize] = info;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
