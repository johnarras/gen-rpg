using Genrpg.LoginServer.Core;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages.LoadIntoMap;
using Genrpg.Shared.MapServer.Entities.MapCache;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.MapServer.Entities;
using System.Linq;
using Genrpg.Shared.Networking.Constants;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.GameSettings;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.ServerShared.CloudComms.Servers.LoginServer;
using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.ServerShared.Purchasing.Services;

namespace Genrpg.LoginServer.CommandHandlers
{
    public class LoadIntoMapHandler : BaseClientCommandHandler<LoadIntoMapCommand>
    {
        private IGameDataService _gameDataService = null;
        private IMapDataService _mapDataService = null;
        private ICloudCommsService _cloudCommsService = null;
        private IPurchasingService _purchasingService = null;

        private static ConcurrentDictionary<string, CachedMap> _mapCache = new ConcurrentDictionary<string, CachedMap>();
        public override async Task Reset()
        {
            _mapCache = new ConcurrentDictionary<string, CachedMap>();
            await Task.CompletedTask;
        }

        protected override async Task InnerHandleMessage(LoginGameState gs, LoadIntoMapCommand command, CancellationToken token)
        {
            FullCachedMap fullCachedMap = await GetCachedMap(gs, command.Env, command.MapId, command.InstanceId, command.GenerateMap);

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
                    ShowError(gs, "Couldn't find map: " + command.MapId);
                    return;
                }
            }

            CoreCharacter coreChar = await _repoService.Load<CoreCharacter>(command.CharId);
            if (coreChar == null)
            {
                ShowError(gs, "Couldn't find new character to load " + command.CharId);
                return;
            }

            if (coreChar.UserId != gs.user.Id)
            {
                ShowError(gs, "You don't own this character");
                return;
            }

            gs.coreCh = coreChar;
            if (gs.coreCh.MapId != command.MapId)
            {
                gs.coreCh.X = fullCachedMap.Map.SpawnX;
                gs.coreCh.Z = fullCachedMap.Map.SpawnY;
            }
            gs.coreCh.Rot = 0;

            gs.ch = new Character(_repoService);
            CharacterUtils.CopyDataFromTo(gs.coreCh, gs.ch);

            List<IUnitData> serverDataList = await _playerDataService.LoadAllPlayerData(gs.rand, gs.ch);

            PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(gs.user, gs.ch, true);

            List<IUnitData> clientDataList = await _playerDataService.MapToClientApi(serverDataList);

            List<IGameSettingsLoader> loaders = _gameDataService.GetAllLoaders();

            string worldDataEnv = _config.DataEnvs[DataCategoryTypes.WorldData];

            if (command.GenerateMap && !string.IsNullOrEmpty(command.WorldDataEnv))
            {
                worldDataEnv = command.WorldDataEnv;
            }

            List<ITopLevelSettings> gameData = _gameDataService.GetClientGameData(gs.ch, false);
            LoadIntoMapResult loadResult = new LoadIntoMapResult()
            {
                Map = SerializationUtils.ConvertType<Map, Map>(fullCachedMap.Map),
                Generating = command.GenerateMap,
                Char = coreChar,
                Host = fullCachedMap.MapInstance?.Host,
                Port = fullCachedMap.MapInstance?.Port ?? 0,
                Serializer = EMapApiSerializers.MessagePack,
                OverrideList = gs.ch.GetOverrideList(),
                GameData = gameData,
                CharData = clientDataList,
                WorldDataEnv = worldDataEnv,
                Stores = offerData,
            };

            gs.user.CurrCharId = gs.coreCh.Id;

            gs.Results.Add(loadResult);

        }
        // This needs to be sent to another server someplace to handle this synchronization and load balancing.
        private async Task<FullCachedMap> GetCachedMap(LoginGameState gs, string env, string mapId, string instanceId, bool generatingMap)
        {
            if (!_mapCache.ContainsKey(mapId))
            {
                Map newMap = await _mapDataService.LoadMap(gs.rand, mapId);
                if (newMap == null || newMap.Zones == null || newMap.Zones.Count < 1)
                {
                    return new FullCachedMap();
                }

                await _mapDataService.SaveMap(_repoService, newMap);

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
                Map = (generatingMap ? cachedMap.FullMap : cachedMap.ClientMap),
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
