using Albion.Network;
using AlbionData.Models;
using AlbionDataSharp.Network;
using AlbionDataSharp.Network.Responses;
using System.Text;
using System.Text.Json;

namespace AlbionDataSharp.Network.Handlers
{
    public class JoinResponseHandler : ResponsePacketHandler<JoinResponse>
    {
        public JoinResponseHandler() : base((int)OperationCodes.Join)
        {
        }

        protected override async Task OnActionAsync(JoinResponse value)
        {
            await Task.CompletedTask;
        }
    }
}
