
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Zones.Entities;

using System.Threading;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Utils.Data;

public class SetupMapData : BaseZoneGenerator
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        if (gs.md == null)
        {
            _zoneGenService.ShowGenError(gs, "Missing MapData");
            return;
        }

        if (gs.map == null)
        {
            _zoneGenService.ShowGenError(gs, "No world found");
            return;
        }
        if (gs.spawns == null || gs.spawns.Data.Count < 1)
        {
            gs.spawns = new MapSpawnData() { Id = gs.map.Id.ToString() };
        }

        bool fixSeeds = false;

#if UNITY_EDITOR

        InitClient initComp = InitClient.EditorInstance;

        
        if (initComp != null &&
            initComp.WorldSize >= 3 &&
            initComp.ZoneSize >= 1 &&
            initComp.MapGenSeed > 0)
        {
            fixSeeds = true;
            if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId))
            {
                gs.map.BlockCount = initComp.WorldSize;
                gs.map.ZoneSize = initComp.ZoneSize;
                gs.map.Seed = initComp.MapGenSeed;
            }
        }

#endif
        if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId) && !fixSeeds)
        {
            gs.map.Seed = (int)(DateTime.UtcNow.Ticks % 2000000000);
            foreach (Zone item in gs.map.Zones)
            {
                item.Seed = (int)(DateTime.UtcNow.Ticks % 1000000000+item.IdKey *235622);
            }
        }

        _terrainManager.ClearPatches(gs);

        int mapSize = gs.map.GetHwid();

        gs.md.dwid = mapSize;
        gs.md.dhgt = mapSize;
        gs.md.ahgt = mapSize;
        gs.md.awid = mapSize;



        if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId))
        {
            for (int gx = 0; gx < gs.map.BlockCount; gx++)
            {
                for (int gy = 0; gy < gs.map.BlockCount; gy++)
                {
                    _terrainManager.SetupOneTerrainPatch(gs, gx, gy, token).Forget();
                }
            }
        }

        if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId))
        {
            gs.md.grassAmounts = new byte[gs.map.GetHwid(), gs.map.GetHhgt(), MapConstants.MaxGrass];

            gs.md.mapZoneIds = new short[gs.map.GetHwid(), gs.map.GetHhgt()];

            gs.md.subZonePercents = new float[gs.map.GetHwid(), gs.map.GetHhgt()];
            gs.md.subZoneIds = new int[gs.map.GetHwid(), gs.map.GetHhgt()];
            gs.md.overrideZoneScales = new float[gs.map.GetHwid(), gs.map.GetHhgt()];

            _terrainManager.SetAllTerrainNeighbors(gs);

            gs.md.alphas = new float[gs.md.awid, gs.md.ahgt, MapConstants.MaxTerrainIndex];
            gs.md.heights = new float[gs.map.GetHwid(), gs.map.GetHhgt()];


            gs.md.zoneCenters = new List<MyPoint>();
            gs.md.mountainHeights = new float[gs.map.GetHwid(), gs.map.GetHhgt()];
            gs.md.nearestMountainTopHeight = new float[gs.map.GetHwid(), gs.map.GetHhgt()];
            gs.md.mountainDistPercent = new float[gs.map.GetHwid(), gs.map.GetHhgt()];
            gs.md.edgeMountainDistPercent = new float[gs.map.GetHwid(), gs.map.GetHhgt()];
            gs.md.mountainCenterDist = new float[gs.map.GetHwid(), gs.map.GetHhgt()];
            gs.md.flags = new int[gs.map.GetHwid(), gs.map.GetHhgt()];
            gs.md.roadDistances = new float[gs.map.GetHwid(), gs.map.GetHhgt()];
            
            gs.md.mapObjects = new int[gs.map.GetHwid(), gs.map.GetHhgt()];
         
            for (int x = 0; x < gs.map.GetHwid(); x++)
            {
                for (int y = 0; y < gs.map.GetHhgt(); y++)
                {
                    gs.md.mapZoneIds[x, y] = 0;
                    gs.md.roadDistances[x, y] = MapConstants.InitialRoadDistance;
                    gs.md.mountainDistPercent[x, y] = 1.0f;
                    gs.md.edgeMountainDistPercent[x, y] = 1.0f;
                    gs.md.mountainCenterDist[x, y] = MapConstants.InitialMountainDistance;
                    gs.md.flags[x, y] = 0;
                }

            }

            for (int x = 0; x < gs.md.awid; x++)
            {
                for (int y = 0; y < gs.md.ahgt; y++)
                {
                    gs.md.alphas[x, y, MapConstants.BaseTerrainIndex] = 1.0f;
                }
            }
        }
        else
        {
            gs.md.flags = null;
            gs.md.alphas = null;
            gs.md.heights = null;
            gs.md.roadDistances = null;
            gs.md.roads = null;
            gs.md.bridgeDistances = null;
        }
    }
}