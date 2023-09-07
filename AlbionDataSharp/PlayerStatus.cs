using AlbionData.Models;
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
            if (locationID == null) return false;
            return Enum.IsDefined(typeof(Location), int.Parse(LocationID));
        }
    }
}
