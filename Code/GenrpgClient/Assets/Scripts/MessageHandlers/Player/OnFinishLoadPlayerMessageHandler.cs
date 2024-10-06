
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Player
{
    public class OnFinishLoadPlayerMessageHandler : BaseClientMapMessageHandler<OnFinishLoadPlayer>
    {
        protected IScreenService _screenService;
        protected override void InnerProcess(OnFinishLoadPlayer msg, CancellationToken token)
        {
            _dispatcher.Dispatch(msg);
            _screenService.CloseAll();
            _screenService.Open(ScreenId.HUD);
        }
    }
}
