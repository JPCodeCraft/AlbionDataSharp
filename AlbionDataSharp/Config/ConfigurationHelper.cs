using Microsoft.Extensions.Configuration;

namespace AlbionDataSharp.Config
{
    internal static class ConfigurationHelper
    {
        public static IConfiguration config;
        public static NatsSettings natsSettings;
        public static void Initialize(IConfiguration Configuration)
        {
            config = Configuration;
            natsSettings = config.GetRequiredSection("Nats").Get<NatsSettings>();
        }
    }
}
