using AlbionData.Models;
using AlbionDataSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbionDataSharp
{
    internal static class PlayerStatus
    {
        private static string locationID;
        private static string playerName;
        private static Servers server = Servers.Unknown;
        //CacheSize limit size of messages in cache
        private const ulong cacheSize = 8192;

        public static MarketHistoryInfo[] MarketHistoryIDLookup { get; } = new MarketHistoryInfo[CacheSize];
        public static ulong CacheSize => cacheSize;

        public static string LocationID { get => locationID; 
            set 
            { 
                locationID = value; 
                Console.WriteLine($"Player location set to {LocationID}");
            } }
        public static string PlayerName
        {
            get => playerName;
            set
            {
                if (playerName == value) return;
                playerName = value;
                Console.WriteLine($"Player name set to {PlayerName}");
            }
        }
        public static Servers Server
        {
            get => server;
            set
            {
                if (server == value) return;
                server = value;
                Console.WriteLine($"Server set to {Server}");
            }
        }

        public static bool CheckLocationIDIsSet()
        {
            if (locationID == null || !Enum.IsDefined(typeof(Location), int.Parse(LocationID)))
            {
                Console.WriteLine($"Player location is not set. Please change maps.");
                return false;
            }
            else return true;
        }
    }
}
