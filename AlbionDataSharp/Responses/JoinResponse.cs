using Albion.Network;
using Microsoft.Extensions.Logging;
using AlbionDataSharp.Status;

namespace AlbionDataSharp.Responses
{
    public class JoinResponse : BaseOperation
    {
        private readonly ILogger<JoinResponse> _logger;
        private readonly PlayerStatus _playerStatus;
        public JoinResponse(ILogger<JoinResponse> logger, PlayerStatus playerStatus, Dictionary<byte, object> parameters) : base(parameters)
        {
            _logger = logger;
            _playerStatus = playerStatus;

            _logger.LogDebug($"Got {GetType().ToString()} packet.");
            try
            {
                if (parameters.TryGetValue(2, out object nameData))
                {
                    _playerStatus.PlayerName = (string)nameData;
                }

                if (parameters.TryGetValue(8, out object locationData))
                {
                    string location = (string)locationData;
                    if (location.Contains("-Auction2")) location = location.Replace("-Auction2", "");
                    _playerStatus.LocationID = location;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
