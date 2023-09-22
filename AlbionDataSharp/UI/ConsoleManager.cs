using AlbionData.Models;
using AlbionDataSharp.Config;
using Serilog;
using Serilog.Events;
using Spectre.Console;
using System.Collections.Concurrent;

namespace AlbionDataSharp.UI
{
    public class ConsoleManager
    {
        private static readonly Table table = new Table()
            .AddColumns("Server", "Offers Sent", "Requests Sent", "Histories(Month) Sent", "Histories(Week) Sent", "Histories(Day) Sent");

        private static readonly Dictionary<string, int> serverRowIndices = new Dictionary<string, int>();

        private static ConcurrentQueue<LogEvent> stateUpdates = new ConcurrentQueue<LogEvent>();

        private static ConcurrentDictionary<string, int> offersSentCount = new ConcurrentDictionary<string, int>();
        private static ConcurrentDictionary<string, int> requestsSentCount = new ConcurrentDictionary<string, int>();
        private static ConcurrentDictionary<string, ConcurrentDictionary<Timescale, int>> historiesSentCount =
            new ConcurrentDictionary<string, ConcurrentDictionary<Timescale, int>>();
        public static void Initialize()
        {
            // Initialize table with server names and empty data
            var allServers = GetAllServers();
            int rowIndex = 0;
            foreach (var server in allServers)
            {
                table.AddRow(server, "0", "0", "0", "0", "0");
                serverRowIndices[server] = rowIndex++;
            }
            AnsiConsole.Write(table);
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
        private static void UpdateTableCell(string server, int columnIndex, string value)
        {
            if (serverRowIndices.TryGetValue(server, out int rowIndex))
            {
                table.UpdateCell(rowIndex, columnIndex, value);
            }
            ReWriteTable();
        }
        public static void IncrementOffersSent(string server, int count)
        {
            offersSentCount.AddOrUpdate(server, count, (key, oldValue) => oldValue + count);
            UpdateTableCell(server, 1, offersSentCount[server].ToString());
        }

        public static void IncrementRequestsSent(string server, int count)
        {
            requestsSentCount.AddOrUpdate(server, count, (key, oldValue) => oldValue + count);
            UpdateTableCell(server, 2, requestsSentCount[server].ToString());
        }

        public static void IncrementHistoriesSent(string server, int count, Timescale timescale)
        {
            var serverCounts = historiesSentCount.GetOrAdd(server, new ConcurrentDictionary<Timescale, int>());
            serverCounts.AddOrUpdate(timescale, count, (key, oldValue) => oldValue + count);
            int columnIndex = timescale switch
            {
                Timescale.Month => 3,
                Timescale.Week => 4,
                Timescale.Day => 5,
                _ => 3
            };
            UpdateTableCell(server, columnIndex, serverCounts[timescale].ToString());
        }

        public static void AddStateUpdate(LogEvent logEvent)
        {
            stateUpdates.Enqueue(logEvent);
            if (stateUpdates.Count > ConfigurationHelper.uiSettings.StateLineCount)
            {
                if (!stateUpdates.TryDequeue(out _))
                {
                    Log.Warning("Failed to dequeue a state update. Queue might be empty.");
                }
            }
        }

        public static async Task MonitorWindowSizeAsync()
        {
            int currentWidth = Console.WindowWidth;
            int currentHeight = Console.WindowHeight;

            while (true)
            {
                if (currentWidth != Console.WindowWidth || currentHeight != Console.WindowHeight)
                {
                    // Window size changed, redraw the table
                    ReWriteTable();

                    currentWidth = Console.WindowWidth;
                    currentHeight = Console.WindowHeight;
                }
                await Task.Delay(500);
            }
        }

        private static void ReWriteTable()
        {
            AnsiConsole.Clear();
            AnsiConsole.Render(table);
        }
    }
}
