using AlbionData.Models;
using AlbionDataSharp.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbionDataSharp.Status
{
    public class PlayerStatus : IPlayerStatus
    {
        private readonly ILogger<PlayerStatus> _logger;
        public PlayerStatus(ILogger<PlayerStatus> logger) 
        {
            _logger = logger;
        }
        private string locationID;
        private string playerName;
        private Servers server = Servers.Unknown;
        //CacheSize limit size of messages in cache
        private const ulong cacheSize = 8192;
        public MarketHistoryInfo[] MarketHistoryIDLookup { get; } = new MarketHistoryInfo[cacheSize];
        public ulong CacheSize => cacheSize;

        public string LocationID
        {
            get => locationID;
            set
            {
                locationID = value;
                _logger.LogInformation($"Player location set to {LocationID}");
            }
        }
        public string PlayerName
        {
            get => playerName;
            set
            {
                if (playerName == value) return;
                playerName = value;
                _logger.LogInformation($"Player name set to {PlayerName}");
            }
        }
        public Servers Server
        {
            get => server;
            set
            {
                if (server == value) return;
                server = value;
                _logger.LogInformation($"Server set to {Server}");
            }
        }

        public bool CheckLocationIDIsSet()
        {
            if (locationID == null || !Enum.IsDefined(typeof(Location), int.Parse(LocationID)))
            {
                _logger.LogCritical($"Player location is not set. Please change maps.");
                return false;
            }
            else return true;
        }
    }
}
