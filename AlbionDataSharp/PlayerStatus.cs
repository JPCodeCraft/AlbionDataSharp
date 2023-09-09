using AlbionData.Models;
using AlbionDataSharp.Models;
using Microsoft.Extensions.Logging;

namespace AlbionDataSharp
{
    internal class PlayerStatus
    {
        private static string locationID;
        private static string playerName;
        private static Servers server = Servers.Unknown;
        //CacheSize limit size of messages in cache
        private const ulong cacheSize = 8192;

        private static ILogger<PlayerStatus> logger;

        public static MarketHistoryInfo[] MarketHistoryIDLookup { get; } = new MarketHistoryInfo[CacheSize];
        public static ulong CacheSize => cacheSize;

        public PlayerStatus()
        {
            logger = Logger.New<PlayerStatus>();
        }
        public static string LocationID 
        { 
            get => locationID; 
            set 
            { 
                locationID = value;
                logger.LogInformation($"Player location set to {LocationID}");
            } }
        public static string PlayerName
        {
            get => playerName;
            set
            {
                if (playerName == value) return;
                playerName = value;
                logger.LogInformation($"Player name set to {PlayerName}");
            }
        }
        public static Servers Server
        {
            get => server;
            set
            {
                if (server == value) return;
                server = value;
                logger.LogInformation($"Server set to {Server}");
            }
        }

        public static bool CheckLocationIDIsSet()
        {
            if (locationID == null || !Enum.IsDefined(typeof(Location), int.Parse(LocationID)))
            {
                logger.LogCritical($"Player location is not set. Please change maps.");
                return false;
            }
            else return true;
        }
    }
}
