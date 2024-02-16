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

public class MapGenData
{

    // Alphamap width and height
    public int awid;
    public int ahgt;

    // Detail map width and height
    public int dwid;
    public int dhgt;

    public byte[,,] grassAmounts;

    // heightmap
    public float[,] heights;
    public float[,] subZonePercents;
    public int[,] subZoneIds;
    public float[,] overrideZoneScales;
    // splatmaps
    public float[,,] alphas;

    public float[,] roadDistances;


    // List of roads created
    public List<List<MyPointF>> roads;

    public List<MyPointF> creviceBridges;
    // Bridges that have been made
    public List<MyPointF> currBridges;

    public ushort[,] bridgeDistances;


    public float[,] mountainNoise;
    public float[,] mountainDecayPower;

    public List<Location>[,] locationGrid;

    // Ends of ramps where special monsters or quests can be placed.
    public List<MyPoint> rampTops;

    public float[,] creviceDepths;

    public int[,] flags;

    public short[,] mapZoneIds;
    public List<MyPoint> zoneCenters;
    public List<ConnectedPairData> zoneConnections;
    public float[,] mountainHeights;
    public float[,] nearestMountainTopHeight;
    public float[,] mountainCenterDist;
    public float[,] mountainDistPercent;
    public float[,] edgeMountainDistPercent;
    public int[,] mapObjects;

    public List<int[]> wallEndpoints;


    public Dictionary<int, List<int>> zoneAdjacencies = new Dictionary<int, List<int>>();



    public List<GenZone> GenZones { get; set; } = new List<GenZone>();



    // Have we copied the heightmap data into the TerrainData?
    public bool HaveSetHeights = false;
    // Have we copied the splatmaps data into the TerrainData?
    public bool HaveSetAlphaSplats = false;

    public bool GeneratingMap = false;

    public Dictionary<long, List<long>> zoneTreeIds = null;
    public Dictionary<long, List<long>> zoneBushIds = null;

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


    
  

 
    public void ClearAlphasAt(GameState gs, int x, int z)
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

    public float GetAverageHeightNear(GameState gs, Map map, int hx, int hy, int radius, int terrainType = -1)
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

    public float GetAverageSplatNear(GameState gs, int x, int y, int radius, int channel)
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

    public float EdgeHeightmapAdjustPercent(GameState gs, Map map, int x, int y)
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
    public void AddMapLocation(GameState gs, Location loc)
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

        if (loc.CenterX < 0 || loc.CenterX >= gs.map.GetHwid() ||
            loc.CenterZ < 0 || loc.CenterZ >= gs.map.GetHhgt())
        {
            return;
        }

        Zone zone = gs.map.Get<Zone>(mapZoneIds[loc.CenterX, loc.CenterZ]);
        if (zone != null)
        {
            zone.Locations.Add(loc);

            int gx = MathUtils.Clamp(0, loc.CenterX / MapConstants.TerrainPatchSize, MapConstants.MaxTerrainGridSize - 1);
            int gy = MathUtils.Clamp(0, loc.CenterZ / MapConstants.TerrainPatchSize, MapConstants.MaxTerrainGridSize - 1);

            locationGrid[gx, gy].Add(loc);

            loc.Id = gs.map.Id + "-" + (++locationCount);
        }
    }

    public float GetMountainDefaultSize(GameState gs, Map map)
    {
        if (map == null)
        {
            return 30;
        }

        float zoneSize = map.ZoneSize * MapConstants.TerrainPatchSize;
        return MathUtils.Clamp(MapConstants.MinMountainWidth, zoneSize / MapConstants.MountainWidthDivisor, MapConstants.MaxMountainWidth);
    }

}