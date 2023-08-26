using Assets.Scripts.Login.Messages.Core;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Login.Messages.CreateChar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UI.Screens.Constants;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class CreateCharResultHandler : BaseClientLoginResultHandler<CreateCharResult>
    {
        private IScreenService _screenService;
        protected override void InnerProcess(UnityGameState gs, CreateCharResult result, CancellationToken token)
        {
            gs.characterStubs = result.AllCharacters;
            _screenService.Open(gs, ScreenId.CharacterSelect);
            _screenService.Close(gs, ScreenId.CharacterCreate);
        }
    }
}
