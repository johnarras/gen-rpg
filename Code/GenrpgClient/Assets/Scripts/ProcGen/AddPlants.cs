
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using Genrpg.Shared.Core.Entities;

using Services;
using Cysharp.Threading.Tasks;
using Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Services.ProcGen;
using System.Threading;
using Assets.Scripts.MapTerrain;
using System.Security.Cryptography;
using Genrpg.Shared.Pathfinding.Constants;
using System.Threading.Tasks;

public class FullDetailPrototype
{
    public ZonePlantType zonePlant = null;
    public PlantType plantType = null;
    public DetailPrototype proto = null;
    public long noiseSeed = 0;
    public int Index = 0;
    public int XGrid = -1;
    public int YGrid = -1;
    public List<long> zoneIds = new List<long>();
}
public class AddPlants : BaseZoneGenerator
{
    public const float GrassBaseScale = 0.75f;
    public const float GrassDensity = 1.4f;
    public const float GrassRandomChance = 0.05f;

    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {

        await base.Generate(gs, token);
        foreach (Zone zone in gs.map.Zones)
        {
            GenerateOne(gs, zone, gs.data.GetGameData<ProcGenSettings>().GetZoneType(zone.ZoneTypeId), zone.XMin, zone.ZMin, zone.XMax, zone.ZMax);
        }

        AddPlantsToMapData(gs);
    }



    public void SetupOneMapGrass(UnityGameState gs, int gx, int gy, CancellationToken token)
    {  
        if (_terrainManager == null)
        {
            gs.loc.Resolve(this);
        }
        InnerSetupOneMapGrass(gs, gx, gy, token).Forget();
    }

    private async UniTask InnerSetupOneMapGrass(UnityGameState gs, int gx, int gy, CancellationToken token)
    {
        if (
              gs.md.terrainPatches == null ||
              gx < 0 || gy < 0 || gx >= MapConstants.MaxTerrainGridSize ||
              gy >= MapConstants.MaxTerrainGridSize)
        {
            gs.logger.Error("Bail out of making grass: " + gx + " " + gy);
            return;
        }

        TerrainPatchData patch = gs.md.terrainPatches[gx, gy];

        if (patch == null)
        {
            gs.logger.Error("Patch missing " + gx + " " + gy);

            return;
        }

        TerrainData tdata = patch.terrainData as TerrainData;

        if (tdata == null)
        {
           gs.logger.Error("Tdata missing: " + gx + " " + gy);

            return;
        }
        if (patch.grassAmounts == null || patch.mainZoneIds == null)
        {
            gs.logger.Error("Core Data missing: " + patch.grassAmounts + " " + patch.mainZoneIds + " " + gx + " " + gy);

            return;
        }

        MyRandom bendRand = new MyRandom(gs.map.Seed + 3234992);

        Color color = Color.white;
        float amount = MathUtils.FloatRange(0.20f, 0.40f, bendRand);
        float speed = MathUtils.FloatRange(0.30f, 0.70f, bendRand);
        float strength = MathUtils.FloatRange(0.30f, 0.70f, bendRand);

        tdata.wavingGrassTint = Color.white;
        tdata.wavingGrassAmount = amount;
        tdata.wavingGrassSpeed = speed;
        tdata.wavingGrassStrength = strength;

        List<FullDetailPrototype> fullProtoList = new List<FullDetailPrototype>();

        List<Zone> currentZones = new List<Zone>();

        if (patch.FullZoneIdList != null)
        {
            foreach (long zid in patch.FullZoneIdList)
            {
                Zone zone = gs.map.Get<Zone>(zid);
                if (zone == null)
                {
                    continue;
                }
                currentZones.Add(zone);
                bool isMainTerrain = patch.MainZoneIdList.Contains(zid);
                UpdateValidPlantTypeList(gs, zone, gx, gy, fullProtoList, isMainTerrain, token);
            }
        }
        else
        {
            gs.logger.Error("No zones for grass: " + gx + " " + gy);
            return;
        }

        DetailPrototype[] protos = new DetailPrototype[fullProtoList.Count];
        int index = 0;

        foreach (FullDetailPrototype item in fullProtoList)
        {
            protos[index] = item.proto;
            index++;
        }

        for (int i = 0; i < protos.Length; i++)
        {
            if (protos[i] == null)
            {
                gs.logger.Error("No proto: " + gx + " " + gy + " idx " + i);
                return;
            }
        }

        bool haveAllArt = false;
        while (!haveAllArt)
        {
            haveAllArt = true;
            for (int i = 0; i < protos.Length; i++)
            {
                if (protos[i] == null || (protos[i].prototypeTexture == null && protos[i].prototype == null))
                {
                    haveAllArt = false;
                    break;
                }
            }
            if (!haveAllArt)
            {
                await UniTask.NextFrame(token);
            }
        }

        tdata.detailPrototypes = protos;

        await UniTask.NextFrame(token);

        tdata.RefreshPrototypes();

        await UniTask.NextFrame(token);

        if (patch.grassAmounts == null)
        {
            return;
        }

        List<int[,]> detailblock = new List<int[,]>();

        for (int i = 0; i < fullProtoList.Count; i++)
        {
            detailblock.Add(new int[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize]);
        }

        int maxOffset = 7;
        int totalOffset = 7 * 2 + 1;

        for (int x = 0; x < MapConstants.TerrainPatchSize - 1; x++ )
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize - 1; y++)
            {
                int finalX = x;
                int finalY = y;

                int offsetX = x + ((x * 11 + y * 17 + gx * 31 + gy * 47) % totalOffset - maxOffset);
                int offsetY = y + ((x * 43 + y * 59 + gx * 37 + gy * 53) % totalOffset - maxOffset);
                
                if (offsetX >= 0 && offsetX < MapConstants.TerrainPatchSize-1 && 
                    offsetY >= 0 && offsetY < MapConstants.TerrainPatchSize-1)
                {
                    finalX = offsetX;
                    finalY = offsetY;
                }

                long zoneId = patch.overrideZoneIds[finalX,finalY];

                if (zoneId < 1)
                {
                    zoneId = patch.mainZoneIds[finalX,finalY];
                }

                List<FullDetailPrototype> currentProtos = fullProtoList.Where(x=>x.zoneIds.Contains(zoneId)).ToList();

                if (currentProtos.Count < 1)
                {
                    continue;
                }

                while (currentProtos.Count < MapConstants.MaxGrass)
                {
                    currentProtos.Add(currentProtos[currentProtos.Count / 3]);
                }

                for (int i = 0; i < MapConstants.MaxGrass && i < currentProtos.Count;  i++)
                {
                    if (patch.grassAmounts[x, y, i] > 0)
                    {
                        FullDetailPrototype proto = currentProtos[i];
                        detailblock[proto.Index][x,y] = (int) patch.grassAmounts[x, y, i];
                    }
                }
            }
        }

        for (int l = 0; l < detailblock.Count; l++)
        {
            tdata.SetDetailLayer(0, 0, l, detailblock[l]);
        }
    }

    public void UpdateValidPlantTypeList(UnityGameState gs, Zone zone,int gx, int gy, List<FullDetailPrototype> fullList,
        bool isMainTerrain, CancellationToken token)
    {
        ZoneType zoneType = gs.data.GetGameData<ProcGenSettings>().GetZoneType(zone.ZoneTypeId);

        List<FullDetailPrototype> retval = new List<FullDetailPrototype>();
        List<ZonePlantType> plist = new List<ZonePlantType>(zone.PlantTypes);


        int maxQuantity = isMainTerrain ? MapConstants.MaxGrass : MapConstants.OverrideMaxGrass;

        while (plist.Count > maxQuantity)
        {
            plist.RemoveAt(gs.rand.Next() % plist.Count);
        }

        for (int p = 0; p < plist.Count; p++)
        {
            ZonePlantType zpt = plist[p];

            FullDetailPrototype currProto = fullList.FirstOrDefault(x => x.zonePlant.PlantTypeId == zpt.PlantTypeId);

            if (currProto != null)
            {
                currProto.zoneIds.Add(zone.IdKey);
                continue;
            }

            PlantType pt = gs.data.GetGameData<ProcGenSettings>().GetPlantType (zpt.PlantTypeId);
            if (pt == null || string.IsNullOrEmpty(pt.Art))
            {
                continue;
            }

            string word = pt.Art;

            DetailPrototype dp = new DetailPrototype();
            dp.renderMode = DetailRenderMode.Grass;
            dp.healthyColor = Color.white;
            dp.dryColor = Color.white;  
            
            // Now we know the plant object exists, so use the zone type plant data also.

            float minHeight = pt.MinScale * GrassBaseScale;
            float maxHeight = pt.MaxScale * GrassBaseScale;
           
            dp.maxHeight = maxHeight;
            dp.maxWidth = maxHeight;
            dp.minHeight = minHeight;
            dp.minWidth = minHeight;

            dp.noiseSpread = 1.0f;

            FullDetailPrototype full = new FullDetailPrototype();
            full.zonePlant = zpt;
            full.plantType = pt;
            full.proto = dp;
            full.Index = fullList.Count;
            full.XGrid = gx;
            full.YGrid = gy;
            full.noiseSeed = zone.Seed % 12783428 + gs.map.Seed % 543333 + p * 13231;
            full.zoneIds.Add(zone.IdKey);
            retval.Add(full);
            fullList.Add(full);
            if (gx >= 0 && gy >= 0)
            {
                _assetService.LoadAsset(gs, AssetCategory.Grass, word, OnDownloadGrass, full, null, token);
            }
        }
        return;
    }


    private void OnDownloadGrass (UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        FullDetailPrototype full = data as FullDetailPrototype;
        if (full == null || full.proto == null)
        {
            return;
        }

        GameObject go = obj as GameObject;

        if (go == null)
        {
            gs.logger.Error("no gameobject: " + url + " " + obj + " " + full.plantType.Art);
            return;
        }

        TextureList tlist = go.GetComponent<TextureList>();
        if (tlist == null || tlist.Textures == null || tlist.Textures.Count < 1)
        {
            full.proto.prototype = go;
            full.proto.renderMode = DetailRenderMode.VertexLit;
            full.proto.usePrototypeMesh = true;
        }
        else
        {
            full.proto.prototypeTexture = tlist.Textures[0];
            full.proto.healthyColor = Color.white;
            full.proto.renderMode = DetailRenderMode.GrassBillboard;
            full.proto.dryColor = Color.white;
        }

        TerrainPatchData grid = _terrainManager.GetMapGrid(gs, full.XGrid, full.YGrid) as TerrainPatchData;
        if (grid == null)
        {
            GameObject.Destroy(go);
            return;
        }
        Terrain terr = grid.terrain as Terrain;
        if (terr == null)
        {
            GameObject.Destroy(go);
            return;
        }
        GameObjectUtils.AddToParent(go, terr.gameObject);
        go.SetActive(false);
    }

    public void GenerateOne(UnityGameState gs, Zone zone, ZoneType zoneType, int startx, int starty, int endx, int endy)
    {
        if ( zone == null || zoneType == null || startx >= endx || starty >= endy ||
            gs.map == null)
        {
            return;
        }

        List<ZonePlantType> plist = zone.PlantTypes;
        if (plist == null)
        {
            return;
        }

        List<FullDetailPrototype> fullList = new List<FullDetailPrototype>();

        UpdateValidPlantTypeList(gs, zone, -1, -1, fullList,true, _token);

        if (fullList == null)
        {
            return;
        }

        int dx = endx - startx + 1;
        int dy = endy - starty + 1;
        int perlinSize = Math.Max(MapConstants.DefaultNoiseSize, Math.Max(dx, dy));
        float perlinScale = perlinSize * 1.0f / MapConstants.DefaultHeightmapSize;
        while (fullList.Count > MapConstants.MaxGrass)
        {
            fullList.RemoveAt(fullList.Count - 1);
        }

        bool hadTargetAlphaIndex = false;

        for (int index = 0; index < fullList.Count; index++)
        {
            FullDetailPrototype full = fullList[index];

            if (full.noiseSeed == 0 || full.plantType == null || full.zonePlant == null)
            {
                continue;
            }
            long pseed = full.noiseSeed;

            int plantChanceTimes = 2;

            List<float[,]> plantChances = new List<float[,]>();

            float density = 0.0f;

            float midSteepVal = 70f;

            GenZone genZone = gs.GetGenZone(zone.IdKey);

            MyRandom rand = new MyRandom(full.noiseSeed);
            for (int i = 0; i < plantChanceTimes; i++)
            {
                float pers = MathUtils.FloatRange(0.1f, 0.3f, rand) * 1.2f;
                float amp = MathUtils.FloatRange(1.0f, 2.0f, rand) * 0.9f;
                float freq = perlinSize * MathUtils.FloatRange(0.04f, 0.25f, rand);

                int octaves = 2;


                freq *= genZone.GrassFreq * zoneType.GrassFreq;

                freq *= perlinScale;


                if (freq < 8)
                {
                    freq = 8;
                }

                density = genZone.GrassDensity * zoneType.GrassDensity;

                amp *= density;

                if (amp < 0.3f)
                {
                    amp = 0.3f;
                }

                plantChances.Add(_noiseService.Generate(gs, pers, freq, amp, octaves, pseed, perlinSize, perlinSize));
            }

            float steepFreq = perlinSize * MathUtils.FloatRange(0.05f, 0.15f, rand);
            float steepAmp = MathUtils.FloatRange(4, 20, rand);
            float steepPers = MathUtils.FloatRange(0.1f, 0.3f, rand);
            int steepOctaves = 2;

            /// Steepness allowed at each coord for this grass to grow or not.
            float[,] steepVals = _noiseService.Generate(gs, steepPers, steepFreq, steepAmp, steepOctaves, pseed + 1, perlinSize, perlinSize);

            int numChecked = 0;
            int badZoneId = 0;
            int nearRoad = 0;
            int nearLocation = 0;
            int didSet = 0;

            int targetAlphaIndex = -1;


            if (!hadTargetAlphaIndex)
            {
                if (rand.NextDouble() < 0.1f)
                {
                    targetAlphaIndex = MapConstants.BaseTerrainIndex;
                }
                else if (rand.NextDouble() < 0.05f)
                {
                    targetAlphaIndex = MapConstants.DirtTerrainIndex;
                }

                if (targetAlphaIndex >= 0)
                {
                    hadTargetAlphaIndex = true;
                }
            }

            bool useUniformDensity = gs.rand.NextDouble() < 0.03f;
            for (int x = startx; x <= endx; x++)
            {
                for (int y = starty; y <= endy; y++)
                {
                    float currDensityMult = MathUtils.FloatRange(0, 2, rand);
                    numChecked++;

                    if (gs.md.mapZoneIds[x,y] != zone.IdKey) // zoneobject
                    {
                        badZoneId++;
                        continue;
                    }
                    if (gs.md.mapObjects[x,y] != 0)
                    {
                        continue;
                    }
                    if (FlagUtils.IsSet(gs.md.flags[x, y], MapGenFlags.BelowWater))
                    {
                        continue;
                    }
                    if (gs.md.alphas[x,y, MapConstants.RoadTerrainIndex] > 0)
                    {
                        bool isNearRoad = false;
                        int roadRad = 0;
                        for (int xx = x - roadRad; xx <= x + roadRad; xx++)
                        {
                            if (xx < 0 || xx >= gs.map.GetHwid())
                            {
                                continue;
                            }
                            for (int yy = y - roadRad; yy <= y + roadRad; yy++)
                            {
                                if (yy < 0 || yy >= gs.map.GetHhgt())
                                {
                                    continue;
                                }
                                if (gs.md.alphas[xx,yy, MapConstants.RoadTerrainIndex] > 0)
                                {
                                    isNearRoad = true;
                                    break;
                                }
                            }
                        }
                        if (isNearRoad)
                        {
                            nearRoad++;
                            continue;
                        }
                    }
                    float hgt = gs.md.SampleHeight(gs, x, 2000, y);
                    if (hgt < MapConstants.MinLandHeight*7/10)
                    {
                        continue;
                    }


                    float steep = gs.md.GetSteepness(gs, x,y);

                    if (steep > (midSteepVal+steepVals[x-startx,y-starty]))
                    {
                        continue;
                    }

                    float chance = 0;
                    if (!full.plantType.HasFlag(PlantFlags.SmallPatches) && !useUniformDensity)
                    {

                        for (int i = 0; i < plantChanceTimes; i++)
                        {
                            float origChance = plantChances[i][x - startx, y - starty];
                            if (origChance < 0)
                            {
                                origChance = -origChance / 2;
                            }
                            chance += origChance;
                        }

                        chance /= plantChanceTimes;

                        if (chance > 1)
                        {
                            chance = 1;
                        }
                    }
                    else
                    {
                        if (gs.rand.NextDouble() > currDensityMult*density / 20.0f)
                        {
                            continue;
                        }
                        else
                        {
                            chance = MathUtils.FloatRange(0, 1, rand);
                        }
                    }

                    if (_zoneGenService.FindMapLocation(gs, x, y, 1) != null)
                    {
                        nearLocation++;
                        continue;
                    }


                    if (targetAlphaIndex >= 0 && targetAlphaIndex < MapConstants.MaxTerrainIndex)                       
                    {
                        float currAlpha = gs.md.alphas[x, y, targetAlphaIndex];
                        chance *= (currAlpha * 1.2f);
                    }

                    short val = (short)(chance * MapConstants.MaxGrassValue);

                    if (val < 1 && rand.NextDouble() < GrassRandomChance)
                    {
                        val = (short)MathUtils.IntRange(1, 3, rand);
                    }

                    if (val > 0)
                    {
                        didSet++;

                        val = (short)Math.Max(steep / 10, val);
                        if (val > MapConstants.MaxGrassValue)
                        {
                            val = MapConstants.MaxGrassValue;
                        }

                        if (val < 3)
                        {
                            val++;
                        }

                        int ny = y - (y / (MapConstants.TerrainPatchSize - 1))*0;
                        int nx = x - (x / (MapConstants.TerrainPatchSize - 1))*0;
                        if (full.plantType.HasFlag(PlantFlags.UsePrefab))
                        {                            
                            val = (short)(val * MapConstants.PrefabPlantDensityScale);
                        }
                        gs.md.grassAmounts[nx, ny, index] = (byte)val;
                    }
                }

            }
        }
    }

    public void AddPlantsToMapData(UnityGameState gs)
    {

        if ( gs.md.grassAmounts == null || gs.md.mapObjects == null)
        {
            return;
        }
        for (int x = 0; x < gs.map.GetHwid(); x++)
        {
            for (int y = 0; y < gs.map.GetHhgt(); y++)
            {
                if (gs.md.mapObjects[x, y] == 0)
                {
                    int val = 0;
                    int[] vals = new int[MapConstants.MaxGrass];
                    for (int i = 0; i < MapConstants.MaxGrass; i++)
                    {
                        int currVal = Math.Min(MapConstants.MaxGrassValue, (int)gs.md.grassAmounts[x, y, i]);
                        vals[i] = currVal;
                        for (int j = 0; j < i; j++)
                        {
                            currVal *= (MapConstants.MaxGrassValue + 1);
                        }
                        val += currVal;
                    }
                    if (val != 0)
                    {
                        gs.md.mapObjects[x, y] = (ushort)(MapConstants.GrassMinCellValue + val);
                    }
                }
            }
        }
    }

}

