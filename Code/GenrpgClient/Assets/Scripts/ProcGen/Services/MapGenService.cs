using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Zones.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.Factions.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapServer.Constants;
using System.Threading.Tasks;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Inventory.Constants;

public interface IMapGenService : IService
{

    Map GenerateMap(UnityGameState gs, Map startMap);
    void CreateZones(UnityGameState gs);
    void SetPrevNextZones(UnityGameState gs);
    void AddNPCs(UnityGameState gs);
    void AddNPC(UnityGameState gs, NPCType npcType);
}


public class MapGenService : IMapGenService
{
    protected IZoneGenService _zoneGenService;
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
            startMap.MaxLevel = 100;
        }
        map.MinLevel = startMap.MinLevel;
        map.MaxLevel = startMap.MaxLevel;
        map.Id = startMap.Id;
        map.Seed = startMap.Seed;
        map.BlockCount = startMap.BlockCount;
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


    private void SetZoneLevels(UnityGameState gs, List<Location> validCenters)
    {
        if ( gs.map == null || gs.map.Zones == null || validCenters == null)
        {
            return;
        }

        int minLevel = gs.map.MinLevel;
        int maxLevel = gs.map.MaxLevel;

        double minLevelRadius = gs.map.GetHwid() + gs.map.GetHhgt();
        double maxLevelRadius = 0;

        foreach (Location center in validCenters)
        {
            double dist = Math.Sqrt(center.CenterX * center.CenterX + center.CenterZ * center.CenterZ);
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
            GenZone genZone = gs.GetGenZone(zone.IdKey);
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

    protected virtual long GetZoneTypeId(UnityGameState gs, Map map, MyRandom rand)
    {
        if (map == null || rand == null)
        {
            return 0;
        }


        List<ZoneType> zoneTypes = gs.data.GetGameData<ZoneTypeSettings>(gs.ch).GetData();
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
            gs.logger.Debug("Add NPCS To Zone: " + zone.IdKey);
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

        for (int l = 0; l < zone.Locations.Count; l++)
        {
            Location loc = zone.Locations[l];
            gs.logger.Debug("Add NPC To Zone: " + zone.IdKey + " at " + loc.CenterX + " " + loc.CenterZ);
            int offsetMult = 0;
            int offsetX = offsetMult * loc.CenterX / (MapConstants.TerrainPatchSize - 1);
            int offsetZ = offsetMult * loc.CenterZ / (MapConstants.TerrainPatchSize - 1);

            NPCType npcType = new NPCType()
            {
                Name = zoneName + "#" + (l + 1),
                QualityTypeId = QualityTypes.Rare,
                MapX = loc.CenterZ + offsetZ,
                MapZ = loc.CenterX + offsetX,
                UnitTypeId = 1,
                FactionTypeId = FactionTypes.Player,
                ItemCount = rand.Next() % 3 + 3,
                ItemQualityTypeId = 0,
            };

            AddNPC(gs, npcType);
        }
    }

    public void AddNPC(UnityGameState gs, NPCType npcType)
    {
        if (npcType.MapX < 0 || npcType.MapZ < 0 || npcType.MapX >= gs.map.GetHwid() || npcType.MapZ >= gs.map.GetHhgt())
        {
            return;
        }

        long npcId = 1;
        if (gs.map.NPCs.Count > 0)
        {
            npcId = gs.map.NPCs.Max(X => X.IdKey) + 1;
        }
        npcType.IdKey = npcId;
        npcType.MapId = gs.map.Id;

        npcType.ZoneId = gs.md.mapZoneIds[npcType.MapX, npcType.MapZ];

        gs.logger.Debug("New NPCID: " + npcId + " in zone " + npcType.ZoneId + " at " + npcType.MapX + " " + npcType.MapZ);

        Zone zone = gs.map.Get<Zone>(npcType.ZoneId);
        if (zone != null)
        {
            npcType.Level = zone.Level;
        }

        gs.spawns.AddSpawn(EntityTypes.NPC, npcType.IdKey, npcType.MapZ, npcType.MapX, npcType.ZoneId);
        gs.spawns.NPCs.Add(new NPCStatus() { IdKey = npcType.IdKey });

        gs.map.NPCs.Add(npcType);
    }

    public void CreateZones(UnityGameState gs)
    {
        if ( gs.map == null || gs.md.mapZoneIds == null)
        {
            return;
        }
        gs.map.Zones = new List<Zone>();

        ZoneType waterZoneType = gs.data.GetGameData<ZoneTypeSettings>(gs.ch).GetData().FirstOrDefault(X => X.Name == "Water");

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

        foreach (ZoneType zt in gs.data.GetGameData<ZoneTypeSettings>(gs.ch).GetData())
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

        List<Location> newCenters = new List<Location>(gs.md.zoneCenters);

        int zonedelta = (int)(gs.map.ZoneSize * MapConstants.TerrainPatchSize / 12);

        int minRad = 30;
        int maxRad = 40;

        gs.md.zoneCenters = new List<Location>();
        while (newCenters.Count > 0)
        {
            int pos = rand.Next() % newCenters.Count;
            Location center = newCenters[pos];
            gs.md.zoneCenters.Add(center);
            newCenters.Remove(center);

            ZoneType zoneTypeChosen = ChooseNextZoneType(gs, rand, zoneGenList, center.CenterX, center.CenterZ);

            Zone zone = _zoneGenService.Generate(gs, currZoneId, zoneTypeChosen.IdKey, rand.Next() % 1000000000);

            if (zone == null)
            {
                continue;
            }
            currZoneId++;
            center.LocationTypeId = LocationType.ZoneCenter;

            Location finalCenter = new Location()
            {
                CenterX = center.CenterX + MathUtils.IntRange(-zonedelta, zonedelta, rand),
                CenterZ = center.CenterZ + MathUtils.IntRange(-zonedelta, zonedelta, rand),
                LocationTypeId = LocationType.ZoneCenter,
                XSize = MathUtils.IntRange(minRad, maxRad, rand),
                ZSize = MathUtils.IntRange(minRad, maxRad, rand),
            };


            gs.map.Zones.Add(zone);
            gs.md.mapZoneIds[finalCenter.CenterX, finalCenter.CenterZ] = (short)zone.IdKey;
            zone.Locations.Add(finalCenter);
            gs.logger.Debug("Add Center to zone: " + zone.IdKey + " at " + finalCenter.CenterX + " " + finalCenter.CenterZ);
            gs.md.AddMapLocation(gs, finalCenter);
        }

        ConnectZones(gs, rand);
        SetMinMaxSizes(gs);
        SetZoneLevels(gs, gs.md.zoneCenters);

    }


    private async void ConnectZones(UnityGameState gs, MyRandom rand)
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

                    GenZone genZone = gs.GetGenZone(zone.IdKey);

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
                            gs.logger.Message("Cell slipped through processing: " + x + " " + z);
                            await Task.Delay(2000);
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
