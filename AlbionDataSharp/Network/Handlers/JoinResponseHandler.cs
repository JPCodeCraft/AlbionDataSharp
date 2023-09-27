using Albion.Network;
using AlbionDataSharp.Network.Responses;
using AlbionDataSharp.State;

namespace AlbionDataSharp.Network.Handlers
{
    public class JoinResponseHandler : ResponsePacketHandler<JoinResponse>
    {
        private readonly PlayerState playerState;
        public JoinResponseHandler(PlayerState playerState) : base((int)OperationCodes.Join)
        {
            this.playerState = playerState;
        }

        protected override async Task OnActionAsync(JoinResponse value)
        {
            playerState.PlayerName = value.playerName;
            playerState.Location = value.playerLocation;
            await Task.CompletedTask;
        }
    }
}
