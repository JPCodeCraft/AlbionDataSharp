using Serilog.Core;
using Serilog.Events;

namespace AlbionDataSharp.UI
{
    public class DelegatingSink : ILogEventSink
    {
        private readonly Action<LogEvent> _writeAction;

        public DelegatingSink(Action<LogEvent> writeAction)
        {
            _writeAction = writeAction ?? throw new ArgumentNullException(nameof(writeAction));
        }

        public void Emit(LogEvent logEvent)
        {
            _writeAction(logEvent);
        }
    }
}
