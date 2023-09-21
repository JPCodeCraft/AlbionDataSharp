using Microsoft.Extensions.Configuration;

namespace AlbionDataSharp.Config
{
    internal static class ConfigurationHelper
    {
        public static IConfiguration config;
        public static NetworkSettings networkSettings;
        public static void Initialize(IConfiguration Configuration)
        {
            config = Configuration;
            networkSettings = config.GetRequiredSection("Network").Get<NetworkSettings>();
        }
    }
}
