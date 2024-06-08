using System;
using System.Collections.Generic;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Zones.Entities;
using Assets.Scripts.MapTerrain;
using UnityEngine;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.ProcGen.Settings.Trees;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using System.Linq;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Interfaces;

public interface IMapGenData : IInjectable
{

    // Alphamap width and height
     int awid { get; set; }
     int ahgt { get; set; }

    // Detail map width and height
     int dwid { get; set; }
     int dhgt { get; set; }

     byte[,,] grassAmounts { get; set; }

    // heightmap
     float[,] heights { get; set; }
     float[,] subZonePercents { get; set; }
     int[,] subZoneIds { get; set; }
     float[,] overrideZoneScales { get; set; }
    // splatmaps
     float[,,] alphas { get; set; }

     float[,] roadDistances { get; set; }


    // List of roads created
     List<List<MyPointF>> roads { get; set; }

     List<MyPointF> creviceBridges { get; set; }
    // Bridges that have been made
     List<MyPointF> currBridges { get; set; }

     ushort[,] bridgeDistances { get; set; }


     float[,] mountainNoise { get; set; }
     float[,] mountainDecayPower { get; set; }

     List<Location>[,] locationGrid { get; set; }

    // Ends of ramps where special monsters or quests can be placed.
     List<MyPoint> rampTops { get; set; }

     float[,] creviceDepths { get; set; }

     int[,] flags { get; set; }

     short[,] mapZoneIds { get; set; }
     List<MyPoint> zoneCenters { get; set; }
     List<ConnectedPairData> zoneConnections { get; set; }
     float[,] mountainHeights { get; set; }
     float[,] nearestMountainTopHeight { get; set; }
     float[,] mountainCenterDist { get; set; }
     float[,] mountainDistPercent { get; set; }
     float[,] edgeMountainDistPercent { get; set; }
     int[,] mapObjects { get; set; }
     List<int[]> wallEndpoints { get; set; }


     Dictionary<int, List<int>> zoneAdjacencies { get; set; }



     List<GenZone> GenZones { get; set; }



    // Have we copied the heightmap data into the TerrainData?
     bool HaveSetHeights { get; set; }
    // Have we copied the splatmaps data into the TerrainData?
     bool HaveSetAlphaSplats { get; set; } 

     bool GeneratingMap { get; set; }

     Dictionary<long, List<long>> zoneTreeIds { get; set; }
     Dictionary<long, List<long>> zoneBushIds { get; set; }

    void ClearGenerationData();


    void ClearAlphasAt(int x, int z);

    GenZone GetGenZone(long zoneId);

    float GetAverageHeightNear(Map map, int hx, int hy, int radius, int terrainType = -1);

    float GetAverageSplatNear(int x, int y, int radius, int channel);

    float EdgeHeightmapAdjustPercent(Map map, int x, int y);
    void AddMapLocation(IMapProvider _mapProvider, Location loc);
    float GetMountainDefaultSize(Map map);
}


public class MapGenData : IMapGenData
{

    // Alphamap width and height
    public int awid { get; set; }
    public int ahgt { get; set; }

    // Detail map width and height
    public int dwid { get; set; }
    public int dhgt { get; set; }

    public byte[,,] grassAmounts { get; set; }

    // heightmap
    public float[,] heights { get; set; }
    public float[,] subZonePercents { get; set; }
    public int[,] subZoneIds { get; set; }
    public float[,] overrideZoneScales { get; set; }
    // splatmaps
    public float[,,] alphas { get; set; }

    public float[,] roadDistances { get; set; }


    // List of roads created
    public List<List<MyPointF>> roads { get; set; }

    public List<MyPointF> creviceBridges { get; set; }
    // Bridges that have been made
    public List<MyPointF> currBridges { get; set; }

    public ushort[,] bridgeDistances { get; set; }


    public float[,] mountainNoise { get; set; }
    public float[,] mountainDecayPower { get; set; }

    public List<Location>[,] locationGrid { get; set; }

    // Ends of ramps where special monsters or quests can be placed.
    public List<MyPoint> rampTops { get; set; }

    public float[,] creviceDepths { get; set; }

    public int[,] flags { get; set; }

    public short[,] mapZoneIds { get; set; }
    public List<MyPoint> zoneCenters { get; set; }
    public List<ConnectedPairData> zoneConnections { get; set; }
    public float[,] mountainHeights { get; set; }
    public float[,] nearestMountainTopHeight { get; set; }
    public float[,] mountainCenterDist { get; set; }
    public float[,] mountainDistPercent { get; set; }
    public float[,] edgeMountainDistPercent { get; set; }
    public int[,] mapObjects { get; set; }
    public List<int[]> wallEndpoints { get; set; }


    public Dictionary<int, List<int>> zoneAdjacencies { get; set; } = new Dictionary<int, List<int>>();



    public List<GenZone> GenZones { get; set; } = new List<GenZone>();



    // Have we copied the heightmap data into the TerrainData?
    public bool HaveSetHeights { get; set; } = false;
    // Have we copied the splatmaps data into the TerrainData?
    public bool HaveSetAlphaSplats { get; set; } = false;

    public bool GeneratingMap { get; set; } = false;

    public Dictionary<long, List<long>> zoneTreeIds { get; set; } = null;
    public Dictionary<long, List<long>> zoneBushIds { get; set; } = null;

    public virtual void ClearGenerationData()
    {

        grassAmounts = null;
        heights = null;
        alphas = null;
        roadDistances = null;
        roads = null;
        creviceBridges = null;
        currBridges = null;
        bridgeDistances = null;
        locationGrid = null;
        rampTops = null;
        creviceDepths = null;
        flags = null;
        mapZoneIds = null;
        zoneCenters = null;
        mountainHeights = null;
        mountainDistPercent = null;
        edgeMountainDistPercent = null;
        wallEndpoints = null;
        mapObjects = null;
        zoneTreeIds = null;
        zoneBushIds = null;
    }

    public MapGenData()
    {
    }


    
  

 
    public void ClearAlphasAt(int x, int z)
    {
        if (x < 0 || z < 0 || x >= awid || z >= ahgt)
        {
            return;
        }

        for (int c = 0; c < MapConstants.MaxTerrainIndex; c++)
        {
            alphas[x, z, c] *= 0;
        }
    }

    public GenZone GetGenZone(long zoneId)
    {
        GenZone genZone = GenZones.FirstOrDefault(x => x.IdKey == zoneId);
        if (genZone == null)
        {
            genZone = new GenZone() { IdKey = zoneId };
            GenZones.Add(genZone);
        }
        return genZone;
    }

    public float GetAverageHeightNear(Map map, int hx, int hy, int radius, int terrainType = -1)
    {
        if (heights == null)
        {
            return -1;
        }

        if (HaveSetHeights)
        {
            throw new Exception("You have already set the heights in the heightmap");
        }

        if (radius < 0)
        {
            radius = 0;
        }

        float totalHeight = 0;
        int totalCells = 0;

        for (int x = hx - radius; x <= hx + radius; x++)
        {
            if (x < 0 || x >= map.GetHwid() || x >= awid)
            {
                continue;
            }
            for (int y = hy - radius; y <= hy + radius; y++)
            {
                if (y < 0 || y >= map.GetHhgt() || y >= map.GetHhgt())
                {
                    continue;
                }
                if (terrainType < 0 || terrainType < alphas.Length && alphas[x, y, terrainType] > 0)
                {
                    totalHeight += heights[x, y];
                    totalCells++;
                }
            }
        }

        if (totalCells < 1)
        {
            return -1;
        }
        return totalHeight / totalCells;

    }

    public float GetAverageSplatNear(int x, int y, int radius, int channel)
    {
        if (alphas == null || radius < 1 || channel < 0 || channel >= MapConstants.MaxTerrainIndex)
        {
            return 0.0f;
        }

        float[,,] alphas2 = alphas;
        int minx = Math.Max(0, x - radius);
        int maxx = Math.Min(alphas2.GetLength(0) - 1, x + radius);
        int miny = Math.Max(0, y - radius);
        int maxy = Math.Min(alphas2.GetLength(1) - 1, y + radius);

        float totalDirt = 0.0f;
        int cellCount = 0;

        for (int xx = minx; xx <= maxx; xx++)
        {
            for (int yy = miny; yy <= maxy; yy++)
            {
                totalDirt += alphas2[xx, yy, channel];
                cellCount++;

            }
        }

        if (cellCount < 1)
        {
            return 0.0f;
        }

        return totalDirt / cellCount;
    }

    public float EdgeHeightmapAdjustPercent(Map map, int x, int y)
    {
        if (x < 0 || y < 0 || x >= map.GetHwid() || y > map.GetHhgt())
        {
            return 0.0f;
        }

        int edgeSize = MapConstants.MapEdgeSize;
        if (edgeSize > map.GetHwid() / 2)
        {
            edgeSize = map.GetHwid() / 2;
        }

        if (x > edgeSize && x < map.GetHwid() - edgeSize && y > edgeSize && y < map.GetHhgt() - edgeSize)
        {
            return 1.0f;
        }



        int minDistX = Math.Min(x, map.GetHwid() - x);
        int minDistY = Math.Min(y, map.GetHhgt() - y);
        int minDist = Math.Min(minDistX, minDistY);

        return 1.0f * minDist / edgeSize;
    }

    protected int locationCount = 0;
    public void AddMapLocation(IMapProvider _mapProvider, Location loc)
    {
        if (loc == null)
        {
            return;
        }
        if (locationGrid == null)
        {
            locationGrid = new List<Location>[MapConstants.MaxTerrainGridSize, MapConstants.MaxTerrainGridSize];
            for (int x = 0; x < MapConstants.MaxTerrainGridSize; x++)
            {
                for (int y = 0; y < MapConstants.MaxTerrainGridSize; y++)
                {
                    locationGrid[x, y] = new List<Location>();
                }
            }
        }

        if (loc.CenterX < 0 || loc.CenterX >= _mapProvider.GetMap().GetHwid() ||
            loc.CenterZ < 0 || loc.CenterZ >= _mapProvider.GetMap().GetHhgt())
        {
            return;
        }

        Zone zone = _mapProvider.GetMap().Get<Zone>(mapZoneIds[loc.CenterX, loc.CenterZ]);
        if (zone != null)
        {
            zone.Locations.Add(loc);

            int gx = MathUtils.Clamp(0, loc.CenterX / MapConstants.TerrainPatchSize, MapConstants.MaxTerrainGridSize - 1);
            int gy = MathUtils.Clamp(0, loc.CenterZ / MapConstants.TerrainPatchSize, MapConstants.MaxTerrainGridSize - 1);

            locationGrid[gx, gy].Add(loc);

            loc.Id = _mapProvider.GetMap().Id + "-" + (++locationCount);
        }
    }

    public float GetMountainDefaultSize(Map map)
    {
        if (map == null)
        {
            return 30;
        }

        float zoneSize = map.ZoneSize * MapConstants.TerrainPatchSize;
        return MathUtils.Clamp(MapConstants.MinMountainWidth, zoneSize / MapConstants.MountainWidthDivisor, MapConstants.MaxMountainWidth);
    }

}