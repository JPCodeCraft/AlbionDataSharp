using AlbionData.Models;
using AlbionDataSharp.Config;
using Serilog.Events;
using Spectre.Console;
using System.Collections.Concurrent;

namespace AlbionDataSharp.UI
{
    public class ConsoleManager
    {
        private static bool shouldRewrite = false;

        private static readonly Table logTable = new Table()
            .Title("[bold yellow]Log Events[/]")
            .Border(TableBorder.Double)
            .AddColumns("Log")
            .HideHeaders()
            .Expand();

        private static readonly Table serversTable = new Table()
            .Title("[bold yellow]Server Statistics - Data Sent[/]")
            .Border(TableBorder.Double)
            .AddColumns("[bold]Server[/]", "[bold]Offers[/]", "[bold]Requests[/]", "[bold]Histories (Month)[/]", "[bold]Histories (Week)[/]", "[bold]Histories (Day)[/]")
            .Expand();
        private static readonly Table playerTable = new Table()
            .Title("[bold underline yellow]Albion Data Sharp[/]")
            .Border(TableBorder.Double)
            .AddColumns("[bold]Player Server[/]", "[bold]Player Name[/]", "[bold]Player Location[/]")
            .Expand();

        private static ConcurrentQueue<LogEvent> stateUpdates = new ConcurrentQueue<LogEvent>();

        private static ConcurrentDictionary<string, int> offersSentCount = new ConcurrentDictionary<string, int>();
        private static ConcurrentDictionary<string, int> requestsSentCount = new ConcurrentDictionary<string, int>();
        private static ConcurrentDictionary<string, ConcurrentDictionary<Timescale, int>> historiesSentCount =
            new ConcurrentDictionary<string, ConcurrentDictionary<Timescale, int>>();


        private static string playerName = string.Empty;
        private static Location playerLocation = 0;
        private static AlbionServer albionServer = AlbionServer.Unknown;

        public static async Task Initialize()
        {
            serversTable.Columns.ToList().ForEach(x => x.Alignment = Justify.Center);
            playerTable.Columns.ToList().ForEach(x => x.Alignment = Justify.Center);
            WriteTable();
            await Monitor();
        }
        private static List<string> GetAllServers()
        {
            return ConfigurationHelper.networkSettings.UploadServers.Select(x => x.Name).ToList();
        }

        public static void SetPlayerName(string name)
        {
            playerName = name;
            Flag();
        }

        public static void SetPlayerLocation(Location location)
        {
            playerLocation = location;
            Flag();
        }

        public static void SetAlbionServer(AlbionServer server)
        {
            albionServer = server;
            Flag();
        }

        public static void IncrementOffersSent(string server, int count)
        {
            offersSentCount.AddOrUpdate(server, count, (key, oldValue) => oldValue + count);
            Flag();
        }

        public static void IncrementRequestsSent(string server, int count)
        {
            requestsSentCount.AddOrUpdate(server, count, (key, oldValue) => oldValue + count);
            Flag();
        }

        public static void IncrementHistoriesSent(string server, int count, Timescale timescale)
        {
            var serverCounts = historiesSentCount.GetOrAdd(server, new ConcurrentDictionary<Timescale, int>());
            serverCounts.AddOrUpdate(timescale, count, (key, oldValue) => oldValue + count);
            Flag();
        }

        public static void AddStateUpdate(LogEvent logEvent)
        {
            stateUpdates.Enqueue(logEvent);
            if (stateUpdates.Count > ConfigurationHelper.uiSettings.MaxLogEntries)
            {
                stateUpdates.TryDequeue(out _);
            }
            Flag();
        }

        public static async Task Monitor()
        {
            int currentWidth = Console.WindowWidth;
            int currentHeight = Console.WindowHeight;

            while (true)
            {
                if (currentWidth != Console.WindowWidth || currentHeight != Console.WindowHeight)
                {
                    Flag();
                    currentWidth = Console.WindowWidth;
                    currentHeight = Console.WindowHeight;
                }
                if (shouldRewrite)
                {
                    WriteTable();
                }
                await Task.Delay(ConfigurationHelper.uiSettings.ConsoleRefreshRateMs);
            }
        }

        private static void Flag()
        {
            shouldRewrite = true;
        }

        private static void WriteTable()
        {
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

                int historiesMonth = histories?.GetValueOrDefault(Timescale.Month) ?? 0;
                int historiesWeek = histories?.GetValueOrDefault(Timescale.Week) ?? 0;
                int historiesDay = histories?.GetValueOrDefault(Timescale.Day) ?? 0;

                serversTable.AddRow(
                    $"[bold teal]{server}[/]",
                    offers.ToString(),
                    requests.ToString(),
                    historiesMonth.ToString(),
                    historiesWeek.ToString(),
                    historiesDay.ToString()
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
