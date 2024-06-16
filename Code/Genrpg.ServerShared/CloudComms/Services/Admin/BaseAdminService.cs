using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Services.Admin
{
    public class BaseAdminService : IAdminService
    {
        protected IGameDataService _gameDataService = null;
        protected IServerConfig _config = null;

        virtual public async Task HandleReloadGameState()
        {
            await _gameDataService.ReloadGameData();
        }

        public virtual async Task OnMapUploaded(MapUploadedAdminMessage message)
        {
            await Task.CompletedTask;
        }

        public virtual async Task OnServerStarted(ServerStartedAdminMessage message)
        {
            await Task.CompletedTask;
        }
    }
}
