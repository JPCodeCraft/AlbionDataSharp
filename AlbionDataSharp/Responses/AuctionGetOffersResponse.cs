using Albion.Network;
using System.Text.Json;
using AlbionData.Models;

namespace AlbionDataSharp.Responses
{
    public class AuctionGetOffersResponse : BaseOperation
    {
        public readonly MarketUpload marketUpload = new();

        public AuctionGetOffersResponse(Dictionary<byte, object> parameters) : base(parameters)
        {
            Console.WriteLine($"Got {GetType().ToString()} packet.");
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
                Console.WriteLine(e.Message);
            }
        }
    }
}
