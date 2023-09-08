using Albion.Network;
using AlbionData.Models;
using AlbionDataSharp.Nats;
using AlbionDataSharp.Responses;
using System.Text;
using System.Text.Json;

namespace AlbionDataSharp.Handlers
{
    public class AuctionGetRequestsResponseHandler : ResponsePacketHandler<AuctionGetRequestsResponse>
    {
        private readonly INatsManager _natsManager;
        public AuctionGetRequestsResponseHandler(INatsManager natsManager) : base((int)OperationCodes.AuctionGetRequests)
        {
            _natsManager = natsManager;
        }

        protected override async Task OnActionAsync(AuctionGetRequestsResponse value)
        {
            _natsManager.Upload(value.marketUpload);
            await Task.CompletedTask;
        }
    }
}
