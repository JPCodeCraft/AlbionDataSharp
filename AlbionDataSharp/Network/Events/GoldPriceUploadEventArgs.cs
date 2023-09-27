using AlbionData.Models;
using AlbionDataSharp.Config;

namespace AlbionDataSharp.Network.Events
{
    public class GoldPriceUploadEventArgs : EventArgs
    {
        public GoldPriceUpload GoldPriceUpload { get; set; }
        public ServerInfo Server { get; set; }
        public GoldPriceUploadEventArgs(GoldPriceUpload goldPriceUpload, ServerInfo serverInfo)
        {
            GoldPriceUpload = goldPriceUpload;
            Server = serverInfo;
        }
    }
}
