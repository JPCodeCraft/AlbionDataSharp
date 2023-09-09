using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbionDataSharp
{
    public static class Logger
    {
        public static ILogger<T> New<T>()
        {
            var factory = LoggerFactory.Create(builder => {
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
                builder.SetMinimumLevel(LogLevel.Trace);
            });
            
            var logger = factory.CreateLogger<T>();
            return logger;
        }
    }
}
