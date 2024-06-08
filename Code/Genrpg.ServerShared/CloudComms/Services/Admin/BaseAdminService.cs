using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Core.Entities;
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

        public async Task Initialize(IGameState gs, CancellationToken toke)
        {
            await Task.CompletedTask;
        }


        protected IGameDataService _gameDataService = null;
        protected IServerConfig _config = null;

        virtual public async Task HandleReloadGameState(ServerGameState gs)
        {
            await _gameDataService.ReloadGameData();
        }

        public virtual async Task OnMapUploaded(ServerGameState gs, MapUploadedAdminMessage message)
        {
            await Task.CompletedTask;
        }

        public virtual async Task OnServerStarted(ServerGameState gs, ServerStartedAdminMessage message)
        {
            await Task.CompletedTask;
        }
    }
}
