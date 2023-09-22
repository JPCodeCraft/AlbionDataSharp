using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.State;
using Serilog;
using Serilog.Events;
using System.Collections.Concurrent;

namespace AlbionDataSharp.UI
{
    public class ConsoleManager
    {
        private static Dictionary<int, int> lineLengths = new Dictionary<int, int>();

        private static ConcurrentQueue<LogEvent> stateUpdates = new ConcurrentQueue<LogEvent>();

        private static ConcurrentDictionary<string, int> offersSentCount = new ConcurrentDictionary<string, int>();
        private static ConcurrentDictionary<string, int> requestsSentCount = new ConcurrentDictionary<string, int>();
        private static ConcurrentDictionary<string, ConcurrentDictionary<Timescale, int>> historiesSentCount =
            new ConcurrentDictionary<string, ConcurrentDictionary<Timescale, int>>();

        private static void IncrementOffersSent(string server, int count)
        {
            offersSentCount.AddOrUpdate(server, count, (key, oldValue) => oldValue + count);
        }

        private static void IncrementRequestsSent(string server, int count)
        {
            requestsSentCount.AddOrUpdate(server, count, (key, oldValue) => oldValue + count);
        }

        public static void IncrementHistoriesSent(string server, int count, Timescale timescale)
        {
            var serverCounts = historiesSentCount.GetOrAdd(server, new ConcurrentDictionary<Timescale, int>());
            serverCounts.AddOrUpdate(timescale, count, (key, oldValue) => oldValue + count);
        }

        public static void UpdateOffersSent(string server, int count)
        {
            int line = ConfigurationHelper.uiSettings.OffersLine;
            EraseLine(line);

            IncrementOffersSent(server, count);

            int lineLength = 0;
            WriteAndCount("Number of Offers sent for ", line, ref lineLength);
            int i = 0;
            foreach (var entry in offersSentCount)
            {
                Console.ForegroundColor = i % 2 == 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
                WriteAndCount("| ", line, ref lineLength);
                WriteAndCount(entry.Key, line, ref lineLength);  // Server name
                Console.ResetColor();
                WriteAndCount(" : ", line, ref lineLength);
                WriteAndCount(entry.Value.ToString(), line, ref lineLength);  // Count of offers
                WriteAndCount(" | ", line, ref lineLength);
            }
            Console.ResetColor();
        }

        public static void UpdateRequestsSent(string server, int count)
        {
            int line = ConfigurationHelper.uiSettings.RequestsLine;
            EraseLine(line);

            IncrementRequestsSent(server, count);

            int lineLength = 0;
            WriteAndCount("Number of Requests sent for ", line, ref lineLength);
            int i = 0;
            foreach (var entry in requestsSentCount)
            {
                Console.ForegroundColor = i % 2 == 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
                WriteAndCount("| ", line, ref lineLength);
                WriteAndCount(entry.Key, line, ref lineLength);  // Server name
                Console.ResetColor();
                WriteAndCount(" : ", line, ref lineLength);
                WriteAndCount(entry.Value.ToString(), line, ref lineLength);  // Count of requests
                WriteAndCount(" | ", line, ref lineLength);
                i++;
            }
            Console.ResetColor();
        }

        public static void UpdateHistoriesSent(string server, int count, Timescale timescale)
        {
            int line = GetLineBasedOnTimescale(timescale);
            EraseLine(line);

            IncrementHistoriesSent(server, count, timescale);

            int lineLength = 0;
            WriteAndCount($"Number of histories ", line, ref lineLength);
            Console.ForegroundColor = ConsoleColor.Green;
            WriteAndCount($"[{timescale}]", line, ref lineLength);
            Console.ResetColor();
            WriteAndCount($"sent for", line, ref lineLength);

            int i = 0;
            foreach (var serverEntry in historiesSentCount)
            {
                if (serverEntry.Value.ContainsKey(timescale))
                {
                    Console.ForegroundColor = i % 2 == 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
                    WriteAndCount($"| {serverEntry.Key} : {serverEntry.Value[timescale]} | ", line, ref lineLength);
                }
            }
            Console.ResetColor();
        }

        public static void UpdateTitle(string title)
        {
            int line = ConfigurationHelper.uiSettings.TitleLine;
            EraseLine(line);

            Console.ForegroundColor = ConsoleColor.Green;
            int lineLength = 0;
            WriteAndCount(title, line, ref lineLength);
            Console.ResetColor();
        }
        public static void UpdateName(string name)
        {
            int line = ConfigurationHelper.uiSettings.NameLine;
            EraseLine(line);

            int lineLength = 0;
            WriteAndCount("Welcome ", line, ref lineLength);
            Console.ForegroundColor = ConsoleColor.Blue;
            WriteAndCount($"{name}", line, ref lineLength);
            Console.ResetColor();
            WriteAndCount($"!", line, ref lineLength);
            Console.ResetColor();
        }
        public static void UpdateLocation(Location location)
        {
            int line = ConfigurationHelper.uiSettings.LocationLine;
            EraseLine(line);

            int lineLength = 0;
            WriteAndCount("Your location is ", line, ref lineLength);
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteAndCount($"{location}.", line, ref lineLength);
            Console.ResetColor();
        }
        public static void UpdateServer(Server server)
        {
            int line = ConfigurationHelper.uiSettings.ServerLine;
            EraseLine(line);

            int lineLength = 0;
            WriteAndCount("Your server is ", line, ref lineLength);
            Console.ForegroundColor = ConsoleColor.Cyan;
            WriteAndCount($"{server}.", line, ref lineLength);
            Console.ResetColor();
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
            int line = ConfigurationHelper.uiSettings.StateStartLine;

            foreach (var stateUpdate in stateUpdates.Reverse())
            {
                EraseLine(line);
                int lineLength = 0;

                var message = stateUpdate.RenderMessage();
                var timestamp = stateUpdate.Timestamp.ToString("yy.MM.dd HH:mm:ss");
                var level = stateUpdate.Level.ToString().ToUpper().Substring(0, 3);

                var formattedMessage = $"{timestamp} [{level}] {message}";

                // Set color based on log level
                switch (stateUpdate.Level)
                {
                    case LogEventLevel.Information:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case LogEventLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogEventLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogEventLevel.Fatal:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
                // Reset color
                WriteAndCount(formattedMessage, line, ref lineLength);
                Console.ResetColor();
                line++;
            }
        }

        private static void EraseLine(int line)
        {
            Console.SetCursorPosition(0, line);
            int eraseLength = lineLengths.ContainsKey(line) ? lineLengths[line] : Console.WindowWidth - 1;
            Console.Write(new string(' ', eraseLength));
            Console.CursorLeft = 0;
        }

        private static void WriteAndCount(string text, int line, ref int lineLength)
        {
            Console.Write(text);
            lineLength += text.Length;
            lineLengths[line] = lineLength;
        }

        private static int GetLineBasedOnTimescale(Timescale timescale)
        {
            switch (timescale)
            {
                case Timescale.Month:
                    return ConfigurationHelper.uiSettings.HistoriesMonthLine;
                case Timescale.Week:
                    return ConfigurationHelper.uiSettings.HistoriesWeekLine;
                case Timescale.Day:
                    return ConfigurationHelper.uiSettings.HistoriesDayLine;
                default:
                    return 0;  // Default line number, you can change this
            }
        }
    }
}
