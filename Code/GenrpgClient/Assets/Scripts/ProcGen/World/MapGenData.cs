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

public class MapGenData
{

    // Alphamap width and height
    public int awid;
    public int ahgt;

    // Detail map width and height
    public int dwid;
    public int dhgt;

    public byte[,,] grassAmounts;

    public TerrainPatchData[,] terrainPatches;
    // heightmap
    public float[,] heights;
    public float[,] overrideZonePercents;
    public int[,] overrideZoneIds;
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
    public List<Location> zoneCenters;
    public List<ConnectedPairData> zoneConnections;
    public float[,] mountainHeights;
    public float[,] nearestMountainTopHeight;
    public float[,] mountainCenterDist;
    public float[,] mountainDistPercent;
    public float[,] edgeMountainDistPercent;
    public int[,] mapObjects;

    public List<int[]> wallEndpoints;


    public Dictionary<int, List<int>> zoneAdjacencies = new Dictionary<int, List<int>>();


    public List<MyPoint> addPatchList;
    public List<MyPoint> removePatchList;
    public List<MyPoint> loadingPatchList;

    // Have we copied the heightmap data into the TerrainData?
    public bool HaveSetHeights = false;
    // Have we copied the splatmaps data into the TerrainData?
    public bool HaveSetAlphaSplats = false;

    public bool GeneratingMap = false;

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

    }

    public MapGenData()
    {
        terrainPatches = new TerrainPatchData[MapConstants.MaxTerrainGridSize, MapConstants.MaxTerrainGridSize];
        addPatchList = new List<MyPoint>();
        removePatchList = new List<MyPoint>();
        loadingPatchList = new List<MyPoint>();
    }


    public float GetSteepness(GameState gs, float xpos, float ypos)
    {
        if (!HaveSetHeights)
        {
            return 0.0f;
            //throw new Exception("Tried to calc steepness before setting heights!");
        }


        int xgrid = (int)(xpos / (MapConstants.TerrainPatchSize - 1));
        int ygrid = (int)(ypos / (MapConstants.TerrainPatchSize - 1));

        float localx = xpos - xgrid * (MapConstants.TerrainPatchSize - 1);
        float localy = ypos - ygrid * (MapConstants.TerrainPatchSize - 1);

        if (localx < 0.1f)
        {
            localx += 0.1f;
        }

        if (localy < 0.1f)
        {
            localy += 0.1f;
        }

        TerrainData tdata2 = GetTerrainData(gs,xgrid, ygrid);

        if (tdata2 == null)
        {
            return 0.0f;
        }

        float endDelta = 0.2f;
        localx = MathUtils.Clamp(endDelta, localx, MapConstants.TerrainPatchSize - 1 - endDelta);
        localy = MathUtils.Clamp(endDelta, localy, MapConstants.TerrainPatchSize - 1 - endDelta);

        return tdata2.GetSteepness((localx+0.0f)/(MapConstants.TerrainPatchSize-1) , (localy+0.0f)/(MapConstants.TerrainPatchSize-1));
    }

    int interpXGrid = 0;
    int interpYGrid = 0;
    float interpLocalX = 0;
    float interpLocalY = 0;
    TerrainData normalTerrainData = null;
    public float GetInterpolatedHeight(GameState gs, float xpos, float ypos)
    {
        if (!HaveSetHeights)
        {
            return 0.0f;
        }
        interpXGrid = (int)(xpos / (MapConstants.TerrainPatchSize - 1));
        interpYGrid = (int)(ypos / (MapConstants.TerrainPatchSize - 1));

        interpLocalX = xpos - interpXGrid * (MapConstants.TerrainPatchSize - 1);
        interpLocalY = ypos - interpYGrid * (MapConstants.TerrainPatchSize - 1);

        if (interpLocalX < 0.1f)
        {
            interpLocalX += 0.1f;
        }

        if (interpLocalY < 0.1f)
        {
            interpLocalY += 0.1f;
        }

        normalTerrainData = GetTerrainData(gs, interpXGrid, interpYGrid);

        if (normalTerrainData == null)
        {
            return 0.0f;
        }


        interpLocalX = MathUtils.Clamp(0.1f, interpLocalX, MapConstants.TerrainPatchSize - 1.1f);
        interpLocalY = MathUtils.Clamp(0.1f, interpLocalY, MapConstants.TerrainPatchSize - 1.1f);

        return normalTerrainData.GetInterpolatedHeight((interpLocalX + 0.0f) / MapConstants.TerrainPatchSize, (interpLocalY + 0.0f) / MapConstants.TerrainPatchSize);
    }

    int sampleXGrid = 0;
    int sampleYGrid = 0;
    Terrain sampleTerrain = null;
    public float SampleHeight (GameState gs, float x, float y, float z)
    {
        if (!HaveSetHeights)
        {
            return 0.0f;
        }


        sampleXGrid = (int)(x / (MapConstants.TerrainPatchSize -1));
        sampleYGrid= (int)(z / (MapConstants.TerrainPatchSize -1));


        sampleTerrain = GetTerrain(gs, sampleXGrid, sampleYGrid);
        if (sampleTerrain == null)
        {
            return 0.0f;
        }



        return sampleTerrain.SampleHeight(GVector3.Create(x, y, z));
    }


    const float NormalEdgePct = 0.001f;
    int divSize = MapConstants.TerrainPatchSize-1;
    int normalXGrid = 0;
    int normalYGrid = 0;
    TerrainPatchData normalPatch = null;
    public GVector3 GetInterpolatedNormal (UnityGameState gs, Map map, float x, float y)
    {
        if (!HaveSetHeights)
        {
            throw new Exception("You must set the terrain heights before interpolating height.");
        }

        float startx = x;
        float starty = y;

        normalXGrid = (int)(x / (divSize));
        normalYGrid = (int)(y / (divSize));

        x -= normalXGrid * (divSize);
        y -= normalYGrid * (divSize);

        if (normalXGrid < 0 || normalYGrid < 0 || normalXGrid >= map.BlockCount || normalYGrid >= map.BlockCount ||
            terrainPatches == null)
        {
            return GVector3.up;
        }

        normalPatch = terrainPatches[normalXGrid, normalYGrid];

        if (normalPatch == null)
        {
            return GVector3.up;
        }

        normalTerrainData = normalPatch.terrainData as TerrainData;

        if (normalTerrainData == null)
        {
            return GVector3.up;
        }
        x = MathUtils.Clamp(NormalEdgePct, x / (divSize), 1-NormalEdgePct);
        y = MathUtils.Clamp(NormalEdgePct, y / (divSize), 1-NormalEdgePct);
        GVector3 norm = GVector3.Create(normalTerrainData.GetInterpolatedNormal(x, y));

        return norm;
    }

    public void SetAllTerrainNeighbors(GameState gs)
    {
        for (int gx = 0; gx < gs.map.BlockCount; gx++)
        {
            for (int gy = 0; gy < gs.map.BlockCount; gy++)
            {
                SetOneTerrainNeighbors(gs, gx, gy);
            }
        }
    }


    TerrainPatchData getDataPatch = null;
    public TerrainData GetTerrainData (GameState gs, int gx, int gy)
    {
        getDataPatch = GetTerrainPatch(gs, gx, gy);
        if (getDataPatch == null)
        {
            return null;
        }
        return getDataPatch.terrainData as TerrainData;

    }

    TerrainPatchData getTerrainPatch = null;
    public Terrain GetTerrain (GameState gs, int gx, int gy)
    {
        getTerrainPatch = GetTerrainPatch(gs, gx, gy);
        if (getTerrainPatch == null)
        {
            return null;
        }
        return getTerrainPatch.terrain as Terrain;
    }

    public void SetOneTerrainNeighbors (GameState gs, int gx, int gy)
    {
        if (gx < 0 || gy < 0)
        {
            return;
        }

        Terrain mid = GetTerrain(gs, gx, gy);
        if (mid == null)
        {
            return;
        }
        Terrain top = GetTerrain(gs, gx, gy + 1);
        Terrain bottom = GetTerrain(gs, gx, gy - 1);
        Terrain left = GetTerrain(gs, gx - 1, gy);
        Terrain right = GetTerrain(gs, gx + 1, gy);
        mid.SetNeighbors(left, top, right, bottom);
    }

    public int GetHeightmapSize (UnityGameState gs)
    {
        if (gs.map == null || gs.map.BlockCount < 4)
        {
            return MapConstants.DefaultHeightmapSize;
        }
        return gs.map.GetMapSize(gs);
    }

    public Texture2D[] _basicTerrainTextures = null;


    public Texture2D GetBasicTerrain(UnityGameState gs, int index)
    {
        if (_basicTerrainTextures == null)
        {
            Color[] colors = new Color[] { GColor.green * 0.6f, new Color(0.6f, 0.3f, 0), GColor.white * 0.4f, GColor.white * 0.8f };
            _basicTerrainTextures = new Texture2D[MapConstants.MaxTerrainIndex];
            for (int c =0; c < colors.Length && c < MapConstants.MaxTerrainIndex; c++)
            {

                Texture2D tex = new Texture2D(4, 4, TextureFormat.ARGB32, false, true);
                Color32[] texColors = tex.GetPixels32();
                for (int i =0; i < texColors.Length; i++)
                {
                    texColors[i] = colors[c];
                }
                tex.SetPixels32(texColors);
                _basicTerrainTextures[c] = tex;
            }
        }
        
        if (index < 0 || index >= _basicTerrainTextures.Length)
        {
            return new Texture2D(2, 2);
        }
        return _basicTerrainTextures[index];
    }

    public static TerrainLayer CreateTerrainLayer (Texture2D diffuse, Texture2D normal = null)
    {
        TerrainLayer tl = new TerrainLayer();
        if (diffuse == null)
        {
            diffuse = new Texture2D(2, 2);
        }

        tl.diffuseTexture = diffuse;
        tl.normalMapTexture = normal;
        SetTerrainLayerData(tl);
        return tl;
    }

    public static void SetTerrainLayerData(TerrainLayer tl)
    {
        if (tl == null)
        {
            return;
        }      
        tl.normalScale = 1.0f;
        tl.metallic = 0.00f; // Set to 0 if using Standard terrain shader.
        tl.smoothness = 0.00f;
        tl.specular = (Color.gray * 0.00f);    
        tl.tileOffset = new Vector2(MapConstants.TerrainLayerOffset, MapConstants.TerrainLayerOffset);
        tl.tileSize = new Vector2(MapConstants.TerrainLayerTileSize, MapConstants.TerrainLayerTileSize);
        tl.diffuseRemapMax = Vector4.zero;
        tl.diffuseRemapMin = Vector4.zero; 
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

    public TerrainPatchData GetTerrainPatch(GameState gs, int gx, int gy, bool createIfNotThere = true)
    {
        if (gx < 0 || gy < 0 || terrainPatches == null ||
            gx >= MapConstants.MaxTerrainGridSize || gy >= MapConstants.MaxTerrainGridSize)
        {
            return null;
        }
        if (terrainPatches[gx, gy] == null)
        {
            SetTerrainPatchAtGridLocation(gs, gx, gy, gs.map, null);
        }
        return terrainPatches[gx, gy];
    }

    public void RemoveTerrainPatch(GameState gs, int gx, int gy)
    {
        if (gx < 0 || gy < 0 || terrainPatches == null ||
            gx >= MapConstants.MaxTerrainGridSize || gy >= MapConstants.MaxTerrainGridSize)
        {
            return;
        }
        terrainPatches[gx, gy] = null;
    }

    public void SetTerrainPatchAtGridLocation(GameState gs, int xgrid, int ygrid, Map map, TerrainPatchData data)
    {
        if (xgrid < 0 || ygrid < 0 || xgrid >= MapConstants.MaxTerrainGridSize || ygrid >= MapConstants.MaxTerrainGridSize ||
           map == null)
        {
            return;
        }

        TerrainPatchData oldPatch = terrainPatches[xgrid, ygrid];

        if (data == null)
        {
            data = new TerrainPatchData();
        }

        data.MapId = map.Id;
        data.MapVersion = map.MapVersion;
        data.X = xgrid;
        data.Y = ygrid;
        if (map != null)
        {
            data.MapId = map.Id;
        }
        terrainPatches[xgrid, ygrid] = data;
        if (oldPatch != null)
        {
            data.terrain = oldPatch.terrain;
            data.terrainData = oldPatch.terrainData;
        }
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

        int gx = MathUtils.Clamp(0, loc.CenterX / MapConstants.TerrainPatchSize, MapConstants.MaxTerrainGridSize - 1);
        int gy = MathUtils.Clamp(0, loc.CenterZ / MapConstants.TerrainPatchSize, MapConstants.MaxTerrainGridSize - 1);


        locationGrid[gx, gy].Add(loc);
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