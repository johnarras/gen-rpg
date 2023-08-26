using Assets.Scripts.Login.Messages.Core;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Login.Messages.CreateChar;
using Genrpg.Shared.Login.Messages.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UI.Screens.Constants;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class ErrorResultHandler : BaseClientLoginResultHandler<ErrorResult>
    {
        private IScreenService _screenService;
        protected override void InnerProcess(UnityGameState gs, ErrorResult result, CancellationToken token)
        {
            FloatingTextScreen.Instance.ShowError(result.Error);
            _screenService.CloseAll(gs);
            _screenService.Open(gs, ScreenId.Login);
        }
    }
}
