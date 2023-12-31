﻿using Albion.Network;
using AlbionData.Models;
using Serilog;
using System.Text.Json;

namespace AlbionDataSharp.Network.Responses
{
    public class AuctionGetOffersResponse : BaseOperation
    {
        public List<MarketOrder> marketOrders = new();

        public AuctionGetOffersResponse(Dictionary<byte, object> parameters) : base(parameters)
        {
            Log.Debug("Got {PacketType} packet.", GetType());

            try
            {
                if (parameters.TryGetValue(0, out object? orders))
                {
                    foreach (var auctionOfferString in (IEnumerable<string>)orders ?? new List<string>())
                    {
                        var order = JsonSerializer.Deserialize<MarketOrder>(auctionOfferString);
                        if (order == null) continue;
                        marketOrders.Add(order);
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
