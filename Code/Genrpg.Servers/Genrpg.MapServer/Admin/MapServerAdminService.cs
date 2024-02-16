using Genrpg.MapServer.Maps;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Inventory.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Services.Maps
{
    public class MapServerAdminService : BaseAdminService, IAdminService
    {
        private IMapServerService _mapServerService = null;

        public override async Task HandleReloadGameState(ServerGameState gs)
        {
            await base.HandleReloadGameState(gs);
            IReadOnlyList<MapInstance> instances = _mapServerService.GetMapInstances();

            foreach (MapInstance instance in instances)
            {
                instance.RefreshGameData();
            }
        }

        public override async Task OnServerStarted(ServerGameState gs, ServerStartedAdminMessage message)
        {
            if (message.ServerId == CloudServerNames.Instance)
            {
                _mapServerService.SendAddMapServerMessage();

                IReadOnlyList<MapInstance> mapInstances = _mapServerService.GetMapInstances();

                foreach (MapInstance mapInstance in mapInstances)
                {
                    mapInstance.SendAddInstanceMessage();
                }
            }
            else if (message.ServerId == CloudServerNames.Player)
            {
                IReadOnlyList<MapInstance> mapInstances = _mapServerService.GetMapInstances();

                foreach (MapInstance mapInstance in mapInstances)
                {
                    mapInstance.SendAllPlayerEnterMapMessages();
                }

            }

            await Task.CompletedTask;
        }
        
        public override async Task OnMapUploaded(ServerGameState gs, MapUploadedAdminMessage message)
        {

            if (message.WorldDataEnv != gs.config.DataEnvs[DataCategoryTypes.WorldData])
            {
                return;
            }
            await _mapServerService.RestartMapsWithId(message.MapId);


        }
    }
}
