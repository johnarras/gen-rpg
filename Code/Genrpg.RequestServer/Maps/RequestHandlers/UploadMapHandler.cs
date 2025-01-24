using Genrpg.RequestServer.ClientUser.RequestHandlers;
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
using Genrpg.Shared.MapServer.WebApi.UploadMap;
using Genrpg.Shared.Utils;

namespace Genrpg.RequestServer.Maps.RequestHandlers
{
    public class UploadMapHandler : BaseClientUserRequestHandler<UploadMapRequest>
    {
        private IMapDataService _mapDataService = null;
        private IMapSpawnDataService _mapSpawnService = null;
        private ICloudCommsService _cloudCommsService = null;

        protected override async Task InnerHandleMessage(WebContext context, UploadMapRequest request, CancellationToken token)
        {
            if (_config.Env == EnvNames.Prod)
            {
                ShowError(context, "Cannot update maps in live");
                return;
            }

            if (request == null)
            {
                ShowError(context, "No map update data sent");
                return;
            }

            if (request.Map == null)
            {
                ShowError(context, "Missing map on update");
                return;
            }

            if (request.Map.Zones == null || request.Map.Zones.Count < 1)
            {
                ShowError(context, "Map had no zones");
                return;
            }

            if (request.SpawnData == null || request.SpawnData.Data == null)
            {
                ShowError(context, "No spawn data sent to server");
                return;
            }

            if (!string.IsNullOrEmpty(request.WorldDataEnv))
            {
                IRepositoryService currRepoService = _repoService;
                IServerConfig newConfig = SerializationUtils.SafeMakeCopy(_config);
                newConfig.DataEnvs[EDataCategories.Worlds.ToString()] = request.WorldDataEnv;

                WebContext newContext = await new ServerSetup().SetupFromConfig<WebContext, WebsiteSetupService>(null, _config.ServerId, token, newConfig);


                IRepositoryService newRepoService = newContext.loc.Get<IRepositoryService>();

                await _mapDataService.SaveMap(newRepoService, request.Map);

                await _mapSpawnService.SaveMapSpawnData(newRepoService, request.SpawnData, request.Map.Id, request.Map.MapVersion);

            }


            await _loginServerService.ResetRequestHandlers();

            MapUploadedAdminMessage mapUploadedMessage = new MapUploadedAdminMessage()
            {
                MapId = request.Map.Id,
                WorldDataEnv = request.WorldDataEnv,
            };

            _cloudCommsService.SendPubSubMessage(mapUploadedMessage);

            context.Responses.Add(new UploadMapResponse());
        }
    }
}
