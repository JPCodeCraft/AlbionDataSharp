using Albion.Network;
using AlbionDataSharp.Models;
using System.Text.Json;

namespace AlbionDataSharp.Responses
{
    public class AuctionGetOffersResponse : BaseOperation
    {
        public readonly List<AuctionEntry> AuctionEntries = new();

        public AuctionGetOffersResponse(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.TryGetValue(0, out object auctionOffers))
                {
                    foreach (var auctionOfferString in (IEnumerable<string>)auctionOffers ?? new List<string>())
                    {
                        AuctionEntries.Add(JsonSerializer.Deserialize<AuctionEntry>(auctionOfferString ?? string.Empty));
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
