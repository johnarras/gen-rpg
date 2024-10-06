using Genrpg.Shared.MapServer.Entities.MapCache;
using Genrpg.Shared.Utils;
using System.Collections.Concurrent;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Networking.Constants;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.WebServer;
using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.ServerShared.Purchasing.Services;
using Genrpg.Shared.Website.Messages.LoadIntoMap;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Website.Messages.Login;

namespace Genrpg.RequestServer.ClientCommands
{
    public class LoadIntoMapHandler : BaseClientCommandHandler<LoadIntoMapCommand>
    {
        private IGameDataService _gameDataService = null;
        private IMapDataService _mapDataService = null;
        private ICloudCommsService _cloudCommsService = null;
        private IPurchasingService _purchasingService = null;

        private ConcurrentDictionary<string, CachedMap> _mapCache = new ConcurrentDictionary<string, CachedMap>();
        public override async Task Reset()
        {
            _mapCache = new ConcurrentDictionary<string, CachedMap>();
            await Task.CompletedTask;
        }

        protected override async Task InnerHandleMessage(WebContext context, LoadIntoMapCommand command, CancellationToken token)
        {
            FullCachedMap fullCachedMap = await GetCachedMap(context, command.Env, command.MapId, command.InstanceId, command.GenerateMap);

            // Check case where the map doesn't exist, if not create that map.
            if (fullCachedMap == null || fullCachedMap.Map == null ||
                fullCachedMap.Map.Zones == null)
            {
                long mapId = -1;
                long.TryParse(command.MapId, out mapId);
                if (command.GenerateMap)
                {
                    fullCachedMap.Map = new Map() { Id = command.MapId };
                }
                else
                {
                    ShowError(context, "Couldn't find map: " + command.MapId);
                    return;
                }
            }

            CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(command.CharId);
            if (coreCh == null)
            {
                ShowError(context, "Couldn't find new character to load " + command.CharId);
                return;
            }

            if (coreCh.UserId != context.user.Id)
            {
                ShowError(context, "You don't own this character");
                return;
            }
            if (coreCh.MapId != command.MapId)
            {
                coreCh.X = fullCachedMap.Map.SpawnX;
                coreCh.Z = fullCachedMap.Map.SpawnY;
                context.Set(coreCh);
            }

            Character ch = new Character(_repoService);
            CharacterUtils.CopyDataFromTo(coreCh, ch);

            List<IUnitData> serverDataList = await _loginPlayerDataService.LoadPlayerDataOnLogin(context, ch);

            PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(context.user, true);

            List<IUnitData> clientDataList = await _playerDataService.MapToClientApi(serverDataList);

            List<IGameSettingsLoader> loaders = _gameDataService.GetAllLoaders();

            string worldDataEnv = _config.DataEnvs[EDataCategories.Worlds.ToString()];

            if (command.GenerateMap && !string.IsNullOrEmpty(command.WorldDataEnv))
            {
                worldDataEnv = command.WorldDataEnv;
            }

            List<ITopLevelSettings> gameData = _gameDataService.GetClientGameData(ch, false);
            gameData = _gameDataService.MapToApi(context.user, gameData);
            LoadIntoMapResult loadResult = new LoadIntoMapResult()
            {
                Map = SerializationUtils.ConvertType<Map, Map>(fullCachedMap.Map),
                Generating = command.GenerateMap,
                Char = coreCh,
                Host = fullCachedMap.MapInstance?.Host,
                Port = fullCachedMap.MapInstance?.Port ?? 0,
                Serializer = EMapApiSerializers.MessagePack,
                DataOverrides = ch.DataOverrides,
                GameData = gameData,
                CharData = clientDataList,
                WorldDataEnv = worldDataEnv,
                Stores = offerData,
            };

            context.user.CurrCharId = coreCh.Id;

            context.Results.Add(loadResult);

            PublicCharacter publicChar = new PublicCharacter()
            {
                Id = coreCh.Id,
                Name = coreCh.Name,
                FactionTypeId = coreCh.FactionTypeId,
                SexTypeId = coreCh.SexTypeId,
                UnitTypeId = coreCh.EntityId
            };

            _repoService.QueueSave(publicChar);

        }
        // This needs to be sent to another server someplace to handle this synchronization and load balancing.
        private async Task<FullCachedMap> GetCachedMap(WebContext context, string env, string mapId, string instanceId, bool generatingMap)
        {
            if (!_mapCache.ContainsKey(mapId))
            {
                Map newMap = await _mapDataService.LoadMap(context.rand, mapId);
                if (newMap == null || newMap.Zones == null || newMap.Zones.Count < 1)
                {
                    return new FullCachedMap();
                }
                CachedMap newCachedMap = new CachedMap()
                {
                    FullMap = newMap,
                };

                Map clientMap = SerializationUtils.SafeMakeCopy(newMap);
                clientMap.CleanForClient();
                newCachedMap.ClientMap = clientMap;

                _mapCache.TryAdd(mapId, newCachedMap);
            }

            GetInstanceQueueResponse response = await _cloudCommsService.SendResponseMessageAsync<GetInstanceQueueResponse>(CloudServerNames.Instance, new GetInstanceQueueRequest() { MapId = mapId });

            if (!generatingMap && (response == null || !string.IsNullOrEmpty(response.ErrorText)))
            {
                return new FullCachedMap();
            }

            CachedMap cachedMap = _mapCache[mapId];

            FullCachedMap fullMap = new FullCachedMap()
            {
                Map = generatingMap ? cachedMap.FullMap : cachedMap.ClientMap,
            };

            if (!generatingMap)
            {
                fullMap.MapInstance = new CachedMapInstance()
                {
                    Host = response.Host,
                    Port = response.Port,
                    InstanceId = response.InstanceId,
                };
            }

            return fullMap;
        }

    }
}
