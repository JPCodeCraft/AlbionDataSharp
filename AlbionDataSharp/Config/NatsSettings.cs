namespace AlbionDataSharp.Config
{
    internal class NatsSettings
    {
        public required string MarketOrdersIngestSubject { get; set; }
        public required string MarketHistoriesIngestSubject { get; set; }
        public required string GoldDataIngestSubject { get; set; }
        public required string AlbionDataEastServer { get; set; }
        public required string AlbionDataWestServer { get; set; }
        public required string[] PrivateWestServers { get; set; }
        public required string[] PrivateEastServers { get; set; }
    }
}
