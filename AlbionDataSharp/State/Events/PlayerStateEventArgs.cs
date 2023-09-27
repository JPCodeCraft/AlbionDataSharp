using AlbionData.Models;
using AlbionDataSharp.Config;

namespace AlbionDataSharp.Network.Events
{
    public class PlayerStateEventArgs : EventArgs
    {
        public Location Location { get; set; }
        public string Name { get; set; }
        public AlbionServer AlbionServer { get; set; }
        public PlayerStateEventArgs(Location location, string name, AlbionServer albionServer)
        {
            Location = location;
            Name = name;
            AlbionServer = albionServer;
        }
    }
}
