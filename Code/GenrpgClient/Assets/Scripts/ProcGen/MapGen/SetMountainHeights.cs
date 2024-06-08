
using System;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using System.Threading;

public class SetMountainHeights : BaseAddMountains
{
    public override async UniTask Generate(CancellationToken token)
    {
        await UniTask.CompletedTask;

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed % 1000000000 + 3323292);

        float pers = MathUtils.FloatRange(0.15f, 0.25f, rand);
        int octaves = 2;
        float amp = MathUtils.FloatRange(0.30f, 0.45f, rand);
        float freq = MathUtils.FloatRange(0.04f, 0.09f, rand) * _mapProvider.GetMap().GetHwid();

        float powerpers = MathUtils.FloatRange(0.10f, 0.20f, rand);
        int poweroctaves = 2;
        float poweramp = MathUtils.FloatRange(0.14f, 0.25f, rand);
        float powerfreq = MathUtils.FloatRange(0.06f, 0.10f, rand) * _mapProvider.GetMap().GetHwid();


        float edgepers = MathUtils.FloatRange(0.20f, 0.30f, rand);
        int edgeoctaves = 2;
        float edgeamp = MathUtils.FloatRange(0.40f, 0.70f, rand);
        float edgefreq = MathUtils.FloatRange(0.03f, 0.05f, rand) * _mapProvider.GetMap().GetHwid();


        float edgePowPers = MathUtils.FloatRange(0.20f, 0.30f, rand);
        int edgePowoctaves = 2;
        float edgePowamp = MathUtils.FloatRange(0.30f, 0.50f, rand);
        float edgePowfreq = MathUtils.FloatRange(0.03f, 0.06f, rand) * _mapProvider.GetMap().GetHwid();



        float[,] noise = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());
        float[,] powernoise = _noiseService.Generate(powerpers, powerfreq, poweramp, poweroctaves, rand.Next(), _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());
        float[,] edgeNoise = _noiseService.Generate(edgepers, edgefreq, edgeamp, edgeoctaves, rand.Next(), _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());
        float[,] edgePowNoise = _noiseService.Generate(edgePowPers, edgePowfreq, edgePowamp, edgePowoctaves, rand.Next(), _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());
        
        float mountainDefaultHeight = _md.GetMountainDefaultSize(_mapProvider.GetMap()) * MathUtils.FloatRange(0.8f, 1.0f, rand);

        float minDistPctCutoff = 0.9f;


        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int y = 0; y < _mapProvider.GetMap().GetHhgt(); y++)
            {

                int ax = Math.Min(x, _mapProvider.GetMap().GetHwid() - 1);
                int ay = Math.Min(y, _mapProvider.GetMap().GetHhgt() - 1);


                float noiseScale = 1.0f;
                float whh = _md.mountainHeights[x, y];

                float roadCheckDistance = MapConstants.MaxRoadCheckDistance;
                float minNoiseDistance = 12.0f;
                float roadDist = _md.roadDistances[x, y];


                if (_md.roadDistances[x, y] < roadCheckDistance)
                {
                    float rpct = roadDist / roadCheckDistance;
                    rpct = (float)(Math.Pow(rpct, 1.6f));
                    float edgeDist = MathUtils.Clamp(0.10f, 0.30f + edgeNoise[x, y], 0.70f);
                    float edgeAmt = (float)(Math.Pow(edgeDist, 1.7f + edgePowNoise[x, y]));
                    
                    float currAmt = rpct * rpct;
                    float noiseVal = noise[x, y];
                    float noiseMinDist = MapConstants.RoadBaseHillScaleDistance * (1 + noiseVal);
                    noiseMinDist = MathUtils.Clamp(minNoiseDistance, noiseMinDist, roadCheckDistance);

                    noiseMinDist = 20.0f;
                    if (_md.roadDistances[x, y] < noiseMinDist)
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

                if (_md.mountainHeights[x, y] == 0 || _md.mountainDistPercent[x, y] >= 1.0f)
                {
                    if (FlagUtils.IsSet(_md.flags[x, y], MapGenFlags.OverrideWallNoiseScale))
                    {
                        _md.heights[x, y] += _md.mountainNoise[x, y] * noiseScale / MapConstants.MapHeight;
                    }
                    continue;
                }



                float distPct = _md.mountainDistPercent[x, y];
                if (distPct >= minDistPctCutoff && distPct <= 1)
                {
                    float noiseMult = 1 - (distPct - minDistPctCutoff) / (1.0f - minDistPctCutoff);
                    whh *= noiseMult;
                }

                float edgePercent = (float)Math.Pow(_md.EdgeHeightmapAdjustPercent(_mapProvider.GetMap(), x, y), 0.09f);

                whh *= edgePercent;
                if (whh != 0)
                {
                    _md.heights[x, y] += (mountainDefaultHeight / MapConstants.MapHeight) * whh;
                    _md.ClearAlphasAt(x, y);
                    _md.alphas[x, y, MapConstants.BaseTerrainIndex] = 1.0f;
                }
                float currentNoise = Math.Abs(_md.mountainNoise[x, y]);
                float maxNoise = Math.Abs(_md.mountainHeights[x, y]) * mountainDefaultHeight * 0.2f;

                if (maxNoise < 0.0001f || currentNoise < 0.0001f)
                {
                    continue;
                }

                if (currentNoise > maxNoise)
                {
                    noiseScale *= (maxNoise) / currentNoise;
                }

                if (FlagUtils.IsSet(_md.flags[x, y], MapGenFlags.OverrideWallNoiseScale))
                {
                    noiseScale = 1.0f;
                }

                _md.heights[x, y] += _md.mountainNoise[x, y] * noiseScale * edgePercent / MapConstants.MapHeight;
            }
        }
    }
}