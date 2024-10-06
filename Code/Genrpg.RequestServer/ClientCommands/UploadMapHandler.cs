using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Setup;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages.UploadMap;

namespace Genrpg.RequestServer.ClientCommands
{
    public class UploadMapHandler : BaseClientCommandHandler<UploadMapCommand>
    {
        private IMapDataService _mapDataService = null;
        private IMapSpawnDataService _mapSpawnService = null;
        private ICloudCommsService _cloudCommsService = null;

        protected override async Task InnerHandleMessage(WebContext context, UploadMapCommand command, CancellationToken token)
        {
            if (_config.Env == EnvNames.Prod)
            {
                ShowError(context, "Cannot update maps in live");
                return;
            }

            if (command == null)
            {
                ShowError(context, "No map update data sent");
                return;
            }

            if (command.Map == null)
            {
                ShowError(context, "Missing map on update");
                return;
            }

            if (command.Map.Zones == null || command.Map.Zones.Count < 1)
            {
                ShowError(context, "Map had no zones");
                return;
            }

            if (command.SpawnData == null || command.SpawnData.Data == null)
            {
                ShowError(context, "No spawn data sent to server");
                return;
            }

            if (!string.IsNullOrEmpty(command.WorldDataEnv))
            {
                IRepositoryService currRepoService = _repoService;
                IServerConfig newConfig = SerializationUtils.SafeMakeCopy(_config);
                newConfig.DataEnvs[EDataCategories.Worlds.ToString()] = command.WorldDataEnv;

                WebContext newContext = await new ServerSetup().SetupFromConfig<WebContext, WebsiteSetupService>(null, _config.ServerId, token, newConfig);


                IRepositoryService newRepoService = newContext.loc.Get<IRepositoryService>();

                await _mapDataService.SaveMap(newRepoService, command.Map);

                await _mapSpawnService.SaveMapSpawnData(newRepoService, command.SpawnData, command.Map.Id, command.Map.MapVersion);

            }


            await _loginServerService.ResetCommandHandlers();

            MapUploadedAdminMessage mapUploadedMessage = new MapUploadedAdminMessage()
            {
                MapId = command.Map.Id,
                WorldDataEnv = command.WorldDataEnv,
            };

            _cloudCommsService.SendPubSubMessage(mapUploadedMessage);

            context.Results.Add(new UploadMapResult());
        }
    }
}
