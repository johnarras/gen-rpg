using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Login.Messages.CreateChar;
using System.Threading;
using UI.Screens.Constants;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class CreateCharResultHandler : BaseClientLoginResultHandler<CreateCharResult>
    {
        private IScreenService _screenService;
        protected override void InnerProcess(CreateCharResult result, CancellationToken token)
        {
            _gs.characterStubs = result.AllCharacters;
            _screenService.Open(ScreenId.CharacterSelect);
            _screenService.Close(ScreenId.CharacterCreate);
        }
    }
}
