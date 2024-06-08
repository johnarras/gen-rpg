using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.ProcGen.Entities;

using System.Threading;
using Genrpg.Shared.ProcGen.Settings.Rocks;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;

internal class FullRockType
{
	public RockType rockType;
	public ZoneRockType zoneTypeRock;
	public ZoneRockType zoneRock;
	public int numPlaced;

	public float weight;


	public List<MyPoint> PlacedRocks;

    public string assetCategory = AssetCategoryNames.Rocks;

	public string assetName = "";
	public string fullURL = "";
	public FullRockType()
	{
		PlacedRocks = new List<MyPoint>();
	}
}

public class AddRocks : BaseZoneGenerator
{
    public const float RandomRockDensity = 1.0f / 4000.0f;
    public int TriesPerRock = 20;
    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.XMin, zone.ZMin, zone.XMax, zone.ZMax);
        }
    }

    public void GenerateOne(Zone zone, ZoneType zoneType, int startx, int starty, int endx, int endy)
    {
        GenZone genZone = _md.GetGenZone(zone.IdKey);

        if (endx <= startx || endy <= starty)
        {
            return;
        }

        int dx = endx - startx;
        int dy = endy - starty;

        MyRandom rand = new MyRandom(zone.Seed % 2000000000 + 15434454);


        float densityMult = MathUtils.FloatRange(1.0f, 1.5f, rand);

        List<FullRockType> list = new List<FullRockType>();

        foreach (ZoneRockType zrt in genZone.RockTypes)
        {
            ZoneRockType ztrt = null;
            foreach (ZoneRockType item in zoneType.RockTypes)
            {
                if (item.RockTypeId == zrt.RockTypeId)
                {
                    ztrt = item;
                    break;
                }
            }

            if (ztrt == null)
            {
                continue;
            }

            RockType rt = _gameData.Get<RockTypeSettings>(_gs.ch).Get(zrt.RockTypeId);
            if (rt == null)
            {
                continue;
            }

            if (rt.ChanceScale <= 0.0f || ztrt.ChanceScale <= 0.0f)
            {
                continue;
            }

            float weight = rt.ChanceScale * ztrt.ChanceScale * zrt.ChanceScale;

            if (weight <= 0)
            {
                continue;
            }


            FullRockType full = new FullRockType();
            full.zoneRock = zrt;
            full.zoneTypeRock = ztrt;
            full.rockType = rt;
            full.weight = weight;
            full.assetName = rt.Name;
            full.fullURL = full.assetName;
            full.assetCategory = AssetCategoryNames.Rocks;
            list.Add(full);

        }

        AddNearbyItemsHelper nearbyHelper = new AddNearbyItemsHelper();



        int size = Math.Max(zone.XMax - zone.XMin, zone.ZMax - zone.ZMin);

        long area = (zone.XMax - zone.XMin) * (zone.ZMax - zone.ZMin);

        long totalNumber = (long)((area * RandomRockDensity) * zoneType.RockDensity * densityMult);


        long totalTries = (long)(totalNumber * TriesPerRock);


        int totalPlaced = 0;
        for (long times = 0; times < totalTries; times++)
        {

            if (totalPlaced >= totalNumber)
            {
                break;
            }

            int x = MathUtils.IntRange(startx, endx, rand);
            int y = MathUtils.IntRange(starty, endy, rand);

            

            if (_zoneGenService.FindMapLocation(x, y, 10) != null)
            {
                continue;
            }

            
            if (_md.mapZoneIds[x,y] != zone.IdKey) // zoneobject
            {
                continue;
            }

            if (_md.roadDistances[x, y] < 10)
            {
                continue;
            }


            int currQuantityToPlace = 1;

            while (rand.NextDouble() < 0.2f && rand.Next() % currQuantityToPlace < 2)
            {
                currQuantityToPlace += rand.Next() % 3 + 1;
            }

            int maxOffset = currQuantityToPlace / 3;

            bool didFinalPlace = false;

            List<long> placedList = new List<long>();
            for (int p = 0; p < currQuantityToPlace; p++)
            {

                int nearbyItemsCount = nearbyHelper.GetNearbyItemsCount(maxOffset, rand);

                FullRockType frt = null;

                double totalChance = 0;
                foreach (FullRockType item in list)
                {
                    if (placedList.Contains(item.rockType.IdKey) &&
                        item.rockType.MaxPerZone > 0)
                    {
                        continue;
                    }

                    totalChance += item.weight;

                }

                if (totalChance <= 0)
                {
                    if (list.Count < 1)
                    {
                        break;
                    }
                    frt = list[rand.Next() % list.Count];

                }
                else
                {
                    double chanceChosen = rand.NextDouble() * totalChance;

                    foreach (FullRockType item in list)
                    {
                        if (item.rockType.MaxPerZone > 0 && placedList.Contains(item.rockType.IdKey))
                        {
                            continue;
                        }
                        chanceChosen -= item.weight;
                        if (chanceChosen <= 0)
                        {
                            frt = item;
                            break;
                        }
                    }

                }

                if (frt == null)
                {
                    continue;
                }


                int px = x + MathUtils.IntRange(-maxOffset, maxOffset, rand);
                int pz = y + MathUtils.IntRange(-maxOffset, maxOffset, rand);

                px -= px / (MapConstants.TerrainPatchSize - 1);
                pz -= pz / (MapConstants.TerrainPatchSize - 1);

                int rdx = px - x;
                int rdy = pz - y;

                float rdist = (float)Math.Sqrt(rdx * rdx + rdy * rdy);
              

                int ipx = (int)(px);
                int ipy = (int)(pz);


                if (ipx < 0 || ipy < 0 || ipx >= _mapProvider.GetMap().GetHwid() || ipy >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

                if (_md.roadDistances[ipx, ipy] < 3)
                {
                    continue;
                }
                float posHeight = _terrainManager.GetInterpolatedHeight(ipx, ipy);

                if (posHeight < MapConstants.MinLandHeight)
                {
                    continue;
                }

                if (_md.mapObjects != null && 
                    _md.mapObjects[ipx, ipy] == 0)
                {
                    int offset =  MapConstants.RockObjectOffset;

                    _md.mapObjects[ipx, ipy] = (int)(offset + frt.rockType.IdKey);

                    didFinalPlace = true;

                    if (rand.NextDouble() * currQuantityToPlace > densityMult)
                    {
                        int numToPlace = 8 - (currQuantityToPlace + 1) / 2;
                        if (numToPlace < 3)
                        {
                            numToPlace = 3;
                        }

                        float currMaxOffset = MathUtils.FloatRange(1.1f, 2.1f, rand);
                        float currMinOffset = currMaxOffset / 2;
                        nearbyHelper.AddItemsNear(_gs, _gameData, _terrainManager, _mapProvider, rand, zoneType, zone, x, y, 0.9f, nearbyItemsCount, currMinOffset, currMaxOffset);
                    }
                }

                if (!placedList.Contains(frt.rockType.IdKey))
                {
                    placedList.Add(frt.rockType.IdKey);
                }

                frt.numPlaced++;
                if (frt.rockType.MaxPerZone > 0 && frt.numPlaced >= frt.rockType.MaxPerZone)
                {
                    list.Remove(frt);
                }                
            }

            if (didFinalPlace)
            {
                totalPlaced++;
            }
        }
    }
}