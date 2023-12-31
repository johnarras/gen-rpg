
using Genrpg.Shared.Players.Messages;
using System.Threading;
using UI.Screens.Constants;

namespace Assets.Scripts.MessageHandlers.Player
{
    public class OnFinishLoadPlayerMessageHandler : BaseClientMapMessageHandler<OnFinishLoadPlayer>
    {
        protected IScreenService _screenService;
        protected override void InnerProcess(UnityGameState gs, OnFinishLoadPlayer msg, CancellationToken token)
        {
            gs.Dispatch(msg);
            _screenService.CloseAll(gs);
            _screenService.Open(gs, ScreenId.HUD);
        }
    }
}
