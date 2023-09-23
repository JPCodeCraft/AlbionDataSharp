using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.Network;
using Serilog;

namespace AlbionDataSharp.State
{
    internal class PlayerStatus
    {
        private static Location location;
        private static string playerName;
        private static AlbionServer server = AlbionServer.Unknown;
        //CacheSize limit size of messages in cache
        private const ulong cacheSize = 8192;

        public static MarketHistoryInfo[] MarketHistoryIDLookup { get; } = new MarketHistoryInfo[CacheSize];
        public static ulong CacheSize => cacheSize;

        public static Location Location
        {
            get => location;
            set
            {
                location = value;
                Log.Information("Player location set to {Location}", Location.ToString());
            }
        }
        public static string PlayerName
        {
            get => playerName;
            set
            {
                if (playerName == value) return;
                playerName = value;
                Log.Information("Player name set to {PlayerName}", PlayerName);
            }
        }
        public static AlbionServer Server
        {
            get => server;
            set
            {
                if (server == value) return;
                server = value;
                Log.Information("Server set to {Server}", Server);
            }
        }

        public static bool CheckLocationIDIsSet()
        {
            if (location == null || !Enum.IsDefined(typeof(Location), Location))
            {
                Log.Warning($"Player location is not set. Please change maps.");
                return false;
            }
            else return true;
        }
    }
}
