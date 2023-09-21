using Microsoft.Extensions.Configuration;

namespace AlbionDataSharp.Config
{
    internal static class ConfigurationHelper
    {
        public static IConfiguration config;
        public static NetworkSettings networkSettings;
        public static UiSettings uiSettings;
        public static void Initialize(IConfiguration Configuration)
        {
            config = Configuration;
            networkSettings = config.GetRequiredSection("Network").Get<NetworkSettings>();
            uiSettings = config.GetRequiredSection("UIConfig").Get<UiSettings>();
        }
    }
}
