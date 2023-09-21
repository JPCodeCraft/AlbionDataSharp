using AlbionData.Models;
using AlbionDataSharp.Network;
using AlbionDataSharp.UI;
using Serilog;

namespace AlbionDataSharp.State
{
    internal class PlayerStatus
    {
        private static string locationID;
        private static string playerName;
        private static Server server = Server.Unknown;
        //CacheSize limit size of messages in cache
        private const ulong cacheSize = 8192;

        public static MarketHistoryInfo[] MarketHistoryIDLookup { get; } = new MarketHistoryInfo[CacheSize];
        public static ulong CacheSize => cacheSize;

        public static string LocationID
        {
            get => locationID;
            set
            {
                locationID = value;
                Log.Information("Player location set to {Location}", LocationID);
                ConsoleManager.UpdateLocation((Location)int.Parse(LocationID));
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
                ConsoleManager.UpdateName(PlayerName);
            }
        }
        public static Server Server
        {
            get => server;
            set
            {
                if (server == value) return;
                server = value;
                Log.Information("Server set to {Server}", Server);
                ConsoleManager.UpdateServer(Server);
            }
        }

        public static bool CheckLocationIDIsSet()
        {
            if (locationID == null || !Enum.IsDefined(typeof(Location), int.Parse(LocationID)))
            {
                Log.Warning($"Player location is not set. Please change maps.");
                return false;
            }
            else return true;
        }
    }
}
