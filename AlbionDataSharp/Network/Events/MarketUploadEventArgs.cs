using AlbionData.Models;
using AlbionDataSharp.Config;

namespace AlbionDataSharp.Network.Events
{
    public class MarketUploadEventArgs : EventArgs
    {
        public MarketUpload MarketUpload { get; set; }
        public ServerInfo Server { get; set; }
        public MarketUploadEventArgs(MarketUpload marketUpload, ServerInfo serverInfo)
        {
            MarketUpload = marketUpload;
            Server = serverInfo;
        }
    }
}
