
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.ProcGen.Settings.Trees;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Xml.Schema;
using UnityEngine;


// Trees are now not a part of the terrain system.

internal class FullTreePrototype
{
    public string Name = "";
    public TreeType treeType { get; set; }
    public IDictionary<string, OverrideTreeType> overrideTreeTypes { get; set; }
    public ZoneTreeType zoneTree = null;
    public ZoneTreeType zoneTypeTree = null;
    public int prototypeIndex = 0;
    public MyRandom posRand;
    public MyRandom chanceRand;
    public MyRandom bareRand;
    public float chanceMult;
    public float overrideChance;

    public double currChance = 0.0f;

    public FullTreePrototype()
    {
        overrideTreeTypes = new Dictionary<string, OverrideTreeType>();
    }

}

internal class OverrideTreeType
{
    public float chance;
    public TreeType treeType;
}


internal class TreeCategory
{
    public int Index;
    public string Name;
    public List<FullTreePrototype> list;
    public int numItems;
    public float freqMult = 1.0f;
    public float densityMult = 1.0f;
    public float posDeltaScale = 1.0f;
    public float overrideChance = 0.0f;
    public float skipChance = 0.0f;
    public int Count;
   
    public TreeCategory()
    {
        list = new List<FullTreePrototype>();
    }
}

internal class ZoneTreeData
{
    public ZoneType zoneType;
    public Zone zone;
    public List<TreeCategory> categories;

    public TreeCategory GetCategory(int index)
    {
        if (categories == null)
        {
            return null;
        }

        return categories.FirstOrDefault(x => x.Index == index);
    }
}

public class AddTrees : BaseZoneGenerator
{
    private IAddNearbyItemsHelper _addNearbyItemsHelper;

    public const int TreeIndex = 1;
    public const int BushIndex = 2;
    public const int WaterIndex = 3;

    public const int TreePlacementSkipSize = 12;
    public const int BushPlacementSkipSize = 6;
    public const int WaterItemPlacementSkipSize = 4;
    public const float WaterChance = 0.65f;
    public const float TreeUniformChance = 1.0f;
    public const float BushUniformChance = 0.02f;
    public const float TreeNoiseChance = 1.0f;
    public const float BushNoiseChance = 0.02f;
    public const float MinWallTreeChance = 0.35f;
    public const float TreeSizeScale = 1.0f;
    public const float BushSizeScale = 1.5f;
    public const float MaxTreeChance = 0.67f;
    public const float MaxBushChance = 0.02f;
    
    private string[] _treeOverrideNames = new String[] { "Fall", "Young", "Bare", "FallYoung" };

    private float[,] extraTreeHeights;

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        Dictionary<long, ZoneTreeData> ztdict = new Dictionary<long, ZoneTreeData>();

        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            ZoneTreeData ndata = CreateZoneTreeData(zone);

            if (ndata != null)
            {
                ztdict[ndata.zone.IdKey] = ndata;
            }
        }

        extraTreeHeights = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];

        AddTreeListsToMap(ztdict);

        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int y = 0; y < _mapProvider.GetMap().GetHhgt(); y++)
            {
                _md.heights[x,y] += extraTreeHeights[x, y];
            }
        }

        foreach (ZoneTreeData ztd in  ztdict.Values)
        {
            foreach (TreeCategory tcat in ztd.categories)
            {
                if (tcat.Count > 0)
                {
                    _logService.Info(tcat.Count + " " + tcat.Name + " Placed in " + ztd.zone.Name + ": [" + ztd.zone.IdKey + "]");
                }
            }
        }

        _zoneGenService.SetAllHeightmaps(_md.heights, token);
    }

    private ZoneTreeData CreateZoneTreeData(Zone zone)
    {
        ZoneTreeData treeData = new ZoneTreeData();

        if (zone == null)
        {
            return null;
        }

        ZoneType zoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId);
        if (zoneType == null)
        {
            return null;
        }

        treeData.zone = zone;
        treeData.zoneType = zoneType;

        MyRandom choiceRand = new MyRandom(zone.Seed + 3241 + zone.IdKey * 13 + zone.ZoneTypeId * 5);

        // Have categories of items to make it easier ot manage different sets of items.
        treeData.categories = SetupTreeCategories(zone, zoneType, choiceRand);

        GenZone genZone = _md.GetGenZone(zone.IdKey);

        if (genZone.TreeTypes == null)
        {
            return null;
        }

        List<ZoneTreeType> tlist = new List<ZoneTreeType>(genZone.TreeTypes);


        // Get valid list of trees and set up some
        // objects so we can modify values later on.
        for (int t = 0; t < tlist.Count; t++)
        {

            ZoneTreeType zoneTree = tlist[t];
            TreeType treeType = _gameData.Get<TreeTypeSettings>(_gs.ch).Get(zoneTree.TreeTypeId);


            if (treeType == null || string.IsNullOrEmpty(treeType.Art))
            {
                continue;
            }

            ZoneTreeType zoneTypeTree = null;

            if (genZone.TreeTypes != null)
            {
                zoneTypeTree = genZone.TreeTypes.FirstOrDefault(x => x.TreeTypeId == zoneTree.TreeTypeId);
            }

            // If we fail to find the proper tree, make it appear at a lower percent
            // chance.
            if (zoneTypeTree == null)
            {
                continue;
            }



            FullTreePrototype full = new FullTreePrototype();
            full.zoneTree = zoneTree;
            full.zoneTypeTree = zoneTypeTree;
            full.treeType = treeType;
            full.prototypeIndex = t;
            full.Name = full.treeType.Name;
            full.posRand = new MyRandom(zone.Seed + treeType.IdKey * 23423 + 324);
            full.chanceRand = new MyRandom(zone.Seed + treeType.IdKey * 23 + 43535);
            full.bareRand = new MyRandom(zone.Seed % 23423243 + treeType.IdKey * 234231);
            full.overrideChance = MathUtils.FloatRange(MapConstants.MaxOverrideTreeTypeChance / 2,
                MapConstants.MaxOverrideTreeTypeChance, choiceRand);
            full.chanceMult = zoneTree.PopulationScale * zoneTypeTree.PopulationScale;

            if (choiceRand.NextDouble() < 0.35f)
            {
                full.overrideChance *= MathUtils.FloatRange(0.4f, 4.0f, choiceRand);
            }
            if (choiceRand.NextDouble() < 0.35f)
            {
                full.chanceMult *= MathUtils.FloatRange(0.5f, 5.0f, choiceRand);
            }
            SetupTreeTypeOverrides(full, treeType);

            if (full.Name == null)
            {
                full.Name = "Tree";
            }

            int categoryIndex = TreeIndex;
            if (full.treeType.HasFlag(TreeFlags.IsWaterItem))
            {
                categoryIndex = WaterIndex;
            }
            else if (full.treeType.HasFlag(TreeFlags.IsBush))
            {
                categoryIndex = BushIndex;
            }

            TreeCategory tc = treeData.GetCategory(categoryIndex);
            tc.list.Add(full);
        }

        foreach (TreeCategory tc in treeData.categories)
        {
            if (tc.list == null)
            {
                continue;
            }

            while (tc.list.Count > tc.numItems)
            {
                tc.list.RemoveAt(choiceRand.Next() % tc.list.Count);
            }
        }

        return treeData;
    }

    private void AddTreeListsToMap (Dictionary<long,ZoneTreeData> treeData)
    { 
        AddTreeCategoryToMap(TreeIndex, treeData, TreePlacementSkipSize);
        AddTreeCategoryToMap(BushIndex, treeData, BushPlacementSkipSize);
        AddTreeCategoryToMap(WaterIndex, treeData, WaterItemPlacementSkipSize);
    }


    private void AddTreeCategoryToMap(int listIndex, Dictionary<long, ZoneTreeData> treeData, int startSkipSize)
    { 
        if (treeData == null)
        {
            return;
        }

        MyRandom choiceRand = new MyRandom(_mapProvider.GetMap().Seed % 1234000000 + listIndex * 234654);


        MyRandom chanceRand = new MyRandom(_mapProvider.GetMap().Seed % 1000000000 + listIndex * 13254);
        MyRandom skipRand = new MyRandom(_mapProvider.GetMap().Seed % 1000000000 + listIndex * 2341983);
        int skipSize = startSkipSize;

        float finalChance = (listIndex == TreeIndex ? MaxTreeChance : MaxBushChance);


        float baseFreq = MathUtils.FloatRange(0.07f, 0.6f, choiceRand) * _mapProvider.GetMap().GetHwid() * 0.1f;
        float baseAmp = MathUtils.FloatRange(0.8f, 1.2f, choiceRand) * 0.8f;
        float basePers = MathUtils.FloatRange(0.2f, 0.3f, choiceRand);


        float roadFreq = MathUtils.FloatRange(0.07f, 0.6f, choiceRand) * _mapProvider.GetMap().GetHwid() * 0.2f;
        float roadAmp = MathUtils.FloatRange(5.0f, 10.0f, choiceRand);
        float roadPers = MathUtils.FloatRange(0.1f, 0.3f, choiceRand);

        float[,] roadNoise = _noiseService.Generate(roadPers, roadFreq, roadAmp, 2, _mapProvider.GetMap().Seed % 329832323 + 874332, _mapProvider.GetMap().GetHwid(),_mapProvider.GetMap().GetHhgt());


        float replaceFreq = MathUtils.FloatRange(0.07f, 0.6f, choiceRand) * _mapProvider.GetMap().GetHwid() * 0.2f;
        float replaceAmp = MathUtils.FloatRange(0.8f, 1.2f, choiceRand);
        float replacePers = MathUtils.FloatRange(0.4f, 0.7f, choiceRand);

        float[,] replaceNoise = _noiseService.Generate(replacePers, replaceFreq, replaceAmp, 2, _mapProvider.GetMap().Seed % 214423231 + 132, _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());


        List<float[,]> allNoises = new List<float[,]>();

        int numNoise = MathUtils.IntRange(4,11, choiceRand);
        for (int i = 0; i < numNoise; i++)
        {
            float freq = MathUtils.FloatRange(0.02f, 0.066f, choiceRand) * _mapProvider.GetMap().GetHwid();
            float amp = MathUtils.FloatRange(0.2f, 0.6f, choiceRand)*1.5f;
            int octaves = 2;
            float pers = MathUtils.FloatRange(0.1f, 0.5f, choiceRand);
            float[,] noise = _noiseService.Generate(pers, freq, amp, octaves, _mapProvider.GetMap().Seed % 23432433 + i * 17, _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());
            allNoises.Add(noise);
        }

        int minRoadDist = 6;
        int startRoadDist = 10;

        int skipRadius = skipSize*3/2;
        bool isWaterItem = (listIndex == WaterIndex);
        bool isBush = (listIndex == BushIndex || listIndex == WaterIndex);

        for (int cx = 0; cx < _mapProvider.GetMap().GetHwid(); cx += skipSize)
        {
            for (int cy = 0; cy < _mapProvider.GetMap().GetHhgt(); cy += skipSize)
            {
                int x = cx + MathUtils.IntRange(-skipRadius, skipRadius, skipRand);
                x = MathUtils.Clamp(0, x, _mapProvider.GetMap().GetHwid() - 1);
                int ddx = -x / (MapConstants.TerrainPatchSize - 1);
                if (x < 0 || x >= _mapProvider.GetMap().GetHwid())
                {
                    continue;
                }
                int y = cy + MathUtils.IntRange(-skipRadius, skipRadius, skipRand);
                y = MathUtils.Clamp(0, y, _mapProvider.GetMap().GetHhgt() - 1);
                int ddy = -y / (MapConstants.TerrainPatchSize - 1);
                if (y < 0 || y >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

                Location closeLoc = _zoneGenService.FindMapLocation(x, y, 15);

                bool forceTrees = false;
                
                if (closeLoc != null)
                {
                    if (listIndex == TreeIndex)
                    {
                        forceTrees = true;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (!isWaterItem)
                {
                    if (_md.bridgeDistances[x, y] < 12)
                    {
                        continue;
                    }

                    float currRoadDist = Math.Max(minRoadDist, startRoadDist + roadNoise[x, y]);

                    if (forceTrees)
                    {
                        currRoadDist = 4;
                    }

                    if (_md.roadDistances[x, y] <= currRoadDist)
                    {
                        continue;
                    }
                }

                
                int zoneId = _md.mapZoneIds[x, y]; // zoneobject
                bool haveSecondaryZone = false;
                if (_md.subZoneIds[x,y] > 0)
                {
                    haveSecondaryZone = true;
                    zoneId = _md.subZoneIds[x, y];
                }


                int zoneRad = 25;
                int numNearbyTries = Math.Min(MathUtils.IntRange(0, 15, choiceRand), MathUtils.IntRange(0, 15, choiceRand));

                if (haveSecondaryZone)
                {
                    zoneRad = 4;
                    numNearbyTries = 0;
                }

                List<int> zonesNearby = new List<int>();

                for (int tries = 0; tries < numNearbyTries; tries++)
                {

                    int nx = x + MathUtils.IntRange(-zoneRad, zoneRad, choiceRand);
                    int ny = y + MathUtils.IntRange(-zoneRad, zoneRad, choiceRand);
                    nx = MathUtils.Clamp(0, nx, _mapProvider.GetMap().GetHwid() - 1);
                    ny = MathUtils.Clamp(0, ny, _mapProvider.GetMap().GetHhgt() - 1);

                    if (_md.mapZoneIds[x,y] != zoneId) // zoneobject
                    {                     
                        zonesNearby.Add(_md.mapZoneIds[x, y]); // zoneobject
                    }
                }

                if (zonesNearby.Count > 0)
                {
                    int index = choiceRand.Next() % numNearbyTries;
                    if (index < zonesNearby.Count)
                    {
                        zoneId = zonesNearby[index];
                    }
                }

                if (closeLoc != null)
                {
                    int checkRadius = 2;
                    bool foundLocationPatch = false;
                    for (int lx = x-checkRadius; lx <= x+checkRadius; lx++)
                    {
                        if (lx < 0 || lx >= _mapProvider.GetMap().GetHwid())
                        {
                            continue;
                        }

                        for (int ly = y - checkRadius; ly <= y+checkRadius; ly++)
                        {
                            if (ly < 0 || ly >= _mapProvider.GetMap().GetHhgt())
                            {
                                continue;
                            }

                            if (FlagUtils.IsSet(_md.flags[lx,ly], MapGenFlags.IsLocationPatch))
                            {
                                foundLocationPatch = true;
                                break;
                            }
                        }
                        if (foundLocationPatch)
                        {
                            break;
                        }
                    }

                    if (foundLocationPatch)
                    {
                        continue;
                    }
                }
                if (FlagUtils.IsSet(_md.flags[x+ddx,y+ddy], MapGenFlags.BelowWater))
                {
                    continue;
                }

                if (FlagUtils.IsSet(_md.flags[x+ddx,y+ddy], MapGenFlags.NearWater) != isWaterItem)
                {
                    continue;
                }

                bool extraMountainChance = false;

                if (listIndex == TreeIndex && _md.mountainHeights[x, y] != 0)
                {
                    extraMountainChance = true;
                }

                double listChanceSum = 0.0f;

                if (!treeData.ContainsKey(zoneId))
                {
                    continue;
                }

                ZoneTreeData ztData = treeData[zoneId];

                TreeCategory category = ztData.GetCategory(listIndex);
                if (category == null || category.list == null)
                {
                    continue;
                }

                if (chanceRand.NextDouble() < category.skipChance && !isWaterItem && !forceTrees)
                {
                    continue;
                }

                List<FullTreePrototype> list = category.list;

                // Get the current chances.
                for (int i = 0; i < list.Count; i++)
                {
                    if (category.freqMult <= 0)
                    {
                        list[i].currChance = list[i].chanceMult * category.densityMult;
                    }
                    else
                    {
                        float val = 0;

                        for (int j = 0; j < numNoise; j++)
                        {
                            float[,] currNoise = allNoises[j];
                            int xindex = ((x + zoneId * 11)*j) % _mapProvider.GetMap().GetHwid();
                            int yindex = ((y + zoneId * 19)*j) % _mapProvider.GetMap().GetHhgt();
                            val += Math.Max(0, currNoise[xindex, yindex]);
                        }
                        list[i].currChance = Math.Max(0, val) * category.densityMult;                       
                    }

                    if (extraMountainChance && list[i].currChance < MinWallTreeChance)
                    {
                        list[i].currChance = MinWallTreeChance;
                    }
                    list[i].currChance *= 1.0f/list.Count;

                    listChanceSum += list[i].currChance;
                }

                List<FullTreePrototype> currList = new List<FullTreePrototype>();

                if (listChanceSum > finalChance)
                {
                    double scaleDownChance = finalChance / listChanceSum;
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].currChance *= scaleDownChance;
                    }
                }

                double chanceChosen = _rand.NextDouble();

                for (int i = 0; i < list.Count; i++)
                {
                    chanceChosen -= list[i].currChance;

                    if (chanceChosen <= 0)
                    {
                        currList.Add(list[i]);
                        break;
                    }
                }

                if (forceTrees && currList.Count < 1 && list.Count > 0)
                {
                    currList.Add(list[choiceRand.Next() % list.Count]);
                }

                foreach (FullTreePrototype full in currList)
                {
                    AddTreeActual(ztData.zone, full, category, x, y, (1+replaceNoise[x,y]));
                }
            }
        }
    }



    // Currently rocks, bushes and blank (regular trees)
    internal List<TreeCategory> SetupTreeCategories(Zone zone, ZoneType zoneType, MyRandom choiceRand)
    {
        if (zone == null || zoneType == null)
        {
            return new List<TreeCategory>();
        }

        List<TreeCategory> list = new List<TreeCategory>();

        TreeCategory tc = null;

        ZoneType zt = zoneType;

        GenZone genZone = _md.GetGenZone(zone.IdKey);

        tc = new TreeCategory();
        tc.Index = TreeIndex;
        tc.Name = "Trees";
        tc.freqMult = genZone.TreeFreq * zoneType.TreeFreq;
        tc.densityMult = genZone.TreeDensity * zoneType.TreeDensity;
        tc.numItems = MathUtils.IntRange(2, 4, choiceRand);
        tc.skipChance = (tc.freqMult <= 0 ? 0.15f : 0.75f);
        if (choiceRand.NextDouble() < 0.2f)
        {
            tc.numItems += MathUtils.IntRange(1, 3, choiceRand);
        }
        tc.densityMult *= (tc.freqMult <= 0 ? TreeUniformChance : TreeNoiseChance);
        list.Add(tc);




        tc = new TreeCategory();
        tc.Index = BushIndex;
        tc.Name = "Bushes";
        tc.freqMult = genZone.BushFreq * zoneType.BushFreq * 2;
        tc.densityMult = genZone.BushDensity * zoneType.BushDensity;
        tc.posDeltaScale = 2.0f;
        tc.numItems = MathUtils.IntRange(3, 5, choiceRand);
        tc.densityMult *= (tc.freqMult <= 0 ? BushUniformChance : BushNoiseChance);
        tc.skipChance =
        tc.skipChance = (tc.freqMult <= 0 ? 0.15f : 0.75f);
        if (choiceRand.NextDouble() < 0.1f)
        {
            tc.numItems += MathUtils.IntRange(1, 3, choiceRand);
        }
        list.Add(tc);


        tc = new TreeCategory();
        tc.Index = WaterIndex;
        tc.Name = "Water";
        float bushDensity = genZone.BushDensity * zoneType.BushDensity;
        if (bushDensity <= 0)
        {
            bushDensity = 0.1f;
        }

        if (bushDensity < 1.0f)
        {
            bushDensity = (float)Math.Sqrt(bushDensity);
        }
        tc.densityMult = WaterChance * MathUtils.FloatRange(0.4f, 1.6f, choiceRand) * bushDensity;
        tc.freqMult *= 0.0f;
        tc.posDeltaScale = 1.0f;
        tc.skipChance =
        tc.skipChance = (tc.freqMult <= 0 ? 0.15f : 0.75f);
        tc.numItems = MathUtils.IntRange(2, 3, choiceRand);
        list.Add(tc);

        return list;
    }

    private TreeType GetFinalTreeOverride(TreeCategory tcat, FullTreePrototype full, Zone zone, float replaceChanceMult)
    {
        if (zone == null || tcat == null || full == null || full.treeType == null)
        {
            return new TreeType();
        }

        if (full.bareRand == null || full.overrideTreeTypes == null || full.overrideTreeTypes.Keys.Count < 1)
        {
            return full.treeType;
        }

        if (full.bareRand.NextDouble() > full.overrideChance*replaceChanceMult)
        {
            return full.treeType;
        }

        double choiceTotal = 0.0f;

        foreach (OverrideTreeType val in full.overrideTreeTypes.Values)
        {
            if (val.treeType != null)
            {
                choiceTotal += val.chance;
            }
        }

        double choiceChosen = full.bareRand.NextDouble() * choiceTotal;

        foreach (OverrideTreeType val in full.overrideTreeTypes.Values)
        {
            if (val.treeType != null)
            {
                choiceChosen -= val.chance;
                if (choiceChosen <= 0)
                {
                    return val.treeType;
                }
            }
        }

        return full.treeType;
    }

    private void SetupTreeTypeOverrides(FullTreePrototype full, TreeType ttype)
    {
        if (ttype == null || ttype.Art == null || full == null || full.bareRand == null)
        {
            return;
        }

        string tname = ttype.Art;
        tname = tname.Replace("Winter", "");

        foreach (TreeType item in _gameData.Get<TreeTypeSettings>(_gs.ch).GetData())
        {
            if (item.Art != null && item.Art != ttype.Art)
            {
                foreach (string name in _treeOverrideNames)
                {
                    if (item.Art.Replace(name, "") == tname)
                    {
                        OverrideTreeType over = new OverrideTreeType()
                        {
                            treeType = item,
                        };
                        over.chance = MathUtils.FloatRange(0, 0.1f, full.bareRand);
                        if (full.bareRand.NextDouble() < 0.1f)
                        {
                            over.chance *= MathUtils.FloatRange(5, 50, full.bareRand);
                        }
                        full.overrideTreeTypes[name] = over;
                    }
                }
            }
        }
    }

    private void AddTreeActual(
                                Zone zone,
                                FullTreePrototype full,
                                TreeCategory tcat,
                                int x, int y, float replaceChanceMult)
    {
        x -= x / (MapConstants.TerrainPatchSize - 1);
        y -= y / (MapConstants.TerrainPatchSize - 1);

        TreeType treeType = full.treeType;

        long treeTypeId = 0;
        if (full.treeType != null)
        {
            treeTypeId = full.treeType.IdKey;
        }
        treeType = GetFinalTreeOverride(tcat, full, zone, replaceChanceMult);

        if (treeTypeId == 0 || treeType == null)
        {
            return;
        }

        if (x >= 0 && y >= 0 && x < _mapProvider.GetMap().GetHwid() && y < _mapProvider.GetMap().GetHhgt())
        {
            if (_md.heights[x,y] <  MapConstants.OceanHeight/MapConstants.MapHeight)
            {
                return;
            }

            if (_md.mapObjects[x, y] == 0)
            {
                int newVal = (int)(MapConstants.TreeObjectOffset + treeType.IdKey);
                _md.mapObjects[x, y] = newVal;
                tcat.Count++;

                float dirtRadius = 1;
                if (tcat.Index == TreeIndex)
                {
                    dirtRadius = (treeType.HasFlag(TreeFlags.IsBush) ? 0 : _gameData.Get<TreeTypeSettings>(_gs.ch).TreeDirtRadius);
                    float dirtScale = 0.6f;
                    dirtRadius *= (float)Math.Pow(TreeSizeScale, 0.9f);
                    dirtRadius *= MathUtils.FloatRange(0.3f, 0.9f, full.posRand);
                    dirtScale *= MathUtils.FloatRange(0.5f, 1.2f, full.posRand);
                    if (dirtScale > 0.7f)
                    {
                        dirtScale = 0.7f;
                    }

                    float maxOverallExtraHeight = MapConstants.MaxTreeBumpHeight / MapConstants.MapHeight;
                    // Put a bump near this item.
                    float overallExtraHeight = MathUtils.FloatRange(0, 1, full.posRand) * maxOverallExtraHeight;

                    float steepness = _terrainManager.GetSteepness(x, y);

                    overallExtraHeight *= (90 - steepness) / 90;

                    dirtRadius *= (float)(1.0f + 0.7f * overallExtraHeight / maxOverallExtraHeight);

                    if (dirtRadius < overallExtraHeight * 3)
                    {
                        dirtRadius = overallExtraHeight * 3;
                    }

                    int maxRadius = (int)Math.Max(dirtRadius * 1.0f, 1);

                    if (treeType.HasFlag(TreeFlags.IsBush))
                    {
                        maxRadius = -1;
                    }
                    int cx = x + MathUtils.IntRange(-1, 1, full.posRand);
                    int cy = y + MathUtils.IntRange(-1, 1, full.posRand);
                    cx = x; cy = y;
                    //cx = y; cy = x;
                    for (int x2 = cx - maxRadius - 1; x2 <= cx + maxRadius; x2++)
                    {
                        if (x2 < 0 || x2 >= _mapProvider.GetMap().GetHwid())
                        {
                            continue;
                        }
                        float dx2 = x2 - cx;
                        for (int y2 = cy - maxRadius - 1; y2 <= cy + maxRadius; y2++)
                        {
                            if (y2 < 0 || y2 >= _mapProvider.GetMap().GetHhgt())
                            {
                                continue;
                            }
                            float dy2 = y2 - cy;


                            float distScale = (float)Math.Sqrt(dx2 * dx2 + dy2 * dy2) / dirtRadius;
                            float dirtIntensity = (float)Math.Pow(Math.Exp(-distScale), 2.0f) * dirtScale;
                            dirtIntensity *= MathUtils.FloatRange(0.7f, 1.3f, full.posRand);
                            if (dirtIntensity > 1)
                            {
                                dirtIntensity = 1;
                            }

                            float oldBase = _md.alphas[x2, y2, MapConstants.BaseTerrainIndex];
                            float oldDirt = _md.alphas[x2, y2, MapConstants.DirtTerrainIndex];
                            float newBase = oldBase * (1 - dirtIntensity);
                            float baseDiff = newBase - oldBase;
                            _md.alphas[x2, y2, MapConstants.BaseTerrainIndex] = newBase;
                            _md.alphas[x2, y2, MapConstants.DirtTerrainIndex] += (oldBase - newBase);

                            //_md.ClearAlphasAt(x2, y2); _md.alphas[x2, y2, MapConstants.DirtTerrainIndex] = 1;
                        }
                    }
                    for (int x2 = cx - maxRadius - 1; x2 <= cx + maxRadius; x2++)
                    {
                        if (x2 < 0 || x2 >= _mapProvider.GetMap().GetHwid())
                        {
                            continue;
                        }
                        float dx2 = x2 - cx;
                        for (int y2 = cy - maxRadius - 1; y2 <= cy + maxRadius; y2++)
                        {
                            if (y2 < 0 || y2 >= _mapProvider.GetMap().GetHhgt())
                            {
                                continue;
                            }
                            float dy2 = y2 - cy;


                            float distScale = (float)Math.Sqrt(dx2 * dx2 + dy2 * dy2) / dirtRadius;
                            float extraHeight = overallExtraHeight * MathUtils.QuadraticSShaped(1 - distScale);
                            if (extraTreeHeights[x2, y2] < extraHeight)
                            {
                                extraTreeHeights[x2, y2] = extraHeight;
                            }
                        }
                    }
                }
                if (!full.treeType.HasFlag(TreeFlags.NoNearbyItems))
                {
                    int numNearbyItems = MathUtils.IntRange(2, 9, full.chanceRand);
                    if (full.chanceRand.NextDouble() < 0.3f)
                    {
                        numNearbyItems += MathUtils.IntRange(2, 9, full.chanceRand);
                    }
                    numNearbyItems += 5;


                    float maxRadius = Math.Max(2.0f, dirtRadius / 2);
                    float minRadius = Math.Max(1.0f, maxRadius / 2);
                       
                    _addNearbyItemsHelper.AddItemsNear(full.posRand, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone, x, y, 1.0f, numNearbyItems,minRadius,maxRadius, false);
                }
            }
        }
    }
}

