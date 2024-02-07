using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.Network;
using AlbionDataSharp.Network.Events;
using AlbionDataSharp.State;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Spectre.Console;
using System.Collections.Concurrent;
using System.Reflection;

namespace AlbionDataSharp.UI
{
    public class ConsoleManager : BackgroundService
    {
        private bool shouldRewrite = false;

        private readonly Table logTable = new Table()
            .Title("[bold yellow]Log Events[/]")
            .Border(TableBorder.Double)
            .AddColumns("Log")
            .HideHeaders()
            .Expand();

        private readonly Table serversTable = new Table()
            .Border(TableBorder.Double)
            .AddColumns("[bold]Server[/]", "[bold]Offers[/]", "[bold]Requests[/]", "[bold]Histories (Month)[/]", "[bold]Histories (Week)[/]", "[bold]Histories (Day)[/]", "[bold]Gold Histories[/]")
            .Expand();
        private readonly Table playerTable = new Table()
            .Border(TableBorder.Double)
            .AddColumns("[bold]Player Server[/]", "[bold]Player Name[/]", "[bold]Player Location[/]")
            .Expand();

        private ConcurrentQueue<LogEvent> stateUpdates = new ConcurrentQueue<LogEvent>();

        private ConcurrentDictionary<ServerInfo, int> offersSentCount = new ConcurrentDictionary<ServerInfo, int>();
        private ConcurrentDictionary<ServerInfo, int> requestsSentCount = new ConcurrentDictionary<ServerInfo, int>();
        private ConcurrentDictionary<ServerInfo, int> goldHistoriesSentCount = new ConcurrentDictionary<ServerInfo, int>();
        private ConcurrentDictionary<ServerInfo, ConcurrentDictionary<Timescale, int>> historiesSentCount =
            new ConcurrentDictionary<ServerInfo, ConcurrentDictionary<Timescale, int>>();

        private ConfigurationService configurationService;
        private Uploader uploader;
        private PlayerState playerState;

        private string playerName = string.Empty;
        private Location playerLocation = 0;
        private AlbionServer albionServer = AlbionServer.Unknown;
        private int uploadQueueSize = 0;

        private string version = string.Empty;

        private EventHandler<MarketUploadEventArgs> marketUploadHandler;
        private EventHandler<GoldPriceUploadEventArgs> goldPriceUploadHandler;
        private EventHandler<MarketHistoriesUploadEventArgs> marketHistoryUploadHandler;
        private EventHandler<PlayerStateEventArgs> playerStateHandler;
        public ConsoleManager(ConfigurationService configurationService, Uploader uploader, PlayerState playerState)
        {
            this.configurationService = configurationService;
            this.uploader = uploader;
            this.playerState = playerState;

            marketUploadHandler = (sender, args) => ProcessMarketUpload(args.MarketUpload, args.Server);
            goldPriceUploadHandler = (sender, args) => ProcessGoldPriceUpload(args.GoldPriceUpload, args.Server);
            marketHistoryUploadHandler = (sender, args) => ProcessMarketHistoriesUpload(args.MarketHistoriesUpload, args.Server);
            playerStateHandler = (sender, args) => ProcessPlayerState(args.Location, args.Name, args.AlbionServer, args.UploadQueueSize);

            uploader.OnMarketUpload += marketUploadHandler;
            uploader.OnGoldPriceUpload += goldPriceUploadHandler;
            uploader.OnMarketHistoryUpload += marketHistoryUploadHandler;
            playerState.OnPlayerStateChanged += playerStateHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Initialize();
        }
        private async Task Initialize()
        {
            version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            playerTable.Title($"[bold underline yellow]Albion Data Sharp (v. {version})[/]");
            serversTable.Columns.ToList().ForEach(x => x.Alignment = Justify.Center);
            playerTable.Columns.ToList().ForEach(x => x.Alignment = Justify.Center);
            WriteTable();
            await Monitor();
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            uploader.OnMarketUpload -= marketUploadHandler;
            uploader.OnGoldPriceUpload -= goldPriceUploadHandler;
            uploader.OnMarketHistoryUpload -= marketHistoryUploadHandler;
            playerState.OnPlayerStateChanged -= playerStateHandler;
            Log.Information("Stopped {type}!", nameof(ConsoleManager));
            Log.CloseAndFlush();
            await base.StopAsync(stoppingToken);
        }
        private List<ServerInfo> GetAllServers()
        {
            return configurationService.NetworkSettings.UploadServers.ToList();
        }
        private void ProcessPlayerState(Location location, string name, AlbionServer albionServer, int uploadQueueSize)
        {
            playerName = name;
            playerLocation = location;
            this.albionServer = albionServer;
            this.uploadQueueSize = uploadQueueSize;
            FlagReWrite();
        }
        private void ProcessMarketUpload(MarketUpload marketUpload, ServerInfo server)
        {
            int offers = marketUpload.Orders.Count(x => x.AuctionType == "offer");
            int requests = marketUpload.Orders.Count(x => x.AuctionType == "request");
            offersSentCount.AddOrUpdate(server, offers, (key, oldValue) => oldValue + offers);
            requestsSentCount.AddOrUpdate(server, requests, (key, oldValue) => oldValue + requests);
            FlagReWrite();
        }
        private void ProcessMarketHistoriesUpload(MarketHistoriesUpload marketHistoriesUpload, ServerInfo server)
        {
            var serverCounts = historiesSentCount.GetOrAdd(server, new ConcurrentDictionary<Timescale, int>());
            var count = marketHistoriesUpload.MarketHistories.Count;
            serverCounts.AddOrUpdate(marketHistoriesUpload.Timescale, count, (key, oldValue) => oldValue + count);
            FlagReWrite();
        }
        private void ProcessGoldPriceUpload(GoldPriceUpload goldPriceUpload, ServerInfo server)
        {
            var count = goldPriceUpload.Prices.Count();
            goldHistoriesSentCount.AddOrUpdate(server, count, (key, oldValue) => oldValue + count);
            FlagReWrite();
        }
        public void AddStateUpdate(LogEvent logEvent)
        {
            stateUpdates.Enqueue(logEvent);
            if (stateUpdates.Count > configurationService.UiSettings.MaxLogEntries)
            {
                stateUpdates.TryDequeue(out _);
            }
            FlagReWrite();
        }
        private async Task Monitor()
        {
            int currentWidth = Console.WindowWidth;
            int currentHeight = Console.WindowHeight;

            while (true)
            {
                if (currentWidth != Console.WindowWidth || currentHeight != Console.WindowHeight)
                {
                    FlagReWrite();
                    currentWidth = Console.WindowWidth;
                    currentHeight = Console.WindowHeight;
                }
                if (shouldRewrite)
                {
                    WriteTable();
                }
                await Task.Delay(configurationService.UiSettings.ConsoleRefreshRateMs);
            }
        }
        private void FlagReWrite()
        {
            shouldRewrite = true;
        }
        private void WriteTable()
        {
            //Sets serversTable Title
            serversTable.Title($"[bold yellow]Server Statistics - Data Sent (Queue Size: {uploadQueueSize})[/]");
            // Clear existing rows
            serversTable.Rows.Clear();
            logTable.Rows.Clear();
            playerTable.Rows.Clear();

            //Fill playerTable with player name and location and server
            string nameText = string.IsNullOrEmpty(playerName) ? "[red]Unknown - change maps![/]" : playerName;
            string locationText = playerLocation == 0 ? "[red]Unknown - change maps![/]" : playerLocation.ToString();
            string serverText = albionServer == AlbionServer.Unknown ? "[red]Unknown - error!!![/]" : albionServer.ToString();
            playerTable.AddRow(serverText, nameText, locationText);

            // Fill serversTable with server names and data
            var allServers = GetAllServers();
            foreach (var server in allServers)
            {
                offersSentCount.TryGetValue(server, out int offers);
                requestsSentCount.TryGetValue(server, out int requests);
                historiesSentCount.TryGetValue(server, out var histories);
                goldHistoriesSentCount.TryGetValue(server, out var goldHistories);

                int historiesMonth = histories?.GetValueOrDefault(Timescale.Month) ?? 0;
                int historiesWeek = histories?.GetValueOrDefault(Timescale.Week) ?? 0;
                int historiesDay = histories?.GetValueOrDefault(Timescale.Day) ?? 0;

                string serverStyle = server.Color + ((server.IsReachable && server.AlbionServer == playerState.AlbionServer) ? "" : " strikethrough");

                serversTable.AddRow(
                    $"[{serverStyle}]{server.Name}[/]",
                    offers.ToString(),
                    requests.ToString(),
                    historiesMonth.ToString(),
                    historiesWeek.ToString(),
                    historiesDay.ToString(),
                    goldHistories.ToString()
                );
            }

            // Fill logTable with state updates
            foreach (var logEvent in stateUpdates.Reverse())
            {
                var message = logEvent.RenderMessage();
                var timestamp = logEvent.Timestamp.ToString("yy.MM.dd HH:mm:ss");
                var level = logEvent.Level.ToString().ToUpper().Substring(0, 3);

                var formattedMessage = $"{timestamp} ({level}) {message}";

                // Set color based on log level
                string colorCode = "white";
                switch (logEvent.Level)
                {
                    case LogEventLevel.Information:
                        colorCode = "cyan";
                        break;
                    case LogEventLevel.Warning:
                        colorCode = "yellow";
                        break;
                    case LogEventLevel.Error:
                        colorCode = "red";
                        break;
                    case LogEventLevel.Fatal:
                        colorCode = "darkred";
                        break;
                }

                logTable.AddRow(new Markup($"[{colorCode}]{formattedMessage}[/]"));
            }

            //Clear the console
            AnsiConsole.Clear();
            //Write tables to console
            AnsiConsole.Write(playerTable);
            AnsiConsole.Write(serversTable);
            AnsiConsole.Write(logTable);

            shouldRewrite = false;
        }
    }
}
