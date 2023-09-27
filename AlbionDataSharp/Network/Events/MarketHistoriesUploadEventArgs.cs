using AlbionData.Models;
using AlbionDataSharp.Config;

namespace AlbionDataSharp.Network.Events
{
    public class MarketHistoriesUploadEventArgs : EventArgs
    {
        public MarketHistoriesUpload MarketHistoriesUpload { get; set; }
        public ServerInfo Server { get; set; }
        public MarketHistoriesUploadEventArgs(MarketHistoriesUpload marketHistoriesUpload, ServerInfo serverInfo)
        {
            MarketHistoriesUpload = marketHistoriesUpload;
            Server = serverInfo;
        }
    }
}
