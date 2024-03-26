using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.Setup;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.DataStores;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Login.Messages.UploadMap;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using Microsoft.Azure.Amqp.Framing;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.CommandHandlers
{
    public class UploadMapHandler : BaseClientCommandHandler<UploadMapCommand>
    {
        private IMapDataService _mapDataService = null;
        private IMapSpawnDataService _mapSpawnService = null;
        private ICloudCommsService _cloudCommsService = null;    
        
        protected override async Task InnerHandleMessage(LoginGameState gs, UploadMapCommand command, CancellationToken token)
        {
            if (_config.Env == EnvNames.Prod)
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

                IRepositoryService currRepoService = _repoService;
                IServerConfig newConfig = SerializationUtils.SafeMakeCopy(_config);
                newConfig.DataEnvs[DataCategoryTypes.WorldData] = command.WorldDataEnv;

                LoginGameState lgs = await SetupUtils.SetupFromConfig<LoginGameState>(null, _config.ServerId, new LoginSetupService(), token, newConfig);

                await _mapDataService.SaveMap(currRepoService, command.Map);

                await _mapSpawnService.SaveMapSpawnData(currRepoService, command.SpawnData, command.Map.Id, command.Map.MapVersion);

            }
           
            foreach (IClientCommandHandler handler in gs.commandHandlers.Values)
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
