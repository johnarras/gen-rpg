using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.Clutter;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;

public class AddClutter : BaseZoneGenerator
{
    public const int MaxChoicesPerClutterType = 3;
    public const float MaxSteepness = 15;
    public const float RandomClutterDensity = 0.00025f;

    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        foreach (Zone zone in gs.map.Zones)
        {
            GenerateOne(gs, zone, gs.data.GetGameData<ZoneTypeSettings>(gs.ch).GetZoneType(zone.ZoneTypeId), zone.XMin, zone.ZMin, zone.XMax, zone.ZMax);
        }
    }

    public void GenerateOne(UnityGameState gs, Zone zone, ZoneType zoneType, int startx, int starty, int endx, int endy)
    {
        if (zone == null || endx <= startx || endy <= starty)
        {
            return;
        }


        int dx = endx - startx;
        int dy = endy - starty;

        MyRandom rand = new MyRandom(zone.Seed % 2000000000 + 15434454);


        float clutterDensity = MathUtils.FloatRange(0.0f, 1.0f, rand) * RandomClutterDensity;

        if (gs.data.GetGameData<ClutterTypeSettings>(gs.ch).GetData() == null)
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

            if (FlagUtils.IsSet(gs.md.flags[x, y], MapGenFlags.BelowWater))
            {
                continue;
            }

            if (x < 0 || x >= gs.map.GetHwid() || y < 0 || y >= gs.map.GetHhgt())
            {
                continue;
            }

            if (_zoneGenService.FindMapLocation(gs, x,y, 5) != null)
            {
                continue;
            }

            if (gs.md.mapZoneIds[x, y] != zone.IdKey) // zoneobject
            {
                continue;
            }

            if (gs.md.roadDistances[x, y] < 10)
            {
                continue;
            }
            if (gs.md.alphas[x, y, MapConstants.RoadTerrainIndex] > 0)
            {
                continue;
            }


            if (gs.md.GetSteepness(gs, x, y) > MaxSteepness)
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
                if (xx < 0 || xx >= gs.map.GetHwid())
                {
                    continue;
                }

                for (int yy = y - maxOffset; yy <= y + maxOffset; yy++)
                {
                    if (yy < 0 || yy >= gs.map.GetHhgt())
                    {
                        continue;
                    }

                    if (gs.md.mapObjects[xx, yy] != 0)
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
            foreach (ClutterType ctype in gs.data.GetGameData<ClutterTypeSettings>(gs.ch).GetData())
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

                int nearbyItemsCount = nearbyHelper.GetNearbyItemsCount(gs, maxOffset, rand);

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

                if (gs.md.mapObjects[px, py] == 0)
                {
                    gs.md.mapObjects[px, py] = (int)(MapConstants.ClutterObjectOffset + ctypeChosen.IdKey);
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
            nearbyHelper.AddItemsNear(gs, rand, zoneType, zone, x, y, 0.9f, numToPlace, 1.0f, currMaxOffset);
        }
    }
}