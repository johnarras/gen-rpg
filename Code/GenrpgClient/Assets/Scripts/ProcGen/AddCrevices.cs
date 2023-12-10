
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using GEntity = UnityEngine.GameObject;


using Genrpg.Shared.Core.Entities;



using Cysharp.Threading.Tasks;

using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;

using System.Threading;

public class CreviceData
{
    public int xStart;
    public int yStart;
    public int xEnd;
    public int yEnd;
    public int xSize;
    public int ySize;
    public Zone zone;
}
public class AddCrevices : BaseZoneGenerator
{

    public const float MinSteepness = 15f;
    public const float MaxSteepness = 60f;
    public const float MaxSteepnessPerturbDelta = 15f;

    public const int SmoothRadiusDelta = 5;
    public const int SmoothRadiusDefault = 12;

    public const int RoadEFfectDist = 16;
    public const int RoadZeroDist = 8;

    protected ILineGenService _lineGenService;

    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        foreach (Zone zone in gs.map.Zones)
        {
            GenerateOne(gs, zone, gs.data.GetGameData<ZoneTypeSettings>(gs.ch).GetZoneType(zone.ZoneTypeId), zone.XMin, zone.XMax, zone.ZMin, zone.ZMax);
        }

        
        SetCreviceDepths(gs);
        gs.md.creviceDepths = null;
    }

    private void SetCreviceDepths (UnityGameState gs)
    {
        if (gs.md.heights == null || gs.md.creviceDepths == null)
        {
            return;
        }

        for (int x = 0; x < gs.map.GetHwid(); x++)
        {
            for (int y = 0; y < gs.map.GetHhgt(); y++)
            {
                float lowerValue = gs.md.creviceDepths[x, y] * MapConstants.DefaultCreviceDepth / MapConstants.MapHeight;


                float roadDist = gs.md.roadDistances[x,y];               
                if (roadDist < RoadEFfectDist)
                {
                    if (roadDist < RoadZeroDist)
                    {
                        lowerValue = 0;
                    }
                    else
                    {
                        lowerValue *= 1.0f * (roadDist - RoadZeroDist) / (RoadEFfectDist - RoadZeroDist);
                    }

                }


                gs.md.heights[x, y] += lowerValue;
            }
        }
    }
    public void GenerateOne(UnityGameState gs, Zone zone, ZoneType zoneType, int startx, int endx, int starty, int endy)
    {
        if (zone == null || zoneType == null || startx >= endx || starty >= endy)
        {
            return;
        }


        if (startx < 0)
        {
            startx = 0;
        }

        if (starty < 0)
        {
            starty = 0;
        }

        if (endx >= gs.map.GetHwid())
        {
            endx = gs.map.GetHwid() - 1;
        }

        if (endy >= gs.map.GetHhgt())
        {
            endy = gs.map.GetHhgt() - 1;
        }

        int xsize = endx - startx + 1;
        int ysize = endy - starty + 1;

        if (xsize < 1 || ysize < 1)
        {
            return;
        }

        CreviceData cdata = new CreviceData()
        {
            xSize = xsize,
            ySize = ysize,
            xStart = startx,
            yStart = starty,
            xEnd = endx,
            yEnd = endy,
            zone = zone,
        };
        if (gs.md.creviceDepths == null)
        {
            gs.md.creviceDepths = new float[gs.map.GetHwid(), gs.map.GetHhgt()];
        }

        MyRandom rand = new MyRandom(zone.Seed + 2333);


        int perlinSize = Math.Max(cdata.xSize + 40, cdata.ySize + 40);

        float depthFreq = 5.0f * MathUtils.FloatRange(0.8f, 1.2f, rand);
        float depthAmp = 0.05f * MathUtils.FloatRange(0.8f, 1.2f, rand);
        int depthOctaves = 2;
        float depthPer = 0.3f * MathUtils.FloatRange(0.8f, 1.2f, rand);

        float[,] depthOffsets = _noiseService.Generate(gs, depthPer, depthFreq, depthAmp, depthOctaves, rand.Next() + 1234, perlinSize, perlinSize);

        float smoothFreq = 5.0f * MathUtils.FloatRange(0.8f, 1.2f, rand);
        float smoothAmp = 0.1f * MathUtils.FloatRange(0.8f, 1.2f, rand);
        int smoothOctaves = 2;
        float smoothPer = 0.3f * MathUtils.FloatRange(0.8f, 1.2f, rand);

        float[,] smoothOffsets = _noiseService.Generate(gs, smoothPer, smoothFreq, smoothAmp, smoothOctaves, rand.Next() % 2345, perlinSize, perlinSize);


        float minCrevices = 2.0f;
        float maxCrevices = 6.0f;

        if (zoneType.CreviceCountScale > 0)
        {
            minCrevices *= zoneType.CreviceCountScale;
            maxCrevices *= zoneType.CreviceCountScale;
        }

        int numCrevices = (int)(MathUtils.FloatRange(minCrevices, maxCrevices, rand));


        for (int c = 0; c < numCrevices; c++)
        {
            AddCreviceDepths(gs, cdata, c, zone, zoneType, depthOffsets, smoothOffsets);
        }


    }

    public void AddCreviceDepths(UnityGameState gs, CreviceData cdata, int index, Zone zone, ZoneType zoneType, float[,] depthOffsets, float[,] smoothOffsets)
    {
        if (cdata == null || zone == null || zoneType == null)
        {
            return;
        }

        MyRandom endPtRand = new MyRandom(zone.Seed % 134634657 + 6623 + index * 13 + index * index * 7);

        int sx = 0;
        int sy = 0;
        int ex = 0;
        int ey = 0;

        int edgeSize = 10;

        int size = Math.Max(cdata.xSize, cdata.ySize);
        int times = 0;
        int minSize = size / 30;
        int maxSize = size * 3 / 4;

        edgeSize = 10;
        minSize = size / 3;
        maxSize = size - edgeSize;

        while (++times < 100)
        {

            sx = MathUtils.IntRange(edgeSize, cdata.xSize - edgeSize, endPtRand) + cdata.xStart;
            sy = MathUtils.IntRange(edgeSize, cdata.ySize - edgeSize, endPtRand) + cdata.yStart;
            ex = MathUtils.IntRange(edgeSize, cdata.xSize - edgeSize, endPtRand) + cdata.xStart;
            ey = MathUtils.IntRange(edgeSize, cdata.ySize - edgeSize, endPtRand) + cdata.yStart;

            int dx = Math.Abs(sx - ex);
            int dy = Math.Abs(sy - ey);

            if (endPtRand.NextDouble() < 0.3f)
            {
                sx = (sx + ex) / 2;
                sy = (sy + ey) / 2;
            }
            else if (endPtRand.NextDouble() < 0.3)
            {
                ex = (sx + ex) / 2;
                ey = (sy + ey) / 2;

            }
            if ((dx >= minSize || dy >= minSize) &&
                (dx < maxSize || dy < maxSize))
            {
                break;
            }

        }

        MyPoint sp = new MyPoint(sx, sy);
        MyPoint ep = new MyPoint(ex, ey);

        InnerAddCreviceDepths(gs, cdata, zone, zoneType, sp, ep, endPtRand.Next() % 100000000, depthOffsets, smoothOffsets);

    }

    private void InnerAddCreviceDepths(UnityGameState gs, CreviceData cdata, Zone zone, ZoneType zoneType, MyPoint sp, MyPoint ep, int randSeed,
        float[,] depthOffsets, float[,] smoothOffsets)
    {
        MyRandom crossRand = new MyRandom(zone.Seed % 1000000000 + 662423 + randSeed);
        MyRandom rand = new MyRandom(zone.Seed % 2010102933 + 783124 + randSeed);

        if (cdata == null || gs.md.creviceDepths == null)
        {
            return;
        }
        if (gs.md.creviceBridges == null)
        {
            gs.md.creviceBridges = new List<MyPointF>();
        }

        float overallDepthMult = (MathUtils.FloatRange(0.5f, 1.2f, rand) +
                           MathUtils.FloatRange(0.5f,1.2f, rand)) *0.6f;

        if (zoneType.CreviceDepthScale > 0)
        {
            overallDepthMult *= zoneType.CreviceDepthScale;
        }

        LineGenParameters ld = GetCreviceParameters(sp, ep, zoneType, rand.Next(), 0);

        List<MyPointF> line = _lineGenService.GetBressenhamLine(gs, sp, ep, ld);

        if (line == null)
        {
            return;
        }





        List<MyPointF> centerPoints = new List<MyPointF>();

        foreach (MyPointF item in line)
        {
            int cx = (int)(item.X);
            int cy = (int)(item.Y);
            if (cx < 0 || cx >= gs.map.GetHwid() || cy < 0 || cy >= gs.map.GetHhgt())
            {
                continue;
            }

            if (item.Z == 1)
            {
                centerPoints.Add(item);
            }
            float currDepthMult = 1.0f;

            if (depthOffsets != null)
            {
                int dx = cx - cdata.xStart;
                int dy = cy - cdata.yStart;

                if (dx >= 0 && dy >= 0 && dx < depthOffsets.GetLength(0) && dy < depthOffsets.GetLength(1))
                {
                    currDepthMult += depthOffsets[dx, dy];
                }

            }

            // Min depth to set crevice.
            gs.md.creviceDepths[cx, cy] = Math.Min(gs.md.creviceDepths[cx, cy], -1 * currDepthMult * overallDepthMult);



        }



        List<MyPointF> sideCenterPoints = new List<MyPointF>();

        int nextCreviceStart = MathUtils.IntRange(50, 90, rand);
        int nextCreviceMod = MathUtils.IntRange(40, 65, rand);

        float sideDepthMult = overallDepthMult * MathUtils.FloatRange(0.5f, 1.0f, rand);

        int nextCreviceDist = nextCreviceStart + crossRand.Next() % nextCreviceMod;
        int pointsSinceLastCrevice = nextCreviceStart + crossRand.Next() % nextCreviceMod;
        foreach (MyPointF cpf in centerPoints)
        {
            MyPoint cp = new MyPoint((int)(cpf.X), (int)(cpf.Y));
            pointsSinceLastCrevice++;
            if (pointsSinceLastCrevice < nextCreviceDist)
            {
                continue;
            }
            pointsSinceLastCrevice = 0;
            nextCreviceDist = nextCreviceStart + crossRand.Next() % nextCreviceMod;

            int cdx = sp.Y - ep.Y;
            int cdy = -(sp.X - ep.X);


            float origSize = MathUtils.Sqrt(cdx * cdx + cdy * cdy);
            if (origSize < 1)
            {
                continue;
            }
            int len = 40 + crossRand.Next() % 45;

            int newdx = (int)(len * cdx / origSize);
            int newdy = (int)(len * cdy / origSize);

            int maxdx = (int)(len * 1.0f);

            int csx = cp.X + newdx + MathUtils.IntRange(-maxdx, maxdx, crossRand);
            int csy = cp.Y - newdy + MathUtils.IntRange(-maxdx, maxdx, crossRand);
            int cex = cp.X + newdx + MathUtils.IntRange(-maxdx, maxdx, crossRand);
            int cey = cp.Y + newdy + MathUtils.IntRange(-maxdx, maxdx, crossRand);

            MyPoint csp = new MyPoint(csx, csy);
            MyPoint cep = new MyPoint(cex, cey);


            ld = GetCreviceParameters(csp, cep, zoneType, rand.Next(), 1);

            List<MyPointF> line2 = _lineGenService.GetBressenhamLine(gs, csp, cep, ld);
            foreach (MyPointF pt2 in line2)
            {

                int cx = (int)(pt2.X);
                int cy = (int)(pt2.Y);
                if (cx < 0 || cx >= gs.map.GetHwid() || cy < 0 || cy >= gs.map.GetHhgt())
                {
                    continue;
                }


                float currDepthMult = 1.0f;

                if (depthOffsets != null)
                {
                    int dx = cx - cdata.xStart;
                    int dy = cy - cdata.yStart;

                    if (dx >= 0 && dy >= 0 && dx < depthOffsets.GetLength(0) && dy < depthOffsets.GetLength(1))
                    {
                        currDepthMult += depthOffsets[dx, dy];
                    }

                }

                // Min depth to create crevice.
                gs.md.creviceDepths[cx, cy] = Math.Min(gs.md.creviceDepths[cx, cy], -1 * currDepthMult * sideDepthMult);
                if (pt2.Z == 1)
                {
                    sideCenterPoints.Add(pt2);
                }

                line.Add(pt2);

            }
        }

        foreach (MyPointF cp in sideCenterPoints)
        {
            centerPoints.Add(cp);
        }

        // Now force bridges to be made.

        for (int c = 0; c < centerPoints.Count; c++)
        {
            MyPointF cp = centerPoints[c];
            int ax = (int)(cp.X * gs.map.GetHwid() / gs.md.awid);
            int ay = (int)(cp.Y * gs.map.GetHhgt() / gs.md.ahgt);
            if (ax >= 0 && ax < gs.md.awid && ay >= 0 && ay < gs.md.ahgt)
            {
                if (gs.md.roadDistances[ax, ay] <= 2 && rand.NextDouble() < 0.03f)
                {
                    gs.md.creviceBridges.Add(cp);
                }
            }
        }

        // now save smoothing for later.
        SmoothNearCrevice(gs, zoneType, cdata, line, smoothOffsets);
    }


    private LineGenParameters GetCreviceParameters(MyPoint sp, MyPoint ep, ZoneType zoneType, int randomSeed, int depth)
    {

        LineGenParameters ld = new LineGenParameters();
        MyRandom rand = new MyRandom(randomSeed);


        int startWidth = 4 + rand.Next() % 3 + rand.Next() % 5;

        if (zoneType.CreviceWidthScale > 0)
        {
            startWidth = (int)(startWidth * zoneType.CreviceWidthScale);
            if (startWidth < 3)
            {
                startWidth = 3;
            }
        }

        if (depth > 0)
        {
            startWidth -= MathUtils.IntRange(startWidth / 4, startWidth * 2 / 3, rand);
        }

        ld.MinWidthSize = startWidth / 3;
        ld.WidthSize = startWidth;
        ld.MaxWidthSize = startWidth * 3;


        ld.WidthSizeChangeAmount = MathUtils.IntRange(2, 12, rand);

        ld.WidthSizeChangeChance = MathUtils.FloatRange(0.1, 0.3, rand);

        ld.WidthPosShiftChance = MathUtils.FloatRange(0.1f, 0.3f, rand);


        ld.WidthPosShiftSize = MathUtils.IntRange(2, 4, rand);

        ld.InitialNoPosShiftLength = MathUtils.IntRange(4, 8, rand);

        ld.MaxWidthPosDrift = MathUtils.FloatRange(0.2f, 0.8f, rand);

        ld.LinePathNoiseScale = MathUtils.FloatRange(0.0f, 1.1f, rand);
        ld.Seed = rand.Next();
        return ld;

    }

    public void SmoothNearCrevice(UnityGameState gs, ZoneType zoneType, CreviceData cdata, List<MyPointF> pts, float[,] smoothChanges)
    {
        if (gs.md.creviceDepths == null || pts == null || cdata == null || zoneType == null)
        {
            return;
        }

        MyRandom smoothRand = new MyRandom(cdata.zone.Seed % 312323221 + 32423);

        float startSmoothRadius = SmoothRadiusDefault;
        float startRadiusDelta = SmoothRadiusDelta;

        startSmoothRadius *= MathUtils.FloatRange(0.8f, 1.3f, smoothRand);
        startRadiusDelta *= MathUtils.FloatRange(0.8f, 1.3f, smoothRand);

        // Loop through all points and make the depths approach 0 based on how far they are from the points in the line.
        // Use min of currval, smooth val to keep the crevices in place.
        foreach (MyPointF pt in pts)
        {
            int cx = (int)(pt.X);
            int cy = (int)(pt.Y);

            if (cx < 0 || cy < 0 || cx >= gs.map.GetHwid() || cy >= gs.map.GetHhgt())
            {
                continue;
            }

            float centerDepth = gs.md.creviceDepths[cx, cy];
            float smoothRadius = startSmoothRadius;

            if (zoneType.CreviceWidthScale > 0)
            {
                smoothRadius *= zoneType.CreviceWidthScale;
            }

            if (smoothChanges != null)
            {
                int dx = cx - cdata.xStart;
                int dy = cy - cdata.yStart;

                if (dx >= 0 && dy >= 0 && dx < smoothChanges.GetLength(0) && dy < smoothChanges.GetLength(1))
                {
                    smoothRadius += smoothChanges[dx, dy] * startSmoothRadius;
                }
                smoothRadius = MathUtils.Clamp(startSmoothRadius / 2, smoothRadius, startSmoothRadius * 2);
            }

            int smoothRadiusInt = (int)(smoothRadius);

            for (int xx = cx - smoothRadiusInt; xx <= cx + smoothRadiusInt; xx++)
            {
                if (xx < 0 || xx >= gs.map.GetHwid())
                {
                    continue;
                }
                float ddx = cx - xx;
                for (int yy = cy - smoothRadiusInt; yy <= cy + smoothRadiusInt; yy++)
                {
                    if (yy < 0 || yy >= gs.map.GetHhgt())
                    {
                        continue;
                    }
                    float ddy = cy - yy;

                    float dist = (float)Math.Sqrt(ddx * ddx + ddy * ddy);
                    if (dist >= smoothRadius)
                    {
                        continue;
                    }

                    float distMult = 1 - dist / smoothRadius;


                    float valueToSet = centerDepth * distMult;

                    float currVal = gs.md.creviceDepths[xx, yy];

                    if (valueToSet < currVal)
                    {
                        gs.md.creviceDepths[xx, yy] = valueToSet;
                    }

                }
            }
        }
    }
}
