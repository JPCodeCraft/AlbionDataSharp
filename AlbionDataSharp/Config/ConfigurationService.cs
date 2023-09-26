using Microsoft.Extensions.Configuration;

namespace AlbionDataSharp.Config
{
    public class ConfigurationService
    {
        public NetworkSettings NetworkSettings { get; }
        public UiSettings UiSettings { get; }

        public ConfigurationService(IConfiguration config)
        {
            NetworkSettings = config.GetSection("Network").Get<NetworkSettings>();
            UiSettings = config.GetSection("UIConfig").Get<UiSettings>();
        }
    }
}
