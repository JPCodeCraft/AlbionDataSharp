using Albion.Network;
using System.Text.Json;
using AlbionData.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using AlbionDataSharp.State;

namespace AlbionDataSharp.Network.Responses
{
    public class JoinResponse : BaseOperation
    {
        public JoinResponse(Dictionary<byte, object> parameters) : base(parameters)
        {
            Log.Debug("Got {PacketType} packet.", GetType());
            try
            {
                if (parameters.TryGetValue(2, out object nameData))
                {
                    PlayerStatus.PlayerName = (string)nameData;
                }

                if (parameters.TryGetValue(8, out object locationData))
                {
                    string location = (string)locationData;
                    if (location.Contains("-Auction2")) location = location.Replace("-Auction2", "");
                    PlayerStatus.LocationID = location;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }
    }
}
