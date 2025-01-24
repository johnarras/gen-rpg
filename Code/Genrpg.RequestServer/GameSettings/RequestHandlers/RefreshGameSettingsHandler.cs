using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.GameSettings.WebApi.RefreshGameSettings;
using Genrpg.RequestServer.ClientUser.RequestHandlers;

namespace Genrpg.RequestServer.GameSettings.RequestHandlers
{
    public class RefreshGameSettingsHandler : BaseClientUserRequestHandler<RefreshGameSettingsRequest>
    {
        private IGameDataService _gameDataService = null;

        protected override async Task InnerHandleMessage(WebContext context, RefreshGameSettingsRequest request, CancellationToken token)
        {
            CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(request.CharId);

            RefreshGameSettingsResponse response = _gameDataService.GetNewGameDataUpdates(coreCh, true);

            if (response != null)
            {
                context.Responses.Add(response);
            }

            await Task.CompletedTask;
        }
    }
}
