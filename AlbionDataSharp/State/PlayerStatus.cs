using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.Network;
using AlbionDataSharp.UI;
using Serilog;

namespace AlbionDataSharp.State
{
    public class PlayerStatus
    {
        private Location location = 0;
        private string playerName = string.Empty;
        private AlbionServer albionServer = AlbionServer.Unknown;

        public MarketHistoryInfo[] MarketHistoryIDLookup { get; init; }
        public ulong CacheSize => 8192;

        public PlayerStatus()
        {
            MarketHistoryIDLookup = new MarketHistoryInfo[CacheSize];
        }

        public Location Location
        {
            get => location;
            set
            {
                location = value;
                Log.Information("Player location set to {Location}", Location.ToString());
                ConsoleManager.SetPlayerLocation(Location);
            }
        }
        public string PlayerName
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
        public AlbionServer AlbionServer
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

        public bool CheckLocationIDIsSet()
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
