
using System.Collections.Generic;
using System.Threading;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Utils.Data;
using UnityEngine;

public class SetupMapData : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        if (_md == null)
        {
            _zoneGenService.ShowGenError("Missing MapData");
            return;
        }

        if (_mapProvider.GetMap() == null)
        {
            _zoneGenService.ShowGenError("No world found");
            return;
        }
        if (_mapProvider.GetSpawns() == null || _mapProvider.GetSpawns().Data.Count < 1)
        {
            _mapProvider.SetSpawns(new MapSpawnData() { Id = _mapProvider.GetMap().Id.ToString() });
        }

     

        int mapSize = _mapProvider.GetMap().GetHwid();

        _md.dwid = mapSize;
        _md.dhgt = mapSize;
        _md.ahgt = mapSize;
        _md.awid = mapSize;

        if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId))
        {
            for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
            {
                for (int gy = 0; gy < _mapProvider.GetMap().BlockCount; gy++)
                {
                    _terrainManager.SetupOneTerrainPatch(gx, gy, token);
                }
            }
        }

        if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId))
        {
            _md.grassAmounts = new byte[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt(), MapConstants.MaxGrass];

            _md.mapZoneIds = new short[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];

            _md.subZonePercents = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.subZoneIds = new int[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.overrideZoneScales = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];

            _terrainManager.SetAllTerrainNeighbors();

            _md.alphas = new float[_md.awid, _md.ahgt, MapConstants.MaxTerrainIndex];
            _md.heights = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];


            _md.zoneCenters = new List<MyPoint>();
            _md.mountainHeights = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.nearestMountainTopHeight = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.mountainDistPercent = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.edgeMountainDistPercent = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.mountainCenterDist = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.flags = new int[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            _md.roadDistances = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
            
            _md.mapObjects = new int[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
         
            for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
            {
                for (int y = 0; y < _mapProvider.GetMap().GetHhgt(); y++)
                {
                    _md.mapZoneIds[x, y] = 0;
                    _md.roadDistances[x, y] = MapConstants.InitialRoadDistance;
                    _md.mountainDistPercent[x, y] = 1.0f;
                    _md.edgeMountainDistPercent[x, y] = 1.0f;
                    _md.mountainCenterDist[x, y] = MapConstants.InitialMountainDistance;
                    _md.flags[x, y] = 0;
                }

            }

            for (int x = 0; x < _md.awid; x++)
            {
                for (int y = 0; y < _md.ahgt; y++)
                {
                    _md.alphas[x, y, MapConstants.BaseTerrainIndex] = 1.0f;
                }
            }
        }
        else
        {
            _md.flags = null;
            _md.alphas = null;
            _md.heights = null;
            _md.roadDistances = null;
            _md.roads = null;
            _md.bridgeDistances = null;
        }
    }
}