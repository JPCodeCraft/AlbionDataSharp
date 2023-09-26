namespace AlbionDataSharp.Config
{
    public class UiSettings
    {
        public required int MaxLogEntries { get; set; }
        public required int ConsoleRefreshRateMs { get; set; }
        public required Serilog.Events.LogEventLevel ConsoleLogLevel { get; set; }
    }
}
