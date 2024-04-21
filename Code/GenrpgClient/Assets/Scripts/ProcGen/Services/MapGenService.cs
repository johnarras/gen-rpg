using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Zones.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Vendors.MapObjectAddons;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.ProcGen.Settings.Locations.Constants;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.NPCs.Settings;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Core.Entities;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.GameSettings;

public interface IMapGenService : IInitializable
{

    Map GenerateMap(UnityGameState gs, Map startMap);
    void CreateZones(UnityGameState gs);
    void SetPrevNextZones(UnityGameState gs);
    void AddNPCs(UnityGameState gs);
}


public class MapGenService : IMapGenService
{
    public async Task Initialize(GameState gs, CancellationToken token)
    {
        await Task.CompletedTask;
    }

    protected IZoneGenService _zoneGenService;
    protected ILogService _logService;
    protected IGameData _gameData;
    public Map GenerateMap(UnityGameState gs, Map startMap)
    {
        if (startMap == null)
        {
            return null;
        }

        Map map = new Map();
        if (startMap.MinLevel == 0 && startMap.MaxLevel == 0)
        {
            startMap.MinLevel = 1;
            startMap.MaxLevel = _gameData.Get<LevelSettings>(null).MaxLevel;
        }
        map.MinLevel = startMap.MinLevel;
        map.MaxLevel = startMap.MaxLevel;
        
        map.Id = startMap.Id;
        map.Seed = startMap.Seed;
        map.BlockCount = Math.Min(MapConstants.MaxTerrainGridSize, startMap.BlockCount);
        map.ZoneSize = startMap.ZoneSize;


        if (map.Seed == 0)
        {
            MyRandom rand = new MyRandom((int)(DateTime.UtcNow.Ticks / 53453 % 100000000));
            map.Seed = rand.Next() % 1000000000;
        }
        map.MapVersion = ++startMap.MapVersion;

        MyRandom mapRand = new MyRandom(map.Seed);

        return map;
    }


    private ZoneType ChooseNextZoneType(UnityGameState gs, MyRandom rand, List<ZoneTypeGenData> zoneGenList, int x, int y)
    {

        float totalSize = gs.map.GetHwid();
        float midPt = totalSize / 2;

        double xNearCenterPt = MathUtils.Clamp(0, 1 - Math.Abs(x - midPt) / midPt, 1);
        double yNearCenterPt = MathUtils.Clamp(0, 1 - Math.Abs(y - midPt) / midPt, 1);

        double totalChance = 0;

        foreach (ZoneTypeGenData item in zoneGenList)
        {
            totalChance += item.chance;
        }

        double chanceChosen = rand.NextDouble() * totalChance;

        ZoneType zoneTypeChosen = null;

        foreach (ZoneTypeGenData item in zoneGenList)
        {
            chanceChosen -= item.chance;

            if (chanceChosen <= 0)
            {
                zoneTypeChosen = item.zoneType;
                break;
            }
        }

        return zoneTypeChosen;

    }

       
    public void SetMinMaxSizes(UnityGameState gs)
    {
        foreach (Zone zone in gs.map.Zones)
        {
            zone.XMin = 1000000;
            zone.ZMin = 1000000;
            zone.XMax = 0;
            zone.ZMax = 0;
        }

        for (int x = 0; x < gs.map.GetHwid(); x++)
        {
            for (int y = 0; y < gs.map.GetHhgt(); y++)
            {
                Zone currZone = gs.map.Zones.FirstOrDefault(xx => xx.IdKey == gs.md.mapZoneIds[x, y]);
                if (currZone != null)
                {
                    currZone.XMin = Math.Min(x, currZone.XMin);
                    currZone.XMax = Math.Max(x, currZone.XMax);
                    currZone.ZMin = Math.Min(y, currZone.ZMin);
                    currZone.ZMax = Math.Max(y, currZone.ZMax);
                }
            }
        }

        if (gs.map != null && gs.map.Zones != null)
        {
            foreach (Zone zone in gs.map.Zones)
            {
                if (zone.XMin >= zone.XMax || zone.ZMin >= zone.ZMax)
                {
                    zone.XMin = 0;
                    zone.XMax = 0;
                    zone.ZMin = 0;
                    zone.ZMax = 0;
                }
            }

        }
    }


    private void SetZoneLevels(UnityGameState gs, List<MyPoint> validCenters)
    {
        if ( gs.map == null || gs.map.Zones == null || validCenters == null)
        {
            return;
        }

        int minLevel = gs.map.MinLevel;
        int maxLevel = gs.map.MaxLevel;

        double minLevelRadius = gs.map.GetHwid() + gs.map.GetHhgt();
        double maxLevelRadius = 0;

        foreach (MyPoint center in validCenters)
        {
            double dist = Math.Sqrt(center.X * center.X + center.Y * center.Y);
            if (dist < minLevelRadius)
            {
                minLevelRadius = dist;
            }

            if (dist > maxLevelRadius)
            {
                maxLevelRadius = dist;
            }
        }

        if (maxLevelRadius <= minLevelRadius)
        {
            maxLevelRadius = gs.map.GetHwid() + gs.map.GetHhgt();
            minLevelRadius = 0;

        }

        // Now get the pct the zone center lies along 
        foreach (Zone zone in gs.map.Zones)
        {
            double dist = 10000000;

            if (zone.IdKey < MapConstants.MapZoneStartId)
            {
                zone.Level = 1;
            }

            foreach (Location loc in zone.Locations)
            {
                double currDist = Math.Sqrt(loc.CenterX * loc.CenterX + loc.CenterX * loc.CenterZ);
                if (currDist < dist)
                {
                    dist = currDist;
                }
            }

            if (dist > 2 * gs.map.GetHwid())
            {
                int cx = (zone.XMin + zone.XMax) / 2;
                int cy = (zone.ZMin + zone.ZMax) / 2;
                dist = Math.Sqrt(cx * cx + cy * cy);

            }
            if (dist < minLevelRadius)
            {
                zone.Level = minLevel;
            }
            else if (dist > maxLevelRadius)
            {
                zone.Level = maxLevel;
            }
            else
            {
                zone.Level = (int)(minLevel + (maxLevel - minLevel) * (dist - minLevelRadius) / (maxLevelRadius - minLevelRadius));
            }
        }

        List<Zone> zones = gs.map.Zones;

        if (maxLevel - minLevel > 3 && zones.Count > 3)
        {
            int totalLevels = maxLevel - minLevel + 1;
            zones = zones.Where(x=>x.IdKey >= MapConstants.MapZoneStartId).OrderBy(x => x.Level).ToList();

            float levelsPerZone = totalLevels * 1.0f / zones.Count;

            zones[zones.Count - 1].Level = maxLevel;
            zones[0].Level = minLevel;

            for (int z = 1; z < zones.Count - 1; z++)
            {
                float currLevel = minLevel + levelsPerZone * z;
                zones[z].Level = (int)(currLevel + 0.5f);
            }
        }
    }

    public virtual void SetPrevNextZones(UnityGameState gs)
    {
        int numAdjacent = 3;
        List<Zone> orderedZones = gs.map.Zones.OrderBy(x => x.Level).ToList();
        orderedZones = orderedZones.Where(x => x.IdKey >= MapConstants.MapZoneStartId).ToList();
        for (int i = 0; i < orderedZones.Count; i++)
        {
            Zone zone = orderedZones[i];
            GenZone genZone = gs.md.GetGenZone(zone.IdKey);
            for (int j = i - numAdjacent; j < i + numAdjacent; j++)
            {
                if (j < 0 || j >= orderedZones.Count)
                {
                    continue;
                }

                Zone otherZone = orderedZones[j];
                long levelDiff = otherZone.Level - zone.Level;
                genZone.ZonesNearLevel.Add(new ZoneRelation() { ZoneId = otherZone.IdKey, Offset = levelDiff });

            }


        }

    }

    protected virtual long GetId(UnityGameState gs, Map map, MyRandom rand)
    {
        if (map == null || rand == null)
        {
            return 0;
        }


        IReadOnlyList<ZoneType> zoneTypes = _gameData.Get<ZoneTypeSettings>(gs.ch).GetData();
        if (zoneTypes == null)
        {
            return 0;
        }

        zoneTypes = zoneTypes.Where(x => x.IdKey > 0).ToList();


        if (zoneTypes.Count < 1)
        {
            return 0;
        }



        double totalWeight = 0;

        foreach (ZoneType zt in zoneTypes)
        {
            totalWeight += zt.ZoneListGenScale;
        }

        if (totalWeight < 0.0001f)
        {
        }


        if (totalWeight > 0.0001f)
        {

            double chosenValue = rand.NextDouble() * totalWeight;

            foreach (ZoneType zt in zoneTypes)
            {
                chosenValue -= zt.ZoneListGenScale;
                if (chosenValue <= 0)
                {
                    return zt.IdKey;
                }
            }
        }
        return zoneTypes[rand.Next() % zoneTypes.Count].IdKey;

    }


    public void AddNPCs(UnityGameState gs)
    {
        foreach (Zone zone in gs.map.Zones)
        {
            AddNPCsToZone(gs, zone);
        }
    }
    private void AddNPCsToZone(UnityGameState gs, Zone zone)
    {
        MyRandom rand = new MyRandom(zone.Seed / 5 + 324121);

        string zoneName = zone.Name;

        if (zoneName.Length > 8)
        {
            zoneName = zoneName.Substring(0, 8);
        }

        IReadOnlyList<BuildingType> buildingTypes = _gameData.Get<BuildingSettings>(null).GetData();
        IReadOnlyList<NPCType> npcTypes = _gameData.Get<NPCSettings>(null).GetData();
        IReadOnlyList<BuildingType> buildings = _gameData.Get<BuildingSettings>(null).GetData();

        for (int l = 0; l < zone.Locations.Count; l++)
        {
            Location loc = zone.Locations[l];

            if (loc.Places == null || loc.Places.Count < 1)
            {
                continue;
            }

            List<LocationPlace> remainingPlaces = new List<LocationPlace>(loc.Places);
            List<NPCType> noBuildingNPCs = new List<NPCType>();
            List<NPCType> buildingNPCs = new List<NPCType>();


            foreach (NPCType npcType in npcTypes)
            {
                BuildingType btype = buildingTypes.FirstOrDefault(x => x.IdKey == npcType.BuildingTypeId);

                if (btype != null && btype.IdKey > 0)
                {
                    buildingNPCs.Add(npcType);
                }
                else
                {
                    noBuildingNPCs.Add(npcType);
                }
            }

            while (remainingPlaces.Count > 0 && buildingNPCs.Count > 0)
            {
                NPCType npc = buildingNPCs[rand.Next() % buildingNPCs.Count];
                buildingNPCs.Remove(npc);
                LocationPlace place = remainingPlaces[rand.Next() % remainingPlaces.Count];
                remainingPlaces.Remove(place);

                int dx = place.EntranceX - place.CenterX;
                int dz = place.EntranceZ - place.CenterZ;

                float rot = (float)(Math.Atan2(dz, dx) * 180f / Math.PI);

                List<IMapObjectAddon> addons = new List<IMapObjectAddon>
                {
                    new VendorAddon() { ItemCount = rand.Next() % 4 + 4, NPCTypeId = npc.IdKey }
                };

                int overridePercent = (int)(gs.md.overrideZoneScales[place.EntranceX,place.EntranceZ]);

                int unitSpawnX = place.EntranceZ - dz / 2;
                int unitSpawnZ = place.EntranceX - dx / 2;

                InitSpawnData initData = new InitSpawnData()
                {
                    EntityTypeId = EntityTypes.Unit,
                    EntityId = 1,
                    SpawnX = unitSpawnX,
                    SpawnZ = unitSpawnZ,
                    ZoneId = zone.IdKey,
                    LocationId = loc.Id,
                    LocationPlaceId = place.Id,
                    ZoneOverridePercent = overridePercent * MapConstants.OverrideZoneScaleMax,
                    Addons = addons,
                    Name = zoneName + " " + npc.Name,
                    Rot = rot,
                };

                gs.spawns.AddSpawn(initData);
                BuildingType btype = buildingTypes.FirstOrDefault(x=>x.IdKey == npc.BuildingTypeId);

                if (btype != null)
                {
                    InitSpawnData buildingInitData = new InitSpawnData()
                    {
                        EntityTypeId = EntityTypes.Building,
                        EntityId = npc.BuildingTypeId,
                        SpawnX = place.CenterZ,
                        SpawnZ = place.CenterX,
                        ZoneId = zone.IdKey,
                        LocationId = loc.Id,
                        LocationPlaceId = place.Id,
                        ZoneOverridePercent = overridePercent * MapConstants.OverrideZoneScaleMax,
                        Addons = null,
                        Name = zoneName + " " + btype.Name,
                        Rot = rot,
                    };

                    gs.spawns.AddSpawn(buildingInitData);
                }
            }
        }
    }

    public void CreateZones(UnityGameState gs)
    {
        if ( gs.map == null || gs.md.mapZoneIds == null)
        {
            return;
        }
        gs.map.Zones = new List<Zone>();

        ZoneType waterZoneType = _gameData.Get<ZoneTypeSettings>(gs.ch).GetData().FirstOrDefault(X => X.Name == "Water");

        if (waterZoneType != null)
        {
            Zone waterZone = new Zone()
            {
                IdKey = MapConstants.OceanZoneId,
                ZoneTypeId = waterZoneType.IdKey,
                Name = "The Great Ocean",
            };
            gs.map.Zones.Add(waterZone);
        }

        long currZoneId = MapConstants.MapZoneStartId;

        MyRandom rand = new MyRandom(gs.map.Seed & 100000000 + 323831);

        List<ZoneTypeGenData> zoneGenList = new List<ZoneTypeGenData>();

        foreach (ZoneType zt in _gameData.Get<ZoneTypeSettings>(gs.ch).GetData())
        {
            if (zt.IdKey < 1 || zt.GenChance <= 0)
            {
                continue;
            }

            ZoneTypeGenData zdata = new ZoneTypeGenData();
            zdata.zoneType = zt;
            zdata.chance = zt.GenChance;
            zoneGenList.Add(zdata);
        }

        if (zoneGenList.Count < 1)
        {
            return;
        }

        List<ZoneTypeGenData> randomizedZoneTypes = zoneGenList.OrderBy(x => Guid.NewGuid()).ToList();

        for (int i = 0; i < randomizedZoneTypes.Count; i++)
        {
            int baseZoneId = i + SharedMapConstants.MinBaseZoneId;
            if (baseZoneId > SharedMapConstants.MaxBaseZoneId)
            {
                break;
            }

            Zone baseZone = _zoneGenService.Generate(gs, baseZoneId, randomizedZoneTypes[i].zoneType.IdKey, rand.Next() % 1000000000);
            gs.map.Zones.Add(baseZone);
        }

        List<MyPoint> newCenters = new List<MyPoint>(gs.md.zoneCenters);

        int zonedelta = (int)(gs.map.ZoneSize * MapConstants.TerrainPatchSize / 12);

        int minRad = 50 + rand.Next() % 26;
        int maxRad = minRad * 3 / 2;

        gs.md.zoneCenters = new List<MyPoint>();
        while (newCenters.Count > 0)
        {
            int pos = rand.Next() % newCenters.Count;
            MyPoint center = newCenters[pos];
            gs.md.zoneCenters.Add(center);
            newCenters.Remove(center);

            ZoneType zoneTypeChosen = ChooseNextZoneType(gs, rand, zoneGenList, center.X, center.Y);

            Zone zone = _zoneGenService.Generate(gs, currZoneId, zoneTypeChosen.IdKey, rand.Next() % 1000000000);

            if (zone == null)
            {
                continue;
            }
            currZoneId++;

            Location finalCenter = new Location()
            {
                CenterX = (int)center.X + MathUtils.IntRange(-zonedelta, zonedelta, rand),
                CenterZ = (int)center.Y + MathUtils.IntRange(-zonedelta, zonedelta, rand),
                LocationTypeId = LocationTypes.ZoneCenter,
                XSize = MathUtils.IntRange(minRad, maxRad, rand),
                ZSize = MathUtils.IntRange(minRad, maxRad, rand),
            };

            gs.map.Zones.Add(zone);
            gs.map.ClearIndex();
            gs.md.mapZoneIds[finalCenter.CenterX, finalCenter.CenterZ] = (short)zone.IdKey;
            _logService.Info("ZoneCenterZoneId at (" + finalCenter.CenterX + "," + finalCenter.CenterZ + ") is " +
                gs.md.mapZoneIds[finalCenter.CenterX, finalCenter.CenterZ]);
            gs.md.AddMapLocation(gs, finalCenter);
        }


        ConnectZones(gs, rand);
        SetMinMaxSizes(gs);
        SetZoneLevels(gs, gs.md.zoneCenters);

        List<Zone> allZones = gs.map.Zones.Where(x => x.IdKey >= SharedMapConstants.MinBaseZoneId).ToList();

        if (allZones.Count > 0)
        {
            gs.map.OverrideZoneId = allZones[gs.rand.Next() % allZones.Count].IdKey;
            gs.map.OverrideZonePercent = 0;
        }
    }


    private void ConnectZones(UnityGameState gs, MyRandom rand)
    {
        while (true)
        {
            bool haveUnsetCell = false;

            short[,] zoneIds = gs.md.mapZoneIds;
            List<short> adjacentZones = new List<short>();
            int[] sizes = new int[2];
            int[] shift = new int[2];
            int pass = 0;
            for (int x = 0; x < gs.map.GetHwid(); x++)
            {
                for (int z = 0; z < gs.map.GetHhgt(); z++)
                {
                    if (zoneIds[x, z] < 0 || zoneIds[x, z] >= MapConstants.MapZoneStartId)
                    {
                        continue;
                    }

                    haveUnsetCell = true;

                    adjacentZones.Clear();
                    AddAdjacentZoneId(zoneIds, adjacentZones, x - 1, z);
                    AddAdjacentZoneId(zoneIds, adjacentZones, x + 1, z);
                    AddAdjacentZoneId(zoneIds, adjacentZones, x, z - 1);
                    AddAdjacentZoneId(zoneIds, adjacentZones, x, z + 1);

                    if (adjacentZones.Count < 1)
                    {
                        continue;
                    }

                    short newZoneId = adjacentZones[rand.Next() % adjacentZones.Count];

                    Zone zone = gs.map.Zones.FirstOrDefault(x => x.IdKey == newZoneId);
                    if (zone == null)
                    {
                        continue;
                    }

                    GenZone genZone = gs.md.GetGenZone(zone.IdKey);

                    if (pass > 0 && rand.NextDouble() > genZone.SpreadChance)
                    {
                        continue;
                    }
                    int minSize = (int)(gs.map.ZoneSize * MapConstants.TerrainPatchSize / 30);
                    int maxSize = minSize * 2;
                    sizes[0] = MathUtils.IntRange(minSize, maxSize, rand);
                    sizes[1] = sizes[0];
                    for (int i = 0; i < 2; i++)
                    {
                        sizes[i] += MathUtils.IntRange(0, sizes[i] / 3, rand);
                        if (rand.NextDouble() < 0.2f)
                        {
                            sizes[i] += MathUtils.IntRange(0, sizes[i] / 2, rand);
                        }
                        shift[i] = MathUtils.IntRange(-sizes[i] / 2, sizes[i] / 2, rand);
                    }


                    float currentPower = MathUtils.FloatRange(2.0f, 2.7f, rand);
                    float rot = MathUtils.FloatRange(0, (float)Math.PI/2,rand);
                    float sin = (float)Math.Sin(rot);
                    float cos = (float)Math.Cos(rot);
                    for (float xx = x - sizes[0] + shift[0]; xx < x + sizes[0] + shift[0]; xx += 0.5f)
                    {
                        float dx = xx - x - shift[0];

                        for (float zz = z - sizes[1] + shift[1]; zz < z + sizes[1] + shift[1]; zz += 0.5f)
                        {
                            float dz = zz - z - shift[1];

                            int nx = x + (int)(Math.Round(dx * cos + dz * sin));
                            int nz = z + (int)(Math.Round(-dx * sin + dz * cos));

                            if (nx < 0 || nx >= gs.map.GetHwid() ||
                                nz < 0 || nz >= gs.map.GetHhgt())
                            {
                                continue;
                            }

                            if (zoneIds[nx, nz] != 0)
                            {
                                continue;
                            }

                            float xpct = Math.Abs((xx - x) / sizes[0]);
                            float zpct = Math.Abs((zz - z) / sizes[1]);

                            xpct = (float)(Math.Pow(xpct, currentPower));
                            zpct = (float)(Math.Pow(zpct, currentPower));

                            if (xpct+zpct > 1)
                            {
                                continue;
                            }

                            zoneIds[nx, nz] = (short)-newZoneId;
                        }
                    }
                    pass++;
                }
            }
            for (int x = 0; x < gs.map.GetHwid(); x++)
            {
                for (int z = 0; z < gs.map.GetHhgt(); z++)
                {
                    if (zoneIds[x, z] < 0)
                    {
                        zoneIds[x, z] = (short)(-zoneIds[x, z]);
                    }
                    if (zoneIds[x,z] < SharedMapConstants.MapZoneStartId)
                    {
                        haveUnsetCell = true;
                    }
                }
            }

            if (!haveUnsetCell)
            {
                bool reallyHaveUnsetCell = false;
                for (int x = 0; x < gs.map.GetHwid(); x++)
                {
                    for (int z = 0; z < gs.map.GetHhgt(); z++)
                    {
                        if (gs.md.mapZoneIds[x,z] < SharedMapConstants.MapZoneStartId)
                        {
                            reallyHaveUnsetCell = true;
                            _logService.Message("Cell slipped through processing: " + x + " " + z);
                        }
                    }
                }
                if (!reallyHaveUnsetCell)
                {
                    break;
                }
            }
        }
    }

    private void AddAdjacentZoneId(short[,] zoneIds, List<short> adjacentZones, int x, int z)
    {
        if (x < 0 || x >= zoneIds.GetLength(0) || z < 0 || z >= zoneIds.GetLength(1) ||
            zoneIds[x,z] < MapConstants.MapZoneStartId)
        {
            return;
        }
        adjacentZones.Add(zoneIds[x, z]);
    }
}
