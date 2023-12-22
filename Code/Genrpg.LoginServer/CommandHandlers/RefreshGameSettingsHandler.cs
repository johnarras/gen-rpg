using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.PlayerData;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Login.Messages.RefreshGameSettings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.CommandHandlers
{
    public class RefreshGameSettingsHandler : BaseLoginCommandHandler<RefreshGameSettingsCommand>
    {
        private IGameDataService _gameDataService = null;

        protected override async Task InnerHandleMessage(LoginGameState gs, RefreshGameSettingsCommand command, CancellationToken token)
        {

            RefreshGameSettingsResult result = _gameDataService.GetNewGameDataUpdates(gs, gs.ch, true);

            if (result != null)
            {
                gs.Results.Add(result);
            }

            await Task.CompletedTask;
        }
    }
}
