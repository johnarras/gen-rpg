using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Website.Messages.CreateChar;
using System.Threading;

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
