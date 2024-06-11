
using System;
using System.Collections.Generic;


using Genrpg.Shared.Utils;

using System.Threading;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using UnityEngine;

public class AddRandomDirt : BaseZoneGenerator
{
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
        if (_md.alphas == null || zone == null || zoneType == null ||
            endx <= startx || endy <= starty)
        {
            return;
        }

        int dx = endx - startx;
        int dy = endy - starty;

        int size = Math.Max(Math.Max(dx, dy), MapConstants.DefaultHeightmapSize);

        int[] replaceTextures = new int[] { MapConstants.DirtTerrainIndex, MapConstants.SteepTerrainIndex };

        float midNoise = 0.8f;

        for (int outertimes = 0; outertimes < 3; outertimes++)
        {

            List<float[,]> perlinOutputs = new List<float[,]>();

            List<float[,]> maxNoiseHeights = new List<float[,]>();

            MyRandom sizeRand = new MyRandom(_mapProvider.GetMap().Seed / 3 + zone.Seed / 2 + 234745 + outertimes);

            for (int r = 0; r < replaceTextures.Length; r++)
            {


                MyRandom rand = new MyRandom(zone.Seed % 23463321 + r * 10293 + r * r * 32123 + outertimes*3);
                bool usePerlin = rand.NextDouble() < 0.5f;

                float freq = MathUtils.FloatRange(size * 0.03f, size * 0.2f, rand)*1.35f;
                float amp = MathUtils.FloatRange(0.4f, 1.0f, rand)*1.1f;


                float pers = MathUtils.FloatRange(0.2f, 0.5f, rand);
                int octaves = 2;

                float[,] noise = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), size, size);
                perlinOutputs.Add(noise);

                float maxFreq = MathUtils.FloatRange(size * 0.03f, size * 0.08f, rand);
                float maxAmp = MathUtils.FloatRange(0.1f, 0.3f, rand);
                float maxPers = MathUtils.FloatRange(0.2f, 0.4f, rand);
                int maxOctaves = 2;

                float[,] maxNoise = _noiseService.Generate(maxPers, maxFreq, maxAmp, maxOctaves, rand.Next(), size, size);
                maxNoiseHeights.Add(maxNoise);
            }

            if (perlinOutputs.Count != replaceTextures.Length)
            {
                return;
            }

            for (int r = 0; r < replaceTextures.Length; r++)
            {
                float[,] changes = perlinOutputs[r];
                int newIndex = replaceTextures[r];
                float[,] maxHeights = maxNoiseHeights[r];
                for (int x = startx; x < endx; x++)
                {
                    for (int y = starty; y < endy; y++)
                    {
                        if (_md.mapZoneIds[x, y] != zone.IdKey)
                        {
                            continue;
                        }
                        float maxPct = midNoise + maxHeights[x-startx, y-starty];
                        if (maxPct > 1)
                        {
                            maxPct = 1;
                        }

                        float basePct = _md.alphas[x, y, MapConstants.BaseTerrainIndex];
                        float newPct = Math.Max(changes[x - startx, y - starty], 0);
                        if (newPct > maxPct)
                        {
                            newPct = maxPct;
                        }

                        if (newPct > basePct)
                        {
                            newPct = basePct;
                        }
                        _md.alphas[x, y, MapConstants.BaseTerrainIndex] -= newPct;
                        _md.alphas[x, y, newIndex] += newPct;
                        if (_md.alphas[x, y, newIndex] > 1)
                        {
                            float diff = _md.alphas[x, y, newIndex] - 1;
                            _md.alphas[x, y, MapConstants.BaseTerrainIndex] = diff;
                            _md.alphas[x, y, newIndex] = 1;
                        }
                    }
                }
            }
        }
    }
}
