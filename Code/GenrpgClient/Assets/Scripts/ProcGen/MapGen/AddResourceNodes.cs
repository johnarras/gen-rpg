using System;
using System.Linq;
using System.Collections.Generic;

using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Entities.Constants;
using System.Threading;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.GroundObjects.Settings;
using UnityEngine;

public class ZoneResourceNodeData
{
    public ResourceNodeData Data;
    public int CurrNum;
    public int MaxNum;
    public double Density;
    public List<GroundObjType> Objects;
    public int SpawnWeightSum = 0;
}

public class ResourceNodeData
{
    public string GroupId;
    public bool NearMountains;
    public int MinDistToFeatures;
    public float Density;
}


public class AddResourceNodes : BaseZoneGenerator
{
    private List<ResourceNodeData> _resources = null;


    public override async Awaitable Generate(CancellationToken token)
    {

        await base.Generate(token);
        _resources = new List<ResourceNodeData>();
        _resources.Add(new ResourceNodeData()
        {
            GroupId = "herb",
            NearMountains = false,
            MinDistToFeatures = 10,
            Density = 0.00006f,

        });
        _resources.Add(new ResourceNodeData()
        {
            GroupId = "metal",
            NearMountains = true,
            MinDistToFeatures = 25,
            Density = 0.00005f,
        });


        _resources.Add(new ResourceNodeData()
        {
            GroupId = "stone",
            NearMountains = true,
            MinDistToFeatures = 25,
            Density = 0.00005f,

        });


        _resources.Add(new ResourceNodeData()
        {
            GroupId = "wood",
            NearMountains = false,
            MinDistToFeatures = 25,
            Density = 0.00005f,

        });

        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.XMin, zone.ZMin, zone.XMax, zone.ZMax);
        }
    }

    public void GenerateOne(Zone zone, ZoneType zoneType, int startx, int starty, int endx, int endy)
    {
        if (zone == null || endx <= startx || endy <= starty)
        {
            return;
        }


        int dx = endx - startx;
        int dy = endy - starty;

        List<ZoneResourceNodeData> zoneList = new List<ZoneResourceNodeData>();

        MyRandom rand = new MyRandom(zone.Seed % 1500000000 + 15423);

        AddNearbyItemsHelper nearbyHelper = new AddNearbyItemsHelper();


        int size = Math.Max(zone.XMax - zone.XMin, zone.ZMax - zone.ZMin);

        int area = (zone.XMax - zone.XMin) * (zone.ZMax - zone.ZMin);


        int totalPlacements = 0;

        for (int r = 0; r < _resources.Count; r++)
        {
            ResourceNodeData rdata = _resources[r];
            List<GroundObjType> categoryObjects = _gameData.Get<GroundObjTypeSettings>(_gs.ch).GetData().Where(x => x.GroupId == rdata.GroupId).ToList();

            if (categoryObjects.Count < 1)
            {
                continue;
            }

            int weightSum = 0;

            foreach (GroundObjType obj in categoryObjects)
            {
                weightSum += obj.SpawnWeight;
            }

            if (weightSum < 1)
            {
                continue;
            }

            ZoneResourceNodeData zdata = new ZoneResourceNodeData() { Data = rdata };
            zoneList.Add(zdata);

            zdata.Objects = categoryObjects;
            zdata.SpawnWeightSum = categoryObjects.Sum(x => x.SpawnWeight);
            zdata.CurrNum = 0;
            zdata.Density = MathUtils.FloatRange(0.0f, 2.0f, rand) * rdata.Density;
            zdata.MaxNum = (int)(area * zdata.Density);

            totalPlacements += zdata.MaxNum;
        }


        int totalTries = 40 * totalPlacements;

        for (long times = 0; times < totalTries; times++)
        {

            int totalToPlace = 0;

            foreach (ZoneResourceNodeData data in zoneList)
            {
                totalToPlace += (data.MaxNum - data.CurrNum);
            }

            if (totalToPlace < 1)
            {
                break;
            }

            int placeChosen = _rand.Next() % totalToPlace;

            ZoneResourceNodeData zdata = null;

            foreach (ZoneResourceNodeData data in zoneList)
            {
                placeChosen -= (data.MaxNum - data.CurrNum);
                if (placeChosen <= 0)
                {
                    zdata = data;
                    break;
                }
            }

            int x = MathUtils.IntRange(startx, endx, rand);
            int y = MathUtils.IntRange(starty, endy, rand);

            int cx = x + (int)(x / (MapConstants.TerrainPatchSize - 1));
            int cy = y + (int)(y / (MapConstants.TerrainPatchSize - 1));

            if (cx < 0 || cx >= _mapProvider.GetMap().GetHwid() || cy < 0 || cy >= _mapProvider.GetMap().GetHhgt())
            {
                continue;
            }

            if (_zoneGenService.FindMapLocation(y, x, zdata.Data.MinDistToFeatures) != null)
            {
                continue;
            }

            
            if (_md.mapZoneIds[cx, cy] != zone.IdKey) // zoneobject
            {
                continue;
            }

            if (FlagUtils.IsSet(_md.flags[cx, cy], MapGenFlags.BelowWater))
            {
                continue;
            }
            if (_md.roadDistances[x, y] < zdata.Data.MinDistToFeatures)
            {
                continue;
            }
            if (_md.alphas[cx, cy, MapConstants.RoadTerrainIndex] > 0)
            {
                continue;
            }

            if (FlagUtils.IsSet(_md.flags[cx,cy],MapGenFlags.NearResourceNode))
            {
                continue;
            }

            if (zdata.Data.NearMountains != (_md.mountainHeights[cx, cy] <= 0))
            {
                continue;
            }


            int maxOffset = 1;

            List<MyPoint2> openPositions = new List<MyPoint2>();

            for (int xx = cx - maxOffset; xx <= cx + maxOffset; xx++)
            {
                if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                {
                    continue;
                }

                for (int yy = cy - maxOffset; yy <= cy + maxOffset; yy++)
                {
                    if (yy < 0 || yy >= _mapProvider.GetMap().GetHhgt())
                    {
                        continue;
                    }

                    if (_md.mapObjects[xx, yy] != 0)
                    {
                        continue;
                    }
                    openPositions.Add(new MyPoint2(xx, yy));
                }
            }

            if (openPositions.Count < 1)
            {
                continue;
            }

            if (openPositions.Count < 1)
            {
                continue;
            }

            MyPoint2 pos = openPositions[rand.Next() % openPositions.Count];
            int px = (int)(pos.X);
            int py = (int)(pos.Y);
            openPositions.Remove(pos);

            int clutterIndex = 0;

            if (zdata.SpawnWeightSum < 1)
            {
                clutterIndex = rand.Next() % zdata.Objects.Count;
            }
            else
            {
                int clutterWeight = _rand.Next() % zdata.SpawnWeightSum;
                for (int i = 0; i < zdata.Objects.Count; i++)
                {
                    clutterWeight -= zdata.Objects[i].SpawnWeight;
                    if (clutterWeight <= 0)
                    {
                        clutterIndex = i;
                    }
                }
            }

            if (clutterIndex < 0 || clutterIndex >= zdata.Objects.Count)
            {
                continue;
            }

            GroundObjType goType = zdata.Objects[clutterIndex];

            InitSpawnData initData = new InitSpawnData()
            {
                EntityTypeId = EntityTypes.GroundObject,
                EntityId = goType.IdKey,
                SpawnX = py,
                SpawnZ = px,
                ZoneId = _md.mapZoneIds[cx, cy],
                ZoneOverridePercent = (int)(_md.overrideZoneScales[cx, cy] * MapConstants.OverrideZoneScaleMax),
            };

            _mapProvider.GetSpawns().AddSpawn(initData);

            zdata.CurrNum++;

            for (int xx = px-MapConstants.MinResourceSeparation; xx <= px+MapConstants.MinResourceSeparation; xx++)
            {
                if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                {
                    continue;
                }

                int ddx = xx - px;
                for (int yy = py-MapConstants.MinResourceSeparation; yy <= py+MapConstants.MinResourceSeparation; yy++)
                {
                    if (yy < 0 || yy >= _mapProvider.GetMap().GetHhgt())
                    {
                        continue;
                    }

                    int ddy = yy - py;
                    if (Math.Sqrt(ddx * ddx + ddy * ddy) > MapConstants.MinResourceSeparation)
                    {
                        continue;
                    }

                    _md.flags[xx, yy] |= MapGenFlags.NearResourceNode;
                }
            }

        }
    }
}