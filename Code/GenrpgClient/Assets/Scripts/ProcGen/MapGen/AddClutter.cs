using System;
using System.Collections.Generic;

using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.Clutter;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using UnityEngine;

public class AddClutter : BaseZoneGenerator
{
    public const int MaxChoicesPerClutterType = 3;
    public const float MaxSteepness = 15;
    public const float RandomClutterDensity = 0.00025f;

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

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

        MyRandom rand = new MyRandom(zone.Seed % 2000000000 + 15434454);


        float clutterDensity = MathUtils.FloatRange(0.0f, 1.0f, rand) * RandomClutterDensity;

        if (_gameData.Get<ClutterTypeSettings>(_gs.ch).GetData() == null)
        {
            return;
        }

        AddNearbyItemsHelper nearbyHelper = new AddNearbyItemsHelper();

        int totalPlaced = 0;


        int size = Math.Max(zone.XMax - zone.XMin, zone.ZMax - zone.ZMin);

        int area = (zone.XMax - zone.XMin) * (zone.ZMax - zone.ZMin);

        int totalNumber = (int)(area * clutterDensity);

        int totalTries = 20 * totalNumber;
        for (long times = 0; times < totalTries; times++)
        {

            if (totalPlaced >= totalNumber)
            {
                break;
            }

            int x = MathUtils.IntRange(startx, endx, rand);
            int y = MathUtils.IntRange(starty, endy, rand);

            if (FlagUtils.IsSet(_md.flags[x, y], MapGenFlags.BelowWater))
            {
                continue;
            }

            if (x < 0 || x >= _mapProvider.GetMap().GetHwid() || y < 0 || y >= _mapProvider.GetMap().GetHhgt())
            {
                continue;
            }

            if (_zoneGenService.FindMapLocation(x,y, 5) != null)
            {
                continue;
            }

            if (_md.mapZoneIds[x, y] != zone.IdKey) // zoneobject
            {
                continue;
            }

            if (_md.roadDistances[x, y] < 10)
            {
                continue;
            }
            if (_md.alphas[x, y, MapConstants.RoadTerrainIndex] > 0)
            {
                continue;
            }


            if (_terrainManager.GetSteepness(x, y) > MaxSteepness)
            {
                continue;
            }

            int currQuantityToPlace = 2 + MathUtils.IntRange(0, 1, rand);


            for (int i = 0; i < 5; i++)
            {
                if (rand.NextDouble() < 0.3f)
                {
                    currQuantityToPlace++;
                }
                else
                {
                    break;
                }
            }

            if (rand.NextDouble() < 0.1f)
            {
                currQuantityToPlace += MathUtils.IntRange(0, currQuantityToPlace / 2, rand);
            }

            int maxOffset = 1;

            if (currQuantityToPlace > 8)
            {
                maxOffset++;
            }

            List<MyPoint2> openPositions = new List<MyPoint2>();

            for (int xx = x - maxOffset; xx <= x + maxOffset; xx++)
            {
                if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                {
                    continue;
                }

                for (int yy = y - maxOffset; yy <= y + maxOffset; yy++)
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

            totalPlaced++;

            int totalClutterChoices = 0;
            Dictionary<ClutterType, int> clutterWeights = new Dictionary<ClutterType, int>();
            foreach (ClutterType ctype in _gameData.Get<ClutterTypeSettings>(_gs.ch).GetData())
            {
                if (ctype.NumChoices > 0)
                {
                    clutterWeights[ctype] = rand.Next() % 20 + 1;
                    totalClutterChoices += clutterWeights[ctype];
                }
            }

            if (totalClutterChoices < 1)
            {
                break;
            }

            for (int p = 0; p < currQuantityToPlace; p++)
            {

                if (openPositions.Count < 1)
                {
                    continue;
                }

                MyPoint2 pos = openPositions[rand.Next() % openPositions.Count];
                int px = (int)(pos.X);
                int py = (int)(pos.Y);
                openPositions.Remove(pos);

                int nearbyItemsCount = nearbyHelper.GetNearbyItemsCount(maxOffset, rand);

                int clutterTypeChosen = rand.Next() % totalClutterChoices;

                ClutterType ctypeChosen = null;
                foreach (ClutterType ctype2 in clutterWeights.Keys)
                {
                    clutterTypeChosen -= clutterWeights[ctype2];
                    if (clutterTypeChosen < 0)
                    {
                        ctypeChosen = ctype2;
                        break;
                    }
                }

                if (ctypeChosen == null)
                {
                    continue;
                }

                if (_md.mapObjects[px, py] == 0)
                {
                    _md.mapObjects[px, py] = (int)(MapConstants.ClutterObjectOffset + ctypeChosen.IdKey);
                }
            }
            int numToPlace = 4 + (currQuantityToPlace + 1) / 2;
            if (numToPlace < 3)
            {
                numToPlace = 3;
            }

            double rval = rand.NextDouble();
            if (rval <= 0.3f)
            {
                numToPlace = 0;
            }
            else if (rval <= 0.85f)
            {
                numToPlace /= 2;
            }

            float currMaxOffset = MathUtils.FloatRange(0.7f, 1.2f, rand);
            nearbyHelper.AddItemsNear(_gs, _gameData, _terrainManager, _mapProvider, rand, zoneType, zone, x, y, 0.9f, numToPlace, 1.0f, currMaxOffset);
        }
    }
}