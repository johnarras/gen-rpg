using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Login.Messages.Error;
using System.Threading;
using UI.Screens.Constants;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class ErrorResultHandler : BaseClientLoginResultHandler<ErrorResult>
    {
        private IScreenService _screenService;
        protected override void InnerProcess(ErrorResult result, CancellationToken token)
        {
            _dispatcher.Dispatch(new ShowFloatingText(result.Error, EFloatingTextArt.Error));
            _screenService.CloseAll();
            _screenService.Open(ScreenId.Login);
        }
    }
}
