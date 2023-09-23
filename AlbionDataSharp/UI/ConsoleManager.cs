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
            .AddColumns("Server", "Offers", "Requests", "Histories (Month)", "Histories (Week)", "Histories (Day)")
            .Expand();

        private static ConcurrentQueue<LogEvent> stateUpdates = new ConcurrentQueue<LogEvent>();

        private static ConcurrentDictionary<string, int> offersSentCount = new ConcurrentDictionary<string, int>();
        private static ConcurrentDictionary<string, int> requestsSentCount = new ConcurrentDictionary<string, int>();
        private static ConcurrentDictionary<string, ConcurrentDictionary<Timescale, int>> historiesSentCount =
            new ConcurrentDictionary<string, ConcurrentDictionary<Timescale, int>>();

        public static async Task Initialize()
        {
            serversTable.Columns.ToList().ForEach(x => x.Alignment = Justify.Center);
            WriteTable();
            await Monitor();
        }
        private static List<string> GetAllServers()
        {
            var allServers = new List<string>();
            if (ConfigurationHelper.networkSettings.AlbionDataServers?.East?.Name != null)
            {
                allServers.Add(ConfigurationHelper.networkSettings.AlbionDataServers.East.Name);
            }
            if (ConfigurationHelper.networkSettings.AlbionDataServers?.West?.Name != null)
            {
                allServers.Add(ConfigurationHelper.networkSettings.AlbionDataServers.West.Name);
            }
            if (ConfigurationHelper.networkSettings.PrivateWestServers != null)
            {
                allServers.AddRange(ConfigurationHelper.networkSettings.PrivateWestServers.Select(s => s.Name));
            }
            if (ConfigurationHelper.networkSettings.PrivateEastServers != null)
            {
                allServers.AddRange(ConfigurationHelper.networkSettings.PrivateEastServers.Select(s => s.Name));
            }
            return allServers;
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

            while (true)
            {
                if (currentWidth != Console.WindowWidth)
                {
                    Flag();
                    currentWidth = Console.WindowWidth;
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
                    $"[bold blue]{server}[/]",
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
            AnsiConsole.Write(serversTable);
            AnsiConsole.Write(logTable);

            shouldRewrite = false;
        }
    }
}
