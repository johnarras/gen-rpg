using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.UI.Screens;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Website.Messages.Error;
using System.Collections.Generic;
using System.Threading;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class ErrorResultHandler : BaseClientLoginResultHandler<ErrorResponse>
    {
        private IScreenService _screenService;
        protected override void InnerProcess(ErrorResponse result, CancellationToken token)
        {

            List<ActiveScreen> screens = _screenService.GetAllScreens();

            bool foundErrorScreen = false;

            foreach (ActiveScreen screen in screens)
            {
                if (screen.Screen is ErrorMessageScreen errorScreen)
                {
                    errorScreen.ShowError(result.Error);
                    foundErrorScreen = true;
                }
            }

            if (foundErrorScreen)
            {
                return;
            }

            _screenService.CloseAll();
            _screenService.Open(ScreenId.Login);

            _dispatcher.Dispatch(new ShowFloatingText(result.Error, EFloatingTextArt.Error));

            _logService.Error(result.Error);
        }
    }
}
