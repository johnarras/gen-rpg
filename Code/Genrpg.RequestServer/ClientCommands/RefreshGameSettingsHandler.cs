using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Website.Messages.RefreshGameSettings;
using Genrpg.RequestServer.Core;

namespace Genrpg.RequestServer.ClientCommands
{
    public class RefreshGameSettingsHandler : BaseClientCommandHandler<RefreshGameSettingsCommand>
    {
        private IGameDataService _gameDataService = null;

        protected override async Task InnerHandleMessage(WebContext context, RefreshGameSettingsCommand command, CancellationToken token)
        {
            CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(command.CharId);

            RefreshGameSettingsResult result = _gameDataService.GetNewGameDataUpdates(coreCh, true);

            if (result != null)
            {
                context.Results.Add(result);
            }

            await Task.CompletedTask;
        }
    }
}
