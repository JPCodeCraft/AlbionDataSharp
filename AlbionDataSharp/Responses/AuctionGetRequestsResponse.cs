using Albion.Network;
using System.Text.Json;
using AlbionData.Models;
using Microsoft.Extensions.Logging;
using AlbionDataSharp.Status;

namespace AlbionDataSharp.Responses
{
    public class AuctionGetRequestsResponse : BaseOperation
    {
        private readonly ILogger<AuctionGetRequestsResponse> _logger;
        private readonly IPlayerStatus _playerStatus;
        public readonly MarketUpload marketUpload = new();

        public AuctionGetRequestsResponse(ILogger<AuctionGetRequestsResponse> logger, Dictionary<byte, object> parameters) : base(parameters)
        {
            _logger = logger;

            _logger.LogDebug($"Got {GetType().ToString()} packet.");
            if (!_playerStatus.CheckLocationIDIsSet())
            {
                return;
            }
            try
            {
                if (parameters.TryGetValue(0, out object orders))
                {
                    foreach (var auctionOfferString in (IEnumerable<string>)orders ?? new List<string>())
                    {
                        var order = JsonSerializer.Deserialize<MarketOrder>(auctionOfferString);
                        order.LocationId = ushort.Parse(_playerStatus.LocationID);
                        marketUpload.Orders.Add(order);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
