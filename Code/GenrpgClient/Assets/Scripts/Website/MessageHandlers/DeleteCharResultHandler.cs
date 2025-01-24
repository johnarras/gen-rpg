using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Characters.WebApi.DeleteChar;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
using System.Threading;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class DeleteCharResultHandler : BaseClientLoginResultHandler<DeleteCharResponse>
    {
        IScreenService _screenService;
        protected override void InnerProcess(DeleteCharResponse result, CancellationToken token)
        {
            _gs.characterStubs = result.AllCharacters;
            ActiveScreen screen = _screenService.GetScreen(ScreenId.CharacterSelect);
            if (screen != null && screen.Screen is CharacterSelectScreen charScreen)
            {
                charScreen.SetupCharacterGrid();
            }
        }
    }
}
