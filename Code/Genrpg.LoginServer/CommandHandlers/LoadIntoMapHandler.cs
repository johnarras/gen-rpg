﻿using Genrpg.LoginServer.Core;
using Genrpg.MonsterServer.MessageHandlers;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.Entities;
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
using Genrpg.Shared.Constants.TempDev;

namespace Genrpg.LoginServer.CommandHandlers
{
    public class LoadIntoMapHandler : BaseLoginCommandHandler<LoadIntoMapCommand>
    {
        private ConcurrentDictionary<string, CachedMap> _mapCache = new ConcurrentDictionary<string, CachedMap>();
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
                    await gs.repo.Save(fullCachedMap.Map);
                    MapSpawnData mapSpawnData = new MapSpawnData { Id = command.MapId.ToString() };
                    await gs.repo.Save(mapSpawnData);
                }
                else
                {
                    ShowError(gs, "Couldn't find map: " + command.MapId);
                    return;
                }
            }
            Character newChar = await gs.repo.Load<Character>(command.CharId);
            if (newChar == null)
            {
                ShowError(gs, "Couldn't find new character to load " + command.CharId);
                return;
            }

            if (newChar.UserId != gs.user.Id)
            {
                ShowError(gs, "You don't own this character");
                return;
            }

            gs.ch = newChar;

            gs.ch.X = fullCachedMap.Map.SpawnX + 5;
            gs.ch.Z = fullCachedMap.Map.SpawnY + 5;
            LoadIntoMapResult loadResult = new LoadIntoMapResult()
            {
                Map = SerializationUtils.ConvertType<Map, Map>(fullCachedMap.Map),
                Generating = command.GenerateMap,
                Char = SerializationUtils.ConvertType<Character, Character>(gs.ch),
                Host = fullCachedMap.MapInstance?.Host,
                Port = fullCachedMap.MapInstance?.Port ?? 0,
                Serializer = EMapApiSerializers.MessagePack,
            };

            List<IUnitData> serverDataList = await PlayerDataUtils.LoadPlayerData(gs, gs.ch);

            List<IUnitData> clientDataList = await PlayerDataUtils.MapToClientApi(serverDataList);

            loadResult.CharData = clientDataList;

            gs.Results.Add(loadResult);

        }
        // This needs to be sent to another server someplace to handle this synchronization and load balancing.
        private async Task<FullCachedMap> GetCachedMap(LoginGameState gs, string env, string mapId, string instanceId, bool generatingMap)
        {
            if (!_mapCache.ContainsKey(mapId))
            {
                IMapDataService mds = gs.loc.Get<IMapDataService>();
                Map newMap = await mds.LoadMap(gs, mapId);
                if (newMap == null || newMap.Zones == null || newMap.Zones.Count < 1)
                {
                    return new FullCachedMap();
                }

                await mds.SaveMap(gs, newMap);

                CachedMap newCachedMap = new CachedMap()
                {
                    FullMap = newMap,
                };

                Map clientMap = SerializationUtils.SafeMakeCopy(newMap);
                clientMap.CleanForClient();
                newCachedMap.ClientMap = clientMap;

                _mapCache.TryAdd(mapId, newCachedMap);
            }

            CachedMap cachedMap = _mapCache[mapId];

            CachedMapInstance mapInstance = cachedMap.Instances.FirstOrDefault(x => x.InstanceId == instanceId);
            if (mapInstance == null)
            {
                // Need to really get this from some instance service

                mapInstance = new CachedMapInstance();

                if (env == EnvNames.Local)
                {
                    mapInstance.Host = TempDevConstants.LocalIP;
                }
                else if (env == EnvNames.Dev || env == EnvNames.Test || env == EnvNames.Prod)
                {
                    mapInstance.Host = TempDevConstants.TestVMIP;
                }

                if (int.TryParse(mapId, out int mapIdInt))
                {
                    mapInstance.Port = 4000 + mapIdInt;
                }
                cachedMap.Instances.Add(mapInstance);

            }

            FullCachedMap fullMap = new FullCachedMap()
            {
                Map = (generatingMap ? cachedMap.FullMap : cachedMap.ClientMap),
                MapInstance = mapInstance,
            };

            return fullMap;
        }

    }
}