using System.Linq;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;

using Genrpg.Shared.Utils;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using System.Text;
using System;
using UnityEngine;

public class AddZoneNoise : BaseZoneGenerator
{
    private const float _amplitude = 3.9f;
    private const float _persistence = 0.5f;
    private const float _freqDiv = 1000f;
    private const float _lacunarity = 1.5f;

    public override async Awaitable Generate(CancellationToken token)
    {
        int noiseSize = _mapProvider.GetMap().GetHwid();
        float ampDelta = 0.05f;
        float zoneAmp = MathUtils.FloatRange(_amplitude * (1 - ampDelta), _amplitude * (1 + ampDelta), _rand);
        float denomDelta = 0.05f;
        float zoneDenom = MathUtils.FloatRange(_freqDiv * (1 - denomDelta), _freqDiv * (1 + denomDelta), _rand);
        float persDelta = 0.05f;
        float pers = MathUtils.FloatRange(_persistence * (1 - persDelta), _persistence * (1 + persDelta), _rand);
        float freq = noiseSize / zoneDenom;
        float lacDelta = 0.05f;
        float lac = MathUtils.FloatRange(_lacunarity * (1 - lacDelta), _lacunarity * (1 + pers), _rand);

        int seed = _rand.Next();
        float[,] heights = _noiseService.Generate(pers, noiseSize / zoneDenom, zoneAmp, 2, seed, noiseSize, noiseSize, 0.5f);

        for (int x =0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int y =0; y < _mapProvider.GetMap().GetHhgt(); y++)
            {
                // Do 1-heights here since most heights are near 0, and few are near 1, we want
                // few near 0 and many near 1 so when the pct is low, very few pieces of
                // terrain will be affected.
                _md.overrideZoneScales[x, y] = 1-MathUtils.Clamp(0, Math.Abs(heights[x, y]), 1);
            }
        }
        
    }
}
