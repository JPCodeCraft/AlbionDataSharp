using AlbionData.Models;
using AlbionDataSharp.Config;
using AlbionDataSharp.State;
using Serilog.Events;

namespace AlbionDataSharp.UI
{
    public class ConsoleManager
    {
        static Queue<LogEvent> stateUpdates = new Queue<LogEvent>();
        public void UpdateTitle(string title)
        {
            Console.SetCursorPosition(0, ConfigurationHelper.uiSettings.TitleLine);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(title);
            Console.ResetColor();
        }
        public void UpdateName(string name)
        {
            Console.SetCursorPosition(0, ConfigurationHelper.uiSettings.LocationLine);
            Console.Write("Welcome ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{name}".PadRight(50, ' '));
            Console.ResetColor();
        }
        public void UpdateLocation(Location location)
        {
            Console.SetCursorPosition(0, ConfigurationHelper.uiSettings.LocationLine);
            Console.Write("Your location was set to ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{location}".PadRight(50, ' '));
            Console.ResetColor();
        }
        public void UpdateServer(Server server)
        {
            Console.SetCursorPosition(0, ConfigurationHelper.uiSettings.ServerLine);
            Console.Write("Your server was set to ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"{server}".PadRight(50, ' '));
            Console.ResetColor();
        }
        public static void AddStateUpdate(LogEvent logEvent)
        {
            stateUpdates.Enqueue(logEvent);
            if (stateUpdates.Count > ConfigurationHelper.uiSettings.StateLineCount)
            {
                stateUpdates.Dequeue();
            }

            Console.SetCursorPosition(0, ConfigurationHelper.uiSettings.StateStartLine);

            foreach (var stateUpdate in stateUpdates.Reverse())
            {
                //erase line
                Console.Write(new string(' ', Console.WindowWidth));
                Console.CursorLeft = 0;

                var message = stateUpdate.RenderMessage();
                var timestamp = stateUpdate.Timestamp.ToString("yy-MM-dd HH:mm:ss");
                var level = stateUpdate.Level.ToString().ToUpper();

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
                Console.WriteLine(formattedMessage.PadRight(50, ' '));
                Console.ResetColor();
            }
        }
    }
}
