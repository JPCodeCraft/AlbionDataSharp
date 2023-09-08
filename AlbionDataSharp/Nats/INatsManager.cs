using AlbionData.Models;

namespace AlbionDataSharp.Nats
{
    public interface INatsManager
    {
        void Upload(MarketHistoriesUpload marketHistoriesUpload);
        void Upload(MarketUpload marketUpload);
    }
}