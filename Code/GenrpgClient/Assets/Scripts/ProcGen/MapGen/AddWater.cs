using System;
using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.MapWater;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using Assets.Scripts.ProcGen.Loading.Utils;
using UnityEngine;

public class AddWater : BaseZoneGenerator
{

    private IAddPoolService _addPoolService;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone);
        }
    }

    /// <summary>
    /// Attempt to add Pool(s) to a given zone.
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="zone"></param>
    public void GenerateOne(Zone zone)
    {
        if (zone == null)
        {
            return;
        }

        ZoneType ztype = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId);
        if (ztype == null)
        {
            return;
        }

        int minx = zone.XMin;
        int maxx = zone.XMax;
        int minz = zone.ZMin;
        int maxz = zone.ZMax;


        int totalSize = (maxx - minx) * (maxz - minz);

        totalSize /= (MapConstants.TerrainPatchSize * MapConstants.TerrainPatchSize);

        totalSize /= 10;

        float worldBaseHeight = 1.0f * MapConstants.MinLandHeight / MapConstants.MapHeight;

        int numPools = totalSize;

        MyRandom rand = new MyRandom(zone.Seed % 1000000000 + 2438932);


        int currentPools = 0;

        int totalTries = 100 * numPools;

        for (int times = 0; times < totalTries; times++)
        {
            if (currentPools >= numPools)
            {
                break;
            }

            int cx = MathUtils.IntRange(minx, maxx, rand);
            int cz = MathUtils.IntRange(minz, maxz, rand);


            int rad = 100;
            float minDistToFeature = rad * 3 / 2;


            bool onEdgeOfMap = false;

            for (int x = cx-rad; x <= cx+rad; x++)
            {
                if (onEdgeOfMap)
                {
                    break;
                }

                if (x < 0 || x >= _mapProvider.GetMap().GetHwid())
                {
                    onEdgeOfMap = true;
                    break;
                }
                int dx = x - cx;
                for (int z = cz-rad; z <= cz+rad; z++)
                {
                    if (z < 0 || z >= _mapProvider.GetMap().GetHhgt())
                    {
                        onEdgeOfMap = true;
                        break;
                    }
                    int dy = z - cz;

                    if (_md.alphas[x,z,MapConstants.RoadTerrainIndex] > 0 ||
                        _md.mountainHeights[x,z] != 0 ||
                        FlagUtils.IsSet(_md.flags[x,z],MapGenFlags.IsLocation |
                        MapGenFlags.NearWater))
                    {
                        float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                        if (dist < minDistToFeature)
                        {
                            minDistToFeature = dist;
                        }
                    }
                }
            }

            if (onEdgeOfMap)
            {
                continue;
            }

            int minDist = rad / (1 + (times * 3) / totalTries);

            if (minDistToFeature < minDist)
            {
                continue;
            }

            int maxRadius = (int)(minDistToFeature - 10);


            WaterGenData poolData = new WaterGenData()
            {
                x = cx,
                z = cz,
                minXSize = maxRadius/2,
                maxXSize = maxRadius,
                minZSize = maxRadius/2,
                maxZSize = maxRadius,
                stepSize = 1,
            };

            int deformSeed = rand.Next();

            AlterHeightsNear(cx, cz, maxRadius, deformSeed, true);


            if (!_addPoolService.TryAddPool(poolData))
            {
                AlterHeightsNear(cx, cz, maxRadius, deformSeed, false);
            }
            else
            {
                currentPools++;
                for (int x = cx-maxRadius; x <= cx+maxRadius; x++)
                {
                    if (x < 0 || x >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }

                    for (int y = cz - maxRadius; y <= cz + maxRadius; y++)
                    {
                        if (y < 0 || y >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }

                        if (_md.heights[x,y] < worldBaseHeight &&
                            FlagUtils.IsSet(_md.flags[x,y], MapGenFlags.BelowWater))
                        {
                            _md.heights[x, y] = worldBaseHeight;
                        }
                    }
                }
            }
        }           
    }


    protected void AlterHeightsNear(int cx, int cy, int maxRadius, int randomSeed, bool lowerHeights)
    {
        MyRandom rand = new MyRandom(randomSeed);

        int raiseLowerMult = (lowerHeights ? -1 : 1);

        int size = maxRadius * 2 + 1;

        float heightAmp = MathUtils.FloatRange(0.4f, 0.7f, rand);
        float heightFreq = MathUtils.FloatRange(0.02f, 0.04f, rand) * size;
        float heightPers = MathUtils.FloatRange(0.2f, 0.5f, rand);
        int heightOctaves = 2;
        float[,] heightNoise = _noiseService.Generate(heightPers, heightFreq, heightAmp, heightOctaves, rand.Next(), size, size);

        int maxRadiusX = MathUtils.IntRange(maxRadius * 2 / 3, maxRadius, rand);
        int maxRadiusY = MathUtils.IntRange(maxRadius * 2 / 3, maxRadius, rand);
        int minRadius = Math.Min(maxRadiusX, maxRadiusY);
        float bottomDepth = MathUtils.FloatRange(minRadius / 4, minRadius / 2, rand);


        float radAmp = MathUtils.FloatRange(0.5f, 0.9f, rand);
        float radFreq = MathUtils.FloatRange(5.0f, 14.0f, rand);
        float radPers = MathUtils.FloatRange(0.2f, 0.8f, rand);
        int radOctaves = 2;

        float[,] radNoise = _noiseService.Generate(radPers, radFreq, radAmp, radOctaves, rand.Next(), 360, 360);


        float midPower = MathUtils.FloatRange(0.2f, 0.4f, rand);
        float powerAmp = MathUtils.FloatRange(0.05f, 0.15f, rand);
        float powerFreq = MathUtils.FloatRange(5.0f, 12.0f, rand);
        float powerPers = MathUtils.FloatRange(0.2f, 0.3f, rand);
        int powerOctaves = 2;
        float[,] powerNoise = _noiseService.Generate(powerPers, powerFreq, powerAmp, powerOctaves, rand.Next(), 360, 360);



        int angleRot = rand.Next() % 360;
        int xmin = cx - maxRadiusX;
        int xmax = cx + maxRadiusX;
        int ymin = cy - maxRadiusY;
        int ymax = cy + maxRadiusY;
        for (int xx = xmin; xx <= xmax; xx++)
        {
            if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
            {
                continue;
            }

            int dx = xx - cx;
            float xpct = (xx - cx) / (1.0f * maxRadiusX);
            int distToEdgeX = Math.Min(Math.Abs(xx - xmin), Math.Abs(xx - xmax));
            int offsetx = xx - xmin;
            for (int yy = ymin; yy <= ymax; yy++)
            {
                int dy = yy - cy;
                if (yy < 0 || yy >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

                float ypct = (yy - cy) / (1.0f * maxRadiusY);
                int offsety = yy - ymin;

                int distToEdgeY = Math.Min(Math.Abs(yy-ymin), Math.Abs(yy-ymax));

                float radMult = 1.0f;

                double angle = Math.Atan2(dx, dy) * 180 / Math.PI + angleRot;

                while (angle >= 360)
                {
                    angle -= 360;
                }

                while (angle < 0)
                {
                    angle += 360;
                }

                int intAngle = (int)(angle);

                float radDelta = radNoise[intAngle, intAngle/2];

                int distToEnd = Math.Abs(Math.Min(intAngle, 360 - intAngle));

                int distToEndCheck = 5;
                if (distToEnd <= distToEndCheck)
                {
                    radDelta *= 1.0f * distToEnd / distToEndCheck;
                }

                if (radDelta < 0)
                {
                    // If delta < 0 set it to at least -0.5f, then cube to shrink it.
                    radDelta = -(float)(Math.Pow(Math.Abs(radDelta), 3.0f));
                    if (radDelta < -0.5f)
                    {
                        radDelta = -0.5f;
                    }
                    // Negative rad = wider area so we bump up against the edge of the region we are modifying.
                }
                radMult = 0.9f + radDelta;




                float powerDelta = powerNoise[intAngle,intAngle/2];

                if (distToEnd <= distToEndCheck)
                {
                    powerDelta *= 1.0f * distToEnd / distToEndCheck;
                }

                float depthPower = midPower + powerDelta;

                float pctToEdge = (float)(Math.Pow((xpct * xpct + ypct * ypct)*radMult, depthPower));

                if (pctToEdge > 1)
                {
                    continue;
                }

                float currNoise = heightNoise[offsetx, offsety];

                float heightDiff = (1 - pctToEdge) * bottomDepth;
                heightDiff += (float)((currNoise * bottomDepth*(1-pctToEdge*pctToEdge)));

                int edgeScaleDist = 8;

                float edgeScaleDown = Math.Min(distToEdgeX, edgeScaleDist) * 1.0f / edgeScaleDist;
                edgeScaleDown *= Math.Min(distToEdgeY, edgeScaleDist) * 1.0f / edgeScaleDist;
                edgeScaleDown = (float)(Math.Pow(edgeScaleDown, 0.8f));

                heightDiff *= edgeScaleDown;

                heightDiff /= MapConstants.MapHeight;


                _md.heights[xx, yy] += heightDiff*raiseLowerMult;
            }
        }
    }

}
