using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.GameSettings.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Services.Admin
{
    public class BaseAdminService : IAdminService
    {
        protected IGameDataService _gameDataService = null;

        virtual public async Task HandleReloadGameState(ServerGameState gs)
        {
            await _gameDataService.ReloadGameData(gs);
        }

        public virtual async Task OnServerStarted(ServerGameState gs, ServerStartedAdminMessage message)
        {
            await Task.CompletedTask;
        }
    }
}
