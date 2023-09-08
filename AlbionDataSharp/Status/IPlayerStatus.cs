using AlbionDataSharp.Models;

namespace AlbionDataSharp.Status
{
    public interface IPlayerStatus
    {
        ulong CacheSize { get; }
        string LocationID { get; set; }
        MarketHistoryInfo[] MarketHistoryIDLookup { get; }
        string PlayerName { get; set; }
        Servers Server { get; set; }

        bool CheckLocationIDIsSet();
    }
}