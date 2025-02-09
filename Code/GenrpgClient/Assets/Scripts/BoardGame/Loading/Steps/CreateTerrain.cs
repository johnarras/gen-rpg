using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.BoardGame.Loading.Constants;
using Assets.Scripts.MapTerrain;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.ProcGen.Services;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Loading
{
    public class CreateTerrain : BaseLoadBoardStep
    {

        private IMapTerrainManager _terrainManager;
        private IMapProvider _mapProvider;
        private INoiseService _noiseService;
        private IZoneGenService _zoneGenService;
        private IMapGenData _mapGenData;       
        public override ELoadBoardSteps GetKey() { return ELoadBoardSteps.CreateTerrain; }


        public override async Awaitable Execute(BoardData boardData, CancellationToken token)
        {
            Map map = new Map() { Id = boardData.Id };
            map.BlockCount = BoardMapConstants.TerrainBlockCount;

            int width = map.GetHwid();
            int height = map.GetHhgt();

            Zone zone = new Zone()
            {
                IdKey = 1,
                Id = boardData.Id,
                ZoneTypeId = boardData.ZoneTypeId,
                XMin = 0,
                XMax = width,
                ZMin = 0,
                ZMax = height,
                Seed = boardData.Seed + 1,
            };

            map.Zones.Add(zone);

            ZoneType ztype = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(boardData.ZoneTypeId);

            if (ztype == null)
            {
                ztype = _gameData.Get<ZoneTypeSettings>(_gs.ch).GetData().First();
            }


            zone.BaseTextureTypeId = ztype.Textures.FirstOrDefault(x => x.TextureChannelId == MapConstants.BaseTerrainIndex).TextureTypeId;
            zone.RoadTextureTypeId = ztype.Textures.FirstOrDefault(x => x.TextureChannelId == MapConstants.RoadTerrainIndex).TextureTypeId;
            zone.DirtTextureTypeId = ztype.Textures.FirstOrDefault(x => x.TextureChannelId == MapConstants.DirtTerrainIndex).TextureTypeId;
            zone.RockTextureTypeId = ztype.Textures.FirstOrDefault(x => x.TextureChannelId == MapConstants.SteepTerrainIndex).TextureTypeId;

            _mapProvider.SetMap(map);
            _mapProvider.SetSpawns(new MapSpawnData());

            _terrainManager.SetFastLoading();
            _terrainManager.ClearPatches();

            for (int x = 0; x < BoardMapConstants.TerrainBlockCount; x++)
            {
                for (int y = 0; y < BoardMapConstants.TerrainBlockCount; y++)
                {
                    try
                    {
                        await _terrainManager.SetupOneTerrainPatch(x, y, token);
                        TerrainPatchData patch = _terrainManager.GetTerrainPatch(x, y);
                        patch.FullZoneIdList.Add(zone.IdKey);
                        patch.MainZoneIdList.Add(zone.IdKey);

                    }
                    catch (Exception e)
                    {
                        _logService.Exception(e, "CreateTerrain");
                    }
                }
            }

            IRandom rand = new MyRandom(boardData.Seed + 2);

            float[,] finalHeights = new float[map.GetHwid(), map.GetHhgt()];

            for (int times = 0; times < 3; times++)
            {
                float freq = MathUtils.FloatRange(0.005f, 0.01f, rand) * map.GetHwid();
                float amp = MathUtils.FloatRange(0.003f, 0.005f, rand);
                float[,] heights = _noiseService.Generate(2, freq, amp, 2, rand.Next(), map.GetHwid(), map.GetHhgt());
                for (int x = 0; x < heights.GetLength(0); x++)
                {
                    for (int z = 0; z < heights.GetLength(1); z++)
                    {
                        finalHeights[x, z] += heights[x, z] + 0.01f;
                    }
                }

            }
            _zoneGenService.SetAllHeightmaps(finalHeights, token);
            _mapGenData.HaveSetHeights = true;

        }
    }
}
