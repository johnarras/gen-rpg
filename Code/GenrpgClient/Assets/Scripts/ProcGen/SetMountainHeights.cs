
using System;
using System.Threading.Tasks;
using Genrpg.Shared.Utils;
using System.Threading;

public class SetMountainHeights : BaseAddMountains
{
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await Task.CompletedTask;

        MyRandom rand = new MyRandom(gs.map.Seed % 1000000000 + 3323292);

        float pers = MathUtils.FloatRange(0.15f, 0.25f, rand);
        int octaves = 2;
        float amp = MathUtils.FloatRange(0.30f, 0.45f, rand);
        float freq = MathUtils.FloatRange(0.04f, 0.09f, rand) * gs.map.GetHwid();

        float powerpers = MathUtils.FloatRange(0.10f, 0.20f, rand);
        int poweroctaves = 2;
        float poweramp = MathUtils.FloatRange(0.14f, 0.25f, rand);
        float powerfreq = MathUtils.FloatRange(0.06f, 0.10f, rand) * gs.map.GetHwid();


        float edgepers = MathUtils.FloatRange(0.20f, 0.30f, rand);
        int edgeoctaves = 2;
        float edgeamp = MathUtils.FloatRange(0.40f, 0.70f, rand);
        float edgefreq = MathUtils.FloatRange(0.03f, 0.05f, rand) * gs.map.GetHwid();


        float edgePowPers = MathUtils.FloatRange(0.20f, 0.30f, rand);
        int edgePowoctaves = 2;
        float edgePowamp = MathUtils.FloatRange(0.30f, 0.50f, rand);
        float edgePowfreq = MathUtils.FloatRange(0.03f, 0.06f, rand) * gs.map.GetHwid();



        float[,] noise = _noiseService.Generate(gs, pers, freq, amp, octaves, rand.Next(), gs.map.GetHwid(), gs.map.GetHhgt());
        float[,] powernoise = _noiseService.Generate(gs, powerpers, powerfreq, poweramp, poweroctaves, rand.Next(), gs.map.GetHwid(), gs.map.GetHhgt());
        float[,] edgeNoise = _noiseService.Generate(gs, edgepers, edgefreq, edgeamp, edgeoctaves, rand.Next(), gs.map.GetHwid(), gs.map.GetHhgt());
        float[,] edgePowNoise = _noiseService.Generate(gs, edgePowPers, edgePowfreq, edgePowamp, edgePowoctaves, rand.Next(), gs.map.GetHwid(), gs.map.GetHhgt());
        
        float mountainDefaultHeight = gs.md.GetMountainDefaultSize(gs, gs.map) * MathUtils.FloatRange(0.8f, 1.0f, rand);

        float minDistPctCutoff = 0.9f;


        for (int x = 0; x < gs.map.GetHwid(); x++)
        {
            for (int y = 0; y < gs.map.GetHhgt(); y++)
            {

                int ax = Math.Min(x, gs.map.GetHwid() - 1);
                int ay = Math.Min(y, gs.map.GetHhgt() - 1);


                float noiseScale = 1.0f;
                float whh = gs.md.mountainHeights[x, y];

                float roadCheckDistance = MapConstants.MaxRoadCheckDistance;
                float minNoiseDistance = 12.0f;
                float roadDist = gs.md.roadDistances[x, y];


                if (gs.md.roadDistances[x, y] < roadCheckDistance)
                {
                    float rpct = roadDist / roadCheckDistance;
                    rpct = (float)(Math.Pow(rpct, 1.6f));
                    float edgeDist = MathUtils.Clamp(0.10f, 0.30f + edgeNoise[x, y], 0.70f);
                    //edgeDist = 0.15f;
                    float edgeAmt = (float)(Math.Pow(edgeDist, 1.7f + edgePowNoise[x, y]));
                    
                    float currAmt = rpct * rpct;
                    float noiseVal = noise[x, y];
                    float noiseMinDist = MapConstants.RoadBaseHillScaleDistance * (1 + noiseVal);
                    noiseMinDist = MathUtils.Clamp(minNoiseDistance, noiseMinDist, roadCheckDistance);

                    noiseMinDist = 20.0f;
                    if (gs.md.roadDistances[x, y] < noiseMinDist)
                    {
                        float currPower = 1.8f;
                        currPower *= MathUtils.Clamp(1.0f, (1.0f + powernoise[x, y]), 2.0f);
                        noiseScale *= (float)(Math.Pow(roadDist / noiseMinDist, currPower));
                    }
                    if (rpct <= edgeDist)
                    {
                        whh *= currAmt;
                    }
                    else if (rpct < 1)
                    {
                        whh *= (edgeAmt + ((rpct - edgeDist) / (1 - edgeDist)) * (1 - edgeAmt));
                    }
                }

                if (gs.md.mountainHeights[x, y] == 0 || gs.md.mountainDistPercent[x, y] >= 1.0f)
                {
                    if (FlagUtils.IsSet(gs.md.flags[x, y], MapGenFlags.OverrideWallNoiseScale))
                    {
                        gs.md.heights[x, y] += gs.md.mountainNoise[x, y] * noiseScale / MapConstants.MapHeight;
                    }
                    continue;
                }



                float distPct = gs.md.mountainDistPercent[x, y];
                if (distPct >= minDistPctCutoff && distPct <= 1)
                {
                    float noiseMult = 1 - (distPct - minDistPctCutoff) / (1.0f - minDistPctCutoff);
                    whh *= noiseMult;
                }

                float edgePercent = (float)Math.Pow(gs.md.EdgeHeightmapAdjustPercent(gs, gs.map, x, y), 0.09f);

                whh *= edgePercent;
                if (whh != 0)
                {
                    gs.md.heights[x, y] += (mountainDefaultHeight / MapConstants.MapHeight) * whh;
                    gs.md.ClearAlphasAt(gs, x, y);
                    gs.md.alphas[x, y, MapConstants.BaseTerrainIndex] = 1.0f;
                }
                float currentNoise = Math.Abs(gs.md.mountainNoise[x, y]);
                float maxNoise = Math.Abs(gs.md.mountainHeights[x, y]) * mountainDefaultHeight * 0.2f;

                if (maxNoise < 0.0001f || currentNoise < 0.0001f)
                {
                    continue;
                }

                if (currentNoise > maxNoise)
                {
                    noiseScale *= (maxNoise) / currentNoise;
                }

                if (FlagUtils.IsSet(gs.md.flags[x, y], MapGenFlags.OverrideWallNoiseScale))
                {
                    noiseScale = 1.0f;
                }

                gs.md.heights[x, y] += gs.md.mountainNoise[x, y] * noiseScale * edgePercent / MapConstants.MapHeight;
            }
        }
    }
}