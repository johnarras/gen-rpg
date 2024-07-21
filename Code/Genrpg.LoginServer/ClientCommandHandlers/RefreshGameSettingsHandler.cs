using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.PlayerData;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Website.Messages.RefreshGameSettings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.ClientCommandHandlers
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
