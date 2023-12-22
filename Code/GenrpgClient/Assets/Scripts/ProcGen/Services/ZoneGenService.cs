

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using Assets.Scripts.Tokens;
using Genrpg.Shared.Login.Messages.LoadIntoMap;
using Assets.Scripts.MapTerrain;
using Genrpg.Shared.Units.Services;
using Genrpg.Shared.Names.Services;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.ProcGen.Settings.Texturse;
using Genrpg.Shared.ProcGen.Settings.Plants;
using Genrpg.Shared.ProcGen.Settings.Trees;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Zones.WorldData;

public interface IZoneGenService : IService
{
    void CancelMapToken();
    UniTask SetOnePatchAlphamaps(UnityGameState gs, TerrainPatchData patch, CancellationToken token);
    void SetOnePatchHeightmaps(UnityGameState gs, TerrainPatchData patch, float[,] globalHeights, float[,] heightOverrides = null);
    void InstantiateMap(UnityGameState gs, string mapId);
    Zone Generate(UnityGameState gs, long zoneId, long zoneTypeId, long extraSeed);

    string GenerateZoneName(UnityGameState gs, long zoneTypeId, long extraSeed);

    float DistancePercentToLocation(UnityGameState gs, int cx, int cy, float maxDistanceAway);
    Location FindMapLocation(UnityGameState gs, int centerx, int centery, float extraborder);

    void ShowGenError(UnityGameState gs, string msg);

    void SetAllAlphamaps(UnityGameState gs, float[,,] alphaMaps, CancellationToken token);

    void SetAllHeightmaps(UnityGameState gs, float[,] heights, CancellationToken token); 
        
    void LoadMap(UnityGameState gsIn, LoadIntoMapCommand loadData);
    void InitTerrainSettings(UnityGameState gs, int gx, int gy, int patchSize, CancellationToken token);
    UniTask OnLoadIntoMap(UnityGameState gs, LoadIntoMapResult data, CancellationToken token);
}

public class ZoneGenService : IZoneGenService, IGameTokenService
{
    protected IReflectionService _reflectionService;
    protected IUnitGenService _unitGenService;
    protected INameGenService _nameGenService;

    public virtual void SetGameToken(CancellationToken token)
    {

    }
    public virtual void CancelMapToken()
    {

    }
    public virtual void LoadMap(UnityGameState gsIn, LoadIntoMapCommand loadData)
    {

    }
    public virtual void InitTerrainSettings(UnityGameState gs, int gx, int gy, int patchSize, CancellationToken token)
    {

    }

    public virtual async UniTask OnLoadIntoMap(UnityGameState gs, LoadIntoMapResult data, CancellationToken token)
    {
        await UniTask.CompletedTask;
    }

    public virtual async UniTask SetOnePatchAlphamaps(UnityGameState gs, TerrainPatchData patch, CancellationToken token)
    {
        await UniTask.CompletedTask;
    }

    public virtual void SetOnePatchHeightmaps(UnityGameState gs, TerrainPatchData patch, float[,] globalHeights, float[,] heightOverrides = null)
    {

    }
    // Add monsters to the zone.
    private bool _haveSetupRoom = false;
    public virtual void InstantiateMap(UnityGameState gs, string mapId)
    {
        if (_haveSetupRoom)
        {
            return;
        }

    }

    public float DistancePercentToLocation(UnityGameState gs, int cx, int cz, float maxDistanceToLocation)
    {
        Location loc = FindMapLocation(gs, cx, cz, maxDistanceToLocation);

        if (loc == null)
        {
            return 1.0f;
        }

        float ddx = 1.0f * (cx - loc.CenterX) / loc.XSize;
        float ddz = 1.0f * (cz - loc.CenterZ) / loc.ZSize;

        float distPct = (float)Math.Sqrt(ddx * ddx + ddz * ddz);

        return MathUtils.Clamp(0, distPct - 1, 1);
    }

    public Location FindMapLocation(UnityGameState gs, int cx, int cz, float borderSize)
    {
        if ( gs.md.locationGrid == null)
        {
            return null;
        }

        int gx = cx / (MapConstants.TerrainPatchSize - 1);
        int gz = cz / (MapConstants.TerrainPatchSize - 1);

        float minDist = 10000000;
        Location chosenLoc = null;

        for (int x = gx - 3; x <= gx + 3; x++)
        {
            if (x < 0 || x >= MapConstants.MaxTerrainGridSize)
            {
                continue;
            }

            for (int z = gz - 3; z <= gz + 3; z++)
            {
                if (z < 0 || z >= MapConstants.MaxTerrainGridSize)
                {
                    continue;
                }

                List<Location> list = gs.md.locationGrid[x, z];
                if (list == null)
                {
                    continue;
                }

                foreach (Location loc in list)
                {
                    if (loc.XSize < 1 || loc.ZSize < 1)
                    {
                        continue;
                    }

                    long dx = Math.Abs(loc.CenterX - cx);
                    long dz = Math.Abs(loc.CenterZ - cz);

                    double xsize = loc.XSize + borderSize;
                    double zsize = loc.ZSize + borderSize;


                    if (loc.IsRectangular())
                    {
                        if (dx <= xsize && dz <= zsize)
                        {
                            return loc;
                        }
                    }
                    else
                    {
                        if (dx * dx * zsize * zsize + dz * dz * xsize * xsize <= xsize * xsize * zsize * zsize)
                        {
                            float dist = (float)Math.Sqrt(dx * dx + dz * dz);
                            if (chosenLoc == null || dist < minDist)
                            {
                                chosenLoc = loc;
                                minDist = dist;
                            }
                        }
                    }
                }
            }
        }

        if (chosenLoc != null)
        {
            return chosenLoc;
        }

        return null;

    }


    public virtual void ShowGenError(UnityGameState gs, string msg)
    {
        gs.logger.Error("ZONE GEN ERROR: " + msg);
    }


    public virtual void SetAllAlphamaps(UnityGameState gs, float[,,] alphaMaps, CancellationToken token)
    {

    }

    public virtual void SetAllHeightmaps(UnityGameState gs, float[,] heights, CancellationToken token)
    {

    }

    /// <summary>
    /// Generate a new zone
    /// </summary>
    /// <param name="gs">GameState</param>
    /// <param name="map">Map parent of zone</param>
    /// <param name="x">x pos of zone</param>
    /// <param name="y">y pos of zone</param>
    /// <param name="zoneId">Id of new zone</param>
    /// <param name="zoneTypeId">Zone type of new zone</param>
    /// <param name="extraSeed">Extra MyRandom seed for this zone</param>
    public virtual Zone Generate(UnityGameState gs, long zoneId, long zoneTypeId, long extraSeed)
    {

        if ( gs.map == null)
        {
            return null;
        }

        long seed = gs.map.Seed / 3 + 431 + extraSeed;
        MyRandom rand = new MyRandom(seed);
        ZoneType zoneType = gs.data.GetGameData<ZoneTypeSettings>(gs.ch).GetZoneType(zoneTypeId);
        if (zoneType == null)
        {
            return null;
        }

        int maxLevel = gs.data.GetGameData<LevelSettings>(gs.ch).MaxLevel;

        Zone zone = new Zone();
        zone.IdKey = zoneId++;
        zone.MapId = gs.map.Id;
        zone.ZoneTypeId = zoneTypeId;
        zone.Seed = seed;
        zone.Level = 1;
        zone.Name = GenerateZoneName(gs, zoneTypeId, gs.map.Seed / 7 + zone.Seed / 11 + rand.Next() % 2346234);

        SetTerrainConstants(gs, gs.map, zone, zoneType, rand);
        SetupFoliage(gs, gs.map, zone, zoneType, rand);
        SetupMonsters(gs, gs.map, zone, zoneType, rand);
        return zone;

    }


    protected virtual void SetTerrainConstants(UnityGameState gs, Map map, Zone zone, ZoneType zt, MyRandom rand)
    {
        if ( map == null || zone == null || zt == null || rand == null)
        {
            return;
        }

        GenZone genZone = gs.GetGenZone(zone.IdKey);

        float densityDelta = 0.10f;
        genZone.DetailAmp = MathUtils.FloatRange(1 - densityDelta, 1 + densityDelta, rand);
        genZone.DetailFreq = MathUtils.FloatRange(1 - densityDelta, 1 + densityDelta, rand);
        genZone.GrassDensity = 1.0f; MathUtils.FloatRange(1 - densityDelta, 1 + densityDelta, rand);
        genZone.TreeDensity = MathUtils.FloatRange(1 - densityDelta, 1 + densityDelta, rand);
        genZone.RockDensity = MathUtils.FloatRange(1 - densityDelta, 1 + densityDelta, rand);
        genZone.RoadDipScale = MathUtils.FloatRange(1 - densityDelta, 1 + densityDelta, rand);
        genZone.BushDensity = MathUtils.FloatRange(1 - densityDelta, 1 + densityDelta, rand);
        genZone.TreeFreq = MathUtils.FloatRange(1 - densityDelta, 1 + densityDelta, rand);
        genZone.GrassFreq = MathUtils.FloatRange(1 - densityDelta, 1 + densityDelta, rand);
        genZone.BushFreq = MathUtils.FloatRange(1 - densityDelta, 1 + densityDelta, rand);
        genZone.RoadDirtScale = MathUtils.FloatRange(1 - densityDelta, 1 + densityDelta, rand);

        genZone.SpreadChance = MathUtils.FloatRange(0.04f, 0.07f, rand);

        if (zt.Textures != null)
        {
            List<TextureType> allTextures = gs.data.GetGameData<TextureTypeSettings>(gs.ch).GetData();
            if (allTextures == null)
            {
                allTextures = new List<TextureType>();
            }
            List<TextureChannel> channels = gs.data.GetGameData<TextureChannelSettings>(gs.ch).GetData();
        
            for (int i = 0; i < channels.Count; i++)
            {
                TextureChannel channel = channels[i];

                List<ZoneTextureType> startTex = zt.Textures.Where(x => x.TextureChannelId == channel.IdKey).ToList();

                List<long> currTextures = new List<long>();
                foreach (ZoneTextureType st in startTex)
                {
                    currTextures.Add(st.TextureTypeId);
                }

                bool addedNew = false;
                do
                {
                    addedNew = false;

                    List<long> tempTex = new List<long>(currTextures);

                    foreach (long item in tempTex)
                    {
                        List<TextureType> otherTexs = allTextures.Where(x => x.ParentTextureTypeId == item).ToList();
                        foreach (TextureType ot in otherTexs)
                        {
                            if (!currTextures.Contains(ot.IdKey))
                            {
                                currTextures.Add(ot.IdKey);
                                addedNew = true;
                            }
                        }
                    }
                }
                while (addedNew);


                if (currTextures.Count > 0)
                {
                    long id = currTextures[rand.Next() % currTextures.Count];
                    if (id == 0)
                    {
                        gs.logger.Debug("Zero texture id " + zone.IdKey + " " + zone.ZoneTypeId + " " + id);
                    }
                    _reflectionService.SetObjectValue(zone, channels[i].Name + "TextureTypeId", id);
                }
                else
                {
                    gs.logger.Debug("CurrentTextures Empty: " + zone.IdKey + " " + zone.ZoneTypeId + " Channel " + channel.Name);
                }
            }
        }

    }




    /// <summary>
    /// Set up plants and trees specific for this zone.
    /// Give small chance for MyRandom plants that don't belong here to appear here.
    /// </summary>
    /// <param name="gs">GameState</param>
    /// <param name="map">Map</param>
    /// <param name="zone">This zone</param>
    /// <param name="zt">This zone type</param>
    /// <param name="rand">Extra MyRandom seed</param>
    protected virtual void SetupFoliage(UnityGameState gs, Map map, Zone zone, ZoneType zt, MyRandom rand)
    {
        if (map == null || zone == null || zt == null || rand == null)
        {
            return;
        }

        MyRandom ztRand = new MyRandom(map.Seed % 500000000 + zone.Seed % 1234567890);

        GenZone genZone = gs.GetGenZone(zone.IdKey);

        if (zt.RockTypes == null)
        {
            zt.RockTypes = new List<ZoneRockType>();
        }

        if (zt.TreeTypes == null)
        {
            zt.TreeTypes = new List<ZoneTreeType>();
        }

        if (zone.PlantTypes == null)
        {
            zone.PlantTypes = new List<ZonePlantType>();
        }

        if (genZone.RockTypes == null)
        {
            genZone.RockTypes = new List<ZoneRockType>();
        }

        if (genZone.TreeTypes == null)
        {
            genZone.TreeTypes = new List<ZoneTreeType>();
        }


        float plantPerturbSize = 0.1f;
        float minPlantPerturb = 1 - plantPerturbSize;
        float maxPlantPerturb = 1 + plantPerturbSize;

        List<int> plantChoices = new List<int>();
        if (!string.IsNullOrEmpty(zt.PlantChoices))
        {
            string[] choiceWords = zt.PlantChoices.Split(' ');
            for (int w = 0; w < choiceWords.Length; w++)
            {
                int newVal = 0;
                if (int.TryParse(choiceWords[w], out newVal))
                {
                    if (newVal > 0)
                    {
                        plantChoices.Add(newVal);
                    }
                }
            }
        }

        foreach (int pid in plantChoices)
        {
            ZonePlantType currentPlant = new ZonePlantType() { PlantTypeId = pid, Density = MathUtils.FloatRange(0.2f, 1.8f, ztRand) };
            zone.PlantTypes.Add(currentPlant);
        }

        List<ZonePlantType> plantTypeList = new List<ZonePlantType>(zone.PlantTypes);
        zone.PlantTypes = new List<ZonePlantType>();

        for (int times = 0; times < MapConstants.MaxGrass; times++)
        {
            List<ZonePlantType> availableList = new List<ZonePlantType>();


            double totalDensity = 0.0;
            foreach (ZonePlantType pt in plantTypeList)
            {
                PlantType ptype = gs.data.GetGameData<PlantTypeSettings>(gs.ch).GetPlantType (pt.PlantTypeId);
                if (ptype != null)
                {
                    if (times < MapConstants.MaxGrass / 2)
                    {
                        if (ptype.HasFlag(PlantFlags.UsePrefab))
                        {
                            continue;
                        }
                    }
                    availableList.Add(pt);
                    totalDensity += pt.Density;
                }
            }

            if (availableList.Count < 1)
            {
                continue;
            }

            ZonePlantType chosenItem = null;

            if (totalDensity <= 0)
            {
                int chosenPos = rand.Next() % availableList.Count;
                chosenItem = availableList[chosenPos];
            }
            else
            {
                double chosenDensity = rand.NextDouble() * totalDensity;
                foreach (ZonePlantType item in availableList)
                {
                    chosenDensity -= item.Density;
                    if (chosenDensity <= 0)
                    {
                        chosenItem = item;
                        break;
                    }
                }
            }

            if (chosenItem != null)
            {
                zone.PlantTypes.Add(chosenItem);
                plantTypeList.Remove(chosenItem);
            }
        }

        // Set up rocks
        int maxNumRocks = 10 + rand.Next() % 10;


        // Copy plant types to zone and perturb.
        foreach (ZoneRockType rt in zt.RockTypes)
        {
            ZoneRockType rt2 = new ZoneRockType();
            rt2.RockTypeId = rt.RockTypeId;

            rt2.ChanceScale = MathUtils.FloatRange(0.3f, 2f, rand);

            genZone.RockTypes.Add(rt2);
        }


        while (genZone.RockTypes.Count > maxNumRocks)
        {
            genZone.RockTypes.RemoveAt(rand.Next() % genZone.RockTypes.Count);
        }



        // Now set up trees
        int maxNumTrees = 4 + rand.Next() % 4;
        int maxNumBushes = 7 + rand.Next() % 7;

        float minPopulationScale = 0.3f;
        float maxPopulationScale = 2.0f;

        // Copy Tree types to zone and perturb.

        List<ZoneTreeType> treeTypes = new List<ZoneTreeType>();
        List<ZoneTreeType> bushTypes = new List<ZoneTreeType>();
        List<ZoneTreeType> waterTypes = new List<ZoneTreeType>();
        foreach (ZoneTreeType ztt in zt.TreeTypes)
        {
            TreeType tt = gs.data.GetGameData<TreeTypeSettings>(gs.ch).GetTreeType(ztt.TreeTypeId);
            if (tt == null)
            {
                continue;
            }

            if (tt.HasFlag(TreeFlags.IsWaterItem))
            {
                waterTypes.Add(ztt);
            }
            else if (tt.HasFlag(TreeFlags.IsBush))
            {
                bushTypes.Add(ztt);
            }
            else
            {
                treeTypes.Add(ztt);
            }
        }

        for (int i = 0; i < 2; i++)
        {
            List<ZoneTreeType> list = null;
            if (i == 0)
            {
                list = treeTypes;
            }
            else if (i == 1)
            {
                list = bushTypes;
            }

            List<ZoneTreeType> newList = new List<ZoneTreeType>();

            foreach (ZoneTreeType tt in list)
            {
                ZoneTreeType tt2 = new ZoneTreeType();
                tt2.TreeTypeId = tt.TreeTypeId;
                tt2.PopulationScale = MathUtils.FloatRange(minPopulationScale, maxPopulationScale, rand);
                newList.Add(tt2);
            }
            int maxNum = i == 0 ? maxNumTrees : maxNumBushes;

            while (newList.Count > maxNum)
            {
                newList.RemoveAt(rand.Next() % newList.Count);
            }
            if (genZone.TreeTypes == null)
            {
                genZone.TreeTypes = new List<ZoneTreeType>();
            }

            foreach (ZoneTreeType newItem in newList)
            {
                genZone.TreeTypes.Add(newItem);
            }
        }
        foreach (ZoneTreeType waterZtt in waterTypes)
        {
            ZoneTreeType wtt2 = new ZoneTreeType()
            {
                TreeTypeId = waterZtt.TreeTypeId,
                PopulationScale = 1.0f,
            };
            genZone.TreeTypes.Add(wtt2);
        }
    }

    protected virtual void SetupMonsters(UnityGameState gs, Map map, Zone zone, ZoneType zoneType, MyRandom rand)
    {
        int basePopMult = 10000;
        if (gs.data == null || map == null || zone == null || zoneType == null || rand == null)
        {
            return;
        }

        zone.Units = new List<ZoneUnitStatus>();

        List<UnitType> units = gs.data.GetGameData<UnitSettings>(gs.ch).GetData();
        if (units == null)
        {
            return;
        }


        List<SpawnItem> spawnItems = new List<SpawnItem>();

        foreach (SpawnItem spawnItem in zoneType.UnitSpawns)
        {

            List<SpawnItem> newSpawnItems = CreateUnitListFromSpawnItems(gs, spawnItem, 1.0f, 0);
            if (spawnItem.MinQuantity > 0 && spawnItem.MaxQuantity >= spawnItem.MinQuantity)
            {
                int numChosen = MathUtils.IntRange((int)spawnItem.MinQuantity, (int)spawnItem.MaxQuantity, rand);
                while (newSpawnItems.Count > numChosen)
                {
                    newSpawnItems.RemoveAt(rand.Next() % newSpawnItems.Count);
                }
            }
            spawnItems = spawnItems.Concat(newSpawnItems).ToList();
        }


        foreach (SpawnItem si in spawnItems)
        {
            if (si.EntityTypeId != EntityTypes.Unit)
            {
                continue;
            }

            UnitType utype = gs.data.GetGameData<UnitSettings>(gs.ch).GetUnitType(si.EntityId);
            if (utype == null)
            {
                continue;
            }

            float chance = (float)si.Weight;

            chance = MathUtils.FloatRange(chance / 2, chance * 2, rand);

            if (rand.NextDouble() < 0.1f)
            {
                chance *= MathUtils.FloatRange(1, 5, rand);
            }
            if (rand.NextDouble() < 0.3f)
            {
                chance *= MathUtils.FloatRange(0.1f, 0.2f, rand);
            }

            int newPop = Math.Max(3, (int)(basePopMult * chance));

            ZoneUnitStatus existingMonster = zone.Units.FirstOrDefault(x => x.UnitTypeId == utype.IdKey);
            if (existingMonster != null)
            {
                existingMonster.Pop += newPop;
            }
            else
            {
                ZoneUnitStatus unitStatus = new ZoneUnitStatus()
                {
                    UnitTypeId = utype.IdKey,
                    Killed = 0,
                    Pop = (int)(basePopMult * chance),
                };

                zone.Units.Add(unitStatus);
                unitStatus.Prefix = _unitGenService.GenerateUnitPrefixName(gs, utype.IdKey, zone, rand, null);

            }
        }
    }

    /// <summary>
    /// This recursively builds a list of spawn items with chances based on the list and makes sure the depth 
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="spawnItemIn"></param>
    /// <param name="retval"></param>
    /// <param name="chance"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    private List<SpawnItem> CreateUnitListFromSpawnItems(UnityGameState gs, SpawnItem spawnItemIn, double chance, int depth)
    {

        List<SpawnItem> retval = new List<SpawnItem>();

        if (depth >= 4)
        {
            return retval;
        }

        if (spawnItemIn.EntityTypeId == EntityTypes.Unit)
        {
            UnitType utype = gs.data.GetGameData<UnitSettings>(gs.ch).GetUnitType(spawnItemIn.EntityId);
            if (utype != null)
            {
                SpawnItem newSi = new SpawnItem()
                {
                    EntityTypeId = EntityTypes.Unit,
                    EntityId = utype.IdKey,
                    MinQuantity = 1,
                    MaxQuantity = 1,
                    GroupId = 0,
                    Weight = spawnItemIn.Weight / 100.0f * chance,

                };
                retval.Add(newSi);
            }
        }
        else if (spawnItemIn.EntityTypeId == EntityTypes.Spawn)
        {
            SpawnTable spawnTable = gs.data.GetGameData<SpawnSettings>(gs.ch).GetSpawnTable(spawnItemIn.EntityId);
            if (spawnTable != null && spawnTable.Items != null)
            {
                foreach (SpawnItem si in spawnTable.Items)
                {
                    retval = retval.Concat(CreateUnitListFromSpawnItems(gs, si, chance * si.Weight / 100.0f * spawnItemIn.Weight / 100.0f, depth + 1)).ToList();
                }
            }
        }
        return retval;

    }

    public string GenerateZoneName(UnityGameState gs, long zoneTypeId, long extraSeed)
    {
        string badName = "The Region";

        MyRandom rand = new MyRandom(extraSeed);

        ZoneType zt = gs.data.GetGameData<ZoneTypeSettings>(gs.ch).GetZoneType(zoneTypeId);
        if (zt == null)
        {
            return badName;
        }

        if (zt.ZoneNames == null || zt.ZoneNames.Count < 1)
        {
            return badName;
        }

        string zoneName = _nameGenService.PickWord(gs, zt.ZoneNames, rand.Next());
        string excludeWord = zoneName;
        if (zoneName == null)
        {
            zoneName = "";
        }

        if (zoneName.Length < 3)
        {
            zoneName = "Land";
        }

        string excludePrefix = zoneName.Substring(0, 3);
        string zonePrefix = _nameGenService.PickWord(gs, zt.ZoneAdjectives, rand.Next(), excludeWord, excludePrefix);

        string prefixDouble = _nameGenService.PickNameListName(gs, "ItemDoublePrefix", rand.Next(), excludeWord, excludePrefix);
        string suffixDouble = _nameGenService.PickNameListName(gs, "ItemDoubleSuffix", rand.Next(), excludeWord, excludePrefix);


        int times = 0;
        while (prefixDouble != null && suffixDouble != null &&
            prefixDouble.ToLower() == suffixDouble.ToLower() && ++times < 20)
        {
            suffixDouble = _nameGenService.PickNameListName(gs, "ItemDoubleSuffix", rand.Next(), excludeWord, excludePrefix);
        }

        string doubleName = _nameGenService.CombinePrefixSuffix(prefixDouble, suffixDouble, 0, rand.Next());
        string prefixName = _nameGenService.PickNameListName(gs, "ZoneNamePrefix", rand.Next(), excludeWord, excludePrefix);


        if (string.IsNullOrEmpty(zoneName))
        {
            return badName;
        }

        if (!string.IsNullOrEmpty(zonePrefix) && rand.NextDouble() < 0.25)
        {
            prefixName = zonePrefix;
        }

        if (!string.IsNullOrEmpty(doubleName) && string.IsNullOrEmpty(prefixName) || rand.NextDouble() < 0.40f)
        {
            prefixName = doubleName;
        }
        else if (!string.IsNullOrEmpty(doubleName) &&
            !string.IsNullOrEmpty(prefixDouble) &&
            !string.IsNullOrEmpty(suffixDouble) &&
            doubleName.IndexOf("-") < 0 &&
            rand.NextDouble() < 0.30f)
        {
            // Need to check if the doublename suffix can be used as a single word zone suffix.
            bool canUseSuffix = false;
            NameList nl = gs.data.GetGameData<NameSettings>(gs.ch).GetNameList("ItemDoubleSuffix");
            if (nl != null && nl.Names != null)
            {
                foreach (WeightedName item in nl.Names)
                {
                    if (item.Name == suffixDouble)
                    {
                        if (string.IsNullOrEmpty(item.Desc) || item.Desc.IndexOf("z") < 0)
                        {
                            canUseSuffix = true;
                            break;
                        }
                    }
                }

            }

            if (canUseSuffix)
            {
                zoneName = doubleName;
            }
        }

        if (zoneName == doubleName)
        {
            prefixName = "";
        }

        string name = prefixName;
        if (!string.IsNullOrEmpty(name))
        {
            name += " ";
        }

        name += zoneName;

        return name;
    }
}
