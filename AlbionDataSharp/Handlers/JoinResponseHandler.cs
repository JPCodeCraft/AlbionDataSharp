using Albion.Network;
using AlbionData.Models;
using AlbionDataSharp.Responses;
using System.Text;
using System.Text.Json;

namespace AlbionDataSharp.Handlers
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
