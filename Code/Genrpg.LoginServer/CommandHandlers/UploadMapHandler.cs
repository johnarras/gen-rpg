using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.DataStores;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.Login.Messages.UploadMap;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using Microsoft.Azure.Amqp.Framing;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.CommandHandlers
{
    public class UploadMapHandler : BaseLoginCommandHandler<UploadMapCommand>
    {
        private IMapDataService _mapDataService = null;
        private IMapSpawnDataService _mapSpawnService = null;
        private ICloudCommsService _cloudCommsService = null;
        protected override async Task InnerHandleMessage(LoginGameState gs, UploadMapCommand command, CancellationToken token)
        {
            if (gs.config.Env == EnvNames.Prod)
            {
                ShowError(gs, "Cannot update maps in live");
                return;
            }

            if (command == null)
            {
                ShowError(gs, "No map update data sent");
                return;
            }

            if (command.Map == null)
            {
                ShowError(gs, "Missing map on update");
                return;
            }

            if (command.Map.Zones == null || command.Map.Zones.Count < 1)
            {
                ShowError(gs, "Map had no zones");
                return;
            }

            if (command.SpawnData == null || command.SpawnData.Data == null)
            {
                ShowError(gs, "No spawn data sent to server");
                return;
            }

            if (!string.IsNullOrEmpty(command.WorldDataEnv))
            {
                ServerConfig newConfig = SerializationUtils.SafeMakeCopy(gs.config);
                newConfig.DataEnvs[DataCategoryTypes.WorldData] = command.WorldDataEnv;
                gs.repo = new ServerRepositorySystem(gs.logger, newConfig.DataEnvs, newConfig.ConnectionStrings, token);
            }
           
            await _mapDataService.SaveMap(gs, command.Map);

            await _mapSpawnService.SaveMapSpawnData(gs, command.SpawnData, command.Map.Id, command.Map.MapVersion);
          
            foreach (ILoginCommandHandler handler in gs.commandHandlers.Values)
            {
                await handler.Reset();
            }

            MapUploadedAdminMessage mapUploadedMessage = new MapUploadedAdminMessage()
            {
                MapId = command.Map.Id,
                WorldDataEnv = command.WorldDataEnv,
            };

            _cloudCommsService.SendPubSubMessage(gs, mapUploadedMessage);

            gs.Results.Add(new UploadMapResult());
        }
    }
}
