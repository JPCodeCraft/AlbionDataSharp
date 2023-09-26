using Albion.Network;
using AlbionData.Models;
using Serilog;

namespace AlbionDataSharp.Network.Responses
{
    public class JoinResponse : BaseOperation
    {
        public readonly Location playerLocation;
        public readonly string playerName;
        public JoinResponse(Dictionary<byte, object> parameters) : base(parameters)
        {
            Log.Debug("Got {PacketType} packet.", GetType());
            try
            {
                if (parameters.TryGetValue(2, out object nameData))
                {
                    playerName = (string)nameData;
                }

                if (parameters.TryGetValue(8, out object locationData))
                {
                    string location = (string)locationData;
                    if (location.Contains("-Auction2")) location = location.Replace("-Auction2", "");
                    playerLocation = (Location)int.Parse(location);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }
    }
}
