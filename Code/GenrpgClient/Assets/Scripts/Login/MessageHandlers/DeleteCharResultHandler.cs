using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Website.Messages.DeleteChar;
using System.Threading;
using UI.Screens.Constants;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class DeleteCharResultHandler : BaseClientLoginResultHandler<DeleteCharResult>
    {
        IScreenService _screenService;
        protected override void InnerProcess(DeleteCharResult result, CancellationToken token)
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
