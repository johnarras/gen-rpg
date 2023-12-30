
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using Genrpg.Shared.Utils;

using System.Threading;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;

public class AddRandomDirt : BaseZoneGenerator
{
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
        if ( gs.md.alphas == null || zone == null || zoneType == null ||
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

            MyRandom sizeRand = new MyRandom(gs.map.Seed / 3 + zone.Seed / 2 + 234745 + outertimes);

            for (int r = 0; r < replaceTextures.Length; r++)
            {


                MyRandom rand = new MyRandom(zone.Seed % 23463321 + r * 10293 + r * r * 32123 + outertimes*3);
                bool usePerlin = rand.NextDouble() < 0.5f;

                float freq = MathUtils.FloatRange(size * 0.03f, size * 0.2f, rand)*1.35f;
                float amp = MathUtils.FloatRange(0.4f, 1.0f, rand)*1.1f;


                float pers = MathUtils.FloatRange(0.2f, 0.5f, rand);
                int octaves = 2;

                float[,] noise = _noiseService.Generate(gs, pers, freq, amp, octaves, rand.Next(), size, size);
                perlinOutputs.Add(noise);

                float maxFreq = MathUtils.FloatRange(size * 0.03f, size * 0.08f, rand);
                float maxAmp = MathUtils.FloatRange(0.1f, 0.3f, rand);
                float maxPers = MathUtils.FloatRange(0.2f, 0.4f, rand);
                int maxOctaves = 2;

                float[,] maxNoise = _noiseService.Generate(gs, maxPers, maxFreq, maxAmp, maxOctaves, rand.Next(), size, size);
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
                        if (gs.md.mapZoneIds[x, y] != zone.IdKey)
                        {
                            continue;
                        }
                        float maxPct = midNoise + maxHeights[x-startx, y-starty];
                        if (maxPct > 1)
                        {
                            maxPct = 1;
                        }

                        float basePct = gs.md.alphas[x, y, MapConstants.BaseTerrainIndex];
                        float newPct = Math.Max(changes[x - startx, y - starty], 0);
                        if (newPct > maxPct)
                        {
                            newPct = maxPct;
                        }

                        if (newPct > basePct)
                        {
                            newPct = basePct;
                        }
                        gs.md.alphas[x, y, MapConstants.BaseTerrainIndex] -= newPct;
                        gs.md.alphas[x, y, newIndex] += newPct;
                        if (gs.md.alphas[x, y, newIndex] > 1)
                        {
                            float diff = gs.md.alphas[x, y, newIndex] - 1;
                            gs.md.alphas[x, y, MapConstants.BaseTerrainIndex] = diff;
                            gs.md.alphas[x, y, newIndex] = 1;
                        }
                    }
                }
            }
        }
    }
}
