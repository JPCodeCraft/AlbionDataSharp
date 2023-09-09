using Albion.Network;
using System.Text.Json;
using AlbionData.Models;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AlbionDataSharp.Responses
{
    public class AuctionGetOffersResponse : BaseOperation
    {
        public readonly MarketUpload marketUpload = new();

        public AuctionGetOffersResponse(Dictionary<byte, object> parameters) : base(parameters)
        {
            Log.Debug("Got {PacketType} packet.", GetType());

            if (!PlayerStatus.CheckLocationIDIsSet()) return;

            try
            {
                if (parameters.TryGetValue(0, out object orders))
                {
                    foreach (var auctionOfferString in (IEnumerable<string>)orders ?? new List<string>())
                    {
                        var order = JsonSerializer.Deserialize<MarketOrder>(auctionOfferString);
                        order.LocationId = ushort.Parse(PlayerStatus.LocationID);
                        marketUpload.Orders.Add(order);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }
    }
}
