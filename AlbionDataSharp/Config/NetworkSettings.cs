namespace AlbionDataSharp.Config
{
    public class NetworkSettings
    {
        public required string MarketOrdersIngestSubject { get; set; }
        public required string MarketHistoriesIngestSubject { get; set; }
        public required string GoldDataIngestSubject { get; set; }
        public required ServerInfo[] UploadServers { get; set; }
        public required float ThreadLimitPercentage { get; set; }
        public required string UserAgent { get; set; }
    }
}
