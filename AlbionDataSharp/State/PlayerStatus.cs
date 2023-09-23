using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.Network;
using AlbionDataSharp.UI;
using Serilog;

namespace AlbionDataSharp.State
{
    internal class PlayerStatus
    {
        private static Location location = 0;
        private static string playerName = string.Empty;
        private static AlbionServer albionServer = AlbionServer.Unknown;
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
                ConsoleManager.SetPlayerLocation(Location);
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
                ConsoleManager.SetPlayerName(PlayerName);
            }
        }
        public static AlbionServer AlbionServer
        {
            get => albionServer;
            set
            {
                if (albionServer == value) return;
                albionServer = value;
                Log.Information("Server set to {Server}", AlbionServer);
                ConsoleManager.SetAlbionServer(AlbionServer);
            }
        }

        public static bool CheckLocationIDIsSet()
        {
            if (location == 0 || !Enum.IsDefined(typeof(Location), Location))
            {
                Log.Warning($"Player location is not set. Please change maps.");
                return false;
            }
            else return true;
        }
    }
}
