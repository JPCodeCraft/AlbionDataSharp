using Albion.Network;
using System.Text.Json;
using AlbionData.Models;

namespace AlbionDataSharp.Responses
{
    public class JoinResponse : BaseOperation
    {

        public JoinResponse(Dictionary<byte, object> parameters) : base(parameters)
        {

            Console.WriteLine($"Got {GetType().ToString()} packet.");
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
                Console.WriteLine(e.Message);
            }
        }
    }
}
