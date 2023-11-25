using Genrpg.MapServer.Maps;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Core;
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
            List<MapInstance> instances = _mapServerService.GetMapInstances();

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

                List<MapInstance> mapInstances = _mapServerService.GetMapInstances();

                foreach (MapInstance mapInstance in mapInstances)
                {
                    mapInstance.SendAddInstanceMessage();
                }
            }
            else if (message.ServerId == CloudServerNames.Player)
            {
                List<MapInstance> mapInstances = _mapServerService.GetMapInstances();

            }

            await Task.CompletedTask;
        }
    }
}
