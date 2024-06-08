
using System;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.Zones.WorldData;

public class AddMountainNoise : BaseAddMountains
{
    public override async UniTask Generate(CancellationToken token)
    {
        await UniTask.CompletedTask;

        int radius = 4;

        _md.mountainNoise = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            if (zone.XMin >= zone.XMax || zone.ZMin >= zone.ZMax)
            {
                continue;
            }

            bool showTerraces = true;

            int minx = Math.Max(0, zone.XMin - radius);
            int maxx = Math.Min(_mapProvider.GetMap().GetHwid() - 1, zone.XMax + radius);
            int miny = Math.Max(0, zone.ZMin - radius);
            int maxy = Math.Min(_mapProvider.GetMap().GetHwid() - 1, zone.ZMax + radius);

            int xsize = (maxx - minx + 1);
            int ysize = (maxy - miny + 1);

            int size = (xsize + ysize) / 2;

            MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed % 500000000 + zone.Seed % 500000000 + 328233);

            float terraceSize = MathUtils.FloatRange(12.0f, 18.0f, rand);

            float freqb = (float)(MathUtils.FloatRange(0.3f, 1.0f, rand) * size / 90.0f)*0.03f;
            float ampb = MathUtils.FloatRange(0.9f, 1.4f, rand)*2.5f;
            int octavesb = 2;
            float persb = MathUtils.FloatRange(0.30f, 0.50f, rand)*1.1f;

            int seedNext = rand.Next();
            float[,] heightsbig = _noiseService.Generate(persb, freqb, ampb, octavesb, seedNext, xsize, ysize);

            float freqedge = (float)(MathUtils.FloatRange(0.5f, 1.5f, rand) * _mapProvider.GetMap().GetHwid() / 40.0f)*0.45f;
            float ampedge = MathUtils.FloatRange(0.1f, 0.2f, rand)*2.5f;
            int octavesedge = 1;
            float persedge = 0.1f;

            float[,] edgeDistances = _noiseService.Generate(persedge, freqedge, ampedge, octavesedge, rand.Next(), xsize, ysize);

            float midMinVal = 0.85f;
            float midMaxVal = 1.0f;

            float midfreq = (size / MathUtils.FloatRange(8.0f, 17.0f, rand))*0.053f;
            float midamp = MathUtils.FloatRange(1.3f, 1.7f, rand) * 2.5f;
            int midoctaves = 2;
            float midpers = MathUtils.FloatRange(0.3f, 0.45f, rand);

            float[,] midVals = _noiseService.Generate(midpers, midfreq, midamp, midoctaves, rand.Next(), xsize, ysize);


            int startPerlinSeed = rand.Next() % 1000000000;

            int times = 2;

            int midTimes = 0;

            for (int i = 0; i < times; i++)
            {
                if (showTerraces && i > 0)
                {
                    continue;
                }

                float freq = (float)(MathUtils.FloatRange(0.05f, 0.07f, rand) * size)*0.05f;
                float amp = MathUtils.FloatRange(10.0f, 20.0f, rand)*2.5f;
                int octaves = 2;
                float pers = MathUtils.FloatRange(0.30f, 0.50f, rand);

                float extraScale = MathUtils.FloatRange(0.9f, 1.3f, rand);
                freq /= extraScale;
                amp *= extraScale;

                float[,] heights = _noiseService.Generate(pers, freq, amp, octaves, startPerlinSeed + 1000 * i, xsize, ysize);

                float secondaryWallScale = MathUtils.FloatRange(0.4f, 1.0f, rand);

                // If the index is 1, do maxHeight-heights[x,y]
                float maxHeight = 0;

                if (i == 1)
                {

                    for (int x = minx; x <= maxx; x++)
                    {
                        for (int y = miny; y <= maxy; y++)
                        {
                            if (Math.Abs(heights[x - minx, y - miny]) > maxHeight)
                            {
                                maxHeight = Math.Abs(heights[x - minx, y - miny]);
                            }
                        }
                    }
                }

                for (int x = minx; x <= maxx; x++)
                {
                    for (int y = miny; y <= maxy; y++)
                    {
                        float zoneScale = 1.0f;

                        if (_md.mapZoneIds[x, y] == zone.IdKey)
                        {
                            float minDist = radius;
                            for (int xx = x - radius; xx <= x + radius; xx++)
                            {
                                float ddx = x - xx;
                                for (int yy = y - radius; yy <= y + radius; yy++)
                                {
                                    float ddy = y - yy;
                                    if (_md.mapZoneIds[x, y] != zone.IdKey)
                                    {
                                        float dist = (float)(Math.Sqrt(ddx * ddx + ddy * ddy));
                                        if (dist < minDist)
                                        {
                                            minDist = dist;
                                        }
                                    }
                                }
                            }
                            // Scale from 1.0f at distance radius to 1.0 to 0.5f at distance 0.
                            zoneScale = 0.5f + 0.5f * minDist / radius;
                        }
                        else // Not in zone but near zone.
                        {
                            float minDist = radius;
                            for (int xx = x - radius; xx <= x + radius; xx++)
                            {
                                float ddx = x - xx;
                                for (int yy = y - radius; yy <= y + radius; yy++)
                                {
                                    float ddy = y - yy;
                                    if (_md.mapZoneIds[x, y] == zone.IdKey)
                                    {
                                        float dist = (float)(Math.Sqrt(ddx * ddx + ddy * ddy));
                                        if (dist < minDist)
                                        {
                                            minDist = dist;
                                        }
                                    }
                                }
                            }
                            // Scale from 0.0 at radius distance to 0.5 at radius 0.
                            zoneScale = 0.5f * (1 - minDist / radius);

                        }
                        float secondaryMidScale = (float)MathUtils.Clamp(0, (midVals[x - minx, y - miny] - midMinVal) / (midMaxVal - midMinVal), 1.0f);

                        if (_md.mountainDistPercent[x, y] >= 1.0f)
                        {


                            if (secondaryMidScale > 0)
                            {
                                _md.flags[x, y] |= MapGenFlags.OverrideWallNoiseScale;
                                midTimes++;
                                _md.mountainNoise[x, y] += secondaryMidScale * zoneScale;
                            }
                            continue;
                        }

                        float bigscale = MathUtils.Clamp(-1, heightsbig[x - minx, y - miny], 1);

                        // at 1 = 0, at -1 = 1, at 0 = 0.5
                        float zeroScale = 0.5f - bigscale / 2.0f;

                        float wallEdgeSize = MathUtils.Clamp(0.8f, (1.0f - ampedge) + edgeDistances[x - minx, y - miny], 0.95f);

                        float wallEdgeScale = 1.0f;

                        float wallDistancePct = _md.mountainDistPercent[x, y];
                        float wallDist = _md.mountainCenterDist[x, y];
                        if (_md.mountainDistPercent[x, y] > wallEdgeSize)
                        {
                            wallEdgeScale = 1 - (_md.mountainDistPercent[x, y] - wallEdgeSize) / (1.0f - wallEdgeSize);
                        }


                        if (secondaryMidScale > wallEdgeScale)
                        {
                            _md.flags[x, y] |= MapGenFlags.OverrideWallNoiseScale;
                            midTimes++;
                            wallEdgeScale = secondaryMidScale * 1.5f;
                        }

                        float hgt = (float)Math.Abs(heights[x - minx, y - miny]);
                        hgt = heights[x - minx, y - miny];

                        if (false && showTerraces)
                        {
                            if (wallDist < MapConstants.InitialMountainDistance)
                            {
                                float heightNoise = hgt;
                                float currTerraceSize = terraceSize + heightNoise / 7;
                                hgt = terraceSize;
                                float currDist = wallDist;
                                while (currDist >= currTerraceSize)
                                {
                                    currDist -= currTerraceSize;
                                }
                                hgt *= currDist / currTerraceSize * 0.8f;
                            }
                        }
                        if (FlagUtils.IsSet(_md.flags[x, y], MapGenFlags.IsSecondaryWall) && !showTerraces)
                        {
                            hgt *= secondaryWallScale;
                        }
                        _md.mountainNoise[x, y] += hgt * wallEdgeScale * zoneScale;
                        continue;
                    }
                }
            }
        }
    }
}
