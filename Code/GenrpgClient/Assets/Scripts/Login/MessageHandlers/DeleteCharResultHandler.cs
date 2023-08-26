using Assets.Scripts.Login.Messages.Core;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Login.Messages.DeleteChar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UI.Screens.Constants;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class DeleteCharResultHandler : BaseClientLoginResultHandler<DeleteCharResult>
    {
        IScreenService _screenService;
        protected override void InnerProcess(UnityGameState gs, DeleteCharResult result, CancellationToken token)
        {
            gs.characterStubs = result.AllCharacters;
            ActiveScreen screen = _screenService.GetScreen(gs, ScreenId.CharacterSelect);
            if (screen != null && screen.Screen is CharacterSelectScreen charScreen)
            {
                charScreen.SetupCharacterGrid();
            }
        }
    }
}
