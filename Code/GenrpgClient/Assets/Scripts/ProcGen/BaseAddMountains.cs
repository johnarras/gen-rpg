
using System.Threading.Tasks;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.MapServer.Entities;


using System;
using System.Collections.Generic;
using System.Threading;

public class BaseAddMountains : BaseZoneGenerator
{
    protected ILineGenService _lineGenService;

    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await Task.CompletedTask;
    }


    public float GetMountainHeightMult(MyRandom rand)
    {


        float heightMult = 1.0f;
        float totalChance = MapConstants.MountainZeroHeightChance + MapConstants.MountainRandomHeightChance;
        double chosenChance = rand.NextDouble();
        if (chosenChance < MapConstants.MountainZeroHeightChance)
        {
            heightMult = 0.01f;
        }
        else if (chosenChance < totalChance)
        {
            heightMult = MathUtils.FloatRange(0.2f, 1.0f, rand);
        }
        return heightMult;
    }

    protected void AddMountainRidge(UnityGameState gs, int sx, int sy, int ex, int ey, long seed, bool boring, float heightMult, bool secondaryMountain = false)
    {
        if (heightMult < 0.01f)
        {
            heightMult = 0.01f;
        }

        LineGenParameters boringLP = new LineGenParameters();
        boringLP.MaxWidthPosDrift = 0;
        boringLP.MinWidthSize = 1;
        boringLP.MaxWidthSize = 1;
        boringLP.WidthPosShiftChance = 0;
        boringLP.WidthPosShiftSize = 0;
        boringLP.WidthSizeChangeAmount = 0;
        boringLP.WidthSizeChangeChance = 0;
        boringLP.LinePathNoiseScale = 0;
        LineGenParameters lp = new LineGenParameters();

        MyRandom lineRand = new MyRandom(seed);

        int mountainWidth = (int)gs.md.GetMountainDefaultSize(gs, gs.map);


        mountainWidth = (int)(MathUtils.IntRange(mountainWidth * 4 / 5, mountainWidth * 6 / 5, lineRand));

        if (secondaryMountain)
        {
            mountainWidth = (int)(mountainWidth * heightMult);
        }

        lp.MaxWidthPosDrift = lineRand.Next() % 100 + 100;
        lp.MinWidthSize = 1;
        lp.MaxWidthSize = 1;

        lp.WidthPosShiftChance = MathUtils.FloatRange(0.12f, 0.23f, lineRand);
        lp.WidthPosShiftChance = 0;
        lp.WidthSize = 1;
        lp.WidthSizeChangeAmount = 0;
        lp.WidthSizeChangeChance = 0.0f;
        lp.LinePathNoiseScale = MathUtils.FloatRange(0.0f, 0.1f, lineRand);
        //lp.LinePathNoiseScale = 0;
        lp.Seed = lineRand.Next();

        int dx = Math.Abs(sx - ex);
        int dy = Math.Abs(sy - ey);


        int maxLen = Math.Max(dx, dy);

        List<MyPointF> points = _lineGenService.GetBressenhamLine(gs, new MyPoint(sx, sy), new MyPoint(ex, ey), (boring ? boringLP : lp));
        if (points == null || points.Count < 1)
        {
            return;
        }
        int startWallWidth = Math.Max(1, lineRand.Next(mountainWidth * 9 / 10, mountainWidth * 11 / 10));

        float baseFreqScaling = MathUtils.Sqrt((sx - ex) * (sx - ex) + (sy - ey) * (sy - ey));

        AddMountainPoints(gs, points, startWallWidth, baseFreqScaling, lineRand.Next(),
            maxLen,secondaryMountain,heightMult);
    }

    public void AddMountainPoints(UnityGameState gs, List<MyPointF> points, int startWallWidth, float baseFreqScaling, 
        int randSeed, int maxLen, bool secondaryMountain, float heightMult)
       { 
        MyRandom lineRand = new MyRandom(randSeed / 3 + 183892);

        float amp = MathUtils.FloatRange(0.07f, 0.22f, lineRand) * 0.8f;
        float freq = MathUtils.FloatRange(0.1f, 0.3f, lineRand) * maxLen * 0.01f * 0.8f;

        int octaves = 2;
        float pers = MathUtils.FloatRange(0.1f, 0.3f, lineRand);
        float[,] mainHeightNoise = _noiseService.Generate(gs, pers, freq, amp, octaves, lineRand.Next(), points.Count, 1);

        float wfreq = MathUtils.FloatRange(0.05f, 0.1f, lineRand) * baseFreqScaling;
        float wamp = MathUtils.FloatRange(0.2f, 0.4f, lineRand);
        float wpers = MathUtils.FloatRange(0.4f, 0.7f, lineRand);       
        int woctaves = 2;
        float[,] widthNoise = _noiseService.Generate(gs, wpers, wfreq, wamp, woctaves, lineRand.Next(), points.Count, 1);

        float wallHeightScale = MathUtils.FloatRange(0.8f, 1.05f, lineRand);
        int currWallWidth = Math.Max(1, startWallWidth + lineRand.Next() % 3 - lineRand.Next() % 3);
        for (int l = 0; l < points.Count; l++)
        {
            MyPointF item = points[l];
            float mainHeight = mainHeightNoise[l, 0];
            int cx = (int)(item.X);
            int cy = (int)(item.Y);

            currWallWidth = (int)(startWallWidth * (1 + widthNoise[l, 0]));

            if (cx < 0 || cy < 0 || cx >= gs.map.GetHwid() || cy >= gs.map.GetHhgt())
            {
                continue;
            }
            if (gs.md.mapZoneIds[cx, cy] < 1)
            {
                gs.md.mapZoneIds[cx, cy] = MapConstants.MountainZoneId;
            }
            float heightToSet = (1.1f * (1.0f + mainHeight)) * wallHeightScale * heightMult * MapConstants.MountainHeightMult;
            if (gs.md.mountainHeights[cx, cy] < heightToSet)
            {
                gs.md.mountainHeights[cx, cy] = heightToSet;
            }

            gs.md.mountainDistPercent[cx, cy] = 0f;
            if (!secondaryMountain)
            {
                gs.md.edgeMountainDistPercent[cx, cy] = 0f;
            }

            float topWidth = 2;
            int mincmidx = Math.Min(gs.map.GetHwid() / 2, cx);
            int maxcmidx = Math.Max(gs.map.GetHwid() / 2, cx);
            int mincmidy = Math.Min(gs.map.GetHhgt() / 2, cy);
            int maxcmidy = Math.Max(gs.map.GetHhgt() / 2, cy);
            for (int y = Math.Max(0, cy - currWallWidth); y <= Math.Min(gs.map.GetHhgt() - 1, cy + currWallWidth); y++)
            {
                int ddy = Math.Abs(y - cy);
                for (int x = Math.Max(0, cx - currWallWidth); x <= Math.Min(gs.map.GetHwid() - 1, cx + currWallWidth); x++)
                {
                    int ddx = Math.Abs(x - cx);

                    double currDist = Math.Sqrt(ddx * ddx + ddy * ddy);
                    double distPct = currDist / currWallWidth;

                    if (currDist < topWidth)
                    {
                        distPct = 0;
                    }
                    else
                    {
                        distPct = (currDist - topWidth) / (currWallWidth - topWidth);
                    }

                    if (distPct >= 1)
                    {
                        continue;
                    }

                    if (gs.md.mapZoneIds[x, y] == 0)
                    {
                        gs.md.mapZoneIds[x, y] = MapConstants.MountainZoneId;
                    }
                    if (gs.md.mountainDistPercent[x, y] > distPct)
                    {
                        gs.md.mountainDistPercent[x, y] = (float)distPct;                            
                    }
                    if (gs.md.mountainCenterDist[x,y] > currDist)
                    {
                        gs.md.mountainCenterDist[x, y] = (float)(currDist);
                        gs.md.nearestMountainTopHeight[x, y] = heightToSet;
                    }
                    if (!secondaryMountain)
                    {
                        if (gs.md.edgeMountainDistPercent[x, y] > distPct)
                        {
                            gs.md.edgeMountainDistPercent[x, y] = (float)distPct;
                        }
                    }
                    gs.md.flags[x, y] |= MapGenFlags.IsEdgeWall;
                    float currPower = MathUtils.Clamp(0.5f, 1.7f, 1.0f + gs.md.mountainDecayPower[x, y]);
                    float newPct = gs.md.mountainHeights[cx, cy] * (float)(1.0f - Math.Pow(distPct, currPower));

                    if (newPct != 0 && gs.md.mountainHeights[x, y] == 0 && secondaryMountain)
                    {
                        gs.md.flags[x, y] |= MapGenFlags.IsSecondaryWall;
                    }

                    if (newPct > gs.md.mountainHeights[x, y])
                    {
                        gs.md.mountainHeights[x, y] = newPct;
                    }

                }
            }
        }
    }
}