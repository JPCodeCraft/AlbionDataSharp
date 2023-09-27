using Albion.Network;
using AlbionDataSharp.Network.Responses;
using AlbionDataSharp.State;

namespace AlbionDataSharp.Network.Handlers
{
    public class JoinResponseHandler : ResponsePacketHandler<JoinResponse>
    {
        private readonly PlayerState playerStatus;
        public JoinResponseHandler(PlayerState playerStatus) : base((int)OperationCodes.Join)
        {
            this.playerStatus = playerStatus;
        }

        protected override async Task OnActionAsync(JoinResponse value)
        {
            playerStatus.PlayerName = value.playerName;
            playerStatus.Location = value.playerLocation;
            await Task.CompletedTask;
        }
    }
}
