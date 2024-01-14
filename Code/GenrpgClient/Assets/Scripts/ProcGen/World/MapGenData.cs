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
        ScaleAlphasAt(gs, x, z, 0);
    }

    public void ScaleAlphasAt(GameState gs, int x, int z, float scale)
    {
        if (x < 0 || z < 0 || x >= awid || z >= ahgt)
        {
            return;
        }

        for (int c = 0; c < MapConstants.MaxTerrainIndex; c++)
        {
            alphas[x, z, c] *= scale;
        }
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

    public float GetDistanceToNonzeroElement(GameState gs, int[,] grid, int cx, int cz, float maxRadiusIn)
    {
        if (grid == null || cx < 0 || cz < 0 || cx >= grid.GetLength(0) || cz >= grid.GetLength(1))
        {
            return -1;
        }

        int xsize = grid.GetLength(0);
        int zsize = grid.GetLength(1);

        int maxRadius = (int)Math.Ceiling(maxRadiusIn);


        float minDist = maxRadiusIn + 1;

        for (int x = cx - maxRadius; x <= cx + maxRadiusIn; x++)
        {
            if (x < 0 || x >= xsize)
            {
                continue;
            }
            for (int z = cz - maxRadius; z <= cz + maxRadiusIn; z++)
            {
                if (z < 0 || z >= zsize)
                {
                    continue;
                }

                if (grid[x, z] <= 0)
                {
                    continue;
                }

                float dist = (float)Math.Sqrt((x - cx) * (x - cx) + (z - cz) * (z - cz));
                if (dist < minDist)
                {
                    minDist = dist;
                }
            }
        }

        if (minDist >= 0 && minDist <= maxRadiusIn)
        {
            return minDist;
        }

        return -1;
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

    public long GetIndexForTree(GameState gs, Zone zone, TreeType treeType, int localSeed)
    {
        if (zone == null || treeType == null)
        {
            return 1;
        }

        if (treeType.VariationCount > 1)
        {
            return 1 + (zone.Seed % 100000000 + treeType.IdKey * 12 + treeType.IdKey * treeType.IdKey + localSeed * 13 + localSeed * treeType.IdKey * 17) % treeType.VariationCount;
        }
        return 1;

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

    public float DistanceToBridge(GameState gs, int x, int y)
    {
        float minDist = 10000000;
        foreach (MyPointF br in currBridges)
        {

            float bx = br.X - x;
            float by = br.Y - y;
            float bdist = MathUtils.Sqrt(bx * bx + by * by);
            if (bdist < minDist)
            {
                minDist = bdist;
            }
        }
        return minDist;

    }

    public Zone GetZoneAt(GameState gs, Map map, int x, int y)
    {
        if (map == null ||
            mapZoneIds == null || x < 0 || y < 0 || x >= mapZoneIds.GetLength(0) || y >= mapZoneIds.GetLength(1))
        {
            return null;
        }

        return map.Get<Zone>(mapZoneIds[x, y]);
    }

    public ZoneType GetZoneTypeAt(UnityGameState gs, Map map, int x, int y)
    {
        Zone zone = GetZoneAt(gs, map, x, y);
        if (zone == null || gs.data == null)
        {
            return null;
        }
        return gs.data.GetGameData<ZoneTypeSettings>(gs.ch).GetZoneType(zone.ZoneTypeId);
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