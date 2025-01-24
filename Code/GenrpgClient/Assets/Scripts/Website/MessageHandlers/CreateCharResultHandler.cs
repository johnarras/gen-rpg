using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Characters.WebApi.CreateChar;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
using System.Threading;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class CreateCharResultHandler : BaseClientLoginResultHandler<CreateCharResponse>
    {
        private IScreenService _screenService;
        protected override void InnerProcess(CreateCharResponse result, CancellationToken token)
        {
            _gs.characterStubs = result.AllCharacters;
            _screenService.Open(ScreenId.CharacterSelect);
            _screenService.Close(ScreenId.CharacterCreate);
        }
    }
}
