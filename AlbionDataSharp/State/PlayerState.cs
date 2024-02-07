using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.Network;
using AlbionDataSharp.Network.Events;
using Serilog;

namespace AlbionDataSharp.State
{
    public class PlayerState
    {
        private Location location = 0;
        private string playerName = string.Empty;
        private AlbionServer albionServer = AlbionServer.Unknown;
        private int uploadQueueSize = 0;

        public MarketHistoryInfo[] MarketHistoryIDLookup { get; init; }
        public ulong CacheSize => 8192;
        private Queue<string> SentDataHashs = new Queue<string>();
        private const int maxHashQueueSize = 30;

        public event EventHandler<PlayerStateEventArgs> OnPlayerStateChanged;

        public PlayerState()
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
                OnPlayerStateChanged?.Invoke(this, new PlayerStateEventArgs(Location, PlayerName, AlbionServer, UploadQueueSize));
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
                OnPlayerStateChanged?.Invoke(this, new PlayerStateEventArgs(Location, PlayerName, AlbionServer, UploadQueueSize));
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
                OnPlayerStateChanged?.Invoke(this, new PlayerStateEventArgs(Location, PlayerName, AlbionServer, UploadQueueSize));
            }
        }
        public int UploadQueueSize
        {
            get => uploadQueueSize;
            set
            {
                uploadQueueSize = value;
                OnPlayerStateChanged?.Invoke(this, new PlayerStateEventArgs(Location, PlayerName, AlbionServer, UploadQueueSize));
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

        public void AddSentDataHash(string hash)
        {
            if (hash == null || hash.Length == 0 || SentDataHashs.Contains(hash)) return;

            if (SentDataHashs.Count == maxHashQueueSize)
            {
                SentDataHashs.Dequeue();
            }
            SentDataHashs.Enqueue(hash);
        }

        public bool CheckHashInQueue(string hash)
        {
            bool result = SentDataHashs.Contains(hash);
            return result;
        }
    }
}
